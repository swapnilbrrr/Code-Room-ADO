<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Index.aspx.cs" Inherits="CodeRoom.WebForms.Challenges.Index" MasterPageFile="~/Site.Master" %>

<asp:Content ID="cTitle" ContentPlaceHolderID="TitleContent" runat="server">Challenges - Code-Room</asp:Content>
<asp:Content ID="cBody" ContentPlaceHolderID="MainContent" runat="server">
<section class="page-shell">
 <div class="container">
  <div class="learning-page-heading">
   <div><span class="eyebrow">PRACTICE LAB</span><h1 class="page-title"><asp:Literal ID="CourseTitle" runat="server" /> challenges</h1><p class="page-intro">Short, focused tasks turn concepts into something you can actually do.</p></div>
   <a class="btn btn-secondary" href='<%= ResolveUrl("~/Courses/Details.aspx?id=") %><%= CourseId %>'>Back to course</a>
  </div>
  <asp:Panel ID="NotFoundPanel" runat="server" CssClass="empty-state" Visible="false">
   <div class="empty-state-icon">🧩</div><h2>Challenges unavailable.</h2><p>The requested course could not be found or is not published.</p>
  </asp:Panel>
  <asp:Panel ID="EmptyPanel" runat="server" CssClass="empty-state" Visible="false">
   <div class="empty-state-icon">🧩</div><h2>More challenges are coming.</h2><p>Continue through the course while this practice set is being expanded.</p>
  </asp:Panel>
  <asp:Repeater ID="ChallengesRepeater" runat="server">
   <HeaderTemplate><div class="challenge-grid"></HeaderTemplate>
   <ItemTemplate>
    <a class="challenge-card" href='<%# ResolveUrl("~/Challenges/Take.aspx?id=" + Eval("Id")) %>'>
     <span class="challenge-number">+<%# Eval("Points") %> XP</span>
     <h2><%#: Eval("Title") %></h2><p><%#: Eval("Instructions") %></p><span class="text-link">Start challenge →</span>
    </a>
   </ItemTemplate>
   <FooterTemplate></div></FooterTemplate>
  </asp:Repeater>
 </div>
</section>
</asp:Content>