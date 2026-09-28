using System;

namespace CodeRoom.WebForms.Controls
{
    /// <summary>
    /// Replaces the NotificationBellViewComponent from the source application.
    /// Reads are added in Phase 4; until then the bell renders its empty state.
    /// The mark-all-read and open actions cannot stay as nested HTML forms because
    /// Web Forms allows a single server form per page, so they become postback controls later.
    /// </summary>
    public partial class NotificationBell : System.Web.UI.UserControl
    {
        protected int UnreadCount { get; private set; }
        protected string SummaryText { get; private set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            UnreadCount = 0;
            SummaryText = "All caught up";
            PhCount.Visible = UnreadCount > 0;
        }
    }
}
