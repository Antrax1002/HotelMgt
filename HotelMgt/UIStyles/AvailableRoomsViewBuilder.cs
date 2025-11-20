using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HotelMgt.Custom;

namespace HotelMgt.UIStyles
{
    public static class AvailableRoomsViewBuilder
    {
        // Builds the Available Rooms UI and returns control references
        public static void Build(
            Control parent,
            out Label lblTitle,
            out Label lblSubtitle,
            out Label lblSummary,
            out TextBox txtSearch,
            out ComboBox cboFilterStatus,
            out ComboBox cboFilterType,
            out DataGridView dgvRooms,
            out RoundedPanel pnlAmenities,
            out Label lblAmenitiesTitle,
            out Label lblAmenitiesText,
            out Label lblDescriptionTitle,
            out Label lblDescriptionText)
        {
            parent.SuspendLayout();
            parent.Controls.Clear();
            parent.BackColor = Color.White;

            // Title + Subtitle
            lblTitle = new Label
            {
                Text = "Room Inventory",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true
            };
            parent.Controls.Add(lblTitle);

            lblSubtitle = new Label
            {
                Text = "View and filter all hotel rooms",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Gray,
                Location = new Point(20, 55),
                AutoSize = true
            };
            parent.Controls.Add(lblSubtitle);

            // Section labels
            int y = 95;
            var lblSearchTitle = new Label { Text = "Search Rooms", Font = new Font("Segoe UI", 9), Location = new Point(20, y), AutoSize = true };
            parent.Controls.Add(lblSearchTitle);
            var lblStatusTitle = new Label { Text = "Filter by Status", Font = new Font("Segoe UI", 9), Location = new Point(340, y), AutoSize = true };
            parent.Controls.Add(lblStatusTitle);
            var lblTypeTitle = new Label { Text = "Filter by Type", Font = new Font("Segoe UI", 9), Location = new Point(620, y), AutoSize = true };
            parent.Controls.Add(lblTypeTitle);

            // Filters
            txtSearch = new TextBox
            {
                Location = new Point(20, y + 22),
                Size = new Size(300, 25),
                Font = new Font("Segoe UI", 10),
                PlaceholderText = "Search by room number"
            };
            parent.Controls.Add(txtSearch);

            cboFilterStatus = new ComboBox
            {
                Location = new Point(340, y + 22),
                Size = new Size(220, 25),
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboFilterStatus.Items.AddRange(new object[] { "All Statuses", "Available", "Occupied", "Reserved", "Maintenance" });
            cboFilterStatus.SelectedIndex = 0;
            parent.Controls.Add(cboFilterStatus);

            cboFilterType = new ComboBox
            {
                Location = new Point(620, y + 22),
                Size = new Size(220, 25),
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboFilterType.Items.AddRange(new object[] { "All Types", "Single", "Double", "Suite", "Deluxe" });
            cboFilterType.SelectedIndex = 0;
            parent.Controls.Add(cboFilterType);

            lblSummary = new Label
            {
                Text = "Showing 0 of 0 rooms",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(100, 116, 139),
                Location = new Point(20, y + 22 + 35),
                AutoSize = true
            };
            parent.Controls.Add(lblSummary);

            // Layout constants
            const int leftMargin = 20;
            const int rightMargin = 20;
            const int gap = 20;
            const int amenitiesWidth = 280;      // compact fixed width
            const int amenitiesMinWidth = 240;   // clamp on very small widths
            const int amenitiesMinHeight = 160;  // minimum panel height
            const int amenitiesMaxHeight = int.MaxValue;  // allow growth up to grid height
            const int gridMinWidth = 400;        // keep grid usable
            const int bottomMargin = 20;

            // Grid + amenities layout (initial placement, then we also handle resize)
            int gridX = leftMargin;
            int gridY = y + 22 + 60;

            // Compute sizes based on current parent size
            int availableWidth = Math.Max(0, parent.ClientSize.Width - leftMargin - rightMargin);
            int targetAmenities = Math.Max(amenitiesMinWidth, amenitiesWidth);
            int gridWidth = Math.Max(gridMinWidth, availableWidth - targetAmenities - gap);
            int gridHeight = Math.Max(200, parent.ClientSize.Height - gridY - bottomMargin);

            dgvRooms = new DataGridView
            {
                Location = new Point(gridX, gridY),
                Size = new Size(gridWidth, gridHeight),
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing,
                DefaultCellStyle = { WrapMode = DataGridViewTriState.True },
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells,
                AllowUserToResizeColumns = false,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing
            };
            parent.Controls.Add(dgvRooms);

            // Apply the shared grid theme + double buffering (matches Employee/Room)
            GridTheme.ApplyStandard(dgvRooms);
            var prop = typeof(Control).GetProperty(
                "DoubleBuffered",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            prop?.SetValue(dgvRooms, true, null);

            // Compact, fixed-width amenities panel (height will be adjusted to content)
            pnlAmenities = new RoundedPanel
            {
                BorderRadius = 12,
                BackColor = Color.FromArgb(245, 248, 255),
                Location = new Point(dgvRooms.Right + gap, gridY),
                Size = new Size(targetAmenities, amenitiesMinHeight),
                AutoScroll = true // enable overflow scrolling for very long content
            };
            // Darker blue outline for stronger contrast
            AttachRoundedBorder(pnlAmenities, 12, Color.FromArgb(59, 130, 246)); // Blue 500
            parent.Controls.Add(pnlAmenities);

            lblAmenitiesTitle = new Label
            {
                Text = "Amenities",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                Location = new Point(14, 12),
                AutoSize = true
            };
            pnlAmenities.Controls.Add(lblAmenitiesTitle);

            lblAmenitiesText = new Label
            {
                Text = "Select a room to view amenities.",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(55, 65, 81),
                Location = new Point(16, 44),
                AutoSize = true,
                MaximumSize = new Size(pnlAmenities.Width - 32, 0)
            };
            pnlAmenities.Controls.Add(lblAmenitiesText);

            // Description section below amenities
            lblDescriptionTitle = new Label
            {
                Text = "Description",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                Location = new Point(14, lblAmenitiesText.Bottom + 16),
                AutoSize = true
            };
            pnlAmenities.Controls.Add(lblDescriptionTitle);

            lblDescriptionText = new Label
            {
                Text = "Select a room to view description.",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(55, 65, 81),
                Location = new Point(16, lblDescriptionTitle.Bottom + 6),
                AutoSize = true,
                MaximumSize = new Size(pnlAmenities.Width - 32, 0)
            };
            pnlAmenities.Controls.Add(lblDescriptionText);

            // Copy out params to locals for use in lambdas (required by C#)
            var gridRef = dgvRooms;
            var panelRef = pnlAmenities;
            var amenRef = lblAmenitiesText;
            var descTitleRef = lblDescriptionTitle;
            var descRef = lblDescriptionText;

            // Reflow handler using locals
            void ReflowLocal()
            {
                ReflowAmenitiesPanel(panelRef, amenRef, descTitleRef, descRef);
            }

            // Centralized layout routine to keep grid wide and panel compact (height reduced)
            void LayoutNow()
            {
                int availW = Math.Max(0, parent.ClientSize.Width - leftMargin - rightMargin);
                int panelW = Math.Max(amenitiesMinWidth, amenitiesWidth);
                int gWidth = Math.Max(gridMinWidth, availW - panelW - gap);
                int gHeight = Math.Max(200, parent.ClientSize.Height - gridY - bottomMargin);

                // Grid takes most space
                gridRef.Location = new Point(gridX, gridY);
                gridRef.Size = new Size(gWidth, gHeight);

                // First, set panel width and a temporary height to allow wrapping computation
                panelRef.Location = new Point(gridRef.Right + gap, gridY);
                panelRef.Size = new Size(panelW, gHeight);

                // Update wrapping widths and flow for accurate measurement
                ReflowLocal();

                // Measure desired content height and clamp to grid height
                int desired = descRef.Bottom + 16; // content bottom + padding
                int clamped = Math.Clamp(desired, amenitiesMinHeight, gHeight);
                panelRef.Size = new Size(panelW, clamped);
            }

            // Hook changes that affect layout using locals
            parent.Resize += (_, __) => LayoutNow();
            amenRef.SizeChanged += (_, __) => LayoutNow();
            amenRef.TextChanged += (_, __) => LayoutNow();
            descRef.SizeChanged += (_, __) => LayoutNow();
            descRef.TextChanged += (_, __) => LayoutNow();

            // Initial layout
            LayoutNow();

            parent.ResumeLayout(false);
            parent.PerformLayout();
        }

        // Public so callers can force a reflow after changing label text
        public static void ReflowAmenitiesPanel(RoundedPanel panel, Label lblAmenitiesText, Label lblDescriptionTitle, Label lblDescriptionText)
        {
            lblAmenitiesText.MaximumSize = new Size(panel.Width - 32, 0);
            lblDescriptionTitle.Location = new Point(14, lblAmenitiesText.Bottom + 16);
            lblDescriptionText.MaximumSize = new Size(panel.Width - 32, 0);
            lblDescriptionText.Location = new Point(16, lblDescriptionTitle.Bottom + 6);

            // Ensure z-order is correct
            lblDescriptionTitle.BringToFront();
            lblDescriptionText.BringToFront();
        }

        private static void AttachRoundedBorder(Control ctrl, int radius, Color borderColor)
        {
            ctrl.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.PixelOffsetMode = PixelOffsetMode.Half;

                // Draw the main rounded border fully inside the client area
                var rect = ctrl.ClientRectangle;
                rect.Inflate(-1, -1); // keep stroke inside to avoid right/bottom clipping
                using (var path = GetRoundedRectPath(rect, radius))
                using (var pen = new Pen(borderColor, 1f) { Alignment = PenAlignment.Inset })
                {
                    g.DrawPath(pen, path);
                }

                // Subtle bottom/right accent to enhance edge visibility
                var accent = Color.FromArgb(
                    Math.Min(255, (int)(borderColor.A * 0.9)),
                    Math.Max(0, borderColor.R - 15),
                    Math.Max(0, borderColor.G - 15),
                    Math.Max(0, borderColor.B - 15));

                using var accentPen = new Pen(accent, 1f) { Alignment = PenAlignment.Inset };

                // Straight segments between the rounded corners
                g.DrawLine(accentPen, rect.Left + radius, rect.Bottom, rect.Right - radius, rect.Bottom); // bottom
                g.DrawLine(accentPen, rect.Right, rect.Top + radius, rect.Right, rect.Bottom - radius);   // right
            };

            // Ensure redraw on size changes
            ctrl.Resize += (_, __) => ctrl.Invalidate();
        }

        private static GraphicsPath GetRoundedRectPath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            int d = radius * 2;
            var arc = new Rectangle(rect.X, rect.Y, d, d);
            path.AddArc(arc, 180, 90);
            arc.X = rect.Right - d;
            path.AddArc(arc, 270, 90);
            arc.Y = rect.Bottom - d;
            path.AddArc(arc, 0, 90);
            arc.X = rect.Left;
            path.AddArc(arc, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
