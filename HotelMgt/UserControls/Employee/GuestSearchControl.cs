using System;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Globalization;
using System.Linq; // for OfType/FirstOrDefault
using Microsoft.Data.SqlClient; // Use Microsoft.Data.SqlClient only
using HotelMgt.Services;
using HotelMgt.UIStyles;
using HotelMgt.Utilities; // for CurrentUser

namespace HotelMgt.UserControls.Employee
{
    public partial class GuestSearchControl : UserControl
    {
        private readonly DatabaseService _dbService;
        private readonly CultureInfo _currencyCulture = CultureInfo.GetCultureInfo("en-PH");

        // UI refs (logic keeps these)
        private Label lblTitle = null!, lblSubtitle = null!;
        private TextBox txtSearch = null!;
        private Button btnSearch = null!;
        private DataGridView dgvGuests = null!;
        private Button? btnDelete; // from builder (Name = "btnDelete")

        // Helper to check role once in one place
        private static bool IsAdmin => string.Equals(CurrentUser.Role, "Admin", StringComparison.OrdinalIgnoreCase);

        public GuestSearchControl()
        {
            InitializeComponent();
            _dbService = new DatabaseService();
            this.Load += GuestSearchControl_Load;
        }

        private void GuestSearchControl_Load(object? sender, EventArgs e)
        {
            // Build UI from UIStyles (includes "btnDelete" beside Search)
            GuestSearchViewBuilder.Build(
                this,
                searchAction: SearchGuests,
                out lblTitle,
                out lblSubtitle,
                out txtSearch,
                out btnSearch,
                out dgvGuests);

            // Hook Delete button created by the builder
            btnDelete = this.Controls.Find("btnDelete", true).OfType<Button>().FirstOrDefault();
            if (btnDelete != null)
            {
                // Only Admins see and can use Delete
                btnDelete.Visible = IsAdmin;
                if (IsAdmin)
                {
                    btnDelete.Click += (_, __) => DeleteSelectedGuest();
                }
            }

            // Grid behavior
            dgvGuests.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvGuests.MultiSelect = false;
            dgvGuests.ReadOnly = true;

            // Enable/disable Delete as selection changes
            dgvGuests.SelectionChanged += (_, __) => UpdateDeleteButtonState();

            // Open history on double-click
            dgvGuests.CellDoubleClick += DgvGuests_CellDoubleClick;
            dgvGuests.RowHeaderMouseDoubleClick += DgvGuests_RowHeaderMouseDoubleClick;

            SharedTimerManager.SharedTick += SharedTimerManager_SharedTick;

            SearchGuests();
        }

        private void DgvGuests_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            OpenGuestHistoryFromSelection();
        }

