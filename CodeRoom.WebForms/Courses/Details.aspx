<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Details.aspx.cs" Inherits="CodeRoom.WebForms.Courses.Details" MasterPageFile="~/Site.Master" %>

<asp:Content ID="cTitle" ContentPlaceHolderID="TitleContent" runat="server"><%: CourseModel == null ? "Course details" : CourseModel.Title %> - Code-Room</asp:Content>

<asp:Content ID="cBody" ContentPlaceHolderID="MainContent" runat="server">
    <section class="page-shell">
        <div class="container course-detail">
            <asp:PlaceHolder ID="CourseNotFound" runat="server" Visible="false">
                <span class="eyebrow">CODE-ROOM</span>
                <h1 class="page-title">Course not found</h1>
                <p class="page-intro">The requested course could not be found.</p>
            </asp:PlaceHolder>

            <asp:PlaceHolder ID="CourseContent" runat="server">
                <div class="course-detail-hero">
                    <div>
                        <span class="eyebrow"><%#: CourseModel.Category.ToUpperInvariant() %></span>
                        <h1 class="page-title"><%#: CourseModel.Title %></h1>
                        <p class="page-intro"><%#: CourseModel.Description %></p>
                        <div class="course-meta-row">
                            <span class="level-pill"><%#: CourseModel.Level %></span>
                            <span>⏱ <%#: CourseModel.EstimatedMinutes %> min</span>
                            <span>📚 <%# CourseModel.Lessons.Count %> lessons</span>
                            <asp:PlaceHolder runat="server" Visible='<%# CourseModel.IsCertification %>'>
                                <span class="cert-pill">🎓 Certification path</span>
                            </asp:PlaceHolder>
                        </div>
                    </div>

                    <div class="course-start-card">
                        <asp:PlaceHolder runat="server" Visible='<%# CourseModel.IsCertification %>'>
                            <span class="eyebrow">CERTIFICATION</span>
                            <strong><%#: string.IsNullOrWhiteSpace(CourseModel.CertificateName) ? "Code-Room Certificate" : CourseModel.CertificateName %></strong>
                            <span>Pass the final exam with <%#: CourseModel.PassingScorePercent %>% or higher.</span>
                        </asp:PlaceHolder>

                        <asp:PlaceHolder runat="server" Visible='<%# !CourseModel.IsCertification %>'>
                            <span class="eyebrow">LEARNING PATH</span>
                            <strong>Learn → practise → assess</strong>
                            <span>Move through the modules at your own pace.</span>
                        </asp:PlaceHolder>

                        <asp:PlaceHolder runat="server" Visible='<%# IsAuthenticated && IsEnrolled %>'>
                            <a class="btn btn-primary btn-full"
                               href='<%# ResolveUrl("~/Lessons/Index.aspx?id=" + CourseModel.Id) %>'>Continue learning</a>
                        </asp:PlaceHolder>

                        <asp:PlaceHolder runat="server" Visible='<%# IsAuthenticated && !IsEnrolled %>'>
                            <asp:HiddenField ID="CourseCsrfToken" runat="server" />
                            <asp:Button ID="EnrollButton" runat="server"
                                CssClass="btn btn-primary btn-full"
                                Text="Enroll &amp; start learning"
                                OnClick="EnrollButton_Click" />
                        </asp:PlaceHolder>

                        <asp:PlaceHolder runat="server" Visible='<%# !IsAuthenticated %>'>
                            <a class="btn btn-primary btn-full"
                               href='<%# LoginToStartUrl %>'>Log in to start</a>
                        </asp:PlaceHolder>

                        <asp:PlaceHolder runat="server" Visible='<%# QuizModel != null %>'>
                            <a class="btn btn-secondary btn-full"
                               href='<%# ResolveUrl("~/Quiz/Take.aspx?id=" + QuizModel.Id) %>'>
                                <%# CourseModel.IsCertification ? "Take final exam" : "Take knowledge check" %> →
                            </a>
                        </asp:PlaceHolder>

                        <asp:PlaceHolder runat="server" Visible='<%# CourseModel.Challenges.Count > 0 %>'>
                            <a class="btn btn-secondary btn-full"
                               href='<%# ResolveUrl("~/Challenges/Index.aspx?id=" + CourseModel.Id) %>'>Practice challenges →</a>
                        </asp:PlaceHolder>
                    </div>
                </div>

                <div class="course-detail-grid">
                    <section class="content-panel">
                        <div class="panel-heading">
                            <div>
                                <span class="eyebrow">CURRICULUM</span>
                                <h2>Course roadmap</h2>
                            </div>
                            <span class="profile-muted"><%# CourseModel.Modules.Count %> modules</span>
                        </div>

                        <asp:PlaceHolder runat="server" Visible='<%# CourseModel.Modules.Count > 0 %>'>
                            <div class="module-list">
                                <asp:Repeater ID="ModulesRepeater" runat="server">
                                    <ItemTemplate>
                                        <details class="module-card" open>
                                            <summary>
                                                <span class="module-number"><%# string.Format("{0:D2}", Eval("Order")) %></span>
                                                <span>
                                                    <strong><%#: Eval("Title") %></strong>
                                                    <small><%#: Eval("Description") %></small>
                                                </span>
                                                <span>⌄</span>
                                            </summary>
                                            <div class="module-lessons">
                                                <asp:Repeater ID="ModuleLessonsRepeater" runat="server"
                                                    DataSource='<%# Eval("Lessons") %>'>
                                                    <ItemTemplate>
                                                        <a class="module-lesson-row"
                                                           href='<%# ResolveUrl("~/Lessons/Index.aspx?id=" + Eval("CourseId") + "&lessonId=" + Eval("Id")) %>'>
                                                            <span class="content-type-icon"><%# ContentTypeIcon(Eval("ContentType")) %></span>
                                                            <span>
                                                                <strong><%#: Eval("Title") %></strong>
                                                                <small><%#: Eval("Summary") %></small>
                                                            </span>
                                                            <span class="module-lesson-time"><%#: Eval("DurationMinutes") %> min</span>
                                                        </a>
                                                    </ItemTemplate>
                                                </asp:Repeater>
                                            </div>
                                        </details>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </div>
                        </asp:PlaceHolder>

                        <asp:PlaceHolder runat="server" Visible='<%# CourseModel.Modules.Count == 0 %>'>
                            <ol class="lesson-outline">
                                <asp:Repeater ID="LessonsOutlineRepeater" runat="server" DataSource='<%# CourseModel.Lessons %>'>
                                    <ItemTemplate>
                                        <li>
                                            <span><%# string.Format("{0:D2}", Eval("Order")) %></span>
                                            <strong><%#: Eval("Title") %></strong>
                                        </li>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </ol>
                        </asp:PlaceHolder>
                    </section>

                    <aside class="course-side-stack">
                        <asp:PlaceHolder runat="server" Visible='<%# Resources.Count > 0 %>'>
                            <section class="content-panel">
                                <div class="panel-heading">
                                    <div><span class="eyebrow">RESOURCES</span><h2>Study materials</h2></div>
                                </div>
                                <div class="resource-list">
                                    <asp:Repeater ID="ResourcesRepeater" runat="server" DataSource='<%# Resources %>'>
                                        <ItemTemplate>
                                            <a class="resource-card" href='<%#: Eval("Url") %>' target="_blank" rel="noopener noreferrer">
                                                <span class="resource-type"><%#: Eval("Type") %></span>
                                                <strong><%#: Eval("Title") %></strong>
                                                <span>Open resource ↗</span>
                                            </a>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                </div>
                            </section>
                        </asp:PlaceHolder>

                        <asp:PlaceHolder runat="server" Visible='<%# CourseModel.Challenges.Count > 0 %>'>
                            <section class="content-panel challenge-summary-card">
                                <span class="eyebrow">PRACTICE LAB</span>
                                <h2><%# CourseModel.Challenges.Count %> challenges</h2>
                                <p>Apply the concepts with short, repeatable tasks and earn XP as you pass them.</p>
                                <a class="btn btn-secondary btn-full"
                                   href='<%# ResolveUrl("~/Challenges/Index.aspx?id=" + CourseModel.Id) %>'>Open practice lab</a>
                            </section>
                        </asp:PlaceHolder>
                    </aside>
                </div>
            </asp:PlaceHolder>
        </div>
    </section>
</asp:Content>
