using System;

namespace CodeRoom.WebForms.Models
{
    /// <summary>A message in a learner's notification bell.</summary>
    public class Notification
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string LinkUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }

        public Notification()
        {
            Type = string.Empty;
            Title = string.Empty;
            Message = string.Empty;
            CreatedAt = DateTime.UtcNow;
        }
    }
}
