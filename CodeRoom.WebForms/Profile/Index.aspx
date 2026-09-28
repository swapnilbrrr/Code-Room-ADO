<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Index.aspx.cs" Inherits="CodeRoom.WebForms.Profile.Index" MasterPageFile="~/Site.Master" %>
<asp:Content ID="cTitle" ContentPlaceHolderID="TitleContent" runat="server">Profile - Code-Room</asp:Content>
<asp:Content ID="cBody" ContentPlaceHolderID="MainContent" runat="server">
<section class="page-shell profile-page"><div class="container">
 <div class="profile-hero profile-hero-enhanced">
  <div class="profile-identity">
   <asp:Panel ID="AvatarImagePanel" runat="server" Visible="false"><asp:Image ID="AvatarImage" runat="server" CssClass="profile-avatar profile-avatar-large" /></asp:Panel>
   <asp:Panel ID="AvatarInitialPanel" runat="server"><asp:Literal ID="AvatarInitial" runat="server" /></asp:Panel>
   <div><span class="eyebrow">CODE-ROOM PROFILE</span><h1 class="page-title"><asp:Literal ID="FullName" runat="server" /></h1><p class="profile-handle">@<asp:Literal ID="Username" runat="server" /> · <asp:Literal ID="RoleLabel" runat="server" /></p><p class="profile-bio"><asp:Literal ID="Bio" runat="server" /></p></div>
  </div>
  <div class="profile-actions"><a id="DashboardButton" runat="server" class="btn btn-secondary">Dashboard</a><a class="btn btn-primary" href='<%= ResolveUrl("~/Profile/Settings.aspx") %>'>Edit profile</a></div>
 </div>
 <asp:Panel ID="AdminPanel" runat="server" Visible="false">
  <div class="profile-stat-grid">
   <article class="profile-stat"><strong><asp:Literal ID="AdminUsers" runat="server" /></strong><span>Platform users</span></article>
   <article class="profile-stat"><strong><asp:Literal ID="AdminCourses" runat="server" /></strong><span>Courses managed</span></article>
   <article class="profile-stat"><strong><asp:Literal ID="AdminAnnouncements" runat="server" /></strong><span>Published announcements</span></article>
   <article class="profile-stat"><strong><asp:Literal ID="AdminAuditEvents" runat="server" /></strong><span>Your audit events</span></article>
   <article class="profile-stat"><strong><asp:Literal ID="AdminCertificates" runat="server" /></strong><span>Certificates issued</span></article>
  </div>
  <div class="profile-main-grid">
   <section class="profile-panel"><div class="panel-heading"><div><span class="eyebrow">ADMIN ACTIVITY</span><h2>Recent platform actions</h2></div><a class="text-link" href='<%= ResolveUrl("~/Admin/Audit.aspx") %>'>Open audit log →</a></div>
    <asp:Panel ID="AdminEmpty" runat="server"><p class="profile-empty">Administrative changes will appear here as you manage the platform.</p></asp:Panel>
    <asp:Repeater ID="AdminActivityRepeater" runat="server"><ItemTemplate><div class="activity-item"><span class="activity-dot"></span><div><strong><%#: Eval("Description") %></strong><time><%# FormatActivityDate(Eval("CreatedAt")) %></time></div></div></ItemTemplate></asp:Repeater>
   </section>
   <section class="profile-panel"><div class="panel-heading"><div><span class="eyebrow">GOVERNANCE</span><h2>Administration workspace</h2></div></div>
    <div class="recommendation-list">
     <a class="recommendation-card" href='<%= ResolveUrl("~/Admin/Users.aspx") %>'><span class="recommendation-icon">01</span><span><strong>Manage users</strong><small>Accounts, usernames and privileged roles.</small></span><span>→</span></a>
     <a class="recommendation-card" href='<%= ResolveUrl("~/Admin/Challenges.aspx") %>'><span class="recommendation-icon">02</span><span><strong>Manage practice</strong><small>Challenges and hands-on learning tasks.</small></span><span>→</span></a>
     <a class="recommendation-card" href='<%= ResolveUrl("~/Admin/Announcements.aspx") %>'><span class="recommendation-icon">03</span><span><strong>Announcements</strong><small>Publish updates to the learner community.</small></span><span>→</span></a>
    </div>
   </section>
  </div>
 </asp:Panel>
 <asp:Panel ID="StudentPanel" runat="server">
  <div class="xp-profile-card"><div><span class="eyebrow">LEVEL <asp:Literal ID="Level" runat="server" /></span><h2><asp:Literal ID="Xp" runat="server" /> XP</h2><p><asp:Literal ID="XpUntilLevel" runat="server" /> XP until your next level</p></div><div class="xp-bar-wrap"><div class="xp-bar"><span id="XpBar" runat="server"></span></div><div class="xp-bar-labels"><span>Level <asp:Literal ID="LevelLeft" runat="server" /></span><span>Level <asp:Literal ID="LevelRight" runat="server" /></span></div></div></div>
  <div class="profile-stat-grid">
   <article class="profile-stat"><strong><asp:Literal ID="CoursesEnrolled" runat="server" /></strong><span>Courses</span></article><article class="profile-stat"><strong><asp:Literal ID="LessonsCompleted" runat="server" /></strong><span>Lessons</span></article><article class="profile-stat"><strong><asp:Literal ID="QuizAttempts" runat="server" /></strong><span>Assessments</span></article><article class="profile-stat"><strong><asp:Literal ID="LearningStreak" runat="server" /></strong><span>Day streak</span></article><article class="profile-stat"><strong><asp:Literal ID="CertificateCount" runat="server" /></strong><span>Certificates</span></article>
  </div>
  <div class="profile-main-grid profile-main-grid-three">
   <section class="profile-panel profile-activity-panel"><div class="panel-heading"><div><span class="eyebrow">ACTIVITY</span><h2>Learning history</h2></div><span class="profile-muted">Last 365 days</span></div>
    <div class="activity-legend"><span>Less</span><i class="activity-level level-0"></i><i class="activity-level level-1"></i><i class="activity-level level-2"></i><i class="activity-level level-3"></i><i class="activity-level level-4"></i><span>More</span></div>
    <div class="activity-grid" aria-label="Learning activity calendar"><asp:Repeater ID="ActivityDaysRepeater" runat="server"><ItemTemplate><span class='activity-cell level-<%# Eval("Level") %>' title='<%# Eval("Title") %>'></span></ItemTemplate></asp:Repeater></div>
   </section>
   <section class="profile-panel"><div class="panel-heading"><div><span class="eyebrow">ACHIEVEMENTS</span><h2>Unlocked</h2></div></div><asp:Panel ID="AchievementEmpty" runat="server"><p class="profile-empty">Finish lessons, quizzes and challenges to unlock your first badge.</p></asp:Panel><asp:Repeater ID="AchievementRepeater" runat="server"><ItemTemplate><div class="achievement-card"><span class="achievement-icon"><%#: Eval("Achievement.Icon") %></span><div><strong><%#: Eval("Achievement.Name") %></strong><p><%#: Eval("Achievement.Description") %></p></div></div></ItemTemplate></asp:Repeater></section>
   <section class="profile-panel"><div class="panel-heading"><div><span class="eyebrow">RECENT</span><h2>Recent activity</h2></div></div><asp:Panel ID="RecentEmpty" runat="server"><p class="profile-empty">Your activity will appear here as you start learning.</p></asp:Panel><asp:Repeater ID="RecentActivityRepeater" runat="server"><ItemTemplate><div class="activity-item"><span class="activity-dot"></span><div><strong><%#: Eval("Description") %></strong><time><%# FormatActivityDate(Eval("CreatedAt")) %></time></div></div></ItemTemplate></asp:Repeater></section>
  </div>
  <section class="profile-panel profile-about"><div class="panel-heading"><div><span class="eyebrow">ACCOUNT</span><h2>Profile details</h2></div><div class="profile-panel-actions"><a class="text-link" href='<%= ResolveUrl("~/Certificates/Index.aspx") %>'>Certificates →</a><a class="text-link" href='<%= ResolveUrl("~/Profile/Settings.aspx") %>'>Settings →</a></div></div><div class="profile-detail-grid"><div><span>Email</span><strong><asp:Literal ID="Email" runat="server" /></strong></div><div><span>Username</span><strong>@<asp:Literal ID="DetailUsername" runat="server" /></strong></div><div><span>Role</span><strong><asp:Literal ID="DetailRole" runat="server" /></strong></div><div><span>Overall progress</span><strong><asp:Literal ID="ProgressPercent" runat="server" />% complete</strong></div></div></section>
 </asp:Panel>
</div></section>
</asp:Content>