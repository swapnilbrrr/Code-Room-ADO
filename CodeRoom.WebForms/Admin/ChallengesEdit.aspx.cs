using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using CodeRoom.WebForms.Data;
using CodeRoom.WebForms.Helpers;
using CodeRoom.WebForms.Models;
using CodeRoom.WebForms.Services;

namespace CodeRoom.WebForms.Admin
{
    public partial class ChallengesEdit : Page
    {
        protected HiddenField ChallengeId;
        protected DropDownList CourseId, LessonId, ValidationMode;
        protected TextBox Title, Instructions, Points, ExpectedAnswer, StarterCode, Hint;
        protected Button SaveButton;
        protected void Page_Load(object s,EventArgs e){if(Auth.RequireAdmin(this))return;if(!IsPostBack){int id;if(!int.TryParse(Request.QueryString["id"],out id)||id<=0){ShowNotFound();return;}var c=new ChallengeRepository().GetById(id);if(c==null){ShowNotFound();return;}BindCourses(c.CourseId);BindLessons(c.CourseId,c.LessonId);Bind(c);}}
        private void BindCourses(int selected){CourseId.Items.Clear();CourseId.Items.Add(new ListItem("Select a course",""));foreach(var c in new CourseRepository().GetAll())CourseId.Items.Add(new ListItem(c.Title,c.Id.ToString()));CourseId.SelectedValue=selected.ToString();}
        private void BindLessons(int courseId,int? selected){LessonId.Items.Clear();LessonId.Items.Add(new ListItem("Link to a course-level challenge",""));foreach(var l in new LessonRepository().GetAllByCourse(courseId))LessonId.Items.Add(new ListItem(l.Title,l.Id.ToString()));if(selected.HasValue)LessonId.SelectedValue=selected.Value.ToString();}
        private void Bind(Challenge c){ChallengeId.Value=c.Id.ToString();Title.Text=c.Title;Instructions.Text=c.Instructions;ValidationMode.SelectedValue=c.ValidationMode;Points.Text=c.Points.ToString();ExpectedAnswer.Text=c.ExpectedAnswer;StarterCode.Text=c.StarterCode??"";Hint.Text=c.Hint??"";}
        protected void CourseChanged(object s,EventArgs e){int id;BindLessons(int.TryParse(CourseId.SelectedValue,out id)?id:0,null);}
        protected void SaveButton_Click(object s,EventArgs e){if(Auth.RequireAdmin(this))return;try{((SiteMaster)Master).ValidateCsrf();}catch(InvalidOperationException){return;}Page.Validate();if(!Page.IsValid)return;int id;if(!int.TryParse(ChallengeId.Value,out id)||id<=0){ShowNotFound();return;}var repo=new ChallengeRepository();var c=repo.GetById(id);if(c==null){ShowNotFound();return;}int courseId;if(!int.TryParse(CourseId.SelectedValue,out courseId)||new CourseRepository().GetById(courseId)==null)return;var lessonId=ParseNullable(LessonId.SelectedValue);if(lessonId.HasValue&&new LessonRepository().GetDetailedById(lessonId.Value)?.CourseId!=courseId){AddError("Select a lesson from the chosen course.");return;}c.CourseId=courseId;c.LessonId=lessonId;c.Title=Title.Text.Trim();c.Instructions=Instructions.Text.Trim();c.ValidationMode=ValidationMode.SelectedValue;c.Points=int.Parse(Points.Text);c.ExpectedAnswer=ExpectedAnswer.Text.Trim();c.StarterCode=NullIfBlank(StarterCode.Text);c.Hint=NullIfBlank(Hint.Text);repo.Update(c);AdminAuditService.Record(Auth.CurrentUserId,DomainValues.AuditAction.Updated,"Challenge",c.Title,"Updated practice challenge "+c.Title+".");Response.Redirect(ResolveUrl("~/Admin/Challenges.aspx"),false);Context.ApplicationInstance.CompleteRequest();}
        protected void ValidatePoints(object s,ServerValidateEventArgs a){int v;a.IsValid=int.TryParse(a.Value,out v)&&v>=5&&v<=500;}private static int? ParseNullable(string v){int x;return int.TryParse(v,out x)&&x>0?(int?)x:null;}private static string NullIfBlank(string v){return string.IsNullOrWhiteSpace(v)?null:v.Trim();}private void AddError(string m){Page.Validators.Add(new CustomValidator{IsValid=false,ErrorMessage=m});}private void ShowNotFound(){Response.StatusCode=404;Response.TrySkipIisCustomErrors=true;Response.Redirect(ResolveUrl("~/Error.aspx"),false);Context.ApplicationInstance.CompleteRequest();}
    }
}