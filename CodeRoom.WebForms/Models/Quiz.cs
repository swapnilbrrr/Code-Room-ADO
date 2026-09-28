using System.Collections.Generic;

namespace CodeRoom.WebForms.Models
{
    /// <summary>An assessment attached to a course: a practice quiz or a certification exam.</summary>
    public class Quiz
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string AssessmentType { get; set; }
        public int TimeLimitMinutes { get; set; }
        public int PassingScorePercent { get; set; }
        public bool IsCertificationExam { get; set; }

        public List<Question> Questions { get; set; }
        public List<QuizAttempt> Attempts { get; set; }

        public Quiz()
        {
            Title = string.Empty;
            Description = string.Empty;
            AssessmentType = "Quiz";
            PassingScorePercent = 70;
            Questions = new List<Question>();
            Attempts = new List<QuizAttempt>();
        }
    }
}
