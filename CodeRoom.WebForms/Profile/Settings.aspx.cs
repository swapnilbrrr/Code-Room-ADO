using System;
using System.IO;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CodeRoom.WebForms.Data;
using CodeRoom.WebForms.Helpers;
using CodeRoom.WebForms.Models;
using CodeRoom.WebForms.Services;

namespace CodeRoom.WebForms.Profile
{
    public partial class Settings : Page
    {
        protected TextBox FullName, Username, Bio, AvatarUrl, Email, CurrentPassword, NewPassword, ConfirmNewPassword;
        protected FileUpload AvatarFile;
        protected DropDownList ProfileVisibility;
        protected CheckBox EmailNotificationsEnabled;
        protected RadioButton ThemeLight, ThemeDark, ThemeSystem;
        protected Literal PreviewInitial, PreviewName, PreviewUsername;
        protected ValidationSummary ValidationSummary;
        protected Button SaveButton;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Auth.RequireLogin(this)) return;
            if (!IsPostBack)
            {
                var user = new UserRepository().GetById(Auth.CurrentUserId);
                if (user == null) { Auth.SignOut(); Response.Redirect(ResolveUrl("~/Authentication/Login.aspx"), false); Context.ApplicationInstance.CompleteRequest(); return; }
                BindUser(user);
            }
        }

        private void BindUser(User user)
        {
            FullName.Text = user.FullName;
            Username.Text = user.Username;
            Email.Text = user.Email;
            Bio.Text = user.Bio ?? string.Empty;
            AvatarUrl.Text = user.AvatarUrl ?? string.Empty;
            ProfileVisibility.SelectedValue = user.ProfileVisibility == DomainValues.Visibility.Members ? DomainValues.Visibility.Members : DomainValues.Visibility.Public;
            EmailNotificationsEnabled.Checked = user.EmailNotificationsEnabled;
            ThemeLight.Checked = string.Equals(user.ThemePreference, DomainValues.Theme.Light, StringComparison.OrdinalIgnoreCase);
            ThemeDark.Checked = string.Equals(user.ThemePreference, DomainValues.Theme.Dark, StringComparison.OrdinalIgnoreCase);
            ThemeSystem.Checked = !ThemeLight.Checked && !ThemeDark.Checked;
            PreviewName.Text = Server.HtmlEncode(user.FullName);
            PreviewUsername.Text = Server.HtmlEncode(user.Username);
            PreviewInitial.Text = Server.HtmlEncode(string.IsNullOrWhiteSpace(user.FullName) ? "C" : user.FullName.Trim().Substring(0, 1).ToUpperInvariant());
        }

        protected void SaveButton_Click(object sender, EventArgs e)
        {
            if (Auth.RequireLogin(this)) return;

            try { ((SiteMaster)Master).ValidateCsrf(); }
            catch (InvalidOperationException) { AddValidationError("Security check failed. Refresh the page and try again."); return; }

            Page.Validate();
            if (!Page.IsValid) return;

            var userRepo = new UserRepository();
            var user = userRepo.GetById(Auth.CurrentUserId);
            if (user == null) { Auth.SignOut(); Response.Redirect(ResolveUrl("~/Authentication/Login.aspx"), false); Context.ApplicationInstance.CompleteRequest(); return; }

            var fullName = (FullName.Text ?? string.Empty).Trim();
            var username = (Username.Text ?? string.Empty).Trim().ToLowerInvariant();
            var email = (Email.Text ?? string.Empty).Trim().ToLowerInvariant();
            var bio = (Bio.Text ?? string.Empty).Trim();
            var avatarUrl = (AvatarUrl.Text ?? string.Empty).Trim();

            if (userRepo.UsernameExists(username, user.Id)) AddValidationError("That username is already in use.");
            if (userRepo.EmailExists(email, user.Id)) AddValidationError("That email address is already in use.");

            var passwordChangeRequested =
                !string.IsNullOrWhiteSpace(CurrentPassword.Text) ||
                !string.IsNullOrWhiteSpace(NewPassword.Text) ||
                !string.IsNullOrWhiteSpace(ConfirmNewPassword.Text);

            if (passwordChangeRequested)
            {
                if (string.IsNullOrWhiteSpace(CurrentPassword.Text) || !PasswordHasher.Verify(CurrentPassword.Text, user.PasswordHash))
                    AddValidationError("Enter your current password to change it.");
                if (string.IsNullOrWhiteSpace(NewPassword.Text))
                    AddValidationError("Enter a new password.");
                if (!string.Equals(NewPassword.Text, ConfirmNewPassword.Text, StringComparison.Ordinal))
                    AddValidationError("The new passwords do not match.");
            }

            if (AvatarFile.HasFile && AvatarFile.PostedFile.ContentLength > 2 * 1024 * 1024)
                AddValidationError("Avatar must be JPG, PNG or WebP and no larger than 2 MB.");

            if (!Page.IsValid) return;

            var profileChanged =
                user.FullName != fullName ||
                user.Username != username ||
                user.Email != email ||
                (user.Bio ?? string.Empty) != bio ||
                (user.AvatarUrl ?? string.Empty) != avatarUrl ||
                user.ThemePreference != SelectedTheme() ||
                user.ProfileVisibility != ProfileVisibility.SelectedValue ||
                user.EmailNotificationsEnabled != EmailNotificationsEnabled.Checked ||
                AvatarFile.HasFile;

            if (AvatarFile.HasFile)
            {
                var extension = Path.GetExtension(AvatarFile.FileName).ToLowerInvariant();
                if (extension != ".jpg" && extension != ".jpeg" && extension != ".png" && extension != ".webp")
                {
                    AddValidationError("Avatar must be JPG, PNG or WebP and no larger than 2 MB.");
                    return;
                }

                var directory = Server.MapPath("~/uploads/avatars");
                Directory.CreateDirectory(directory);
                var fileName = Guid.NewGuid().ToString("N") + extension;
                var filePath = Path.Combine(directory, fileName);
                AvatarFile.SaveAs(filePath);

                DeleteOwnedAvatar(user.AvatarUrl);
                avatarUrl = "/uploads/avatars/" + fileName;
            }

            user.FullName = fullName;
            user.Username = username;
            user.Email = email;
            user.Bio = string.IsNullOrWhiteSpace(bio) ? null : bio;
            user.AvatarUrl = string.IsNullOrWhiteSpace(avatarUrl) ? null : avatarUrl;
            user.ThemePreference = SelectedTheme();
            user.ProfileVisibility = ProfileVisibility.SelectedValue == DomainValues.Visibility.Members ? DomainValues.Visibility.Members : DomainValues.Visibility.Public;
            user.EmailNotificationsEnabled = EmailNotificationsEnabled.Checked;

            userRepo.Update(user);
            if (passwordChangeRequested)
                userRepo.UpdatePasswordHash(user.Id, PasswordHasher.Hash(NewPassword.Text));

            if (profileChanged || passwordChangeRequested)
            {
                new LearningActivityService().Record(
                    user.Id,
                    DomainValues.ActivityType.ProfileUpdated,
                    passwordChangeRequested ? "Updated profile and password" : "Updated profile details",
                    "Profile updated",
                    passwordChangeRequested ? "Your profile and password have been updated successfully." : "Your profile details have been updated successfully.",
                    "/Profile/Index.aspx",
                    "ProfileUpdated");
            }

            new LearningActivityService().TryRecordStreakMilestone(user.Id);
            Auth.RefreshIdentity(user);

            Toast.Success("Changes saved", passwordChangeRequested
                ? "Your profile and password are up to date."
                : "Your profile details are up to date.");
            Response.Redirect(ResolveUrl("~/Profile/Index.aspx"), false);
            Context.ApplicationInstance.CompleteRequest();
        }

        protected void ValidateFullName(object source, ServerValidateEventArgs args)
        {
            var value = (args.Value ?? string.Empty).Trim();
            args.IsValid = value.Length >= 3 && value.Length <= 80;
        }

        protected void ValidateUsername(object source, ServerValidateEventArgs args)
        {
            var value = (args.Value ?? string.Empty).Trim();
            args.IsValid = value.Length >= 3 && value.Length <= 30 && Regex.IsMatch(value, "^[a-zA-Z0-9._-]+$");
        }

        protected void ValidateEmail(object source, ServerValidateEventArgs args)
        {
            try { new MailAddress((args.Value ?? string.Empty).Trim()); args.IsValid = (args.Value ?? string.Empty).Trim().Length <= 120; }
            catch { args.IsValid = false; }
        }

        protected void ValidateAvatarUrl(object source, ServerValidateEventArgs args)
        {
            var value = (args.Value ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(value)) { args.IsValid = true; return; }
            Uri uri;
            args.IsValid = Uri.TryCreate(value, UriKind.Absolute, out uri) &&
                           (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps) &&
                           value.Length <= 300;
        }

        protected void ValidatePassword(object source, ServerValidateEventArgs args)
        {
            args.IsValid = string.IsNullOrWhiteSpace(args.Value) || args.Value.Length >= 8;
        }

        private string SelectedTheme()
        {
            if (ThemeLight.Checked) return DomainValues.Theme.Light;
            if (ThemeDark.Checked) return DomainValues.Theme.Dark;
            return DomainValues.Theme.System;
        }

        private void AddValidationError(string message)
        {
            var validator = new CustomValidator
            {
                IsValid = false,
                ErrorMessage = message,
                ValidationGroup = string.Empty
            };
            Page.Validators.Add(validator);
        }

        private static void DeleteOwnedAvatar(string avatarUrl)
        {
            if (string.IsNullOrWhiteSpace(avatarUrl) ||
                !avatarUrl.StartsWith("/uploads/avatars/", StringComparison.OrdinalIgnoreCase) ||
                HttpContext.Current == null)
                return;

            var path = HttpContext.Current.Server.MapPath("~" + avatarUrl);
            if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
                File.Delete(path);
        }
    }
}