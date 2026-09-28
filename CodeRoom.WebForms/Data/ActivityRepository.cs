using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CodeRoom.WebForms.Models;

namespace CodeRoom.WebForms.Data
{
    /// <summary>
    /// ADO.NET access to UserActivities, the feed that also drives the streak calculation.
    /// Reads are limited to the window the dashboard uses instead of loading a whole history.
    /// </summary>
    public class ActivityRepository
    {
        public static int Insert(SqlConnection connection, SqlTransaction transaction, UserActivity activity)
        {
            const string sql =
                "INSERT INTO dbo.UserActivities (UserId, ActivityType, Description, CreatedAt) " +
                "OUTPUT INSERTED.Id VALUES (@UserId, @ActivityType, @Description, @CreatedAt);";

            using (var command = SqlHelper.Prepare(connection, transaction, sql))
            {
                SqlHelper.AddInt(command, "@UserId", activity.UserId);
                SqlHelper.AddNVarChar(command, "@ActivityType", activity.ActivityType, 40);
                SqlHelper.AddNVarChar(command, "@Description", activity.Description, 220);
                SqlHelper.AddDateTime(command, "@CreatedAt",
                    activity.CreatedAt == default(DateTime) ? DateTime.UtcNow : activity.CreatedAt);

                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        public int Insert(UserActivity activity)
        {
            return SqlHelper.WithTransaction((connection, transaction) => Insert(connection, transaction, activity));
        }

        /// <summary>Feed entries of the last year, newest first.</summary>
        public List<UserActivity> GetFeed(int userId, int days = 364, int take = 50)
        {
            const string sql =
                "SELECT TOP (@Take) Id, UserId, ActivityType, Description, CreatedAt " +
                "FROM dbo.UserActivities " +
                "WHERE UserId = @UserId AND CreatedAt >= @Since " +
                "ORDER BY CreatedAt DESC;";

            return SqlHelper.ReadList(sql, Map,
                new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                new SqlParameter("@Since", SqlDbType.DateTime2) { Value = DateTime.UtcNow.AddDays(-days) },
                new SqlParameter("@Take", SqlDbType.Int) { Value = take });
        }

        /// <summary>
        /// The recent window used by the streak calculation. Only the type and the date are needed, so the
        /// description stays out of the result set.
        /// </summary>
        public List<UserActivity> GetRecentForStreak(int userId, int take = 400)
        {
            const string sql =
                "SELECT TOP (@Take) Id, UserId, ActivityType, Description, CreatedAt " +
                "FROM dbo.UserActivities WHERE UserId = @UserId ORDER BY CreatedAt DESC;";

            return SqlHelper.ReadList(sql, Map,
                new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                new SqlParameter("@Take", SqlDbType.Int) { Value = take });
        }

        public int CountByUser(int userId)
        {
            const string sql = "SELECT COUNT(1) FROM dbo.UserActivities WHERE UserId = @UserId;";

            return Convert.ToInt32(SqlHelper.ExecuteScalar(sql,
                new SqlParameter("@UserId", SqlDbType.Int) { Value = userId }));
        }

        public int CountForDay(int userId, DateTime dayUtc)
        {
            const string sql =
                "SELECT COUNT(1) FROM dbo.UserActivities " +
                "WHERE UserId = @UserId AND CreatedAt >= @Start AND CreatedAt < @End;";

            return Convert.ToInt32(SqlHelper.ExecuteScalar(sql,
                new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                new SqlParameter("@Start", SqlDbType.DateTime2) { Value = dayUtc.Date },
                new SqlParameter("@End", SqlDbType.DateTime2) { Value = dayUtc.Date.AddDays(1) }));
        }

        internal static UserActivity Map(SqlDataReader reader)
        {
            return new UserActivity
            {
                Id = SqlHelper.GetInt(reader, "Id"),
                UserId = SqlHelper.GetInt(reader, "UserId"),
                ActivityType = SqlHelper.GetString(reader, "ActivityType"),
                Description = SqlHelper.GetString(reader, "Description"),
                CreatedAt = SqlHelper.GetDateTime(reader, "CreatedAt")
            };
        }
    }
}
