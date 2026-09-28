using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CodeRoom.WebForms.Models;
using CodeRoom.WebForms.Services;

namespace CodeRoom.WebForms.Data
{
    /// <summary>ADO.NET access to Challenges, the hands-on tasks attached to a course or lesson.</summary>
    public class ChallengeRepository
    {
        private const string Columns =
            "Id, CourseId, LessonId, Title, Instructions, StarterCode, Hint, ExpectedAnswer, " +
            "ValidationMode, Points";

        public static List<Challenge> GetByCourse(int courseId)
        {
            const string sql = "SELECT " + Columns + " FROM dbo.Challenges WHERE CourseId = @CourseId ORDER BY Id;";

            return SqlHelper.ReadList(sql, Map,
                new SqlParameter("@CourseId", SqlDbType.Int) { Value = courseId });
        }

        public Challenge GetById(int challengeId)
        {
            const string sql = "SELECT " + Columns + " FROM dbo.Challenges WHERE Id = @ChallengeId;";

            return SqlHelper.ReadFirst(sql, Map,
                new SqlParameter("@ChallengeId", SqlDbType.Int) { Value = challengeId });
        }

        /// <summary>The task a learner sees when they open a lesson, if one is attached.</summary>
        public Challenge GetFirstForLesson(int lessonId)
        {
            const string sql =
                "SELECT TOP (1) " + Columns + " FROM dbo.Challenges WHERE LessonId = @LessonId ORDER BY Id;";

            return SqlHelper.ReadFirst(sql, Map,
                new SqlParameter("@LessonId", SqlDbType.Int) { Value = lessonId });
        }

        /// <summary>
        /// Server side validation exactly as the source application does it: whole answer for Exact,
        /// substring for Contains, both case insensitive.
        /// </summary>
        public static bool Validate(Challenge challenge, string answer)
        {
            if (challenge == null || answer == null)
            {
                return false;
            }

            var submitted = answer.Trim();
            var expected = challenge.ExpectedAnswer.Trim();

            if (string.Equals(challenge.ValidationMode, DomainValues.ValidationMode.Contains,
                    StringComparison.OrdinalIgnoreCase))
            {
                return submitted.IndexOf(expected, StringComparison.OrdinalIgnoreCase) >= 0;
            }

            return string.Equals(submitted, expected, StringComparison.OrdinalIgnoreCase);
        }

        public DataTable GetAllGrid()
        {
            const string sql =
                "SELECT ch.Id, ch.Title, ch.ValidationMode, ch.Points, c.Title AS CourseTitle, " +
                "       l.Title AS LessonTitle " +
                "FROM dbo.Challenges AS ch " +
                "INNER JOIN dbo.Courses AS c ON c.Id = ch.CourseId " +
                "LEFT JOIN dbo.Lessons AS l ON l.Id = ch.LessonId " +
                "ORDER BY c.Title, ch.Id;";

            return SqlHelper.GetTable(sql);
        }

        // ----------------------------------------------------------------- writes --

        public int Insert(Challenge challenge)
        {
            return SqlHelper.WithTransaction((connection, transaction) => Insert(connection, transaction, challenge));
        }

        public int Insert(SqlConnection connection, SqlTransaction transaction, Challenge challenge)
        {
            const string sql =
                "INSERT INTO dbo.Challenges " +
                "(CourseId, LessonId, Title, Instructions, StarterCode, Hint, ExpectedAnswer, " +
                " ValidationMode, Points) " +
                "OUTPUT INSERTED.Id VALUES " +
                "(@CourseId, @LessonId, @Title, @Instructions, @StarterCode, @Hint, @ExpectedAnswer, " +
                " @ValidationMode, @Points);";

            using (var command = SqlHelper.Prepare(connection, transaction, sql))
            {
                AddParameters(command, challenge);
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        public void Update(SqlConnection connection, SqlTransaction transaction, Challenge challenge)
        {
            const string sql =
                "UPDATE dbo.Challenges SET CourseId = @CourseId, LessonId = @LessonId, Title = @Title, " +
                "Instructions = @Instructions, StarterCode = @StarterCode, Hint = @Hint, " +
                "ExpectedAnswer = @ExpectedAnswer, ValidationMode = @ValidationMode, Points = @Points " +
                "WHERE Id = @Id;";

            using (var command = SqlHelper.Prepare(connection, transaction, sql))
            {
                AddParameters(command, challenge);
                SqlHelper.AddInt(command, "@Id", challenge.Id);
                command.ExecuteNonQuery();
            }
        }

        public void Update(Challenge challenge)
        {
            SqlHelper.WithTransaction((connection, transaction) => Update(connection, transaction, challenge));
        }

        public void Delete(SqlConnection connection, SqlTransaction transaction, int challengeId)
        {
            const string sql = "DELETE FROM dbo.Challenges WHERE Id = @ChallengeId;";

            using (var command = SqlHelper.Prepare(connection, transaction, sql))
            {
                SqlHelper.AddInt(command, "@ChallengeId", challengeId);
                command.ExecuteNonQuery();
            }
        }

        public void Delete(int challengeId)
        {
            SqlHelper.WithTransaction((connection, transaction) => Delete(connection, transaction, challengeId));
        }

        public int CountAll()
        {
            return Convert.ToInt32(SqlHelper.ExecuteScalar("SELECT COUNT(1) FROM dbo.Challenges;"));
        }

        private static void AddParameters(SqlCommand command, Challenge challenge)
        {
            SqlHelper.AddInt(command, "@CourseId", challenge.CourseId);
            SqlHelper.Add(command, "@LessonId", SqlDbType.Int, SqlHelper.Value(challenge.LessonId));
            SqlHelper.AddNVarChar(command, "@Title", challenge.Title, 160);
            SqlHelper.AddNVarChar(command, "@Instructions", challenge.Instructions, 1800);
            SqlHelper.AddNVarChar(command, "@StarterCode", challenge.StarterCode, 2000);
            SqlHelper.AddNVarChar(command, "@Hint", challenge.Hint, 600);
            SqlHelper.AddNVarChar(command, "@ExpectedAnswer", challenge.ExpectedAnswer, 1000);
            SqlHelper.AddNVarChar(command, "@ValidationMode", challenge.ValidationMode, 30);
            SqlHelper.AddInt(command, "@Points", challenge.Points);
        }

        internal static Challenge Map(SqlDataReader reader)
        {
            return new Challenge
            {
                Id = SqlHelper.GetInt(reader, "Id"),
                CourseId = SqlHelper.GetInt(reader, "CourseId"),
                LessonId = SqlHelper.GetNullableInt(reader, "LessonId"),
                Title = SqlHelper.GetString(reader, "Title"),
                Instructions = SqlHelper.GetString(reader, "Instructions"),
                StarterCode = SqlHelper.GetNullableString(reader, "StarterCode"),
                Hint = SqlHelper.GetNullableString(reader, "Hint"),
                ExpectedAnswer = SqlHelper.GetString(reader, "ExpectedAnswer"),
                ValidationMode = SqlHelper.GetString(reader, "ValidationMode"),
                Points = SqlHelper.GetInt(reader, "Points")
            };
        }
    }
}
