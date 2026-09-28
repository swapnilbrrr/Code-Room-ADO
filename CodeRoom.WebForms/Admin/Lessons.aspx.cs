using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using CodeRoom.WebForms.Data;
using CodeRoom.WebForms.Helpers;
using CodeRoom.WebForms.Services;

namespace CodeRoom.WebForms.Admin
{
    public partial class Lessons : Page
    {
        protected DropDownList CourseFilter;
        protected Repeater LessonsRepeater;
        protected Panel EmptyPanel;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Auth.RequireAdmin(this)) return;
            if (!IsPostBack)
            {
                BindCourseFilter();
                BindLessons();
            }
        }

        private void BindCourseFilter()
        {
            CourseFilter.Items.Clear();
            CourseFilter.Items.Add(new ListItem("All courses", ""));
            foreach (var course in new CourseRepository().GetAll())
                CourseFilter.Items.Add(new ListItem(course.Title, course.Id.ToString()));
            var raw = Request.QueryString["courseId"];
            if (!string.IsNullOrWhiteSpace(raw) && CourseFilter.Items.FindByValue(raw) != null)
                CourseFilter.SelectedValue = raw;
        }

        protected void CourseFilter_Changed(object sender, EventArgs e)
        {
            BindLessons();
        }

        private void BindLessons()
        {
            int courseId;
            var raw = CourseFilter.SelectedValue;
            var list = !int.TryParse(raw, out courseId) || courseId <= 0
                ? new LessonRepository().GetAllByCourse(0)
                : new LessonRepository().GetAllByCourse(courseId);
            if (!int.TryParse(raw, out courseId) || courseId <= 0)
            {
                var courses = new CourseRepository().GetAll();
                list = courses.SelectMany(c => new LessonRepository().GetAllByCourse(c.Id).Select(l => { l.Course = c; return l; })).OrderBy(l => l.Course.Title).ThenBy(l => l.Order).ToList();
            }
            else
            {
                var course = new CourseRepository().GetById(courseId);
                foreach (var lesson in list) lesson.Course = course;
            }
            LessonsRepeater.DataSource = list;
            LessonsRepeater.DataBind();
            EmptyPanel.Visible = list.Count == 0;
        }

        protected void DeleteButton_Command(object sender, CommandEventArgs e)
        {
            if (Auth.RequireAdmin(this)) return;
            try { ((SiteMaster)Master).ValidateCsrf(); } catch (InvalidOperationException) { return; }
            int id; if (!int.TryParse(Convert.ToString(e.CommandArgument), out id) || id <= 0) return;
            var repo = new LessonRepository();
            var lesson = repo.GetDetailedById(id);
            if (lesson == null) return;
            repo.Delete(id);
            AdminAuditService.Record(Auth.CurrentUserId, DomainValues.AuditAction.Deleted, "Lesson", lesson.Title, "Deleted lesson " + lesson.Title + ".");
            Response.Redirect(ResolveUrl("~/Admin/Lessons.aspx"), false);
            Context.ApplicationInstance.CompleteRequest();
        }
    }
}