using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CodeRoom.WebForms.Models;


namespace CodeRoom.WebForms.Data
{
    /// <summary>ADO.NET access to QuizAttempts, the submitted assessment results.</summary>
    public class QuizAttemptRepository
    {
        public int Insert(SqlConnection connection, SqlTransaction transaction, QuizAttempt attempt)
        {
            const string sql =
                "INSERT INTO dbo.QuizAttempts (UserId, QuizId, Score, TotalQuestions, AttemptedAt) " +
                "OUTPUT INSERTED.Id VALUES (@UserId, @QuizId, @Score, @TotalQuestions, @AttemptedAt);";

            using (var command = SqlHelper.Prepare(connection, transaction, sql))
            {
                SqlHelper.AddInt(command, "@UserId", attempt.UserId);
                SqlHelper.AddInt(command, "@QuizId", attempt.QuizId);
                SqlHelper.AddInt(command, "@Score", attempt.Score);
                SqlHelper.AddInt(command, "@TotalQuestions", attempt.TotalQuestions);
                SqlHelper.AddDateTime(command, "@AttemptedAt",
                    attempt.AttemptedAt == default(DateTime) ? DateTime.UtcNow : attempt.AttemptedAt);

                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        public int Insert(QuizAttempt attempt)
        {
            return SqlHelper.WithTransaction((connection, transaction) => Insert(connection, transaction, attempt));
        }

        public QuizAttempt GetById(int attemptId)
        {
            const string sql =
                "SELECT Id, UserId, QuizId, Score, TotalQuestions, AttemptedAt " +
                "FROM dbo.QuizAttempts WHERE Id = @AttemptId;";

            return SqlHelper.ReadFirst(sql, Map,
                new SqlParameter("@AttemptId", SqlDbType.Int) { Value = attemptId });
        }

        /// <summary>Most recent attempts of a learner, each carrying its quiz for the results link.</summary>
        public List<QuizAttempt> GetRecentForUser(int userId, int take = 10)
        {
            const string sql =
                "SELECT TOP (@Take) a.Id, a.UserId, a.QuizId, a.Score, a.TotalQuestions, a.AttemptedAt, " +
                "       q.CourseId, q.Title, q.Description, q.AssessmentType, q.TimeLimitMinutes, " +
                "       q.PassingScorePercent, q.IsCertificationExam " +
                "FROM dbo.QuizAttempts AS a " +
                "INNER JOIN dbo.Quizzes AS q ON q.Id = a.QuizId " +
                "WHERE a.UserId = @UserId " +
                "ORDER BY a.AttemptedAt DESC;";

            var attempts = SqlHelper.ReadList(sql, MapWithQuiz,
                new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                new SqlParameter("@Take", SqlDbType.Int) { Value = take });

            return attempts;
        }

        public List<QuizAttempt> GetByQuizForUser(int userId, int quizId)
        {
            const string sql =
                "SELECT Id, UserId, QuizId, Score, TotalQuestions, AttemptedAt " +
                "FROM dbo.QuizAttempts WHERE UserId = @UserId AND QuizId = @QuizId ORDER BY AttemptedAt DESC;";

            return SqlHelper.ReadList(sql, Map,
                new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                new SqlParameter("@QuizId", SqlDbType.Int) { Value = quizId });
        }

        public int GetAveragePercentForUser(int userId)
        {
            const string sql =
                "SELECT AVG(CASE WHEN TotalQuestions = 0 THEN 0.0 " +
                "ELSE (Score * 100.0 / TotalQuestions) END) " +
                "FROM dbo.QuizAttempts WHERE UserId = @UserId;";

            var value = SqlHelper.ExecuteScalar(sql,
                new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });

            return value == null || value == SqlHelper.Null ? 0 : Convert.ToInt32(Math.Round(Convert.ToDouble(value)));
        }

        public int CountByUser(int userId)
        {
            const string sql = "SELECT COUNT(1) FROM dbo.QuizAttempts WHERE UserId = @UserId;";

            return Convert.ToInt32(SqlHelper.ExecuteScalar(sql,
                new SqlParameter("@UserId", SqlDbType.Int) { Value = userId }));
        }

        public int CountAll()
        {
            return Convert.ToInt32(SqlHelper.ExecuteScalar("SELECT COUNT(1) FROM dbo.QuizAttempts;"));
        }

        internal static QuizAttempt Map(SqlDataReader reader)
        {
            return new QuizAttempt
            {
                Id = SqlHelper.GetInt(reader, "Id"),
                UserId = SqlHelper.GetInt(reader, "UserId"),
                QuizId = SqlHelper.GetInt(reader, "QuizId"),
                Score = SqlHelper.GetInt(reader, "Score"),
                TotalQuestions = SqlHelper.GetInt(reader, "TotalQuestions"),
                AttemptedAt = SqlHelper.GetDateTime(reader, "AttemptedAt")
            };
        }

        private static QuizAttempt MapWithQuiz(SqlDataReader reader)
        {
            return new QuizAttempt
            {
                Id = SqlHelper.GetInt(reader, "Id"),
                UserId = SqlHelper.GetInt(reader, "UserId"),
                QuizId = SqlHelper.GetInt(reader, "QuizId"),
                Score = SqlHelper.GetInt(reader, "Score"),
                TotalQuestions = SqlHelper.GetInt(reader, "TotalQuestions"),
                AttemptedAt = SqlHelper.GetDateTime(reader, "AttemptedAt"),
                Quiz = new Quiz
                {
                    Id = SqlHelper.GetInt(reader, "QuizId"),
                    CourseId = SqlHelper.GetInt(reader, "CourseId"),
                    Title = SqlHelper.GetString(reader, "Title"),
                    Description = SqlHelper.GetNullableString(reader, "Description"),
                    AssessmentType = SqlHelper.GetString(reader, "AssessmentType"),
                    TimeLimitMinutes = SqlHelper.GetInt(reader, "TimeLimitMinutes"),
                    PassingScorePercent = SqlHelper.GetInt(reader, "PassingScorePercent"),
                    IsCertificationExam = SqlHelper.GetBool(reader, "IsCertificationExam")
                }
            };
        }
    }
}
