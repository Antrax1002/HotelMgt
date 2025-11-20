using System;
using System.Data;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using HotelMgt.Services;

namespace HotelMgt.Dialogs
{
    public sealed class GuestCheckInHistoryForm : Form
    {
        private readonly int _guestId;
        private readonly string _guestName;
        private readonly DatabaseService _db = new DatabaseService();

        private DataGridView _grid = null!;
        private Label _title = null!;
        private Label _empty = null!;
        private Label _count = null!;
        private Button _btnClose = null!;
        private Button _btnRefresh = null!;

        public GuestCheckInHistoryForm(int guestId, string guestName)
        {
            _guestId = guestId;
            _guestName = guestName;

            InitializeComponent();
            Load += (_, __) => LoadHistory();
            KeyPreview = true;
            KeyDown += (_, e) => { if (e.KeyCode == Keys.Escape) Close(); };
        }

        private void InitializeComponent()
        {
            Text = "Guest Check-In History";
            Size = new Size(880, 560);
            // Make the window non-resizable
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.White;
            Font = new Font("Segoe UI", 9F);

            _title = new Label
            {
                Text = $"Check-In History — {_guestName}",
                Font = new Font("Segoe UI", 15F, FontStyle.Bold),
                Location = new Point(18, 14),
                AutoSize = true
            };
            Controls.Add(_title);

            _btnRefresh = new Button
            {
                Text = "Refresh",
                Location = new Point(18, 52),
                Size = new Size(90, 30),
                BackColor = Color.FromArgb(37, 99, 235),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _btnRefresh.FlatAppearance.BorderSize = 0;
            _btnRefresh.Click += (_, __) => LoadHistory();
            Controls.Add(_btnRefresh);

            _btnClose = new Button
            {
                Text = "Close",
                Location = new Point(118, 52),
                Size = new Size(90, 30),
                BackColor = Color.FromArgb(220, 38, 38),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _btnClose.FlatAppearance.BorderSize = 0;
            _btnClose.Click += (_, __) => Close();
            Controls.Add(_btnClose);

            _count = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                ForeColor = Color.FromArgb(55, 65, 81),
                Location = new Point(220, 59),
                AutoSize = true
            };
            Controls.Add(_count);

            _grid = new DataGridView
            {
                Location = new Point(18, 92),
                Size = new Size(ClientSize.Width - 36, ClientSize.Height - 140),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                AllowUserToResizeColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoGenerateColumns = false,
                ScrollBars = ScrollBars.Both // allow scrolling instead of forcing window resize
            };
            Controls.Add(_grid);

            _empty = new Label
            {
                Text = "No check-in history found for this guest.",
                Font = new Font("Segoe UI", 10F, FontStyle.Italic),
                ForeColor = Color.DimGray,
                AutoSize = true,
                Location = new Point(22, 100),
                Visible = false
            };
            Controls.Add(_empty);

            // Columns (named for safe lookup)
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "CheckInDateTime", DataPropertyName = "CheckInDateTime", HeaderText = "Checked In", MinimumWidth = 120 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "ExpectedCheckOutDate", DataPropertyName = "ExpectedCheckOutDate", HeaderText = "Expected Out", MinimumWidth = 110 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "ActualCheckOutDateTime", DataPropertyName = "ActualCheckOutDateTime", HeaderText = "Actual Out", MinimumWidth = 110 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "RoomNumber", DataPropertyName = "RoomNumber", HeaderText = "Room", MinimumWidth = 80 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "NumberOfGuests", DataPropertyName = "NumberOfGuests", HeaderText = "Guests", MinimumWidth = 80 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "ReservationCode", DataPropertyName = "ReservationCode", HeaderText = "Reservation", MinimumWidth = 110 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Notes", DataPropertyName = "Notes", HeaderText = "Notes", MinimumWidth = 180 });

            // Per-column autosize strategy:
            // - All non-Notes columns size to their content (AllCells)
            // - Notes fills remaining width and wraps
            foreach (DataGridViewColumn col in _grid.Columns)
            {
                col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                if (col.Name == "Notes")
                {
                    col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    col.FillWeight = 50;
                }
                else
                {
                    col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                }
            }

            ApplyGridTheme();
            _grid.CellFormatting += Grid_CellFormatting;
            _grid.DataBindingComplete += (_, __) =>
            {
                // Let columns compute widths based on content (non-Notes)
                _grid.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
                // Ensure rows expand for wrapped Notes
                _grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
                UpdateCount();
            };
        }

        private void ApplyGridTheme()
        {
            TryEnableDoubleBuffering(_grid);

            _grid.BorderStyle = BorderStyle.None;
            _grid.BackgroundColor = Color.White;
            _grid.GridColor = Color.FromArgb(229, 231, 235);
            _grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            _grid.RowHeadersVisible = false;

            _grid.EnableHeadersVisualStyles = false;
            _grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            _grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(37, 99, 235);
            _grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            _grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            _grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(6, 4, 6, 4);
            _grid.ColumnHeadersHeight = 40;

            _grid.DefaultCellStyle.Font = new Font("Segoe UI", 10F);
            _grid.DefaultCellStyle.ForeColor = Color.FromArgb(31, 41, 55);
            _grid.DefaultCellStyle.BackColor = Color.White;
            _grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            _grid.DefaultCellStyle.SelectionForeColor = Color.FromArgb(30, 64, 175);
            _grid.DefaultCellStyle.Padding = new Padding(8, 6, 8, 6);
            _grid.DefaultCellStyle.WrapMode = DataGridViewTriState.False;

            _grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);

            // Base row height; autosize will grow as needed
            _grid.RowTemplate.Height = 24;

            // Wrap only Notes; taller padding for readability
            var notesCol = GetColumn("Notes");
            if (notesCol != null)
            {
                notesCol.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                notesCol.DefaultCellStyle.Padding = new Padding(8, 8, 8, 8);
            }

            // Prefer vertical top alignment for multi-line content
            _grid.DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopLeft;
            foreach (DataGridViewColumn col in _grid.Columns)
                col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopLeft;

            _grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
        }

