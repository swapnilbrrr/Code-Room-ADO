namespace CodeRoom.WebForms.Models
{
    /// <summary>One multiple choice question of a quiz.</summary>
    public class Question
    {
        public int Id { get; set; }
        public int QuizId { get; set; }
        public string QuestionText { get; set; }
        public string OptionA { get; set; }
        public string OptionB { get; set; }
        public string OptionC { get; set; }
        public string OptionD { get; set; }
        public string CorrectOption { get; set; }

        public Question()
        {
            QuestionText = string.Empty;
            OptionA = string.Empty;
            OptionB = string.Empty;
            OptionC = string.Empty;
            OptionD = string.Empty;
            CorrectOption = "A";
        }
    }
}
