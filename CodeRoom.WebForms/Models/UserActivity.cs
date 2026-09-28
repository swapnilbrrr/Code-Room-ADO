using System;

namespace CodeRoom.WebForms.Models
{
    /// <summary>An entry in a learner's activity feed. Also drives streaks.</summary>
    public class UserActivity
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string ActivityType { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }

        public UserActivity()
        {
            ActivityType = string.Empty;
            Description = string.Empty;
            CreatedAt = DateTime.UtcNow;
        }
    }
}
