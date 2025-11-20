using System.Drawing;
using System.Windows.Forms;
using HotelMgt.Custom;

namespace HotelMgt.UserControls.Admin
{
    partial class RoomManagementControl
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            _headerPanel = new RoundedPanel();
            _lblTitle = new Label();
            _lblDesc = new Label();
            _btnAddRoom = new Button();
            _dgvRooms = new DataGridView();

            SuspendLayout();

            // this (UserControl)
            BackColor = Color.FromArgb(244, 246, 250);
            Dock = DockStyle.Fill;

            // _headerPanel
            _headerPanel.BackColor = Color.White;
            _headerPanel.BorderRadius = 12;
            _headerPanel.Location = new Point(20, 20);
            _headerPanel.Size = new Size(Width - 40, 90);
            _headerPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            // _lblTitle
            _lblTitle.Text = "Room Management";
            _lblTitle.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            _lblTitle.ForeColor = Color.FromArgb(30, 41, 59);
            _lblTitle.AutoSize = true;
            _lblTitle.Location = new Point(15, 12);

            // _lblDesc
            _lblDesc.Text = "Add, edit and manage hotel rooms and pricing";
            _lblDesc.Font = new Font("Segoe UI", 9, FontStyle.Regular);
            _lblDesc.ForeColor = Color.FromArgb(100, 116, 139);
            _lblDesc.AutoSize = true;
            _lblDesc.Location = new Point(15, 42);

            // _btnAddRoom
            _btnAddRoom.Text = "Add Room";
            _btnAddRoom.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            _btnAddRoom.BackColor = Color.Black;
            _btnAddRoom.ForeColor = Color.White;
            _btnAddRoom.FlatStyle = FlatStyle.Flat;
            _btnAddRoom.FlatAppearance.BorderSize = 0;
            _btnAddRoom.Size = new Size(150, 34);
            _btnAddRoom.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            _btnAddRoom.Location = new Point(_headerPanel.Width - _btnAddRoom.Width - 15, 28);

            // keep button right-aligned when header panel resizes
            _headerPanel.Resize += (_, __) =>
            {
                _btnAddRoom.Location = new Point(_headerPanel.ClientSize.Width - _btnAddRoom.Width - 15, 28);
            };

            _headerPanel.Controls.AddRange(new Control[]
            {
                _lblTitle, _lblDesc, _btnAddRoom
            });

            // _dgvRooms
            _dgvRooms.Location = new Point(20, _headerPanel.Bottom + 12);
            _dgvRooms.Size = new Size(Width - 40, Height - _headerPanel.Bottom - 32);
            _dgvRooms.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;

            _dgvRooms.BackgroundColor = Color.White;
            _dgvRooms.BorderStyle = BorderStyle.None;
            _dgvRooms.ReadOnly = true;
            _dgvRooms.AllowUserToAddRows = false;
            _dgvRooms.AllowUserToDeleteRows = false;
            _dgvRooms.AllowUserToResizeRows = false;
            _dgvRooms.RowHeadersVisible = false;
            _dgvRooms.MultiSelect = false;
            _dgvRooms.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _dgvRooms.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _dgvRooms.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            _dgvRooms.EnableHeadersVisualStyles = false;
            _dgvRooms.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(243, 244, 246);
            _dgvRooms.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(55, 65, 81);
            _dgvRooms.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            _dgvRooms.DefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Regular);
            _dgvRooms.DefaultCellStyle.ForeColor = Color.FromArgb(30, 41, 59);
            _dgvRooms.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            _dgvRooms.DefaultCellStyle.SelectionForeColor = Color.FromArgb(30, 41, 59);
            _dgvRooms.GridColor = Color.FromArgb(215, 220, 230);
            _dgvRooms.AllowUserToOrderColumns = false;
            _dgvRooms.AutoGenerateColumns = false;

            // Data columns
            var colId = new DataGridViewTextBoxColumn { HeaderText = "ID", DataPropertyName = "RoomID", FillWeight = 50, Name = "colId" };
            var colNumber = new DataGridViewTextBoxColumn { HeaderText = "Room", DataPropertyName = "RoomNumber", FillWeight = 90, Name = "colNumber" };
            var colType = new DataGridViewTextBoxColumn { HeaderText = "Type", DataPropertyName = "RoomType", FillWeight = 110, Name = "colType" };
            var colFloor = new DataGridViewTextBoxColumn { HeaderText = "Floor", DataPropertyName = "Floor", FillWeight = 70, Name = "colFloor" };
            var colPrice = new DataGridViewTextBoxColumn { HeaderText = "Price/Night", DataPropertyName = "PriceText", FillWeight = 110, Name = "colPrice" };
            var colMax = new DataGridViewTextBoxColumn { HeaderText = "Max Guests", DataPropertyName = "MaxOccupancy", FillWeight = 90, Name = "colMaxGuests" };
            var colStatus = new DataGridViewTextBoxColumn { HeaderText = "Status", DataPropertyName = "Status", FillWeight = 90, Name = "colStatus" };
            var colAmenities = new DataGridViewTextBoxColumn { HeaderText = "Amenities", DataPropertyName = "Amenities", FillWeight = 200, Name = "colAmenities" };

            _dgvRooms.Columns.AddRange(new DataGridViewColumn[]
            {
                colId, colNumber, colType, colFloor, colPrice, colMax, colStatus, colAmenities
            });

            // Legacy placeholder Action column (will be replaced at runtime)
            var colAction = new DataGridViewButtonColumn
            {
                HeaderText = "Action",
                Text = "Edit",
                UseColumnTextForButtonValue = true,
                FillWeight = 80,
                Name = "colAction"
            };
            _dgvRooms.Columns.Add(colAction);

            // Add to control
            Controls.Add(_headerPanel);
            Controls.Add(_dgvRooms);

            ResumeLayout(false);
        }
    }
}
