using System;

namespace CodeRoom.WebForms.Models
{
    /// <summary>A platform wide notice published by an administrator.</summary>
    public class Announcement
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public DateTime PublishedAt { get; set; }
        public bool IsPublished { get; set; }

        public Announcement()
        {
            Title = string.Empty;
            Message = string.Empty;
            PublishedAt = DateTime.UtcNow;
            IsPublished = true;
        }
    }
}