        private void DgvGuests_RowHeaderMouseDoubleClick(object? sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0) return;
            OpenGuestHistoryFromSelection();
        }

        private void OpenGuestHistoryFromSelection()
        {
            if (dgvGuests.CurrentRow?.DataBoundItem is not DataRowView drv) return;

            int guestId = drv.Row.Field<int>("GuestID");
            string first = drv.Row.Field<string?>("FirstName") ?? "";
            string last = drv.Row.Field<string?>("LastName") ?? "";
            string guestName = $"{first} {last}".Trim();

            using var dlg = new Dialogs.GuestCheckInHistoryForm(guestId, guestName);
            dlg.StartPosition = FormStartPosition.CenterParent;
            dlg.ShowDialog(FindForm());
        }

        private void SearchGuests()
        {
            try
            {
                using var conn = _dbService.GetConnection();
                conn.Open();

                var where = new StringBuilder("WHERE 1=1");
                var cmd = new SqlCommand { Connection = conn };

                string term = txtSearch.Text.Trim();

                if (!string.IsNullOrWhiteSpace(term))
                {
                    where.Append(@"
                     AND (
                         FirstName LIKE @Term
                      OR MiddleName LIKE @Term
                      OR LastName LIKE @Term
                      OR (FirstName + ' ' + MiddleName + ' ' + LastName) LIKE @Term
                      OR (FirstName + ' ' + LastName) LIKE @Term
                      OR Email LIKE @Term
                      OR PhoneNumber LIKE @Term
                      OR IDNumber LIKE @Term
                     )");
                    cmd.Parameters.AddWithValue("@Term", $"%{term}%");
                }

                cmd.CommandText = $@"
                        SELECT 
                            GuestID,
                            FirstName,
                            MiddleName,
                            LastName,
                            Email,
                            PhoneNumber,
                            IDType,
                            IDNumber
                        FROM Guests
                        {where}
                        ORDER BY CreatedAt DESC";

                using var adapter = new SqlDataAdapter(cmd);
                var raw = new DataTable();
                adapter.Fill(raw);

                // Bind the raw data; formatting is handled by the builder's grid setup
                dgvGuests.DataSource = raw;

                // Allow wrapping for contact/email if needed
                dgvGuests.DefaultCellStyle.WrapMode = DataGridViewTriState.True;

                UpdateDeleteButtonState();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching guests: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateDeleteButtonState()
        {
            if (btnDelete == null) return;

            if (!IsAdmin)
            {
                btnDelete.Enabled = false;
                return;
            }

            bool enabled = dgvGuests?.CurrentRow?.DataBoundItem is DataRowView;
            btnDelete.Enabled = enabled;
            btnDelete.BackColor = enabled ? Color.FromArgb(220, 38, 38) : Color.FromArgb(200, 200, 200);
        }

        private void DeleteSelectedGuest()
        {
            if (!IsAdmin)
            {
                MessageBox.Show("Only admins can delete guests.", "Access Denied",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (dgvGuests.CurrentRow?.DataBoundItem is not DataRowView drv)
            {
                MessageBox.Show("Select a guest first.", "Delete Guest",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int guestId = drv.Row.Field<int>("GuestID");
            string first = drv.Row.Field<string?>("FirstName") ?? "";
            string last = drv.Row.Field<string?>("LastName") ?? "";
            string guestName = (first + " " + last).Trim();

            var confirm = MessageBox.Show(
                $"Delete guest '{guestName}' (ID {guestId}) and ALL their history (reservations, check-ins, amenities, payments)?\n\nThis cannot be undone.",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes) return;

            try
            {
                using var conn = _dbService.GetConnection();
                conn.Open();

                // 1) Block if there are active check-ins (still in-house)
                using (var checkCmd = new SqlCommand(
                    "SELECT COUNT(*) FROM CheckIns WHERE GuestID = @Id AND ActualCheckOutDateTime IS NULL;", conn))
                {
                    checkCmd.Parameters.AddWithValue("@Id", guestId);
                    var activeCount = (int)(checkCmd.ExecuteScalar() ?? 0);
                    if (activeCount > 0)
                    {
                        MessageBox.Show(
                            "Cannot delete this guest because they have active check-in(s). Please check them out or cancel related stays first.",
                            "Delete Blocked",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                        return;
                    }
                }

                using var tx = conn.BeginTransaction();

                try
                {
                    // 2) Cascade delete (manual) in safe FK order
                    // Payments referencing guest or any of the guest's reservations
                    using (var cmd = new SqlCommand(@"
DELETE FROM CheckInAmenities
WHERE CheckInID IN (SELECT CheckInID FROM CheckIns WHERE GuestID = @GuestID);

DELETE FROM Payments
WHERE GuestID = @GuestID
   OR ReservationID IN (SELECT ReservationID FROM Reservations WHERE GuestID = @GuestID);

DELETE FROM CheckIns
WHERE GuestID = @GuestID;

DELETE FROM Reservations
WHERE GuestID = @GuestID;

DELETE FROM Guests
WHERE GuestID = @GuestID;", conn, tx))
                    {
                        cmd.Parameters.AddWithValue("@GuestID", guestId);
                        cmd.ExecuteNonQuery();
                    }

                    tx.Commit();
                }
                catch
                {
                    try { tx.Rollback(); } catch { }
                    throw;
                }

                MessageBox.Show("Guest and all related history deleted.", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                SearchGuests();
            }
            catch (SqlException sx)
            {
                MessageBox.Show(
                    $"Delete failed: {sx.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting guest: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                UpdateDeleteButtonState();
            }
        }

        // Optional leftovers
        private void DgvGuests_SelectionChanged(object? sender, EventArgs e) { /* no-op */ }
        private void ShowGuestHistory(int guestId) { /* stub */ }

        private void SharedTimerManager_SharedTick(object? sender, EventArgs e)
        {
            if (this.Visible && this.Parent?.Visible == true)
            {
                   // Optionally check for changes before refreshing
                   SearchGuests();
               }
           }
    }
}