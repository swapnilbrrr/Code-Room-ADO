using System;
using System.Web.UI;
using CodeRoom.WebForms.Data;
using CodeRoom.WebForms.Helpers;
using CodeRoom.WebForms.Models;

namespace CodeRoom.WebForms.Challenges
{
    public partial class Index : Page
    {
        protected System.Web.UI.WebControls.Literal CourseTitle;
        protected System.Web.UI.WebControls.Panel NotFoundPanel;
        protected System.Web.UI.WebControls.Panel EmptyPanel;
        protected System.Web.UI.WebControls.Repeater ChallengesRepeater;
        protected int CourseId { get; private set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Auth.RequireLogin(this)) return;

            int courseId;
            if (!int.TryParse(Request.QueryString["id"], out courseId) || courseId <= 0)
            {
                ShowNotFound();
                return;
            }

            var course = new CourseRepository().GetById(courseId);
            if (course == null || !course.IsPublished)
            {
                ShowNotFound();
                return;
            }

            CourseId = course.Id;
            CourseTitle.Text = course.Title;

            if (!Auth.IsAdmin && !new EnrollmentRepository().IsEnrolled(Auth.CurrentUserId, course.Id))
            {
                Toast.Error("Enrollment required", "Enroll in this course before opening the practice lab.");
                Response.Redirect(ResolveUrl("~/Courses/Details.aspx?id=" + course.Id), false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            var challenges = ChallengeRepository.GetByCourse(course.Id);
            ChallengesRepeater.DataSource = challenges;
            ChallengesRepeater.DataBind();
            EmptyPanel.Visible = challenges.Count == 0;
        }

        private void ShowNotFound()
        {
            Response.StatusCode = 404;
            Response.TrySkipIisCustomErrors = true;
            NotFoundPanel.Visible = true;
            EmptyPanel.Visible = false;
        }
    }
}