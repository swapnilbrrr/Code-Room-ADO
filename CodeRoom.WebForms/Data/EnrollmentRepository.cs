using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CodeRoom.WebForms.Models;

namespace CodeRoom.WebForms.Data
{
    /// <summary>ADO.NET access to Enrollments, the learner to course link.</summary>
    public class EnrollmentRepository
    {
        public bool IsEnrolled(int userId, int courseId)
        {
            const string sql =
                "SELECT COUNT(1) FROM dbo.Enrollments WHERE UserId = @UserId AND CourseId = @CourseId;";

            return Count(sql, userId, courseId) > 0;
        }

        public List<Enrollment> GetByUser(int userId)
        {
            const string sql =
                "SELECT Id, UserId, CourseId, EnrolledAt FROM dbo.Enrollments " +
                "WHERE UserId = @UserId ORDER BY EnrolledAt DESC;";

            return SqlHelper.ReadList(sql, Map, new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
        }

        public List<int> GetEnrolledCourseIds(int userId)
        {
            const string sql = "SELECT CourseId FROM dbo.Enrollments WHERE UserId = @UserId;";

            var ids = new List<int>();

            using (var connection = DbConnectionFactory.Open())
            using (var command = SqlHelper.Prepare(connection, null, sql))
            {
                SqlHelper.AddInt(command, "@UserId", userId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ids.Add(reader.GetInt32(0));
                    }
                }
            }

            return ids;
        }

        /// <summary>
        /// Enrol a learner. Runs inside the caller's transaction so the enrollment, its activity entry and
        /// the award XP commit together. The unique (UserId, CourseId) index makes a double click harmless.
        /// </summary>
        public static int Insert(SqlConnection connection, SqlTransaction transaction, int userId, int courseId)
        {
            const string sql =
                "IF NOT EXISTS (SELECT 1 FROM dbo.Enrollments WHERE UserId = @UserId AND CourseId = @CourseId) " +
                "BEGIN " +
                "    INSERT INTO dbo.Enrollments (UserId, CourseId, EnrolledAt) " +
                "    OUTPUT INSERTED.Id VALUES (@UserId, @CourseId, @EnrolledAt); " +
                "END " +
                "ELSE " +
                "BEGIN " +
                "    SELECT Id FROM dbo.Enrollments WHERE UserId = @UserId AND CourseId = @CourseId; " +
                "END";

            using (var command = SqlHelper.Prepare(connection, transaction, sql))
            {
                SqlHelper.AddInt(command, "@UserId", userId);
                SqlHelper.AddInt(command, "@CourseId", courseId);
                SqlHelper.AddDateTime(command, "@EnrolledAt", DateTime.UtcNow);

                var result = command.ExecuteScalar();
                return result == null || result == SqlHelper.Null ? 0 : Convert.ToInt32(result);
            }
        }

        public int Insert(int userId, int courseId)
        {
            return SqlHelper.WithTransaction((connection, transaction) => Insert(connection, transaction, userId, courseId));
        }

        public static void Delete(SqlConnection connection, SqlTransaction transaction, int userId, int courseId)
        {
            const string sql = "DELETE FROM dbo.Enrollments WHERE UserId = @UserId AND CourseId = @CourseId;";

            using (var command = SqlHelper.Prepare(connection, transaction, sql))
            {
                SqlHelper.AddInt(command, "@UserId", userId);
                SqlHelper.AddInt(command, "@CourseId", courseId);
                command.ExecuteNonQuery();
            }
        }

        public void Delete(int userId, int courseId)
        {
            SqlHelper.WithTransaction((connection, transaction) => Delete(connection, transaction, userId, courseId));
        }

        public int CountByCourse(int courseId)
        {
            const string sql = "SELECT COUNT(1) FROM dbo.Enrollments WHERE CourseId = @CourseId;";

            return Convert.ToInt32(SqlHelper.ExecuteScalar(sql,
                new SqlParameter("@CourseId", SqlDbType.Int) { Value = courseId }));
        }

        public int CountByUser(int userId)
        {
            const string sql = "SELECT COUNT(1) FROM dbo.Enrollments WHERE UserId = @UserId;";

            return Convert.ToInt32(SqlHelper.ExecuteScalar(sql,
                new SqlParameter("@UserId", SqlDbType.Int) { Value = userId }));
        }

        public int CountAll()
        {
            return Convert.ToInt32(SqlHelper.ExecuteScalar("SELECT COUNT(1) FROM dbo.Enrollments;"));
        }

        /// <summary>Enrollment counts per course for the administration dashboard.</summary>
        public DataTable GetCourseTotals()
        {
            const string sql =
                "SELECT c.Id, c.Title, COUNT(e.Id) AS Learners " +
                "FROM dbo.Courses AS c " +
                "LEFT JOIN dbo.Enrollments AS e ON e.CourseId = c.Id " +
                "GROUP BY c.Id, c.Title " +
                "ORDER BY Learners DESC, c.Title;";

            return SqlHelper.GetTable(sql);
        }

        private static int Count(string sql, int userId, int courseId)
        {
            using (var connection = DbConnectionFactory.Open())
            using (var command = SqlHelper.Prepare(connection, null, sql))
            {
                SqlHelper.AddInt(command, "@UserId", userId);
                SqlHelper.AddInt(command, "@CourseId", courseId);
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        internal static Enrollment Map(SqlDataReader reader)
        {
            return new Enrollment
            {
                Id = SqlHelper.GetInt(reader, "Id"),
                UserId = SqlHelper.GetInt(reader, "UserId"),
                CourseId = SqlHelper.GetInt(reader, "CourseId"),
                EnrolledAt = SqlHelper.GetDateTime(reader, "EnrolledAt")
            };
        }
    }
}
