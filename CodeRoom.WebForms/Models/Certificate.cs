using System;

namespace CodeRoom.WebForms.Models
{
    /// <summary>Proof that a learner passed a certification exam for a course.</summary>
    public class Certificate
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int CourseId { get; set; }
        public int QuizAttemptId { get; set; }
        public string CertificateNumber { get; set; }
        public string Title { get; set; }
        public DateTime IssuedAt { get; set; }

        public Course Course { get; set; }
        public QuizAttempt QuizAttempt { get; set; }

        public Certificate()
        {
            CertificateNumber = string.Empty;
            Title = string.Empty;
            IssuedAt = DateTime.UtcNow;
        }
    }
}
