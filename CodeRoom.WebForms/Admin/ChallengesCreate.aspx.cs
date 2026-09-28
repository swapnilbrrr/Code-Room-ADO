using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using CodeRoom.WebForms.Data;
using CodeRoom.WebForms.Helpers;
using CodeRoom.WebForms.Models;
using CodeRoom.WebForms.Services;

namespace CodeRoom.WebForms.Admin
{
    public partial class ChallengesCreate : Page
    {
        protected DropDownList CourseId, LessonId, ValidationMode;
        protected TextBox Title, Instructions, Points, ExpectedAnswer, StarterCode, Hint;
        protected Button SaveButton;
        protected void Page_Load(object s,EventArgs e){if(Auth.RequireAdmin(this))return;if(!IsPostBack){BindCourses();Points.Text="50";}}
        private void BindCourses(){CourseId.Items.Clear();CourseId.Items.Add(new ListItem("Select a course",""));foreach(var c in new CourseRepository().GetAll())CourseId.Items.Add(new ListItem(c.Title,c.Id.ToString()));LessonId.Items.Clear();LessonId.Items.Add(new ListItem("Link to a course-level challenge",""));}
        protected void CourseChanged(object s,EventArgs e){BindLessons(ParseNullable(CourseId.SelectedValue),null);}
        private void BindLessons(int? courseId,int? selected){LessonId.Items.Clear();LessonId.Items.Add(new ListItem("Link to a course-level challenge",""));if(!courseId.HasValue)return;foreach(var l in new LessonRepository().GetAllByCourse(courseId.Value))LessonId.Items.Add(new ListItem(l.Title,l.Id.ToString()));if(selected.HasValue)LessonId.SelectedValue=selected.Value.ToString();}
        protected void SaveButton_Click(object s,EventArgs e){if(Auth.RequireAdmin(this))return;try{((SiteMaster)Master).ValidateCsrf();}catch(InvalidOperationException){return;}Page.Validate();if(!Page.IsValid)return;int courseId=int.Parse(CourseId.SelectedValue);var course=new CourseRepository().GetById(courseId);if(course==null)return;var lessonId=ParseNullable(LessonId.SelectedValue);if(lessonId.HasValue&&new LessonRepository().GetDetailedById(lessonId.Value)?.CourseId!=courseId){AddError("Select a lesson from the chosen course.");return;}var c=new Challenge{CourseId=courseId,LessonId=lessonId,Title=Title.Text.Trim(),Instructions=Instructions.Text.Trim(),StarterCode=NullIfBlank(StarterCode.Text),Hint=NullIfBlank(Hint.Text),ExpectedAnswer=ExpectedAnswer.Text.Trim(),ValidationMode=ValidationMode.SelectedValue,Points=int.Parse(Points.Text)};c.Id=new ChallengeRepository().Insert(c);AdminAuditService.Record(Auth.CurrentUserId,DomainValues.AuditAction.Created,"Challenge",c.Title,"Created practice challenge "+c.Title+".");Response.Redirect(ResolveUrl("~/Admin/Challenges.aspx"),false);Context.ApplicationInstance.CompleteRequest();}
        protected void ValidateTitle(object s,ServerValidateEventArgs a){var v=(a.Value??"").Trim();a.IsValid=v.Length>=3&&v.Length<=160;}protected void ValidateInstructions(object s,ServerValidateEventArgs a){var v=(a.Value??"").Trim();a.IsValid=v.Length>=10&&v.Length<=1800;}protected void ValidatePoints(object s,ServerValidateEventArgs a){int v;a.IsValid=int.TryParse(a.Value,out v)&&v>=5&&v<=500;}
        private static int? ParseNullable(string v){int x;return int.TryParse(v,out x)&&x>0?(int?)x:null;}private static string NullIfBlank(string v){return string.IsNullOrWhiteSpace(v)?null:v.Trim();}private void AddError(string m){Page.Validators.Add(new CustomValidator{IsValid=false,ErrorMessage=m});}
    }
}