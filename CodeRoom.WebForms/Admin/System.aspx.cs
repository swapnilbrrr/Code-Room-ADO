using System;
using System.Globalization;
using System.Web.UI;
using CodeRoom.WebForms.Data;
using CodeRoom.WebForms.Helpers;

namespace CodeRoom.WebForms.Admin
{
    public partial class System : Page
    {
        protected global::System.Web.UI.WebControls.Literal UserCount, AdminCount, SuperAdminCount, CourseCount, CertificateCount, AuditCount;
        protected void Page_Load(object s,EventArgs e){if(Auth.RequireSuperAdmin(this))return;if(!IsPostBack)Bind();}
        private void Bind(){var table=new AdminRepository().GetDashboardCounts();if(table.Rows.Count==0)return;var r=table.Rows[0];UserCount.Text=Convert.ToString(r["Users"],CultureInfo.InvariantCulture);AdminCount.Text=Convert.ToString(r["Administrators"],CultureInfo.InvariantCulture);SuperAdminCount.Text=Convert.ToString(r["SuperAdministrators"],CultureInfo.InvariantCulture);CourseCount.Text=Convert.ToString(r["Courses"],CultureInfo.InvariantCulture);CertificateCount.Text=Convert.ToString(r["Certificates"],CultureInfo.InvariantCulture);AuditCount.Text=new AdminRepository().CountAuditEntries().ToString(CultureInfo.InvariantCulture);}
    }
}