<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Index.aspx.cs" Inherits="CodeRoom.WebForms.Lessons.Index" MasterPageFile="~/Site.Master" %>

<asp:Content ID="cTitle" ContentPlaceHolderID="TitleContent" runat="server"><%: CurrentLesson == null ? "Lesson" : CurrentLesson.Title %> - Code-Room</asp:Content>

<asp:Content ID="cBody" ContentPlaceHolderID="MainContent" runat="server">
    <section class="page-shell">
        <div class="container lesson-layout">
            <asp:PlaceHolder ID="LessonNotFound" runat="server" Visible="false">
                <div class="content-panel narrow-page">
                    <span class="eyebrow">CODE-ROOM</span>
                    <h1>Lesson not found</h1>
                    <p class="page-intro">The requested lesson could not be found.</p>
                    <a class="btn btn-primary" href='<%= ResolveUrl("~/Courses/Index.aspx") %>'>Back to courses</a>
                </div>
            </asp:PlaceHolder>

            <asp:PlaceHolder ID="LessonContent" runat="server">
                <main class="content-panel lesson-content">
                    <div class="lesson-kicker">
                        <span class="eyebrow"><%#: CourseModel.Title %></span>
                        <span class="lesson-content-type"><%#: CurrentLesson.ContentType %> · <%#: CurrentLesson.DurationMinutes %> min</span>
                    </div>
                    <h1><%#: CurrentLesson.Title %></h1>
                    <asp:PlaceHolder runat="server" Visible='<%# !string.IsNullOrWhiteSpace(CurrentLesson.Summary) %>'>
                        <p class="lesson-summary"><%#: CurrentLesson.Summary %></p>
                    </asp:PlaceHolder>

                    <div class="lesson-tool-row">
                        <button type="button" class="btn btn-secondary" data-lesson-speak aria-label="Read lesson aloud">🔊 Read aloud</button>
                        <button type="button" class="btn btn-secondary" data-lesson-stop-speech hidden>■ Stop audio</button>
                        <asp:PlaceHolder runat="server" Visible='<%# !string.IsNullOrWhiteSpace(CurrentLesson.AudioUrl) %>'>
                            <audio controls preload="none" class="lesson-audio" aria-label="Audio lesson">
                                <source src='<%#: CurrentLesson.AudioUrl %>' />
                            </audio>
                        </asp:PlaceHolder>
                    </div>

                    <asp:PlaceHolder runat="server" Visible='<%# !string.IsNullOrWhiteSpace(CurrentLesson.VideoUrl) %>'>
                        <div class="lesson-video">
                            <iframe
                                class="lesson-video-frame"
                                src='<%#: CurrentLesson.VideoUrl %>'
                                title='<%#: "Video lesson: " + CurrentLesson.Title %>'
                                loading="lazy"
                                allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share"
                                allowfullscreen></iframe>
                            <p class="lesson-video-caption">Watch the supporting tutorial, then use the notes and practice task below to reinforce the idea.</p>
                        </div>
                    </asp:PlaceHolder>

                    <article class="lesson-copy" data-lesson-content>
                        <asp:Repeater ID="ContentParagraphsRepeater" runat="server">
                            <ItemTemplate>
                                <asp:PlaceHolder runat="server" Visible='<%# Convert.ToBoolean(Eval("IsSection")) %>'>
                                    <div class="lesson-copy-block">
                                        <span class="lesson-section-label"><%#: Eval("Label") %></span>
                                        <p><%#: Eval("Body") %></p>
                                    </div>
                                </asp:PlaceHolder>
                                <asp:PlaceHolder runat="server" Visible='<%# !Convert.ToBoolean(Eval("IsSection")) %>'>
                                    <p><%#: Eval("Body") %></p>
                                </asp:PlaceHolder>
                            </ItemTemplate>
                        </asp:Repeater>
                    </article>

                    <asp:PlaceHolder runat="server" Visible='<%# !string.IsNullOrWhiteSpace(CurrentLesson.ResourceUrl) %>'>
                        <div class="lesson-resource">
                            <strong>SUPPORTING RESOURCE</strong>
                            <a href='<%#: CurrentLesson.ResourceUrl %>' target="_blank" rel="noopener noreferrer">Open the reference material ↗</a>
                        </div>
                    </asp:PlaceHolder>

                    <div class="lesson-practice-strip">
                        <div>
                            <span class="eyebrow">NEXT STEP</span>
                            <h2>Turn the concept into a skill.</h2>
                            <p>Finish the lesson, then test yourself with a quiz or practical challenge.</p>
                        </div>
                        <div class="lesson-practice-actions">
                            <asp:PlaceHolder runat="server" Visible='<%# ChallengeModel != null %>'>
                                <a class="btn btn-secondary" href='<%# ChallengeUrl() %>'>⚡ Practice challenge</a>
                            </asp:PlaceHolder>
                            <asp:PlaceHolder runat="server" Visible='<%# QuizModel != null %>'>
                                <a class="btn btn-secondary" href='<%# QuizUrl() %>'>🧠 <%# CourseModel.IsCertification ? "Final exam" : "Knowledge check" %></a>
                            </asp:PlaceHolder>
                        </div>
                    </div>

                    <div class="lesson-actions">
                        <asp:PlaceHolder runat="server" Visible='<%# CompletedLessonIds.Contains(CurrentLesson.Id) %>'>
                            <span class="status-pill status-done">✓ Completed</span>
                        </asp:PlaceHolder>

                        <asp:PlaceHolder runat="server" Visible='<%# !CompletedLessonIds.Contains(CurrentLesson.Id) %>'>
                            <asp:HiddenField ID="CompleteCsrfToken" runat="server" />
                            <asp:Button ID="CompleteButton" runat="server"
                                CssClass="btn btn-primary"
                                Text="Mark lesson complete"
                                OnClick="CompleteButton_Click"
                                CausesValidation="false" />
                        </asp:PlaceHolder>
                    </div>

                    <div class="lesson-nav" aria-label="Lesson navigation">
                        <asp:PlaceHolder runat="server" Visible='<%# PreviousLesson != null %>'>
                            <a class="btn btn-secondary"
                               href='<%# PreviousLessonUrl %>'>← Previous lesson</a>
                        </asp:PlaceHolder>
                        <asp:PlaceHolder runat="server" Visible='<%# PreviousLesson == null %>'>
                            <span></span>
                        </asp:PlaceHolder>

                        <asp:PlaceHolder runat="server" Visible='<%# NextLesson != null %>'>
                            <a class="btn btn-secondary"
                               href='<%# NextLessonUrl %>'>Next lesson →</a>
                        </asp:PlaceHolder>
                    </div>
                </main>

                <aside class="content-panel lesson-sidebar">
                    <div class="lesson-sidebar-head">
                        <div>
                            <span class="eyebrow">ROADMAP</span>
                            <h3><%#: ModuleName %></h3>
                        </div>
                        <span class="lesson-sidebar-progress"><%#: ProgressPercent %>%</span>
                    </div>
                    <div class="progress-track"><span style='<%# "width: " + ProgressPercent + "%" %>'></span></div>
                    <small><%#: CompletedCount %> of <%#: TotalCount %> lessons completed</small>
                    <hr />
                    <asp:Repeater ID="LessonsRepeater" runat="server" DataSource='<%# Lessons %>'>
                        <ItemTemplate>
                            <a class='<%# "lesson-link " + LessonClass(Eval("Id")) %>'
                               href='<%# LessonUrl(Eval("Id")) %>'>
                                <span><%#: LessonStatus(Eval("Id")) %><%#: string.Format("{0:D2}", Eval("Order")) %>. <%#: Eval("Title") %></span>
                                <small><%#: Eval("DurationMinutes") %> min</small>
                            </a>
                        </ItemTemplate>
                    </asp:Repeater>
                </aside>
            </asp:PlaceHolder>
        </div>
    </section>
</asp:Content>

<asp:Content ID="cScripts" ContentPlaceHolderID="ScriptsContent" runat="server">
<script>
(() => {
    const content = document.querySelector('[data-lesson-content]');
    const speak = document.querySelector('[data-lesson-speak]');
    const stop = document.querySelector('[data-lesson-stop-speech]');
    if (!content || !speak || !('speechSynthesis' in window)) return;

    speak.addEventListener('click', () => {
        window.speechSynthesis.cancel();
        const utterance = new SpeechSynthesisUtterance(content.innerText);
        utterance.rate = 0.95;
        utterance.addEventListener('end', () => {
            stop.hidden = true;
        });
        window.speechSynthesis.speak(utterance);
        stop.hidden = false;
    });

    if (stop) {
        stop.addEventListener('click', () => {
            window.speechSynthesis.cancel();
            stop.hidden = true;
        });
    }
})();
</script>
</asp:Content>
