using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using CodeRoom.WebForms.Data;
using CodeRoom.WebForms.Helpers;
using CodeRoom.WebForms.Services;

namespace CodeRoom.WebForms.Admin
{
    public partial class QuizzesManage : Page
    {
        protected Literal CourseTitle, QuizTitle, SuccessMessage;
        protected HtmlAnchor AddQuestionLink;
        protected Panel SuccessPanel, EmptyPanel;
        protected Repeater QuestionsRepeater;
        protected void Page_Load(object s,EventArgs e){if(Auth.RequireAdmin(this))return;if(!IsPostBack)Bind();}
        private void Bind(){int id;if(!int.TryParse(Request.QueryString["id"],out id)||id<=0){ShowNotFound();return;}var quiz=new QuizRepository().GetById(id);if(quiz==null){ShowNotFound();return;}var course=new CourseRepository().GetById(quiz.CourseId);if(course==null){ShowNotFound();return;}CourseTitle.Text=Server.HtmlEncode(course.Title);QuizTitle.Text=Server.HtmlEncode(quiz.Title);AddQuestionLink.HRef=ResolveUrl("~/Admin/QuizzesQuestionCreate.aspx?quizId="+quiz.Id);var questions=new QuizRepository().GetQuestions(quiz.Id);QuestionsRepeater.DataSource=questions;QuestionsRepeater.DataBind();EmptyPanel.Visible=questions.Count==0;}
        protected void DeleteButton_Command(object s,CommandEventArgs e){if(Auth.RequireAdmin(this))return;try{((SiteMaster)Master).ValidateCsrf();}catch(InvalidOperationException){return;}int id;if(!int.TryParse(Convert.ToString(e.CommandArgument),out id)||id<=0)return;var repo=new QuizRepository();var q=GetQuestion(id);if(q==null)return;repo.DeleteQuestion(id); Bind(); }
        private Question GetQuestion(int id){const string sql="SELECT Id, QuizId, QuestionText, OptionA, OptionB, OptionC, OptionD, CorrectOption FROM dbo.Questions WHERE Id=@Id;";return SqlHelper.ReadFirst(sql, QuizRepository.MapQuestion,new System.Data.SqlClient.SqlParameter("@Id",System.Data.SqlDbType.Int){Value=id});}
        private void ShowNotFound(){Response.StatusCode=404;Response.TrySkipIisCustomErrors=true;Response.Redirect(ResolveUrl("~/Error.aspx"),false);Context.ApplicationInstance.CompleteRequest();}
    }
}