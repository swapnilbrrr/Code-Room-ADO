using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using CodeRoom.WebForms.Data;
using CodeRoom.WebForms.Helpers;
using CodeRoom.WebForms.Models;
using CodeRoom.WebForms.Services;

namespace CodeRoom.WebForms.Lessons
{
    public partial class Index : Page
    {
        protected global::System.Web.UI.WebControls.PlaceHolder LessonNotFound;
        protected global::System.Web.UI.WebControls.PlaceHolder LessonContent;
        protected global::System.Web.UI.WebControls.Repeater ContentParagraphsRepeater;
        protected global::System.Web.UI.WebControls.HiddenField CompleteCsrfToken;
        protected global::System.Web.UI.WebControls.Button CompleteButton;
        protected global::System.Web.UI.WebControls.Repeater LessonsRepeater;

        private readonly CourseRepository courses = new CourseRepository();
        private readonly LessonRepository lessons = new LessonRepository();
        private readonly EnrollmentRepository enrollments = new EnrollmentRepository();
        private readonly ProgressRepository progress = new ProgressRepository();
        private readonly QuizRepository quizzes = new QuizRepository();
        private readonly ChallengeRepository challenges = new ChallengeRepository();
        private readonly LearningActivityService learningActivity = new LearningActivityService();

        protected Course CourseModel { get; private set; }
        protected List<Lesson> Lessons { get; private set; }
        protected Lesson CurrentLesson { get; private set; }
        protected HashSet<int> CompletedLessonIds { get; private set; }
        protected Quiz QuizModel { get; private set; }
        protected Challenge ChallengeModel { get; private set; }
        protected int ProgressPercent { get; private set; }
        protected int CompletedCount { get; private set; }
        protected int TotalCount { get { return Lessons == null ? 0 : Lessons.Count; } }
        protected Lesson PreviousLesson { get; private set; }
        protected Lesson NextLesson { get; private set; }
        protected string ModuleName { get; private set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Auth.RequireLogin(this))
                return;

            if (!LoadLesson())
                return;

            if (!IsPostBack)
            {
                EnsureCompletionToken();
            }

            ContentParagraphsRepeater.DataSource = BuildParagraphs(CurrentLesson.Content);
            LessonsRepeater.DataSource = Lessons;
            DataBind();
        }

        protected void CompleteButton_Click(object sender, EventArgs e)
        {
            if (Auth.RequireLogin(this))
                return;

            try
            {
                var master = Master as CodeRoom.WebForms.SiteMaster;
                if (master == null)
                    throw new InvalidOperationException("The shared page security context is unavailable.");

                master.ValidateCsrf();

                if (CurrentLesson == null || CourseModel == null)
                {
                    ShowNotFound();
                    return;
                }

                if (!enrollments.IsEnrolled(Auth.CurrentUserId, CourseModel.Id))
                {
                    Toast.Set("Enrollment required", "Enroll in this course before completing lessons.", "!");
                    Response.Redirect(
                        ResolveUrl("~/Courses/Details.aspx?id=" + CourseModel.Id),
                        false);
                    Context.ApplicationInstance.CompleteRequest();
                    return;
                }

                var firstCompletion = learningActivity.CompleteLesson(
                    Auth.CurrentUserId,
                    CurrentLesson.Id,
                    CourseModel.Id,
                    CurrentLesson.Title,
                    CourseModel.Title);

                if (firstCompletion)
                {
                    Toast.Set(
                        "Lesson completed",
                        "Progress saved. Keep your streak alive.",
                        "✓");
                }

                Response.Redirect(
                    ResolveUrl("~/Lessons/Index.aspx?id=" + CourseModel.Id + "&lessonId=" + CurrentLesson.Id),
                    false);
                Context.ApplicationInstance.CompleteRequest();
            }
            catch (InvalidOperationException ex)
            {
                Toast.Set("Unable to save progress", ex.Message, "!");
            }
        }

        protected class LessonParagraph
        {
            public bool IsSection { get; set; }
            public string Label { get; set; }
            public string Body { get; set; }
        }

