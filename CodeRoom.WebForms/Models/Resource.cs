namespace CodeRoom.WebForms.Models
{
    /// <summary>A reference link shown with a course, or globally when CourseId is null.</summary>
    public class Resource
    {
        public int Id { get; set; }
        public int? CourseId { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public string Type { get; set; }

        public Resource()
        {
            Title = string.Empty;
            Url = string.Empty;
            Type = "Document";
        }
    }
}
