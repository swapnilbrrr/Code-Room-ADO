<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Index.aspx.cs" Inherits="CodeRoom.WebForms.Courses.Index" MasterPageFile="~/Site.Master" %>

<asp:Content ID="cTitle" ContentPlaceHolderID="TitleContent" runat="server">Courses - Code-Room</asp:Content>

<asp:Content ID="cBody" ContentPlaceHolderID="MainContent" runat="server">
    <section class="page-shell">
        <div class="container">
            <div class="catalogue-header">
                <div>
                    <span class="eyebrow">LEARNING LIBRARY</span>
                    <h1 class="page-title">Courses</h1>
                    <p class="page-intro">Focused courses that take you from fundamentals to practical technology skills.</p>
                </div>
                <div class="catalogue-count" aria-label="Available course count">
                    <strong><%# Courses.Count %></strong>
                    <span>courses</span>
                </div>
            </div>

            <div class="course-toolbar" aria-label="Course filters">
                <label class="search-field">
                    <span class="sr-only">Search courses</span>
                    <input class="input" type="search" data-course-search placeholder="Search by course or topic..." />
                </label>
                <label>
                    <span class="sr-only">Filter by category</span>
                    <select class="input" data-course-category>
                        <option value="all">All categories</option>
                        <option value="programming">Programming</option>
                        <option value="web">Web development</option>
                        <option value="security">Cybersecurity</option>
                        <option value="database">Databases</option>
                        <option value="networking">Networking</option>
                        <option value="cloud">Cloud</option>
                        <option value="linux">Linux</option>
                    </select>
                </label>
                <label>
                    <span class="sr-only">Filter by level</span>
                    <select class="input" data-course-level>
                        <option value="all">All levels</option>
                        <option value="beginner">Beginner</option>
                        <option value="intermediate">Intermediate</option>
                        <option value="advanced">Advanced</option>
                    </select>
                </label>
            </div>

            <div class="course-grid" aria-live="polite">
                <asp:Repeater ID="CoursesRepeater" runat="server">
                    <ItemTemplate>
                        <article class="course-card" data-course-card
                            data-category="<%# CategorySlug(Eval("Category")) %>"
                            data-level="<%# LevelSlug(Eval("Level")) %>">
                            <div class="course-card-top">
                                <span class="course-tag"><%#: Eval("Category") %></span>
                                <span class="level-pill"><%#: Eval("Level") %></span>
                            </div>
                            <asp:PlaceHolder runat="server" Visible='<%# Convert.ToBoolean(Eval("IsCertification")) %>'>
                                <span class="cert-pill">🎓 Certification path</span>
                            </asp:PlaceHolder>
                            <h2><%#: Eval("Title") %></h2>
                            <p><%#: Eval("Description") %></p>
                            <div class="course-meta">
                                <span><%# LessonCount(Eval("Lessons")) %> lessons</span>
                                <span>⏱ <%#: Eval("EstimatedMinutes") %> min</span>
                                <asp:PlaceHolder runat="server" Visible='<%# Convert.ToBoolean(Eval("IsCertification")) %>'>
                                    <span>Pass <%#: Eval("PassingScorePercent") %>%</span>
                                </asp:PlaceHolder>
                            </div>
                            <a class="btn btn-secondary"
                               href='<%# ResolveUrl("~/Courses/Details.aspx?id=" + Eval("Id")) %>'>View course</a>
                        </article>
                    </ItemTemplate>
                </asp:Repeater>
            </div>

            <div class="empty-state" data-course-empty hidden>
                <h2>No courses found</h2>
                <p>Try a different search term or filter.</p>
            </div>
        </div>
    </section>
</asp:Content>
