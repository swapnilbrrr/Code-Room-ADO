using System;
using System.Collections.Generic;

namespace CodeRoom.WebForms.Models
{
    /// <summary>A learning path made of modules, lessons, challenges and a quiz.</summary>
    public class Course
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Slug { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Level { get; set; }
        public int EstimatedMinutes { get; set; }
        public bool IsCertification { get; set; }
        public string CertificateName { get; set; }
        public int PassingScorePercent { get; set; }
        public string ThumbnailUrl { get; set; }
        public bool IsPublished { get; set; }
        public DateTime CreatedAt { get; set; }

        public List<CourseModule> Modules { get; set; }
        public List<Lesson> Lessons { get; set; }
        public List<Challenge> Challenges { get; set; }
        public List<Enrollment> Enrollments { get; set; }
        public List<Certificate> Certificates { get; set; }

        public Course()
        {
            Title = string.Empty;
            Slug = string.Empty;
            Description = string.Empty;
            Category = string.Empty;
            Level = "Beginner";
            EstimatedMinutes = 120;
            PassingScorePercent = 70;
            IsPublished = true;
            CreatedAt = DateTime.UtcNow;
            Modules = new List<CourseModule>();
            Lessons = new List<Lesson>();
            Challenges = new List<Challenge>();
            Enrollments = new List<Enrollment>();
            Certificates = new List<Certificate>();
        }
    }
}
