using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.UI;
using CodeRoom.WebForms.Data;
using CodeRoom.WebForms.Helpers;
using CodeRoom.WebForms.Models;
using CodeRoom.WebForms.Services;

namespace CodeRoom.WebForms.Profile
{
    public partial class Index : Page
    {
        protected System.Web.UI.WebControls.Panel AvatarImagePanel;
        protected System.Web.UI.WebControls.Image AvatarImage;
        protected System.Web.UI.WebControls.Panel AvatarInitialPanel;
        protected System.Web.UI.WebControls.Literal AvatarInitial;
        protected System.Web.UI.WebControls.Literal FullName, Username, RoleLabel, Bio;
        protected System.Web.UI.HtmlControls.HtmlAnchor DashboardButton;
        protected System.Web.UI.WebControls.Panel AdminPanel, StudentPanel;
        protected System.Web.UI.WebControls.Literal AdminUsers, AdminCourses, AdminAnnouncements, AdminAuditEvents, AdminCertificates;
        protected System.Web.UI.WebControls.Panel AdminEmpty;
        protected System.Web.UI.WebControls.Repeater AdminActivityRepeater;
        protected System.Web.UI.WebControls.Literal Level, Xp, XpUntilLevel, LevelLeft, LevelRight;
        protected System.Web.UI.HtmlControls.HtmlGenericControl XpBar;
        protected System.Web.UI.WebControls.Literal CoursesEnrolled, LessonsCompleted, QuizAttempts, LearningStreak, CertificateCount;
        protected System.Web.UI.WebControls.Repeater ActivityDaysRepeater, AchievementRepeater, RecentActivityRepeater;
        protected System.Web.UI.WebControls.Panel AchievementEmpty, RecentEmpty;
        protected System.Web.UI.WebControls.Literal Email, DetailUsername, DetailRole, ProgressPercent;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Auth.RequireLogin(this)) return;

