using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CodeRoom.WebForms.Models;
using CodeRoom.WebForms.Services;

namespace CodeRoom.WebForms.Data
{
    /// <summary>
    /// ADO.NET access to Quizzes and Questions. Deleting a quiz removes its questions in the same
    /// transaction, because the assessment chain ends in Certificates, which SQL Server keeps as NO ACTION.
    /// </summary>
    public class QuizRepository
    {
        private const string Columns =
            "Id, CourseId, Title, Description, AssessmentType, TimeLimitMinutes, PassingScorePercent, " +
            "IsCertificationExam";

        public Quiz GetByCourse(int courseId)
        {
            const string sql = "SELECT TOP (1) " + Columns + " FROM dbo.Quizzes WHERE CourseId = @CourseId ORDER BY Id;";

            return SqlHelper.ReadFirst(sql, Map,
                new SqlParameter("@CourseId", SqlDbType.Int) { Value = courseId });
        }

        public Quiz GetById(int quizId)
        {
            const string sql = "SELECT " + Columns + " FROM dbo.Quizzes WHERE Id = @QuizId;";

            return SqlHelper.ReadFirst(sql, Map,
                new SqlParameter("@QuizId", SqlDbType.Int) { Value = quizId });
        }

        /// <summary>Quiz with its questions, ready to render or to mark.</summary>
        public Quiz GetForTaking(int quizId)
        {
            var quiz = GetById(quizId);

            if (quiz == null)
            {
                return null;
            }

            quiz.Questions = GetQuestions(quizId);
            return quiz;
        }

        public Quiz GetForCourseTaking(int courseId)
        {
            var quiz = GetByCourse(courseId);
            return quiz == null ? null : GetForTaking(quiz.Id);
        }

        public List<Question> GetQuestions(int quizId)
        {
            const string sql =
                "SELECT Id, QuizId, QuestionText, OptionA, OptionB, OptionC, OptionD, CorrectOption " +
                "FROM dbo.Questions WHERE QuizId = @QuizId ORDER BY Id;";

            return SqlHelper.ReadList(sql, MapQuestion,
                new SqlParameter("@QuizId", SqlDbType.Int) { Value = quizId });
        }

        /// <summary>Question ids and correct options only, for scoring a submission server side.</summary>
        public Dictionary<int, string> GetAnswerKey(int quizId)
        {
            const string sql = "SELECT Id, CorrectOption FROM dbo.Questions WHERE QuizId = @QuizId;";

            var key = new Dictionary<int, string>();

            using (var connection = DbConnectionFactory.Open())
            using (var command = SqlHelper.Prepare(connection, null, sql))
            {
                SqlHelper.AddInt(command, "@QuizId", quizId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        key[reader.GetInt32(0)] = reader.GetString(1);
                    }
                }
            }

            return key;
        }

        public int CountQuestions(int quizId)
        {
            const string sql = "SELECT COUNT(1) FROM dbo.Questions WHERE QuizId = @QuizId;";

            return Convert.ToInt32(SqlHelper.ExecuteScalar(sql,
                new SqlParameter("@QuizId", SqlDbType.Int) { Value = quizId }));
        }

        /// <summary>The pass mark rule of the source application: plain quizzes use 70, exams use their own value.</summary>
        public bool HasPassed(Quiz quiz, int percentage)
        {
            if (quiz == null)
            {
                return false;
            }

            var required = string.Equals(quiz.AssessmentType, DomainValues.AssessmentType.Quiz,
                StringComparison.OrdinalIgnoreCase) ? 70 : quiz.PassingScorePercent;

            return percentage >= required;
        }

        public DataTable GetAllGrid()
        {
            const string sql =
                "SELECT q.Id, q.Title, q.AssessmentType, q.TimeLimitMinutes, q.PassingScorePercent, " +
                "       q.IsCertificationExam, c.Title AS CourseTitle, " +
                "       (SELECT COUNT(1) FROM dbo.Questions AS qs WHERE qs.QuizId = q.Id) AS QuestionCount " +
                "FROM dbo.Quizzes AS q " +
                "INNER JOIN dbo.Courses AS c ON c.Id = q.CourseId " +
                "ORDER BY c.Title, q.Title;";

            return SqlHelper.GetTable(sql);
        }

