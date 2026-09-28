using System;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using CodeRoom.WebForms.Data;
using CodeRoom.WebForms.Helpers;
using CodeRoom.WebForms.Models;
using CodeRoom.WebForms.Services;

namespace CodeRoom.WebForms.Admin
{
    public partial class QuizzesQuestionEdit : Page
    {
        protected HiddenField QuestionId, QuizId;
        protected TextBox QuestionText, OptionA, OptionB, OptionC, OptionD;
        protected DropDownList CorrectOption;
        protected HtmlAnchor CancelLink;
        protected Button SaveButton;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Auth.RequireAdmin(this)) return;
            if (!IsPostBack)
            {
                int id;
                if (!int.TryParse(Request.QueryString["id"], out id) || id <= 0) { ShowNotFound(); return; }
                var q = new QuizRepository().GetQuestionById(id);
                if (q == null) { ShowNotFound(); return; }
                QuestionId.Value=q.Id.ToString(); QuizId.Value=q.QuizId.ToString(); QuestionText.Text=q.QuestionText; OptionA.Text=q.OptionA; OptionB.Text=q.OptionB; OptionC.Text=q.OptionC; OptionD.Text=q.OptionD; CorrectOption.SelectedValue=q.CorrectOption; CancelLink.HRef=ResolveUrl("~/Admin/QuizzesManage.aspx?id="+q.QuizId);
            }
        }

        protected void SaveButton_Click(object sender,EventArgs e)
        {
            if(Auth.RequireAdmin(this))return;try{((SiteMaster)Master).ValidateCsrf();}catch(InvalidOperationException){return;}Page.Validate();if(!Page.IsValid)return;
            int id;if(!int.TryParse(QuestionId.Value,out id)||id<=0){ShowNotFound();return;}var repo=new QuizRepository();var q=repo.GetQuestionById(id);if(q==null){ShowNotFound();return;}
            q.QuestionText=QuestionText.Text.Trim();q.OptionA=OptionA.Text.Trim();q.OptionB=OptionB.Text.Trim();q.OptionC=OptionC.Text.Trim();q.OptionD=OptionD.Text.Trim();q.CorrectOption=CorrectOption.SelectedValue;
            using(var connection=DbConnectionFactory.Open())using(var tx=connection.BeginTransaction()){repo.UpdateQuestion(connection,tx,q);tx.Commit();}
            AdminAuditService.Record(Auth.CurrentUserId,DomainValues.AuditAction.Updated,"Question","Question #"+q.Id,"Updated a quiz question.");
            Response.Redirect(ResolveUrl("~/Admin/QuizzesManage.aspx?id="+q.QuizId),false);Context.ApplicationInstance.CompleteRequest();
        }

        private void ShowNotFound(){Response.StatusCode=404;Response.TrySkipIisCustomErrors=true;Response.Redirect(ResolveUrl("~/Error.aspx"),false);Context.ApplicationInstance.CompleteRequest();}
    }
}