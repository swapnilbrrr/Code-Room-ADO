using System;

namespace CodeRoom.WebForms.Models
{
    /// <summary>Links a learner to a course.</summary>
    public class Enrollment
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int CourseId { get; set; }
        public DateTime EnrolledAt { get; set; }

        public Enrollment()
        {
            EnrolledAt = DateTime.UtcNow;
        }
    }
}
