using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using CodeRoom.WebForms.Data;
using CodeRoom.WebForms.Helpers;
using CodeRoom.WebForms.Models;
using CodeRoom.WebForms.Services;

namespace CodeRoom.WebForms.Admin
{
    public partial class ResourcesCreate : Page
    {
        protected TextBox Title, Url;
        protected DropDownList Type, CourseId;
        protected Button SaveButton;
        protected void Page_Load(object s,EventArgs e){if(Auth.RequireAdmin(this))return;if(!IsPostBack)BindCourses();}
        private void BindCourses(){CourseId.Items.Clear();CourseId.Items.Add(new ListItem("General (no course)",""));foreach(var c in new CourseRepository().GetAll())CourseId.Items.Add(new ListItem(c.Title,c.Id.ToString()));}
        protected void SaveButton_Click(object s,EventArgs e){if(Auth.RequireAdmin(this))return;try{((SiteMaster)Master).ValidateCsrf();}catch(InvalidOperationException){return;}Page.Validate();if(!Page.IsValid)return;int? courseId=ParseNullable(CourseId.SelectedValue);var r=new Resource{Title=Title.Text.Trim(),Url=Url.Text.Trim(),Type=Type.SelectedValue,CourseId=courseId};r.Id=new ResourceRepository().Insert(r);AdminAuditService.Record(Auth.CurrentUserId,DomainValues.AuditAction.Created,"Resource",r.Title,"Created resource "+r.Title+".");Response.Redirect(ResolveUrl("~/Admin/Resources.aspx"),false);Context.ApplicationInstance.CompleteRequest();}
        protected void ValidateUrl(object s,ServerValidateEventArgs a){Uri u;a.IsValid=Uri.TryCreate((a.Value??"").Trim(),UriKind.Absolute,out u)&&(u.Scheme==Uri.UriSchemeHttp||u.Scheme==Uri.UriSchemeHttps)&&(a.Value??"").Trim().Length<=400;}
        private static int? ParseNullable(string v){int x;return int.TryParse(v,out x)&&x>0?(int?)x:null;}
    }
}