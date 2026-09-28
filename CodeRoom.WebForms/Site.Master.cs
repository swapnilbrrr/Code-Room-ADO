using System;
using System.Configuration;
using CodeRoom.WebForms.Helpers;

namespace CodeRoom.WebForms
{
    public partial class SiteMaster : System.Web.UI.MasterPage
    {
        protected string DisplayName { get; private set; }
        protected string Username { get; private set; }
        protected string Initial { get; private set; }
        protected string RoleLabel { get; private set; }
        protected string CurrentYear { get; private set; }
        protected string ToastTitle { get; private set; }
        protected string ToastMessage { get; private set; }
        protected string ToastIcon { get; private set; }

        protected string AssetVersion
        {
            get { return ConfigurationManager.AppSettings["AssetVersion"] ?? "1"; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Auth.EnsureSessionIdentity();
            bool authenticated = Auth.IsLoggedIn;
            bool admin = Auth.IsAdmin;

            DisplayName = Auth.CurrentFullName;
            Username = Auth.CurrentUsername;
            Initial = Auth.Initial;
            RoleLabel = Auth.RoleLabel;
            CurrentYear = DateTime.UtcNow.Year.ToString();

            PhNavDashboard.Visible = authenticated;
            PhAuthenticated.Visible = authenticated;
            PhAnonymous.Visible = !authenticated;
            PhMobileAuth.Visible = authenticated;
            PhMobileAnonymous.Visible = !authenticated;
            PhFooterAuth.Visible = authenticated;
            PhFooterAnonymous.Visible = !authenticated;

            PhAdminConsole.Visible = admin;
            PhMobileAdminConsole.Visible = admin;
            PhCertificates.Visible = !admin;

            ToastTitle = Toast.Title;
            ToastMessage = Toast.Message;
            ToastIcon = Toast.Icon;
            PhToast.Visible = Toast.HasTitle;
            Toast.Clear();
        }
    }
}
