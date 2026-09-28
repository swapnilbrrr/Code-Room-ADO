using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CodeRoom.WebForms.Models;

namespace CodeRoom.WebForms.Data
{
    /// <summary>
    /// ADO.NET access to Courses and to the aggregate the catalogue and course pages need
    /// (modules, lessons, challenges, resources and the course quiz).
    /// </summary>
    public class CourseRepository
    {
        private const string Columns =
            "Id, Title, Slug, Description, Category, Level, EstimatedMinutes, IsCertification, " +
            "CertificateName, PassingScorePercent, ThumbnailUrl, IsPublished, CreatedAt";

        // ------------------------------------------------------------------ reads --

        public List<Course> GetPublished()
        {
            const string sql = "SELECT " + Columns + " FROM dbo.Courses WHERE IsPublished = 1 ORDER BY Title;";
            var courses = SqlHelper.ReadList(sql, Map);

            foreach (var course in courses)
            {
                course.Lessons = LessonRepository.GetByCourse(course.Id);
            }

            return courses;
        }

        public List<Course> GetAll()
        {
            const string sql = "SELECT " + Columns + " FROM dbo.Courses ORDER BY Title;";
            return SqlHelper.ReadList(sql, Map);
        }

        public Course GetBySlug(string slug)
        {
            const string sql = "SELECT " + Columns + " FROM dbo.Courses WHERE Slug = @Slug;";

            return SqlHelper.ReadFirst(sql, Map,
                new SqlParameter("@Slug", SqlDbType.NVarChar, 140) { Value = slug });
        }

        public Course GetById(int courseId)
        {
            const string sql = "SELECT " + Columns + " FROM dbo.Courses WHERE Id = @CourseId;";

            return SqlHelper.ReadFirst(sql, Map,
                new SqlParameter("@CourseId", SqlDbType.Int) { Value = courseId });
        }

        /// <summary>The course with its modules, lessons and challenges loaded, as the detail page needs.</summary>
        public Course GetDetails(int courseId)
        {
            var course = GetById(courseId);

            if (course == null)
            {
                return null;
            }

            course.Modules = new CourseModuleRepository().GetCourseOutline(courseId);
            course.Lessons = LessonRepository.GetByCourse(courseId);
            course.Challenges = ChallengeRepository.GetByCourse(courseId);

            return course;
        }

        /// <summary>Courses the learner is enrolled in, each with its lessons attached.</summary>
        public List<Course> GetEnrolledCourses(int userId)
        {
            const string sql =
                "SELECT c.Id, c.Title, c.Slug, c.Description, c.Category, c.Level, c.EstimatedMinutes, " +
                "       c.IsCertification, c.CertificateName, c.PassingScorePercent, c.ThumbnailUrl, " +
                "       c.IsPublished, c.CreatedAt " +
                "FROM dbo.Courses AS c " +
                "INNER JOIN dbo.Enrollments AS e ON e.CourseId = c.Id " +
                "WHERE e.UserId = @UserId " +
                "ORDER BY e.EnrolledAt DESC;";

            var courses = SqlHelper.ReadList(sql, Map,
                new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });

            foreach (var course in courses)
            {
                course.Lessons = LessonRepository.GetByCourse(course.Id);
            }

            return courses;
        }

