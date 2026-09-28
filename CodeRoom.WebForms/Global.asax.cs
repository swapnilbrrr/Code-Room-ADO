using System;
using System.IO;
using CodeRoom.WebForms.Data;

namespace CodeRoom.WebForms
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            // Prepare CodeRoomDb (create it when missing, apply the guarded schema script, seed the
            // baseline content) before the first request is served. Initialization failures are
            // logged and rethrown so the application cannot start against a broken schema.
            try
            {
                var result = DatabaseInitializer.InitializeWithSeedData();

                Log("Initialized " + result.DatabaseName
                    + (result.DatabaseCreated ? " (created)" : " (already present)")
                    + ", " + result.BatchesExecuted + " schema batches, "
                    + result.SeedRowsCreated + " seed rows created.");
            }
            catch (Exception ex)
            {
                Log("Initialization failed: " + ex);
                throw;
            }
        }

        protected void Session_Start(object sender, EventArgs e)
        {
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            var error = Server.GetLastError();

            if (error != null)
            {
                Log("Unhandled error: " + error.GetBaseException().Message);
            }
        }

        protected void Session_End(object sender, EventArgs e)
        {
        }

        private void Log(string message)
        {
            var path = Server.MapPath("~/App_Data/Startup.log");

            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.AppendAllText(path, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "  " + message + Environment.NewLine);
        }
    }
}
