using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using CodeRoom.WebForms.Data;
using CodeRoom.WebForms.Helpers;
using CodeRoom.WebForms.Models;
using CodeRoom.WebForms.Services;

namespace CodeRoom.WebForms.Admin
{
    public partial class LessonsEdit : Page
    {
        protected HiddenField LessonId;
        protected DropDownList CourseId, ContentType;
        protected TextBox Order, Title, SummaryText, DurationMinutes, Content, VideoUrl, AudioUrl, ResourceUrl;
        protected CheckBox IsPublished;
        protected Button SaveButton;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Auth.RequireAdmin(this)) return;
            if (!IsPostBack)
            {
                int id;if(!int.TryParse(Request.QueryString["id"],out id)||id<=0){ShowNotFound();return;}
                var lesson=new LessonRepository().GetDetailedById(id);if(lesson==null){ShowNotFound();return;}
                BindCourses(lesson.CourseId);BindLesson(lesson);
            }
        }

        private void BindCourses(int selected){CourseId.Items.Clear();CourseId.Items.Add(new ListItem("Select a course",""));foreach(var c in new CourseRepository().GetAll())CourseId.Items.Add(new ListItem(c.Title,c.Id.ToString()));CourseId.SelectedValue=selected.ToString();}
        private void BindLesson(Lesson l){LessonId.Value=l.Id.ToString();Order.Text=l.Order.ToString();Title.Text=l.Title;SummaryText.Text=l.Summary??"";ContentType.SelectedValue=l.ContentType;DurationMinutes.Text=l.DurationMinutes.ToString();Content.Text=l.Content;VideoUrl.Text=l.VideoUrl??"";AudioUrl.Text=l.AudioUrl??"";ResourceUrl.Text=l.ResourceUrl??"";IsPublished.Checked=l.IsPublished;}
        protected void SaveButton_Click(object sender,EventArgs e)
        {
            if(Auth.RequireAdmin(this))return;try{((SiteMaster)Master).ValidateCsrf();}catch(InvalidOperationException){return;}Page.Validate();if(!Page.IsValid)return;
            if(!SafeUrl.IsAllowed(VideoUrl.Text)||!SafeUrl.IsAllowed(AudioUrl.Text)||!SafeUrl.IsAllowed(ResourceUrl.Text)){Toast.Error("Unsafe media link","Video, audio and resource URLs must start with http://, https:// or /.");return;}
            int id;if(!int.TryParse(LessonId.Value,out id)||id<=0){ShowNotFound();return;}
            var repo=new LessonRepository();var existing=repo.GetDetailedById(id);if(existing==null){ShowNotFound();return;}
            var courseId=int.Parse(CourseId.SelectedValue);if(new CourseRepository().GetById(courseId)==null)return;
            var lesson=BuildLesson(id,courseId);
            using(var connection=DbConnectionFactory.Open())using(var tx=connection.BeginTransaction()){var module=EnsureModule(connection,tx,courseId);lesson.CourseModuleId=module.Id;LessonRepository.Update(connection,tx,lesson);tx.Commit();}
            AdminAuditService.Record(Auth.CurrentUserId,DomainValues.AuditAction.Updated,"Lesson",lesson.Title,"Updated lesson "+lesson.Title+".");
            Response.Redirect(ResolveUrl("~/Admin/Lessons.aspx?courseId="+courseId),false);Context.ApplicationInstance.CompleteRequest();
        }
        private Lesson BuildLesson(int id,int courseId){return new Lesson{Id=id,CourseId=courseId,Title=Title.Text.Trim(),Summary=string.IsNullOrWhiteSpace(SummaryText.Text)?null:SummaryText.Text.Trim(),Content=Content.Text,ContentType=ContentType.SelectedValue,VideoUrl=NullIfBlank(VideoUrl.Text),AudioUrl=NullIfBlank(AudioUrl.Text),ResourceUrl=NullIfBlank(ResourceUrl.Text),Order=int.Parse(Order.Text),DurationMinutes=int.Parse(DurationMinutes.Text),IsPublished=IsPublished.Checked};}
        private CourseModule EnsureModule(global::System.Data.SqlClient.SqlConnection c,global::System.Data.SqlClient.SqlTransaction t,int courseId)
        {
            const string sql = "SELECT TOP (1) m.Id, m.CourseId, m.Title, m.Description, m.ModuleOrder " +
                "FROM dbo.CourseModules AS m LEFT JOIN dbo.Lessons AS l ON l.CourseModuleId = m.Id " +
                "WHERE m.CourseId = @CourseId GROUP BY m.Id, m.CourseId, m.Title, m.Description, m.ModuleOrder " +
                "ORDER BY COUNT(l.Id), m.ModuleOrder, m.Id;";
            var module = SqlHelper.ReadFirst(c, t, sql, CourseModuleRepository.Map,
                new global::System.Data.SqlClient.SqlParameter("@CourseId", global::System.Data.SqlDbType.Int) { Value = courseId });
            if (module != null) return module;

            var created = new CourseModule { CourseId = courseId, Title = "Module 1 — Foundations",
                Description = "Foundational concepts and guided practice.", Order = 1 };
            created.Id = new CourseModuleRepository().Insert(c, t, created);
            return created;
        }
        private static string NullIfBlank(string v){return string.IsNullOrWhiteSpace(v)?null:v.Trim();}
        protected void ValidateOrder(object s,ServerValidateEventArgs a){int v;a.IsValid=int.TryParse(a.Value,out v)&&v>=1&&v<=999;}
        protected void ValidateTitle(object s,ServerValidateEventArgs a){var v=(a.Value??"").Trim();a.IsValid=v.Length>=3&&v.Length<=150;}
        protected void ValidateDuration(object s,ServerValidateEventArgs a){int v;a.IsValid=int.TryParse(a.Value,out v)&&v>=1&&v<=600;}
        private void ShowNotFound(){Response.StatusCode=404;Response.TrySkipIisCustomErrors=true;Response.Redirect(ResolveUrl("~/Error.aspx"),false);Context.ApplicationInstance.CompleteRequest();}
    }
}