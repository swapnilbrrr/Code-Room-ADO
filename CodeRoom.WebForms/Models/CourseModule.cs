using System.Collections.Generic;

namespace CodeRoom.WebForms.Models
{
    /// <summary>A stage inside a course. Lessons attach to a module.</summary>
    public class CourseModule
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int Order { get; set; }

        public List<Lesson> Lessons { get; set; }

        public CourseModule()
        {
            Title = string.Empty;
            Description = string.Empty;
            Lessons = new List<Lesson>();
        }
    }
}
