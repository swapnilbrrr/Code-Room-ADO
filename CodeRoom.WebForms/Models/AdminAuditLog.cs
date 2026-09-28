using System;

namespace CodeRoom.WebForms.Models
{
    /// <summary>An audit entry written whenever an administrator changes content.</summary>
    public class AdminAuditLog
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Action { get; set; }
        public string EntityType { get; set; }
        public string EntityName { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }

        public AdminAuditLog()
        {
            Action = string.Empty;
            EntityType = string.Empty;
            Description = string.Empty;
            CreatedAt = DateTime.UtcNow;
        }
    }
}
