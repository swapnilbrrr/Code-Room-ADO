using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using CodeRoom.WebForms.Data;
using CodeRoom.WebForms.Helpers;
using CodeRoom.WebForms.Services;

namespace CodeRoom.WebForms.Admin
{
    public partial class Challenges : Page
    {
        protected Repeater ChallengesRepeater;
        protected Panel EmptyPanel;
        protected void Page_Load(object s,EventArgs e){if(Auth.RequireAdmin(this))return;if(!IsPostBack)Bind();}
        private void Bind(){var table=new ChallengeRepository().GetAllGrid();ChallengesRepeater.DataSource=table;ChallengesRepeater.DataBind();EmptyPanel.Visible=table.Rows.Count==0;}
        protected void DeleteButton_Command(object s,CommandEventArgs e){if(Auth.RequireAdmin(this))return;try{((SiteMaster)Master).ValidateCsrf();}catch(InvalidOperationException){return;}int id;if(!int.TryParse(Convert.ToString(e.CommandArgument),out id)||id<=0)return;var repo=new ChallengeRepository();var c=repo.GetById(id);if(c==null)return;repo.Delete(id);AdminAuditService.Record(Auth.CurrentUserId,DomainValues.AuditAction.Deleted,"Challenge",c.Title,"Deleted practice challenge "+c.Title+".");Response.Redirect(ResolveUrl("~/Admin/Challenges.aspx"),false);Context.ApplicationInstance.CompleteRequest();}
    }
}