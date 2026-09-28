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

        // <%=%> blocks are not evaluated inside <head runat="server">, so the stylesheet links are
        // emitted from code instead of inline markup.
        private string BuildStylesheetLinks()
        {
            var version = System.Web.HttpUtility.HtmlAttributeEncode(AssetVersion);
            return string.Format(
                "<link rel=\"stylesheet\" href=\"{0}?v={1}\" />{2}<link rel=\"stylesheet\" href=\"{3}?v={1}\" />",
                ResolveUrl("~/Assets/css/site.css"),
                version,
                Environment.NewLine,
                ResolveUrl("~/Assets/css/ui-updates.css") + "?v=" + version);
        }

        public void ValidateCsrf()
        {
            Csrf.Validate(MasterCsrfToken);
        }

        protected void LogoutButton_Click(object sender, EventArgs e)
        {
            try
            {
                ValidateCsrf();
            }
            catch (InvalidOperationException)
            {
                Response.Redirect(ResolveUrl("~/Authentication/Login.aspx"), false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            Auth.SignOut();
            Response.Redirect(ResolveUrl("~/Default.aspx"), false);
            Context.ApplicationInstance.CompleteRequest();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            StylesheetLinks.Text = BuildStylesheetLinks();

            if (!IsPostBack)
            {
                Csrf.EnsureToken(MasterCsrfToken);
            }

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
