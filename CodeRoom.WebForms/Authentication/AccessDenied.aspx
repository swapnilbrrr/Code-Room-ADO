<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AccessDenied.aspx.cs" Inherits="CodeRoom.WebForms.Authentication.AccessDenied" MasterPageFile="~/Site.Master" %>

<asp:Content ID="cTitle" ContentPlaceHolderID="TitleContent" runat="server">Access denied - Code-Room</asp:Content>

<asp:Content ID="cBody" ContentPlaceHolderID="MainContent" runat="server">
    <section class="auth-shell">
        <div class="auth-card">
            <span class="eyebrow">ACCESS DENIED</span>
            <h1>You don't have permission</h1>
            <p>Your account isn't authorised to view that page. If you think this is a mistake, contact an administrator.</p>
            <a class="btn btn-primary btn-full" href='<%= ResolveUrl("~/Default.aspx") %>'>Back to home</a>
        </div>
    </section>
</asp:Content>
