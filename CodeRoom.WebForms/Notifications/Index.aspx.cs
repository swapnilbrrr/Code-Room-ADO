using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using CodeRoom.WebForms.Data;
using CodeRoom.WebForms.Helpers;
using CodeRoom.WebForms.Models;
using CodeRoom.WebForms.Services;

namespace CodeRoom.WebForms.Notifications
{
    public partial class Index : Page
    {
        protected Button MarkAllReadButton;
        protected Panel EmptyPanel;
        protected Repeater NotificationsRepeater;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Auth.RequireLogin(this)) return;

            if (!IsPostBack)
                BindNotifications();
        }

        private void BindNotifications()
        {
            var repository = new NotificationRepository();
            var notifications = repository.GetAllByUser(Auth.CurrentUserId);
            var announcements = new AnnouncementRepository().GetPublished(1000)
                .Where(a => !notifications.Any(n =>
                    string.Equals(n.Type, DomainValues.NotificationType.Announcement, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(n.Title, a.Title, StringComparison.Ordinal) &&
                    string.Equals(n.Message, a.Message, StringComparison.Ordinal)))
                .ToList();

            var items = new List<NotificationItem>();
            foreach (var notification in notifications)
            {
                items.Add(new NotificationItem
                {
                    NotificationId = notification.Id,
                    Type = notification.Type,
                    Title = notification.Title,
                    Message = notification.Message,
                    LinkUrl = IsLocalUrl(notification.LinkUrl) ? notification.LinkUrl : null,
                    CreatedAt = notification.CreatedAt,
                    IsRead = notification.IsRead,
                    Label = notification.Type,
                    Icon = IconFor(notification.Type)
                });
            }

            foreach (var announcement in announcements)
            {
                items.Add(new NotificationItem
                {
                    NotificationId = null,
                    Type = DomainValues.NotificationType.Announcement,
                    Title = announcement.Title,
                    Message = announcement.Message,
                    LinkUrl = ResolveUrl("~/Notifications/Index.aspx"),
                    CreatedAt = announcement.PublishedAt,
                    IsRead = true,
                    Label = "Announcement",
                    Icon = "📢"
                });
            }

            items = items.OrderByDescending(x => x.CreatedAt).ToList();
            NotificationsRepeater.DataSource = items;
            NotificationsRepeater.DataBind();
            EmptyPanel.Visible = items.Count == 0;
            MarkAllReadButton.Visible = notifications.Any(n => !n.IsRead);
        }

        protected void MarkAllReadButton_Click(object sender, EventArgs e)
        {
            if (Auth.RequireLogin(this)) return;
            try { ((SiteMaster)Master).ValidateCsrf(); }
            catch (InvalidOperationException) { Toast.Error("Security check failed", "Refresh the page and try again."); return; }

            new NotificationRepository().MarkAllRead(Auth.CurrentUserId);
            Response.Redirect(ResolveUrl("~/Notifications/Index.aspx"), false);
            Context.ApplicationInstance.CompleteRequest();
        }

        protected void OpenButton_Command(object sender, CommandEventArgs e)
        {
            if (Auth.RequireLogin(this)) return;
            try { ((SiteMaster)Master).ValidateCsrf(); }
            catch (InvalidOperationException) { Toast.Error("Security check failed", "Refresh the page and try again."); return; }

            int notificationId;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out notificationId) || notificationId <= 0)
            {
                Response.Redirect(ResolveUrl("~/Notifications/Index.aspx"), false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            var notification = new NotificationRepository().GetById(notificationId, Auth.CurrentUserId);
            if (notification == null)
            {
                Response.Redirect(ResolveUrl("~/Notifications/Index.aspx"), false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            new NotificationRepository().MarkRead(Auth.CurrentUserId, notification.Id);
            var link = notification.LinkUrl;
            if (!string.IsNullOrWhiteSpace(link) && IsLocalUrl(link))
                Response.Redirect(ResolveUrl(link), false);
            else
                Response.Redirect(ResolveUrl("~/Notifications/Index.aspx"), false);

            Context.ApplicationInstance.CompleteRequest();
        }

        protected string FormatDate(object value)
        {
            return Convert.ToDateTime(value).ToLocalTime().ToString("dd MMM yyyy, HH:mm");
        }

        private static bool IsLocalUrl(string url)
        {
            return !string.IsNullOrWhiteSpace(url) && url[0] == '/' &&
                (url.Length == 1 || (url.Length > 1 && url[1] != '/' && url[1] != '\\'));
        }

        private static string IconFor(string type)
        {
            if (string.Equals(type, DomainValues.NotificationType.StreakMilestone, StringComparison.OrdinalIgnoreCase)) return "🔥";
            if (string.Equals(type, DomainValues.NotificationType.Achievement, StringComparison.OrdinalIgnoreCase)) return "🏆";
            if (string.Equals(type, DomainValues.NotificationType.Certificate, StringComparison.OrdinalIgnoreCase)) return "🎓";
            if (string.Equals(type, DomainValues.NotificationType.Announcement, StringComparison.OrdinalIgnoreCase)) return "📢";
            return "•";
        }

        private sealed class NotificationItem
        {
            public int? NotificationId { get; set; }
            public string Type { get; set; }
            public string Title { get; set; }
            public string Message { get; set; }
            public string LinkUrl { get; set; }
            public DateTime CreatedAt { get; set; }
            public bool IsRead { get; set; }
            public string Label { get; set; }
            public string Icon { get; set; }
            public bool HasLink { get { return !string.IsNullOrWhiteSpace(LinkUrl); } }
        }
    }
}