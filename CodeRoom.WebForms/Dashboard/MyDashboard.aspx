<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="MyDashboard.aspx.cs" Inherits="CodeRoom.WebForms.Dashboard.MyDashboard" MasterPageFile="~/Site.Master" %>

<asp:Content ID="cTitle" ContentPlaceHolderID="TitleContent" runat="server">Dashboard - Code-Room</asp:Content>

<asp:Content ID="cBody" ContentPlaceHolderID="MainContent" runat="server">
    <section class="page-shell dashboard-page">
        <div class="container">
            <div class="dashboard-hero">
                <div>
                    <span class="eyebrow">PERSONAL DASHBOARD</span>
                    <h1 class="page-title">Welcome back, <asp:Literal ID="WelcomeName" runat="server" /></h1>
                    <p class="page-intro">Pick up where you left off, keep your streak alive and explore your next skill.</p>
                </div>
                <div class="dashboard-identity">
                    <span><asp:Literal ID="UsernameText" runat="server" /></span>
                    <strong>Student</strong>
                </div>
            </div>

            <div class="dashboard-xp-card">
                <div>
                    <span class="eyebrow">LEVEL <asp:Literal ID="LevelText" runat="server" /></span>
                    <strong><asp:Literal ID="XpText" runat="server" /> XP</strong>
                </div>
                <div class="dashboard-xp-progress">
                    <div class="xp-bar"><span id="XpBar" runat="server"></span></div>
                    <small><asp:Literal ID="LevelProgressText" runat="server" /> / 250 XP to next level</small>
                </div>
                <a class="text-link" href="<%= ResolveUrl("~/Profile/Index.aspx") %>">View profile →</a>
            </div>

            <div class="dashboard-stat-grid">
                <article class="dashboard-stat"><span>Courses</span><strong><asp:Literal ID="CoursesCount" runat="server" /></strong><small>paths started</small></article>
                <article class="dashboard-stat"><span>Lessons</span><strong><asp:Literal ID="LessonsCount" runat="server" /></strong><small>completed</small></article>
                <article class="dashboard-stat"><span>Quiz average</span><strong><asp:Literal ID="QuizAverage" runat="server" />%</strong><small>across attempts</small></article>
                <article class="dashboard-stat"><span>Streak</span><strong>🔥 <asp:Literal ID="StreakCount" runat="server" /></strong><small>days learning</small></article>
                <article class="dashboard-stat"><span>Certificates</span><strong><asp:Literal ID="CertificateCount" runat="server" /></strong><small>earned</small></article>
            </div>

            <div class="dashboard-main-grid">
                <section class="dashboard-panel dashboard-courses-panel">
                    <div class="panel-heading">
                        <div>
                            <span class="eyebrow">CONTINUE LEARNING</span>
                            <h2>Pick up where you left off</h2>
                        </div>
                        <a class="text-link" href="<%= ResolveUrl("~/Courses/Index.aspx") %>">Explore all →</a>
                    </div>

                    <asp:PlaceHolder ID="CoursesEmpty" runat="server" Visible="false">
                        <div class="empty-state compact">
                            <div class="empty-state-icon">🧭</div>
                            <h3>Your learning path is waiting.</h3>
                            <p>Choose a course and your dashboard will start tracking your journey.</p>
                            <a class="btn btn-primary" href="<%= ResolveUrl("~/Courses/Index.aspx") %>">Explore courses</a>
                        </div>
                    </asp:PlaceHolder>

                    <asp:Repeater ID="CoursesRepeater" runat="server">
                        <HeaderTemplate><div class="dashboard-course-list"></HeaderTemplate>
                        <ItemTemplate>
                            <article class="dashboard-course-card">
                                <div>
                                    <span class="course-card-meta"><asp:Literal runat="server" Text='<%# Encode(Eval("Category")) %>' /> · <asp:Literal runat="server" Text='<%# Encode(Eval("Level")) %>' /></span>
                                    <h3><asp:Literal runat="server" Text='<%# Encode(Eval("Title")) %>' /></h3>
                                    <p><asp:Literal runat="server" Text='<%# Eval("CompletedLessons") %>' /> of <asp:Literal runat="server" Text='<%# Eval("TotalLessons") %>' /> lessons completed</p>
                                    <div class="progress-track"><span style='width:<%# Eval("Percent") %>%'></span></div>
                                </div>
                                <a class="btn btn-secondary" href='<%# ResolveUrl("~/Lessons/Index.aspx?id=" + Eval("CourseId")) %>'>Continue</a>
                            </article>
                        </ItemTemplate>
                        <FooterTemplate></div></FooterTemplate>
                    </asp:Repeater>
                </section>

                <section class="dashboard-panel dashboard-recommendations">
                    <div class="panel-heading">
                        <div>
                            <span class="eyebrow">FOR YOU</span>
                            <h2>Recommended next</h2>
                        </div>
                    </div>

                    <asp:PlaceHolder ID="RecommendationsEmpty" runat="server" Visible="false">
                        <p class="profile-empty">Complete a course and we'll keep building your next-step suggestions.</p>
                    </asp:PlaceHolder>

                    <asp:Repeater ID="RecommendationsRepeater" runat="server">
                        <HeaderTemplate><div class="recommendation-list"></HeaderTemplate>
                        <ItemTemplate>
                            <a class="recommendation-card" href='<%# ResolveUrl("~/Courses/Details.aspx?id=" + Eval("CourseId")) %>'>
                                <span class="recommendation-icon"><%# Convert.ToBoolean(Eval("IsCertification")) ? "🎓" : "✦" %></span>
                                <span>
                                    <strong><asp:Literal runat="server" Text='<%# Encode(Eval("Title")) %>' /></strong>
                                    <small><asp:Literal runat="server" Text='<%# Encode(Eval("Reason")) %>' /></small>
                                </span>
                                <span>→</span>
                            </a>
                        </ItemTemplate>
                        <FooterTemplate></div></FooterTemplate>
                    </asp:Repeater>
                </section>
            </div>

            <div class="dashboard-main-grid">
                <section class="dashboard-panel">
                    <div class="panel-heading">
                        <div>
                            <span class="eyebrow">ACTIVITY</span>
                            <h2>Your learning rhythm</h2>
                        </div>
                        <span class="profile-muted">365 days</span>
                    </div>
                    <div class="activity-grid dashboard-heatmap" aria-label="Learning activity calendar">
                        <asp:Repeater ID="ActivityDaysRepeater" runat="server">
                            <ItemTemplate><span class='activity-cell level-<%# Eval("Level") %>' title='<%# Eval("Tooltip") %>'></span></ItemTemplate>
                        </asp:Repeater>
                    </div>
                    <div class="activity-legend">
                        <span>Less</span><i class="activity-level level-0"></i><i class="activity-level level-1"></i><i class="activity-level level-2"></i><i class="activity-level level-3"></i><i class="activity-level level-4"></i><span>More</span>
                    </div>
                </section>

                <section class="dashboard-panel">
                    <div class="panel-heading">
                        <div>
                            <span class="eyebrow">RECENT</span>
                            <h2>Learning activity</h2>
                        </div>
                    </div>
                    <asp:PlaceHolder ID="RecentActivityEmpty" runat="server" Visible="false">
                        <p class="profile-empty">Complete a lesson or quiz to start your activity feed.</p>
                    </asp:PlaceHolder>
                    <asp:Repeater ID="RecentActivityRepeater" runat="server">
                        <HeaderTemplate><div class="activity-list"></HeaderTemplate>
                        <ItemTemplate>
                            <div class="activity-item">
                                <span class="activity-dot"></span>
                                <div><strong><asp:Literal runat="server" Text='<%# Encode(Eval("Description")) %>' /></strong><time><asp:Literal runat="server" Text='<%# Eval("CreatedAtLocal") %>' /></time></div>
                            </div>
                        </ItemTemplate>
                        <FooterTemplate></div></FooterTemplate>
                    </asp:Repeater>
                </section>
            </div>

            <div class="dashboard-main-grid">
                <section class="dashboard-panel">
                    <div class="panel-heading">
                        <div><span class="eyebrow">ASSESSMENTS</span><h2>Quiz history</h2></div>
                    </div>
                    <asp:PlaceHolder ID="AttemptsEmpty" runat="server" Visible="false">
                        <p class="profile-empty">No assessments yet. Take your first quiz from a course.</p>
                    </asp:PlaceHolder>
                    <asp:Repeater ID="AttemptsRepeater" runat="server">
                        <HeaderTemplate><div class="attempt-list"></HeaderTemplate>
                        <ItemTemplate>
                            <div class="attempt-row">
                                <div><strong><asp:Literal runat="server" Text='<%# Encode(Eval("Title")) %>' /></strong><small><asp:Literal runat="server" Text='<%# Eval("AttemptedAtLocal") %>' /></small></div>
                                <span class="score-pill"><asp:Literal runat="server" Text='<%# Eval("Percent") %>' />%</span>
                            </div>
                        </ItemTemplate>
                        <FooterTemplate></div></FooterTemplate>
                    </asp:Repeater>
                </section>

                <section class="dashboard-panel">
                    <div class="panel-heading">
                        <div><span class="eyebrow">NEWS</span><h2>Announcements</h2></div>
                    </div>
                    <asp:PlaceHolder ID="AnnouncementsEmpty" runat="server" Visible="false">
                        <p class="profile-empty">There are no published announcements right now.</p>
                    </asp:PlaceHolder>
                    <asp:Repeater ID="AnnouncementsRepeater" runat="server">
                        <HeaderTemplate><div class="announcement-list"></HeaderTemplate>
                        <ItemTemplate>
                            <article class="announcement-card">
                                <strong><asp:Literal runat="server" Text='<%# Encode(Eval("Title")) %>' /></strong>
                                <p><asp:Literal runat="server" Text='<%# Encode(Eval("Message")) %>' /></p>
                                <time><asp:Literal runat="server" Text='<%# Eval("PublishedAtLocal") %>' /></time>
                            </article>
                        </ItemTemplate>
                        <FooterTemplate></div></FooterTemplate>
                    </asp:Repeater>
                </section>
            </div>

            <div class="dashboard-footer-actions">
                <a class="btn btn-secondary" href='<%= PracticeUrl %>'><asp:Literal ID="PracticeLabel" runat="server" /></a>
                <a class="btn btn-secondary" href="<%= ResolveUrl("~/Certificates/Index.aspx") %>">Certificates</a>
            </div>
        </div>
    </section>
</asp:Content>
