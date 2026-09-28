using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using CodeRoom.WebForms.Data;
using CodeRoom.WebForms.Helpers;
using CodeRoom.WebForms.Models;
using CodeRoom.WebForms.Services;

namespace CodeRoom.WebForms.Admin
{
    public partial class QuizzesCreate : Page
    {
        protected DropDownList CourseId, AssessmentType;
        protected TextBox Title, TimeLimit, PassingScore, Description;
        protected CheckBox IsCertificationExam;
        protected Button SaveButton;
        protected void Page_Load(object sender,EventArgs e){if(Auth.RequireAdmin(this))return;if(!IsPostBack)BindCourses();}
        private void BindCourses(){CourseId.Items.Clear();CourseId.Items.Add(new ListItem("Select a course",""));foreach(var c in new CourseRepository().GetAll())CourseId.Items.Add(new ListItem(c.Title,c.Id.ToString()));}
        protected void SaveButton_Click(object s,EventArgs e){if(Auth.RequireAdmin(this))return;try{((SiteMaster)Master).ValidateCsrf();}catch(InvalidOperationException){return;}Page.Validate();if(!Page.IsValid)return;int courseId=int.Parse(CourseId.SelectedValue);if(new CourseRepository().GetById(courseId)==null)return;var q=Build(0,courseId);q.Id=new QuizRepository().Insert(q);AdminAuditService.Record(Auth.CurrentUserId,DomainValues.AuditAction.Created,"Quiz",q.Title,"Created assessment "+q.Title+".");Response.Redirect(ResolveUrl("~/Admin/QuizzesManage.aspx?id="+q.Id),false);Context.ApplicationInstance.CompleteRequest();}
        private Quiz Build(int id,int courseId){return new Quiz{Id=id,CourseId=courseId,Title=Title.Text.Trim(),Description=Description.Text.Trim(),AssessmentType=AssessmentType.SelectedValue,TimeLimitMinutes=int.Parse(TimeLimit.Text),PassingScorePercent=int.Parse(PassingScore.Text),IsCertificationExam=IsCertificationExam.Checked};}
        protected void ValidateTitle(object s,ServerValidateEventArgs a){var v=(a.Value??"").Trim();a.IsValid=v.Length>=3&&v.Length<=150;}
        protected void ValidateTime(object s,ServerValidateEventArgs a){int v;a.IsValid=int.TryParse(a.Value,out v)&&v>=0&&v<=180;}
        protected void ValidatePassing(object s,ServerValidateEventArgs a){int v;a.IsValid=int.TryParse(a.Value,out v)&&v>=0&&v<=100;}
        protected void ValidateDescription(object s,ServerValidateEventArgs a){a.IsValid=(a.Value??"").Length<=500;}
    }
}