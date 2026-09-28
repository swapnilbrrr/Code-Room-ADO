using System;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using CodeRoom.WebForms.Data;
using CodeRoom.WebForms.Helpers;
using CodeRoom.WebForms.Models;
using CodeRoom.WebForms.Services;

namespace CodeRoom.WebForms.Admin
{
    public partial class UsersEdit : Page
    {
        protected HiddenField UserId;
        protected TextBox FullName, Username, Email, Password;
        protected DropDownList Role;
        protected Button SaveButton;
        protected void Page_Load(object s,EventArgs e){if(Auth.RequireAdmin(this))return;if(!IsPostBack){int id;if(!int.TryParse(Request.QueryString["id"],out id)||id<=0){ShowNotFound();return;}var u=new UserRepository().GetById(id);if(u==null){ShowNotFound();return;}UserId.Value=u.Id.ToString();FullName.Text=u.FullName;Username.Text=u.Username;Email.Text=u.Email;Role.SelectedValue=u.Role;}}
        protected void SaveButton_Click(object s,EventArgs e)
        {
            if(Auth.RequireAdmin(this))return;try{((SiteMaster)Master).ValidateCsrf();}catch(InvalidOperationException){return;}Page.Validate();if(!Page.IsValid)return;
            int id;if(!int.TryParse(UserId.Value,out id)||id<=0){ShowNotFound();return;}var repo=new UserRepository();var existing=repo.GetById(id);if(existing==null){ShowNotFound();return;}
            var username=Username.Text.Trim().ToLowerInvariant();var email=Email.Text.Trim().ToLowerInvariant();
            if(repo.UsernameExists(username,id))AddError("This username is already in use.");if(repo.EmailExists(email,id))AddError("This email is already registered.");if(Role.SelectedValue!="Student"&&!Auth.IsSuperAdmin)AddError("Only a Super Administrator can grant that role.");
            if(existing.Id==Auth.CurrentUserId&&!string.Equals(Role.SelectedValue,existing.Role,StringComparison.Ordinal)){AddError("You cannot change your own administrator role.");}
            if(existing.IsSuperAdmin&&!Auth.IsSuperAdmin){Toast.Error("Action blocked","Only a Super Administrator can modify a Super Administrator account.");return;}
            if(!Page.IsValid)return;
            existing.FullName=FullName.Text.Trim();existing.Username=username;existing.Email=email;existing.Role=Role.SelectedValue;if(!string.IsNullOrWhiteSpace(Password.Text))existing.PasswordHash=PasswordHasher.Hash(Password.Text);repo.Update(existing);
            AdminAuditService.Record(Auth.CurrentUserId,DomainValues.AuditAction.Updated,"User",username,"Updated account "+existing.FullName+".");Response.Redirect(ResolveUrl("~/Admin/Users.aspx"),false);Context.ApplicationInstance.CompleteRequest();
        }
        protected void ValidatePassword(object s,ServerValidateEventArgs a){a.IsValid=string.IsNullOrWhiteSpace(a.Value)||(a.Value??"").Length>=8;}
        protected void AddError(string m){Page.Validators.Add(new CustomValidator{IsValid=false,ErrorMessage=m});}
        private void ShowNotFound(){Response.StatusCode=404;Response.TrySkipIisCustomErrors=true;Response.Redirect(ResolveUrl("~/Error.aspx"),false);Context.ApplicationInstance.CompleteRequest();}
    }
}