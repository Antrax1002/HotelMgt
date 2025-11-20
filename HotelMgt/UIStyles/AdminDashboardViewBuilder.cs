using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using HotelMgt.Custom;
using HotelMgt.otherUI;

namespace HotelMgt.UIStyles
{
    public static class AdminDashboardViewBuilder
    {
        // HEADER

        public static void BuildHeader(
            Panel panelHeader,
            out RoundedPanel headerLogoPanel)
        {
            headerLogoPanel = new RoundedPanel
            {
                BorderRadius = 10,
                BackColor = Color.FromArgb(37, 99, 160),
                Size = new Size(44, 44),
                Margin = Padding.Empty
            };

            var logoImage = LoadEmployeeLogoImage();
            headerLogoPanel.Tag = logoImage;

            headerLogoPanel.Paint -= HeaderLogoPanel_Paint;
            headerLogoPanel.Paint += HeaderLogoPanel_Paint;

            panelHeader.Controls.Add(headerLogoPanel);
            headerLogoPanel.BringToFront();
        }

        // NEW: one-shot header initializer (build + layout + event wiring + role badge styling)
        public static void InitializeHeader(
            Panel panelHeader,
            Label lblTitle,
            Label lblWelcome,
            Button btnLogout,
            out RoundedPanel headerLogoPanel)
        {
            BuildHeader(panelHeader, out headerLogoPanel);


            // Copy out params to locals before using in lambdas (avoid capturing out/ref)
            var headerRef = headerLogoPanel;
            var titleRef = lblTitle;
            var welcomeRef = lblWelcome;
            var logoutRef = btnLogout;

            void Reflow()
            {
                LayoutHeaderStack(panelHeader, headerRef, titleRef, welcomeRef);
                AlignHeaderRightControls(panelHeader, logoutRef);
            }

            // Initial layout
            Reflow();

            // Wire a single resize handler using locals
            EventHandler? resizeHandler = null;
            resizeHandler = (_, __) => Reflow();
            panelHeader.Resize += resizeHandler;
        }

        public static void LayoutHeaderStack(Panel panelHeader, RoundedPanel headerLogoPanel, Label lblTitle, Label lblWelcome)
        {
            int marginLeft = 20;
            int spacingX = 10;
            int spacingY = 2;

            headerLogoPanel.Location = new Point(marginLeft, (panelHeader.Height - headerLogoPanel.Height) / 2);

            lblTitle.AutoSize = true;
            lblWelcome.AutoSize = true;

            int combinedH = lblTitle.Height + spacingY + lblWelcome.Height;
            int blockTop = (panelHeader.Height - combinedH) / 2;
            int textLeft = headerLogoPanel.Right + spacingX;

            lblTitle.Location = new Point(textLeft, blockTop);
            lblWelcome.Location = new Point(textLeft, lblTitle.Bottom + spacingY);
        }

        public static void AlignHeaderRightControls(Panel panelHeader, Button btnLogout)
        {
            const int paddingRight = 20;
            const int spacing = 10;

            btnLogout.Location = new Point(
                panelHeader.ClientSize.Width - paddingRight - btnLogout.Width,
                (panelHeader.ClientSize.Height - btnLogout.Height) / 2);

        }


        public static void ApplyRoundedRegion(Control control, int radius)
        {
            if (control == null) return;
            if (control.Width <= 0 || control.Height <= 0) return;

            using var path = GetRoundedRectPath(new Rectangle(Point.Empty, control.Size), radius);
            control.Region = new Region(path);
        }

        private static void HeaderLogoPanel_Paint(object? sender, PaintEventArgs e)
        {
            if (sender is not Control pnl) return;
            var logoImage = pnl.Tag as Image;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            e.Graphics.CompositingQuality = CompositingQuality.HighQuality;

            var outerRect = pnl.ClientRectangle;
            outerRect.Width -= 1; outerRect.Height -= 1;

            const int inset = 4;
            var innerRect = Rectangle.Inflate(outerRect, -inset, -inset);

            using (var innerPath = GetRoundedRectPath(innerRect, 8))
            using (var fill = new SolidBrush(Color.White))
            {
                e.Graphics.FillPath(fill, innerPath);

                var prevClip = e.Graphics.Clip;
                e.Graphics.SetClip(innerPath);

                if (logoImage != null && innerRect.Width > 0 && innerRect.Height > 0)
                {
                    float imgW = logoImage.Width;
                    float imgH = logoImage.Height;
                    float scale = Math.Min(innerRect.Width / imgW, innerRect.Height / imgH);
                    int drawW = (int)Math.Round(imgW * scale);
                    int drawH = (int)Math.Round(imgH * scale);
                    int drawX = innerRect.X + (innerRect.Width - drawW) / 2;
                    int drawY = innerRect.Y + (innerRect.Height - drawH) / 2;

                    e.Graphics.DrawImage(logoImage, new Rectangle(drawX, drawY, drawW, drawH));
                }

                e.Graphics.SetClip(prevClip, CombineMode.Replace);
            }

            using (var outerPath = GetRoundedRectPath(outerRect, 10))
            using (var pen = new Pen(Color.FromArgb(220, 225, 235), 1.5f))
            {
                e.Graphics.DrawPath(pen, outerPath);
            }
        }

