using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CodeRoom.WebForms.Models;
using CodeRoom.WebForms.Services;

namespace CodeRoom.WebForms.Data
{
    /// <summary>
    /// ADO.NET access to the Users table. Every statement is parameterised; the write methods come in
    /// pairs - one that opens its own connection and one that joins a caller supplied transaction, so
    /// registration, profile updates and administrative changes can group several tables into one
    /// unit of work.
    /// </summary>
    public class UserRepository
    {
        private const string SelectColumns =
            "Id, FullName, Username, Email, PasswordHash, Bio, AvatarUrl, Role, Xp, " +
            "ThemePreference, ProfileVisibility, EmailNotificationsEnabled, CreatedAt";

        public User GetById(int userId)
        {
            const string sql = "SELECT " + SelectColumns + " FROM dbo.Users WHERE Id = @UserId;";

            using (var connection = DbConnectionFactory.Open())
            using (var command = SqlHelper.Prepare(connection, null, sql))
            {
                SqlHelper.AddInt(command, "@UserId", userId);
                return ReadOne(command);
            }
        }

        public User GetByEmail(string email)
        {
            const string sql = "SELECT " + SelectColumns + " FROM dbo.Users WHERE Email = @Email;";
            return GetBySingleValue(sql, "@Email", email, 120);
        }

        public User GetByUsername(string username)
        {
            const string sql = "SELECT " + SelectColumns + " FROM dbo.Users WHERE Username = @Username;";
            return GetBySingleValue(sql, "@Username", username, 60);
        }

        public bool EmailExists(string email, int? excludeUserId = null)
        {
            const string sql =
                "SELECT COUNT(1) FROM dbo.Users WHERE Email = @Email " +
                "AND (@ExcludeUserId IS NULL OR Id <> @ExcludeUserId);";

            return SqlHelper.Exists(sql,
                new SqlParameter("@Email", SqlDbType.NVarChar, 120) { Value = email },
                new SqlParameter("@ExcludeUserId", SqlDbType.Int) { Value = SqlHelper.Value(excludeUserId) });
        }

        public bool UsernameExists(string username, int? excludeUserId = null)
        {
            const string sql =
                "SELECT COUNT(1) FROM dbo.Users WHERE Username = @Username " +
                "AND (@ExcludeUserId IS NULL OR Id <> @ExcludeUserId);";

            return SqlHelper.Exists(sql,
                new SqlParameter("@Username", SqlDbType.NVarChar, 60) { Value = username },
                new SqlParameter("@ExcludeUserId", SqlDbType.Int) { Value = SqlHelper.Value(excludeUserId) });
        }

        // ---------------------------------------------------------------- writes --

        /// <summary>Insert outside of a transaction and return the generated identity.</summary>
        public int Insert(User user)
        {
            using (var connection = DbConnectionFactory.Open())
            using (var transaction = connection.BeginTransaction())
            {
                var id = Insert(connection, transaction, user);
                transaction.Commit();
                return id;
            }
        }

