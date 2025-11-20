using System;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.Data.SqlClient; // Use Microsoft.Data.SqlClient only
using HotelMgt.Services;
using System.Globalization; // ADD
using HotelMgt.Custom; // RoundedPanel
using HotelMgt.UIStyles; // ADD
using HotelMgt.Core.Events;

namespace HotelMgt.UserControls.Employee
{
    public partial class AvailableRoomsControl : UserControl
    {
        private readonly DatabaseService _dbService;
        private readonly CultureInfo _currencyCulture = CultureInfo.GetCultureInfo("en-PH");

        // UI refs (logic keeps these)
        private Label lblTitle = null!, lblSubtitle = null!;
        private Label lblSummary = null!;
        private TextBox txtSearch = null!;
        private ComboBox cboFilterStatus = null!, cboFilterType = null!;
        private DataGridView dgvRooms = null!;

        // Amenities side panel
        private RoundedPanel pnlAmenities = null!;
        private Label lblAmenitiesTitle = null!;
        private Label lblAmenitiesText = null!;
        private Label lblDescriptionTitle = null!; // NEW
        private Label lblDescriptionText = null!;  // NEW

        private readonly BindingSource _roomsSource = new();

        public AvailableRoomsControl()
        {
            InitializeComponent();
            _dbService = new DatabaseService();

            this.Load += AvailableRoomsControl_Load;

            // Subscribe to cross-module room changes and refresh this view
            RoomEvents.RoomsChanged += OnRoomsChanged;
            Disposed += (_, __) => RoomEvents.RoomsChanged -= OnRoomsChanged;
        }

        private void OnRoomsChanged(object? sender, RoomsChangedEventArgs e)
        {
            // Marshal to UI thread if needed
            if (InvokeRequired)
            {
                BeginInvoke(new Action(LoadRooms));
                return;
            }
            LoadRooms();
        }

        private void AvailableRoomsControl_Load(object? sender, EventArgs e)
        {
            // Build UI via UIStyles builder
            AvailableRoomsViewBuilder.Build(
                this,
                out lblTitle,
                out lblSubtitle,
                out lblSummary,
                out txtSearch,
                out cboFilterStatus,
                out cboFilterType,
                out dgvRooms,
                out pnlAmenities,
                out lblAmenitiesTitle,
                out lblAmenitiesText,
                out lblDescriptionTitle,
                out lblDescriptionText);

            // Bind the DataGridView after it exists
            dgvRooms.DataSource = _roomsSource;

            // Wire events to logic
            txtSearch.TextChanged += (_, __) => LoadRooms();
            cboFilterStatus.SelectedIndexChanged += (_, __) => LoadRooms();
            cboFilterType.SelectedIndexChanged += (_, __) => LoadRooms();
            dgvRooms.SelectionChanged += DgvRooms_SelectionChanged;

            LoadRoomTypes();
            LoadRooms();
        }

        private void LoadRoomTypes() { }

