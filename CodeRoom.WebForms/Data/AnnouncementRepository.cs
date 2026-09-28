using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CodeRoom.WebForms.Models;
using CodeRoom.WebForms.Services;

namespace CodeRoom.WebForms.Data
{
    /// <summary>
    /// ADO.NET access to Announcements. Publishing an announcement also writes a notification for every
    /// learner that accepts them, so the announcement row and its broadcast are exposed as transaction
    /// aware methods that belong to one unit of work.
    /// </summary>
    public class AnnouncementRepository
    {
        private const string Columns = "Id, Title, Message, PublishedAt, IsPublished";

        public List<Announcement> GetPublished(int take = 10)
        {
            const string sql =
                "SELECT TOP (@Take) " + Columns + " FROM dbo.Announcements " +
                "WHERE IsPublished = 1 ORDER BY PublishedAt DESC;";

            return SqlHelper.ReadList(sql, Map, new SqlParameter("@Take", SqlDbType.Int) { Value = take });
        }

        public List<Announcement> GetAll()
        {
            const string sql = "SELECT " + Columns + " FROM dbo.Announcements ORDER BY PublishedAt DESC;";
            return SqlHelper.ReadList(sql, Map);
        }

        public Announcement GetById(int announcementId)
        {
            const string sql = "SELECT " + Columns + " FROM dbo.Announcements WHERE Id = @Id;";

            return SqlHelper.ReadFirst(sql, Map,
                new SqlParameter("@Id", SqlDbType.Int) { Value = announcementId });
        }

        public DataTable GetAllGrid()
        {
            const string sql = "SELECT Id, Title, IsPublished, PublishedAt FROM dbo.Announcements ORDER BY PublishedAt DESC;";
            return SqlHelper.GetTable(sql);
        }

        public int Insert(SqlConnection connection, SqlTransaction transaction, Announcement announcement)
        {
            const string sql =
                "INSERT INTO dbo.Announcements (Title, Message, PublishedAt, IsPublished) " +
                "OUTPUT INSERTED.Id VALUES (@Title, @Message, @PublishedAt, @IsPublished);";

            using (var command = SqlHelper.Prepare(connection, transaction, sql))
            {
                AddParameters(command, announcement);
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        public int Insert(Announcement announcement)
        {
            return SqlHelper.WithTransaction((connection, transaction) => Insert(connection, transaction, announcement));
        }

        public void Update(SqlConnection connection, SqlTransaction transaction, Announcement announcement)
        {
            const string sql =
                "UPDATE dbo.Announcements SET Title = @Title, Message = @Message, " +
                "PublishedAt = @PublishedAt, IsPublished = @IsPublished WHERE Id = @Id;";

            using (var command = SqlHelper.Prepare(connection, transaction, sql))
            {
                AddParameters(command, announcement);
                SqlHelper.AddInt(command, "@Id", announcement.Id);
                command.ExecuteNonQuery();
            }
        }

        public void Update(Announcement announcement)
        {
            SqlHelper.WithTransaction((connection, transaction) => Update(connection, transaction, announcement));
        }

        public void Delete(SqlConnection connection, SqlTransaction transaction, int announcementId)
        {
            const string sql = "DELETE FROM dbo.Announcements WHERE Id = @Id;";

            using (var command = SqlHelper.Prepare(connection, transaction, sql))
            {
                SqlHelper.AddInt(command, "@Id", announcementId);
                command.ExecuteNonQuery();
            }
        }

        public void Delete(int announcementId)
        {
            SqlHelper.WithTransaction((connection, transaction) => Delete(connection, transaction, announcementId));
        }

        /// <summary>
        /// Deliver an announcement to every learner that has notifications switched on. The set based
        /// INSERT keeps this a single round trip whatever the audience size.
        /// </summary>
        public static int Broadcast(SqlConnection connection, SqlTransaction transaction, string title, string message, string linkUrl)
        {
            const string sql =
                "INSERT INTO dbo.Notifications (UserId, Type, Title, Message, LinkUrl, CreatedAt, IsRead) " +
                "SELECT u.Id, @Type, @Title, @Message, @LinkUrl, @CreatedAt, 0 " +
                "FROM dbo.Users AS u " +
                "WHERE u.EmailNotificationsEnabled = 1 " +
                "  AND NOT EXISTS (SELECT 1 FROM dbo.Notifications AS n " +
                "                   WHERE n.UserId = u.Id AND n.Type = @Type AND n.Title = @Title);";

            using (var command = SqlHelper.Prepare(connection, transaction, sql))
            {
                SqlHelper.AddNVarChar(command, "@Type", DomainValues.NotificationType.Announcement, 40);
                SqlHelper.AddNVarChar(command, "@Title", title, 150);
                SqlHelper.AddNVarChar(command, "@Message", message, 500);
                SqlHelper.AddNVarChar(command, "@LinkUrl", linkUrl, 300);
                SqlHelper.AddDateTime(command, "@CreatedAt", DateTime.UtcNow);
                return command.ExecuteNonQuery();
            }
        }

        public int Broadcast(string title, string message, string linkUrl)
        {
            return SqlHelper.WithTransaction((connection, transaction) =>
                Broadcast(connection, transaction, title, message, linkUrl));
        }

        public int CountPublished()
        {
            const string sql = "SELECT COUNT(1) FROM dbo.Announcements WHERE IsPublished = 1;";
            return Convert.ToInt32(SqlHelper.ExecuteScalar(sql));
        }

        private static void AddParameters(SqlCommand command, Announcement announcement)
        {
            SqlHelper.AddNVarChar(command, "@Title", announcement.Title, 150);
            SqlHelper.AddNVarChar(command, "@Message", announcement.Message, 1000);
            SqlHelper.AddDateTime(command, "@PublishedAt",
                announcement.PublishedAt == default(DateTime) ? DateTime.UtcNow : announcement.PublishedAt);
            SqlHelper.AddBool(command, "@IsPublished", announcement.IsPublished);
        }

        internal static Announcement Map(SqlDataReader reader)
        {
            return new Announcement
            {
                Id = SqlHelper.GetInt(reader, "Id"),
                Title = SqlHelper.GetString(reader, "Title"),
                Message = SqlHelper.GetString(reader, "Message"),
                PublishedAt = SqlHelper.GetDateTime(reader, "PublishedAt"),
                IsPublished = SqlHelper.GetBool(reader, "IsPublished")
            };
        }
    }
}
