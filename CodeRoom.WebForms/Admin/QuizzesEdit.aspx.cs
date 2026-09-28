using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using CodeRoom.WebForms.Data;
using CodeRoom.WebForms.Helpers;
using CodeRoom.WebForms.Models;
using CodeRoom.WebForms.Services;

namespace CodeRoom.WebForms.Admin
{
    public partial class QuizzesEdit : Page
    {
        protected HiddenField QuizId;
        protected DropDownList CourseId, AssessmentType;
        protected TextBox Title, TimeLimit, PassingScore, Description;
        protected CheckBox IsCertificationExam;
        protected Button SaveButton;
        protected void Page_Load(object s,EventArgs e){if(Auth.RequireAdmin(this))return;if(!IsPostBack){int id;if(!int.TryParse(Request.QueryString["id"],out id)||id<=0){ShowNotFound();return;}var q=new QuizRepository().GetById(id);if(q==null){ShowNotFound();return;}BindCourses(q.CourseId);QuizId.Value=q.Id.ToString();Title.Text=q.Title;Description.Text=q.Description;AssessmentType.SelectedValue=q.AssessmentType;TimeLimit.Text=q.TimeLimitMinutes.ToString();PassingScore.Text=q.PassingScorePercent.ToString();IsCertificationExam.Checked=q.IsCertificationExam;}}
        private void BindCourses(int selected){CourseId.Items.Clear();CourseId.Items.Add(new ListItem("Select a course",""));foreach(var c in new CourseRepository().GetAll())CourseId.Items.Add(new ListItem(c.Title,c.Id.ToString()));CourseId.SelectedValue=selected.ToString();}
        protected void SaveButton_Click(object s,EventArgs e){if(Auth.RequireAdmin(this))return;try{((SiteMaster)Master).ValidateCsrf();}catch(InvalidOperationException){return;}Page.Validate();if(!Page.IsValid)return;int id;if(!int.TryParse(QuizId.Value,out id)||id<=0){ShowNotFound();return;}var repo=new QuizRepository();var existing=repo.GetById(id);if(existing==null){ShowNotFound();return;}int courseId=int.Parse(CourseId.SelectedValue);if(new CourseRepository().GetById(courseId)==null)return;var q=new Quiz{Id=id,CourseId=courseId,Title=Title.Text.Trim(),Description=Description.Text.Trim(),AssessmentType=AssessmentType.SelectedValue,TimeLimitMinutes=int.Parse(TimeLimit.Text),PassingScorePercent=int.Parse(PassingScore.Text),IsCertificationExam=IsCertificationExam.Checked};repo.Update(q);AdminAuditService.Record(Auth.CurrentUserId,DomainValues.AuditAction.Updated,"Quiz",q.Title,"Updated assessment "+q.Title+".");Response.Redirect(ResolveUrl("~/Admin/Quizzes.aspx"),false);Context.ApplicationInstance.CompleteRequest();}
        protected void ValidateTitle(object s,ServerValidateEventArgs a){var v=(a.Value??"").Trim();a.IsValid=v.Length>=3&&v.Length<=150;}
        protected void ValidateTime(object s,ServerValidateEventArgs a){int v;a.IsValid=int.TryParse(a.Value,out v)&&v>=0&&v<=180;}
        protected void ValidatePassing(object s,ServerValidateEventArgs a){int v;a.IsValid=int.TryParse(a.Value,out v)&&v>=0&&v<=100;}
        protected void ValidateDescription(object s,ServerValidateEventArgs a){a.IsValid=(a.Value??"").Length<=500;}
        private void ShowNotFound(){Response.StatusCode=404;Response.TrySkipIisCustomErrors=true;Response.Redirect(ResolveUrl("~/Error.aspx"),false);Context.ApplicationInstance.CompleteRequest();}
    }
}