using System;
using System.Windows.Forms;
using System.ComponentModel;

namespace HotelMgt.Forms
{
    public partial class RoomEditorForm : Form
    {
        private readonly TextBox txtRoomNumber;
        private readonly ComboBox cboRoomType;
        private readonly NumericUpDown nudPrice;
        private readonly NumericUpDown nudMaxOccupancy;
        private readonly ComboBox cboStatus;
        private readonly NumericUpDown nudFloor;
        private readonly TextBox txtAmenities;
        private readonly TextBox txtDescription;
        private readonly Button btnOK;
        private readonly Button btnCancel;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string RoomNumber
        {
            get => txtRoomNumber.Text;
            set => txtRoomNumber.Text = value ?? "";
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int Floor
        {
            get => (int)nudFloor.Value;
            set => nudFloor.Value = Math.Max(nudFloor.Minimum, Math.Min(nudFloor.Maximum, value));
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string RoomType
        {
            get => cboRoomType.SelectedItem?.ToString() ?? "";
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    if (!cboRoomType.Items.Contains(value))
                        cboRoomType.Items.Add(value);
                    cboRoomType.SelectedItem = value;
                }
                else
                {
                    cboRoomType.SelectedIndex = -1;
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public decimal PricePerNight
        {
            get => nudPrice.Value;
            set => nudPrice.Value = Math.Max(nudPrice.Minimum, Math.Min(nudPrice.Maximum, value));
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int MaxGuests
        {
            get => (int)nudMaxOccupancy.Value;
            set => nudMaxOccupancy.Value = Math.Max(nudMaxOccupancy.Minimum, Math.Min(nudMaxOccupancy.Maximum, value));
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Amenities
        {
            get => txtAmenities.Text;
            set => txtAmenities.Text = value ?? "";
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Description
        {
            get => txtDescription.Text;
            set => txtDescription.Text = value ?? "";
        }

        public void SetRoomTypes(string[] types)
        {
            cboRoomType.Items.Clear();
            if (types != null)
                cboRoomType.Items.AddRange(types);
        }

        public RoomEditorForm()
        {
            this.Text = "Room Editor";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.ClientSize = new System.Drawing.Size(500, 420);

            // TableLayoutPanel for two-column layout
            var table = new TableLayoutPanel
            {
                ColumnCount = 2,
                RowCount = 6,
                Dock = DockStyle.Top,
                Padding = new Padding(18, 18, 18, 0),
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            for (int i = 0; i < 6; i++)
                table.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));

            // Controls
            txtRoomNumber = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right };
            cboRoomType = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Anchor = AnchorStyles.Left | AnchorStyles.Right };
            nudPrice = new NumericUpDown { DecimalPlaces = 2, Maximum = 1000000, Minimum = 0, Anchor = AnchorStyles.Left | AnchorStyles.Right };
            nudMaxOccupancy = new NumericUpDown { Minimum = 1, Maximum = 20, Anchor = AnchorStyles.Left | AnchorStyles.Right };
            cboStatus = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Anchor = AnchorStyles.Left | AnchorStyles.Right };
            nudFloor = new NumericUpDown { Minimum = 1, Maximum = 100, Anchor = AnchorStyles.Left | AnchorStyles.Right };
            txtAmenities = new TextBox { Multiline = true, Height = 40, Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top };
            txtDescription = new TextBox { Multiline = true, Height = 60, Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top };
            btnOK = new Button { Text = "OK", Width = 90, DialogResult = DialogResult.OK, Anchor = AnchorStyles.Right };
            btnCancel = new Button { Text = "Cancel", Width = 90, DialogResult = DialogResult.Cancel, Anchor = AnchorStyles.Left };

            cboRoomType.Items.AddRange(new object[] { "Single", "Double", "Deluxe", "Suite" });
            cboStatus.Items.AddRange(new object[] { "Available", "Occupied", "Reserved", "Maintenance" });

            // Row 0: Room Number - Room Type
            table.Controls.Add(new Label { Text = "Room Number:", Anchor = AnchorStyles.Left, AutoSize = true }, 0, 0);
            table.Controls.Add(new Label { Text = "Room Type:", Anchor = AnchorStyles.Left, AutoSize = true }, 1, 0);
            table.Controls.Add(txtRoomNumber, 0, 1);
            table.Controls.Add(cboRoomType, 1, 1);

            // Row 2: Price/Night - Max Occupancy
            table.Controls.Add(new Label { Text = "Price/Night:", Anchor = AnchorStyles.Left, AutoSize = true }, 0, 2);
            table.Controls.Add(new Label { Text = "Max Occupancy:", Anchor = AnchorStyles.Left, AutoSize = true }, 1, 2);
            table.Controls.Add(nudPrice, 0, 3);
            table.Controls.Add(nudMaxOccupancy, 1, 3);

            // Row 4: Status - Floor
            table.Controls.Add(new Label { Text = "Status:", Anchor = AnchorStyles.Left, AutoSize = true }, 0, 4);
            table.Controls.Add(new Label { Text = "Floor:", Anchor = AnchorStyles.Left, AutoSize = true }, 1, 4);
            table.Controls.Add(cboStatus, 0, 5);
            table.Controls.Add(nudFloor, 1, 5);

            // Amenities (spans both columns)
            var lblAmenities = new Label { Text = "Amenities:", Anchor = AnchorStyles.Left, AutoSize = true, Top = table.Bottom + 10 };
            txtAmenities.Width = 440;
            txtAmenities.Margin = new Padding(0, 4, 0, 0);

            // Description (spans both columns)
            var lblDescription = new Label { Text = "Description:", Anchor = AnchorStyles.Left, AutoSize = true, Top = table.Bottom + 60 };
            txtDescription.Width = 440;
            txtDescription.Margin = new Padding(0, 4, 0, 0);

            // Buttons panel
            var buttonPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                Dock = DockStyle.Bottom,
                Padding = new Padding(0, 10, 18, 18),
                Height = 50,
                AutoSize = true
            };
            buttonPanel.Controls.Add(btnOK);
            buttonPanel.Controls.Add(btnCancel);

            // Add controls to form
            this.Controls.Add(buttonPanel);
            this.Controls.Add(txtDescription);
            this.Controls.Add(lblDescription);
            this.Controls.Add(txtAmenities);
            this.Controls.Add(lblAmenities);
            this.Controls.Add(table);

            // Position amenities and description below the table
            lblAmenities.Top = table.Bottom + 10;
            txtAmenities.Top = lblAmenities.Bottom + 2;
            lblDescription.Top = txtAmenities.Bottom + 10;
            txtDescription.Top = lblDescription.Bottom + 2;

            // Set left for amenities/description
            lblAmenities.Left = 18;
            txtAmenities.Left = 18;
            lblDescription.Left = 18;
            txtDescription.Left = 18;

            // Set width for amenities/description
            txtAmenities.Width = this.ClientSize.Width - 36;
            txtDescription.Width = this.ClientSize.Width - 36;

            // Set Accept/Cancel buttons
            this.AcceptButton = btnOK;
            this.CancelButton = btnCancel;

            btnOK.Click += btnOK_Click;
            btnCancel.Click += btnCancel_Click;
        }

        private void btnOK_Click(object? sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object? sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}