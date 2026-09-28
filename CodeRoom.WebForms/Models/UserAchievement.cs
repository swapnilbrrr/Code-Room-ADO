using System;

namespace CodeRoom.WebForms.Models
{
    /// <summary>The record of one learner unlocking one achievement.</summary>
    public class UserAchievement
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int AchievementId { get; set; }
        public DateTime EarnedAt { get; set; }

        public Achievement Achievement { get; set; }

        public UserAchievement()
        {
            EarnedAt = DateTime.UtcNow;
        }
    }
}