        private static Image? LoadEmployeeLogoImage()
        {
            var resObj = Properties.Resources.ResourceManager.GetObject("employeeLogo");
            if (resObj is Image resImage)
                return new Bitmap(resImage);

            var filePath = Path.Combine(AppContext.BaseDirectory, "Images", "employeeLogo.png");
            if (File.Exists(filePath))
            {
                using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                using var tmp = Image.FromStream(fs);
                return new Bitmap(tmp);
            }

            return null;
        }

        // OVERVIEW

        public static void BuildOverviewTab(
            TabPage tabOverview,
            out Label lblAvailableRooms,
            out Label lblOccupiedRooms,
            out Label lblReservedRooms,
            out Label lblActiveCheckIns,
            out DataGridView dgvCurrentOccupancy,
            out DateTimePicker dtpLogDate,
            out ComboBox cboLogEmployee,
            out ComboBox cboLogType,
            out Label lblLogSummary,
            out DataGridView dgvActivityLogs,
            out Label lblActivityLogsEmpty)
        {
            // Initialize out parameters to null
            lblAvailableRooms = lblOccupiedRooms = lblReservedRooms = lblActiveCheckIns = lblLogSummary = lblActivityLogsEmpty = null!;
            dgvCurrentOccupancy = dgvActivityLogs = null!;
            dtpLogDate = null!;
            cboLogEmployee = cboLogType = null!;

            if (tabOverview == null) return;

            tabOverview.BackColor = Color.FromArgb(240, 244, 248);
            tabOverview.AutoScroll = false;

            var mainFlowPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                BackColor = tabOverview.BackColor,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            tabOverview.Controls.Add(mainFlowPanel);

            mainFlowPanel.HorizontalScroll.Maximum = 0;
            mainFlowPanel.HorizontalScroll.Enabled = false;
            mainFlowPanel.HorizontalScroll.Visible = false;

            // --- Title ---
            Label lblOverviewTitle = new Label
            {
                Text = "Dashboard Overview",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 20)
            };
            mainFlowPanel.Controls.Add(lblOverviewTitle);

            // --- Stat Cards Section (responsive wrapping) ---
            var statsTable = new TableLayoutPanel
            {
                ColumnCount = 4,
                RowCount = 1,
                Dock = DockStyle.Top,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 20),
                BackColor = tabOverview.BackColor
            };
            statsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            statsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            statsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            statsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            mainFlowPanel.Controls.Add(statsTable);
            mainFlowPanel.SetFlowBreak(statsTable, true);

            var card1 = CreateStatCard("Available Rooms", Properties.Resources.ResourceManager.GetObject("ic_bed") as Image, out lblAvailableRooms);
            var card2 = CreateStatCard("Occupied Rooms", Properties.Resources.ResourceManager.GetObject("ic_users") as Image, out lblOccupiedRooms);
            var card3 = CreateStatCard("Reserved Rooms", Properties.Resources.ResourceManager.GetObject("ic_calendar") as Image, out lblReservedRooms);
            var card4 = CreateStatCard("Active Check-Ins", Properties.Resources.ResourceManager.GetObject("ic_checkin") as Image, out lblActiveCheckIns);
            var statCards = new Control[] { card1, card2, card3, card4 };

            // initial attach; layout will be recalculated dynamically
            statsTable.Controls.Add(card1, 0, 0);
            statsTable.Controls.Add(card2, 1, 0);
            statsTable.Controls.Add(card3, 2, 0);
            statsTable.Controls.Add(card4, 3, 0);

            // --- Current Occupancy Section ---
            var occPanel = new RoundedPanel
            {
                BorderRadius = 12,
                BackColor = Color.White,
                Margin = new Padding(0, 0, 0, 20),
                Padding = new Padding(20)
            };
            mainFlowPanel.Controls.Add(occPanel);
            mainFlowPanel.SetFlowBreak(occPanel, true);

            var lblOccTitle = new Label { Text = "Current Occupancy", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.FromArgb(30, 41, 59), AutoSize = true, Dock = DockStyle.Top };
            var lblOccSub = new Label { Text = "Guests currently checked in at the hotel", Font = new Font("Segoe UI", 9), ForeColor = Color.FromArgb(100, 116, 139), AutoSize = true, Dock = DockStyle.Top, Padding = new Padding(0, 0, 0, 15) };

