using System;
using CodeRoom.WebForms.Data;
using CodeRoom.WebForms.Models;

namespace CodeRoom.WebForms.Services
{
    /// <summary>Central audit writer for Web Forms administration actions.</summary>
    public static class AdminAuditService
    {
        public static void Record(int userId, string action, string entityType, string entityName, string description)
        {
            new AdminRepository().Record(new AdminAuditLog
            {
                UserId = userId,
                Action = action,
                EntityType = entityType,
                EntityName = entityName,
                Description = description,
                CreatedAt = DateTime.UtcNow
            });
        }
    }
}