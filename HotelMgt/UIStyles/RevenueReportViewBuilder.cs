using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using HotelMgt.Custom; // RoundedPanel

namespace HotelMgt.UIStyles
{
    public static class RevenueReportViewBuilder
    {
        // ----------- BASIC (legacy non-scroll) BUILD METHODS -----------

        public static void Build(
            UserControl host,
            out Label lblTodayRevenue, out Label lblTodayTxn,
            out Label lblMonthRevenue, out Label lblMonthTxn,
            out Label lblYearRevenue, out Label lblYearTxn)
        {
            host.SuspendLayout();

            var cardsHost = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = false,
                Height = 200,
                ColumnCount = 3,
                RowCount = 1,
                Padding = new Padding(16, 12, 16, 0),
                BackColor = host.BackColor,
                Name = "rev_cardsHost"
            };
            cardsHost.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3333F));
            cardsHost.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3333F));
            cardsHost.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3333F));

            var blueCardBack = Color.FromArgb(219, 234, 254);
            var revenueBlue = Color.FromArgb(37, 99, 235);

            var todayCard = CreateStatCard("Today's Revenue", revenueBlue, blueCardBack, out var todayRevenue, out var todayTxn);
            var monthCard = CreateStatCard("This Month", revenueBlue, blueCardBack, out var monthRevenue, out var monthTxn);
            var yearCard  = CreateStatCard("This Year",  revenueBlue, blueCardBack, out var yearRevenue,  out var yearTxn);

            cardsHost.Controls.Add(todayCard, 0, 0);
            cardsHost.Controls.Add(monthCard, 1, 0);
            cardsHost.Controls.Add(yearCard,  2, 0);

            var existing = host.Controls["rev_cardsHost"];
            if (existing != null) host.Controls.Remove(existing);

            host.Controls.Add(cardsHost);

            lblTodayRevenue = todayRevenue;
            lblTodayTxn = todayTxn;
            lblMonthRevenue = monthRevenue;
            lblMonthTxn = monthTxn;
            lblYearRevenue = yearRevenue;
            lblYearTxn = yearTxn;

            host.ResumeLayout();
        }

        public static void BuildDetailsSection(
            UserControl host,
            out TabControl tabReports,
            out DateTimePicker dtpDay,
            out DateTimePicker dtpMonth,
            out NumericUpDown nudYear,
            out TextBox txtSearch,
            out RoundedPanel summaryPanel,
            out Label lblSummaryTitle,
            out Label lblSummaryRevenue,
            out Label lblSummaryTxn,
            out DataGridView dgvBreakdown)
        {
            host.SuspendLayout();

            var container = new RoundedPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(16, 36, 16, 16),
                BackColor = host.BackColor,
                BorderRadius = 12
            };

            var lblTitle = new Label
            {
                AutoSize = true,
                Text = "Revenue Reports",
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 65, 85),
                Margin = new Padding(0, 0, 0, 4)
            };
            var lblSubtitle = new Label
            {
                AutoSize = true,
                Text = "View detailed revenue breakdown by day, month, or year",
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(100, 116, 139),
                Margin = new Padding(0, 0, 0, 14)
            };

            tabReports = new TabControl
            {
                Dock = DockStyle.Top,
                Height = 44,
                Appearance = TabAppearance.Normal,
                Margin = new Padding(0, 0, 0, 12),
                SizeMode = TabSizeMode.Fixed,
                ItemSize = new Size(150, 30)
            };
            tabReports.TabPages.Add(new TabPage("Daily Report")   { BackColor = host.BackColor });
            tabReports.TabPages.Add(new TabPage("Monthly Report") { BackColor = host.BackColor });
            tabReports.TabPages.Add(new TabPage("Yearly Report")  { BackColor = host.BackColor });

            var selectorRow = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 46,
                ColumnCount = 2,
                RowCount = 1,
                Margin = new Padding(0, 0, 0, 14)
            };
            selectorRow.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            selectorRow.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            var lblSelect = new Label
            {
                AutoSize = true,
                Text = "Select Date",
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(71, 85, 105),
                Margin = new Padding(0, 12, 8, 0)
            };

            dtpDay = new DateTimePicker
            {
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "yyyy-MM-dd",
                Width = 170
            };
            dtpMonth = new DateTimePicker
            {
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "MMMM yyyy",
                ShowUpDown = true,
                Width = 170,
                Visible = false
            };
            nudYear = new NumericUpDown
            {
                Minimum = 2000,
                Maximum = 2100,
                Value = DateTime.Today.Year,
                Width = 110,
                Visible = false
            };
            txtSearch = new TextBox { Visible = false, Width = 0, Height = 0 };

            selectorRow.Controls.Add(lblSelect, 0, 0);
            selectorRow.Controls.Add(dtpDay, 1, 0);
            selectorRow.Controls.Add(dtpMonth, 1, 0);
            selectorRow.Controls.Add(nudYear, 1, 0);

            summaryPanel = new RoundedPanel
            {
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(239, 246, 255),
                Padding = new Padding(16, 12, 16, 12),
                Margin = new Padding(0, 0, 0, 14),
                BorderRadius = 12
            };
            var sumLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2
            };
            sumLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            sumLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            lblSummaryTitle = new Label
            {
                AutoSize = true,
                Text = "Total Revenue for Today",
                Font = new Font("Segoe UI", 11F),
                ForeColor = Color.FromArgb(30, 41, 59)
            };
            lblSummaryRevenue = new Label
            {
                AutoSize = true,
                Text = "$0.00",
                Font = new Font("Segoe UI", 17F, FontStyle.Bold),
                ForeColor = Color.FromArgb(37, 99, 235)
            };
            lblSummaryTxn = new Label
            {
                AutoSize = true,
                Text = "0 transactions",
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = Color.FromArgb(71, 85, 105)
            };
            sumLayout.Controls.Add(lblSummaryTitle, 0, 0);
            sumLayout.Controls.Add(lblSummaryRevenue, 1, 0);
            sumLayout.Controls.Add(lblSummaryTxn, 0, 1);
            summaryPanel.Controls.Add(sumLayout);

            dgvBreakdown = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                EnableHeadersVisualStyles = false,
                RowHeadersVisible = false,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = Color.FromArgb(229, 231, 235),
                ScrollBars = ScrollBars.Vertical
            };
            GridTheme.ApplyStandard(dgvBreakdown);
            dgvBreakdown.ColumnHeadersDefaultCellStyle.BackColor = Color.White;
            dgvBreakdown.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(30, 41, 59);
            dgvBreakdown.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            dgvBreakdown.RowTemplate.Height = 34;

            var stack = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 6,
                Padding = new Padding(0),
                BackColor = host.BackColor
            };
            stack.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            stack.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            stack.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            stack.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            stack.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            stack.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            stack.Controls.Add(lblTitle, 0, 0);
            stack.Controls.Add(lblSubtitle, 0, 1);
            stack.Controls.Add(tabReports, 0, 2);
            stack.Controls.Add(selectorRow, 0, 3);
            stack.Controls.Add(summaryPanel, 0, 4);
            stack.Controls.Add(dgvBreakdown, 0, 5);

            container.Controls.Add(stack);
            host.Controls.Add(container);

            host.ResumeLayout();
        }

        // ----------- RESPONSIVE + SCROLLABLE IMPLEMENTATION -----------

        public static void InitializeResponsive(
            UserControl host,
            out Label lblTodayRevenue, out Label lblTodayTxn,
            out Label lblMonthRevenue, out Label lblMonthTxn,
            out Label lblYearRevenue,  out Label lblYearTxn,
            out TabControl tabReports,
            out DateTimePicker dtpDay,
            out DateTimePicker dtpMonth,
            out NumericUpDown nudYear,
            out TextBox txtSearch,
            out RoundedPanel summaryPanel,
            out Label lblSummaryTitle,
            out Label lblSummaryRevenue,
            out Label lblSummaryTxn,
            out DataGridView dgvBreakdown)
        {
            if (host == null) throw new ArgumentNullException(nameof(host));

            BuildScrollableHost(host, out var mainFlow, out var statsHost, out var detailsHost);

            // KPI cards (responsive 1–3 cols)
            var statsTable = new TableLayoutPanel
            {
                ColumnCount = 3,
                RowCount = 1,
                Dock = DockStyle.Top,
                AutoSize = true,
                BackColor = statsHost.BackColor,
                Margin = new Padding(0),
                Padding = new Padding(16, 12, 16, 0),
                Name = "rev_statsTable"
            };
            statsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3333F));
            statsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3333F));
            statsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3333F));

            var blueCardBack = Color.FromArgb(219, 234, 254);
            var revenueBlue = Color.FromArgb(37, 99, 235);

            var cardToday = CreateStatCard("Today's Revenue", revenueBlue, blueCardBack, out lblTodayRevenue, out lblTodayTxn);
            var cardMonth = CreateStatCard("This Month",       revenueBlue, blueCardBack, out lblMonthRevenue, out lblMonthTxn);
            var cardYear  = CreateStatCard("This Year",        revenueBlue, blueCardBack, out lblYearRevenue,  out lblYearTxn);
            var kpiCards = new Control[] { cardToday, cardMonth, cardYear };

            statsTable.Controls.Add(cardToday, 0, 0);
            statsTable.Controls.Add(cardMonth, 1, 0);
            statsTable.Controls.Add(cardYear,  2, 0);
            statsHost.Controls.Clear();
            statsHost.Controls.Add(statsTable);

            // Details section
            detailsHost.SuspendLayout();

            var lblTitle = new Label
            {
                AutoSize = true,
                Text = "Revenue Reports",
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 65, 85),
                Margin = new Padding(0, 0, 0, 4)
            };
            var lblSubtitle = new Label
            {
                AutoSize = true,
                Text = "View detailed revenue breakdown by day, month, or year",
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(100, 116, 139),
                Margin = new Padding(0, 0, 0, 14)
            };

            tabReports = new TabControl
            {
                Dock = DockStyle.Top,
                Height = 44,
                Appearance = TabAppearance.Normal,
                Margin = new Padding(0, 0, 0, 12),
                SizeMode = TabSizeMode.Fixed,
                ItemSize = new Size(150, 30)
            };
            var pgDaily   = new TabPage("Daily Report")   { BackColor = detailsHost.BackColor };
            var pgMonthly = new TabPage("Monthly Report") { BackColor = detailsHost.BackColor };
            var pgYearly  = new TabPage("Yearly Report")  { BackColor = detailsHost.BackColor };
            tabReports.TabPages.Add(pgDaily);
            tabReports.TabPages.Add(pgMonthly);
            tabReports.TabPages.Add(pgYearly);

            try
            {
                AdminDashboardViewBuilder.StyleTabControl(
                    tabReports,
                    pageIconKeys: new Dictionary<TabPage, string>(),
                    out var _);
            }
            catch { /* optional */ }

            var selectorRow = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 46,
                ColumnCount = 2,
                RowCount = 1,
                Margin = new Padding(0, 0, 0, 14)
            };
            selectorRow.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            selectorRow.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            var lblSelect = new Label
            {
                AutoSize = true,
                Text = "Select Date",
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(71, 85, 105),
                Margin = new Padding(0, 12, 8, 0)
            };

            dtpDay = new DateTimePicker
            {
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "yyyy-MM-dd",
                Width = 170
            };
            dtpMonth = new DateTimePicker
            {
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "MMMM yyyy",
                ShowUpDown = true,
                Width = 170,
                Visible = false
            };
            nudYear = new NumericUpDown
            {
                Minimum = 2000,
                Maximum = 2100,
                Value = DateTime.Today.Year,
                Width = 110,
                Visible = false
            };
            txtSearch = new TextBox { Visible = false, Width = 0, Height = 0 };

            selectorRow.Controls.Add(lblSelect, 0, 0);
            selectorRow.Controls.Add(dtpDay, 1, 0);
            selectorRow.Controls.Add(dtpMonth, 1, 0);
            selectorRow.Controls.Add(nudYear, 1, 0);

            summaryPanel = new RoundedPanel
            {
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(239, 246, 255),
                Padding = new Padding(16, 12, 16, 12),
                Margin = new Padding(0, 0, 0, 14),
                BorderRadius = 12
            };
            var sumLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2
            };
            sumLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            sumLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            lblSummaryTitle = new Label
            {
                AutoSize = true,
                Text = "Total Revenue for Today",
                Font = new Font("Segoe UI", 11F),
                ForeColor = Color.FromArgb(30, 41, 59)
            };
            lblSummaryRevenue = new Label
            {
                AutoSize = true,
                Text = "$0.00",
                Font = new Font("Segoe UI", 17F, FontStyle.Bold),
                ForeColor = Color.FromArgb(37, 99, 235)
            };
            lblSummaryTxn = new Label
            {
                AutoSize = true,
                Text = "0 transactions",
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = Color.FromArgb(71, 85, 105)
            };
            sumLayout.Controls.Add(lblSummaryTitle, 0, 0);
            sumLayout.Controls.Add(lblSummaryRevenue, 1, 0);
            sumLayout.Controls.Add(lblSummaryTxn, 0, 1);
            summaryPanel.Controls.Add(sumLayout);

            dgvBreakdown = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                EnableHeadersVisualStyles = false,
                RowHeadersVisible = false,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = Color.FromArgb(229, 231, 235),
                ScrollBars = ScrollBars.Vertical
            };
            GridTheme.ApplyStandard(dgvBreakdown);
            dgvBreakdown.ColumnHeadersDefaultCellStyle.BackColor = Color.White;
            dgvBreakdown.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(30, 41, 59);
            dgvBreakdown.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            dgvBreakdown.RowTemplate.Height = 34;

            var detailsStack = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 6,
                Padding = new Padding(0),
                BackColor = detailsHost.BackColor
            };
            detailsStack.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            detailsStack.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            detailsStack.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            detailsStack.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            detailsStack.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            detailsStack.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            detailsStack.Controls.Add(lblTitle, 0, 0);
            detailsStack.Controls.Add(lblSubtitle, 0, 1);
            detailsStack.Controls.Add(tabReports, 0, 2);
            detailsStack.Controls.Add(selectorRow, 0, 3);
            detailsStack.Controls.Add(summaryPanel, 0, 4);
            detailsStack.Controls.Add(dgvBreakdown, 0, 5);

            detailsHost.Controls.Clear();
            detailsHost.Controls.Add(detailsStack);
            detailsHost.ResumeLayout(performLayout: true);

            // Responsive wiring (outer scroll enabled)
            WireResponsiveReportsAdaptive(
                mainFlow,
                detailsHost,
                dgvBreakdown,
                fixedOverheadControls: new Control[] { lblTitle, lblSubtitle, tabReports, selectorRow, summaryPanel },
                minRows: 7,
                maxRows: 16,
                preferredViewportFraction: 0.72);

            WireResponsiveKpiReflow(mainFlow, statsTable, kpiCards);

            // Copy out params to avoid capturing out variables in lambdas
            var tabRef = tabReports;
            var dayRef = dtpDay;
            var monthRef = dtpMonth;
            var yearRef = nudYear;
            var sumTitleRef = lblSummaryTitle;

            tabRef.SelectedIndexChanged += (_, __) =>
            {
                dayRef.Visible = tabRef.SelectedIndex == 0;
                monthRef.Visible = tabRef.SelectedIndex == 1;
                yearRef.Visible = tabRef.SelectedIndex == 2;
                sumTitleRef.Text = tabRef.SelectedIndex switch
                {
                    0 => "Total Revenue for Today",
                    1 => "Total Revenue for This Month",
                    2 => "Total Revenue for This Year",
                    _ => sumTitleRef.Text
                };
            };
        }

        // ----------- INTERNAL HELPERS (SCROLLABLE IMPLEMENTATION) -----------

        private static void BuildScrollableHost(
            Control root,
            out FlowLayoutPanel mainFlow,
            out Control statsHost,
            out Control detailsHost)
        {
            if (root == null) throw new ArgumentNullException(nameof(root));

            root.SuspendLayout();
            root.Controls.Clear();

            mainFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                BackColor = root.BackColor,
                Margin = Padding.Empty,
                Padding = new Padding(0),
                MinimumSize = new Size(350, 350) // <-- Add this line to ensure scrollbars on small windows
            };

            // Hide horizontal scroll
            mainFlow.HorizontalScroll.Maximum = 0;
            mainFlow.HorizontalScroll.Enabled = false;
            mainFlow.HorizontalScroll.Visible = false;

            root.Controls.Add(mainFlow);

            var kpiHost = new Panel
            {
                AutoSize = true,
                Dock = DockStyle.Top,
                BackColor = root.BackColor,
                Margin = new Padding(0, 0, 0, 12)
            };
            mainFlow.Controls.Add(kpiHost);
            mainFlow.SetFlowBreak(kpiHost, true);

            var details = new RoundedPanel
            {
                BorderRadius = 12,
                BackColor = Color.White,
                Padding = new Padding(20),
                Margin = new Padding(0, 0, 0, 0),
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };
            mainFlow.Controls.Add(details);
            mainFlow.SetFlowBreak(details, true);

            statsHost = kpiHost;
            detailsHost = details;

            root.ResumeLayout(performLayout: true);
        }

        private static void WireResponsiveReportsAdaptive(
            FlowLayoutPanel mainFlow,
            Control detailsHost,
            DataGridView breakdownGrid,
            Control[] fixedOverheadControls,
            int minRows = 7,
            int maxRows = 16,
            double preferredViewportFraction = 0.70)
        {
            if (mainFlow == null || detailsHost == null || breakdownGrid == null)
                return;

            void Apply()
            {
                // Uniform width
                int reserve = mainFlow.VerticalScroll.Visible ? SystemInformation.VerticalScrollBarWidth : 0;
                int targetWidth = Math.Max(0, mainFlow.ClientSize.Width - reserve);
                foreach (Control c in mainFlow.Controls)
                    c.Width = targetWidth;

                AdjustBreakdownSectionHeight(
                    detailsHost,
                    breakdownGrid,
                    fixedOverheadControls,
                    minRows,
                    maxRows,
                    preferredViewportFraction,
                    mainFlow);

                ForceOuterScroll(mainFlow);
            }

            // First pass and handle-created pass
            Apply();
            if (!mainFlow.IsHandleCreated)
                mainFlow.HandleCreated += (_, __) => Apply();

            mainFlow.SizeChanged += (_, __) => Apply();
            mainFlow.Layout += (_, __) => Apply();
            breakdownGrid.DataBindingComplete += (_, __) => Apply();
            breakdownGrid.RowsAdded += (_, __) => Apply();
            breakdownGrid.RowsRemoved += (_, __) => Apply();
        }

        private static void AdjustBreakdownSectionHeight(
            Control sectionPanel,
            DataGridView grid,
            Control[] overheadControls,
            int minRows,
            int maxRows,
            double preferredViewportFraction,
            FlowLayoutPanel viewportSource)
        {
            if (sectionPanel.Parent == null || grid == null) return;

            int viewportH = viewportSource.ClientSize.Height;
            if (viewportH <= 0)
                viewportH = Screen.FromControl(sectionPanel).WorkingArea.Height;

            // Metrics
            int rowH = Math.Max(24, grid.RowTemplate.Height);
            int headerH = Math.Max(28, grid.ColumnHeadersHeight);

            // Overhead height
            int overhead = 0;
            if (overheadControls != null)
            {
                foreach (var ctrl in overheadControls)
                {
                    if (ctrl == null) continue;
                    int h = ctrl.Height;
                    if (h <= 0) h = ctrl.PreferredSize.Height;
                    overhead += h;
                    if (ctrl is TableLayoutPanel tl)
                        overhead += tl.Padding.Vertical;
                }
            }

            // Target rows based on preferred fraction
            int targetSectionH = (int)Math.Round(viewportH * preferredViewportFraction);
            int availableForGrid = targetSectionH - overhead - sectionPanel.Padding.Vertical;

            int tentativeRows = (availableForGrid - headerH - 2) / rowH;
            tentativeRows = Math.Clamp(tentativeRows, minRows, maxRows);
            int rows = Math.Max(tentativeRows, minRows);

            // Grid content height (ensure visibility even on small monitors)
            int gridH = headerH + (rowH * rows) + 2;
            int minGridVisible = headerH + (rowH * Math.Max(minRows, 6)) + 2;
            if (gridH < minGridVisible)
                gridH = minGridVisible;

            // Final section height
            int desired = sectionPanel.Padding.Vertical + overhead + gridH + 12;

            // If page still below viewport, expand to force outer scrollbar
            int kpiBlockHeight = 0;
            foreach (Control child in viewportSource.Controls)
            {
                if (child == sectionPanel) break;
                kpiBlockHeight += child.Height + child.Margin.Vertical;
            }

            int totalProjected = kpiBlockHeight + desired;
            if (totalProjected < viewportH)
            {
                int extra = viewportH - totalProjected + 200;
                desired += extra;
            }

            sectionPanel.Height = desired;

            // Ensure grid fills
            grid.Dock = DockStyle.Fill;
            grid.MinimumSize = new Size(0, 0);
            grid.MaximumSize = new Size(int.MaxValue, int.MaxValue);
        }

        private static void WireResponsiveKpiReflow(
            FlowLayoutPanel mainFlow,
            TableLayoutPanel statsTable,
            Control[] cards)
        {
            if (mainFlow == null || statsTable == null || cards == null || cards.Length == 0) return;

            void Reflow()
            {
                int reserve = mainFlow.VerticalScroll.Visible ? SystemInformation.VerticalScrollBarWidth : 0;
                int totalWidth = Math.Max(0, mainFlow.ClientSize.Width - reserve);
                ReflowStatsTable(statsTable, cards, totalWidth, maxCols: 3);
                ForceOuterScroll(mainFlow);
            }

            mainFlow.SizeChanged += (_, __) => Reflow();
            mainFlow.Layout += (_, __) => Reflow();
            if (mainFlow.IsHandleCreated) Reflow();
        }

        private static void ForceOuterScroll(FlowLayoutPanel flow)
        {
            if (flow == null) return;

            int totalHeight = 0;
            foreach (Control c in flow.Controls)
                totalHeight += c.Height + c.Margin.Vertical;

            // Increase slack to ensure scrollbar appears even on small windows
            int slack = 200; // was 32, now 200 for more aggressive scroll trigger
            flow.AutoScrollMinSize = new Size(0, totalHeight + slack);
        }

        // ----------- SHARED CARD CREATION -----------

        private static RoundedPanel CreateStatCard(
            string title,
            Color revenueForeColor,
            Color cardBackColor,
            out Label lblRevenue,
            out Label lblTransactions)
        {
            var card = new RoundedPanel
            {
                Dock = DockStyle.Fill,
                BackColor = cardBackColor,
                Padding = new Padding(18, 14, 18, 14),
                Margin = new Padding(10),
                BorderRadius = 12
            };

            var lblTitle = new Label
            {
                AutoSize = true,
                Text = title,
                Font = new Font("Segoe UI", 13F),
                ForeColor = Color.FromArgb(30, 41, 59)
            };
            lblRevenue = new Label
            {
                AutoSize = true,
                Text = "$0.00",
                Font = new Font("Segoe UI", 22F, FontStyle.Bold),
                ForeColor = revenueForeColor,
                Margin = new Padding(0, 20, 0, 6)
            };
            lblTransactions = new Label
            {
                AutoSize = true,
                Text = "0 transactions",
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = Color.FromArgb(30, 41, 59)
            };

            var inner = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3
            };
            inner.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            inner.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            inner.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            inner.Controls.Add(lblTitle, 0, 0);
            inner.Controls.Add(lblRevenue, 0, 1);
            inner.Controls.Add(lblTransactions, 0, 2);

            card.Controls.Add(inner);
            return card;
        }

        // Add inside the RevenueReportViewBuilder class (helpers region)
        private static void ReflowStatsTable(TableLayoutPanel table, Control[] cards, int totalWidth, int maxCols)
        {
            if (table == null || cards == null || cards.Length == 0) return;

            int minCard = 250;   // approximate min card width
            int gap = 16;
            int cols = Math.Clamp(
                totalWidth <= 0 ? maxCols :
                Math.Max(1, Math.Min(maxCols, (totalWidth + gap) / (minCard + gap))),
                1, maxCols);

            int rows = (int)Math.Ceiling(cards.Length / (double)cols);

            table.SuspendLayout();

            // Preserve the card instances; just rebuild the grid layout
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
    }
}