        public List<Course> GetPublishedExceptEnrolled(int userId)
        {
            const string sql =
                "SELECT " + Columns + " FROM dbo.Courses AS c " +
                "WHERE c.IsPublished = 1 " +
                "  AND NOT EXISTS (SELECT 1 FROM dbo.Enrollments AS e WHERE e.CourseId = c.Id AND e.UserId = @UserId) " +
                "ORDER BY c.Title;";

            return SqlHelper.ReadList(sql, Map, new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
        }

        /// <summary>Id + Title pairs for the administration drop downs.</summary>
        public DataTable GetOptionsTable()
        {
            const string sql = "SELECT Id, Title FROM dbo.Courses ORDER BY Title;";
            return SqlHelper.GetTable(sql);
        }

        public List<KeyValuePair<int, string>> GetIdTitleMap()
        {
            const string sql = "SELECT Id, Title FROM dbo.Courses ORDER BY Title;";

            return SqlHelper.ReadList(sql, reader => new KeyValuePair<int, string>(
                SqlHelper.GetInt(reader, "Id"),
                SqlHelper.GetString(reader, "Title")));
        }

        public bool SlugExists(string slug, int? excludeCourseId = null)
        {
            const string sql =
                "SELECT COUNT(1) FROM dbo.Courses WHERE Slug = @Slug " +
                "AND (@ExcludeId IS NULL OR Id <> @ExcludeId);";

            return SqlHelper.Exists(sql,
                new SqlParameter("@Slug", SqlDbType.NVarChar, 140) { Value = slug },
                new SqlParameter("@ExcludeId", SqlDbType.Int) { Value = SqlHelper.Value(excludeCourseId) });
        }

        public int CountPublished()
        {
            const string sql = "SELECT COUNT(1) FROM dbo.Courses WHERE IsPublished = 1;";
            return Convert.ToInt32(SqlHelper.ExecuteScalar(sql));
        }

        public int CountAll()
        {
            const string sql = "SELECT COUNT(1) FROM dbo.Courses;";
            return Convert.ToInt32(SqlHelper.ExecuteScalar(sql));
        }

        // ----------------------------------------------------------------- writes --

        public int Insert(Course course)
        {
            using (var connection = DbConnectionFactory.Open())
            using (var transaction = connection.BeginTransaction())
            {
                var id = Insert(connection, transaction, course);
                transaction.Commit();
                return id;
            }
        }

        /// <summary>Insert inside the caller's transaction, returning the new identity.</summary>
        public int Insert(SqlConnection connection, SqlTransaction transaction, Course course)
        {
            const string sql =
                "INSERT INTO dbo.Courses " +
                "(Title, Slug, Description, Category, Level, EstimatedMinutes, IsCertification, " +
                " CertificateName, PassingScorePercent, ThumbnailUrl, IsPublished, CreatedAt) " +
                "OUTPUT INSERTED.Id VALUES " +
                "(@Title, @Slug, @Description, @Category, @Level, @EstimatedMinutes, @IsCertification, " +
                " @CertificateName, @PassingScorePercent, @ThumbnailUrl, @IsPublished, @CreatedAt);";

            using (var command = SqlHelper.Prepare(connection, transaction, sql))
            {
                AddParameters(command, course);
                SqlHelper.AddDateTime(command, "@CreatedAt",
                    course.CreatedAt == default(DateTime) ? DateTime.UtcNow : course.CreatedAt);
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        public void Update(Course course)
        {
            using (var connection = DbConnectionFactory.Open())
            using (var transaction = connection.BeginTransaction())
            {
                Update(connection, transaction, course);
                transaction.Commit();
            }
        }

        public static void Update(SqlConnection connection, SqlTransaction transaction, Course course)
        {
            const string sql =
                "UPDATE dbo.Courses SET Title = @Title, Slug = @Slug, Description = @Description, " +
                "Category = @Category, Level = @Level, EstimatedMinutes = @EstimatedMinutes, " +
                "IsCertification = @IsCertification, CertificateName = @CertificateName, " +
                "PassingScorePercent = @PassingScorePercent, ThumbnailUrl = @ThumbnailUrl, " +
                "IsPublished = @IsPublished WHERE Id = @Id;";

            using (var command = SqlHelper.Prepare(connection, transaction, sql))
            {
                AddParameters(command, course);
                SqlHelper.AddInt(command, "@Id", course.Id);
                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Delete a course with every dependent row removed in a safe order, inside one transaction.
        /// Certificates go first (the Certificates -> QuizAttempts link is NO ACTION), then the
        /// assessment chain, then the learning content. Course-owned resources are detached back to
        /// the global list, matching the original SET NULL relationship.
        /// </summary>
        public void Delete(int courseId)
        {
            SqlHelper.WithTransaction((connection, transaction) => Delete(connection, transaction, courseId));
        }

        public void Delete(SqlConnection connection, SqlTransaction transaction, int courseId)
        {
            CertificateRepository.DeleteByCourse(connection, transaction, courseId);

            Execute(connection, transaction,
                "UPDATE dbo.Challenges SET LessonId = NULL WHERE CourseId = @CourseId;", courseId);

            Execute(connection, transaction,
                "DELETE FROM dbo.Questions WHERE QuizId IN (SELECT Id FROM dbo.Quizzes WHERE CourseId = @CourseId);", courseId);

            Execute(connection, transaction,
                "DELETE FROM dbo.QuizAttempts WHERE QuizId IN (SELECT Id FROM dbo.Quizzes WHERE CourseId = @CourseId);", courseId);

            Execute(connection, transaction, "DELETE FROM dbo.Quizzes WHERE CourseId = @CourseId;", courseId);

            Execute(connection, transaction,
                "DELETE FROM dbo.Progress WHERE LessonId IN (SELECT Id FROM dbo.Lessons WHERE CourseId = @CourseId);", courseId);

            Execute(connection, transaction,
                "DELETE FROM dbo.Lessons WHERE CourseId = @CourseId;", courseId);

            Execute(connection, transaction, "DELETE FROM dbo.Challenges WHERE CourseId = @CourseId;", courseId);
            Execute(connection, transaction, "DELETE FROM dbo.CourseModules WHERE CourseId = @CourseId;", courseId);
            Execute(connection, transaction, "DELETE FROM dbo.Enrollments WHERE CourseId = @CourseId;", courseId);

            Execute(connection, transaction,
                "UPDATE dbo.Resources SET CourseId = NULL WHERE CourseId = @CourseId;", courseId);

            Execute(connection, transaction, "DELETE FROM dbo.Courses WHERE Id = @CourseId;", courseId);
        }

        // ----------------------------------------------------------------- mapping --

        internal static Course Map(SqlDataReader reader)
        {
            return new Course
            {
                Id = SqlHelper.GetInt(reader, "Id"),
                Title = SqlHelper.GetString(reader, "Title"),
                Slug = SqlHelper.GetString(reader, "Slug"),
                Description = SqlHelper.GetString(reader, "Description"),
                Category = SqlHelper.GetString(reader, "Category"),
                Level = SqlHelper.GetString(reader, "Level"),
                EstimatedMinutes = SqlHelper.GetInt(reader, "EstimatedMinutes"),
                IsCertification = SqlHelper.GetBool(reader, "IsCertification"),
                CertificateName = SqlHelper.GetNullableString(reader, "CertificateName"),
                PassingScorePercent = SqlHelper.GetInt(reader, "PassingScorePercent"),
                ThumbnailUrl = SqlHelper.GetNullableString(reader, "ThumbnailUrl"),
                IsPublished = SqlHelper.GetBool(reader, "IsPublished"),
                CreatedAt = SqlHelper.GetDateTime(reader, "CreatedAt")
            };
        }

        private static void AddParameters(SqlCommand command, Course course)
        {
            SqlHelper.AddNVarChar(command, "@Title", course.Title, 120);
            SqlHelper.AddNVarChar(command, "@Slug", course.Slug, 140);
            SqlHelper.AddNVarChar(command, "@Description", course.Description, 1000);
            SqlHelper.AddNVarChar(command, "@Category", course.Category, 60);
            SqlHelper.AddNVarChar(command, "@Level", course.Level, 30);
            SqlHelper.AddInt(command, "@EstimatedMinutes", course.EstimatedMinutes);
            SqlHelper.AddBool(command, "@IsCertification", course.IsCertification);
            SqlHelper.AddNVarChar(command, "@CertificateName", course.CertificateName, 160);
            SqlHelper.AddInt(command, "@PassingScorePercent", course.PassingScorePercent);
            SqlHelper.AddNVarChar(command, "@ThumbnailUrl", course.ThumbnailUrl, 300);
            SqlHelper.AddBool(command, "@IsPublished", course.IsPublished);
        }

        private static void Execute(SqlConnection connection, SqlTransaction transaction, string sql, int courseId)
        {
            using (var command = SqlHelper.Prepare(connection, transaction, sql))
            {
                SqlHelper.AddInt(command, "@CourseId", courseId);
                command.ExecuteNonQuery();
            }
        }
    }
}
