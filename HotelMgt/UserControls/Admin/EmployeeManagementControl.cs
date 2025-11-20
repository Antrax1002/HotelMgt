using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using HotelMgt.Services;
using HotelMgt.UIStyles;
using System.Threading;
using HotelMgt.Custom;
using HotelMgt.Forms;

namespace HotelMgt.UserControls.Admin
{
    public partial class EmployeeManagementControl : UserControl
    {
        private readonly DatabaseService _dbService;

        private RoundedPanel _headerPanel = null!;
        private Label _lblTitle = null!;
        private Label _lblDesc = null!;
        private Button _btnAddEmployee = null!;
        private DataGridView _dgvEmployees = null!;

        // Paint hook flag for merged header (kept to avoid double hooking)
        private bool _actionsHeaderPaintHooked;

        // Prevents re-entrancy when opening modals (guards against double-clicks)
        private int _modalGate;

        // NEW: guard to ensure we only rebuild UI once
        private bool _uiBuilt;

        public EmployeeManagementControl()
        {
            InitializeComponent();
            _dbService = new DatabaseService();
            this.Load += EmployeeManagementControl_Load;
        }

        private void EmployeeManagementControl_Load(object? sender, EventArgs e)
        {
            // Build the UI using the ViewBuilder once; this replaces designer-created controls
            if (!_uiBuilt)
            {
                EmployeeManagementViewBuilder.Build(
                    this,
                    out _headerPanel,
                    out _lblTitle,
                    out _lblDesc,
                    out _btnAddEmployee,
                    out _dgvEmployees);

                _uiBuilt = true;

                // Wire events on the new controls
                WireEvents();
            }

            // Ensure action columns and merged header styling
            EmployeeManagementViewBuilder.ConfigureActionColumns(_dgvEmployees);
            if (!_actionsHeaderPaintHooked)
            {
                EmployeeManagementViewBuilder.HookMergedActionsHeader(_dgvEmployees);
                _actionsHeaderPaintHooked = true;
            }

            LoadEmployees();
        }

        private void WireEvents()
        {
            _btnAddEmployee.Click += BtnAddEmployee_Click;
            _dgvEmployees.CellContentClick += DgvEmployees_CellContentClick;
        }

