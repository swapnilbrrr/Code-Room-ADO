<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="CodeRoom.WebForms.Authentication.Login" MasterPageFile="~/Site.Master" %>

<asp:Content ID="cTitle" ContentPlaceHolderID="TitleContent" runat="server">Login - Code-Room</asp:Content>

<asp:Content ID="cBody" ContentPlaceHolderID="MainContent" runat="server">
    <section class="auth-shell">
        <div class="auth-card">
            <span class="eyebrow">WELCOME BACK</span>
            <h1>Sign in to Code-Room</h1>
            <p>Continue your learning journey.</p>

            <asp:HiddenField ID="CsrfToken" runat="server" />
            <asp:HiddenField ID="ReturnUrl" runat="server" />

            <asp:ValidationSummary ID="ValidationSummary1" runat="server"
                ValidationGroup="Login" CssClass="validation-summary"
                HeaderText="Please correct the following:" />

            <div class="form-grid">
                <label class="form-field form-field-wide">
                    <span>Email</span>
                    <asp:TextBox ID="Email" runat="server" CssClass="input"
                        TextMode="Email" autocomplete="email" />
                    <asp:RequiredFieldValidator ID="EmailRequired" runat="server"
                        ControlToValidate="Email" ValidationGroup="Login"
                        ErrorMessage="Email is required." CssClass="field-error"
                        Display="Dynamic" />
                </label>

                <label class="form-field form-field-wide">
                    <span>Password</span>
                    <asp:TextBox ID="Password" runat="server" CssClass="input"
                        TextMode="Password" autocomplete="current-password" />
                    <asp:RequiredFieldValidator ID="PasswordRequired" runat="server"
                        ControlToValidate="Password" ValidationGroup="Login"
                        ErrorMessage="Password is required." CssClass="field-error"
                        Display="Dynamic" />
                </label>

                <div class="checkbox-row">
                    <asp:CheckBox ID="RememberMe" runat="server" Text="Remember me" />
                </div>
            </div>

            <asp:Label ID="ErrorMessage" runat="server" CssClass="validation-summary"
                Visible="false" EnableViewState="false" />

            <asp:Button ID="LoginButton" runat="server" Text="Sign in"
                CssClass="btn btn-primary btn-full" ValidationGroup="Login"
                OnClick="LoginButton_Click" />

            <p class="auth-footer">New to Code-Room? <a href='<%= ResolveUrl("~/Authentication/Register.aspx") %>'>Create an account</a></p>
        </div>
    </section>
</asp:Content>