        /// <summary>
        /// Insert inside the caller's transaction. Registration uses this so the account row, its
        /// first activity and its welcome notification either all exist or none of them do.
        /// </summary>
        public int Insert(SqlConnection connection, SqlTransaction transaction, User user)
        {
            const string sql =
                "INSERT INTO dbo.Users " +
                "(FullName, Username, Email, PasswordHash, Bio, AvatarUrl, Role, Xp, " +
                " ThemePreference, ProfileVisibility, EmailNotificationsEnabled, CreatedAt) " +
                "OUTPUT INSERTED.Id " +
                "VALUES (@FullName, @Username, @Email, @PasswordHash, @Bio, @AvatarUrl, @Role, @Xp, " +
                "        @ThemePreference, @ProfileVisibility, @EmailNotificationsEnabled, @CreatedAt);";

            using (var command = SqlHelper.Prepare(connection, transaction, sql))
            {
                AddUserParameters(command, user);
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        /// <summary>Save the editable profile fields. The password hash is never touched here.</summary>
        public void Update(User user)
        {
            const string sql =
                "UPDATE dbo.Users SET FullName = @FullName, Username = @Username, Email = @Email, " +
                "Bio = @Bio, AvatarUrl = @AvatarUrl, Role = @Role, ThemePreference = @ThemePreference, " +
                "ProfileVisibility = @ProfileVisibility, " +
                "EmailNotificationsEnabled = @EmailNotificationsEnabled " +
                "WHERE Id = @Id;";

            using (var connection = DbConnectionFactory.Open())
            using (var command = SqlHelper.Prepare(connection, null, sql))
            {
                AddUserParameters(command, user);
                SqlHelper.AddInt(command, "@Id", user.Id);
                command.ExecuteNonQuery();
            }
        }

        public void UpdatePasswordHash(int userId, string passwordHash)
        {
            const string sql = "UPDATE dbo.Users SET PasswordHash = @PasswordHash WHERE Id = @UserId;";

            using (var connection = DbConnectionFactory.Open())
            using (var command = SqlHelper.Prepare(connection, null, sql))
            {
                SqlHelper.AddText(command, "@PasswordHash", passwordHash);
                SqlHelper.AddInt(command, "@UserId", userId);
                command.ExecuteNonQuery();
            }
        }

        public void UpdatePasswordHash(SqlConnection connection, SqlTransaction transaction, int userId, string passwordHash)
        {
            const string sql = "UPDATE dbo.Users SET PasswordHash = @PasswordHash WHERE Id = @UserId;";

            using (var command = SqlHelper.Prepare(connection, transaction, sql))
            {
                SqlHelper.AddText(command, "@PasswordHash", passwordHash);
                SqlHelper.AddInt(command, "@UserId", userId);
                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Award experience atomically in the database. Reading Xp into C# and writing it back would
        /// lose updates when a learner triggers two rewards at once, so the increment is done by SQL.
        /// </summary>
        public static int AddXp(SqlConnection connection, SqlTransaction transaction, int userId, int amount)
        {
            const string sql =
                "UPDATE dbo.Users SET Xp = Xp + @Amount WHERE Id = @UserId; " +
                "SELECT Xp FROM dbo.Users WHERE Id = @UserId;";

            using (var command = SqlHelper.Prepare(connection, transaction, sql))
            {
                SqlHelper.AddInt(command, "@Amount", Math.Max(0, amount));
                SqlHelper.AddInt(command, "@UserId", userId);
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        public int AddXp(int userId, int amount)
        {
            using (var connection = DbConnectionFactory.Open())
            {
                return AddXp(connection, null, userId, amount);
            }
        }

        /// <summary>
        /// Remove an account. The certificate rows are deleted first because SQL Server enforces the
        /// Certificates -> QuizAttempts link as NO ACTION, so they must go before their attempts and
        /// before the user that owns both.
        /// </summary>
        public void Delete(int userId)
        {
            SqlHelper.WithTransaction((connection, transaction) => Delete(connection, transaction, userId));
        }

        public void Delete(SqlConnection connection, SqlTransaction transaction, int userId)
        {
            CertificateRepository.DeleteByUser(connection, transaction, userId);

            const string sql = "DELETE FROM dbo.Users WHERE Id = @UserId;";

            using (var command = SqlHelper.Prepare(connection, transaction, sql))
            {
                SqlHelper.AddInt(command, "@UserId", userId);
                command.ExecuteNonQuery();
            }
        }

        // ---------------------------------------------------------------- reading --

        public List<User> GetAll(int take = 0)
        {
            var sql = "SELECT " + SelectColumns + " FROM dbo.Users ORDER BY Id DESC;";

            if (take > 0)
            {
                sql = "SELECT TOP (@Take) " + SelectColumns + " FROM dbo.Users ORDER BY Id DESC;";
                return SqlHelper.ReadList(sql, Map, new SqlParameter("@Take", SqlDbType.Int) { Value = take });
            }

            return SqlHelper.ReadList(sql, Map);
        }

        /// <summary>Administrative grid source: filterable, ordered and bound straight to a GridView.</summary>
        public DataTable SearchTable(string search, string role)
        {
            const string sql =
                "SELECT Id, FullName, Username, Email, Role, Xp, CreatedAt " +
                "FROM dbo.Users " +
                "WHERE (@Search IS NULL OR FullName LIKE @Like OR Username LIKE @Like OR Email LIKE @Like) " +
                "  AND (@Role IS NULL OR Role = @Role) " +
                "ORDER BY Id DESC;";

            var like = string.IsNullOrWhiteSpace(search) ? null : "%" + search.Trim() + "%";

            return SqlHelper.GetTable(sql,
                new SqlParameter("@Search", SqlDbType.NVarChar, 120) { Value = SqlHelper.Value(search?.Trim()) },
                new SqlParameter("@Like", SqlDbType.NVarChar, 130) { Value = (object)like ?? SqlHelper.Null },
                new SqlParameter("@Role", SqlDbType.NVarChar, 30) { Value = SqlHelper.Value(role) });
        }

        public int CountAll()
        {
            const string sql = "SELECT COUNT(1) FROM dbo.Users;";

            using (var connection = DbConnectionFactory.Open())
            using (var command = SqlHelper.Prepare(connection, null, sql))
            {
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        public int CountByRole(string role)
        {
            const string sql = "SELECT COUNT(1) FROM dbo.Users WHERE Role = @Role;";

            using (var connection = DbConnectionFactory.Open())
            using (var command = SqlHelper.Prepare(connection, null, sql))
            {
                SqlHelper.AddNVarChar(command, "@Role", role, 30);
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        /// <summary>Role names used by the administration drop downs.</summary>
        public static List<string> RoleOptions()
        {
            return new List<string> { Roles.Student, Roles.Admin, Roles.SuperAdmin };
        }

        // ----------------------------------------------------------------- mapping --

        internal static User Map(SqlDataReader reader)
        {
            return new User
            {
                Id = SqlHelper.GetInt(reader, "Id"),
                FullName = SqlHelper.GetString(reader, "FullName"),
                Username = SqlHelper.GetString(reader, "Username"),
                Email = SqlHelper.GetString(reader, "Email"),
                PasswordHash = SqlHelper.GetString(reader, "PasswordHash"),
                Bio = SqlHelper.GetNullableString(reader, "Bio"),
                AvatarUrl = SqlHelper.GetNullableString(reader, "AvatarUrl"),
                Role = SqlHelper.GetString(reader, "Role"),
                Xp = SqlHelper.GetInt(reader, "Xp"),
                ThemePreference = SqlHelper.GetString(reader, "ThemePreference"),
                ProfileVisibility = SqlHelper.GetString(reader, "ProfileVisibility"),
                EmailNotificationsEnabled = SqlHelper.GetBool(reader, "EmailNotificationsEnabled"),
                CreatedAt = SqlHelper.GetDateTime(reader, "CreatedAt")
            };
        }

        private static void AddUserParameters(SqlCommand command, User user)
        {
            SqlHelper.AddNVarChar(command, "@FullName", user.FullName, 80);
            SqlHelper.AddNVarChar(command, "@Username", user.Username, 60);
            SqlHelper.AddNVarChar(command, "@Email", user.Email, 120);
            SqlHelper.AddText(command, "@PasswordHash", user.PasswordHash);
            SqlHelper.AddNVarChar(command, "@Bio", user.Bio, 500);
            SqlHelper.AddNVarChar(command, "@AvatarUrl", user.AvatarUrl, 300);
            SqlHelper.AddNVarChar(command, "@Role", user.Role, 30);
            SqlHelper.AddInt(command, "@Xp", user.Xp);
            SqlHelper.AddNVarChar(command, "@ThemePreference", user.ThemePreference, 20);
            SqlHelper.AddNVarChar(command, "@ProfileVisibility", user.ProfileVisibility, 20);
            SqlHelper.AddBool(command, "@EmailNotificationsEnabled", user.EmailNotificationsEnabled);
            SqlHelper.AddDateTime(command, "@CreatedAt", user.CreatedAt == default(DateTime) ? DateTime.UtcNow : user.CreatedAt);
        }

        private User GetBySingleValue(string sql, string parameterName, string value, int size)
        {
            using (var connection = DbConnectionFactory.Open())
            using (var command = SqlHelper.Prepare(connection, null, sql))
            {
                SqlHelper.AddNVarChar(command, parameterName, value, size);
                return ReadOne(command);
            }
        }

        private static User ReadOne(SqlCommand command)
        {
            using (var reader = command.ExecuteReader())
            {
                return reader.Read() ? Map(reader) : null;
            }
        }
    }
}
