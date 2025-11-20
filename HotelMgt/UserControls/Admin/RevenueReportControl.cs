using System;
using System.Data;
using System.Globalization;
using System.Windows.Forms;
using Microsoft.Data.SqlClient; // Use Microsoft.Data.SqlClient only
using HotelMgt.Services;
using HotelMgt.UIStyles; // RevenueReportViewBuilder

namespace HotelMgt.UserControls.Admin
{
    public partial class RevenueReportControl : UserControl
    {
        private readonly DatabaseService _dbService = new();
        private readonly CultureInfo _currencyCulture = CultureInfo.GetCultureInfo("en-PH");

        // Cards (top)
        private Label _lblTodayRevenue = null!;
        private Label _lblTodayTxn = null!;
        private Label _lblMonthRevenue = null!;
        private Label _lblMonthTxn = null!;
        private Label _lblYearRevenue = null!;
        private Label _lblYearTxn = null!;

        // Details section
        private TabControl _tabReports = null!;
        private DateTimePicker _dtpDay = null!;
        private DateTimePicker _dtpMonth = null!;
        private NumericUpDown _nudYear = null!;
        private TextBox _txtSearch = null!;
        private Label _lblSummaryTitle = null!;
        private Label _lblSummaryRevenue = null!;
        private Label _lblSummaryTxn = null!;
        private DataGridView _dgvBreakdown = null!;

        public RevenueReportControl()
        {
            InitializeComponent();
            this.Load += RevenueReportControl_Load;
            this.VisibleChanged += RevenueReportControl_VisibleChanged;
        }

        private void RevenueReportControl_Load(object? sender, EventArgs e)
        {
            RevenueReportViewBuilder.BuildDetailsSection(
                this,
                out _tabReports,
                out _dtpDay,
                out _dtpMonth,
                out _nudYear,
                out _txtSearch,
                out _,
                out _lblSummaryTitle,
                out _lblSummaryRevenue,
                out _lblSummaryTxn,
                out _dgvBreakdown);

            RevenueReportViewBuilder.Build(
                this,
                out _lblTodayRevenue, out _lblTodayTxn,
                out _lblMonthRevenue, out _lblMonthTxn,
                out _lblYearRevenue, out _lblYearTxn);

            WireDetailsEvents();

            LoadRevenueStats();
            RefreshDetails();
        }

        private void RevenueReportControl_VisibleChanged(object? sender, EventArgs e)
        {
            if (this.Visible)
            {
                LoadRevenueStats();
                RefreshDetails();
            }
        }

        private void WireDetailsEvents()
        {
            _tabReports.SelectedIndexChanged += (_, __) =>
            {
                switch (_tabReports.SelectedIndex)
                {
                    case 0:
                        ToggleSelector(showDay: true, showMonth: false, showYear: false, caption: "Select Date");
                        break;
                    case 1:
                        ToggleSelector(showDay: false, showMonth: true, showYear: false, caption: "Select Month");
                        break;
                    case 2:
                        ToggleSelector(showDay: false, showMonth: false, showYear: true, caption: "Select Year");
                        break;
                }
                RefreshDetails();
            };

            _dtpDay.ValueChanged += (_, __) => { if (_tabReports.SelectedIndex == 0) RefreshDetails(); };
            _dtpMonth.ValueChanged += (_, __) => { if (_tabReports.SelectedIndex == 1) RefreshDetails(); };
            _nudYear.ValueChanged += (_, __) => { if (_tabReports.SelectedIndex == 2) RefreshDetails(); };
            _txtSearch.TextChanged += (_, __) => RefreshDetails();
        }

        private void ToggleSelector(bool showDay, bool showMonth, bool showYear, string caption)
        {
            var selectorRow = _dtpDay.Parent;
            if (selectorRow is TableLayoutPanel tlp && tlp.Controls.Count > 0)
            {
                foreach (Control c in tlp.Controls)
                {
                    if (c is Label lbl && lbl.Text.StartsWith("Select "))
                    {
                        lbl.Text = caption;
                        break;
                    }
                }
            }

            _dtpDay.Visible = showDay;
            _dtpMonth.Visible = showMonth;
            _nudYear.Visible = showYear;
        }

