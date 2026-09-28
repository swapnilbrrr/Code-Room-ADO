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
    public partial class QuizzesManage : Page
    {
        protected Literal CourseTitle, QuizTitle, SuccessMessage;
        protected HtmlAnchor AddQuestionLink;
        protected Panel SuccessPanel, EmptyPanel;
        protected Repeater QuestionsRepeater;
        protected void Page_Load(object s,EventArgs e){if(Auth.RequireAdmin(this))return;if(!IsPostBack)Bind();}
        private void Bind(){int id;if(!int.TryParse(Request.QueryString["id"],out id)||id<=0){ShowNotFound();return;}var quiz=new QuizRepository().GetById(id);if(quiz==null){ShowNotFound();return;}var course=new CourseRepository().GetById(quiz.CourseId);if(course==null){ShowNotFound();return;}CourseTitle.Text=Server.HtmlEncode(course.Title);QuizTitle.Text=Server.HtmlEncode(quiz.Title);AddQuestionLink.HRef=ResolveUrl("~/Admin/QuizzesQuestionCreate.aspx?quizId="+quiz.Id);var questions=new QuizRepository().GetQuestions(quiz.Id);QuestionsRepeater.DataSource=questions;QuestionsRepeater.DataBind();EmptyPanel.Visible=questions.Count==0;}
        protected void DeleteButton_Command(object s,CommandEventArgs e){if(Auth.RequireAdmin(this))return;try{((SiteMaster)Master).ValidateCsrf();}catch(InvalidOperationException){return;}int id;if(!int.TryParse(Convert.ToString(e.CommandArgument),out id)||id<=0)return;var repo=new QuizRepository();var q=GetQuestion(id, CurrentQuizId());if(q==null)return;repo.DeleteQuestion(id); Bind(); }
        private int CurrentQuizId(){int id;return int.TryParse(Request.QueryString["id"],out id)&&id>0?id:0;}
        private Question GetQuestion(int id,int quizId){const string sql="SELECT Id, QuizId, QuestionText, OptionA, OptionB, OptionC, OptionD, CorrectOption FROM dbo.Questions WHERE Id=@Id AND QuizId=@QuizId;";return SqlHelper.ReadFirst(sql, QuizRepository.MapQuestion,new global::System.Data.SqlClient.SqlParameter("@Id",global::System.Data.SqlDbType.Int){Value=id},new global::System.Data.SqlClient.SqlParameter("@QuizId",global::System.Data.SqlDbType.Int){Value=quizId});}
        private void ShowNotFound(){Response.StatusCode=404;Response.TrySkipIisCustomErrors=true;Response.Redirect(ResolveUrl("~/Error.aspx"),false);Context.ApplicationInstance.CompleteRequest();}
    }
}