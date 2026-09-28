using System.Collections.Generic;

namespace CodeRoom.WebForms.Models
{
    /// <summary>A single study item inside a course and, where assigned, a module.</summary>
    public class Lesson
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public int? CourseModuleId { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public string Content { get; set; }
        public string ContentType { get; set; }
        public string VideoUrl { get; set; }
        public string AudioUrl { get; set; }
        public string ResourceUrl { get; set; }
        public int Order { get; set; }
        public int DurationMinutes { get; set; }
        public bool IsPublished { get; set; }

        public List<Challenge> Challenges { get; set; }

        public Lesson()
        {
            Title = string.Empty;
            Content = string.Empty;
            ContentType = "Reading";
            DurationMinutes = 10;
            IsPublished = true;
            Challenges = new List<Challenge>();
        }
    }
}
