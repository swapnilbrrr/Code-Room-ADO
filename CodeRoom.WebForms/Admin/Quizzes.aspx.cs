using System;
using System.Collections.Generic;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using CodeRoom.WebForms.Data;
using CodeRoom.WebForms.Helpers;
using CodeRoom.WebForms.Services;

namespace CodeRoom.WebForms.Admin
{
    public partial class Quizzes : Page
    {
        protected Repeater QuizzesRepeater;
        protected Panel EmptyPanel;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Auth.RequireAdmin(this)) return;
            if (!IsPostBack) Bind();
        }

        private void Bind()
        {
            var table = new QuizRepository().GetAllGrid();
            var rows = new List<object>();
            foreach (DataRow row in table.Rows)
            {
                rows.Add(new
                {
                    Id = row["Id"],
                    Title = row["Title"],
                    Course = new { Title = row["CourseTitle"] },
                    QuestionCount = row["QuestionCount"]
                });
            }

            QuizzesRepeater.DataSource = rows;
            QuizzesRepeater.DataBind();
            EmptyPanel.Visible = rows.Count == 0;
        }

        protected void DeleteButton_Command(object sender, CommandEventArgs e)
        {
            if (Auth.RequireAdmin(this)) return;
            try { ((SiteMaster)Master).ValidateCsrf(); } catch (InvalidOperationException) { return; }

            int id;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out id) || id <= 0) return;
            var repo = new QuizRepository();
            var quiz = repo.GetById(id);
            if (quiz == null) return;

            repo.Delete(id);
            AdminAuditService.Record(Auth.CurrentUserId, DomainValues.AuditAction.Deleted, "Quiz", quiz.Title,
                "Deleted assessment " + quiz.Title + ".");
            Response.Redirect(ResolveUrl("~/Admin/Quizzes.aspx"), false);
            Context.ApplicationInstance.CompleteRequest();
        }
    }
}