        // ----------------------------------------------------------------- writes --

        public int Insert(Quiz quiz)
        {
            return SqlHelper.WithTransaction((connection, transaction) => Insert(connection, transaction, quiz));
        }

        public int Insert(SqlConnection connection, SqlTransaction transaction, Quiz quiz)
        {
            const string sql =
                "INSERT INTO dbo.Quizzes " +
                "(CourseId, Title, Description, AssessmentType, TimeLimitMinutes, PassingScorePercent, " +
                " IsCertificationExam) " +
                "OUTPUT INSERTED.Id VALUES " +
                "(@CourseId, @Title, @Description, @AssessmentType, @TimeLimitMinutes, @PassingScorePercent, " +
                " @IsCertificationExam);";

            using (var command = SqlHelper.Prepare(connection, transaction, sql))
            {
                AddParameters(command, quiz);
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        public void Update(SqlConnection connection, SqlTransaction transaction, Quiz quiz)
        {
            const string sql =
                "UPDATE dbo.Quizzes SET CourseId = @CourseId, Title = @Title, Description = @Description, " +
                "AssessmentType = @AssessmentType, TimeLimitMinutes = @TimeLimitMinutes, " +
                "PassingScorePercent = @PassingScorePercent, IsCertificationExam = @IsCertificationExam " +
                "WHERE Id = @Id;";

            using (var command = SqlHelper.Prepare(connection, transaction, sql))
            {
                AddParameters(command, quiz);
                SqlHelper.AddInt(command, "@Id", quiz.Id);
                command.ExecuteNonQuery();
            }
        }

        public void Update(Quiz quiz)
        {
            SqlHelper.WithTransaction((connection, transaction) => Update(connection, transaction, quiz));
        }

        /// <summary>
        /// Delete a quiz, its questions and any certificates earned from its attempts in one transaction.
        /// Certificates must go before the attempts because their link is enforced as NO ACTION.
        /// </summary>
        public void Delete(int quizId)
        {
            SqlHelper.WithTransaction((connection, transaction) => Delete(connection, transaction, quizId));
        }

        public void Delete(SqlConnection connection, SqlTransaction transaction, int quizId)
        {
            CertificateRepository.DeleteByQuiz(connection, transaction, quizId);

            using (var command = SqlHelper.Prepare(connection, transaction,
                "DELETE FROM dbo.Questions WHERE QuizId = @QuizId;"))
            {
                SqlHelper.AddInt(command, "@QuizId", quizId);
                command.ExecuteNonQuery();
            }

            using (var command = SqlHelper.Prepare(connection, transaction,
                "DELETE FROM dbo.QuizAttempts WHERE QuizId = @QuizId;"))
            {
                SqlHelper.AddInt(command, "@QuizId", quizId);
                command.ExecuteNonQuery();
            }

            using (var command = SqlHelper.Prepare(connection, transaction,
                "DELETE FROM dbo.Quizzes WHERE Id = @QuizId;"))
            {
                SqlHelper.AddInt(command, "@QuizId", quizId);
                command.ExecuteNonQuery();
            }
        }

        // ---------------------------------------------------------------- questions --

        public int InsertQuestion(SqlConnection connection, SqlTransaction transaction, Question question)
        {
            const string sql =
                "INSERT INTO dbo.Questions " +
                "(QuizId, QuestionText, OptionA, OptionB, OptionC, OptionD, CorrectOption) " +
                "OUTPUT INSERTED.Id VALUES " +
                "(@QuizId, @QuestionText, @OptionA, @OptionB, @OptionC, @OptionD, @CorrectOption);";

            using (var command = SqlHelper.Prepare(connection, transaction, sql))
            {
                AddQuestionParameters(command, question);
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        public int InsertQuestion(Question question)
        {
            return SqlHelper.WithTransaction((connection, transaction) => InsertQuestion(connection, transaction, question));
        }

        public void UpdateQuestion(SqlConnection connection, SqlTransaction transaction, Question question)
        {
            const string sql =
                "UPDATE dbo.Questions SET QuestionText = @QuestionText, OptionA = @OptionA, OptionB = @OptionB, " +
                "OptionC = @OptionC, OptionD = @OptionD, CorrectOption = @CorrectOption WHERE Id = @Id;";

            using (var command = SqlHelper.Prepare(connection, transaction, sql))
            {
                var parameter = command.Parameters.Add("@Id", SqlDbType.Int);
                parameter.Value = question.Id;
                AddQuestionParameters(command, question);
                command.ExecuteNonQuery();
            }
        }

        public void DeleteQuestion(SqlConnection connection, SqlTransaction transaction, int questionId)
        {
            const string sql = "DELETE FROM dbo.Questions WHERE Id = @QuestionId;";

            using (var command = SqlHelper.Prepare(connection, transaction, sql))
            {
                SqlHelper.AddInt(command, "@QuestionId", questionId);
                command.ExecuteNonQuery();
            }
        }

        private static void AddParameters(SqlCommand command, Quiz quiz)
        {
            SqlHelper.AddInt(command, "@CourseId", quiz.CourseId);
            SqlHelper.AddNVarChar(command, "@Title", quiz.Title, 150);
            SqlHelper.AddNVarChar(command, "@Description", quiz.Description, 500);
            SqlHelper.AddNVarChar(command, "@AssessmentType", quiz.AssessmentType, 30);
            SqlHelper.AddInt(command, "@TimeLimitMinutes", quiz.TimeLimitMinutes);
            SqlHelper.AddInt(command, "@PassingScorePercent", quiz.PassingScorePercent);
            SqlHelper.AddBool(command, "@IsCertificationExam", quiz.IsCertificationExam);
        }

        private static void AddQuestionParameters(SqlCommand command, Question question)
        {
            SqlHelper.AddInt(command, "@QuizId", question.QuizId);
            SqlHelper.AddNVarChar(command, "@QuestionText", question.QuestionText, 400);
            SqlHelper.AddNVarChar(command, "@OptionA", question.OptionA, 200);
            SqlHelper.AddNVarChar(command, "@OptionB", question.OptionB, 200);
            SqlHelper.AddNVarChar(command, "@OptionC", question.OptionC, 200);
            SqlHelper.AddNVarChar(command, "@OptionD", question.OptionD, 200);
            SqlHelper.AddNVarChar(command, "@CorrectOption", question.CorrectOption, 1);
        }

        internal static Quiz Map(SqlDataReader reader)
        {
            return new Quiz
            {
                Id = SqlHelper.GetInt(reader, "Id"),
                CourseId = SqlHelper.GetInt(reader, "CourseId"),
                Title = SqlHelper.GetString(reader, "Title"),
                Description = SqlHelper.GetString(reader, "Description"),
                AssessmentType = SqlHelper.GetString(reader, "AssessmentType"),
                TimeLimitMinutes = SqlHelper.GetInt(reader, "TimeLimitMinutes"),
                PassingScorePercent = SqlHelper.GetInt(reader, "PassingScorePercent"),
                IsCertificationExam = SqlHelper.GetBool(reader, "IsCertificationExam")
            };
        }

        internal static Question MapQuestion(SqlDataReader reader)
        {
            return new Question
            {
                Id = SqlHelper.GetInt(reader, "Id"),
                QuizId = SqlHelper.GetInt(reader, "QuizId"),
                QuestionText = SqlHelper.GetString(reader, "QuestionText"),
                OptionA = SqlHelper.GetString(reader, "OptionA"),
                OptionB = SqlHelper.GetString(reader, "OptionB"),
                OptionC = SqlHelper.GetString(reader, "OptionC"),
                OptionD = SqlHelper.GetString(reader, "OptionD"),
                CorrectOption = SqlHelper.GetString(reader, "CorrectOption")
            };
        }
    }
}
