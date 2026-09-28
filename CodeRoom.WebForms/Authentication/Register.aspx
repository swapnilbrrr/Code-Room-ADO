<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Register.aspx.cs" Inherits="CodeRoom.WebForms.Authentication.Register" MasterPageFile="~/Site.Master" %>

<asp:Content ID="cTitle" ContentPlaceHolderID="TitleContent" runat="server">Register - Code-Room</asp:Content>

<asp:Content ID="cBody" ContentPlaceHolderID="MainContent" runat="server">
    <section class="auth-shell">
        <div class="auth-card">
            <span class="eyebrow">GET STARTED</span>
            <h1>Create your account</h1>
            <p>Join Code-Room and start learning at your own pace.</p>

            <asp:HiddenField ID="CsrfToken" runat="server" />

            <asp:ValidationSummary ID="ValidationSummary1" runat="server"
                ValidationGroup="Register" CssClass="validation-summary"
                HeaderText="Please correct the following:" />

            <div class="form-grid">
                <label class="form-field form-field-wide">
                    <span>Full name</span>
                    <asp:TextBox ID="FullName" runat="server" CssClass="input"
                        MaxLength="80" autocomplete="name" />
                    <asp:RequiredFieldValidator ID="FullNameRequired" runat="server"
                        ControlToValidate="FullName" ValidationGroup="Register"
                        ErrorMessage="Full name is required." CssClass="field-error" Display="Dynamic" />
                </label>

                <label class="form-field form-field-wide">
                    <span>Username</span>
                    <asp:TextBox ID="Username" runat="server" CssClass="input"
                        MaxLength="30" autocomplete="username" />
                    <asp:RequiredFieldValidator ID="UsernameRequired" runat="server"
                        ControlToValidate="Username" ValidationGroup="Register"
                        ErrorMessage="Username is required." CssClass="field-error" Display="Dynamic" />
                    <asp:RegularExpressionValidator ID="UsernameFormat" runat="server"
                        ControlToValidate="Username" ValidationGroup="Register"
                        ValidationExpression="^[A-Za-z0-9_.-]{3,30}$"
                        ErrorMessage="Use 3-30 letters, numbers, dots, underscores or hyphens."
                        CssClass="field-error" Display="Dynamic" />
                </label>

                <label class="form-field form-field-wide">
                    <span>Email</span>
                    <asp:TextBox ID="Email" runat="server" CssClass="input"
                        TextMode="Email" MaxLength="120" autocomplete="email" />
                    <asp:RequiredFieldValidator ID="EmailRequired" runat="server"
                        ControlToValidate="Email" ValidationGroup="Register"
                        ErrorMessage="Email is required." CssClass="field-error" Display="Dynamic" />
                    <asp:RegularExpressionValidator ID="EmailFormat" runat="server"
                        ControlToValidate="Email" ValidationGroup="Register"
                        ValidationExpression="^[^@s]+@[^@s]+.[^@s]+$"
                        ErrorMessage="Enter a valid email address."
                        CssClass="field-error" Display="Dynamic" />
                </label>

                <label class="form-field">
                    <span>Password</span>
                    <asp:TextBox ID="Password" runat="server" CssClass="input"
                        TextMode="Password" MaxLength="128" autocomplete="new-password" />
                    <asp:RequiredFieldValidator ID="PasswordRequired" runat="server"
                        ControlToValidate="Password" ValidationGroup="Register"
                        ErrorMessage="Password is required." CssClass="field-error" Display="Dynamic" />
                    <asp:RegularExpressionValidator ID="PasswordFormat" runat="server"
                        ControlToValidate="Password" ValidationGroup="Register"
                        ValidationExpression="^(?=.*[a-z])(?=.*[A-Z])(?=.*d).{8,128}$"
                        ErrorMessage="Use 8+ characters with upper, lower and a number."
                        CssClass="field-error" Display="Dynamic" />
                </label>

                <label class="form-field">
                    <span>Confirm password</span>
                    <asp:TextBox ID="ConfirmPassword" runat="server" CssClass="input"
                        TextMode="Password" MaxLength="128" autocomplete="new-password" />
                    <asp:CompareValidator ID="PasswordMatch" runat="server"
                        ControlToValidate="ConfirmPassword" ControlToCompare="Password"
                        ValidationGroup="Register" Operator="Equal" Type="String"
                        ErrorMessage="Passwords do not match." CssClass="field-error" Display="Dynamic" />
                </label>
            </div>

            <asp:Label ID="ErrorMessage" runat="server" CssClass="validation-summary"
                Visible="false" EnableViewState="false" />

            <asp:Button ID="RegisterButton" runat="server" Text="Create account"
                CssClass="btn btn-primary btn-full" ValidationGroup="Register"
                OnClick="RegisterButton_Click" />

            <p class="auth-footer">Already registered? <a href='<%= ResolveUrl("~/Authentication/Login.aspx") %>'>Sign in</a></p>
        </div>
    </section>
</asp:Content>
