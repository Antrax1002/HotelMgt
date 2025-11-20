using Microsoft.Data.SqlClient;
using System;

namespace HotelMgt.Services
{
    public class ActivityLogService
    {
        private readonly DatabaseService _dbService;

        public ActivityLogService()
        {
            _dbService = new DatabaseService();
        }

        // Keeps the same signature used across the app
        public void LogActivity(int employeeId, string activityType, string description, int? relatedEntityId = null)
        {
            try
            {
                using (var conn = _dbService.GetConnection())
                {
                    conn.Open();
                    // Align with ActivityLog schema: EmployeeID, ActivityType, ActivityDescription, RelatedEntityID, ActivityDateTime
                    string query = @"
                        INSERT INTO ActivityLog (EmployeeID, ActivityType, ActivityDescription, RelatedEntityID, ActivityDateTime)
                        VALUES (@EmployeeID, @ActivityType, @ActivityDescription, @RelatedEntityID, @ActivityDateTime)";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@EmployeeID", employeeId);
                        cmd.Parameters.AddWithValue("@ActivityType", activityType);
                        cmd.Parameters.AddWithValue("@ActivityDescription", description);
                        cmd.Parameters.AddWithValue("@RelatedEntityID",
                            relatedEntityId.HasValue ? (object)relatedEntityId.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@ActivityDateTime", DateTime.Now);

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                // Do not throw - activity logging shouldn't break the app
                Console.WriteLine($"Activity log error: {ex.Message}");
            }
        }
    }
}
