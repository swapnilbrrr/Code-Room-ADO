<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Faq.aspx.cs" Inherits="CodeRoom.WebForms.Faq" MasterPageFile="~/Site.Master" %>

<asp:Content ID="cTitle" ContentPlaceHolderID="TitleContent" runat="server">FAQ - Code-Room</asp:Content>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">
    <section class="page-shell">
        <div class="container narrow-page">
            <span class="eyebrow">HELP CENTRE</span>
            <h1>Frequently asked questions</h1>
            <div class="faq-list">
                <details open><summary>Is Code-Room free to use?</summary><p>Code-Room is designed as a student learning platform. Access rules can be configured by administrators.</p></details>
                <details><summary>Can I track my course progress?</summary><p>Yes. The platform records completed lessons and quiz attempts for enrolled students.</p></details>
                <details><summary>Can administrators create courses?</summary><p>Yes. The planned admin area provides content management for courses, lessons, quizzes and resources.</p></details>
            </div>
        </div>
    </section>
</asp:Content>
