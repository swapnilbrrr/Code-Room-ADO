using System;

namespace CodeRoom.WebForms
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            // SQL Server schema creation and seeding are added in Phase 3.
            // The Web Forms project intentionally starts without a database dependency.
        }

        protected void Session_Start(object sender, EventArgs e)
        {
        }

        protected void Application_Error(object sender, EventArgs e)
        {
        }

        protected void Session_End(object sender, EventArgs e)
        {
        }
    }
}
