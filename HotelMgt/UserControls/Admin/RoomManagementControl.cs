using HotelMgt.Core.Events;
using HotelMgt.Custom;
using HotelMgt.Forms;
using HotelMgt.Services;
using HotelMgt.UIStyles;
using HotelMgt.Utilities;
using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace HotelMgt.UserControls.Admin
{
    public partial class RoomManagementControl : UserControl
    {
        private readonly DatabaseService _dbService;
        private readonly CultureInfo _currencyCulture = CultureInfo.GetCultureInfo("en-PH");

        private RoundedPanel _headerPanel = null!;
        private Label _lblTitle = null!;
        private Label _lblDesc = null!;
        private Button _btnAddRoom = null!;
        private DataGridView _dgvRooms = null!;

        // Prevents re-entrancy when opening modals (guards against double-clicks)
        private int _modalGate;

        private DateTime? _lastRoomsUpdatedAt;

        public RoomManagementControl()
        {
            InitializeComponent();
            _dbService = new DatabaseService();
            this.Load += RoomManagementControl_Load;
            SharedTimerManager.SharedTick += SharedTimerManager_SharedTick; // Subscribe to shared timer
            this.Disposed += (s, e) => SharedTimerManager.SharedTick -= SharedTimerManager_SharedTick; // Unsubscribe on dispose
        }

        private void RoomManagementControl_Load(object? sender, EventArgs e)
        {
            // Build the UI (designer moved to builder)
            RoomManagementViewBuilder.Build(
                this,
                out _headerPanel,
                out _lblTitle,
                out _lblDesc,
                out _btnAddRoom,
                out _dgvRooms);

            WireEvents();

            // Ensure action columns and merged header are configured by the builder
            RoomManagementViewBuilder.ConfigureActionColumns(_dgvRooms);
            RoomManagementViewBuilder.HookMergedActionsHeader(_dgvRooms);

            LoadRooms();
        }

        private void WireEvents()
        {
            _btnAddRoom.Click += BtnAddRoom_Click;
            _dgvRooms.CellContentClick += DgvRooms_CellContentClick;
            _dgvRooms.CellFormatting += DgvRooms_CellFormatting;
        }

        private void LoadRooms()
        {
            try
            {
                using var conn = _dbService.GetConnection();
                conn.Open();

                // Ensure statuses match actual occupancy before loading
                NormalizeOccupiedRooms(conn);

                using var cmd = new SqlCommand(@"
SELECT RoomID, RoomNumber, RoomType, Floor, PricePerNight, MaxOccupancy, Status, Amenities, Description, CreatedAt, UpdatedAt
FROM Rooms
ORDER BY Floor, TRY_CONVERT(int, RoomNumber), RoomNumber;", conn);

                using var adapter = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                adapter.Fill(dt);

                // Derived currency text shown in the grid (use PHP culture)
                if (!dt.Columns.Contains("PriceText"))
                    dt.Columns.Add("PriceText", typeof(string));

                foreach (DataRow row in dt.Rows)
                {
                    var price = row["PricePerNight"] == DBNull.Value ? 0m : Convert.ToDecimal(row["PricePerNight"]);
                    row["PriceText"] = price.ToString("C2", _currencyCulture);
                }

                _dgvRooms.DataSource = dt;

                // Ensure action columns after rebinding (idempotent)
                RoomManagementViewBuilder.ConfigureActionColumns(_dgvRooms);
                _dgvRooms.Invalidate();

                // Track latest UpdatedAt
                _lastRoomsUpdatedAt = dt.AsEnumerable()
                    .Select(r => r.Field<DateTime?>("UpdatedAt"))
                    .Where(d => d.HasValue)
                    .OrderByDescending(d => d)
                    .FirstOrDefault();
            }
            catch (Exception ex)
            {
                using (RoomManagementViewBuilder.PauseShield())
                    MessageBox.Show($"Error loading rooms: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SharedTimerManager_SharedTick(object? sender, EventArgs e)
        {
            // Only refresh if the control is visible and parented (tab is active)
            if (!this.Visible || !this.Parent?.Visible == true) return;

            try
            {
                var latest = GetLatestRoomUpdatedAt();
                if (latest.HasValue && (!_lastRoomsUpdatedAt.HasValue || latest > _lastRoomsUpdatedAt))
                {
                    LoadRooms();
                }
            }
            catch
            {
                // swallow
            }
        }

        // Helper to get the latest UpdatedAt from Rooms
        private DateTime? GetLatestRoomUpdatedAt()
        {
            using var conn = _dbService.GetConnection();
            conn.Open();
            using var cmd = new SqlCommand("SELECT MAX(UpdatedAt) FROM Rooms", conn);
            var result = cmd.ExecuteScalar();
            return result is DateTime dt ? dt : (result != DBNull.Value ? Convert.ToDateTime(result) : null);
        }

        private void BtnAddRoom_Click(object? sender, EventArgs e)
        {
            if (!TryEnterModalGate()) return;

            var prevAddEnabled = _btnAddRoom.Enabled;
            var prevGridEnabled = _dgvRooms.Enabled;
            _btnAddRoom.Enabled = false;
            _dgvRooms.Enabled = false;

            try
            {
                using var form = new RoomEditorForm();
                form.Text = "Add Room";
                form.SetRoomTypes(new[] { "Single", "Double", "Deluxe", "Suite" });

                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    if (string.IsNullOrWhiteSpace(form.RoomNumber))
                    {
                        MessageBox.Show(form, "Room Number is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(form.RoomType))
                    {
                        MessageBox.Show(form, "Room Type is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(form.Amenities))
                    {
                        MessageBox.Show(form, "Amenities are required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    try
                    {
                        using var conn = _dbService.GetConnection();
                        conn.Open();

                        using var cmd = new SqlCommand(@"
INSERT INTO Rooms (RoomNumber, RoomType, PricePerNight, MaxOccupancy, Status, Floor, Amenities, Description, CreatedAt, UpdatedAt)
VALUES (@RoomNumber, @RoomType, @PricePerNight, @MaxOccupancy, 'Available', @Floor, @Amenities, @Description, GETDATE(), GETDATE());", conn);

                        cmd.Parameters.AddWithValue("@RoomNumber", form.RoomNumber.Trim());
                        cmd.Parameters.AddWithValue("@RoomType", form.RoomType);
                        cmd.Parameters.AddWithValue("@PricePerNight", form.PricePerNight);
                        cmd.Parameters.AddWithValue("@MaxOccupancy", form.MaxGuests);
                        cmd.Parameters.AddWithValue("@Floor", form.Floor);
                        cmd.Parameters.AddWithValue("@Amenities", string.IsNullOrWhiteSpace(form.Amenities) ? (object)DBNull.Value : form.Amenities.Trim());
                        cmd.Parameters.AddWithValue("@Description", string.IsNullOrWhiteSpace(form.Description) ? (object)DBNull.Value : form.Description.Trim());

                        cmd.ExecuteNonQuery();

                        RoomManagementViewBuilder.ShowToastAfterDialogClose(this, form, "Operation Successful", 1000);
                        LoadRooms();
                        // Optionally publish event here
                    }
                    catch (SqlException sx) when (sx.Number == 2627 || sx.Number == 2601)
                    {
                        MessageBox.Show(form, "Room Number already exists.", "Duplicate", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    catch (Exception ex2)
                    {
                        MessageBox.Show(form, $"Error adding room: {ex2.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            finally
            {
                _btnAddRoom.Enabled = prevAddEnabled;
                _dgvRooms.Enabled = prevGridEnabled;
                LeaveModalGate();
            }
        }

        private void OpenEditRoom(int roomId, DataRow row)
        {
            if (!TryEnterModalGate()) return;

            var prevAddEnabled = _btnAddRoom.Enabled;
            var prevGridEnabled = _dgvRooms.Enabled;
            _btnAddRoom.Enabled = false;
            _dgvRooms.Enabled = false;

            try
            {
                using var form = new RoomEditorForm();
                form.Text = "Edit Room";
                form.SetRoomTypes(new[] { "Single", "Double", "Deluxe", "Suite" });

                // Pre-populate fields
                form.RoomNumber = Convert.ToString(row["RoomNumber"]) ?? "";
                form.Floor = row["Floor"] == DBNull.Value ? 1 : Convert.ToInt32(row["Floor"]);
                form.RoomType = Convert.ToString(row["RoomType"]) ?? "";
                form.PricePerNight = row["PricePerNight"] == DBNull.Value ? 0 : Convert.ToDecimal(row["PricePerNight"]);
                form.MaxGuests = row["MaxOccupancy"] == DBNull.Value ? 1 : Convert.ToInt32(row["MaxOccupancy"]);
                form.Amenities = row["Amenities"] == DBNull.Value ? "" : Convert.ToString(row["Amenities"]) ?? "";
                form.Description = row["Description"] == DBNull.Value ? "" : Convert.ToString(row["Description"]) ?? "";

                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    if (string.IsNullOrWhiteSpace(form.RoomNumber))
                    {
                        MessageBox.Show(form, "Room Number is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(form.RoomType))
                    {
                        MessageBox.Show(form, "Room Type is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(form.Amenities))
                    {
                        MessageBox.Show(form, "Amenities are required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    try
                    {
                        using var conn = _dbService.GetConnection();
                        conn.Open();

                        using var cmd = new SqlCommand(@"
UPDATE Rooms
SET RoomNumber = @RoomNumber,
    RoomType = @RoomType,
    PricePerNight = @PricePerNight,
    MaxOccupancy = @MaxOccupancy,
    Floor = @Floor,
    Amenities = @Amenities,
    Description = @Description,
    UpdatedAt = GETDATE()
WHERE RoomID = @RoomID;", conn);

                        cmd.Parameters.AddWithValue("@RoomID", roomId);
                        cmd.Parameters.AddWithValue("@RoomNumber", form.RoomNumber.Trim());
                        cmd.Parameters.AddWithValue("@RoomType", form.RoomType);
                        cmd.Parameters.AddWithValue("@PricePerNight", form.PricePerNight);
                        cmd.Parameters.AddWithValue("@MaxOccupancy", form.MaxGuests);
                        cmd.Parameters.AddWithValue("@Floor", form.Floor);
                        cmd.Parameters.AddWithValue("@Amenities", string.IsNullOrWhiteSpace(form.Amenities) ? (object)DBNull.Value : form.Amenities.Trim());
                        cmd.Parameters.AddWithValue("@Description", string.IsNullOrWhiteSpace(form.Description) ? (object)DBNull.Value : form.Description.Trim());

                        cmd.ExecuteNonQuery();

                        RoomManagementViewBuilder.ShowToastAfterDialogClose(this, form, "Operation Successful", 1000);
                        LoadRooms();
                        // Optionally publish event here
                    }
                    catch (SqlException sx) when (sx.Number == 2627 || sx.Number == 2601)
                    {
                        MessageBox.Show(form, "Room Number already exists.", "Duplicate", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    catch (Exception ex2)
                    {
                        MessageBox.Show(form, $"Error updating room: {ex2.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            finally
            {
                _btnAddRoom.Enabled = prevAddEnabled;
                _dgvRooms.Enabled = prevGridEnabled;
                LeaveModalGate();
            }
        }

        private void DgvRooms_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (_dgvRooms == null) return;
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            var column = _dgvRooms.Columns?[e.ColumnIndex];
            if (column is not DataGridViewButtonColumn) return;

            var gridRow = _dgvRooms.Rows[e.RowIndex];
            if (gridRow?.DataBoundItem is not DataRowView rowView) return;
            var row = rowView.Row;
            if (row == null) return;

            int? maybeRoomId = row.Field<int?>("RoomID");
            if (maybeRoomId is null) return;
            int roomId = maybeRoomId.Value;

            string roomNumber = row.Field<string?>("RoomNumber") ?? $"#{roomId}";
            string status = row.Field<string?>("Status") ?? "Available";

            switch (column.Name)
            {
                case RoomManagementViewBuilder.ColEdit:
                    OpenEditRoom(roomId, row);
                    break;

                case RoomManagementViewBuilder.ColMaintenance:
                    ToggleMaintenance(roomId, roomNumber, status);
                    break;
            }
        }

        private void ToggleMaintenance(int roomId, string roomNumber, string currentStatus)
        {
            bool setToMaintenance = !string.Equals(currentStatus, "Maintenance", StringComparison.OrdinalIgnoreCase);
            var msg = setToMaintenance
                ? $"Set room #{roomNumber} to Maintenance?"
                : $"Return room #{roomNumber} to Available?";
            var confirm = MessageBox.Show(msg, "Maintenance", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            try
            {
                using var conn = _dbService.GetConnection();
                conn.Open();

                using var cmd = new SqlCommand(@"
UPDATE Rooms
SET Status = @Status, UpdatedAt = GETDATE()
WHERE RoomID = @RoomID;", conn);

                cmd.Parameters.AddWithValue("@RoomID", roomId);
                cmd.Parameters.AddWithValue("@Status", setToMaintenance ? "Maintenance" : "Available");
                cmd.ExecuteNonQuery();

                LoadRooms();
            }
            catch (Exception ex)
            {
                using (RoomManagementViewBuilder.PauseShield())
                    MessageBox.Show($"Error updating maintenance: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Normalize "Occupied" rooms without active guests to "Available"
        private static void NormalizeOccupiedRooms(SqlConnection conn)
        {
            const string sql = @"
                            UPDATE r
                            SET r.Status = 'Available', r.UpdatedAt = GETDATE()
                            FROM Rooms r
                            LEFT JOIN (
                                SELECT DISTINCT RoomID
                                FROM CheckIns
                                WHERE ActualCheckOutDateTime IS NULL
                            ) ci ON ci.RoomID = r.RoomID
                            WHERE r.Status = 'Occupied' AND ci.RoomID IS NULL;";
            using var cmd = new SqlCommand(sql, conn);
            cmd.ExecuteNonQuery();
        }

        private bool TryEnterModalGate() => Interlocked.CompareExchange(ref _modalGate, 1, 0) == 0;
        private void LeaveModalGate() => Interlocked.Exchange(ref _modalGate, 0);

        private void DgvRooms_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (_dgvRooms.Columns[e.ColumnIndex].Name == RoomManagementViewBuilder.ColMaintenance
                && _dgvRooms.Rows[e.RowIndex].DataBoundItem is DataRowView rowView)
            {
                var status = (rowView.Row["Status"] as string ?? "").Trim();
                //if (string.Equals(status, "Available", StringComparison.OrdinalIgnoreCase))
                //{
                //    e.Value = "Maintenance";
                //    _dgvRooms.Rows[e.RowIndex].Cells[e.ColumnIndex].ReadOnly = false;
                //}
                //else if (string.Equals(status, "Maintenance", StringComparison.OrdinalIgnoreCase))
                //{
                //    e.Value = "Available";
                //    _dgvRooms.Rows[e.RowIndex].Cells[e.ColumnIndex].ReadOnly = false;
                //}
                //else
                //{
                //    // Optionally hide or disable for other statuses
                //    e.Value = "";
                //    _dgvRooms.Rows[e.RowIndex].Cells[e.ColumnIndex].ReadOnly = true;
                //}

                if (string.Equals(status, "Maintenance", StringComparison.OrdinalIgnoreCase))
                {
                    e.Value = "Available";
                    _dgvRooms.Rows[e.RowIndex].Cells[e.ColumnIndex].ReadOnly = false;
                }

                else
                {
                    e.Value = "Maintenance";
                    _dgvRooms.Rows[e.RowIndex].Cells[e.ColumnIndex].ReadOnly = false;
                }


                e.FormattingApplied = true;
            }
        }
    }
}
