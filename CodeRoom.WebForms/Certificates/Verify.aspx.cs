using System;
using System.Globalization;
using System.Web.UI;
using CodeRoom.WebForms.Data;

namespace CodeRoom.WebForms.Certificates
{
    public partial class Verify : Page
    {
        protected System.Web.UI.WebControls.TextBox Number;
        protected System.Web.UI.WebControls.Button VerifyButton;
        protected System.Web.UI.WebControls.Panel FailedPanel, ResultPanel;
        protected System.Web.UI.WebControls.Literal ResultTitle, ResultUser, ResultCourse, ResultNumber, ResultDate;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                Number.Text = Request.QueryString["number"] ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(Number.Text))
                    LoadResult(Number.Text.Trim());
            }
        }

        protected void VerifyButton_Click(object sender, EventArgs e)
        {
            LoadResult((Number.Text ?? string.Empty).Trim());
        }

        private void LoadResult(string number)
        {
            FailedPanel.Visible = false;
            ResultPanel.Visible = false;
            if (string.IsNullOrWhiteSpace(number))
                return;

            var certificate = new CertificateRepository().GetByNumber(number);
            if (certificate == null)
            {
                FailedPanel.Visible = true;
                return;
            }

            var course = new CourseRepository().GetById(certificate.CourseId);
            var user = new UserRepository().GetById(certificate.UserId);
            if (course == null || user == null)
            {
                FailedPanel.Visible = true;
                return;
            }

            ResultTitle.Text = Server.HtmlEncode(certificate.Title);
            ResultUser.Text = Server.HtmlEncode(user.FullName);
            ResultCourse.Text = Server.HtmlEncode(course.Title);
            ResultNumber.Text = Server.HtmlEncode(certificate.CertificateNumber);
            ResultDate.Text = certificate.IssuedAt.ToLocalTime().ToString("dd MMM yyyy", CultureInfo.InvariantCulture);
            ResultPanel.Visible = true;
        }
    }
}