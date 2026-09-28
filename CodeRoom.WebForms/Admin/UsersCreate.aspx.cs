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
    public partial class UsersCreate : Page
    {
        protected TextBox FullName, Username, Email, Password;
        protected DropDownList Role;
        protected Button SaveButton;
        protected void Page_Load(object s,EventArgs e){if(Auth.RequireAdmin(this))return;}
        protected void SaveButton_Click(object s,EventArgs e){if(Auth.RequireAdmin(this))return;try{((SiteMaster)Master).ValidateCsrf();}catch(InvalidOperationException){return;}Page.Validate();if(!Page.IsValid)return;var repo=new UserRepository();var username=Username.Text.Trim().ToLowerInvariant();var email=Email.Text.Trim().ToLowerInvariant();if(repo.UsernameExists(username,0)){AddError("This username is already in use.");}if(repo.EmailExists(email,0)){AddError("This email is already registered.");}if(Role.SelectedValue!="Student"&&!Auth.IsSuperAdmin){AddError("Only a Super Administrator can grant that role.");}if(!Page.IsValid)return;var user=new User{FullName=FullName.Text.Trim(),Username=username,Email=email,Role=Role.SelectedValue,PasswordHash=PasswordHasher.Hash(Password.Text)};user.Id=repo.Insert(user);AdminAuditService.Record(Auth.CurrentUserId,DomainValues.AuditAction.Created,"User",username,"Created "+user.Role+" account "+user.FullName+".");Response.Redirect(ResolveUrl("~/Admin/Users.aspx"),false);Context.ApplicationInstance.CompleteRequest();}
        protected void ValidateUsername(object s,ServerValidateEventArgs a){var v=(a.Value??"").Trim();a.IsValid=v.Length>=3&&v.Length<=30&&Regex.IsMatch(v,"^[a-zA-Z0-9._-]+$");}
        protected void ValidatePassword(object s,ServerValidateEventArgs a){a.IsValid=(a.Value??"").Length>=8;}
        protected void AddError(string m){Page.Validators.Add(new CustomValidator{IsValid=false,ErrorMessage=m});}
    }
}