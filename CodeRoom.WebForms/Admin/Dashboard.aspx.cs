using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Web.UI;
using CodeRoom.WebForms.Data;
using CodeRoom.WebForms.Helpers;

namespace CodeRoom.WebForms.Admin
{
    public partial class Dashboard : Page
    {
        protected System.Web.UI.WebControls.Literal RoleBadge, UserCount, StudentCount, EnrollmentCount, AttemptCount, QuizCount, CertificateCount, ChallengeCount, AuditCount, AdminCount;
        protected System.Web.UI.WebControls.Panel SuperAdminModule;
        protected System.Web.UI.WebControls.Repeater RecentUsersRepeater, AuditRepeater;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Auth.RequireAdmin(this)) return;
            if (IsPostBack) return;

            var repo = new AdminRepository();
            var counts = repo.GetDashboardCounts();
            if (counts.Rows.Count > 0)
            {
                var row = counts.Rows[0];
                UserCount.Text = Number(row, "Users");
                StudentCount.Text = Number(row, "Students");
                EnrollmentCount.Text = Number(row, "Enrollments");
                AttemptCount.Text = Number(row, "Attempts");
                QuizCount.Text = Number(row, "Quizzes");
                CertificateCount.Text = Number(row, "Certificates");
                ChallengeCount.Text = Number(row, "Challenges");
                AdminCount.Text = Number(row, "Administrators");
            }

            AuditCount.Text = repo.CountAuditEntries().ToString(CultureInfo.InvariantCulture);
            RoleBadge.Text = Auth.IsSuperAdmin ? "Super Administrator" : "Administrator";
            SuperAdminModule.Visible = Auth.IsSuperAdmin;

            var users = repo.GetRecentUsers(5).Select(u => new
            {
                u.FullName,
                u.Username,
                u.Role,
                Initial = string.IsNullOrWhiteSpace(u.FullName) ? "C" : u.FullName.Trim().Substring(0, 1).ToUpperInvariant(),
                Date = u.CreatedAt.ToLocalTime().ToString("dd MMM", CultureInfo.InvariantCulture)
            }).ToList();
            RecentUsersRepeater.DataSource = users;
            RecentUsersRepeater.DataBind();

            var audits = repo.GetRecent(6);
            var userRepo = new UserRepository();
            var rows = new List<object>();
            foreach (var audit in audits)
            {
                var user = userRepo.GetById(audit.UserId);
                rows.Add(new
                {
                    audit.Action,
                    audit.EntityType,
                    audit.Description,
                    Initial = user == null || string.IsNullOrWhiteSpace(user.FullName) ? "A" : user.FullName.Trim().Substring(0, 1).ToUpperInvariant(),
                    Time = audit.CreatedAt.ToLocalTime().ToString("HH:mm", CultureInfo.InvariantCulture)
                });
            }
            AuditRepeater.DataSource = rows;
            AuditRepeater.DataBind();
        }

        private static string Number(DataRow row, string column)
        {
            if (!row.Table.Columns.Contains(column)) return "0";
            return Convert.ToString(row[column], CultureInfo.InvariantCulture);
        }
    }
}