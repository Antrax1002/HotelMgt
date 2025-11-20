using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using HotelMgt.Custom; // Add this for RoundedButton

namespace HotelMgt.UIStyles
{
    public static class GuestSearchViewBuilder
    {
        // Builds the UI under the provided parent and wires the search action
        // Note: A "Delete" button is now included in the layout (named "btnDelete").
        //       Existing callers do not need to change their signature; retrieve it by name if needed.
        public static void Build(
            Control parent,
            Action searchAction,
            out Label lblTitle,
            out Label lblSubtitle,
            out TextBox txtSearch,
            out Button btnSearch,
            out DataGridView dgvGuests)
        {
            parent.SuspendLayout();
            parent.Controls.Clear();

            var root = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(20)
            };
            parent.Controls.Add(root);

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 6, // Add extra row for Delete button
                BackColor = Color.White
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // title
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // subtitle
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // "Search Guests"
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // search row
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // grid
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Delete button row
            root.Controls.Add(layout);

            lblTitle = new Label
            {
                Text = "Guest Search",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 5)
            };
            layout.Controls.Add(lblTitle, 0, 0);

            lblSubtitle = new Label
            {
                Text = "Search guests by name, email, phone, or ID number.",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Gray,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 12)
            };
            layout.Controls.Add(lblSubtitle, 0, 1);

            var lblField = new Label
            {
                Text = "Search Guests",
                Font = new Font("Segoe UI", 9),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 6)
            };
            layout.Controls.Add(lblField, 0, 2);

            // Search row: input + Search + Import + Export + Delete
            var searchRow = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 5,
                RowCount = 1,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 12)
            };
            searchRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));   // textbox stretches
            searchRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90F));  // Search
            searchRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90F));  // Import
            searchRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90F));  // Export
            searchRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90F));  // Delete

            txtSearch = new TextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10),
                Height = 38 // Match button height
#if NET6_0_OR_GREATER
                , PlaceholderText = "Enter name, email, phone, or ID number..."
