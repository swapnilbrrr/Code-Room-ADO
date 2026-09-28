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
    public partial class CoursesCreate : Page
    {
        protected TextBox Title, Slug, EstimatedMinutes, PassingScore, Description, CertificateName, ThumbnailUrl;
        protected DropDownList Category, Level;
        protected CheckBox IsCertification, IsPublished;
        protected Button SaveButton;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Auth.RequireAdmin(this)) return;
        }

        protected void SaveButton_Click(object sender, EventArgs e)
        {
            if (Auth.RequireAdmin(this)) return;
            try { ((SiteMaster)Master).ValidateCsrf(); } catch (InvalidOperationException) { return; }
            Page.Validate();
            if (!Page.IsValid) return;

            var slug = Slug.Text.Trim().ToLowerInvariant();
            var repo = new CourseRepository();
            if (repo.SlugExists(slug)) { AddError("This slug is already in use."); return; }

            var course = BuildCourse(0, slug);
            course.CreatedAt = DateTime.UtcNow;
            course.Id = repo.Insert(course);
            AdminAuditService.Record(Auth.CurrentUserId, DomainValues.AuditAction.Created, "Course", course.Title, "Created course " + course.Title + ".");
            Response.Redirect(ResolveUrl("~/Admin/Courses.aspx"), false);
            Context.ApplicationInstance.CompleteRequest();
        }

        protected void ValidateTitle(object source, ServerValidateEventArgs args) { var v=(args.Value??"").Trim(); args.IsValid=v.Length>=3&&v.Length<=120; }
        protected void ValidateSlug(object source, ServerValidateEventArgs args) { args.IsValid=Regex.IsMatch((args.Value??"").Trim(), "^[a-z0-9-]{1,140}$"); }
        protected void ValidateMinutes(object source, ServerValidateEventArgs args) { int v; args.IsValid=int.TryParse(args.Value,out v)&&v>=10&&v<=1000; }
        protected void ValidatePassing(object source, ServerValidateEventArgs args) { int v; args.IsValid=int.TryParse(args.Value,out v)&&v>=50&&v<=100; }
        protected void ValidateDescription(object source, ServerValidateEventArgs args) { var v=(args.Value??"").Trim(); args.IsValid=v.Length>=10&&v.Length<=1000; }
        protected void ValidateThumbnail(object source, ServerValidateEventArgs args) { args.IsValid=IsValidUrl(args.Value); }

        private Course BuildCourse(int id, string slug) { int minutes=int.Parse(EstimatedMinutes.Text); int passing=int.Parse(PassingScore.Text); return new Course { Id=id, Title=Title.Text.Trim(), Slug=slug, Description=Description.Text.Trim(), Category=Category.SelectedValue, Level=Level.SelectedValue, EstimatedMinutes=minutes, PassingScorePercent=passing, IsCertification=IsCertification.Checked, CertificateName=string.IsNullOrWhiteSpace(CertificateName.Text)?null:CertificateName.Text.Trim(), ThumbnailUrl=string.IsNullOrWhiteSpace(ThumbnailUrl.Text)?null:ThumbnailUrl.Text.Trim(), IsPublished=IsPublished.Checked, CreatedAt=DateTime.UtcNow }; }
        private void AddError(string message) { var v=new CustomValidator { IsValid=false, ErrorMessage=message }; Page.Validators.Add(v); }
        private static bool IsValidUrl(string value) { value=(value??"").Trim(); if(value=="") return true; Uri u; return Uri.TryCreate(value,UriKind.Absolute,out u)&&(u.Scheme==Uri.UriSchemeHttp||u.Scheme==Uri.UriSchemeHttps)&&value.Length<=300; }
    }
}