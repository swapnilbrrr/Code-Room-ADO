using System;
using System.Globalization;
using System.Web.UI;
using CodeRoom.WebForms.Data;
using CodeRoom.WebForms.Helpers;

namespace CodeRoom.WebForms.Quizzes
{
    public partial class Results : Page
    {
        protected System.Web.UI.WebControls.Panel NotFoundPanel, ResultPanel, CertificatePanel, CertificatesLinkPanel;
        protected System.Web.UI.WebControls.Literal StatusEyebrow, ResultHeading, Score, Total, Percent, QuizTitle, PassMessage, PassingScore, CertificateTitle, CertificateNumber;
        protected System.Web.UI.HtmlControls.HtmlGenericControl ResultStatus;
        protected System.Web.UI.HtmlControls.HtmlAnchor CertificateLink, CourseLink;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Auth.RequireLogin(this)) return;

            int attemptId;
            if (!int.TryParse(Request.QueryString["id"], out attemptId) || attemptId <= 0)
            {
                ShowNotFound();
                return;
            }

            var attempt = new QuizAttemptRepository().GetById(attemptId);
            if (attempt == null || attempt.UserId != Auth.CurrentUserId)
            {
                ShowNotFound();
                return;
            }

            var quiz = new QuizRepository().GetById(attempt.QuizId);
            if (quiz == null)
            {
                ShowNotFound();
                return;
            }

            var percentage = attempt.TotalQuestions == 0
                ? 0
                : (int)Math.Round(attempt.Score * 100.0 / attempt.TotalQuestions, MidpointRounding.AwayFromZero);
            var passed = new QuizRepository().HasPassed(quiz, percentage);
            var certificate = new CertificateRepository().GetByAttemptId(attempt.Id, Auth.CurrentUserId);

            Score.Text = attempt.Score.ToString(CultureInfo.InvariantCulture);
            Total.Text = attempt.TotalQuestions.ToString(CultureInfo.InvariantCulture);
            Percent.Text = percentage.ToString(CultureInfo.InvariantCulture);
            QuizTitle.Text = Server.HtmlEncode(quiz.Title);
            PassingScore.Text = quiz.PassingScorePercent.ToString(CultureInfo.InvariantCulture);
            StatusEyebrow.Text = passed ? "PASSED" : "ASSESSMENT COMPLETE";
            ResultHeading.Text = passed ? "Great work." : "Assessment complete.";
            PassMessage.Text = passed ? "✓ Passing score reached" : "↻ Keep practising";
            ResultStatus.Attributes["class"] = "result-status " + (passed ? "result-passed" : "result-failed");
            CourseLink.HRef = ResolveUrl("~/Courses/Details.aspx?id=" + quiz.CourseId);

            if (certificate != null)
            {
                CertificatePanel.Visible = true;
                CertificateTitle.Text = Server.HtmlEncode(certificate.Title);
                CertificateNumber.Text = Server.HtmlEncode(certificate.CertificateNumber);
                CertificateLink.HRef = ResolveUrl("~/Certificates/Details.aspx?id=" + certificate.Id);
            }

            CertificatesLinkPanel.Visible = quiz.IsCertificationExam;
            ResultPanel.Visible = true;
        }

        private void ShowNotFound()
        {
            Response.StatusCode = 404;
            Response.TrySkipIisCustomErrors = true;
            NotFoundPanel.Visible = true;
            ResultPanel.Visible = false;
        }
    }
}