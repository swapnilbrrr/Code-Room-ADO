using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using CodeRoom.WebForms.Data;
using CodeRoom.WebForms.Helpers;
using CodeRoom.WebForms.Models;
using CodeRoom.WebForms.Services;

namespace CodeRoom.WebForms.Dashboard
{
    public partial class MyDashboard : Page
    {
        private readonly UserRepository users = new UserRepository();
        private readonly CourseRepository courses = new CourseRepository();
        private readonly ProgressRepository progress = new ProgressRepository();
        private readonly QuizAttemptRepository attempts = new QuizAttemptRepository();
        private readonly AnnouncementRepository announcements = new AnnouncementRepository();
        private readonly ActivityRepository activities = new ActivityRepository();
        private readonly CertificateRepository certificates = new CertificateRepository();

        protected Literal WelcomeName;
        protected Literal UsernameText;
        protected Literal LevelText;
        protected Literal XpText;
        protected Literal LevelProgressText;
        protected Literal CoursesCount;
        protected Literal LessonsCount;
        protected Literal QuizAverage;
        protected Literal StreakCount;
        protected Literal CertificateCount;
        protected Literal PracticeLabel;
        protected HtmlGenericControl XpBar;
        protected PlaceHolder CoursesEmpty;
        protected PlaceHolder RecommendationsEmpty;
        protected PlaceHolder RecentActivityEmpty;
        protected PlaceHolder AttemptsEmpty;
        protected PlaceHolder AnnouncementsEmpty;
        protected Repeater CoursesRepeater;
        protected Repeater RecommendationsRepeater;
        protected Repeater ActivityDaysRepeater;
        protected Repeater RecentActivityRepeater;
        protected Repeater AttemptsRepeater;
        protected Repeater AnnouncementsRepeater;

        public string PracticeUrl { get; private set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Auth.RequireLogin(this))
            {
                return;
            }

