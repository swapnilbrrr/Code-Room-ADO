using System;

namespace CodeRoom.WebForms.Models
{
    /// <summary>A submitted quiz attempt with its raw score.</summary>
    public class QuizAttempt
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int QuizId { get; set; }
        public int Score { get; set; }
        public int TotalQuestions { get; set; }
        public DateTime AttemptedAt { get; set; }

        public Quiz Quiz { get; set; }

        public QuizAttempt()
        {
            AttemptedAt = DateTime.UtcNow;
        }

        public int PercentScore
        {
            get { return TotalQuestions == 0 ? 0 : (int)Math.Round(Score * 100.0 / TotalQuestions, MidpointRounding.AwayFromZero); }
        }
    }
}
