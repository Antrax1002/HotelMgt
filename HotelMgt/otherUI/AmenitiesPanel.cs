using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using HotelMgt.Services;

namespace HotelMgt.otherUI
{
    /// <summary>
    /// A reusable panel for selecting amenities with checkboxes and quantity boxes, split into two columns.
    /// </summary>
    public class AmenitiesPanel : UserControl
    {
        private readonly DatabaseService _dbService = new DatabaseService();

        // Represents a selected amenity and its quantity
        public class AmenitySelection
        {
            public int AmenityID { get; set; }
            public string Name { get; set; } = "";
            public decimal Price { get; set; }
            public int Quantity { get; set; }
        }

        // Internal row tracking for value retrieval
        private readonly List<(CheckBox chk, NumericUpDown qty, int amenityId, string name, decimal price)> _rows = new();

        public event EventHandler? SelectionChanged;

        public AmenitiesPanel()
        {
            this.Dock = DockStyle.Fill;
            this.AutoScroll = true;
            this.BackColor = Color.White;
            BuildUI();
        }

        private void BuildUI()
        {
            var amenities = LoadAmenities();
            if (amenities.Count == 0)
            {
                Controls.Add(new Label
                {
                    Text = "No amenities available.",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = Color.Gray
                });
                return;
            }

            int mid = (amenities.Count + 1) / 2;
            var left = amenities.GetRange(0, mid);
            var right = amenities.GetRange(mid, amenities.Count - mid);

            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = Math.Max(left.Count, right.Count),
                AutoSize = true,
                Padding = new Padding(16),
                BackColor = Color.White
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            for (int i = 0; i < table.RowCount; i++)
            {
                table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                if (i < left.Count)
                    table.Controls.Add(CreateAmenityRow(left[i]), 0, i);
                if (i < right.Count)
                    table.Controls.Add(CreateAmenityRow(right[i]), 1, i);
            }

            var header = new Label
            {
                Text = "Inclusion", // Changed from "Extra Amenities / Services"
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 32,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(4, 0, 0, 0)
            };

            Controls.Add(table);
            Controls.Add(header);
            table.BringToFront();
        }

        private void OnSelectionChanged()
        {
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        private Control CreateAmenityRow((int id, string name, decimal price) amenity)
        {
            // Use a TableLayoutPanel for proper horizontal layout
            var row = new TableLayoutPanel
            {
                ColumnCount = 3,
                RowCount = 1,
                Dock = DockStyle.Top,
                AutoSize = true,
                Margin = new Padding(0, 2, 0, 2),
                BackColor = Color.Transparent
            };
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70f)); // Label
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 32f)); // Checkbox
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 60f)); // NumericUpDown

            var lbl = new Label
            {
                // Use PHP instead of currency symbol
                Text = $"{amenity.name} (PHP {amenity.price:#,0.00})",
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 6, 0, 0)
            };

            var chk = new CheckBox
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(0, 0, 0, 0)
            };

            var qty = new NumericUpDown
            {
                Dock = DockStyle.Fill,
                Minimum = 0,
                Maximum = 99,
                Value = 0,
                Enabled = false,
                Font = new Font("Segoe UI", 10, FontStyle.Regular)
            };

            chk.CheckedChanged += (s, e) =>
            {
                qty.Enabled = chk.Checked;
                qty.Value = chk.Checked ? 1 : 0;
                OnSelectionChanged();
            };

            qty.ValueChanged += (s, e) => OnSelectionChanged();

            row.Controls.Add(lbl, 0, 0);
            row.Controls.Add(chk, 1, 0);
            row.Controls.Add(qty, 2, 0);

            _rows.Add((chk, qty, amenity.id, amenity.name, amenity.price));
            return row;
        }

        private List<(int id, string name, decimal price)> LoadAmenities()
        {
            var list = new List<(int, string, decimal)>();
            try
            {
                using var conn = _dbService.GetConnection();
                conn.Open();
                using var cmd = new SqlCommand("SELECT AmenityID, Name, Price FROM Amenities WHERE IsActive = 1 ORDER BY Name", conn);
                using var rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    list.Add((rdr.GetInt32(0), rdr.GetString(1), rdr.GetDecimal(2)));
            }
            catch
            {
                // Optionally log or handle error
            }
            return list;
        }

        /// <summary>
        /// Returns a list of selected amenities and their quantities.
        /// </summary>
        public List<AmenitySelection> GetSelectedAmenities()
        {
            var result = new List<AmenitySelection>();
            foreach (var (chk, qty, id, name, price) in _rows)
            {
                if (chk.Checked && qty.Value > 0)
                    result.Add(new AmenitySelection { AmenityID = id, Name = name, Price = price, Quantity = (int)qty.Value });
            }
            return result;
        }
    }
}