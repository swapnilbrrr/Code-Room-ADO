using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using CodeRoom.WebForms.Data;
using CodeRoom.WebForms.Helpers;
using CodeRoom.WebForms.Services;

namespace CodeRoom.WebForms.Admin
{
    public partial class Quizzes : Page
    {
        protected Repeater QuizzesRepeater;
        protected Panel EmptyPanel;
        protected void Page_Load(object sender, EventArgs e){if(Auth.RequireAdmin(this))return;if(!IsPostBack)Bind();}
        private void Bind(){var repo=new QuizRepository();var courses=new CourseRepository();var rows=repo.GetAllGrid().AsEnumerable().Select(r=>new{Id=r.Field<int>("Id"),Title=r.Field<string>("Title"),Course=new{Title=r.Field<string>("CourseTitle")},QuestionCount=r.Field<int>("QuestionCount")}).ToList();QuizzesRepeater.DataSource=rows;QuizzesRepeater.DataBind();EmptyPanel.Visible=rows.Count==0;}
        protected void DeleteButton_Command(object s,CommandEventArgs e){if(Auth.RequireAdmin(this))return;try{((SiteMaster)Master).ValidateCsrf();}catch(InvalidOperationException){return;}int id;if(!int.TryParse(Convert.ToString(e.CommandArgument),out id)||id<=0)return;var q=new QuizRepository().GetById(id);if(q==null)return;new QuizRepository().Delete(id);AdminAuditService.Record(Auth.CurrentUserId,DomainValues.AuditAction.Deleted,"Quiz",q.Title,"Deleted assessment "+q.Title+".");Response.Redirect(ResolveUrl("~/Admin/Quizzes.aspx"),false);Context.ApplicationInstance.CompleteRequest();}
    }
}