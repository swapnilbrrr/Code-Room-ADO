using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CodeRoom.WebForms.Models;

namespace CodeRoom.WebForms.Data
{
    /// <summary>
    /// ADO.NET access to Achievements and UserAchievements. Awarding is guarded by the unique
    /// (UserId, AchievementId) index, so a repeated award attempt is a no-op rather than an error.
    /// </summary>
    public class AchievementRepository
    {
        public List<Achievement> GetAll()
        {
            const string sql =
                "SELECT Id, Name, Description, Code, Icon, XpReward FROM dbo.Achievements ORDER BY Id;";

            return SqlHelper.ReadList(sql, MapAchievement);
        }

        public Achievement GetByCode(string code)
        {
            const string sql =
                "SELECT Id, Name, Description, Code, Icon, XpReward " +
                "FROM dbo.Achievements WHERE Code = @Code;";

            return SqlHelper.ReadFirst(sql, MapAchievement,
                new SqlParameter("@Code", SqlDbType.NVarChar, 40) { Value = code });
        }

        public List<UserAchievement> GetByUser(int userId, int take = 12)
        {
            const string sql =
                "SELECT TOP (@Take) ua.Id, ua.UserId, ua.AchievementId, ua.EarnedAt, " +
                "       a.Name, a.Description, a.Code, a.Icon, a.XpReward " +
                "FROM dbo.UserAchievements AS ua " +
                "INNER JOIN dbo.Achievements AS a ON a.Id = ua.AchievementId " +
                "WHERE ua.UserId = @UserId " +
                "ORDER BY ua.EarnedAt DESC;";

            return SqlHelper.ReadList(sql, MapAward,
                new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                new SqlParameter("@Take", SqlDbType.Int) { Value = take });
        }

        public bool HasUserAchievement(int userId, string code)
        {
            const string sql =
                "SELECT COUNT(1) FROM dbo.UserAchievements AS ua " +
                "INNER JOIN dbo.Achievements AS a ON a.Id = ua.AchievementId " +
                "WHERE ua.UserId = @UserId AND a.Code = @Code;";

            return Convert.ToInt32(SqlHelper.ExecuteScalar(sql,
                new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                new SqlParameter("@Code", SqlDbType.NVarChar, 40) { Value = code })) > 0;
        }

        /// <summary>
        /// Award inside the caller's transaction. Returns the achievement when this call unlocked it, or
        /// null when the learner already owned it, which tells the caller whether the reward XP applies.
        /// </summary>
        public Achievement Award(SqlConnection connection, SqlTransaction transaction, int userId, string code)
        {
            var achievement = GetByCode(connection, transaction, code);

            if (achievement == null)
            {
                return null;
            }

            const string sql =
                "IF NOT EXISTS (SELECT 1 FROM dbo.UserAchievements " +
                "               WHERE UserId = @UserId AND AchievementId = @AchievementId) " +
                "BEGIN " +
                "    INSERT INTO dbo.UserAchievements (UserId, AchievementId, EarnedAt) " +
                "    VALUES (@UserId, @AchievementId, @EarnedAt); " +
                "END " +
                "ELSE SELECT 0;";

            using (var command = SqlHelper.Prepare(connection, transaction, sql))
            {
                SqlHelper.AddInt(command, "@UserId", userId);
                SqlHelper.AddInt(command, "@AchievementId", achievement.Id);
                SqlHelper.AddDateTime(command, "@EarnedAt", DateTime.UtcNow);

                var result = command.ExecuteScalar();
                var alreadyOwned = result != null && result != SqlHelper.Null && Convert.ToInt32(result) == 0;

                return alreadyOwned ? null : achievement;
            }
        }

        public Achievement Award(int userId, string code)
        {
            return SqlHelper.WithTransaction((connection, transaction) => Award(connection, transaction, userId, code));
        }

        /// <summary>
        /// Inserts a badge definition when its code is still free. Used by the seeder so a repeated
        /// start-up cannot create duplicate achievements against the unique Code index.
        /// </summary>
        public int InsertIfMissing(SqlConnection connection, SqlTransaction transaction, Achievement achievement)
        {
            const string sql =
                "IF NOT EXISTS (SELECT 1 FROM dbo.Achievements WHERE Code = @Code) " +
                "BEGIN " +
                "    INSERT INTO dbo.Achievements (Name, Description, Code, Icon, XpReward) " +
                "    VALUES (@Name, @Description, @Code, @Icon, @XpReward); " +
                "    SELECT CAST(SCOPE_IDENTITY() AS INT); " +
                "END " +
                "ELSE SELECT 0;";

            using (var command = SqlHelper.Prepare(connection, transaction, sql))
            {
                SqlHelper.AddNVarChar(command, "@Name", achievement.Name, 80);
                SqlHelper.AddNVarChar(command, "@Description", achievement.Description, 200);
                SqlHelper.AddNVarChar(command, "@Code", achievement.Code, 40);
                SqlHelper.AddNVarChar(command, "@Icon", achievement.Icon, 20);
                SqlHelper.AddInt(command, "@XpReward", achievement.XpReward);

                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        public int CountAll()
        {
            return Convert.ToInt32(SqlHelper.ExecuteScalar("SELECT COUNT(1) FROM dbo.Achievements;"));
        }

        private static Achievement GetByCode(SqlConnection connection, SqlTransaction transaction, string code)
        {
            const string sql = "SELECT Id, Name, Description, Code, Icon, XpReward FROM dbo.Achievements WHERE Code = @Code;";

            return SqlHelper.ReadFirst(connection, transaction, sql, MapAchievement,
                new SqlParameter("@Code", SqlDbType.NVarChar, 40) { Value = code });
        }

        internal static Achievement MapAchievement(SqlDataReader reader)
        {
            return new Achievement
            {
                Id = SqlHelper.GetInt(reader, "Id"),
                Name = SqlHelper.GetString(reader, "Name"),
                Description = SqlHelper.GetString(reader, "Description"),
                Code = SqlHelper.GetString(reader, "Code"),
                Icon = SqlHelper.GetString(reader, "Icon"),
                XpReward = SqlHelper.GetInt(reader, "XpReward")
            };
        }

        internal static UserAchievement MapAward(SqlDataReader reader)
        {
            return new UserAchievement
            {
                Id = SqlHelper.GetInt(reader, "Id"),
                UserId = SqlHelper.GetInt(reader, "UserId"),
                AchievementId = SqlHelper.GetInt(reader, "AchievementId"),
                EarnedAt = SqlHelper.GetDateTime(reader, "EarnedAt"),
                Achievement = new Achievement
                {
                    Id = SqlHelper.GetInt(reader, "AchievementId"),
                    Name = SqlHelper.GetString(reader, "Name"),
                    Description = SqlHelper.GetString(reader, "Description"),
                    Code = SqlHelper.GetString(reader, "Code"),
                    Icon = SqlHelper.GetString(reader, "Icon"),
                    XpReward = SqlHelper.GetInt(reader, "XpReward")
                }
            };
        }
    }
}
