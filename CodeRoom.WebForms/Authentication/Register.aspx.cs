using System;
using System.Data.SqlClient;
using CodeRoom.WebForms.Data;
using CodeRoom.WebForms.Helpers;
using CodeRoom.WebForms.Models;
using CodeRoom.WebForms.Services;

namespace CodeRoom.WebForms.Authentication
{
    public partial class Register : System.Web.UI.Page
    {
        private readonly UserRepository users = new UserRepository();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Auth.IsLoggedIn)
            {
                Response.Redirect(ResolveUrl("~/Dashboard/MyDashboard.aspx"), false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            Csrf.EnsureToken(CsrfToken);
        }

        protected void RegisterButton_Click(object sender, EventArgs e)
        {
            Page.Validate("Register");
            if (!Page.IsValid) return;

            try
            {
                Csrf.Validate(CsrfToken);

                var fullName = (FullName.Text ?? string.Empty).Trim();
                var username = (Username.Text ?? string.Empty).Trim().ToLowerInvariant();
                var email = (Email.Text ?? string.Empty).Trim().ToLowerInvariant();
                var password = Password.Text ?? string.Empty;

                if (fullName.Length < 2)
                {
                    ShowError("Please enter your full name.");
                    return;
                }

                if (users.EmailExists(email))
                {
                    ShowError("An account with this email already exists.");
                    return;
                }

                if (users.UsernameExists(username))
                {
                    ShowError("That username is already in use.");
                    return;
                }

                var user = new User
                {
                    FullName = fullName,
                    Username = username,
                    Email = email,
                    PasswordHash = PasswordHasher.Hash(password),
                    Role = Roles.Student
                };

                var userId = SqlHelper.WithTransaction((connection, transaction) =>
                {
                    user.Id = users.Insert(connection, transaction, user);

                    UserRepository.AddXp(connection, transaction, user.Id, 25);

                    ActivityRepository.Insert(connection, transaction, new UserActivity
                    {
                        UserId = user.Id,
                        ActivityType = DomainValues.ActivityType.AccountCreated,
                        Description = "Created a Code-Room account",
                        CreatedAt = DateTime.UtcNow
                    });

                    new NotificationRepository().Insert(connection, transaction, new Notification
                    {
                        UserId = user.Id,
                        Type = DomainValues.NotificationType.Activity,
                        Title = "Welcome to Code-Room",
                        Message = "Your account is ready. Start a course and build your learning streak.",
                        LinkUrl = "~/Dashboard/MyDashboard.aspx",
                        CreatedAt = DateTime.UtcNow,
                        IsRead = false
                    });

                    return user.Id;
                });

                user.Id = userId;
                Auth.SignIn(user, true);

                Response.Redirect(ResolveUrl("~/Dashboard/MyDashboard.aspx"), false);
                Context.ApplicationInstance.CompleteRequest();
            }
            catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
            {
                ShowError("That email or username is already in use.");
            }
            catch (InvalidOperationException ex)
            {
                ShowError(ex.Message);
            }
        }

        private void ShowError(string message)
        {
            ErrorMessage.Text = Server.HtmlEncode(message);
            ErrorMessage.Visible = true;
        }
    }
}
