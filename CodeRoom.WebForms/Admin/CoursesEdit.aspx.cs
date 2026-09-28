using System;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using CodeRoom.WebForms.Data;
using CodeRoom.WebForms.Helpers;
using CodeRoom.WebForms.Models;
using CodeRoom.WebForms.Services;

namespace CodeRoom.WebForms.Admin
{
    public partial class CoursesEdit : Page
    {
        protected HiddenField CourseId;
        protected TextBox Title, Slug, EstimatedMinutes, PassingScore, Description, CertificateName, ThumbnailUrl;
        protected DropDownList Category, Level;
        protected CheckBox IsCertification, IsPublished;
        protected Button SaveButton;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Auth.RequireAdmin(this)) return;
            if (!IsPostBack)
            {
                int id; if (!int.TryParse(Request.QueryString["id"], out id) || id <= 0) { ShowNotFound(); return; }
                var course = new CourseRepository().GetById(id);
                if (course == null) { ShowNotFound(); return; }
                CourseId.Value = id.ToString();
                Title.Text=course.Title; Slug.Text=course.Slug; Description.Text=course.Description; Category.SelectedValue=course.Category; Level.SelectedValue=course.Level; EstimatedMinutes.Text=course.EstimatedMinutes.ToString(); PassingScore.Text=course.PassingScorePercent.ToString(); CertificateName.Text=course.CertificateName??""; ThumbnailUrl.Text=course.ThumbnailUrl??""; IsCertification.Checked=course.IsCertification; IsPublished.Checked=course.IsPublished;
            }
        }

        protected void SaveButton_Click(object sender, EventArgs e)
        {
            if (Auth.RequireAdmin(this)) return;
            try { ((SiteMaster)Master).ValidateCsrf(); } catch (InvalidOperationException) { return; }
            Page.Validate();
            if (!Page.IsValid) return;

            int id; if (!int.TryParse(CourseId.Value, out id) || id <= 0) { ShowNotFound(); return; }
            var repo=new CourseRepository(); var existing=repo.GetById(id); if(existing==null){ShowNotFound();return;}
            var slug=Slug.Text.Trim().ToLowerInvariant();
            if(repo.SlugExists(slug,id)){AddError("This slug is already in use.");return;}
            var course=new Course { Id=id, Title=Title.Text.Trim(), Slug=slug, Description=Description.Text.Trim(), Category=Category.SelectedValue, Level=Level.SelectedValue, EstimatedMinutes=int.Parse(EstimatedMinutes.Text), PassingScorePercent=int.Parse(PassingScore.Text), IsCertification=IsCertification.Checked, CertificateName=string.IsNullOrWhiteSpace(CertificateName.Text)?null:CertificateName.Text.Trim(), ThumbnailUrl=string.IsNullOrWhiteSpace(ThumbnailUrl.Text)?null:ThumbnailUrl.Text.Trim(), IsPublished=IsPublished.Checked, CreatedAt=existing.CreatedAt };
            repo.Update(course);
            AdminAuditService.Record(Auth.CurrentUserId, DomainValues.AuditAction.Updated, "Course", course.Title, "Updated course " + course.Title + ".");
            Response.Redirect(ResolveUrl("~/Admin/Courses.aspx"), false); Context.ApplicationInstance.CompleteRequest();
        }

        protected void ValidateTitle(object source, ServerValidateEventArgs args) { var v=(args.Value??"").Trim(); args.IsValid=v.Length>=3&&v.Length<=120; }
        protected void ValidateSlug(object source, ServerValidateEventArgs args) { args.IsValid=Regex.IsMatch((args.Value??"").Trim(), "^[a-z0-9-]{1,140}$"); }
        protected void ValidateMinutes(object source, ServerValidateEventArgs args) { int v; args.IsValid=int.TryParse(args.Value,out v)&&v>=10&&v<=1000; }
        protected void ValidatePassing(object source, ServerValidateEventArgs args) { int v; args.IsValid=int.TryParse(args.Value,out v)&&v>=50&&v<=100; }
        protected void ValidateDescription(object source, ServerValidateEventArgs args) { var v=(args.Value??"").Trim(); args.IsValid=v.Length>=10&&v.Length<=1000; }
        protected void ValidateThumbnail(object source, ServerValidateEventArgs args) { args.IsValid=IsValidUrl(args.Value); }
        private void AddError(string message){var v=new CustomValidator{IsValid=false,ErrorMessage=message};Page.Validators.Add(v);}
        private void ShowNotFound(){Response.StatusCode=404;Response.TrySkipIisCustomErrors=true;Response.Redirect(ResolveUrl("~/Error.aspx"),false);Context.ApplicationInstance.CompleteRequest();}
        private static bool IsValidUrl(string value){value=(value??"").Trim();if(value=="")return true;Uri u;return Uri.TryCreate(value,UriKind.Absolute,out u)&&(u.Scheme==Uri.UriSchemeHttp||u.Scheme==Uri.UriSchemeHttps)&&value.Length<=300;}
    }
}