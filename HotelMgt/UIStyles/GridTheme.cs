using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace HotelMgt.UIStyles
{
    public static class GridTheme
    {
        // Tracks which grids have event hooks to avoid duplicate subscriptions
        private sealed class AppliedMarker { }
        private static readonly ConditionalWeakTable<DataGridView, AppliedMarker> Applied = new();

        public static void ApplyStandard(DataGridView grid)
        {
            if (grid is null) return;

            ApplyStyles(grid);

            if (!Applied.TryGetValue(grid, out _))
            {
                Applied.Add(grid, new AppliedMarker());

                // Re-apply after binding or columns change (late style mutations)
                grid.DataBindingComplete += (_, __) => ApplyStyles(grid);
                grid.ColumnAdded += (_, __) => ApplyStyles(grid);
                grid.ColumnRemoved += (_, __) => ApplyStyles(grid);

                // Cleanup marker on handle destroy
                grid.HandleDestroyed += (_, __) =>
                {
                    try { Applied.Remove(grid); } catch { /* ignore */ }
                };
            }
        }

        private static void ApplyStyles(DataGridView grid)
        {
            // Header style (Room look)
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(243, 244, 246);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(55, 65, 81);
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            grid.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(243, 244, 246); // neutral
            grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.FromArgb(55, 65, 81);
            grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;

            // Strip header inner borders explicitly (prevents vertical seams)
            var h = grid.AdvancedColumnHeadersBorderStyle;
            h.Left = DataGridViewAdvancedCellBorderStyle.None;
            h.Right = DataGridViewAdvancedCellBorderStyle.None;
            h.Top = DataGridViewAdvancedCellBorderStyle.None;
            h.Bottom = DataGridViewAdvancedCellBorderStyle.None;

            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grid.ColumnHeadersHeight = 34;

            // Cells
            grid.DefaultCellStyle.Font = new Font("Segoe UI", 9);
            grid.DefaultCellStyle.ForeColor = Color.FromArgb(30, 41, 59);
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            grid.DefaultCellStyle.SelectionForeColor = Color.FromArgb(30, 41, 59);
            grid.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            grid.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
            grid.DefaultCellStyle.Padding = new Padding(6, 6, 6, 6);

            // Only horizontal separators; remove verticals explicitly
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            var c = grid.AdvancedCellBorderStyle;
            c.Left = DataGridViewAdvancedCellBorderStyle.None;
            c.Right = DataGridViewAdvancedCellBorderStyle.None;
            c.Top = DataGridViewAdvancedCellBorderStyle.None;
            c.Bottom = DataGridViewAdvancedCellBorderStyle.Single;

            // Row header borders removed fully
            grid.RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            var rh = grid.AdvancedRowHeadersBorderStyle;
            rh.Left = DataGridViewAdvancedCellBorderStyle.None;
            rh.Right = DataGridViewAdvancedCellBorderStyle.None;
            rh.Top = DataGridViewAdvancedCellBorderStyle.None;
            rh.Bottom = DataGridViewAdvancedCellBorderStyle.None;

            // Alternating rows and grid lines
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 250, 251);
            grid.RowHeadersVisible = false;
            grid.GridColor = Color.FromArgb(229, 231, 235);

            // Behavior
            grid.ReadOnly = true;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.AllowUserToResizeRows = false;
            grid.MultiSelect = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.BorderStyle = BorderStyle.None;
            grid.BackgroundColor = Color.White;
            grid.RowTemplate.Height = 32;
            grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

            // Normalize button columns surface (avoid 3D artifacts causing seam illusions)
            foreach (DataGridViewColumn col in grid.Columns)
            {
                if (col is DataGridViewButtonColumn btnCol)
                {
                    btnCol.FlatStyle = FlatStyle.Standard; // match Room
                    btnCol.SortMode = DataGridViewColumnSortMode.NotSortable;
                    btnCol.ReadOnly = true;
                }
            }
        }
    }
}