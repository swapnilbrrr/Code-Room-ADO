<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="CodeRoom.WebForms.Authentication.Login" MasterPageFile="~/Site.Master" %>

<asp:Content ID="cTitle" ContentPlaceHolderID="TitleContent" runat="server">Login - Code-Room</asp:Content>

<asp:Content ID="cBody" ContentPlaceHolderID="MainContent" runat="server">
    <section class="auth-shell">
        <div class="auth-card">
            <span class="eyebrow">WELCOME BACK</span>
            <h1>Sign in to Code-Room</h1>
            <p>Continue your learning journey.</p>

            <div class="content-panel">
                <h2>Sign-in arrives in Phase 3</h2>
                <p>This page reserves the URL, layout and styling used by the source application. Credential checking is wired to the ADO.NET data layer in the next phase, so no sign-in is possible yet.</p>
            </div>

            <p class="auth-footer">New to Code-Room? <a href='<%= ResolveUrl("~/Authentication/Register.aspx") %>'>Create an account</a></p>
        </div>
    </section>
</asp:Content>
