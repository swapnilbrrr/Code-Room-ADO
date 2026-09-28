using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CodeRoom.WebForms.Models;

namespace CodeRoom.WebForms.Data
{
    /// <summary>
    /// ADO.NET access to the administration side of the data: the audit trail, the dashboard counters and
    /// the disconnected tables the admin grids bind to.
    /// </summary>
    public class AdminRepository
    {
        /// <summary>
        /// Write an audit entry. The source application records the entity change first and the audit row
        /// in a separate step, so this method takes a transaction when the caller wants both to commit
        /// together and runs standalone otherwise.
        /// </summary>
        public int Record(SqlConnection connection, SqlTransaction transaction, AdminAuditLog entry)
        {
            const string sql =
                "INSERT INTO dbo.AdminAuditLogs (UserId, [Action], EntityType, EntityName, Description, CreatedAt) " +
                "OUTPUT INSERTED.Id VALUES (@UserId, @Action, @EntityType, @EntityName, @Description, @CreatedAt);";

            using (var command = SqlHelper.Prepare(connection, transaction, sql))
            {
                AddParameters(command, entry);
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        public int Record(AdminAuditLog entry)
        {
            return SqlHelper.WithTransaction((connection, transaction) => Record(connection, transaction, entry));
        }

        public List<AdminAuditLog> GetRecent(int take = 100)
        {
            const string sql =
                "SELECT TOP (@Take) a.Id, a.UserId, a.[Action], a.EntityType, a.EntityName, a.Description, " +
                "       a.CreatedAt, u.FullName, u.Username " +
                "FROM dbo.AdminAuditLogs AS a " +
                "INNER JOIN dbo.Users AS u ON u.Id = a.UserId " +
                "ORDER BY a.CreatedAt DESC;";

            return SqlHelper.ReadList(sql, Map, new SqlParameter("@Take", SqlDbType.Int) { Value = take });
        }

        public DataTable GetAuditGrid(int take = 100)
        {
            const string sql =
                "SELECT TOP (@Take) a.Id, a.[Action], a.EntityType, a.EntityName, a.Description, " +
                "       a.CreatedAt, u.Username AS Admin " +
                "FROM dbo.AdminAuditLogs AS a " +
                "INNER JOIN dbo.Users AS u ON u.Id = a.UserId " +
                "ORDER BY a.CreatedAt DESC;";

            return SqlHelper.GetTable(sql, new SqlParameter("@Take", SqlDbType.Int) { Value = take });
        }

        public int CountAuditEntries()
        {
            return Convert.ToInt32(SqlHelper.ExecuteScalar("SELECT COUNT(1) FROM dbo.AdminAuditLogs;"));
        }

        /// <summary>The counters the admin dashboard shows, collected in a single round trip.</summary>
        public DataTable GetDashboardCounts()
        {
            const string sql =
                "SELECT (SELECT COUNT(1) FROM dbo.Users) AS Users, " +
                "       (SELECT COUNT(1) FROM dbo.Users WHERE Role <> N'Student') AS Administrators, " +
                "       (SELECT COUNT(1) FROM dbo.Courses) AS Courses, " +
                "       (SELECT COUNT(1) FROM dbo.Lessons) AS Lessons, " +
                "       (SELECT COUNT(1) FROM dbo.Quizzes) AS Quizzes, " +
                "       (SELECT COUNT(1) FROM dbo.Challenges) AS Challenges, " +
                "       (SELECT COUNT(1) FROM dbo.Resources) AS Resources, " +
                "       (SELECT COUNT(1) FROM dbo.Enrollments) AS Enrollments, " +
                "       (SELECT COUNT(1) FROM dbo.QuizAttempts) AS Attempts, " +
                "       (SELECT COUNT(1) FROM dbo.Certificates) AS Certificates, " +
                "       (SELECT COUNT(1) FROM dbo.Announcements) AS Announcements;";

            return SqlHelper.GetTable(sql);
        }

        /// <summary>Newest registrations, shown on the administration overview.</summary>
        public List<User> GetRecentUsers(int take = 5)
        {
            const string sql =
                "SELECT TOP (@Take) Id, FullName, Username, Email, PasswordHash, Bio, AvatarUrl, Role, Xp, " +
                "ThemePreference, ProfileVisibility, EmailNotificationsEnabled, CreatedAt " +
                "FROM dbo.Users ORDER BY CreatedAt DESC;";

            return SqlHelper.ReadList(sql, UserRepository.Map,
                new SqlParameter("@Take", SqlDbType.Int) { Value = take });
        }

        /// <summary>Delete an account together with everything it owns, in one transaction.</summary>
        public void DeleteUser(int userId)
        {
            SqlHelper.WithTransaction((connection, transaction) =>
                new UserRepository().Delete(connection, transaction, userId));
        }

        private static void AddParameters(SqlCommand command, AdminAuditLog entry)
        {
            SqlHelper.AddInt(command, "@UserId", entry.UserId);
            SqlHelper.AddNVarChar(command, "@Action", entry.Action, 60);
            SqlHelper.AddNVarChar(command, "@EntityType", entry.EntityType, 80);
            SqlHelper.AddNVarChar(command, "@EntityName", entry.EntityName, 120);
            SqlHelper.AddNVarChar(command, "@Description", entry.Description, 500);
            SqlHelper.AddDateTime(command, "@CreatedAt",
                entry.CreatedAt == default(DateTime) ? DateTime.UtcNow : entry.CreatedAt);
        }

        internal static AdminAuditLog Map(SqlDataReader reader)
        {
            return new AdminAuditLog
            {
                Id = SqlHelper.GetInt(reader, "Id"),
                UserId = SqlHelper.GetInt(reader, "UserId"),
                Action = SqlHelper.GetString(reader, "Action"),
                EntityType = SqlHelper.GetString(reader, "EntityType"),
                EntityName = SqlHelper.GetNullableString(reader, "EntityName"),
                Description = SqlHelper.GetString(reader, "Description"),
                CreatedAt = SqlHelper.GetDateTime(reader, "CreatedAt")
            };
        }
    }
}
