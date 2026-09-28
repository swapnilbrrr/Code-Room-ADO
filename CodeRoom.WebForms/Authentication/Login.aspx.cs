using System;
using CodeRoom.WebForms.Data;
using CodeRoom.WebForms.Helpers;
using CodeRoom.WebForms.Services;

namespace CodeRoom.WebForms.Authentication
{
    public partial class Login : System.Web.UI.Page
    {
        private readonly UserRepository users = new UserRepository();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Auth.IsLoggedIn)
            {
                Response.Redirect(ResolveUrl("~/Dashboard/MyDashboard.aspx"), false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            if (!IsPostBack)
            {
                ReturnUrl.Value = GetSafeReturnUrl(Request.QueryString["returnUrl"]);
            }

            Csrf.EnsureToken(CsrfToken);
        }

        protected void LoginButton_Click(object sender, EventArgs e)
        {
            Page.Validate("Login");
            if (!Page.IsValid) return;

            try
            {
                Csrf.Validate(CsrfToken);

                var email = (Email.Text ?? string.Empty).Trim().ToLowerInvariant();
                var password = Password.Text ?? string.Empty;
                var user = users.GetByEmail(email);

                if (user == null || !PasswordHasher.Verify(password, user.PasswordHash))
                {
                    ShowError("Invalid email or password.");
                    return;
                }

                Auth.SignIn(user, RememberMe.Checked);

                var target = GetSafeReturnUrl(ReturnUrl.Value);
                if (string.IsNullOrEmpty(target))
                {
                    target = ResolveUrl("~/Dashboard/MyDashboard.aspx");
                }

                Response.Redirect(target, false);
                Context.ApplicationInstance.CompleteRequest();
            }
            catch (InvalidOperationException ex)
            {
                ShowError(ex.Message);
            }
        }

        private void ShowError(string message)
        {
            ErrorMessage.Text = Server.HtmlEncode(message);
            ErrorMessage.Visible = true;
        }

        private string GetSafeReturnUrl(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;
            if (value[0] != '/') return string.Empty;
            if (value.Length > 1 && (value[1] == '/' || value[1] == '\\')) return string.Empty;
            return value;
        }
    }
}
