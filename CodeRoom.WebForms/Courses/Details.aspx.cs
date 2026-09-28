using System;
using System.Collections.Generic;
using System.Web.UI;
using CodeRoom.WebForms.Data;
using CodeRoom.WebForms.Helpers;
using CodeRoom.WebForms.Models;
using CodeRoom.WebForms.Services;

namespace CodeRoom.WebForms.Courses
{
    public partial class Details : Page
    {
        private readonly CourseRepository courses = new CourseRepository();
        private readonly EnrollmentRepository enrollments = new EnrollmentRepository();
        private readonly QuizRepository quizzes = new QuizRepository();
        private readonly ResourceRepository resources = new ResourceRepository();
        private readonly LearningActivityService learningActivity = new LearningActivityService();

        protected Course CourseModel { get; private set; }
        protected Quiz QuizModel { get; private set; }
        protected List<Resource> Resources { get; private set; }
        protected bool IsAuthenticated { get { return Auth.IsLoggedIn; } }
        protected bool IsEnrolled { get; private set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            LoadCourse();

            if (!IsPostBack && CourseModel != null && IsAuthenticated)
            {
                EnsureEnrollmentToken();
            }

            DataBind();
        }

        protected void EnrollButton_Click(object sender, EventArgs e)
        {
            if (!Auth.IsLoggedIn)
            {
                RedirectToLogin();
                return;
            }

            try
            {
                var master = Master as CodeRoom.WebForms.SiteMaster;
                if (master == null)
                {
                    throw new InvalidOperationException("The shared page security context is unavailable.");
                }

                master.ValidateCsrf();

                if (CourseModel == null || !CourseModel.IsPublished)
                {
                    ShowNotFound();
                    return;
                }

                var userId = Auth.CurrentUserId;
                var enrolledNow = learningActivity.Enroll(
                    userId,
                    CourseModel.Id,
                    CourseModel.Title,
                    CourseModel.Category,
                    ResolveUrl("~/Lessons/Index.aspx?id=" + CourseModel.Id));

                if (enrolledNow)
                {
                    var totalEnrollments = enrollments.CountByUser(userId);
                    var firstCourse = totalEnrollments == 1;
                    Toast.Set(
                        firstCourse ? "Achievement unlocked" : "Course enrolled",
                        firstCourse
                            ? "First course unlocked. Your Code-Room journey has started."
                            : "You're ready to start " + CourseModel.Title + ".",
                        firstCourse ? "🏆" : "✓");
                }
                Response.Redirect(ResolveUrl("~/Lessons/Index.aspx?id=" + CourseModel.Id), false);
                Context.ApplicationInstance.CompleteRequest();
            }
            catch (InvalidOperationException ex)
            {
                Toast.Set("Unable to enrol", ex.Message, "!");
            }
        }

        protected string ContentTypeIcon(object value)
        {
            switch ((Convert.ToString(value) ?? string.Empty))
            {
                case "Video": return "▶";
                case "Audio": return "♫";
                case "Challenge": return "⚡";
                case "Assessment": return "◎";
                case "Final Exam": return "🎓";
                default: return "▤";
            }
        }

        private void LoadCourse()
        {
            int id;
            if (!int.TryParse(Request.QueryString["id"], out id) || id <= 0)
            {
                ShowNotFound();
                return;
            }

            CourseModel = courses.GetDetails(id);
            if (CourseModel == null)
            {
                ShowNotFound();
                return;
            }

            QuizModel = quizzes.GetByCourse(id);
            Resources = resources.GetByCourse(id, 8);
            IsEnrolled = IsAuthenticated && enrollments.IsEnrolled(Auth.CurrentUserId, id);
        }

        private void EnsureEnrollmentToken()
        {
            var token = FindHiddenFieldRecursive(this, "CourseCsrfToken");
            if (token != null)
            {
                Csrf.EnsureToken(token);
            }
        }

        private static System.Web.UI.WebControls.HiddenField FindHiddenFieldRecursive(Control root, string id)
        {
            foreach (Control child in root.Controls)
            {
                if (child.ID == id)
                {
                    var field = child as System.Web.UI.WebControls.HiddenField;
                    if (field != null) return field;
                }

                var nested = FindHiddenFieldRecursive(child, id);
                if (nested != null) return nested;
            }

            return null;
        }

        private void RedirectToLogin()
        {
            var rawId = Convert.ToString(Request.QueryString["id"]);
            var returnUrl = ResolveUrl("~/Courses/Details.aspx?id=" + Server.UrlEncode(rawId));
            Response.Redirect(ResolveUrl("~/Authentication/Login.aspx?returnUrl=" + Server.UrlEncode(returnUrl)), false);
            Context.ApplicationInstance.CompleteRequest();
        }

        private void ShowNotFound()
        {
            CourseContent.Visible = false;
            CourseNotFound.Visible = true;
            Resources = new List<Resource>();
            CourseModel = null;
        }
    }
}
