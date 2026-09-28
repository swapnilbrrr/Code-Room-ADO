using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CodeRoom.WebForms.Models;

namespace CodeRoom.WebForms.Data
{
    /// <summary>ADO.NET access to Lessons, including the module assignment the course builder uses.</summary>
    public class LessonRepository
    {
        private const string Columns =
            "Id, CourseId, CourseModuleId, Title, Summary, Content, ContentType, VideoUrl, AudioUrl, " +
            "ResourceUrl, [Order], DurationMinutes, IsPublished";

        /// <summary>Lessons of a course in module and lesson order, without the heavy Content column.</summary>
        public static List<Lesson> GetByCourse(int courseId)
        {
            const string sql =
                "SELECT Id, CourseId, CourseModuleId, Title, Summary, ContentType, VideoUrl, AudioUrl, " +
                "ResourceUrl, [Order], DurationMinutes, IsPublished " +
                "FROM dbo.Lessons WHERE CourseId = @CourseId ORDER BY [Order];";

            return SqlHelper.ReadList(sql, MapSummary,
                new SqlParameter("@CourseId", SqlDbType.Int) { Value = courseId });
        }

        public static List<Lesson> GetByModule(int moduleId)
        {
            const string sql =
                "SELECT Id, CourseId, CourseModuleId, Title, Summary, ContentType, VideoUrl, AudioUrl, " +
                "ResourceUrl, [Order], DurationMinutes, IsPublished " +
                "FROM dbo.Lessons WHERE CourseModuleId = @ModuleId ORDER BY [Order];";

            return SqlHelper.ReadList(sql, MapSummary,
                new SqlParameter("@ModuleId", SqlDbType.Int) { Value = moduleId });
        }

        /// <summary>A single lesson with its full content, as the lesson page renders it.</summary>
        public Lesson GetDetailedById(int lessonId)
        {
            const string sql = "SELECT " + Columns + " FROM dbo.Lessons WHERE Id = @LessonId;";

            return SqlHelper.ReadFirst(sql, Map, new SqlParameter("@LessonId", SqlDbType.Int) { Value = lessonId });
        }

