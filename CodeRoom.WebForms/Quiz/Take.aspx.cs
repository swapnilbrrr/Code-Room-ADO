using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using CodeRoom.WebForms.Data;
using CodeRoom.WebForms.Models;
using CodeRoom.WebForms.Services;
using CodeRoom.WebForms.Helpers;

namespace CodeRoom.WebForms.Quizzes
{
    public partial class Take : Page
    {
        private readonly QuizRepository quizzes = new QuizRepository();

        // The client-side countdown is cosmetic; the deadline is only trustworthy when the server
        // stamps the start time and rejects late submissions. Two minutes of grace absorb latency.
        private static readonly TimeSpan Grace = TimeSpan.FromMinutes(2);
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Auth.RequireLogin(this)) return;
            if (!IsPostBack) LoadQuiz();
        }
        private void LoadQuiz()
        {
            int quizId;
            if (!int.TryParse(Request.QueryString["id"], out quizId)) { ShowNotFound(); return; }
            var quiz = quizzes.GetForTaking(quizId);
            if (quiz == null || quiz.Questions.Count == 0) { ShowNotFound(); return; }
            if (!Auth.IsAdmin && !new EnrollmentRepository().IsEnrolled(Auth.CurrentUserId, quiz.CourseId))
            {
                Toast.Error("Enrollment required", "Enroll in this course before taking the assessment.");
                Response.Redirect("~/Courses/Details.aspx?id=" + quiz.CourseId, false); Context.ApplicationInstance.CompleteRequest(); return;
            }
            QuizId.Value=quiz.Id.ToString(); PageTitle.Text=Server.HtmlEncode(quiz.Title)+" - Code-Room";
            AssessmentEyebrow.Text=quiz.IsCertificationExam ? "FINAL ASSESSMENT" : "KNOWLEDGE CHECK"; QuizTitle.Text=Server.HtmlEncode(quiz.Title);
            QuizDescription.Text=Server.HtmlEncode(quiz.Description); QuestionCount.Text=quiz.Questions.Count.ToString(); PassingScore.Text=GetRequiredScore(quiz).ToString();
            TimeLimit.Text=quiz.TimeLimitMinutes+":00"; QuizTimer.Attributes["data-minutes"]=quiz.TimeLimitMinutes.ToString(); TimeInfo.Text=quiz.TimeLimitMinutes>0 ? "You have "+quiz.TimeLimitMinutes+" minutes." : "No time limit.";
            SubmitHint.Text=quiz.IsCertificationExam ? "Passing this exam may unlock a certificate." : "You can review your score after submission.";
            SubmitButton.Text=quiz.IsCertificationExam ? "Submit examination" : "Submit answers"; TimerPanel.Visible=quiz.TimeLimitMinutes>0;
            QuestionsRepeater.DataSource=quiz.Questions; QuestionsRepeater.DataBind();
            if (!IsPostBack && quiz.TimeLimitMinutes > 0) Session[StartKey(quiz.Id)] = DateTime.UtcNow;
        }

        private static string StartKey(int quizId) { return "CR_QuizStart_" + quizId; }
        protected void SubmitButton_Click(object sender, EventArgs e)
        {
            if (Auth.RequireLogin(this)) return;
            try { ((SiteMaster)Master).ValidateCsrf(); }
            catch (InvalidOperationException) { Toast.Error("Security check failed", "Refresh the page and submit again."); return; }

            int quizId; if(!int.TryParse(QuizId.Value,out quizId)){ShowNotFound();return;}
            var quiz=quizzes.GetForTaking(quizId); if(quiz==null||quiz.Questions.Count==0){ShowNotFound();return;}
            if(!Auth.IsAdmin && !new EnrollmentRepository().IsEnrolled(Auth.CurrentUserId,quiz.CourseId)){Toast.Error("Enrollment required","Enroll in this course before taking the assessment.");Response.Redirect("~/Courses/Details.aspx?id="+quiz.CourseId,false);Context.ApplicationInstance.CompleteRequest();return;}
            if (quiz.TimeLimitMinutes > 0)
            {
                var started = Session[StartKey(quiz.Id)] as DateTime?;
                if (started == null)
                {
                    Toast.Error("Session expired", "Start the assessment again to submit answers.");
                    LoadQuiz(); return;
                }
                if (DateTime.UtcNow - started.Value > TimeSpan.FromMinutes(quiz.TimeLimitMinutes) + Grace)
                {
                    Session.Remove(StartKey(quiz.Id));
                    Toast.Error("Time expired", "The time limit for this assessment has passed.");
                    return;
                }
            }
            var answers=new Dictionary<int,string>();
            foreach(var question in quiz.Questions){var value=Request.Form["answer_"+question.Id];if(!string.IsNullOrWhiteSpace(value))answers[question.Id]=value.Trim().ToUpperInvariant();}
            if(answers.Count!=quiz.Questions.Count){Toast.Error("Incomplete assessment","Please answer every question before submitting.");LoadQuiz();return;}
            var attempt=new LearningActivityService().SubmitQuiz(Auth.CurrentUserId,quiz,answers);
            Session.Remove(StartKey(quiz.Id));
            Response.Redirect("~/Quiz/Results.aspx?id="+attempt.Id,false); Context.ApplicationInstance.CompleteRequest();
        }
        private static int GetRequiredScore(Quiz quiz){return string.Equals(quiz.AssessmentType,DomainValues.AssessmentType.Quiz,StringComparison.OrdinalIgnoreCase)?70:quiz.PassingScorePercent;}
        private void ShowNotFound(){QuizContent.Visible=false;NotFound.Visible=true;Response.StatusCode=404;Response.TrySkipIisCustomErrors=true;}
        protected HtmlGenericControl QuizTimer; protected Panel QuizContent; protected Panel NotFound; protected Literal PageTitle; protected Literal AssessmentEyebrow; protected Literal QuizTitle; protected Literal QuizDescription; protected Literal QuestionCount; protected Literal PassingScore; protected Literal TimeLimit; protected Literal TimeInfo; protected Literal SubmitHint; protected Panel TimerPanel; protected HiddenField QuizId; protected Repeater QuestionsRepeater; protected Button SubmitButton;
    }
}