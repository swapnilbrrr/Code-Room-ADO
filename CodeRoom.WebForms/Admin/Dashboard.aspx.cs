using System;
using CodeRoom.WebForms.Helpers;

namespace CodeRoom.WebForms.Admin
{
    public partial class Dashboard : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Auth.RequireAdmin(this)) return;
        }
    }
}