        private void LoadRevenueStats()
        {
            try
            {
                using var conn = _dbService.GetConnection();
                conn.Open();

                var today = DateTime.Today;
                var monthStart = new DateTime(today.Year, today.Month, 1);
                var yearStart = new DateTime(today.Year, 1, 1);

                (decimal sumToday, int countToday) = GetRevenueAggregate(conn,
                    "CAST(PaymentDate AS DATE) = @D0",
                    new SqlParameter("@D0", today));

                (decimal sumMonth, int countMonth) = GetRevenueAggregate(conn,
                    "PaymentDate >= @D1 AND PaymentDate < @D2",
                    new SqlParameter("@D1", monthStart),
                    new SqlParameter("@D2", monthStart.AddMonths(1)));

                (decimal sumYear, int countYear) = GetRevenueAggregate(conn,
                    "PaymentDate >= @D3 AND PaymentDate < @D4",
                    new SqlParameter("@D3", yearStart),
                    new SqlParameter("@D4", yearStart.AddYears(1)));

                _lblTodayRevenue.Text = sumToday.ToString("C2", _currencyCulture);
                _lblTodayTxn.Text = $"{countToday} transaction{(countToday == 1 ? "" : "s")}";

                _lblMonthRevenue.Text = sumMonth.ToString("C2", _currencyCulture);
                _lblMonthTxn.Text = $"{countMonth} transaction{(countMonth == 1 ? "" : "s")}";

                _lblYearRevenue.Text = sumYear.ToString("C2", _currencyCulture);
                _lblYearTxn.Text = $"{countYear} transaction{(countYear == 1 ? "" : "s")}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading revenue stats: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RefreshDetails()
        {
            try
            {
                switch (_tabReports.SelectedIndex)
                {
                    case 0: LoadDailyReport(); break;
                    case 1: LoadMonthlyReport(); break;
                    case 2: LoadYearlyReport(); break;
                    default: LoadDailyReport(); break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading report: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadDailyReport()
        {
            using var conn = _dbService.GetConnection();
            conn.Open();

            var day = _dtpDay.Value.Date;
            var filter = _txtSearch.Text?.Trim();
            var like = $"%{filter}%";

            var sql = @"
SELECT
    CONVERT(time(0), p.PaymentDate) AS [Time],
    p.PaymentID AS [Payment ID],
    p.Amount AS [Amount],
    p.PaymentMethod AS [Method],
    (e.FirstName + ' ' + e.LastName) AS [Employee],
    p.TransactionReference AS [Reference]
FROM Payments p
INNER JOIN Employees e ON e.EmployeeID = p.EmployeeID
WHERE CAST(p.PaymentDate AS DATE) = @Day
  AND (@Filter IS NULL OR p.TransactionReference LIKE @Like OR CONVERT(nvarchar(20), p.PaymentID) LIKE @Like)
ORDER BY p.PaymentDate DESC;";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Day", day);
            cmd.Parameters.AddWithValue("@Filter", string.IsNullOrWhiteSpace(filter) ? DBNull.Value : filter as object);
            cmd.Parameters.AddWithValue("@Like", like);

            using var da = new SqlDataAdapter(cmd);
            var dt = new DataTable();
            da.Fill(dt);

            _dgvBreakdown.DataSource = dt;
            FormatDailyGrid();
            LimitGridToVisibleRows(_dgvBreakdown, 5);

            decimal sum = 0m;
            foreach (DataRow r in dt.Rows)
                sum += Convert.ToDecimal(r["Amount"]);

            _lblSummaryTitle.Text = $"Total (Top 5) for {day:MMMM dd, yyyy}";
            _lblSummaryRevenue.Text = sum.ToString("C2", _currencyCulture);
            _lblSummaryTxn.Text = $"{dt.Rows.Count} transaction{(dt.Rows.Count == 1 ? "" : "s")}";
        }

        private void LoadMonthlyReport()
        {
            using var conn = _dbService.GetConnection();
            conn.Open();

            var monthStart = new DateTime(_dtpMonth.Value.Year, _dtpMonth.Value.Month, 1);
            var monthEnd = monthStart.AddMonths(1);

            var sql = @"
SELECT 
    CAST(p.PaymentDate AS DATE) AS [Date],
    COUNT(*) AS [Transactions],
    SUM(p.Amount) AS [Total Revenue],
    AVG(p.Amount) AS [Avg Transaction]
FROM Payments p
WHERE p.PaymentDate >= @Start AND p.PaymentDate < @End
GROUP BY CAST(p.PaymentDate AS DATE)
ORDER BY [Date] DESC;";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Start", monthStart);
            cmd.Parameters.AddWithValue("@End", monthEnd);

            using var da = new SqlDataAdapter(cmd);
            var dt = new DataTable();
            da.Fill(dt);

            _dgvBreakdown.DataSource = dt;
            FormatMonthlyGrid();
            LimitGridToVisibleRows(_dgvBreakdown, 5);

            decimal total = 0m;
            int txns = 0;
            foreach (DataRow r in dt.Rows)
            {
                total += r["Total Revenue"] == DBNull.Value ? 0m : Convert.ToDecimal(r["Total Revenue"]);
                txns += r["Transactions"] == DBNull.Value ? 0 : Convert.ToInt32(r["Transactions"]);
            }

            _lblSummaryTitle.Text = $"Top 5 Days in {monthStart:MMMM yyyy}";
            _lblSummaryRevenue.Text = total.ToString("C2", _currencyCulture);
            _lblSummaryTxn.Text = $"{txns} transaction{(txns == 1 ? "" : "s")}";
        }

        private void LoadYearlyReport()
        {
            using var conn = _dbService.GetConnection();
            conn.Open();

            int year = (int)_nudYear.Value;

            var sql = @"
SELECT 
    DATENAME(month, DATEFROMPARTS(@Y, MONTH(p.PaymentDate), 1)) AS [Month],
    MONTH(p.PaymentDate) AS [MonthNum],
    COUNT(*) AS [Transactions],
    SUM(p.Amount) AS [Total Revenue],
    AVG(p.Amount) AS [Avg Transaction]
FROM Payments p
WHERE YEAR(p.PaymentDate) = @Y
GROUP BY YEAR(p.PaymentDate), MONTH(p.PaymentDate)
ORDER BY [MonthNum];";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Y", year);

            using var da = new SqlDataAdapter(cmd);
            var dt = new DataTable();
            da.Fill(dt);

            if (dt.Columns.Contains("MonthNum"))
                dt.Columns["MonthNum"].ColumnMapping = MappingType.Hidden;

            _dgvBreakdown.DataSource = dt;
            FormatYearlyGrid();
            LimitGridToVisibleRows(_dgvBreakdown, 5);

            decimal total = 0m;
            int txns = 0;
            foreach (DataRow r in dt.Rows)
            {
                total += r["Total Revenue"] == DBNull.Value ? 0m : Convert.ToDecimal(r["Total Revenue"]);
                txns += r["Transactions"] == DBNull.Value ? 0 : Convert.ToInt32(r["Transactions"]);
            }

            _lblSummaryTitle.Text = $"Top 5 Months of {year}";
            _lblSummaryRevenue.Text = total.ToString("C2", _currencyCulture);
            _lblSummaryTxn.Text = $"{txns} transaction{(txns == 1 ? "" : "s")}";
        }

        // All columns left aligned (data + headers)
        private void ApplyLeftAlignmentToAllColumns(DataGridView grid)
        {
            if (grid == null) return;
            grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            foreach (DataGridViewColumn col in grid.Columns)
            {
                col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            }
        }

        // Formats now only set number/currency formats, alignment forced left
        private void FormatDailyGrid()
        {
            var cols = _dgvBreakdown.Columns;
            if (cols["Time"] is { } cTime)
            {
                cTime.FillWeight = 80;
                cTime.DefaultCellStyle.Format = @"hh\:mm";
            }
            if (cols["Amount"] is { } cAmt)
            {
                cAmt.DefaultCellStyle.Format = "C2";
                cAmt.DefaultCellStyle.FormatProvider = _currencyCulture;
                cAmt.FillWeight = 110;
            }
            if (cols["Payment ID"] is { } cId)
            {
                cId.FillWeight = 90;
            }

            ApplyLeftAlignmentToAllColumns(_dgvBreakdown);
        }

        private void FormatMonthlyGrid()
        {
            var cols = _dgvBreakdown.Columns;
            if (cols["Date"] is { } cDate)
            {
                cDate.DefaultCellStyle.Format = "MM/dd/yyyy";
                cDate.FillWeight = 110;
            }
            if (cols["Transactions"] is { } cTx)
            {
                cTx.FillWeight = 90;
            }
            if (cols["Total Revenue"] is { } cTotal)
            {
                cTotal.DefaultCellStyle.Format = "C2";
                cTotal.DefaultCellStyle.FormatProvider = _currencyCulture;
                cTotal.FillWeight = 120;
            }
            if (cols["Avg Transaction"] is { } cAvg)
            {
                cAvg.DefaultCellStyle.Format = "C2";
                cAvg.DefaultCellStyle.FormatProvider = _currencyCulture;
                cAvg.FillWeight = 120;
            }

            ApplyLeftAlignmentToAllColumns(_dgvBreakdown);
        }

        private void FormatYearlyGrid()
        {
            var cols = _dgvBreakdown.Columns;
            if (cols["Transactions"] is { } cTx)
            {
                cTx.FillWeight = 90;
            }
            if (cols["Total Revenue"] is { } cTotal)
            {
                cTotal.DefaultCellStyle.Format = "C2";
                cTotal.DefaultCellStyle.FormatProvider = _currencyCulture;
                cTotal.FillWeight = 120;
            }
            if (cols["Avg Transaction"] is { } cAvg)
            {
                cAvg.DefaultCellStyle.Format = "C2";
                cAvg.DefaultCellStyle.FormatProvider = _currencyCulture;
                cAvg.FillWeight = 120;
            }

            ApplyLeftAlignmentToAllColumns(_dgvBreakdown);
        }

        private static (decimal sum, int count) GetRevenueAggregate(SqlConnection conn, string whereClause, params SqlParameter[] parameters)
        {
            var sql = $@"
SELECT 
    SUM(CAST(Amount AS decimal(18,2))) AS TotalAmount,
    COUNT(1) AS TxnCount
FROM Payments
WHERE {whereClause};";

            using var cmd = new SqlCommand(sql, conn);
            foreach (var p in parameters) cmd.Parameters.Add(p);

            using var rdr = cmd.ExecuteReader();
            if (rdr.Read())
            {
                var sum = rdr.IsDBNull(0) ? 0m : rdr.GetDecimal(0);
                var count = rdr.IsDBNull(1) ? 0 : rdr.GetInt32(1);
                return (sum, count);
            }
            return (0m, 0);
        }

        private void LimitGridToVisibleRows(DataGridView grid, int visibleRows)
        {
            if (grid == null) return;
            var header = grid.ColumnHeadersHeight;
            var rowH = grid.RowTemplate.Height;
            grid.ScrollBars = ScrollBars.Vertical;
            grid.Dock = DockStyle.Top;
            grid.Height = header + (rowH * visibleRows) + 2;
        }
    }
}