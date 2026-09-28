using System;
using System.Linq;
using System.Web.UI;
using CodeRoom.WebForms.Data;
using CodeRoom.WebForms.Helpers;
using CodeRoom.WebForms.Models;
using CodeRoom.WebForms.Services;

namespace CodeRoom.WebForms.Challenges
{
    public partial class Take : Page
    {
        protected System.Web.UI.WebControls.Panel NotFoundPanel;
        protected System.Web.UI.WebControls.Panel ChallengePanel;
        protected System.Web.UI.WebControls.Literal Category;
        protected System.Web.UI.WebControls.Literal ChallengeTitle;
        protected System.Web.UI.WebControls.Literal Points;
        protected System.Web.UI.WebControls.Repeater InstructionRepeater;
        protected System.Web.UI.WebControls.Panel StarterPanel;
        protected System.Web.UI.WebControls.Literal StarterCode;
        protected System.Web.UI.WebControls.Panel HintPanel;
        protected System.Web.UI.WebControls.Literal Hint;
        protected System.Web.UI.WebControls.TextBox Answer;
        protected System.Web.UI.WebControls.HiddenField CsrfToken;
        protected System.Web.UI.WebControls.Button SubmitButton;
        protected System.Web.UI.WebControls.RequiredFieldValidator AnswerValidator;
        protected int CourseId { get; private set; }
        private Challenge CurrentChallenge { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Auth.RequireLogin(this)) return;
            Csrf.EnsureToken(CsrfToken);

            int challengeId;
            if (!int.TryParse(Request.QueryString["id"], out challengeId) || challengeId <= 0)
            {
                ShowNotFound();
                return;
            }

            CurrentChallenge = new ChallengeRepository().GetById(challengeId);
            if (CurrentChallenge == null)
            {
                ShowNotFound();
                return;
            }

            var course = new CourseRepository().GetById(CurrentChallenge.CourseId);
            if (course == null || !course.IsPublished)
            {
                ShowNotFound();
                return;
            }

            CourseId = course.Id;
            if (!Auth.IsAdmin && !new EnrollmentRepository().IsEnrolled(Auth.CurrentUserId, course.Id))
            {
                Toast.Error("Enrollment required", "Enroll in this course before opening the practice lab.");
                Response.Redirect(ResolveUrl("~/Courses/Details.aspx?id=" + course.Id), false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            Category.Text = Server.HtmlEncode(course.Category ?? string.Empty).ToUpperInvariant();
            ChallengeTitle.Text = Server.HtmlEncode(CurrentChallenge.Title);
            Points.Text = CurrentChallenge.Points.ToString();
            StarterCode.Text = Server.HtmlEncode(CurrentChallenge.StarterCode ?? string.Empty);
            Hint.Text = Server.HtmlEncode(CurrentChallenge.Hint ?? string.Empty);
            StarterPanel.Visible = !string.IsNullOrWhiteSpace(CurrentChallenge.StarterCode);
            HintPanel.Visible = !string.IsNullOrWhiteSpace(CurrentChallenge.Hint);
            InstructionRepeater.DataSource = (CurrentChallenge.Instructions ?? string.Empty)
                .Split(new[] { "\r\n\r\n", "\n\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim()).ToList();
            InstructionRepeater.DataBind();
            ChallengePanel.Visible = true;
        }

        protected void SubmitButton_Click(object sender, EventArgs e)
        {
            if (Auth.RequireLogin(this)) return;
            try { Csrf.Validate(CsrfToken); }
            catch (InvalidOperationException) { Toast.Error("Security check failed", "Refresh the page and try again."); return; }

            var challengeId = CurrentChallenge != null ? CurrentChallenge.Id : ParseId();
            if (challengeId <= 0) { ShowNotFound(); return; }

            var challenge = new ChallengeRepository().GetById(challengeId);
            if (challenge == null) { ShowNotFound(); return; }

            var course = new CourseRepository().GetById(challenge.CourseId);
            if (course == null || !course.IsPublished) { ShowNotFound(); return; }

            if (!Auth.IsAdmin && !new EnrollmentRepository().IsEnrolled(Auth.CurrentUserId, course.Id))
            {
                Response.Redirect(ResolveUrl("~/Courses/Details.aspx?id=" + course.Id), false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            if (!ChallengeRepository.Validate(challenge, Answer.Text ?? string.Empty))
            {
                Toast.Info("Not quite yet", "Review the hint and try again.");
                return;
            }

            new LearningActivityService().CompleteChallenge(Auth.CurrentUserId, challenge);
            Toast.Success("Challenge passed", "+" + challenge.Points + " XP · " + challenge.Title);
            Response.Redirect(ResolveUrl("~/Challenges/Take.aspx?id=" + challenge.Id), false);
            Context.ApplicationInstance.CompleteRequest();
        }

        private int ParseId()
        {
            int id; return int.TryParse(Request.QueryString["id"], out id) ? id : 0;
        }

        private void ShowNotFound()
        {
            Response.StatusCode = 404;
            Response.TrySkipIisCustomErrors = true;
            NotFoundPanel.Visible = true;
            ChallengePanel.Visible = false;
        }
    }
}