using System;
using System.Collections.Generic;
using System.Web.UI;
using CodeRoom.WebForms.Data;
using CodeRoom.WebForms.Models;

namespace CodeRoom.WebForms.Courses
{
    public partial class Index : Page
    {
        private readonly CourseRepository courses = new CourseRepository();

        protected IReadOnlyList<Course> Courses { get; private set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            Courses = courses.GetPublished();
            CoursesRepeater.DataSource = Courses;
            CoursesRepeater.DataBind();
            DataBind();
        }

        protected string CategorySlug(object value)
        {
            var category = Convert.ToString(value) ?? string.Empty;
            switch (category)
            {
                case "Programming": return "programming";
                case "Web Development": return "web";
                case "Cybersecurity": return "security";
                case "Databases": return "database";
                case "Networking": return "networking";
                case "Cloud": return "cloud";
                case "Linux": return "linux";
                default: return "other";
            }
        }

        protected string LevelSlug(object value)
        {
            var level = (Convert.ToString(value) ?? string.Empty).ToLowerInvariant();
            switch (level)
            {
                case "beginner": return "beginner";
                case "intermediate": return "intermediate";
                case "advanced": return "advanced";
                default: return "beginner";
            }
        }

        protected int LessonCount(object value)
        {
            var lessons = value as ICollection<Lesson>;
            return lessons == null ? 0 : lessons.Count;
        }
    }
}
