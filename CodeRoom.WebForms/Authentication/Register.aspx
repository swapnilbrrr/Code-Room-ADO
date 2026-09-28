<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Register.aspx.cs" Inherits="CodeRoom.WebForms.Authentication.Register" MasterPageFile="~/Site.Master" %>

<asp:Content ID="cTitle" ContentPlaceHolderID="TitleContent" runat="server">Register - Code-Room</asp:Content>

<asp:Content ID="cBody" ContentPlaceHolderID="MainContent" runat="server">
    <section class="auth-shell">
        <div class="auth-card">
            <span class="eyebrow">GET STARTED</span>
            <h1>Create your account</h1>
            <p>Join Code-Room and start learning at your own pace.</p>

            <div class="content-panel">
                <h2>Registration arrives in Phase 3</h2>
                <p>This page reserves the URL, layout and styling used by the source application. Account creation is wired to the ADO.NET data layer in the next phase, so no account can be created yet.</p>
            </div>

            <p class="auth-footer">Already registered? <a href='<%= ResolveUrl("~/Authentication/Login.aspx") %>'>Sign in</a></p>
        </div>
    </section>
</asp:Content>
