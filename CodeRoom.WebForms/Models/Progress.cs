using System;

namespace CodeRoom.WebForms.Models
{
    /// <summary>One learner's completion state for one lesson.</summary>
    public class Progress
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int LessonId { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}
