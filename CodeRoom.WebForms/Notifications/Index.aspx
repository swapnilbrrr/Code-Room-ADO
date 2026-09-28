<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Index.aspx.cs" Inherits="CodeRoom.WebForms.Notifications.Index" MasterPageFile="~/Site.Master" %>
<asp:Content ID="cTitle" ContentPlaceHolderID="TitleContent" runat="server">Notifications - Code-Room</asp:Content>
<asp:Content ID="cBody" ContentPlaceHolderID="MainContent" runat="server">
<section class="page-shell notifications-page"><div class="container">
 <div class="page-heading-row"><div><span class="eyebrow">NOTIFICATION CENTRE</span><h1 class="page-title">Stay in the loop.</h1><p class="page-intro">Announcements, course milestones, achievements and account updates.</p></div><asp:Button ID="MarkAllReadButton" runat="server" CssClass="btn btn-secondary" Text="Mark all as read" OnClick="MarkAllReadButton_Click" CausesValidation="false" Visible="false" /></div>
 <asp:Panel ID="EmptyPanel" runat="server" CssClass="empty-state" Visible="false"><h2>You're all caught up.</h2><p>New updates will appear here.</p></asp:Panel>
 <div class="notifications-list"><asp:Repeater ID="NotificationsRepeater" runat="server">
  <ItemTemplate><article class='notification-page-item <%# Convert.ToBoolean(Eval("IsRead")) ? "" : "is-unread" %>'>
   <div class="notification-icon"><%# Eval("Icon") %></div><div class="notification-page-copy"><div class="notification-page-top"><strong><%# Eval("Title") %></strong><time><%# FormatDate(Eval("CreatedAt")) %></time></div><p><%# Eval("Message") %></p>
   <div class="notification-page-meta"><span><%# Eval("Label") %></span>
    <asp:PlaceHolder ID="UnreadOpen" runat="server" Visible='<%# !Convert.ToBoolean(Eval("IsRead")) && Convert.ToBoolean(Eval("HasLink")) %>'><asp:Button ID="OpenButton" runat="server" CssClass="text-link notification-open-button" Text="Open →" CommandArgument='<%# Eval("NotificationId") %>' OnCommand="OpenButton_Command" CausesValidation="false" /></asp:PlaceHolder>
    <asp:PlaceHolder ID="ReadOpen" runat="server" Visible='<%# Convert.ToBoolean(Eval("IsRead")) && Convert.ToBoolean(Eval("HasLink")) %>'><a class="text-link" href='<%# Eval("LinkUrl") %>'>Open <span>→</span></a></asp:PlaceHolder>
   </div></div>
  </article></ItemTemplate>
 </asp:Repeater></div>
</div></section>
</asp:Content>