        private static List<LessonParagraph> BuildParagraphs(string content)
        {
            var sectionLabels = new HashSet<string>(StringComparer.Ordinal)
            {
                "WHY IT MATTERS", "KEY CONCEPTS", "HOW TO THINK ABOUT IT",
                "WORKED EXAMPLE", "IN PRACTICE", "COMMON MISTAKES", "PRACTICE FOCUS",
                "CHECK YOURSELF", "KEY TAKEAWAY"
            };

            var paragraphs = new List<LessonParagraph>();
            var rawParagraphs = (content ?? string.Empty)
                .Split(new[] { "\r\n\r\n", "\n\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var raw in rawParagraphs)
            {
                var paragraph = raw.Trim();
                var separator = paragraph.IndexOf(':');
                var label = separator > 0 ? paragraph.Substring(0, separator).Trim() : string.Empty;

                if (sectionLabels.Contains(label))
                {
                    paragraphs.Add(new LessonParagraph
                    {
                        IsSection = true,
                        Label = label,
                        Body = paragraph.Substring(separator + 1).Trim()
                    });
                }
                else
                {
                    paragraphs.Add(new LessonParagraph
                    {
                        IsSection = false,
                        Label = string.Empty,
                        Body = paragraph
                    });
                }
            }

            return paragraphs;
        }

        protected string ContentTypeIcon(object value)
        {
            switch (Convert.ToString(value) ?? string.Empty)
            {
                case "Video": return "▶";
                case "Audio": return "♫";
                case "Challenge": return "⚡";
                case "Assessment": return "◎";
                case "Final Exam": return "🎓";
                default: return "▤";
            }
        }

        protected string LessonClass(object value)
        {
            var id = Convert.ToInt32(value);
            var classes = new List<string>();

            if (CurrentLesson != null && id == CurrentLesson.Id)
                classes.Add("active");

            if (CompletedLessonIds != null && CompletedLessonIds.Contains(id))
                classes.Add("completed");

            return string.Join(" ", classes);
        }

        protected string LessonStatus(object value)
        {
            var id = Convert.ToInt32(value);
            return CompletedLessonIds != null && CompletedLessonIds.Contains(id) ? "✓ " : string.Empty;
        }

        protected string LessonUrl(object value)
        {
            return ResolveUrl("~/Lessons/Index.aspx?id=" + CourseModel.Id + "&lessonId=" + Convert.ToInt32(value));
        }

        protected string ChallengeUrl()
        {
            return ChallengeModel == null
                ? string.Empty
                : ResolveUrl("~/Challenges/Take.aspx?id=" + ChallengeModel.Id);
        }

        protected string QuizUrl()
        {
            return QuizModel == null
                ? string.Empty
                : ResolveUrl("~/Quiz/Take.aspx?id=" + QuizModel.Id);
        }

        private bool LoadLesson()
        {
            int courseId;
            if (!int.TryParse(Request.QueryString["id"], out courseId) || courseId <= 0)
            {
                ShowNotFound();
                return false;
            }

            CourseModel = courses.GetDetails(courseId);
            if (CourseModel == null || !CourseModel.IsPublished)
            {
                ShowNotFound();
                return false;
            }

            if (!enrollments.IsEnrolled(Auth.CurrentUserId, courseId) && !Auth.IsAdmin)
            {
                Toast.Set("Enrollment required", "Enroll in this course to start the lesson.", "!");
                Response.Redirect(
                    ResolveUrl("~/Courses/Details.aspx?id=" + courseId),
                    false);
                Context.ApplicationInstance.CompleteRequest();
                return false;
            }

            Lessons = lessons.GetAllByCourse(courseId);
            if (Lessons.Count == 0)
            {
                ShowNotFound();
                return false;
            }

            CompletedLessonIds = new HashSet<int>(
                progress.GetCompletedLessonIds(Auth.CurrentUserId, courseId));

            int requestedLessonId;
            var hasRequestedLesson =
                int.TryParse(Request.QueryString["lessonId"], out requestedLessonId) &&
                requestedLessonId > 0;

            CurrentLesson = hasRequestedLesson
                ? Lessons.FirstOrDefault(l => l.Id == requestedLessonId)
                : null;

            if (CurrentLesson == null)
                CurrentLesson = Lessons.FirstOrDefault(l => !CompletedLessonIds.Contains(l.Id));

            if (CurrentLesson == null)
                CurrentLesson = Lessons.First();

            CurrentLesson.Content = LessonContentBuilder.EnsureRichContent(
                CurrentLesson.Content,
                CourseModel.Title,
                CurrentLesson.Title,
                CurrentLesson.Order);

            if (string.IsNullOrWhiteSpace(CurrentLesson.VideoUrl) && CurrentLesson.Order == 1)
                CurrentLesson.VideoUrl = LessonMediaCatalog.VideoFor(CourseModel.Title);

            if (string.IsNullOrWhiteSpace(CurrentLesson.ResourceUrl) && CurrentLesson.Order == 1)
                CurrentLesson.ResourceUrl = LessonMediaCatalog.ResourceFor(CourseModel.Title);

            var currentIndex = Lessons.FindIndex(l => l.Id == CurrentLesson.Id);
            PreviousLesson = currentIndex > 0 ? Lessons[currentIndex - 1] : null;
            NextLesson = currentIndex >= 0 && currentIndex < Lessons.Count - 1
                ? Lessons[currentIndex + 1]
                : null;

            CompletedCount = CompletedLessonIds.Count;
            ProgressPercent = TotalCount == 0
                ? 0
                : (int)Math.Round((CompletedCount * 100.0) / TotalCount);

            var module = CurrentLesson.CourseModuleId.HasValue
                ? CourseModel.Modules.FirstOrDefault(m => m.Id == CurrentLesson.CourseModuleId.Value)
                : null;
            ModuleName = module == null ? "Course module" : module.Title;

            QuizModel = quizzes.GetByCourse(courseId);
            ChallengeModel = challenges.GetFirstForLesson(CurrentLesson.Id)
                ?? CourseModel.Challenges.FirstOrDefault(c => !c.LessonId.HasValue);

            return true;
        }

        private void EnsureCompletionToken()
        {
            Csrf.EnsureToken(CompleteCsrfToken);
        }

        private void ShowNotFound()
        {
            Response.StatusCode = 404;
            Response.TrySkipIisCustomErrors = true;
            LessonContent.Visible = false;
            LessonNotFound.Visible = true;
            CourseModel = null;
            Lessons = new List<Lesson>();
            CompletedLessonIds = new HashSet<int>();
        }
    }
}
