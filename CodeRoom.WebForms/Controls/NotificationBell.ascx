<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="NotificationBell.ascx.cs" Inherits="CodeRoom.WebForms.Controls.NotificationBell" %>
<div class="notification-menu">
    <button class="icon-button notification-trigger" type="button" data-notification-toggle aria-expanded="false" aria-controls="notification-popover" aria-label="Open notifications" title="Notifications">
        <svg class="icon-svg notification-bell" viewBox="0 0 24 24" aria-hidden="true">
            <path d="M18 9a6 6 0 0 0-12 0c0 7-3 7-3 9h18c0-2-3-2-3-9"></path>
            <path d="M10 21h4"></path>
        </svg>
        <asp:PlaceHolder ID="PhCount" runat="server" Visible="false">
            <span class="notification-count"><%: UnreadCount %></span>
        </asp:PlaceHolder>
    </button>

    <div id="notification-popover" class="notification-popover" data-notification-menu hidden>
        <div class="notification-popover-header">
            <div>
                <strong>Notifications</strong>
                <span><%: SummaryText %></span>
            </div>
        </div>

        <div class="notification-popover-list">
            <div class="notification-empty">No new updates yet.</div>
        </div>

        <a class="notification-view-all" href='<%= ResolveUrl("~/Notifications/Index.aspx") %>'>View all notifications &rarr;</a>
    </div>
</div>
