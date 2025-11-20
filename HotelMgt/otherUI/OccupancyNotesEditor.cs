using System;
using System.Data;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace HotelMgt.otherUI
{
    public static class OccupancyNotesEditor
    {
        public static void Enable(
            DataGridView occupancyGrid,
            Func<SqlConnection> getConnection)
        {
            if (occupancyGrid == null) return;

            occupancyGrid.CellDoubleClick -= OccupancyGrid_CellDoubleClick;
            occupancyGrid.CellDoubleClick += OccupancyGrid_CellDoubleClick;

            void OccupancyGrid_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
            {
                if (e.RowIndex < 0) return;
                var row = occupancyGrid.Rows[e.RowIndex];

                int? checkInId = ResolveCheckInId(row, getConnection);
                if (checkInId is null)
                {
                    MessageBox.Show("Could not determine the selected Check-In. Try selecting a different row.", "Edit Description",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                string room = Convert.ToString(row.Cells["Room"]?.Value) ?? "";
                string guest = Convert.ToString(row.Cells["GuestName"]?.Value) ?? "";
                string currentNotes = Convert.ToString(row.Cells["Notes"]?.Value) ?? "";

                using var dlg = new HotelMgt.otherUI.EditDescriptionForm(room, guest, currentNotes)
                {
                    StartPosition = FormStartPosition.CenterScreen,
                    ShowInTaskbar = false,
                    MinimizeBox = false,
                    MaximizeBox = false,
                    FormBorderStyle = FormBorderStyle.FixedDialog
                };

                var wa = Screen.FromPoint(Cursor.Position).WorkingArea;
                dlg.Width = Math.Min(dlg.Width, wa.Width - 40);
                dlg.Height = Math.Min(dlg.Height, wa.Height - 40);
                dlg.MinimumSize = dlg.Size;
                dlg.MaximumSize = dlg.Size;

                if (dlg.ShowDialog(occupancyGrid.FindForm()) == DialogResult.OK)
                {
                    var newNotes = dlg.Description;
                    bool ok = SaveNotes(checkInId.Value, newNotes, getConnection);
                    if (!ok) return;

                    if (occupancyGrid.CurrentRow != null)
                    {
                        occupancyGrid.CurrentRow.Cells["Notes"].Value = newNotes;
                        occupancyGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCells;
                    }
                }
            }
        }

        private static int? ResolveCheckInId(DataGridViewRow row, Func<SqlConnection> getConnection)
        {
            try
            {
                var room = row.Cells["Room"]?.Value?.ToString();
                var guest = row.Cells["GuestName"]?.Value?.ToString();
                DateTime? checkInDate = null;
                var rawDate = row.Cells["Check In"]?.Value;
                if (rawDate is DateTime dt) checkInDate = dt.Date;
                else if (DateTime.TryParse(Convert.ToString(rawDate), out var dt2)) checkInDate = dt2.Date;

                using var conn = getConnection();
                conn.Open();

                // 1) Try strict match: Room + Guest + CheckInDate + active
                using (var strict = new SqlCommand(@"
                        SELECT TOP 1 ci.CheckInID
                        FROM CheckIns ci
                        INNER JOIN Rooms rm ON rm.RoomID = ci.RoomID
                        INNER JOIN Guests g ON g.GuestID = ci.GuestID
                        WHERE ci.ActualCheckOutDateTime IS NULL
                          AND rm.RoomNumber = @Room
                          AND (@Guest IS NULL OR (g.FirstName + ' ' + g.LastName) = @Guest)
                          AND (@CheckInDate IS NULL OR CAST(ci.CheckInDateTime AS DATE) = @CheckInDate)
                        ORDER BY ci.CheckInDateTime DESC;", conn))
                {
                    strict.Parameters.AddWithValue("@Room", (object?)room ?? DBNull.Value);
                    strict.Parameters.AddWithValue("@Guest", string.IsNullOrWhiteSpace(guest) ? (object)DBNull.Value : guest);
                    strict.Parameters.AddWithValue("@CheckInDate", checkInDate.HasValue ? checkInDate.Value : (object)DBNull.Value);

                    var obj = strict.ExecuteScalar();
                    if (obj is int id1) return id1;
                }

                // 2) Fallback: Room + active (unique per room at a time)
                using (var byRoom = new SqlCommand(@"
                    SELECT TOP 1 ci.CheckInID
                    FROM CheckIns ci
                    INNER JOIN Rooms rm ON rm.RoomID = ci.RoomID
                    WHERE ci.ActualCheckOutDateTime IS NULL
                      AND rm.RoomNumber = @Room
                    ORDER BY ci.CheckInDateTime DESC;", conn))
                {
                    byRoom.Parameters.AddWithValue("@Room", (object?)room ?? DBNull.Value);
                    var obj = byRoom.ExecuteScalar();
                    if (obj is int id2) return id2;
                }

                // 3) Fallback: Guest + active (less precise, but last resort)
                using (var byGuest = new SqlCommand(@"
                        SELECT TOP 1 ci.CheckInID
                        FROM CheckIns ci
                        INNER JOIN Guests g ON g.GuestID = ci.GuestID
                        WHERE ci.ActualCheckOutDateTime IS NULL
                          AND (@Guest IS NOT NULL AND (g.FirstName + ' ' + g.LastName) = @Guest)
                        ORDER BY ci.CheckInDateTime DESC;", conn))
                {
                    byGuest.Parameters.AddWithValue("@Guest", string.IsNullOrWhiteSpace(guest) ? (object)DBNull.Value : guest);
                    var obj = byGuest.ExecuteScalar();
                    if (obj is int id3) return id3;
                }

                return (int?)null;
            }
            catch
            {
                return (int?)null;
            }
        }

        private static bool SaveNotes(int checkInId, string? newNotes, Func<SqlConnection> getConnection)
        {
            try
            {
                using var conn = getConnection();
                conn.Open();
                using var cmd = new SqlCommand("UPDATE CheckIns SET Notes = @Notes WHERE CheckInID = @Id;", conn);
                if (string.IsNullOrWhiteSpace(newNotes))
                    cmd.Parameters.AddWithValue("@Notes", DBNull.Value);
                else
                    cmd.Parameters.AddWithValue("@Notes", newNotes);
                cmd.Parameters.AddWithValue("@Id", checkInId);
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Save failed: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
    }
}