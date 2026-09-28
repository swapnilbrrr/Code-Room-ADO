using System;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;
using CodeRoom.WebForms.Data;
using CodeRoom.WebForms.Helpers;
using CodeRoom.WebForms.Services;

namespace CodeRoom.WebForms.Admin
{
    public partial class Courses : Page
    {
        protected Repeater CoursesRepeater;
        protected Panel EmptyPanel, SuccessPanel;
        protected Literal SuccessMessage;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Auth.RequireAdmin(this)) return;
            if (!IsPostBack) BindCourses();
        }

        private void BindCourses()
        {
            var courses = new CourseRepository().GetAll();
            foreach (var course in courses) course.Lessons = new LessonRepository().GetAllByCourse(course.Id);
            CoursesRepeater.DataSource = courses;
            CoursesRepeater.DataBind();
            EmptyPanel.Visible = courses.Count == 0;
        }

        protected void DeleteButton_Command(object sender, CommandEventArgs e)
        {
            if (Auth.RequireAdmin(this)) return;
            try { ((SiteMaster)Master).ValidateCsrf(); }
            catch (InvalidOperationException) { Toast.Error("Security check failed", "Refresh the page and try again."); return; }

            int id;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out id) || id <= 0) return;
            var repo = new CourseRepository();
            var course = repo.GetById(id);
            if (course == null) return;

            repo.Delete(id);
            AdminAuditService.Record(Auth.CurrentUserId, DomainValues.AuditAction.Deleted, "Course", course.Title, "Deleted course " + course.Title + ".");
            Toast.Success("Course deleted", course.Title + " was removed.");
            Response.Redirect(ResolveUrl("~/Admin/Courses.aspx"), false);
            Context.ApplicationInstance.CompleteRequest();
        }
    }
}