using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using CodeRoom.WebForms.Data;
using CodeRoom.WebForms.Helpers;
using CodeRoom.WebForms.Services;

namespace CodeRoom.WebForms.Admin
{
    public partial class Resources : Page
    {
        protected Repeater ResourcesRepeater;
        protected Panel EmptyPanel;
        protected void Page_Load(object sender,EventArgs e){if(Auth.RequireAdmin(this))return;if(!IsPostBack)Bind();}
        private void Bind(){var table=new ResourceRepository().GetAllGrid();ResourcesRepeater.DataSource=table;ResourcesRepeater.DataBind();EmptyPanel.Visible=table.Rows.Count==0;}
        protected void DeleteButton_Command(object s,CommandEventArgs e){if(Auth.RequireAdmin(this))return;try{((SiteMaster)Master).ValidateCsrf();}catch(InvalidOperationException){return;}int id;if(!int.TryParse(Convert.ToString(e.CommandArgument),out id)||id<=0)return;var repo=new ResourceRepository();var r=repo.GetById(id);if(r==null)return;repo.Delete(id);AdminAuditService.Record(Auth.CurrentUserId,DomainValues.AuditAction.Deleted,"Resource",r.Title,"Deleted resource "+r.Title+".");Response.Redirect(ResolveUrl("~/Admin/Resources.aspx"),false);Context.ApplicationInstance.CompleteRequest();}
    }
}