<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Logout.aspx.cs" Inherits="CodeRoom.WebForms.Authentication.Logout" %>
<!DOCTYPE html>
<html><head runat="server"><title>Logout - Code-Room</title></head><body>
<form id="form1" runat="server">
    <asp:HiddenField ID="CsrfToken" runat="server" />
    <asp:Button ID="LogoutButton" runat="server" Text="Confirm logout" OnClick="LogoutButton_Click" />
</form>
</body></html>
