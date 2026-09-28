using System;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;
using CodeRoom.WebForms.Data;
using CodeRoom.WebForms.Helpers;
using CodeRoom.WebForms.Services;

namespace CodeRoom.WebForms.Admin
{
    public partial class Users : Page
    {
        protected Repeater UsersRepeater;
        protected void Page_Load(object s,EventArgs e){if(Auth.RequireAdmin(this))return;if(!IsPostBack)Bind();}
        private void Bind(){var users=new UserRepository().GetAll();UsersRepeater.DataSource=users;UsersRepeater.DataBind();}
        protected string FormatDate(object v){return Convert.ToDateTime(v).ToLocalTime().ToString("dd MMM yyyy",CultureInfo.InvariantCulture);}
        protected void DeleteButton_Command(object s,CommandEventArgs e){if(Auth.RequireAdmin(this))return;try{((SiteMaster)Master).ValidateCsrf();}catch(InvalidOperationException){return;}int id;if(!int.TryParse(Convert.ToString(e.CommandArgument),out id)||id<=0)return;if(id==Auth.CurrentUserId){Toast.Error("Action blocked","You cannot delete your own account.");return;}var repo=new UserRepository();var user=repo.GetById(id);if(user==null)return;if(user.IsSuperAdmin&&!Auth.IsSuperAdmin){Toast.Error("Action blocked","Only a Super Administrator can delete that account.");return;}repo.Delete(id);AdminAuditService.Record(Auth.CurrentUserId,DomainValues.AuditAction.Deleted,"User",user.Username,"Deleted account "+user.FullName+".");Response.Redirect(ResolveUrl("~/Admin/Users.aspx"),false);Context.ApplicationInstance.CompleteRequest();}
    }
}