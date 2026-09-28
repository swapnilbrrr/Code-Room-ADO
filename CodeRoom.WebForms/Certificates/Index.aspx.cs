using System;
using System.Globalization;
using System.Web.UI;
using CodeRoom.WebForms.Data;
using CodeRoom.WebForms.Helpers;

namespace CodeRoom.WebForms.Certificates
{
    public partial class Index : Page
    {
        protected System.Web.UI.WebControls.Literal CertificateCount;
        protected System.Web.UI.WebControls.Panel EmptyPanel;
        protected System.Web.UI.WebControls.Repeater CertificatesRepeater;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Auth.RequireLogin(this)) return;
            if (IsPostBack) return;

            var certificates = new CertificateRepository().GetByUser(Auth.CurrentUserId);
            var courses = new CourseRepository();
            foreach (var certificate in certificates)
                certificate.Course = courses.GetById(certificate.CourseId);

            CertificateCount.Text = certificates.Count.ToString(CultureInfo.InvariantCulture);
            EmptyPanel.Visible = certificates.Count == 0;
            CertificatesRepeater.DataSource = certificates;
            CertificatesRepeater.DataBind();
        }

        protected string FormatDate(object value)
        {
            return Convert.ToDateTime(value).ToLocalTime().ToString("dd MMM yyyy", CultureInfo.InvariantCulture);
        }
    }
}