        private void LoadEmployees()
        {
            try
            {
                using var conn = _dbService.GetConnection();
                conn.Open();

                using var cmd = new SqlCommand(@"
                    SELECT EmployeeID, FirstName, MiddleName, LastName, Email, PhoneNumber, Username, Role, HireDate, IsActive
                    FROM Employees
                    ORDER BY HireDate DESC, EmployeeID DESC;", conn);

                using var adapter = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                adapter.Fill(dt);

                if (!dt.Columns.Contains("Name"))
                    dt.Columns.Add("Name", typeof(string));
                if (!dt.Columns.Contains("StatusText"))
                    dt.Columns.Add("StatusText", typeof(string));

                foreach (DataRow row in dt.Rows)
                {
                    var first = Convert.ToString(row["FirstName"]) ?? "";
                    var middle = dt.Columns.Contains("MiddleName") ? (Convert.ToString(row["MiddleName"]) ?? "") : "";
                    var last = Convert.ToString(row["LastName"]) ?? "";
                    // Show middle initial if present
                    var middleDisplay = string.IsNullOrWhiteSpace(middle) ? "" : $" {middle.Trim()[0]}.";
                    row["Name"] = $"{first}{middleDisplay} {last}".Trim();

                    bool isActive = row["IsActive"] != DBNull.Value && Convert.ToBoolean(row["IsActive"]);
                    row["StatusText"] = isActive ? "Active" : "Inactive";
                }

                _dgvEmployees.DataSource = dt;

                // Ensure action columns after rebinding (idempotent)
                EmployeeManagementViewBuilder.ConfigureActionColumns(_dgvEmployees);
                _dgvEmployees.Invalidate(); // refresh header paint
            }
            catch (Exception ex)
            {
                using (EmployeeManagementViewBuilder.PauseShield())
                    MessageBox.Show($"Error loading employees: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAddEmployee_Click(object? sender, EventArgs e)
        {
            if (!TryEnterModalGate()) return;

            var prevAddEnabled = _btnAddEmployee.Enabled;
            var prevGridEnabled = _dgvEmployees.Enabled;
            _btnAddEmployee.Enabled = false;
            _dgvEmployees.Enabled = false;

            try
            {
                using var form = new EmployeeEditorForm();
                form.IsEmailUsernameUnique = IsEmailUsernameUnique;
                form.CurrentEmployeeId = null; // Add mode

                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    try
                    {
                        using var conn = _dbService.GetConnection();
                        conn.Open();

                        using var cmd = new SqlCommand(@"
INSERT INTO Employees (FirstName, MiddleName, LastName, Email, PhoneNumber, Username, PasswordHash, Role, IsActive, HireDate, CreatedAt, UpdatedAt)
VALUES (@FirstName, @MiddleName, @LastName, @Email, @Phone, @Username, @Password, @Role, 1, @HireDate, @Now, @Now);", conn);

                        cmd.Parameters.AddWithValue("@FirstName", (form.FirstName ?? string.Empty).Trim());
                        cmd.Parameters.AddWithValue("@MiddleName", (form.MiddleName ?? string.Empty).Trim());
                        cmd.Parameters.AddWithValue("@LastName", (form.LastName ?? string.Empty).Trim());
                        cmd.Parameters.AddWithValue("@Email", (form.Email ?? string.Empty).Trim());
                        cmd.Parameters.AddWithValue("@Phone", string.IsNullOrWhiteSpace(form.Phone) ? (object)DBNull.Value : form.Phone.Trim());
                        cmd.Parameters.AddWithValue("@Username", (form.Username ?? string.Empty).Trim());
                        cmd.Parameters.AddWithValue("@Password", form.Password ?? string.Empty); // plaintext for now
                        cmd.Parameters.AddWithValue("@Role", form.Role);
                        cmd.Parameters.AddWithValue("@HireDate", form.HireDate.Date);
                        cmd.Parameters.AddWithValue("@Now", DateTime.Now);

                        cmd.ExecuteNonQuery();

                        EmployeeManagementViewBuilder.ShowToast(this, "Operation Successful", 1000);
                        LoadEmployees();
                    }
                    catch (SqlException sx) when (sx.Number == 2627 || sx.Number == 2601)
                    {
                        using (EmployeeManagementViewBuilder.PauseShield())
                            MessageBox.Show(form, "Email or Username already exists.", "Duplicate", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    catch (Exception ex2)
                    {
                        using (EmployeeManagementViewBuilder.PauseShield())
                            MessageBox.Show(form, $"Error adding employee: {ex2.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            finally
            {
                _btnAddEmployee.Enabled = prevAddEnabled;
                _dgvEmployees.Enabled = prevGridEnabled;
                LeaveModalGate();
            }
        }

        private void DgvEmployees_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            var grid = _dgvEmployees;
            if (grid is null) return;
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            var column = grid.Columns?[e.ColumnIndex];
            if (column is not DataGridViewButtonColumn) return;

            var gridRow = grid.Rows[e.RowIndex];
            if (gridRow?.DataBoundItem is not DataRowView rowView) return;
            var row = rowView.Row;
            if (row == null) return;

            int? maybeEmployeeId = row.Field<int?>("EmployeeID");
            if (maybeEmployeeId is null) return;
            int employeeId = maybeEmployeeId.Value;

            bool isActive = row.Field<bool?>("IsActive") ?? false;

            switch (column.Name)
            {
                case EmployeeManagementViewBuilder.ColEdit:
                    OpenEditEmployee(employeeId, row);
                    break;

                case EmployeeManagementViewBuilder.ColStatus:
                    ToggleEmployeeStatus(employeeId, isActive);
                    break;

                case EmployeeManagementViewBuilder.ColRemove:
                    RemoveEmployee(employeeId, row.Field<string?>("Name") ?? $"Employee #{employeeId}");
                    break;
            }
        }

        private void OpenEditEmployee(int employeeId, DataRow row)
        {
            if (!TryEnterModalGate()) return;

            var prevAddEnabled = _btnAddEmployee.Enabled;
            var prevGridEnabled = _dgvEmployees.Enabled;
            _btnAddEmployee.Enabled = false;
            _dgvEmployees.Enabled = false;

            try
            {
                using var form = new EmployeeEditorForm();
                // Pre-populate fields
                form.FirstName = Convert.ToString(row["FirstName"]) ?? "";
                form.MiddleName = Convert.ToString(row["MiddleName"]) ?? "";

                form.LastName = Convert.ToString(row["LastName"]) ?? "";
                form.Email = Convert.ToString(row["Email"]) ?? "";
                form.Phone = row["PhoneNumber"] == DBNull.Value ? "" : Convert.ToString(row["PhoneNumber"]) ?? "";
                form.Username = Convert.ToString(row["Username"]) ?? "";
                form.Role = Convert.ToString(row["Role"]) ?? "Employee";
                form.HireDate = row["HireDate"] == DBNull.Value ? DateTime.Today : Convert.ToDateTime(row["HireDate"]);
                form.CurrentEmployeeId = employeeId;
                form.IsEmailUsernameUnique = IsEmailUsernameUnique;

                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    try
                    {
                        using var conn = _dbService.GetConnection();
                        conn.Open();

                        bool hasPwd = !string.IsNullOrWhiteSpace(form.Password);
                        string sql = @"
                                        UPDATE Employees
                                        SET FirstName=@FirstName,
                                            MiddleName=@MiddleName,
                                            LastName=@LastName,
                                            Email=@Email,
                                            PhoneNumber=@Phone,
                                            Username=@Username,
                                            Role=@Role,
                                            HireDate=@HireDate,
                                            UpdatedAt=@Now" + (hasPwd ? ", PasswordHash=@Password" : "") + @"
                                        WHERE EmployeeID=@EmployeeID;";

                        using var cmd = new SqlCommand(sql, conn);
                        cmd.Parameters.AddWithValue("@EmployeeID", employeeId);
                        cmd.Parameters.AddWithValue("@FirstName", (form.FirstName ?? string.Empty).Trim());
                        cmd.Parameters.AddWithValue("@MiddleName", (form.MiddleName ?? string.Empty).Trim());
                        cmd.Parameters.AddWithValue("@LastName", (form.LastName ?? string.Empty).Trim());
                        cmd.Parameters.AddWithValue("@Email", (form.Email ?? string.Empty).Trim());
                        cmd.Parameters.AddWithValue("@Phone", string.IsNullOrWhiteSpace(form.Phone) ? (object)DBNull.Value : form.Phone.Trim());
                        cmd.Parameters.AddWithValue("@Username", (form.Username ?? string.Empty).Trim());
                        cmd.Parameters.AddWithValue("@Role", form.Role);
                        cmd.Parameters.AddWithValue("@HireDate", form.HireDate.Date);
                        cmd.Parameters.AddWithValue("@Now", DateTime.Now);
                        if (hasPwd)
                            cmd.Parameters.AddWithValue("@Password", form.Password ?? string.Empty);

                        cmd.ExecuteNonQuery();

                        EmployeeManagementViewBuilder.ShowToast(this, "Operation Successful", 1000);
                        LoadEmployees();
                    }
                    catch (SqlException sx) when (sx.Number == 2627 || sx.Number == 2601)
                    {
                        using (EmployeeManagementViewBuilder.PauseShield())
                            MessageBox.Show(form, "Email or Username already exists.", "Duplicate", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    catch (Exception ex2)
                    {
                        using (EmployeeManagementViewBuilder.PauseShield())
                            MessageBox.Show(form, $"Error updating employee: {ex2.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            finally
            {
                _btnAddEmployee.Enabled = prevAddEnabled;
                _dgvEmployees.Enabled = prevGridEnabled;
                LeaveModalGate();
            }
        }

        private bool IsEmailUsernameUnique(string email, string username, int? currentId)
        {
            using var conn = _dbService.GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(@"
                SELECT COUNT(*) FROM Employees
                WHERE (Email = @Email OR Username = @Username)
                AND (@CurrentId IS NULL OR EmployeeID <> @CurrentId)", conn);
            cmd.Parameters.AddWithValue("@Email", email);
            cmd.Parameters.AddWithValue("@Username", username);
            cmd.Parameters.AddWithValue("@CurrentId", (object?)currentId ?? DBNull.Value);
            int count = (int)cmd.ExecuteScalar();
            return count == 0;
        }

        private void ToggleEmployeeStatus(int employeeId, bool currentIsActive)
        {
            var targetText = currentIsActive ? "inactive" : "active";
            var confirm = MessageBox.Show(
                $"Change status to {targetText}?",
                "Confirm Status Change",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            try
            {
                using var conn = _dbService.GetConnection();
                conn.Open();

                using var cmd = new SqlCommand(@"
UPDATE Employees
SET IsActive = CASE WHEN IsActive = 1 THEN 0 ELSE 1 END,
    UpdatedAt = @Now
WHERE EmployeeID = @EmployeeID;", conn);

                cmd.Parameters.AddWithValue("@EmployeeID", employeeId);
                cmd.Parameters.AddWithValue("@Now", DateTime.Now);
                cmd.ExecuteNonQuery();

                LoadEmployees();
            }
            catch (Exception ex)
            {
                using (EmployeeManagementViewBuilder.PauseShield())
                    MessageBox.Show($"Error changing status: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RemoveEmployee(int employeeId, string displayName)
        {
            var confirm = MessageBox.Show(
                $"Remove {displayName}? This action cannot be undone.",
                "Confirm Remove",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes) return;

            try
            {
                using var conn = _dbService.GetConnection();
                conn.Open();

                using var cmd = new SqlCommand("DELETE FROM Employees WHERE EmployeeID = @EmployeeID;", conn);
                cmd.Parameters.AddWithValue("@EmployeeID", employeeId);
                cmd.ExecuteNonQuery();

                LoadEmployees();
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                using (EmployeeManagementViewBuilder.PauseShield())
                    MessageBox.Show("Cannot remove employee due to related records.", "Operation blocked", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                using (EmployeeManagementViewBuilder.PauseShield())
                    MessageBox.Show($"Error removing employee: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool TryEnterModalGate() => Interlocked.CompareExchange(ref _modalGate, 1, 0) == 0;
        private void LeaveModalGate() => Interlocked.Exchange(ref _modalGate, 0);
    }
}