        private void LoadRooms()
        {
            try
            {
                using var conn = _dbService.GetConnection();
                conn.Open();

                var sb = new StringBuilder(@"
                    SELECT 
                        RoomID,
                        RoomNumber,
                        RoomType,
                        PricePerNight,
                        MaxOccupancy,
                        Status,
                        Floor,
                        Amenities,
                        Description
                    FROM Rooms
                    WHERE 1=1");

                using var cmd = new SqlCommand { Connection = conn };

                var term = txtSearch.Text.Trim();
                if (!string.IsNullOrEmpty(term))
                {
                    sb.Append(" AND RoomNumber LIKE @Search");
                    cmd.Parameters.AddWithValue("@Search", $"%{term}%");
                }

                if (cboFilterStatus.SelectedIndex > 0)
                {
                    sb.Append(" AND Status = @Status");
                    cmd.Parameters.AddWithValue("@Status", Convert.ToString(cboFilterStatus.SelectedItem) ?? string.Empty);
                }

                if (cboFilterType.SelectedIndex > 0)
                {
                    sb.Append(" AND RoomType = @RoomType");
                    cmd.Parameters.AddWithValue("@RoomType", Convert.ToString(cboFilterType.SelectedItem) ?? string.Empty);
                }

                sb.Append(" ORDER BY RoomNumber");
                cmd.CommandText = sb.ToString();

                using var adapter = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                adapter.Fill(dt);

                // Bind via BindingSource (keeps the grid instance stable)
                _roomsSource.SuspendBinding();
                _roomsSource.DataSource = dt;
                _roomsSource.ResumeBinding();

                // Safe column access pattern
                var cols = dgvRooms.Columns;

                if (cols["RoomID"] is { } colRoomId)
                    colRoomId.Visible = false;

                if (cols["RoomNumber"] is { } colRoomNumber)
                {
                    colRoomNumber.HeaderText = "Room";
                    colRoomNumber.FillWeight = 85;
                    colRoomNumber.MinimumWidth = 90;
                    colRoomNumber.Resizable = DataGridViewTriState.False;
                }

                if (cols["RoomType"] is { } colRoomType)
                {
                    colRoomType.HeaderText = "Type";
                    colRoomType.FillWeight = 80;
                    colRoomType.MinimumWidth = 90;
                    colRoomType.Resizable = DataGridViewTriState.False;
                }

                if (cols["Floor"] is { } colFloor)
                {
                    colFloor.HeaderText = "Floor";
                    colFloor.FillWeight = 60;
                    colFloor.MinimumWidth = 70;
                    colFloor.Resizable = DataGridViewTriState.False;
                }

                if (cols["MaxOccupancy"] is { } colMaxOcc)
                {
                    colMaxOcc.HeaderText = "Max Guests";
                    colMaxOcc.FillWeight = 90;
                    colMaxOcc.MinimumWidth = 100;
                    colMaxOcc.Resizable = DataGridViewTriState.False;
                }

                if (cols["Status"] is { } colStatus)
                {
                    colStatus.HeaderText = "Status";
                    colStatus.FillWeight = 110;
                    colStatus.Resizable = DataGridViewTriState.False;
                }

                if (cols["PricePerNight"] is { } colPrice)
                {
                    colPrice.HeaderText = "Rate/Night";
                    colPrice.DefaultCellStyle.Format = "C2";
                    colPrice.DefaultCellStyle.FormatProvider = _currencyCulture;
                    colPrice.FillWeight = 120;
                    colPrice.MinimumWidth = 120;
                    colPrice.Resizable = DataGridViewTriState.False;
                }

                // Hide details in table; show on right
                if (cols["Amenities"] is { } colAmenities)
                    colAmenities.Visible = false;
                if (cols["Description"] is { } colDescription)
                    colDescription.Visible = false;

                // Summary
                int totalRooms;
                using (var countCmd = new SqlCommand("SELECT COUNT(*) FROM Rooms", conn))
                {
                    totalRooms = Convert.ToInt32(countCmd.ExecuteScalar());
                }
                lblSummary.Text = $"Showing {dt.Rows.Count} of {totalRooms} rooms";

                // Initialize side panel content and layout
                UpdateSidePanelFromSelection();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading rooms: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvRooms_SelectionChanged(object? sender, EventArgs e)
        {
            UpdateSidePanelFromSelection();
        }

        private void UpdateSidePanelFromSelection()
        {
            if (dgvRooms?.CurrentRow?.DataBoundItem is DataRowView drv)
            {
                var amenities = Convert.ToString(drv["Amenities"]);
                var description = Convert.ToString(drv["Description"]);
                lblAmenitiesText.Text = string.IsNullOrWhiteSpace(amenities) ? "No amenities listed." : FormatAmenitiesList(amenities);
                lblDescriptionText.Text = string.IsNullOrWhiteSpace(description) ? "No description." : description;
            }
            else
            {
                lblAmenitiesText.Text = "Select a room to view amenities.";
                lblDescriptionText.Text = "Select a room to view description.";
            }

            // Force reflow so Description is always beneath Amenities
            AvailableRoomsViewBuilder.ReflowAmenitiesPanel(pnlAmenities, lblAmenitiesText, lblDescriptionTitle, lblDescriptionText);
        }

        private static string FormatAmenitiesList(string raw)
        {
            var parts = raw
                .Replace("\r", " ")
                .Replace("\n", " ")
                .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);

            var sb = new StringBuilder();
            foreach (var p in parts)
            {
                var item = p.Trim();
                if (item.Length == 0) continue;
                sb.AppendLine($"• {item}");
            }

            return sb.Length == 0 ? "No amenities listed." : sb.ToString().TrimEnd();
        }
    }
}