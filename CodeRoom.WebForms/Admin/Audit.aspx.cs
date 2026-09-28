using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.UI;
using CodeRoom.WebForms.Data;
using CodeRoom.WebForms.Helpers;

namespace CodeRoom.WebForms.Admin
{
    public partial class Audit : Page
    {
        protected System.Web.UI.WebControls.Panel EmptyPanel;
        protected System.Web.UI.WebControls.Repeater AuditRepeater;
        protected void Page_Load(object s,EventArgs e){if(Auth.RequireAdmin(this))return;if(!IsPostBack)Bind();}
        private void Bind(){var repo=new AdminRepository();var logs=repo.GetRecent(100);var users=new UserRepository();var rows=new List<object>();foreach(var l in logs){var u=users.GetById(l.UserId);rows.Add(new{l.Action,l.EntityType,l.EntityName,l.Description,Initial=u==null||string.IsNullOrWhiteSpace(u.FullName)?"A":u.FullName.Trim().Substring(0,1).ToUpperInvariant(),Date=l.CreatedAt.ToLocalTime().ToString("dd MMM yyyy · HH:mm",CultureInfo.InvariantCulture)});}AuditRepeater.DataSource=rows;AuditRepeater.DataBind();EmptyPanel.Visible=rows.Count==0;}
    }
}