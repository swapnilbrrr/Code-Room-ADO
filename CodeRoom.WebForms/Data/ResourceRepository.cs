using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CodeRoom.WebForms.Models;

namespace CodeRoom.WebForms.Data
{
    /// <summary>
    /// ADO.NET access to Resources. A resource either belongs to a course or, when CourseId is NULL,
    /// is shown on the platform wide resources page.
    /// </summary>
    public class ResourceRepository
    {
        private const string Columns = "Id, CourseId, Title, Url, Type";

        public List<Resource> GetByCourse(int courseId, int take = 8)
        {
            const string sql =
                "SELECT TOP (@Take) " + Columns + " FROM dbo.Resources " +
                "WHERE CourseId = @CourseId ORDER BY Title;";

            return SqlHelper.ReadList(sql, Map,
                new SqlParameter("@CourseId", SqlDbType.Int) { Value = courseId },
                new SqlParameter("@Take", SqlDbType.Int) { Value = take });
        }

        public List<Resource> GetGlobal(int take = 20)
        {
            const string sql =
                "SELECT TOP (@Take) " + Columns + " FROM dbo.Resources " +
                "WHERE CourseId IS NULL ORDER BY Id;";

            return SqlHelper.ReadList(sql, Map, new SqlParameter("@Take", SqlDbType.Int) { Value = take });
        }

        public List<Resource> GetAll()
        {
            const string sql = "SELECT " + Columns + " FROM dbo.Resources ORDER BY CourseId, Title;";
            return SqlHelper.ReadList(sql, Map);
        }

        public Resource GetById(int resourceId)
        {
            const string sql = "SELECT " + Columns + " FROM dbo.Resources WHERE Id = @Id;";

            return SqlHelper.ReadFirst(sql, Map, new SqlParameter("@Id", SqlDbType.Int) { Value = resourceId });
        }

        /// <summary>Seeding guard: a URL is only stored once, wherever it is referenced from.</summary>
        public bool UrlExists(string url)
        {
            const string sql = "SELECT COUNT(1) FROM dbo.Resources WHERE Url = @Url;";

            return SqlHelper.Exists(sql,
                new SqlParameter("@Url", SqlDbType.NVarChar, 400) { Value = url });
        }

        public DataTable GetAllGrid()
        {
            const string sql =
                "SELECT r.Id, r.Title, r.Url, r.Type, c.Title AS CourseTitle " +
                "FROM dbo.Resources AS r " +
                "LEFT JOIN dbo.Courses AS c ON c.Id = r.CourseId " +
                "ORDER BY c.Title, r.Title;";

            return SqlHelper.GetTable(sql);
        }

        public int Insert(SqlConnection connection, SqlTransaction transaction, Resource resource)
        {
            const string sql =
                "INSERT INTO dbo.Resources (CourseId, Title, Url, Type) " +
                "OUTPUT INSERTED.Id VALUES (@CourseId, @Title, @Url, @Type);";

            using (var command = SqlHelper.Prepare(connection, transaction, sql))
            {
                AddParameters(command, resource);
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        public int Insert(Resource resource)
        {
            return SqlHelper.WithTransaction((connection, transaction) => Insert(connection, transaction, resource));
        }

        public void Update(SqlConnection connection, SqlTransaction transaction, Resource resource)
        {
            const string sql =
                "UPDATE dbo.Resources SET CourseId = @CourseId, Title = @Title, Url = @Url, Type = @Type " +
                "WHERE Id = @Id;";

            using (var command = SqlHelper.Prepare(connection, transaction, sql))
            {
                AddParameters(command, resource);
                SqlHelper.AddInt(command, "@Id", resource.Id);
                command.ExecuteNonQuery();
            }
        }

        public void Update(Resource resource)
        {
            SqlHelper.WithTransaction((connection, transaction) => Update(connection, transaction, resource));
        }

        public void Delete(SqlConnection connection, SqlTransaction transaction, int resourceId)
        {
            const string sql = "DELETE FROM dbo.Resources WHERE Id = @Id;";

            using (var command = SqlHelper.Prepare(connection, transaction, sql))
            {
                SqlHelper.AddInt(command, "@Id", resourceId);
                command.ExecuteNonQuery();
            }
        }

        public void Delete(int resourceId)
        {
            SqlHelper.WithTransaction((connection, transaction) => Delete(connection, transaction, resourceId));
        }

        public int CountAll()
        {
            return System.Convert.ToInt32(SqlHelper.ExecuteScalar("SELECT COUNT(1) FROM dbo.Resources;"));
        }

        private static void AddParameters(SqlCommand command, Resource resource)
        {
            SqlHelper.Add(command, "@CourseId", SqlDbType.Int, SqlHelper.Value(resource.CourseId));
            SqlHelper.AddNVarChar(command, "@Title", resource.Title, 150);
            SqlHelper.AddNVarChar(command, "@Url", resource.Url, 400);
            SqlHelper.AddNVarChar(command, "@Type", resource.Type, 40);
        }

        internal static Resource Map(SqlDataReader reader)
        {
            return new Resource
            {
                Id = SqlHelper.GetInt(reader, "Id"),
                CourseId = SqlHelper.GetNullableInt(reader, "CourseId"),
                Title = SqlHelper.GetString(reader, "Title"),
                Url = SqlHelper.GetString(reader, "Url"),
                Type = SqlHelper.GetString(reader, "Type")
            };
        }
    }
}
