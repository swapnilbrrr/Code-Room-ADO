<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="MyDashboard.aspx.cs" Inherits="CodeRoom.WebForms.Dashboard.MyDashboard" MasterPageFile="~/Site.Master" %>

<asp:Content ID="cTitle" ContentPlaceHolderID="TitleContent" runat="server">Dashboard - Code-Room</asp:Content>

<asp:Content ID="cBody" ContentPlaceHolderID="MainContent" runat="server">
    <section class="page-shell">
        <div class="container narrow-page">
            <span class="eyebrow">CODE-ROOM</span>
            <h1>My dashboard</h1>
            <p class="page-intro">Your enrolled courses, progress and activity will appear here.</p>
            <div class="content-panel">
                <h2>Placeholder</h2>
                <p>The page structure and shared styling for this area are in place. Its data-backed behaviour is migrated in a later phase, so no content is shown here yet.</p>
            </div>
        </div>
    </section>
</asp:Content>
