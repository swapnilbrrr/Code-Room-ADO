using System;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;
using CodeRoom.WebForms.Data;
using CodeRoom.WebForms.Helpers;
using CodeRoom.WebForms.Models;
using CodeRoom.WebForms.Services;

namespace CodeRoom.WebForms.Admin
{
    public partial class AnnouncementsEdit : Page
    {
        protected HiddenField AnnouncementId;
        protected TextBox Title, Message, PublishedAt;
        protected CheckBox IsPublished;
        protected Button SaveButton;
        protected void Page_Load(object s,EventArgs e){if(Auth.RequireAdmin(this))return;if(!IsPostBack){int id;if(!int.TryParse(Request.QueryString["id"],out id)||id<=0){ShowNotFound();return;}var item=new AnnouncementRepository().GetById(id);if(item==null){ShowNotFound();return;}AnnouncementId.Value=item.Id.ToString();Title.Text=item.Title;Message.Text=item.Message;PublishedAt.Text=item.PublishedAt.ToString("yyyy-MM-dd",CultureInfo.InvariantCulture);IsPublished.Checked=item.IsPublished;}}
        protected void SaveButton_Click(object s,EventArgs e){if(Auth.RequireAdmin(this))return;try{((SiteMaster)Master).ValidateCsrf();}catch(InvalidOperationException){return;}Page.Validate();if(!Page.IsValid)return;int id;if(!int.TryParse(AnnouncementId.Value,out id)||id<=0){ShowNotFound();return;}var repo=new AnnouncementRepository();var item=repo.GetById(id);if(item==null){ShowNotFound();return;}var wasPublished=item.IsPublished;item.Title=Title.Text.Trim();item.Message=Message.Text.Trim();item.PublishedAt=ParseDate(PublishedAt.Text);item.IsPublished=IsPublished.Checked;repo.Update(item);AdminAuditService.Record(Auth.CurrentUserId,DomainValues.AuditAction.Updated,"Announcement",item.Title,"Updated announcement "+item.Title+".");if(!wasPublished&&item.IsPublished)new AnnouncementRepository().Broadcast(item.Title,item.Message,"/Notifications/Index.aspx");Response.Redirect(ResolveUrl("~/Admin/Announcements.aspx"),false);Context.ApplicationInstance.CompleteRequest();}
        private static DateTime ParseDate(string v){DateTime d;return DateTime.TryParseExact(v,"yyyy-MM-dd",CultureInfo.InvariantCulture,DateTimeStyles.AssumeUniversal|DateTimeStyles.AdjustToUniversal,out d)?d:DateTime.UtcNow;}
        private void ShowNotFound(){Response.StatusCode=404;Response.TrySkipIisCustomErrors=true;Response.Redirect(ResolveUrl("~/Error.aspx"),false);Context.ApplicationInstance.CompleteRequest();}
    }
}