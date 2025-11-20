using System;
using System.Drawing;
using System.Windows.Forms;
using HotelMgt.Custom; // Add this at the top

namespace HotelMgt.UIStyles
{
    public static class CheckOutViewBuilder
    {
        public static void Build(
            Control parent,
            out Label lblTitle,
            out Label lblSubtitle,
            out TextBox txtSearch,
            out RoundedButton btnSearch, // Change type
            out DataGridView dgvActiveCheckIns,
            out Panel panelCheckOutDetails,
            out Label lblGuestName,
            out Label lblRoomInfo,
            out Label lblCheckInDate,
            out Label lblCheckOutDate,
            out Label lblExpectedCheckOutDate,
            out Label lblNights,
            out Label lblRatePerNight,
            out TextBox txtExtra,
            out Label lblTotalAmount,
            out ComboBox cboPaymentMethod,
            out TextBox txtTransactionRef,
            out RoundedButton btnProcessCheckOut // Change type
        )
        {
            parent.SuspendLayout();
            parent.Controls.Clear();

            parent.BackColor = Color.FromArgb(240, 244, 248);
            parent.Dock = DockStyle.Fill;

            // Title
            lblTitle = new Label
            {
                Text = "Guest Check-Out",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            parent.Controls.Add(lblTitle);

            lblSubtitle = new Label
            {
                Text = "Process checkout and final payment",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Gray,
                Location = new Point(20, 55),
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            parent.Controls.Add(lblSubtitle);

            // Search (no label)
            txtSearch = new TextBox
            {
                Location = new Point(20, 100),
                Size = new Size(400, 25),
                Font = new Font("Segoe UI", 10),
#if NET6_0_OR_GREATER
                PlaceholderText = "Room number or guest name",
#endif
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            parent.Controls.Add(txtSearch);

            btnSearch = new RoundedButton
            {
                Text = "Search",
                Location = new Point(430, 98),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(37, 99, 235),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                BorderRadius = 10
            };
            btnSearch.FlatAppearance.BorderSize = 0;
            parent.Controls.Add(btnSearch);

            // Top grid: 3 visible rows, vertical scroll, themed
            dgvActiveCheckIns = new DataGridView
            {
                Name = "dgvActiveCheckIns",
                Location = new Point(20, 140),
                Size = new Size(parent.ClientSize.Width - 40, 180),
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                AllowUserToResizeColumns = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None,
                RowTemplate = { Height = 36 },
                DefaultCellStyle = { WrapMode = DataGridViewTriState.False },
                RowHeadersVisible = false,
                RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ScrollBars = ScrollBars.Vertical,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            GridTheme.ApplyStandard(dgvActiveCheckIns);
            parent.Controls.Add(dgvActiveCheckIns);

            // Helpers
            int CalcGridHeightForRows(DataGridView g, int rows)
            {
                int header = g.ColumnHeadersVisible ? g.ColumnHeadersHeight : 28;
                int rowH = Math.Max(24, g.RowTemplate.Height);
                return header + (rowH * rows) + 2;
            }
            void SizeGridToThreeRows(DataGridView g) => g.Height = CalcGridHeightForRows(g, 3);

            var mainGrid = dgvActiveCheckIns;
            mainGrid.DataBindingComplete += (_, __) => SizeGridToThreeRows(mainGrid);
            SizeGridToThreeRows(mainGrid);

            // Details panel
            panelCheckOutDetails = new Panel
            {
                Location = new Point(20, dgvActiveCheckIns.Bottom + 20),
                Size = new Size(parent.ClientSize.Width - 40, Math.Max(280, parent.ClientSize.Height - (dgvActiveCheckIns.Bottom + 40))),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Visible = false,
                AutoScroll = true, // Enable scrollbars if content overflows
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };
            parent.Controls.Add(panelCheckOutDetails);

            // Details layout
            var detailsLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5,
                Padding = new Padding(12),
                BackColor = Color.White,
                AutoSize = false // Prevents unwanted growth, allows fill
            };
            detailsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            detailsLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));          // title
            detailsLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));          // info rows
            detailsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));     // main split: info/desc + amenities
            detailsLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));          // payment row
            detailsLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));          // button row
            panelCheckOutDetails.Controls.Add(detailsLayout);

            // Row 0: Title
            var lblPanelTitle = new Label
            {
                Text = "Check-Out Details",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                AutoSize = true,
                Margin = new Padding(8, 4, 8, 8)
            };
            detailsLayout.Controls.Add(lblPanelTitle, 0, 0);

            // Row 1: Info rows
            var infoTable = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 2,
                RowCount = 9,
                Margin = new Padding(8, 0, 8, 8)
            };
            infoTable.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            infoTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            detailsLayout.Controls.Add(infoTable, 0, 1);

            static Label Caption(string text) => new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(100, 116, 139),
                AutoSize = true,
                Margin = new Padding(0, 2, 10, 2)
            };
            static Label ValueLabel() => new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                AutoSize = true,
                Margin = new Padding(0, 2, 0, 2)
            };

            // Build rows and assign OUT refs
            infoTable.Controls.Add(Caption("Guest:"), 0, 0);
            lblGuestName = ValueLabel(); infoTable.Controls.Add(lblGuestName, 1, 0);

            infoTable.Controls.Add(Caption("Room:"), 0, 1);
            lblRoomInfo = ValueLabel(); infoTable.Controls.Add(lblRoomInfo, 1, 1);

            infoTable.Controls.Add(Caption("Check-In Date:"), 0, 2);
            lblCheckInDate = ValueLabel(); infoTable.Controls.Add(lblCheckInDate, 1, 2);

            infoTable.Controls.Add(Caption("Check-Out Date:"), 0, 3);
            lblCheckOutDate = ValueLabel(); infoTable.Controls.Add(lblCheckOutDate, 1, 3);

            infoTable.Controls.Add(Caption("Expected Check-Out Date:"), 0, 4);
            lblExpectedCheckOutDate = ValueLabel(); infoTable.Controls.Add(lblExpectedCheckOutDate, 1, 4);

            infoTable.Controls.Add(Caption("Nights Stayed:"), 0, 5);
            lblNights = ValueLabel(); infoTable.Controls.Add(lblNights, 1, 5);

            infoTable.Controls.Add(Caption("Rate Per Night:"), 0, 6);
            lblRatePerNight = ValueLabel(); infoTable.Controls.Add(lblRatePerNight, 1, 6);

            // --- Total Amount row ---
            infoTable.Controls.Add(Caption("Total Amount:"), 0, 8);
            lblTotalAmount = ValueLabel(); infoTable.Controls.Add(lblTotalAmount, 1, 8);

            // --- Extra row (moved below Total Amount) ---
            infoTable.Controls.Add(Caption("Extra:"), 0, 9);
            var txtExtraLocal = new TextBox
            {
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                Width = 100, // Reduced width for a more compact input
                Margin = new Padding(0, 2, 0, 2),
                Anchor = AnchorStyles.Left,
                Text = "0.00"
            };
            infoTable.Controls.Add(txtExtraLocal, 1, 9);
            txtExtra = txtExtraLocal;

            // Numeric input and formatting behavior
            txtExtraLocal.GotFocus += (s, e) =>
            {
                if (txtExtraLocal.Text == "0.00")
                    txtExtraLocal.Clear();
            };
            txtExtraLocal.KeyPress += (s, e) =>
            {
                if (char.IsControl(e.KeyChar))
                    return;
                if (char.IsDigit(e.KeyChar))
                    return;
                if (e.KeyChar == '.' && !txtExtraLocal.Text.Contains("."))
                    return;
                e.Handled = true;
            };
            txtExtraLocal.Leave += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtExtraLocal.Text))
                {
                    txtExtraLocal.Text = "0.00";
                    return;
                }
                if (double.TryParse(txtExtraLocal.Text, out double val))
                    txtExtraLocal.Text = val.ToString("N2");
                else
                    txtExtraLocal.Text = "0.00";
            };

            // Row 2: Side-by-side "Description" (left) and "Selected Amenities" (right)
            var mainSplit = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Margin = new Padding(0, 0, 0, 8),
                BackColor = Color.White,
                AutoSize = false // Ensures it fills available space
            };
            mainSplit.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55f)); // left: info/desc
            mainSplit.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45f)); // right: amenities
            detailsLayout.Controls.Add(mainSplit, 0, 2);

            // --- LEFT COLUMN: infoTable + description ---
            var leftStack = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = Color.White,
                Margin = new Padding(0, 0, 8, 0),
                AutoSize = false // Ensures fill
            };
            leftStack.RowStyles.Add(new RowStyle(SizeType.AutoSize));      // infoTable
            leftStack.RowStyles.Add(new RowStyle(SizeType.AutoSize));      // desc label
            leftStack.RowStyles.Add(new RowStyle(SizeType.Percent, 100f)); // description box
            mainSplit.Controls.Add(leftStack, 0, 0);

            // Move infoTable here (already created above)
            leftStack.Controls.Add(infoTable, 0, 0);

            // Description label and box
            var descLabel = new Label
            {
                Text = "Extra Amenities",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                AutoSize = true,
                Margin = new Padding(0, 8, 0, 4)
            };
            leftStack.Controls.Add(descLabel, 0, 1);

            var txtDesc = new TextBox
            {
                Name = "txtCheckInDescription",
                Multiline = true,
                ReadOnly = true,
                Enabled = false,
                ScrollBars = ScrollBars.Vertical,
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 9),
                BackColor = Color.White,
                Margin = new Padding(0)
            };
            leftStack.Controls.Add(txtDesc, 0, 2);

            // --- RIGHT COLUMN: amenities label + grid ---
            var rightStack = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Color.White,
                Margin = new Padding(8, 0, 0, 0),
                AutoSize = false // Ensures fill
            };
            rightStack.RowStyles.Add(new RowStyle(SizeType.AutoSize));      // label
            rightStack.RowStyles.Add(new RowStyle(SizeType.Percent, 100f)); // grid
            mainSplit.Controls.Add(rightStack, 1, 0);

            var lblAmenities = new Label
            {
                Text = "Inclusions",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 6)
            };
            rightStack.Controls.Add(lblAmenities, 0, 0);

            var dgvAmenities = new DataGridView
            {
                Name = "dgvAmenities",
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.FromArgb(10, 34, 64), // Navy blue
                BorderStyle = BorderStyle.None,
                EnableHeadersVisualStyles = false,
                RowHeadersVisible = false,
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None,
                RowTemplate = { Height = 32 },
                ScrollBars = ScrollBars.Vertical,
                Margin = new Padding(0),
                TabStop = false // Prevents focus via Tab
            };
            // Prevent selection highlight and mouse interaction
            dgvAmenities.DefaultCellStyle.SelectionBackColor = dgvAmenities.DefaultCellStyle.BackColor;
            dgvAmenities.DefaultCellStyle.SelectionForeColor = dgvAmenities.DefaultCellStyle.ForeColor;
            dgvAmenities.CurrentCell = null;
            dgvAmenities.ClearSelection();
            dgvAmenities.Enabled = true; // Keep enabled for appearance

            // Suppress selection on mouse and keyboard
            dgvAmenities.SelectionChanged += (s, e) => dgvAmenities.ClearSelection();
            dgvAmenities.CellMouseDown += (s, e) => dgvAmenities.ClearSelection();
            dgvAmenities.CellMouseUp += (s, e) => dgvAmenities.ClearSelection();
            dgvAmenities.KeyDown += (s, e) => dgvAmenities.ClearSelection();
            rightStack.Controls.Add(dgvAmenities, 0, 1);

            // Row 3: Payment row
            var paymentRow = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 4,
                RowCount = 2,
                Margin = new Padding(8, 0, 8, 8)
            };
            paymentRow.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            paymentRow.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            paymentRow.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            paymentRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            detailsLayout.Controls.Add(paymentRow, 0, 3);

            var lblPayment = new Label
            {
                Text = "Payment Method *",
                Font = new Font("Segoe UI", 9),
                AutoSize = true,
                Margin = new Padding(0, 4, 8, 0)
            };
            paymentRow.Controls.Add(lblPayment, 0, 0);

            cboPaymentMethod = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10),
                Width = 220,
                Margin = new Padding(0, 0, 12, 0)
            };
            cboPaymentMethod.Items.AddRange(new object[] { "Cash", "Credit Card", "Debit Card" });
            cboPaymentMethod.SelectedIndex = 0;
            paymentRow.Controls.Add(cboPaymentMethod, 1, 0);

            var lblTransRef = new Label
            {
                Text = "Transaction Reference",
                Font = new Font("Segoe UI", 9),
                AutoSize = true,
                Margin = new Padding(0, 4, 8, 0)
            };
            paymentRow.Controls.Add(lblTransRef, 2, 0);

            txtTransactionRef = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Width = 280,
                Margin = new Padding(0, 0, 0, 0),
                Anchor = AnchorStyles.Left | AnchorStyles.Right
            };
            paymentRow.Controls.Add(txtTransactionRef, 3, 0);

            // Row 4: Button row
            var buttonRow = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 1,
                RowCount = 1,
                Margin = new Padding(8, 0, 8, 8)
            };
            buttonRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            detailsLayout.Controls.Add(buttonRow, 0, 4);

            btnProcessCheckOut = new RoundedButton
            {
                Text = "Process Check-Out",
                Size = new Size(520, 45),
                BackColor = Color.FromArgb(10, 34, 64),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Left,
                Margin = new Padding(0, 8, 0, 0),
                BorderRadius = 10
            };
            btnProcessCheckOut.FlatAppearance.BorderSize = 0;
            buttonRow.Controls.Add(btnProcessCheckOut, 0, 0);

            // Reflow
            var host = parent;
            var detailsPanelLocal = panelCheckOutDetails;
            var amenGrid = dgvAmenities;
            var descBox = txtDesc;

            int CalcAmenGridHeightForRows(DataGridView g, int rows) => CalcGridHeightForRows(g, rows);

            void Reflow()
            {
                // 1) Manage top grid height (3..1 rows)
                int targetTopRows = 3;
                for (int rows = 3; rows >= 1; rows--)
                {
                    int gridHeight = CalcGridHeightForRows(mainGrid, rows);
                    mainGrid.Height = gridHeight;

                    detailsPanelLocal.Top = mainGrid.Bottom + 20;
                    detailsPanelLocal.Height = Math.Max(200, host.ClientSize.Height - detailsPanelLocal.Top - 20);

                    int nonAmen = lblPanelTitle.Height + infoTable.Height + paymentRow.Height + buttonRow.Height
                                  + detailsLayout.Padding.Vertical + 24;

                    int minAmenHeight = CalcAmenGridHeightForRows(amenGrid, 1);
                    if (detailsPanelLocal.ClientSize.Height - nonAmen >= minAmenHeight)
                    {
                        targetTopRows = rows;
                        break;
                    }
                }
                mainGrid.Height = CalcGridHeightForRows(mainGrid, targetTopRows);
                detailsPanelLocal.Top = mainGrid.Bottom + 20;
                detailsPanelLocal.Height = Math.Max(200, host.ClientSize.Height - detailsPanelLocal.Top - 20);

                // 2) Size amenities grid to fit (1..3 rows) and mirror to description height
                int availableForAmen = detailsPanelLocal.ClientSize.Height
                                       - (lblPanelTitle.Height + infoTable.Height + paymentRow.Height + buttonRow.Height
                                          + detailsLayout.Padding.Vertical + 24);

                int header = amenGrid.ColumnHeadersVisible ? amenGrid.ColumnHeadersHeight : 28;
                int rowH = Math.Max(24, amenGrid.RowTemplate.Height);

                int maxRowsFit = 1;
                if (availableForAmen > header + 2)
                {
                    maxRowsFit = Math.Min(3, Math.Max(1, (availableForAmen - header - 2) / rowH));
                }
                int amenHeight = CalcAmenGridHeightForRows(amenGrid, maxRowsFit);
                amenGrid.Height = amenHeight;
                descBox.Height = amenHeight; // keep same visual height
            }

            host.Resize += (_, __) => Reflow();
            mainGrid.DataBindingComplete += (_, __) => Reflow();
            amenGrid.DataBindingComplete += (_, __) => Reflow();

            parent.BeginInvoke((Action)Reflow);

            parent.ResumeLayout(false);
            parent.PerformLayout();
        }
    }
}