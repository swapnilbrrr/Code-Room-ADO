using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using CodeRoom.WebForms.Data;
using CodeRoom.WebForms.Helpers;
using CodeRoom.WebForms.Models;
using CodeRoom.WebForms.Services;

namespace CodeRoom.WebForms.Admin
{
    public partial class ResourcesEdit : Page
    {
        protected HiddenField ResourceId;
        protected TextBox Title, Url;
        protected DropDownList Type, CourseId;
        protected Button SaveButton;
        protected void Page_Load(object s,EventArgs e){if(Auth.RequireAdmin(this))return;if(!IsPostBack){int id;if(!int.TryParse(Request.QueryString["id"],out id)||id<=0){ShowNotFound();return;}var r=new ResourceRepository().GetById(id);if(r==null){ShowNotFound();return;}BindCourses(r.CourseId);ResourceId.Value=r.Id.ToString();Title.Text=r.Title;Url.Text=r.Url;Type.SelectedValue=r.Type;}}
        private void BindCourses(int? selected){CourseId.Items.Clear();CourseId.Items.Add(new ListItem("General (no course)",""));foreach(var c in new CourseRepository().GetAll())CourseId.Items.Add(new ListItem(c.Title,c.Id.ToString()));if(selected.HasValue)CourseId.SelectedValue=selected.Value.ToString();}
        protected void SaveButton_Click(object s,EventArgs e){if(Auth.RequireAdmin(this))return;try{((SiteMaster)Master).ValidateCsrf();}catch(InvalidOperationException){return;}Page.Validate();if(!Page.IsValid)return;int id;if(!int.TryParse(ResourceId.Value,out id)||id<=0){ShowNotFound();return;}var repo=new ResourceRepository();var r=repo.GetById(id);if(r==null){ShowNotFound();return;}r.Title=Title.Text.Trim();r.Url=Url.Text.Trim();r.Type=Type.SelectedValue;r.CourseId=ParseNullable(CourseId.SelectedValue);repo.Update(r);AdminAuditService.Record(Auth.CurrentUserId,DomainValues.AuditAction.Updated,"Resource",r.Title,"Updated resource "+r.Title+".");Response.Redirect(ResolveUrl("~/Admin/Resources.aspx"),false);Context.ApplicationInstance.CompleteRequest();}
        protected void ValidateUrl(object s,ServerValidateEventArgs a){Uri u;a.IsValid=Uri.TryCreate((a.Value??"").Trim(),UriKind.Absolute,out u)&&(u.Scheme==Uri.UriSchemeHttp||u.Scheme==Uri.UriSchemeHttps)&&(a.Value??"").Trim().Length<=400;}
        private static int? ParseNullable(string v){int x;return int.TryParse(v,out x)&&x>0?(int?)x:null;}
        private void ShowNotFound(){Response.StatusCode=404;Response.TrySkipIisCustomErrors=true;Response.Redirect(ResolveUrl("~/Error.aspx"),false);Context.ApplicationInstance.CompleteRequest();}
    }
}