            if (Auth.IsAdmin)
            {
                Response.Redirect(ResolveUrl("~/Admin/Dashboard.aspx"), false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            if (IsPostBack)
            {
                return;
            }

            LoadDashboard();
        }

        protected string Encode(object value)
        {
            return HttpUtility.HtmlEncode(Convert.ToString(value) ?? string.Empty);
        }

        private void LoadDashboard()
        {
            var user = users.GetById(Auth.CurrentUserId);
            if (user == null)
            {
                Auth.SignOut();
                Response.Redirect(ResolveUrl("~/Authentication/Login.aspx"), false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            var enrolledCourses = courses.GetEnrolledCourses(user.Id);
            var enrolledRows = enrolledCourses
                .Take(4)
                .Select(course => new DashboardCourseRow
                {
                    CourseId = course.Id,
                    Title = course.Title,
                    Category = course.Category,
                    Level = course.Level,
                    TotalLessons = course.Lessons == null ? 0 : course.Lessons.Count,
                    CompletedLessons = progress.CountCompletedInCourse(user.Id, course.Id)
                })
                .ToList();

            foreach (var row in enrolledRows)
            {
                row.Percent = row.TotalLessons == 0
                    ? 0
                    : (int)Math.Round(row.CompletedLessons * 100.0 / row.TotalLessons);
            }

            var enrolledIds = new HashSet<int>(enrolledCourses.Select(c => c.Id));
            var totalLessons = enrolledCourses.Sum(c => c.Lessons == null ? 0 : c.Lessons.Count);
            var completedLessons = enrolledCourses.Sum(c => progress.CountCompletedInCourse(user.Id, c.Id));

            EnsureEnrollmentActivities(user.Id, enrolledCourses);

            var activityFeed = activities.GetFeed(user.Id, 364, 5000);
            var streakActivities = activities.GetRecentForStreak(user.Id, 400);
            var recentAttempts = attempts.GetRecentForUser(user.Id, 5);
            var attemptCount = attempts.CountByUser(user.Id);
            var quizAverage = attempts.GetAveragePercentForUser(user.Id);

            var primaryCategory = enrolledCourses
                .Select(c => c.Category)
                .FirstOrDefault(category => !string.IsNullOrWhiteSpace(category));

            var recommendedPool = courses.GetPublishedExceptEnrolled(user.Id)
                .OrderByDescending(c => c.IsCertification)
                .ThenBy(c => c.Category)
                .ThenByDescending(c => c.CreatedAt)
                .Take(6)
                .ToList();

            var recommended = !string.IsNullOrWhiteSpace(primaryCategory)
                ? recommendedPool
                    .Where(c => string.Equals(c.Category, primaryCategory, StringComparison.OrdinalIgnoreCase))
                    .Concat(recommendedPool.Where(c => !string.Equals(c.Category, primaryCategory, StringComparison.OrdinalIgnoreCase)))
                    .Take(4)
                    .Select(c => new RecommendedCourseRow
                    {
                        CourseId = c.Id,
                        Title = c.Title,
                        IsCertification = c.IsCertification,
                        Reason = string.Equals(c.Category, primaryCategory, StringComparison.OrdinalIgnoreCase)
                            ? "Because you're learning " + c.Category
                            : "Recommended for your learning path"
                    })
                    .ToList()
                : recommendedPool
                    .Take(4)
                    .Select(c => new RecommendedCourseRow
                    {
                        CourseId = c.Id,
                        Title = c.Title,
                        IsCertification = c.IsCertification,
                        Reason = "Recommended for your learning path"
                    })
                    .ToList();

            var activityDays = BuildActivityDays(activityFeed);
            var level = LearningActivityService.GetLevel(user.Xp);
            var levelProgress = LearningActivityService.GetLevelProgress(user.Xp);
            var learningStreak = LearningActivityService.CalculateStreak(streakActivities);

            WelcomeName.Text = Encode(FirstName(user.FullName));
            UsernameText.Text = Encode("@" + user.Username);
            LevelText.Text = level.ToString();
            XpText.Text = user.Xp.ToString();
            LevelProgressText.Text = levelProgress.ToString();
            XpBar.Style["width"] = Math.Min(100, Math.Max(0, levelProgress * 100 / 250)) + "%";

            CoursesCount.Text = enrolledCourses.Count.ToString();
            LessonsCount.Text = completedLessons.ToString();
            QuizAverage.Text = quizAverage.ToString();
            StreakCount.Text = learningStreak.ToString();
            CertificateCount.Text = certificates.CountByUser(user.Id);

            CoursesEmpty.Visible = enrolledRows.Count == 0;
            CoursesRepeater.Visible = enrolledRows.Count > 0;
            CoursesRepeater.DataSource = enrolledRows;
            CoursesRepeater.DataBind();

            RecommendationsEmpty.Visible = recommended.Count == 0;
            RecommendationsRepeater.Visible = recommended.Count > 0;
            RecommendationsRepeater.DataSource = recommended;
            RecommendationsRepeater.DataBind();

            ActivityDaysRepeater.DataSource = activityDays;
            ActivityDaysRepeater.DataBind();

            var recentActivity = activityFeed
                .OrderByDescending(a => a.CreatedAt)
                .Take(7)
                .Select(a => new RecentActivityRow
                {
                    Description = a.Description,
                    CreatedAtLocal = a.CreatedAt.ToLocalTime().ToString("dd MMM, HH:mm")
                })
                .ToList();

            RecentActivityEmpty.Visible = recentActivity.Count == 0;
            RecentActivityRepeater.Visible = recentActivity.Count > 0;
            RecentActivityRepeater.DataSource = recentActivity;
            RecentActivityRepeater.DataBind();

            var attemptRows = recentAttempts
                .Select(a => new AttemptRow
                {
                    Title = a.Quiz == null ? "Quiz" : a.Quiz.Title,
                    AttemptedAtLocal = a.AttemptedAt.ToLocalTime().ToString("dd MMM yyyy"),
                    Percent = a.TotalQuestions == 0
                        ? 0
                        : (int)Math.Round(a.Score * 100.0 / a.TotalQuestions)
                })
                .ToList();

            AttemptsEmpty.Visible = attemptRows.Count == 0;
            AttemptsRepeater.Visible = attemptRows.Count > 0;
            AttemptsRepeater.DataSource = attemptRows;
            AttemptsRepeater.DataBind();

            var publishedAnnouncements = announcements.GetPublished(3)
                .Select(a => new AnnouncementRow
                {
                    Title = a.Title,
                    Message = a.Message,
                    PublishedAtLocal = a.PublishedAt.ToLocalTime().ToString("dd MMM yyyy")
                })
                .ToList();

            AnnouncementsEmpty.Visible = publishedAnnouncements.Count == 0;
            AnnouncementsRepeater.Visible = publishedAnnouncements.Count > 0;
            AnnouncementsRepeater.DataSource = publishedAnnouncements;
            AnnouncementsRepeater.DataBind();

            if (enrolledCourses.Count > 0)
            {
                PracticeUrl = ResolveUrl("~/Challenges/Index.aspx?id=" + enrolledCourses[0].Id);
                PracticeLabel.Text = "Practice challenges";
            }
            else
            {
                PracticeUrl = ResolveUrl("~/Courses/Index.aspx");
                PracticeLabel.Text = "Find a practice path";
            }
        }

        private void EnsureEnrollmentActivities(int userId, IEnumerable<Course> enrolledCourses)
        {
            var existing = new HashSet<string>(
                activities.GetEnrollmentDescriptions(userId),
                StringComparer.Ordinal);

            var missing = enrolledCourses
                .Where(course => !existing.Contains("Enrolled in " + course.Title))
                .ToList();

            if (missing.Count == 0)
            {
                return;
            }

            SqlHelper.WithTransaction((connection, transaction) =>
            {
                foreach (var course in missing)
                {
                    ActivityRepository.Insert(connection, transaction, new UserActivity
                    {
                        UserId = userId,
                        ActivityType = DomainValues.ActivityType.CourseEnrolled,
                        Description = "Enrolled in " + course.Title,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            });
        }

        private static List<ActivityDayRow> BuildActivityDays(IEnumerable<UserActivity> activities)
        {
            var counts = activities
                .GroupBy(a => a.CreatedAt.Date)
                .ToDictionary(g => g.Key, g => g.Count());

            var start = DateTime.UtcNow.Date.AddDays(-364);
            return Enumerable.Range(0, 365)
                .Select(offset =>
                {
                    var date = start.AddDays(offset);
                    var count = counts.ContainsKey(date) ? counts[date] : 0;
                    var level = count == 0 ? 0 : count == 1 ? 1 : count <= 3 ? 2 : count <= 6 ? 3 : 4;

                    return new ActivityDayRow
                    {
                        Level = level,
                        Tooltip = date.ToString("dd MMM yyyy") + ": " + count + " activities"
                    };
                })
                .ToList();
        }

        private static string FirstName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                return "Learner";
            }

            return fullName.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0];
        }

        private sealed class DashboardCourseRow
        {
            public int CourseId { get; set; }
            public string Title { get; set; }
            public string Category { get; set; }
            public string Level { get; set; }
            public int TotalLessons { get; set; }
            public int CompletedLessons { get; set; }
            public int Percent { get; set; }
        }

        private sealed class RecommendedCourseRow
        {
            public int CourseId { get; set; }
            public string Title { get; set; }
            public bool IsCertification { get; set; }
            public string Reason { get; set; }
        }

        private sealed class ActivityDayRow
        {
            public int Level { get; set; }
            public string Tooltip { get; set; }
        }

        private sealed class RecentActivityRow
        {
            public string Description { get; set; }
            public string CreatedAtLocal { get; set; }
        }

        private sealed class AttemptRow
        {
            public string Title { get; set; }
            public string AttemptedAtLocal { get; set; }
            public int Percent { get; set; }
        }

        private sealed class AnnouncementRow
        {
            public string Title { get; set; }
            public string Message { get; set; }
            public string PublishedAtLocal { get; set; }
        }
    }
}
