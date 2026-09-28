using System;
using CodeRoom.WebForms.Helpers;

namespace CodeRoom.WebForms.Authentication
{
    public partial class Logout : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack) Csrf.EnsureToken(CsrfToken);
        }

        protected void LogoutButton_Click(object sender, EventArgs e)
        {
            try
            {
                Csrf.Validate(CsrfToken);
                Auth.SignOut();
                Response.Redirect(ResolveUrl("~/Default.aspx"), false);
                Context.ApplicationInstance.CompleteRequest();
            }
            catch (InvalidOperationException)
            {
                Response.Redirect(ResolveUrl("~/Authentication/Login.aspx"), false);
                Context.ApplicationInstance.CompleteRequest();
            }
        }
    }
}
