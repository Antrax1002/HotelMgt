using System;
using System.Data;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace HotelMgt.Utilities
{
    public static class GuestLookupHelper
    {
        public static GuestLookupResult LookupOrPromptGuest(
            IWin32Window owner,
            SqlConnection conn,
            SqlTransaction transaction,
            string firstName,
            string middleName,
            string lastName,
            Action<string, string, string, string, string, string, string>? onGuestReview = null)
        {
            var result = new GuestLookupResult();
            bool shouldAbort = false;
            bool shouldRollback = false;

            using (var find = new SqlCommand(@"
                SELECT TOP 1 GuestID, Email, PhoneNumber, IDType, IDNumber
                FROM Guests
                WHERE LOWER(FirstName) = LOWER(@FirstName)
                  AND LOWER(ISNULL(MiddleName, '')) = LOWER(ISNULL(@MiddleName, ''))
                  AND LOWER(LastName) = LOWER(@LastName);", conn, transaction))
            {
                find.Parameters.AddWithValue("@FirstName", firstName);
                find.Parameters.AddWithValue("@MiddleName", string.IsNullOrWhiteSpace(middleName) ? "" : middleName);
                find.Parameters.AddWithValue("@LastName", lastName);

                using var reader = find.ExecuteReader();
                if (reader.Read())
                {
                    result.GuestId = reader.GetInt32(0);
                    string foundEmail = reader.IsDBNull(1) ? "" : reader.GetString(1);
                    string foundPhone = reader.IsDBNull(2) ? "" : reader.GetString(2);
                    string foundIDType = reader.IsDBNull(3) ? "" : reader.GetString(3);
                    string foundIDNumber = reader.IsDBNull(4) ? "" : reader.GetString(4);

                    result.IsExistingGuest = true;

                    // Show guest details form
                    var detailsForm = new HotelMgt.Forms.GuestDetailsForm(
                        firstName, middleName, lastName, foundEmail, foundPhone, foundIDType, foundIDNumber);

                    if (detailsForm.ShowDialog(owner) == DialogResult.OK && detailsForm.IsConfirmed)
                    {
                        onGuestReview?.Invoke(
                            detailsForm.FirstName,
                            detailsForm.MiddleName,
                            detailsForm.LastName,
                            detailsForm.Email,
                            detailsForm.Phone,
                            detailsForm.IDType,
                            detailsForm.IDNumber
                        );
                        shouldAbort = false;      // <-- Only abort if not confirmed
                        shouldRollback = false;
                    }
                    else
                    {
                        shouldAbort = true;
                        shouldRollback = true;
                    }
                }
            }
            // Now, after the reader is disposed, you can safely rollback if needed
            if (shouldRollback)
            {
                transaction.Rollback();
            }
            result.AbortCheckIn = shouldAbort;
            return result;
        }
    }
}