        /// <summary>The lesson a learner should open next: the first one they have not completed.</summary>
        public Lesson GetFirstIncomplete(int courseId, int userId)
        {
            const string sql =
                "SELECT TOP (1) l.Id, l.CourseId, l.CourseModuleId, l.Title, l.Summary, l.Content, " +
                "       l.ContentType, l.VideoUrl, l.AudioUrl, l.ResourceUrl, l.[Order], " +
                "       l.DurationMinutes, l.IsPublished " +
                "FROM dbo.Lessons AS l " +
                "WHERE l.CourseId = @CourseId " +
                "  AND NOT EXISTS (SELECT 1 FROM dbo.Progress AS p " +
                "                   WHERE p.LessonId = l.Id AND p.UserId = @UserId AND p.IsCompleted = 1) " +
                "ORDER BY l.[Order];";

            return SqlHelper.ReadFirst(sql, Map,
                new SqlParameter("@CourseId", SqlDbType.Int) { Value = courseId },
                new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
        }

        public List<Lesson> GetAllByCourse(int courseId)
        {
            const string sql = "SELECT " + Columns + " FROM dbo.Lessons WHERE CourseId = @CourseId ORDER BY [Order];";

            return SqlHelper.ReadList(sql, Map,
                new SqlParameter("@CourseId", SqlDbType.Int) { Value = courseId });
        }

        /// <summary>Next free position in the course, used when an administrator adds a lesson.</summary>
        public static int NextOrder(SqlConnection connection, SqlTransaction transaction, int courseId)
        {
            const string sql = "SELECT ISNULL(MAX([Order]), 0) + 1 FROM dbo.Lessons WHERE CourseId = @CourseId;";

            using (var command = SqlHelper.Prepare(connection, transaction, sql))
            {
                SqlHelper.AddInt(command, "@CourseId", courseId);
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        /// <summary>
        /// Insert a lesson inside the caller's transaction. The admin flow that adds a lesson can also
        /// create the module it belongs to in the same unit of work, so the id comes back from here.
        /// </summary>
        public static int Insert(SqlConnection connection, SqlTransaction transaction, Lesson lesson)
        {
            const string sql =
                "INSERT INTO dbo.Lessons " +
                "(CourseId, CourseModuleId, Title, Summary, Content, ContentType, VideoUrl, AudioUrl, " +
                " ResourceUrl, [Order], DurationMinutes, IsPublished) " +
                "OUTPUT INSERTED.Id VALUES " +
                "(@CourseId, @CourseModuleId, @Title, @Summary, @Content, @ContentType, @VideoUrl, @AudioUrl, " +
                " @ResourceUrl, @Order, @DurationMinutes, @IsPublished);";

            using (var command = SqlHelper.Prepare(connection, transaction, sql))
            {
                AddParameters(command, lesson);
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        public int Insert(Lesson lesson)
        {
            return SqlHelper.WithTransaction((connection, transaction) => Insert(connection, transaction, lesson));
        }

        public static void Update(SqlConnection connection, SqlTransaction transaction, Lesson lesson)
        {
            const string sql =
                "UPDATE dbo.Lessons SET CourseId = @CourseId, CourseModuleId = @CourseModuleId, " +
                "Title = @Title, Summary = @Summary, Content = @Content, ContentType = @ContentType, " +
                "VideoUrl = @VideoUrl, AudioUrl = @AudioUrl, ResourceUrl = @ResourceUrl, " +
                "[Order] = @Order, DurationMinutes = @DurationMinutes, IsPublished = @IsPublished " +
                "WHERE Id = @Id;";

            using (var command = SqlHelper.Prepare(connection, transaction, sql))
            {
                AddParameters(command, lesson);
                SqlHelper.AddInt(command, "@Id", lesson.Id);
                command.ExecuteNonQuery();
            }
        }

        public void Update(Lesson lesson)
        {
            SqlHelper.WithTransaction((connection, transaction) => Update(connection, transaction, lesson));
        }

        /// <summary>
        /// Delete a lesson. Challenges keep their row but lose the reference (the original SET NULL
        /// relationship, which SQL Server cannot declare alongside the course cascade), and the learner
        /// progress rows are removed before the lesson itself.
        /// </summary>
        public void Delete(SqlConnection connection, SqlTransaction transaction, int lessonId)
        {
            Execute(connection, transaction,
                "UPDATE dbo.Challenges SET LessonId = NULL WHERE LessonId = @LessonId;", lessonId);

            Execute(connection, transaction,
                "DELETE FROM dbo.Progress WHERE LessonId = @LessonId;", lessonId);

            Execute(connection, transaction,
                "DELETE FROM dbo.Lessons WHERE Id = @LessonId;", lessonId);
        }

        public void Delete(int lessonId)
        {
            SqlHelper.WithTransaction((connection, transaction) => Delete(connection, transaction, lessonId));
        }

        /// <summary>Assignment table used by the course builder grid.</summary>
        public DataTable GetCourseGrid(int courseId)
        {
            const string sql =
                "SELECT l.Id, l.Title, l.ContentType, l.[Order], l.DurationMinutes, l.IsPublished, " +
                "       l.CourseModuleId, m.Title AS ModuleTitle " +
                "FROM dbo.Lessons AS l " +
                "LEFT JOIN dbo.CourseModules AS m ON m.Id = l.CourseModuleId " +
                "WHERE l.CourseId = @CourseId " +
                "ORDER BY l.[Order];";

            return SqlHelper.GetTable(sql, new SqlParameter("@CourseId", SqlDbType.Int) { Value = courseId });
        }

        public static int CountByCourse(int courseId)
        {
            const string sql = "SELECT COUNT(1) FROM dbo.Lessons WHERE CourseId = @CourseId;";

            return Convert.ToInt32(SqlHelper.ExecuteScalar(sql,
                new SqlParameter("@CourseId", SqlDbType.Int) { Value = courseId }));
        }

        public static int CountAll()
        {
            return Convert.ToInt32(SqlHelper.ExecuteScalar("SELECT COUNT(1) FROM dbo.Lessons;"));
        }

        private static void Execute(SqlConnection connection, SqlTransaction transaction, string sql, int lessonId)
        {
            using (var command = SqlHelper.Prepare(connection, transaction, sql))
            {
                SqlHelper.AddInt(command, "@LessonId", lessonId);
                command.ExecuteNonQuery();
            }
        }

        private static void AddParameters(SqlCommand command, Lesson lesson)
        {
            SqlHelper.AddInt(command, "@CourseId", lesson.CourseId);
            SqlHelper.Add(command, "@CourseModuleId", SqlDbType.Int, SqlHelper.Value(lesson.CourseModuleId));
            SqlHelper.AddNVarChar(command, "@Title", lesson.Title, 150);
            SqlHelper.AddNVarChar(command, "@Summary", lesson.Summary, 180);
            SqlHelper.AddText(command, "@Content", lesson.Content);
            SqlHelper.AddNVarChar(command, "@ContentType", lesson.ContentType, 30);
            SqlHelper.AddNVarChar(command, "@VideoUrl", lesson.VideoUrl, 300);
            SqlHelper.AddNVarChar(command, "@AudioUrl", lesson.AudioUrl, 300);
            SqlHelper.AddNVarChar(command, "@ResourceUrl", lesson.ResourceUrl, 300);
            SqlHelper.AddInt(command, "@Order", lesson.Order);
            SqlHelper.AddInt(command, "@DurationMinutes", lesson.DurationMinutes);
            SqlHelper.AddBool(command, "@IsPublished", lesson.IsPublished);
        }

        internal static Lesson Map(SqlDataReader reader)
        {
            return new Lesson
            {
                Id = SqlHelper.GetInt(reader, "Id"),
                CourseId = SqlHelper.GetInt(reader, "CourseId"),
                CourseModuleId = SqlHelper.GetNullableInt(reader, "CourseModuleId"),
                Title = SqlHelper.GetString(reader, "Title"),
                Summary = SqlHelper.GetNullableString(reader, "Summary"),
                Content = SqlHelper.GetString(reader, "Content"),
                ContentType = SqlHelper.GetString(reader, "ContentType"),
                VideoUrl = SqlHelper.GetNullableString(reader, "VideoUrl"),
                AudioUrl = SqlHelper.GetNullableString(reader, "AudioUrl"),
                ResourceUrl = SqlHelper.GetNullableString(reader, "ResourceUrl"),
                Order = SqlHelper.GetInt(reader, "Order"),
                DurationMinutes = SqlHelper.GetInt(reader, "DurationMinutes"),
                IsPublished = SqlHelper.GetBool(reader, "IsPublished")
            };
        }

        /// <summary>Mapping for the reduced column list used by course outlines and grids.</summary>
        internal static Lesson MapSummary(SqlDataReader reader)
        {
            return new Lesson
            {
                Id = SqlHelper.GetInt(reader, "Id"),
                CourseId = SqlHelper.GetInt(reader, "CourseId"),
                CourseModuleId = SqlHelper.GetNullableInt(reader, "CourseModuleId"),
                Title = SqlHelper.GetString(reader, "Title"),
                Summary = SqlHelper.GetNullableString(reader, "Summary"),
                ContentType = SqlHelper.GetString(reader, "ContentType"),
                VideoUrl = SqlHelper.GetNullableString(reader, "VideoUrl"),
                AudioUrl = SqlHelper.GetNullableString(reader, "AudioUrl"),
                ResourceUrl = SqlHelper.GetNullableString(reader, "ResourceUrl"),
                Order = SqlHelper.GetInt(reader, "Order"),
                DurationMinutes = SqlHelper.GetInt(reader, "DurationMinutes"),
                IsPublished = SqlHelper.GetBool(reader, "IsPublished")
            };
        }
    }
}
