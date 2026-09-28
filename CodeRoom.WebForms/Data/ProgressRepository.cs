using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CodeRoom.WebForms.Models;

namespace CodeRoom.WebForms.Data
{
    /// <summary>
    /// ADO.NET access to Progress, one row per learner and lesson. Marking a lesson complete is the
    /// heaviest flow in the application, so the write is transaction aware and reports whether the
    /// completion was new; the caller only awards XP and achievements the first time.
    /// </summary>
    public class ProgressRepository
    {
        public Progress Get(int userId, int lessonId)
        {
            const string sql =
                "SELECT Id, UserId, LessonId, IsCompleted, CompletedAt " +
                "FROM dbo.Progress WHERE UserId = @UserId AND LessonId = @LessonId;";

            return SqlHelper.ReadFirst(sql, Map,
                new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                new SqlParameter("@LessonId", SqlDbType.Int) { Value = lessonId });
        }

        public bool IsCompleted(int userId, int lessonId)
        {
            const string sql =
                "SELECT COUNT(1) FROM dbo.Progress " +
                "WHERE UserId = @UserId AND LessonId = @LessonId AND IsCompleted = 1;";

            return Convert.ToInt32(SqlHelper.ExecuteScalar(sql,
                new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                new SqlParameter("@LessonId", SqlDbType.Int) { Value = lessonId })) > 0;
        }

        /// <summary>Lesson ids the learner has finished inside one course, for outline check marks.</summary>
        public List<int> GetCompletedLessonIds(int userId, int courseId)
        {
            const string sql =
                "SELECT p.LessonId FROM dbo.Progress AS p " +
                "INNER JOIN dbo.Lessons AS l ON l.Id = p.LessonId " +
                "WHERE p.UserId = @UserId AND l.CourseId = @CourseId AND p.IsCompleted = 1;";

            var ids = new List<int>();

            using (var connection = DbConnectionFactory.Open())
            using (var command = SqlHelper.Prepare(connection, null, sql))
            {
                SqlHelper.AddInt(command, "@UserId", userId);
                SqlHelper.AddInt(command, "@CourseId", courseId);

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
        /// Upsert the completion row. Returns true when this call completed the lesson for the first time,
        /// which is the signal the caller needs before awarding XP, streaks and certificates.
        /// </summary>
        public bool MarkCompleted(SqlConnection connection, SqlTransaction transaction, int userId, int lessonId)
        {
            const string sql =
                "DECLARE @exists int = (SELECT COUNT(1) FROM dbo.Progress " +
                "                         WHERE UserId = @UserId AND LessonId = @LessonId); " +
                "DECLARE @wasComplete int = (SELECT COUNT(1) FROM dbo.Progress " +
                "                             WHERE UserId = @UserId AND LessonId = @LessonId AND IsCompleted = 1); " +
                "IF @exists = 0 " +
                "BEGIN " +
                "    INSERT INTO dbo.Progress (UserId, LessonId, IsCompleted, CompletedAt) " +
                "    VALUES (@UserId, @LessonId, 1, @Now); " +
                "END " +
                "ELSE IF @wasComplete = 0 " +
                "BEGIN " +
                "    UPDATE dbo.Progress SET IsCompleted = 1, CompletedAt = @Now " +
                "    WHERE UserId = @UserId AND LessonId = @LessonId; " +
                "END " +
                "SELECT CASE WHEN @wasComplete = 0 THEN 1 ELSE 0 END;";

            using (var command = SqlHelper.Prepare(connection, transaction, sql))
            {
                SqlHelper.AddDateTime(command, "@Now", DateTime.UtcNow);
                SqlHelper.AddInt(command, "@UserId", userId);
                SqlHelper.AddInt(command, "@LessonId", lessonId);

                var result = command.ExecuteScalar();
                return result != null && result != SqlHelper.Null && Convert.ToInt32(result) == 1;
            }
        }

        public bool MarkCompleted(int userId, int lessonId)
        {
            return SqlHelper.WithTransaction((connection, transaction) => MarkCompleted(connection, transaction, userId, lessonId));
        }

        /// <summary>Reset a completion, used when an administrator changes lesson content.</summary>
        public void MarkIncomplete(SqlConnection connection, SqlTransaction transaction, int userId, int lessonId)
        {
            const string sql =
                "UPDATE dbo.Progress SET IsCompleted = 0, CompletedAt = NULL " +
                "WHERE UserId = @UserId AND LessonId = @LessonId;";

            using (var command = SqlHelper.Prepare(connection, transaction, sql))
            {
                SqlHelper.AddInt(command, "@UserId", userId);
                SqlHelper.AddInt(command, "@LessonId", lessonId);
                command.ExecuteNonQuery();
            }
        }

        /// <summary>Number of completed lessons for one learner in one course.</summary>
        public int CountCompletedInCourse(int userId, int courseId)
        {
            const string sql =
                "SELECT COUNT(1) FROM dbo.Progress AS p " +
                "INNER JOIN dbo.Lessons AS l ON l.Id = p.LessonId " +
                "WHERE p.UserId = @UserId AND l.CourseId = @CourseId AND p.IsCompleted = 1;";

            return Convert.ToInt32(SqlHelper.ExecuteScalar(sql,
                new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                new SqlParameter("@CourseId", SqlDbType.Int) { Value = courseId }));
        }

        public int CountCompletedForUser(int userId)
        {
            const string sql = "SELECT COUNT(1) FROM dbo.Progress WHERE UserId = @UserId AND IsCompleted = 1;";

            return Convert.ToInt32(SqlHelper.ExecuteScalar(sql,
                new SqlParameter("@UserId", SqlDbType.Int) { Value = userId }));
        }

        /// <summary>Has every lesson of the course been completed? Used to award CourseCompleted.</summary>
        public bool IsCourseComplete(int userId, int courseId)
        {
            const string sql =
                "SELECT CASE WHEN COUNT(1) > 0 AND COUNT(1) = SUM(CASE WHEN p.IsCompleted = 1 THEN 1 ELSE 0 END) " +
                "           THEN 1 ELSE 0 END " +
                "FROM dbo.Lessons AS l " +
                "LEFT JOIN dbo.Progress AS p ON p.LessonId = l.Id AND p.UserId = @UserId " +
                "WHERE l.CourseId = @CourseId;";

            return Convert.ToInt32(SqlHelper.ExecuteScalar(sql,
                new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                new SqlParameter("@CourseId", SqlDbType.Int) { Value = courseId })) == 1;
        }

        public static void DeleteForUserAndCourse(SqlConnection connection, SqlTransaction transaction, int userId, int courseId)
        {
            const string sql =
                "DELETE FROM dbo.Progress WHERE UserId = @UserId " +
                "AND LessonId IN (SELECT Id FROM dbo.Lessons WHERE CourseId = @CourseId);";

            using (var command = SqlHelper.Prepare(connection, transaction, sql))
            {
                SqlHelper.AddInt(command, "@UserId", userId);
                SqlHelper.AddInt(command, "@CourseId", courseId);
                command.ExecuteNonQuery();
            }
        }

        internal static Progress Map(SqlDataReader reader)
        {
            return new Progress
            {
                Id = SqlHelper.GetInt(reader, "Id"),
                UserId = SqlHelper.GetInt(reader, "UserId"),
                LessonId = SqlHelper.GetInt(reader, "LessonId"),
                IsCompleted = SqlHelper.GetBool(reader, "IsCompleted"),
                CompletedAt = SqlHelper.GetNullableDateTime(reader, "CompletedAt")
            };
        }
    }
}
