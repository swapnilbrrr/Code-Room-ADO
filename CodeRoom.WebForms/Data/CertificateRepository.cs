using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CodeRoom.WebForms.Models;

namespace CodeRoom.WebForms.Data
{
    /// <summary>
    /// ADO.NET access to Certificates. Issuing one is a transaction: the attempt, the certificate row and
    /// the award XP must commit together. Because SQL Server keeps Certificates -> QuizAttempts as
    /// NO ACTION, this repository also exposes the explicit dependent deletes other tables need before
    /// they can remove an attempt, a quiz, a course or a user.
    /// </summary>
    public class CertificateRepository
    {
        private const string Columns =
            "Id, UserId, CourseId, QuizAttemptId, CertificateNumber, Title, IssuedAt";

        public List<Certificate> GetByUser(int userId)
        {
            const string sql =
                "SELECT " + Columns + " FROM dbo.Certificates WHERE UserId = @UserId ORDER BY IssuedAt DESC;";

            return SqlHelper.ReadList(sql, Map, new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
        }

        public Certificate GetById(int certificateId)
        {
            const string sql = "SELECT " + Columns + " FROM dbo.Certificates WHERE Id = @CertificateId;";

            return SqlHelper.ReadFirst(sql, Map,
                new SqlParameter("@CertificateId", SqlDbType.Int) { Value = certificateId });
        }

        public Certificate GetOwnedByUser(int certificateId, int userId)
        {
            const string sql =
                "SELECT " + Columns + " FROM dbo.Certificates " +
                "WHERE Id = @CertificateId AND UserId = @UserId;";

            return SqlHelper.ReadFirst(sql, Map,
                new SqlParameter("@CertificateId", SqlDbType.Int) { Value = certificateId },
                new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
        }

        public Certificate GetByNumber(string certificateNumber)
        {
            const string sql = "SELECT " + Columns + " FROM dbo.Certificates WHERE CertificateNumber = @Number;";

            return SqlHelper.ReadFirst(sql, Map,
                new SqlParameter("@Number", SqlDbType.NVarChar, 40) { Value = certificateNumber });
        }

        /// <summary>One learner can only hold one certificate per course.</summary>
        public bool ExistsForUserAndCourse(int userId, int courseId)
        {
            const string sql =
                "SELECT COUNT(1) FROM dbo.Certificates WHERE UserId = @UserId AND CourseId = @CourseId;";

            return Convert.ToInt32(SqlHelper.ExecuteScalar(sql,
                new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                new SqlParameter("@CourseId", SqlDbType.Int) { Value = courseId })) > 0;
        }

        public int Insert(SqlConnection connection, SqlTransaction transaction, Certificate certificate)
        {
            const string sql =
                "INSERT INTO dbo.Certificates " +
                "(UserId, CourseId, QuizAttemptId, CertificateNumber, Title, IssuedAt) " +
                "OUTPUT INSERTED.Id VALUES " +
                "(@UserId, @CourseId, @QuizAttemptId, @CertificateNumber, @Title, @IssuedAt);";

            using (var command = SqlHelper.Prepare(connection, transaction, sql))
            {
                AddParameters(command, certificate);
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        public int Insert(Certificate certificate)
        {
            return SqlHelper.WithTransaction((connection, transaction) => Insert(connection, transaction, certificate));
        }

        /// <summary>CR-{course}-{learner}-{attempt}, the format the source application prints on the certificate.</summary>
        public static string FormatNumber(int courseId, int userId, int attemptId)
        {
            return string.Format("CR-{0:D3}-{1:D3}-{2:D5}", courseId, userId, attemptId);
        }

        public DataTable GetAllGrid()
        {
            const string sql =
                "SELECT ct.Id, ct.CertificateNumber, ct.Title, ct.IssuedAt, " +
                "       u.FullName, u.Username, c.Title AS CourseTitle " +
                "FROM dbo.Certificates AS ct " +
                "INNER JOIN dbo.Users AS u ON u.Id = ct.UserId " +
                "INNER JOIN dbo.Courses AS c ON c.Id = ct.CourseId " +
                "ORDER BY ct.IssuedAt DESC;";

            return SqlHelper.GetTable(sql);
        }

        public int CountAll()
        {
            return Convert.ToInt32(SqlHelper.ExecuteScalar("SELECT COUNT(1) FROM dbo.Certificates;"));
        }

        // ---------------------------------------------- explicit dependent deletes --

        public static void DeleteByUser(SqlConnection connection, SqlTransaction transaction, int userId)
        {
            Delete(connection, transaction, "UserId", userId);
        }

        public static void DeleteByCourse(SqlConnection connection, SqlTransaction transaction, int courseId)
        {
            Delete(connection, transaction, "CourseId", courseId);
        }

        public static void DeleteByQuiz(SqlConnection connection, SqlTransaction transaction, int quizId)
        {
            const string sql =
                "DELETE FROM dbo.Certificates " +
                "WHERE QuizAttemptId IN (SELECT Id FROM dbo.QuizAttempts WHERE QuizId = @QuizId);";

            using (var command = SqlHelper.Prepare(connection, transaction, sql))
            {
                SqlHelper.AddInt(command, "@QuizId", quizId);
                command.ExecuteNonQuery();
            }
        }

        public static void DeleteByAttempt(SqlConnection connection, SqlTransaction transaction, int attemptId)
        {
            Delete(connection, transaction, "QuizAttemptId", attemptId);
        }

        public void Delete(int certificateId)
        {
            const string sql = "DELETE FROM dbo.Certificates WHERE Id = @CertificateId;";

            using (var connection = DbConnectionFactory.Open())
            using (var command = SqlHelper.Prepare(connection, null, sql))
            {
                SqlHelper.AddInt(command, "@CertificateId", certificateId);
                command.ExecuteNonQuery();
            }
        }

        private static void Delete(SqlConnection connection, SqlTransaction transaction, string column, int value)
        {
            // column is an internal constant chosen by the caller, never application input.
            var sql = "DELETE FROM dbo.Certificates WHERE " + column + " = @Value;";

            using (var command = SqlHelper.Prepare(connection, transaction, sql))
            {
                SqlHelper.AddInt(command, "@Value", value);
                command.ExecuteNonQuery();
            }
        }

        private static void AddParameters(SqlCommand command, Certificate certificate)
        {
            SqlHelper.AddInt(command, "@UserId", certificate.UserId);
            SqlHelper.AddInt(command, "@CourseId", certificate.CourseId);
            SqlHelper.AddInt(command, "@QuizAttemptId", certificate.QuizAttemptId);
            SqlHelper.AddNVarChar(command, "@CertificateNumber", certificate.CertificateNumber, 40);
            SqlHelper.AddNVarChar(command, "@Title", certificate.Title, 160);
            SqlHelper.AddDateTime(command, "@IssuedAt",
                certificate.IssuedAt == default(DateTime) ? DateTime.UtcNow : certificate.IssuedAt);
        }

        internal static Certificate Map(SqlDataReader reader)
        {
            return new Certificate
            {
                Id = SqlHelper.GetInt(reader, "Id"),
                UserId = SqlHelper.GetInt(reader, "UserId"),
                CourseId = SqlHelper.GetInt(reader, "CourseId"),
                QuizAttemptId = SqlHelper.GetInt(reader, "QuizAttemptId"),
                CertificateNumber = SqlHelper.GetString(reader, "CertificateNumber"),
                Title = SqlHelper.GetString(reader, "Title"),
                IssuedAt = SqlHelper.GetDateTime(reader, "IssuedAt")
            };
        }
    }
}
