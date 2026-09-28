using System;
using CodeRoom.WebForms.Helpers;

namespace CodeRoom.WebForms.Authentication
{
    public partial class Logout : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Auth.SignOut();
            Response.Redirect(ResolveUrl("~/Default.aspx"), false);
            Context.ApplicationInstance.CompleteRequest();
        }
    }
}
