using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CodeRoom.WebForms.Models;

namespace CodeRoom.WebForms.Data
{
    /// <summary>ADO.NET access to Notifications, the bell feed in the header.</summary>
    public class NotificationRepository
    {
        private const string Columns = "Id, UserId, Type, Title, Message, LinkUrl, CreatedAt, IsRead";

        public List<Notification> GetByUser(int userId, int take = 20)
        {
            const string sql =
                "SELECT TOP (@Take) " + Columns + " FROM dbo.Notifications " +
                "WHERE UserId = @UserId ORDER BY CreatedAt DESC;";

            return SqlHelper.ReadList(sql, Map,
                new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                new SqlParameter("@Take", SqlDbType.Int) { Value = take });
        }

        public List<Notification> GetUnread(int userId, int take = 10)
        {
            const string sql =
                "SELECT TOP (@Take) " + Columns + " FROM dbo.Notifications " +
                "WHERE UserId = @UserId AND IsRead = 0 ORDER BY CreatedAt DESC;";

            return SqlHelper.ReadList(sql, Map,
                new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                new SqlParameter("@Take", SqlDbType.Int) { Value = take });
        }

        public int CountUnread(int userId)
        {
            const string sql = "SELECT COUNT(1) FROM dbo.Notifications WHERE UserId = @UserId AND IsRead = 0;";

            return Convert.ToInt32(SqlHelper.ExecuteScalar(sql,
                new SqlParameter("@UserId", SqlDbType.Int) { Value = userId }));
        }

        public Notification GetById(int notificationId, int userId)
        {
            const string sql = "SELECT " + Columns + " FROM dbo.Notifications WHERE Id = @Id AND UserId = @UserId;";

            return SqlHelper.ReadFirst(sql, Map,
                new SqlParameter("@Id", SqlDbType.Int) { Value = notificationId },
                new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
        }

        /// <summary>Insert inside the caller's transaction, so a reward never leaves a dangling notification.</summary>
        public int Insert(SqlConnection connection, SqlTransaction transaction, Notification notification)
        {
            const string sql =
                "INSERT INTO dbo.Notifications (UserId, Type, Title, Message, LinkUrl, CreatedAt, IsRead) " +
                "OUTPUT INSERTED.Id VALUES (@UserId, @Type, @Title, @Message, @LinkUrl, @CreatedAt, @IsRead);";

            using (var command = SqlHelper.Prepare(connection, transaction, sql))
            {
                AddParameters(command, notification);
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        public int Insert(Notification notification)
        {
            return SqlHelper.WithTransaction((connection, transaction) => Insert(connection, transaction, notification));
        }

        /// <summary>
        /// Duplicate guard used before broadcasting: the same title for the same learner on the same day is
        /// only delivered once.
        /// </summary>
        public bool Exists(int userId, string type, string title, DateTime sinceUtc)
        {
            const string sql =
                "SELECT COUNT(1) FROM dbo.Notifications " +
                "WHERE UserId = @UserId AND Type = @Type AND Title = @Title AND CreatedAt >= @Since;";

            return Convert.ToInt32(SqlHelper.ExecuteScalar(sql,
                new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                new SqlParameter("@Type", SqlDbType.NVarChar, 40) { Value = type },
                new SqlParameter("@Title", SqlDbType.NVarChar, 150) { Value = title },
                new SqlParameter("@Since", SqlDbType.DateTime2) { Value = sinceUtc })) > 0;
        }

        public void MarkRead(int userId, int notificationId)
        {
            const string sql = "UPDATE dbo.Notifications SET IsRead = 1 WHERE Id = @Id AND UserId = @UserId;";

            Execute(userId, notificationId, sql);
        }

        public int MarkAllRead(int userId)
        {
            const string sql = "UPDATE dbo.Notifications SET IsRead = 1 WHERE UserId = @UserId AND IsRead = 0;";

            using (var connection = DbConnectionFactory.Open())
            using (var command = SqlHelper.Prepare(connection, null, sql))
            {
                SqlHelper.AddInt(command, "@UserId", userId);
                return command.ExecuteNonQuery();
            }
        }

        /// <summary>Clear broadcast notifications when an announcement is unpublished.</summary>
        public static int DeleteByTypeAndTitle(SqlConnection connection, SqlTransaction transaction, string type, string title)
        {
            const string sql = "DELETE FROM dbo.Notifications WHERE Type = @Type AND Title = @Title;";

            using (var command = SqlHelper.Prepare(connection, transaction, sql))
            {
                SqlHelper.AddNVarChar(command, "@Type", type, 40);
                SqlHelper.AddNVarChar(command, "@Title", title, 150);
                return command.ExecuteNonQuery();
            }
        }

        private static void AddParameters(SqlCommand command, Notification notification)
        {
            SqlHelper.AddInt(command, "@UserId", notification.UserId);
            SqlHelper.AddNVarChar(command, "@Type", notification.Type, 40);
            SqlHelper.AddNVarChar(command, "@Title", notification.Title, 150);
            SqlHelper.AddNVarChar(command, "@Message", notification.Message, 500);
            SqlHelper.AddNVarChar(command, "@LinkUrl", notification.LinkUrl, 300);
            SqlHelper.AddDateTime(command, "@CreatedAt",
                notification.CreatedAt == default(DateTime) ? DateTime.UtcNow : notification.CreatedAt);
            SqlHelper.AddBool(command, "@IsRead", notification.IsRead);
        }

        private void Execute(int userId, int notificationId, string sql)
        {
            using (var connection = DbConnectionFactory.Open())
            using (var command = SqlHelper.Prepare(connection, null, sql))
            {
                SqlHelper.AddInt(command, "@Id", notificationId);
                SqlHelper.AddInt(command, "@UserId", userId);
                command.ExecuteNonQuery();
            }
        }

        internal static Notification Map(SqlDataReader reader)
        {
            return new Notification
            {
                Id = SqlHelper.GetInt(reader, "Id"),
                UserId = SqlHelper.GetInt(reader, "UserId"),
                Type = SqlHelper.GetString(reader, "Type"),
                Title = SqlHelper.GetString(reader, "Title"),
                Message = SqlHelper.GetString(reader, "Message"),
                LinkUrl = SqlHelper.GetNullableString(reader, "LinkUrl"),
                CreatedAt = SqlHelper.GetDateTime(reader, "CreatedAt"),
                IsRead = SqlHelper.GetBool(reader, "IsRead")
            };
        }
    }
}
