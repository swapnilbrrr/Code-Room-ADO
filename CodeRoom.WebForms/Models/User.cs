using System;
using System.Collections.Generic;
using CodeRoom.WebForms.Services;

namespace CodeRoom.WebForms.Models
{
    /// <summary>A platform account. Authentication is custom and session backed.</summary>
    public class User
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Bio { get; set; }
        public string AvatarUrl { get; set; }
        public string Role { get; set; }
        public int Xp { get; set; }
        public string ThemePreference { get; set; }
        public string ProfileVisibility { get; set; }
        public bool EmailNotificationsEnabled { get; set; }
        public DateTime CreatedAt { get; set; }

        public List<Enrollment> Enrollments { get; set; }
        public List<Progress> Progress { get; set; }
        public List<QuizAttempt> QuizAttempts { get; set; }
        public List<UserActivity> Activities { get; set; }
        public List<Notification> Notifications { get; set; }
        public List<UserAchievement> UserAchievements { get; set; }
        public List<Certificate> Certificates { get; set; }
        public List<AdminAuditLog> AuditLogs { get; set; }

        public User()
        {
            FullName = string.Empty;
            Username = string.Empty;
            Email = string.Empty;
            PasswordHash = string.Empty;
            Role = Roles.Student;
            ThemePreference = "system";
            ProfileVisibility = "Public";
            EmailNotificationsEnabled = true;
            CreatedAt = DateTime.UtcNow;
            Enrollments = new List<Enrollment>();
            Progress = new List<Progress>();
            QuizAttempts = new List<QuizAttempt>();
            Activities = new List<UserActivity>();
            Notifications = new List<Notification>();
            UserAchievements = new List<UserAchievement>();
            Certificates = new List<Certificate>();
            AuditLogs = new List<AdminAuditLog>();
        }

        public bool IsAdmin { get { return Role == Roles.Admin || Role == Roles.SuperAdmin; } }

        public bool IsSuperAdmin { get { return Role == Roles.SuperAdmin; } }

        public int Level { get { return Math.Max(1, (Xp / 250) + 1); } }

        public int LevelProgress { get { return Xp % 250; } }
    }
}