        private static void TryEnableDoubleBuffering(DataGridView g)
        {
            try
            {
                typeof(DataGridView)
                    .GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic)?
                    .SetValue(g, true);
            }
            catch { }
        }

        private DataGridViewColumn? GetColumn(string nameOrDataProperty)
        {
            foreach (DataGridViewColumn c in _grid.Columns)
            {
                if (string.Equals(c.Name, nameOrDataProperty, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(c.DataPropertyName, nameOrDataProperty, StringComparison.OrdinalIgnoreCase))
                    return c;
            }
            return null;
        }

        private void Grid_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex < 0 || e.RowIndex < 0) return;

            var col = _grid.Columns[e.ColumnIndex].DataPropertyName;

            if (e.Value == null || e.Value == DBNull.Value)
            {
                if (col is "ActualCheckOutDateTime" or "ReservationCode" or "Notes")
                    e.Value = "";
                return;
            }

            if (col is "CheckInDateTime" or "ExpectedCheckOutDate" or "ActualCheckOutDateTime")
            {
                if (e.Value is DateTime dt)
                    e.Value = dt.ToString("yyyy-MM-dd HH:mm");
            }
        }

        private void UpdateCount()
        {
            if (_grid.DataSource is DataTable dt)
                _count.Text = $"Records: {dt.Rows.Count}";
            else
                _count.Text = "Records: 0";
        }

        private void LoadHistory()
        {
            try
            {
                using var conn = _db.GetConnection();
                conn.Open();

                const string sql = @"
SELECT 
    ci.CheckInID,
    ci.CheckInDateTime,
    ci.ExpectedCheckOutDate,
    ci.ActualCheckOutDateTime,
    ci.NumberOfGuests,
    ci.Notes,
    rm.RoomNumber,
    r.ReservationCode
FROM CheckIns ci
LEFT JOIN Rooms rm ON ci.RoomID = rm.RoomID
LEFT JOIN Reservations r ON ci.ReservationID = r.ReservationID
WHERE ci.GuestID = @GuestID
ORDER BY ci.CheckInDateTime DESC;";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@GuestID", _guestId);

                using var adapter = new SqlDataAdapter(cmd);
                var table = new DataTable();
                adapter.Fill(table);

                _grid.DataSource = table;
                _empty.Visible = table.Rows.Count == 0;

                // After binding, let columns size to content (non-Notes) and rows wrap Notes
                _grid.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
                _grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

                UpdateCount();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Failed to load check-in history.\n\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}