            var user = new UserRepository().GetById(Auth.CurrentUserId);
            if (user == null)
            {
                Auth.SignOut();
                Response.Redirect(ResolveUrl("~/Authentication/Login.aspx"), false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            BindIdentity(user);
            if (user.IsAdmin) BindAdmin(user);
            else BindStudent(user);
        }

        private void BindIdentity(User user)
        {
            FullName.Text = Server.HtmlEncode(user.FullName);
            Username.Text = Server.HtmlEncode(user.Username);
            DetailUsername.Text = Server.HtmlEncode(user.Username);
            RoleLabel.Text = Server.HtmlEncode(user.Role);
            DetailRole.Text = Server.HtmlEncode(user.Role);
            Email.Text = Server.HtmlEncode(user.Email);

            var bio = string.IsNullOrWhiteSpace(user.Bio)
                ? (user.IsAdmin ? "Platform operations and governance." : "Build your learning identity as you learn, practise and grow.")
                : user.Bio;
            Bio.Text = Server.HtmlEncode(bio);

            AvatarInitial.Text = Server.HtmlEncode(string.IsNullOrWhiteSpace(user.FullName)
                ? "C"
                : user.FullName.Trim().Substring(0, 1).ToUpperInvariant());
            AvatarInitialPanel.Visible = string.IsNullOrWhiteSpace(user.AvatarUrl);
            AvatarImagePanel.Visible = !AvatarInitialPanel.Visible;
            if (AvatarImagePanel.Visible)
            {
                AvatarImage.ImageUrl = user.AvatarUrl;
                AvatarImage.AlternateText = user.FullName + " avatar";
            }

            DashboardButton.HRef = ResolveUrl(user.IsAdmin ? "~/Admin/Dashboard.aspx" : "~/Dashboard/MyDashboard.aspx");
            DashboardButton.InnerText = user.IsAdmin ? "Admin Console" : "Dashboard";
        }

        private void BindAdmin(User user)
        {
            AdminPanel.Visible = true;
            StudentPanel.Visible = false;

            var counts = new AdminRepository().GetDashboardCounts();
            var row = counts.Rows.Count == 0 ? null : counts.Rows[0];
            AdminUsers.Text = row == null ? "0" : Convert.ToString(row["Users"], CultureInfo.InvariantCulture);
            AdminCourses.Text = row == null ? "0" : Convert.ToString(row["Courses"], CultureInfo.InvariantCulture);
            AdminAnnouncements.Text = row == null ? "0" : Convert.ToString(row["Announcements"], CultureInfo.InvariantCulture);
            AdminCertificates.Text = row == null ? "0" : Convert.ToString(row["Certificates"], CultureInfo.InvariantCulture);
            AdminAuditEvents.Text = new AdminRepository().CountAuditEntriesForUser(user.Id).ToString(CultureInfo.InvariantCulture);

            var recent = new AdminRepository().GetRecentForUser(user.Id, 8);
            AdminActivityRepeater.DataSource = recent;
            AdminActivityRepeater.DataBind();
            AdminEmpty.Visible = recent.Count == 0;
        }

        private void BindStudent(User user)
        {
            AdminPanel.Visible = false;
            StudentPanel.Visible = true;

            var enrollments = new EnrollmentRepository().GetByUser(user.Id);
            var courseRepo = new CourseRepository();
            var lessonRepo = new LessonRepository();
            var totalLessons = 0;

            foreach (var enrollment in enrollments)
            {
                var course = courseRepo.GetById(enrollment.CourseId);
                if (course != null)
                    totalLessons += lessonRepo.GetAllByCourse(course.Id).Count;
            }

            var completed = new ProgressRepository().CountCompletedForUser(user.Id);
            var attempts = new QuizAttemptRepository().CountByUser(user.Id);
            var certificates = new CertificateRepository().CountByUser(user.Id);
            var activities = new ActivityRepository().GetFeed(user.Id, 364, 500);
            var streak = LearningActivityService.CalculateStreak(activities);
            var achievements = new AchievementRepository().GetByUser(user.Id, 12);
            var progress = totalLessons == 0 ? 0 : (int)Math.Round(completed * 100.0 / totalLessons);

            Level.Text = user.Level.ToString(CultureInfo.InvariantCulture);
            LevelLeft.Text = user.Level.ToString(CultureInfo.InvariantCulture);
            LevelRight.Text = (user.Level + 1).ToString(CultureInfo.InvariantCulture);
            Xp.Text = user.Xp.ToString(CultureInfo.InvariantCulture);
            XpUntilLevel.Text = (250 - user.LevelProgress).ToString(CultureInfo.InvariantCulture);
            XpBar.Style["width"] = ((user.LevelProgress / 250.0) * 100.0).ToString("0", CultureInfo.InvariantCulture) + "%";
            CoursesEnrolled.Text = enrollments.Count.ToString(CultureInfo.InvariantCulture);
            LessonsCompleted.Text = completed.ToString(CultureInfo.InvariantCulture);
            QuizAttempts.Text = attempts.ToString(CultureInfo.InvariantCulture);
            LearningStreak.Text = streak.ToString(CultureInfo.InvariantCulture);
            CertificateCount.Text = certificates.ToString(CultureInfo.InvariantCulture);
            ProgressPercent.Text = progress.ToString(CultureInfo.InvariantCulture);

            var counts = activities.GroupBy(a => a.CreatedAt.Date).ToDictionary(g => g.Key, g => g.Count());
            var start = DateTime.UtcNow.Date.AddDays(-364);
            var days = Enumerable.Range(0, 365).Select(i =>
            {
                var date = start.AddDays(i);
                var count = counts.ContainsKey(date) ? counts[date] : 0;
                var level = count == 0 ? 0 : count == 1 ? 1 : count <= 3 ? 2 : count <= 6 ? 3 : 4;
                return new ActivityDay
                {
                    Level = level,
                    Title = date.ToString("dd MMM yyyy", CultureInfo.InvariantCulture) + ": " + count + " activities"
                };
            }).ToList();

            ActivityDaysRepeater.DataSource = days;
            ActivityDaysRepeater.DataBind();
            AchievementRepeater.DataSource = achievements;
            AchievementRepeater.DataBind();
            AchievementEmpty.Visible = achievements.Count == 0;

            var recent = activities.OrderByDescending(a => a.CreatedAt).Take(8).ToList();
            RecentActivityRepeater.DataSource = recent;
            RecentActivityRepeater.DataBind();
            RecentEmpty.Visible = recent.Count == 0;
        }

        protected string FormatActivityDate(object value)
        {
            if (value == null || value == DBNull.Value) return string.Empty;
            return Convert.ToDateTime(value).ToLocalTime().ToString("dd MMM, HH:mm", CultureInfo.InvariantCulture);
        }

        private sealed class ActivityDay
        {
            public int Level { get; set; }
            public string Title { get; set; }
        }
    }
}