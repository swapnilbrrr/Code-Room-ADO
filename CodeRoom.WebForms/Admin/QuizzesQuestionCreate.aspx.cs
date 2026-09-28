using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using CodeRoom.WebForms.Data;
using CodeRoom.WebForms.Helpers;
using CodeRoom.WebForms.Models;
using CodeRoom.WebForms.Services;

namespace CodeRoom.WebForms.Admin
{
    public partial class QuizzesQuestionCreate : Page
    {
        protected HiddenField QuizId;
        protected TextBox QuestionText, OptionA, OptionB, OptionC, OptionD;
        protected DropDownList CorrectOption;
        protected HtmlControls.HtmlAnchor CancelLink;
        protected Button SaveButton;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Auth.RequireAdmin(this)) return;
            if (!IsPostBack)
            {
                int quizId;
                if (!int.TryParse(Request.QueryString["quizId"], out quizId) || quizId <= 0 || new QuizRepository().GetById(quizId) == null)
                { Response.StatusCode=404; Response.TrySkipIisCustomErrors=true; Response.Redirect(ResolveUrl("~/Error.aspx"),false); Context.ApplicationInstance.CompleteRequest(); return; }
                QuizId.Value = quizId.ToString();
                CancelLink.HRef = ResolveUrl("~/Admin/QuizzesManage.aspx?id=" + quizId);
            }
        }

        protected void SaveButton_Click(object sender, EventArgs e)
        {
            if (Auth.RequireAdmin(this)) return;
            try { ((SiteMaster)Master).ValidateCsrf(); } catch (InvalidOperationException) { return; }
            Page.Validate(); if (!Page.IsValid) return;
            int quizId; if(!int.TryParse(QuizId.Value,out quizId)||new QuizRepository().GetById(quizId)==null)return;
            var q=new Question{QuizId=quizId,QuestionText=QuestionText.Text.Trim(),OptionA=OptionA.Text.Trim(),OptionB=OptionB.Text.Trim(),OptionC=OptionC.Text.Trim(),OptionD=OptionD.Text.Trim(),CorrectOption=CorrectOption.SelectedValue};
            new QuizRepository().InsertQuestion(q);
            AdminAuditService.Record(Auth.CurrentUserId,DomainValues.AuditAction.Created,"Question","Quiz #"+quizId,"Added a quiz question.");
            Response.Redirect(ResolveUrl("~/Admin/QuizzesManage.aspx?id="+quizId),false);Context.ApplicationInstance.CompleteRequest();
        }
    }
}