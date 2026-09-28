namespace CodeRoom.WebForms.Models
{
    /// <summary>A hands on task inside a course, validated against an expected answer.</summary>
    public class Challenge
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public int? LessonId { get; set; }
        public string Title { get; set; }
        public string Instructions { get; set; }
        public string StarterCode { get; set; }
        public string Hint { get; set; }
        public string ExpectedAnswer { get; set; }
        public string ValidationMode { get; set; }
        public int Points { get; set; }

        public Challenge()
        {
            Title = string.Empty;
            Instructions = string.Empty;
            ExpectedAnswer = string.Empty;
            ValidationMode = "Exact";
            Points = 50;
        }
    }
}
