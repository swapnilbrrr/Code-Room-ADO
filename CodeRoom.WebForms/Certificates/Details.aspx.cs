using System;
using System.Globalization;
using System.Web.UI;
using CodeRoom.WebForms.Data;
using CodeRoom.WebForms.Helpers;

namespace CodeRoom.WebForms.Certificates
{
    public partial class Details : Page
    {
        protected System.Web.UI.WebControls.Panel NotFoundPanel, CertificatePanel;
        protected System.Web.UI.WebControls.Literal CertificateTitle, RecipientName, CourseTitle, CertificateNumber, IssuedDate;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Auth.RequireLogin(this)) return;

            int id;
            if (!int.TryParse(Request.QueryString["id"], out id) || id <= 0)
            {
                ShowNotFound();
                return;
            }

            var certificate = new CertificateRepository().GetOwnedByUser(id, Auth.CurrentUserId);
            if (certificate == null)
            {
                ShowNotFound();
                return;
            }

            var course = new CourseRepository().GetById(certificate.CourseId);
            if (course == null)
            {
                ShowNotFound();
                return;
            }

            CertificateTitle.Text = Server.HtmlEncode(certificate.Title);
            RecipientName.Text = Server.HtmlEncode(Auth.CurrentFullName);
            CourseTitle.Text = Server.HtmlEncode(course.Title);
            CertificateNumber.Text = Server.HtmlEncode(certificate.CertificateNumber);
            IssuedDate.Text = certificate.IssuedAt.ToLocalTime().ToString("dd MMM yyyy", CultureInfo.InvariantCulture);
            CertificatePanel.Visible = true;
        }

        private void ShowNotFound()
        {
            Response.StatusCode = 404;
            Response.TrySkipIisCustomErrors = true;
            NotFoundPanel.Visible = true;
            CertificatePanel.Visible = false;
        }
    }
}