<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Error.aspx.cs" Inherits="CodeRoom.WebForms.Error" MasterPageFile="~/Site.Master" %>

<asp:Content ID="cTitle" ContentPlaceHolderID="TitleContent" runat="server">Error - Code-Room</asp:Content>

<asp:Content ID="cBody" ContentPlaceHolderID="MainContent" runat="server">
    <section class="page-shell">
        <div class="container narrow-page">
            <span class="eyebrow">SOMETHING WENT WRONG</span>
            <h1>We couldn't complete that request.</h1>
            <p class="page-intro">Please return to the home page and try again.</p>
            <a class="btn btn-primary" href='<%= ResolveUrl("~/Default.aspx") %>'>Back to home</a>
        </div>
    </section>
</asp:Content>
