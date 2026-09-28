using System;
using System.Web.UI;
using CodeRoom.WebForms.Helpers;

namespace CodeRoom.WebForms.Authentication
{
    /// <summary>
    /// Clears the session identity and returns to the home page, matching
    /// AccountController.Logout in the source application. No database access is
    /// involved, so this page is complete as migrated.
    /// </summary>
    public partial class Logout : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Auth.SignOut();
            Response.Redirect(ResolveUrl("~/Default.aspx"), endResponse: true);
        }
    }
}