#endif
            };
            txtSearch.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    searchAction?.Invoke();
                }
            };
            searchRow.Controls.Add(txtSearch, 0, 0);

            btnSearch = new RoundedButton
            {
                Text = "Search",
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(37, 99, 235),
                ForeColor = Color.White,
                Height = 38,
                Margin = new Padding(8, 0, 0, 0)
            };
            btnSearch.Click += (_, __) => searchAction?.Invoke();
            searchRow.Controls.Add(btnSearch, 1, 0);

            var btnImport = new RoundedButton
            {
                Name = "btnImport",
                Text = "Import",
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(16, 185, 129), // Green
                ForeColor = Color.White,
                Height = 38,
                Margin = new Padding(8, 0, 0, 0)
            };
            // btnImport.Click += ... (add your handler)
            searchRow.Controls.Add(btnImport, 2, 0);

            var btnExport = new RoundedButton
            {
                Name = "btnExport",
                Text = "Export",
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(59, 130, 246), // Blue
                ForeColor = Color.White,
                Height = 38,
                Margin = new Padding(8, 0, 0, 0)
            };
            // btnExport.Click += ... (add your handler)
            searchRow.Controls.Add(btnExport, 3, 0);

            var btnDelete = new RoundedButton
            {
                Name = "btnDelete",
                Text = "Delete",
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(220, 38, 38),
                ForeColor = Color.White,
                Height = 38,
                Margin = new Padding(8, 0, 0, 0),
                Enabled = false
            };
            searchRow.Controls.Add(btnDelete, 4, 0);

            layout.Controls.Add(searchRow, 0, 3);

            // Guests grid
            dgvGuests = new DataGridView
            {
                Dock = DockStyle.Fill,
                ScrollBars = ScrollBars.Vertical // avoid horizontal scroll
            };
            GridTheme.ApplyStandard(dgvGuests);
            ApplyBalancedFillColumns(dgvGuests);
            ApplyGuestDisplayFormatting(dgvGuests);

            dgvGuests.AutoGenerateColumns = false;
            dgvGuests.DataBindingComplete += (s, __) =>
            {
                if (s is DataGridView g)
                {
                    ConfigureGuestGridColumns(g);
                    g.Invalidate();
                }
            };
            dgvGuests.ColumnAdded += (s, __) =>
            {
                if (s is DataGridView g)
                {
                    ConfigureGuestGridColumns(g);
                    g.Invalidate();
                }
            };

            layout.Controls.Add(dgvGuests, 0, 4);

            parent.ResumeLayout(performLayout: true);
        }

        // Balanced fill behavior
        private static void ApplyBalancedFillColumns(DataGridView grid)
        {
            void Balance()
            {
                if (grid.Columns.Count == 0) return;
                foreach (DataGridViewColumn col in grid.Columns)
                {
                    col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    col.FillWeight = 1f;
                }
            }
            grid.DataBindingComplete += (_, __) => Balance();
            grid.ColumnAdded += (_, __) => Balance();
            if (grid.IsHandleCreated && grid.Columns.Count > 0) Balance();
        }

        private static void ConfigureGuestGridColumns(DataGridView grid)
        {
            var state = s_guestGridStates.GetValue(grid, _ => new GridConfigState());
            if (state.Configuring) return;

            if (!grid.AutoGenerateColumns &&
                grid.Columns.Count == 4 &&
                grid.Columns.Cast<DataGridViewColumn>().Select(c => c.HeaderText)
                    .SequenceEqual(new[] { "Name", "Contact", "Email", "ID" }))
                return;

            state.Configuring = true;
            try
            {
                grid.AutoGenerateColumns = false;
                grid.Columns.Clear();

                DataGridViewTextBoxColumn Add(string name, int weight = 1)
                {
                    var col = new DataGridViewTextBoxColumn
                    {
                        Name = name,
                        HeaderText = name,
                        ReadOnly = true,
                        AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                        FillWeight = weight
                    };
                    grid.Columns.Add(col);
                    return col;
                }

                Add("Name", 2);
                Add("Contact", 1);
                Add("Email", 1);
                Add("ID", 1);
            }
            finally
            {
                state.Configuring = false;
            }
        }

        private static void ApplyGuestDisplayFormatting(DataGridView grid)
        {
            object? GetField(object? dataItem, params string[] names)
            {
                if (dataItem == null) return null;
                if (dataItem is DataRowView drv)
                {
                    var row = drv.Row;
                    foreach (var n in names)
                        if (row.Table.Columns.Contains(n)) return row[n];
                    return null;
                }
                if (dataItem is IDictionary<string, object> dict)
                {
                    foreach (var n in names)
                        if (dict.TryGetValue(n, out var v)) return v;
                    return null;
                }
                var t = dataItem.GetType();
                foreach (var n in names)
                {
                    var p = t.GetProperty(n, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    if (p != null) return p.GetValue(dataItem);
                }
                return null;
            }

            string? S(object? v) => v == null || v == DBNull.Value ? null : Convert.ToString(v)?.Trim();

            grid.CellFormatting += (s, e) =>
            {
                if (e.RowIndex < 0) return;
                var col = grid.Columns[e.ColumnIndex];
                var header = (col.HeaderText ?? col.Name ?? string.Empty).Trim();
                var dataItem = grid.Rows[e.RowIndex].DataBoundItem;

                if (header.Equals("Name", StringComparison.OrdinalIgnoreCase))
                {
                    var first = S(GetField(dataItem, "FirstName"));
                    var middle = S(GetField(dataItem, "MiddleName"));
                    var last = S(GetField(dataItem, "LastName"));
                    var full = string.Join(" ", new[] { first, middle, last }.Where(x => !string.IsNullOrEmpty(x)));
                    e.Value = full;
                    e.FormattingApplied = true;
                    return;
                }

                if (header.Equals("Contact", StringComparison.OrdinalIgnoreCase))
                {
                    var number = S(GetField(dataItem, "PhoneNumber", "Phone", "Mobile", "ContactNo", "ContactNumber"));
                    e.Value = number ?? string.Empty;
                    e.FormattingApplied = true;
                    return;
                }

                if (header.Equals("Email", StringComparison.OrdinalIgnoreCase))
                {
                    var email = S(GetField(dataItem, "Email", "EmailAddress"));
                    e.Value = email ?? string.Empty;
                    e.FormattingApplied = true;
                    return;
                }

                if (header.Equals("ID", StringComparison.OrdinalIgnoreCase))
                {
                    var idType = S(GetField(dataItem, "IDType", "IdType"));
                    var idNumber = S(GetField(dataItem, "IDNumber", "IdNumber"));
                    e.Value = (idType, idNumber) switch
                    {
                        (not null, not null) => $"[{idType}] {idNumber}",
                        (not null, null) => $"[{idType}]",
                        (null, not null) => idNumber,
                        _ => string.Empty
                    };
                    e.FormattingApplied = true;
                }
            };

            grid.DataBindingComplete += (_, __) => grid.Invalidate();
            grid.ColumnAdded += (_, __) => grid.Invalidate();
        }

        private sealed class GridConfigState { public bool Configuring; }
        private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<DataGridView, GridConfigState> s_guestGridStates = new();
    }
}
