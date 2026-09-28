<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Settings.aspx.cs" Inherits="CodeRoom.WebForms.Profile.Settings" MasterPageFile="~/Site.Master" %>
<asp:Content ID="cTitle" ContentPlaceHolderID="TitleContent" runat="server">Settings - Code-Room</asp:Content>
<asp:Content ID="cBody" ContentPlaceHolderID="MainContent" runat="server">
<section class="page-shell settings-page"><div class="container settings-shell">
 <aside class="settings-sidebar" aria-label="Settings sections"><span class="eyebrow">ACCOUNT</span><a href="#profile" class="settings-tab is-active">Profile</a><a href="#account" class="settings-tab">Account</a><a href="#security" class="settings-tab">Security</a><a href="#notifications" class="settings-tab">Notifications</a><a href="#appearance" class="settings-tab">Appearance</a></aside>
 <div class="settings-content">
  <div class="settings-heading"><div><span class="eyebrow">SETTINGS</span><h1 class="page-title">Account settings</h1><p class="page-intro">Control your Code-Room identity, security and learning experience.</p></div><a class="btn btn-secondary" href='<%= ResolveUrl("~/Profile/Index.aspx") %>'>View profile</a></div>
  <asp:ValidationSummary ID="ValidationSummary" runat="server" CssClass="validation-summary" />
  <section class="settings-card settings-section-card" id="profile"><div class="settings-card-heading"><div><h2>Profile</h2><p>These details are shown on your learning identity.</p></div></div>
   <div class="settings-profile-preview"><div class="profile-avatar profile-avatar-large"><asp:Literal ID="PreviewInitial" runat="server" /></div><div><strong><asp:Literal ID="PreviewName" runat="server" /></strong><span>@<asp:Literal ID="PreviewUsername" runat="server" /></span></div></div>
   <div class="form-grid">
    <label>Full name<asp:TextBox ID="FullName" runat="server" CssClass="input" autocomplete="name" /><asp:RequiredFieldValidator ID="FullNameRequired" runat="server" ControlToValidate="FullName" ErrorMessage="Full name is required." CssClass="field-error" Display="Dynamic" /><asp:CustomValidator ID="FullNameLength" runat="server" ControlToValidate="FullName" OnServerValidate="ValidateFullName" ErrorMessage="Full name must be between 3 and 80 characters." CssClass="field-error" Display="Dynamic" /></label>
    <label>Username<asp:TextBox ID="Username" runat="server" CssClass="input" autocomplete="username" /><asp:RequiredFieldValidator ID="UsernameRequired" runat="server" ControlToValidate="Username" ErrorMessage="Username is required." CssClass="field-error" Display="Dynamic" /><asp:CustomValidator ID="UsernameRules" runat="server" ControlToValidate="Username" OnServerValidate="ValidateUsername" ErrorMessage="Username must be 3-30 characters and use only letters, numbers, dots, underscores or hyphens." CssClass="field-error" Display="Dynamic" /></label>
    <label class="form-span-2">Bio<asp:TextBox ID="Bio" runat="server" CssClass="input settings-textarea" TextMode="MultiLine" Rows="4" MaxLength="500" placeholder="What are you learning right now?" /></label>
    <label class="form-field">Upload avatar<asp:FileUpload ID="AvatarFile" runat="server" CssClass="input" accept=".jpg,.jpeg,.png,.webp,image/jpeg,image/png,image/webp" /><small class="settings-help">JPG, PNG or WebP · max 2 MB.</small></label>
    <label class="form-field">Avatar URL<asp:TextBox ID="AvatarUrl" runat="server" CssClass="input" placeholder="https://..." /><small class="settings-help">Optional remote image. Upload takes priority.</small><asp:CustomValidator ID="AvatarUrlValidator" runat="server" ControlToValidate="AvatarUrl" OnServerValidate="ValidateAvatarUrl" ErrorMessage="Enter a valid avatar URL." CssClass="field-error" Display="Dynamic" /></label>
   </div>
  </section>
  <section class="settings-card settings-section-card" id="account"><div class="settings-card-heading"><div><h2>Account</h2><p>Your sign-in identity and profile visibility.</p></div></div><div class="form-grid">
   <label>Email address<asp:TextBox ID="Email" runat="server" CssClass="input" TextMode="SingleLine" autocomplete="email" /><asp:RequiredFieldValidator ID="EmailRequired" runat="server" ControlToValidate="Email" ErrorMessage="Email address is required." CssClass="field-error" Display="Dynamic" /><asp:CustomValidator ID="EmailValidator" runat="server" ControlToValidate="Email" OnServerValidate="ValidateEmail" ErrorMessage="Enter a valid email address." CssClass="field-error" Display="Dynamic" /></label>
   <label>Profile visibility<asp:DropDownList ID="ProfileVisibility" runat="server" CssClass="input"><asp:ListItem Value="Public">Public</asp:ListItem><asp:ListItem Value="Members">Code-Room members</asp:ListItem></asp:DropDownList></label>
  </div></section>
  <section class="settings-card settings-section-card" id="security"><div class="settings-card-heading"><div><h2>Security</h2><p>Change your password without leaving the settings area.</p></div></div><div class="form-grid">
   <label>Current password<asp:TextBox ID="CurrentPassword" runat="server" CssClass="input" TextMode="Password" autocomplete="current-password" /></label>
   <label>New password<asp:TextBox ID="NewPassword" runat="server" CssClass="input" TextMode="Password" autocomplete="new-password" /><asp:CustomValidator ID="NewPasswordValidator" runat="server" ControlToValidate="NewPassword" OnServerValidate="ValidatePassword" ErrorMessage="New password must be at least 8 characters." CssClass="field-error" Display="Dynamic" /></label>
   <label>Confirm new password<asp:TextBox ID="ConfirmNewPassword" runat="server" CssClass="input" TextMode="Password" autocomplete="new-password" /></label>
  </div></section>
  <section class="settings-card settings-section-card" id="notifications"><div class="settings-card-heading"><div><h2>Notifications</h2><p>Choose how much activity reaches your account feed.</p></div></div><label class="switch-row"><span><strong>Activity notifications</strong><small>Show achievements, streaks, course and assessment updates in the notification centre.</small></span><asp:CheckBox ID="EmailNotificationsEnabled" runat="server" CssClass="switch-input" /></label></section>
  <section class="settings-card settings-section-card" id="appearance"><div class="settings-card-heading"><div><h2>Appearance</h2><p>Choose the visual mode used by Code-Room.</p></div></div><asp:HiddenField ID="ThemePreference" runat="server" /><div class="theme-options" data-theme-setting role="group" aria-label="Theme preference"><button type="button" class="theme-option" data-theme-choice="light" aria-pressed="false"><span class="theme-option-icon" aria-hidden="true">☀</span><span class="theme-option-copy"><strong>Light</strong><small>Clean, bright workspace</small></span><span class="theme-option-check" aria-hidden="true">✓</span></button><button type="button" class="theme-option" data-theme-choice="dark" aria-pressed="false"><span class="theme-option-icon" aria-hidden="true">☾</span><span class="theme-option-copy"><strong>Dark</strong><small>Comfortable for low light</small></span><span class="theme-option-check" aria-hidden="true">✓</span></button><button type="button" class="theme-option" data-theme-choice="system" aria-pressed="false"><span class="theme-option-icon" aria-hidden="true">◐</span><span class="theme-option-copy"><strong>System</strong><small>Follow your device setting</small></span><span class="theme-option-check" aria-hidden="true">✓</span></button></div></section>
  <div class="settings-savebar"><span>Changes are stored in your Code-Room account.</span><asp:Button ID="SaveButton" runat="server" CssClass="btn btn-primary" Text="Save settings" OnClick="SaveButton_Click" CausesValidation="true" /></div>
 </div>
</div></section>
</asp:Content>
<asp:Content ID="cScripts" ContentPlaceHolderID="ScriptsContent" runat="server"><script>
(function () {
    var field = document.getElementById('<%= ThemePreference.ClientID %>');
    var options = document.querySelectorAll('[data-theme-choice]');
    var preference = (field && field.value ? field.value : localStorage.getItem('code-room-theme') || 'system').toLowerCase();

    function applyTheme(value) {
        preference = value;
        if (field) field.value = value;
        localStorage.setItem('code-room-theme', value);
        document.documentElement.dataset.themePreference = value;
        var resolved = value === 'system'
            ? (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light')
            : value;
        document.documentElement.dataset.theme = resolved;
        options.forEach(function (button) {
            var active = button.getAttribute('data-theme-choice') === value;
            button.classList.toggle('is-selected', active);
            button.setAttribute('aria-pressed', active ? 'true' : 'false');
        });
    }

    options.forEach(function (button) {
        button.addEventListener('click', function () {
            applyTheme(button.getAttribute('data-theme-choice'));
        });
    });

    applyTheme(preference);
})();
</script></asp:Content>