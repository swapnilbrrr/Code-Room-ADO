using System;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;
using CodeRoom.WebForms.Data;
using CodeRoom.WebForms.Helpers;
using CodeRoom.WebForms.Services;

namespace CodeRoom.WebForms.Admin
{
    public partial class Announcements : Page
    {
        protected Repeater AnnouncementsRepeater;
        protected Panel EmptyPanel;
        protected void Page_Load(object s,EventArgs e){if(Auth.RequireAdmin(this))return;if(!IsPostBack)Bind();}
        private void Bind(){var items=new AnnouncementRepository().GetAll();AnnouncementsRepeater.DataSource=items;AnnouncementsRepeater.DataBind();EmptyPanel.Visible=items.Count==0;}
        protected string FormatDate(object value){return Convert.ToDateTime(value).ToString("dd MMM yyyy",CultureInfo.InvariantCulture);}
        protected void DeleteButton_Command(object s,CommandEventArgs e){if(Auth.RequireAdmin(this))return;try{((SiteMaster)Master).ValidateCsrf();}catch(InvalidOperationException){return;}int id;if(!int.TryParse(Convert.ToString(e.CommandArgument),out id)||id<=0)return;var repo=new AnnouncementRepository();var item=repo.GetById(id);if(item==null)return;repo.Delete(id);AdminAuditService.Record(Auth.CurrentUserId,DomainValues.AuditAction.Deleted,"Announcement",item.Title,"Deleted announcement "+item.Title+".");Response.Redirect(ResolveUrl("~/Admin/Announcements.aspx"),false);Context.ApplicationInstance.CompleteRequest();}
    }
}