            dgvCurrentOccupancy = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                EnableHeadersVisualStyles = false,
                AllowUserToResizeRows = false,
                ScrollBars = ScrollBars.Vertical
            };
            GridTheme.ApplyStandard(dgvCurrentOccupancy);

            // Fill remaining space; height will be computed dynamically
            occPanel.Controls.Add(dgvCurrentOccupancy);
            occPanel.Controls.Add(lblOccSub);
            occPanel.Controls.Add(lblOccTitle);

            // --- Activity Logs Section ---
            var pnlActivityLogs = new RoundedPanel { BorderRadius = 12, BackColor = Color.White, Padding = new Padding(20) };
            mainFlowPanel.Controls.Add(pnlActivityLogs);
            mainFlowPanel.SetFlowBreak(pnlActivityLogs, true);

            var lblLogTitle = new Label { Text = "Activity Logs", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.FromArgb(30, 41, 59), AutoSize = true, Dock = DockStyle.Top };
            var lblLogSub = new Label { Text = "Track all employee activities and transactions", Font = new Font("Segoe UI", 9), ForeColor = Color.FromArgb(100, 116, 139), AutoSize = true, Dock = DockStyle.Top, Padding = new Padding(0, 0, 0, 15) };

            var filtersTable = new TableLayoutPanel { ColumnCount = 3, RowCount = 2, Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(0, 0, 0, 10) };
            filtersTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            filtersTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            filtersTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));

            dtpLogDate = new DateTimePicker { Dock = DockStyle.Fill, Format = DateTimePickerFormat.Short, Value = DateTime.Today };
            cboLogEmployee = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10) };
            cboLogType = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10) };

            filtersTable.Controls.Add(new Label { Text = "Date", AutoSize = true }, 0, 0);
            filtersTable.Controls.Add(new Label { Text = "Employee", AutoSize = true }, 1, 0);
            filtersTable.Controls.Add(new Label { Text = "Activity Type", AutoSize = true }, 2, 0);
            filtersTable.Controls.Add(dtpLogDate, 0, 1);
            filtersTable.Controls.Add(cboLogEmployee, 1, 1);
            filtersTable.Controls.Add(cboLogType, 2, 1);

            lblLogSummary = new Label { Text = $"Showing 0 activities for {DateTime.Today:yyyy-MM-dd}", Font = new Font("Segoe UI", 9), ForeColor = Color.FromArgb(100, 116, 139), AutoSize = true, Dock = DockStyle.Top, Padding = new Padding(0, 5, 0, 5) };

            dgvActivityLogs = new DataGridView { Dock = DockStyle.Fill, ScrollBars = ScrollBars.Vertical };
            GridTheme.ApplyStandard(dgvActivityLogs);
            ApplyAmPmTimeFormat(dgvActivityLogs);

            lblActivityLogsEmpty = new Label { Text = "No activities found for the selected filters", Font = new Font("Segoe UI", 9, FontStyle.Italic), ForeColor = Color.FromArgb(100, 116, 139), TextAlign = ContentAlignment.MiddleCenter, Dock = DockStyle.Fill, Visible = false };
            dgvActivityLogs.Controls.Add(lblActivityLogsEmpty);

            pnlActivityLogs.Controls.Add(dgvActivityLogs);
            pnlActivityLogs.Controls.Add(lblLogSummary);
            pnlActivityLogs.Controls.Add(filtersTable);
            pnlActivityLogs.Controls.Add(lblLogSub);
            pnlActivityLogs.Controls.Add(lblLogTitle);

            // --- Dynamic resize handlers ---

            // Copy out parameters to locals before using them in a local function/closure
            var occGridRef = dgvCurrentOccupancy;
            var logsGridRef = dgvActivityLogs;
            var logSummaryRef = lblLogSummary;

            void ResizeChildren()
            {
                // reserve space for vertical scrollbar to prevent horizontal spill
                int reserve = mainFlowPanel.VerticalScroll.Visible ? SystemInformation.VerticalScrollBarWidth : 0;
                int targetWidth = Math.Max(0, mainFlowPanel.ClientSize.Width - reserve);
                foreach (Control ctrl in mainFlowPanel.Controls)
                    ctrl.Width = targetWidth;

                // Reflow stats grid responsively (1–4 columns)
                ReflowStatsTable(statsTable, statCards, targetWidth);

                // Adjust section heights to viewport (no hardcoded row counts)
                AdjustGridSectionHeight(occPanel, occGridRef, minRows: 3, maxRows: 8, fractionOfViewport: 0.38, lblOccTitle, lblOccSub);
                AdjustGridSectionHeight(pnlActivityLogs, logsGridRef, minRows: 4, maxRows: 10, fractionOfViewport: 0.50, lblLogTitle, lblLogSub, filtersTable, logSummaryRef);
            }

            mainFlowPanel.SizeChanged += (s, e) => ResizeChildren();
            mainFlowPanel.Layout += (s, e) => ResizeChildren();

            // Initial layout pass
            ResizeChildren();
        }

        // NEW: responsive stat cards reflow for 1–4 columns based on width
        private static void ReflowStatsTable(TableLayoutPanel table, Control[] cards, int totalWidth)
        {
            if (cards == null || cards.Length == 0) return;

            int minCard = 250;   // approx min card width
            int gap = 16;        // cell margin already on cards; keep conservative calc
            int maxCols = 4;

            int cols = Math.Clamp(totalWidth <= 0 ? maxCols : Math.Max(1, Math.Min(maxCols, (totalWidth + gap) / (minCard + gap))), 1, maxCols);
            int rows = (int)Math.Ceiling(cards.Length / (double)cols);

            table.SuspendLayout();
            table.Controls.Clear();
            table.ColumnStyles.Clear();
            table.RowStyles.Clear();

            table.ColumnCount = cols;
            table.RowCount = rows;

            for (int i = 0; i < cols; i++)
                table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / cols));
            for (int r = 0; r < rows; r++)
                table.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            int idx = 0;
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    if (idx >= cards.Length) break;
                    var card = cards[idx++];
                    card.Dock = DockStyle.Fill;
                    table.Controls.Add(card, c, r);
                }
            }
            table.ResumeLayout(performLayout: true);
        }

        // NEW: dynamically compute grid rows/section height to fit viewport fraction
        private static void AdjustGridSectionHeight(RoundedPanel panel, DataGridView grid, int minRows, int maxRows, double fractionOfViewport, params Control[] topControls)
        {
            if (panel.Parent == null) return;

            // viewport based on the flow panel’s client area
            var viewportH = panel.Parent.ClientSize.Height;
            if (viewportH <= 0)
                viewportH = Screen.FromControl(panel).WorkingArea.Height;

            // Area we aim to use for the whole section
            int targetSectionH = Math.Max(160, (int)Math.Round(viewportH * fractionOfViewport));

            // height consumed by labels/filters above the grid
            int tops = 0;
            foreach (var c in topControls)
            {
                if (c == null) continue;
                int h = c.Height;
                if (h <= 0) h = c.PreferredSize.Height;
                tops += h;
            }

            int rowH = Math.Max(24, grid.RowTemplate.Height);
            int headerH = Math.Max(28, grid.ColumnHeadersHeight);

            // compute rows so that (panel padding + tops + grid(height)) ~= targetSectionH
            int availableForGrid = Math.Max(80, targetSectionH - panel.Padding.Vertical - tops);
            int rows = (availableForGrid - headerH - 2) / rowH;
            rows = Math.Clamp(rows, minRows, maxRows);

            // final heights
            int gridContentH = headerH + (rowH * rows) + 2;
            int total = panel.Padding.Vertical + tops + gridContentH;

            // apply
            panel.Height = Math.Max(total, 140);
            // let grid fill; no min/max clamping => fully dynamic
            grid.MinimumSize = new Size(0, 0);
            grid.MaximumSize = new Size(int.MaxValue, int.MaxValue);
            grid.Dock = DockStyle.Fill;
        }

        private static RoundedPanel CreateStatCard(string title, Image? icon, out Label valueLabel)
        {
            const int iconSize = 28;
            const int gap = 12;

            var card = new RoundedPanel
            {
                BorderRadius = 14,
                BackColor = Color.FromArgb(219, 234, 254),          // light blue
                Height = 130,
                Margin = new Padding(8),                            // symmetric cell margin
                Padding = new Padding(20),
                Tag = icon,
                Dock = DockStyle.Fill                               // fill table cell => balanced spacing
            };

            // Position title and value to the right of the icon zone
            int leftPadding = 20;
            int contentLeft = leftPadding + iconSize + gap;

            var lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 11f, FontStyle.Regular),
                ForeColor = Color.FromArgb(71, 85, 105),
                Location = new Point(contentLeft, 15),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            // Value aligned below title with a bit more emphasis
            var valueTop = Math.Max(60, lblTitle.Location.Y + lblTitle.PreferredHeight + 10);

            valueLabel = new Label
            {
                Text = "0",
                Font = new Font("Segoe UI", 30f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                Location = new Point(contentLeft, valueTop),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            card.Controls.Add(lblTitle);
            card.Controls.Add(valueLabel);

            // Bluish border accent to match theme
            var borderColor = Color.FromArgb(147, 197, 253);
            AttachRoundedBorderAndIcon(card, 14, borderColor);

            return card;
        }

        private static void AttachRoundedBorderAndIcon(Control ctrl, int radius, Color borderColor)
        {
            ctrl.Paint -= Control_DrawRoundedBorderAndIcon;
            ctrl.Paint += Control_DrawRoundedBorderAndIcon;

            void Control_DrawRoundedBorderAndIcon(object? sender, PaintEventArgs e)
            {
                if (sender is not Control control) return;
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.CompositingQuality = CompositingQuality.HighQuality;

                // Rounded rect and subtle gradient fill based on BackColor
                var rect = control.ClientRectangle;
                rect.Width -= 1;
                rect.Height -= 1;

                using var path = GetRoundedRectPath(rect, radius);

                var baseColor = control.BackColor;
                var c1 = Color.FromArgb(baseColor.A,
                    Math.Min(255, baseColor.R + 10),
                    Math.Min(255, baseColor.G + 10),
                    Math.Min(255, baseColor.B + 10));
                var c2 = Color.FromArgb(baseColor.A,
                    Math.Max(0, baseColor.R - 8),
                    Math.Max(0, baseColor.G - 8),
                    Math.Max(0, baseColor.B - 8));

                using (var lg = new LinearGradientBrush(rect, c1, c2, 20f))
                {
                    g.FillPath(lg, path);
                }

                // Icon with soft circular badge
                if (control.Tag is Image icon)
                {
                    const int iconSize = 28;
                    const int iconX = 20;
                    var iconY = control.Padding.Top + (control.Height - control.Padding.Vertical - iconSize) / 2;
                    var iconRect = new Rectangle(iconX, iconY, iconSize, iconSize);

                    var badgeSize = iconSize + 10;
                    var badgeRect = new Rectangle(
                        iconRect.X - (badgeSize - iconSize) / 2,
                        iconRect.Y - (badgeSize - iconSize) / 2,
                        badgeSize, badgeSize);

                    using (var badgeBrush = new SolidBrush(Color.FromArgb(40, borderColor)))
                    using (var badgePath = new GraphicsPath())
                    {
                        badgePath.AddEllipse(badgeRect);
                        g.FillPath(badgeBrush, badgePath);
                    }

                    g.DrawImage(icon, iconRect);
                }

                // Border
                using var pen = new Pen(borderColor, 1.25f);
                g.DrawPath(pen, path);
            }
        }

        // TABS (unchanged from previous response)

        public static void StyleTabControl(
            TabControl tc,
            IDictionary<TabPage, string> pageIconKeys,
            out ImageList? imageList)
        {
            tc.BackColor = Color.FromArgb(244, 246, 250);
            tc.DrawMode = TabDrawMode.OwnerDrawFixed;
            tc.SizeMode = TabSizeMode.Fixed; // enforce equal widths
            tc.ItemSize = new Size(150, 36);
            tc.Padding = new Point(18, 6);

            // Flicker mitigation and multiline wrap support
            tc.HotTrack = false;                 // disable native hover paints
            tc.Appearance = TabAppearance.Normal;

            // Enable true double-buffering + anti-flicker flags
            EnableFlickerFreePainting(tc);

            imageList = BuildTabImageList();
            if (imageList != null)
            {
                tc.ImageList = imageList;
                foreach (var kvp in pageIconKeys)
                    TryAssignImageKey(kvp.Key, imageList, kvp.Value);
            }

            tc.DrawItem -= TabControl_DrawItem;
            tc.DrawItem += TabControl_DrawItem;

            // Force redraw on content changes (icons/text/etc.)
            tc.ControlAdded -= (_, __) => tc.Invalidate();
            tc.ControlAdded += (_, __) => tc.Invalidate();

            // Balanced, responsive widths + hover tracking
            EnableResponsiveTabWidth(tc, minWidth: 110, maxWidth: 220, height: 36);
        }

        // Track per-TabControl state (reentrancy + hover)
        private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<TabControl, ResponsiveTabState> s_tabStates
            = new();

        private sealed class ResponsiveTabState
        {
            public bool Updating;
            public bool HandlersAttached;
            public int MinWidth;
            public int MaxWidth;
            public int Height;
            public int HoveredIndex = -1;
        }

        // Balanced widths: tabs fill the row; wrapped rows keep consistent widths.
        private static void EnableResponsiveTabWidth(TabControl tc, int minWidth, int maxWidth, int height)
        {
            var state = s_tabStates.GetValue(tc, _ => new ResponsiveTabState());
            state.MinWidth = minWidth;
            state.MaxWidth = maxWidth;
            state.Height = height;

            void Update()
            {
                if (state.Updating) return;
                state.Updating = true;
                try
                {
                    int count = tc.TabPages.Count;
                    if (count <= 0) return;

                    int avail = Math.Max(0, tc.ClientSize.Width - 8); // margin for control chrome

                    // Compute columns per row to avoid scroll and keep balanced widths
                    int minCols = Math.Max(1, Math.Min(count, avail / state.MinWidth));
                    int cols = Math.Max(1, minCols);
                    bool wrapNeeded = cols < count;

                    // Balanced width per tab for current row
                    int per = Math.Max(state.MinWidth, avail / cols);
                    int clamped = Math.Clamp(per, state.MinWidth, state.MaxWidth);

                    bool changed = false;

                    if (tc.Multiline != wrapNeeded) { tc.Multiline = wrapNeeded; changed = true; }

                    // Always fixed to enforce equal widths; height is respected, width enforced
                    if (tc.SizeMode != TabSizeMode.Fixed) { tc.SizeMode = TabSizeMode.Fixed; changed = true; }

                    var desiredSize = new Size(clamped, state.Height);
                    if (tc.ItemSize != desiredSize) { tc.ItemSize = desiredSize; changed = true; }

                    var desiredPadding = new Point(wrapNeeded ? 12 : 18, 6);
                    if (tc.Padding != desiredPadding) { tc.Padding = desiredPadding; changed = true; }

                    if (changed) tc.Invalidate();
                }
                finally { state.Updating = false; }
            }

            int HitTestIndex(Point clientPoint)
            {
                for (int i = 0; i < tc.TabPages.Count; i++)
                {
                    if (tc.GetTabRect(i).Contains(clientPoint)) return i;
                }
                return -1;
            }

            void OnMouseMove(object? s, MouseEventArgs e)
            {
                int old = state.HoveredIndex;
                int idx = HitTestIndex(e.Location);
                if (old == idx) return;

                state.HoveredIndex = idx;

                if (old != -1) InvalidateTab(tc, old);
                if (idx != -1) InvalidateTab(tc, idx);
            }

            void OnMouseLeave(object? s, EventArgs e)
            {
                if (state.HoveredIndex != -1)
                {
                    int old = state.HoveredIndex;
                    state.HoveredIndex = -1;
                    InvalidateTab(tc, old);
                }
            }

            if (!state.HandlersAttached)
            {
                tc.SizeChanged += (_, __) => Update();
                tc.ControlAdded += (_, __) => Update();
                tc.ControlRemoved += (_, __) => Update();
                tc.HandleCreated += (_, __) => Update();
                tc.MouseMove += OnMouseMove;
                tc.MouseLeave += OnMouseLeave;
                tc.Disposed += (_, __) => s_tabStates.Remove(tc);
                state.HandlersAttached = true;
            }

            if (tc.IsHandleCreated) Update();
        }

        // Flat, rectangular tabs with icon + text and a bottom indicator. No gradients or shadows.
        private static void TabControl_DrawItem(object? sender, DrawItemEventArgs e)
        {
            if (sender is not TabControl tc) return;

            s_tabStates.TryGetValue(tc, out var state);
            int hoverIndex = state?.HoveredIndex ?? -1;

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            bool selected = (e.Index == tc.SelectedIndex);
            bool hovered = (!selected && e.Index == hoverIndex);
            var page = tc.TabPages[e.Index];

            // Tab bounds (flat rectangle)
            var rect = tc.GetTabRect(e.Index);
            rect.Inflate(-4, -6); // a little inner padding

            // Palette with clear contrast against strip
            var stripBack  = tc.BackColor;                         // 244,246,250
            var normalBack = Color.FromArgb(247, 249, 252);        // slightly lighter than strip
            var hoverBack  = Color.FromArgb(242, 246, 252);        // hover fill
            var selectBack = Color.White;                          // selected fill
            var separator  = Color.FromArgb(224, 229, 235);        // right separator between tabs
            var baseline   = Color.FromArgb(215, 220, 230);        // bottom baseline
            var indicator  = Color.FromArgb(59, 130, 246);         // blue underline
            var textSel    = Color.FromArgb(17, 24, 39);
            var textNorm   = Color.FromArgb(55, 65, 81);

            // Always fill tab background for visibility
            var back = selected ? selectBack : hovered ? hoverBack : normalBack;
            using (var fill = new SolidBrush(back))
                g.FillRectangle(fill, rect);

            // Right separator (not for the last tab in a row; harmless even if drawn)
            using (var sepPen = new Pen(separator, 1f))
                g.DrawLine(sepPen, rect.Right, rect.Top + 6, rect.Right, rect.Bottom - 6);

            // Bottom baseline for alignment
            using (var basePen = new Pen(baseline, 1f))
                g.DrawLine(basePen, rect.Left, rect.Bottom, rect.Right, rect.Bottom);

            // Selected underline (thickness reduced to 2px)
            if (selected)
            {
                int lh = 1; // was 3
                int inset = 10;
                using var pen = new Pen(indicator, lh) { StartCap = LineCap.Round, EndCap = LineCap.Round };
                g.DrawLine(pen, rect.Left + inset, rect.Bottom - lh, rect.Right - inset, rect.Bottom - lh);
            }

            // Content: icon + text (left aligned)
            int leftPad = 12;
            int gap = 8;
            int imgSize = 16;
            int x = rect.Left + leftPad;

            if (tc.ImageList != null && page.ImageIndex >= 0 && page.ImageIndex < tc.ImageList.Images.Count)
            {
                var img = tc.ImageList.Images[page.ImageIndex];
                var imgRect = new Rectangle(x, rect.Y + (rect.Height - imgSize) / 2, imgSize, imgSize);
                g.DrawImage(img, imgRect);
                x = imgRect.Right + gap;
            }

            var textColor = selected ? textSel : textNorm;
            using var fontToUse = selected ? new Font(e.Font, FontStyle.Bold) : (Font)e.Font.Clone();

            // Draw text inside the remaining space to avoid clipping
            var textRect = new Rectangle(x, rect.Y, Math.Max(0, rect.Right - x - 8), rect.Height);
            TextRenderer.DrawText(
                g,
                page.Text,
                fontToUse,
                textRect,
                textColor,
                TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
        }

        private static GraphicsPath GetRoundedRectPath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            if (radius <= 0)
            {
                path.AddRectangle(rect);
                return path;
            }
            int d = radius * 2;
            if (d > rect.Width) d = rect.Width;
            if (d > rect.Height) d = rect.Height;
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

        // Full tabs setup to keep forms free of UI code.
        // Wraps the TabControl in a host panel for left padding, styles tabs, softens pages, and enables responsive widths.
        public static void SetupTabs(
            TabControl tc,
            int leftPadding,
            IDictionary<TabPage, string> pageIconKeys,
            out ImageList? imageList,
            out Panel? hostPanel)
        {
            hostPanel = null;
            EmbedTabControlInHost(tc, leftPadding, ref hostPanel);

            // Optional host padding + background unified
            if (hostPanel != null)
            {
                var pad = hostPanel.Padding;
                hostPanel.Padding = new Padding(pad.Left > 0 ? pad.Left : leftPadding, 16, pad.Right, 20);
                hostPanel.BackColor = tc.Parent?.BackColor ?? Color.FromArgb(244, 246, 250);
            }

            StyleTabControl(tc, pageIconKeys, out imageList);
            SoftenTabPages(tc);
        }

        // Make each TabPage background consistent with the host and avoid default styles.
        public static void SoftenTabPages(TabControl tc)
        {
            var pageBack = Color.FromArgb(244, 246, 250);
            foreach (TabPage page in tc.TabPages)
            {
                page.UseVisualStyleBackColor = false;
                page.BackColor = pageBack;
            }
        }

        // Wrap TabControl in a host panel with left padding without breaking z-order.
        public static void EmbedTabControlInHost(TabControl tc, int leftPadding, ref Panel? hostPanel)
        {
            if (tc.Parent == null || hostPanel != null) return;

            var parent = tc.Parent;
            parent.SuspendLayout();

            hostPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(leftPadding, 0, 0, 0),
                BackColor = Color.FromArgb(244, 246, 250)
            };

            int z = parent.Controls.GetChildIndex(tc);
            parent.Controls.Remove(tc);

            hostPanel.Controls.Add(tc);
            tc.Dock = DockStyle.Fill;

            parent.Controls.Add(hostPanel);
            parent.Controls.SetChildIndex(hostPanel, z);

            parent.ResumeLayout(performLayout: true);
        }

        // Helpers (images for tabs)
        private static ImageList? BuildTabImageList()
        {
            var list = new ImageList
            {
                ColorDepth = ColorDepth.Depth32Bit,
                ImageSize = new Size(16, 16)
            };

            bool any = false;
            any |= TryAddImage(list, "ic_home",       Properties.Resources.ResourceManager.GetObject("ic_home") as Image);
            any |= TryAddImage(list, "ic_checkin",    Properties.Resources.ResourceManager.GetObject("ic_checkin") as Image);
            any |= TryAddImage(list, "ic_checkout",   Properties.Resources.ResourceManager.GetObject("ic_checkout") as Image);
            any |= TryAddImage(list, "ic_calendar",   Properties.Resources.ResourceManager.GetObject("ic_calendar") as Image);
            any |= TryAddImage(list, "ic_bed",        Properties.Resources.ResourceManager.GetObject("ic_bed") as Image);
            any |= TryAddImage(list, "ic_search",     Properties.Resources.ResourceManager.GetObject("ic_search") as Image);
            any |= TryAddImage(list, "ic_users",      Properties.Resources.ResourceManager.GetObject("ic_users") as Image);
            any |= TryAddImage(list, "ic_settings",   Properties.Resources.ResourceManager.GetObject("ic_settings") as Image);
            any |= TryAddImage(list, "ic_chart",      Properties.Resources.ResourceManager.GetObject("ic_chart") as Image);

            return any ? list : null;
        }

        private static bool TryAddImage(ImageList list, string key, Image? img)
        {
            if (img == null) return false;
            list.Images.Add(key, img);
            return true;
        }

        private static void TryAssignImageKey(TabPage? page, ImageList list, string key)
        {
            if (page == null) return;
            int idx = list.Images.IndexOfKey(key);
            if (idx >= 0) page.ImageIndex = idx;
        }

        // Enables true double-buffered painting to avoid background erase flicker.
        private static void EnableFlickerFreePainting(TabControl tc)
        {
            // Avoid ControlStyles.UserPaint on TabControl; rely on double-buffering + WM_PAINT optimizations
            var setStyle = typeof(Control).GetMethod("SetStyle", BindingFlags.Instance | BindingFlags.NonPublic);
            var flags = ControlStyles.AllPaintingInWmPaint
                     | ControlStyles.OptimizedDoubleBuffer
                     | ControlStyles.ResizeRedraw;
            setStyle?.Invoke(tc, new object[] { flags, true });

            // Ensure DoubleBuffered is on
            typeof(Control).GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic)
                   ?.SetValue(tc, true);

            // Double-buffer TabPages as well (existing and future)
            void Buffer(Control c) =>
                typeof(Control).GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(c, true);

            foreach (TabPage page in tc.TabPages) Buffer(page);
            tc.ControlAdded += (_, e) => { if (e.Control is TabPage p) Buffer(p); };
        }

        // Repaint only the given tab's bounds (avoid full-control invalidation on hover).
        private static void InvalidateTab(TabControl tc, int index)
        {
            if (index < 0 || index >= tc.TabPages.Count) return;
            var r = tc.GetTabRect(index);
            r.Inflate(8, 8); // small margin for border/underline
            tc.Invalidate(r);
        }

        // 2) Apply AM/PM to Activity Logs DateTime columns
        // Place helper near other private helpers
        private static void ApplyAmPmTimeFormat(DataGridView grid)
        {
            void Apply()
            {
                foreach (DataGridViewColumn col in grid.Columns)
                {
                    if (col.ValueType == typeof(DateTime))
                    {
                        // Respect existing formats if already set
                        if (!string.IsNullOrWhiteSpace(col.DefaultCellStyle.Format)) continue;

                        var header = col.HeaderText?.Trim() ?? col.Name ?? string.Empty;
                        if (header.IndexOf("date", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            // Show full date and time with AM/PM
                            col.DefaultCellStyle.Format = "yyyy-MM-dd hh:mm tt";
                        }
                        else if (header.IndexOf("time", StringComparison.OrdinalIgnoreCase) >= 0
                              || header.IndexOf("timestamp", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            // Time only with AM/PM
                            col.DefaultCellStyle.Format = "hh:mm tt";
                        }
                        else
                        {
                            // Default to time with AM/PM if ambiguous
                            col.DefaultCellStyle.Format = "hh:mm tt";
                        }
                    }
                }
            }

            grid.DataBindingComplete += (_, __) => Apply();
            grid.ColumnAdded += (_, __) => Apply();

            if (grid.IsHandleCreated && grid.Columns.Count > 0) Apply();
        }

        // NEW: Enable double‑click editing of the Notes (Description) for an active Check-In inside the occupancy grid.
        // saveNotes delegate: (checkInId, newNotes) => true on success, false to keep dialog open.
        public static void EnableOccupancyNotesEditing(
            DataGridView occupancyGrid,
            Func<int, string?, bool> saveNotes)
        {
            EnableOccupancyNotesEditing(
                occupancyGrid,
                row =>
                {
                    // Best-effort resolution via a hidden "CheckInID" column
                    if (occupancyGrid.Columns.Contains("CheckInID") &&
                        row.Cells["CheckInID"]?.Value is int id) return id;
                    return null;
                },
                saveNotes);
        }

        public static void EnableOccupancyNotesEditing(
            DataGridView occupancyGrid,
            Func<DataGridViewRow, int?> resolveCheckInId,
            Func<int, string?, bool> saveNotes)
        {
            if (occupancyGrid == null) return;
            if (resolveCheckInId == null) throw new ArgumentNullException(nameof(resolveCheckInId));
            if (saveNotes == null) throw new ArgumentNullException(nameof(saveNotes));

            occupancyGrid.CellDoubleClick -= OccupancyGrid_CellDoubleClick;
            occupancyGrid.CellDoubleClick += OccupancyGrid_CellDoubleClick;

            void OccupancyGrid_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
            {
                if (e.RowIndex < 0) return;
                var row = occupancyGrid.Rows[e.RowIndex];

                int? maybeId = resolveCheckInId(row);
                if (maybeId is null)
                {
                    MessageBox.Show("Could not determine the selected Check-In. Try selecting a different row.", "Edit Description",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                int checkInId = maybeId.Value;
                string room = Convert.ToString(row.Cells["Room"]?.Value) ?? "";
                string guest = Convert.ToString(row.Cells["GuestName"]?.Value) ?? "";
                string currentNotes = Convert.ToString(row.Cells["Notes"]?.Value) ?? "";

                ShowEditor(checkInId, room, guest, currentNotes);
            }

            void ShowEditor(int checkInId, string room, string guest, string currentNotes)
            {
                using var dlg = new EditDescriptionForm(room, guest, currentNotes)
                {
                    StartPosition = FormStartPosition.CenterScreen,
                    ShowInTaskbar = false,
                    MinimizeBox = false,
                    MaximizeBox = false,
                    FormBorderStyle = FormBorderStyle.FixedDialog
                };

                // Keep it on-screen and non-resizable
                var wa = Screen.FromPoint(Cursor.Position).WorkingArea;
                dlg.Width = Math.Min(dlg.Width, wa.Width - 40);
                dlg.Height = Math.Min(dlg.Height, wa.Height - 40);
                dlg.MinimumSize = dlg.Size;
                dlg.MaximumSize = dlg.Size;

                if (dlg.ShowDialog(occupancyGrid.FindForm()) == DialogResult.OK)
                {
                    var newNotes = dlg.Description;
                    bool ok;
                    try
                    {
                        ok = saveNotes(checkInId, string.IsNullOrWhiteSpace(newNotes) ? null : newNotes);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to save description.\n{ex.Message}", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    if (!ok) return;

                    if (occupancyGrid.CurrentRow != null)
                    {
                        occupancyGrid.CurrentRow.Cells["Notes"].Value = newNotes;
                        occupancyGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCells;
                    }
                }
            }
        }
    }
}