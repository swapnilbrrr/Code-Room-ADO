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
    public partial class AnnouncementsCreate : Page
    {
        protected TextBox Title, Message, PublishedAt;
        protected CheckBox IsPublished;
        protected Button SaveButton;
        protected void Page_Load(object s,EventArgs e){if(Auth.RequireAdmin(this))return;if(!IsPostBack)PublishedAt.Text=DateTime.UtcNow.ToString("yyyy-MM-dd",CultureInfo.InvariantCulture);}
        protected void SaveButton_Click(object s,EventArgs e){if(Auth.RequireAdmin(this))return;try{((SiteMaster)Master).ValidateCsrf();}catch(InvalidOperationException){return;}Page.Validate();if(!Page.IsValid)return;var item=new Announcement{Title=Title.Text.Trim(),Message=Message.Text.Trim(),PublishedAt=ParseDate(PublishedAt.Text),IsPublished=IsPublished.Checked};item.Id=new AnnouncementRepository().Insert(item);AdminAuditService.Record(Auth.CurrentUserId,DomainValues.AuditAction.Created,"Announcement",item.Title,"Created announcement "+item.Title+".");if(item.IsPublished)new AnnouncementRepository().Broadcast(item.Title,item.Message,"/Notifications/Index.aspx");Response.Redirect(ResolveUrl("~/Admin/Announcements.aspx"),false);Context.ApplicationInstance.CompleteRequest();}
        private static DateTime ParseDate(string v){DateTime d;return DateTime.TryParseExact(v,"yyyy-MM-dd",CultureInfo.InvariantCulture,DateTimeStyles.AssumeUniversal|DateTimeStyles.AdjustToUniversal,out d)?d:DateTime.UtcNow;}
    }
}