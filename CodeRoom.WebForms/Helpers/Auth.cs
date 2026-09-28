using System;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using System.Web.UI;
using CodeRoom.WebForms.Data;
using CodeRoom.WebForms.Models;
using CodeRoom.WebForms.Services;
using AppRoles = CodeRoom.WebForms.Services.Roles;

namespace CodeRoom.WebForms.Helpers
{
    public static class Auth
    {
        private const string KeyUserId = "CR_UserId";
        private const string KeyFullName = "CR_FullName";
        private const string KeyUsername = "CR_Username";
        private const string KeyEmail = "CR_Email";
        private const string KeyRole = "CR_Role";
        private const string LoginPage = "~/Authentication/Login.aspx";
        private const string DeniedPage = "~/Authentication/AccessDenied.aspx";

        private static HttpSessionState Session
        {
            get { return HttpContext.Current != null ? HttpContext.Current.Session : null; }
        }

        private static object Read(string key)
        {
            var session = Session;
            return session == null ? null : session[key];
        }

        public static bool IsLoggedIn
        {
            get
            {
                EnsureSessionIdentity();
                object value = Read(KeyUserId);
                return value != null && Convert.ToInt32(value) > 0;
            }
        }

        public static int CurrentUserId
        {
            get
            {
                EnsureSessionIdentity();
                object value = Read(KeyUserId);
                return value == null ? 0 : Convert.ToInt32(value);
            }
        }

        public static string CurrentFullName
        {
            get
            {
                EnsureSessionIdentity();
                object value = Read(KeyFullName);
                return value == null ? string.Empty : value.ToString();
            }
        }

        public static string CurrentUsername
        {
            get
            {
                EnsureSessionIdentity();
                object value = Read(KeyUsername);
                return value == null ? string.Empty : value.ToString();
            }
        }

        public static string CurrentEmail
        {
            get
            {
                EnsureSessionIdentity();
                object value = Read(KeyEmail);
                return value == null ? string.Empty : value.ToString();
            }
        }

        public static string CurrentRole
        {
            get
            {
                EnsureSessionIdentity();
                object value = Read(KeyRole);
                return value == null ? string.Empty : value.ToString();
            }
        }

        public static string FirstName
        {
            get
            {
                var name = CurrentFullName;
                return string.IsNullOrWhiteSpace(name) ? "Learner" : name.Split(' ')[0];
            }
        }

        public static string Initial
        {
            get
            {
                var name = FirstName;
                return string.IsNullOrWhiteSpace(name) ? "C" : name.Substring(0, 1).ToUpperInvariant();
            }
        }

        public static bool IsAdmin { get { return IsInRole(AppRoles.Admin) || IsInRole(AppRoles.SuperAdmin); } }
        public static bool IsSuperAdmin { get { return IsInRole(AppRoles.SuperAdmin); } }

        public static string RoleLabel
        {
            get
            {
                if (IsSuperAdmin) return "Super Administrator";
                return IsInRole(AppRoles.Admin) ? "Administrator" : "Student";
            }
        }

        public static bool IsInRole(string role)
        {
            return string.Equals(CurrentRole, role, StringComparison.OrdinalIgnoreCase);
        }

        public static void SignIn(User user, bool rememberMe)
        {
            if (user == null) throw new ArgumentNullException("user");
            SignIn(user.Id, user.FullName, user.Username, user.Email, user.Role, rememberMe);
        }

        public static void SignIn(int userId, string fullName, string username, string email, string role, bool rememberMe)
        {
            var session = Session;
            if (session == null) throw new InvalidOperationException("Session state is required for authentication.");

            session.Clear();
            session[KeyUserId] = userId;
            session[KeyFullName] = fullName;
            session[KeyUsername] = username;
            session[KeyEmail] = email;
            session[KeyRole] = role;

            var now = DateTime.Now;
            var ticket = new FormsAuthenticationTicket(
                1, username, now, now.Add(FormsAuthentication.Timeout), rememberMe,
                userId.ToString(), FormsAuthentication.FormsCookiePath);

            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(ticket))
            {
                HttpOnly = true,
                Path = FormsAuthentication.FormsCookiePath
            };

            if (rememberMe) cookie.Expires = ticket.Expiration;
            if (HttpContext.Current != null && HttpContext.Current.Request.IsSecureConnection) cookie.Secure = true;

            HttpContext.Current.Response.Cookies.Set(cookie);
        }

        public static void EnsureSessionIdentity()
        {
            if (Session == null || Session[KeyUserId] != null) return;

            var context = HttpContext.Current;
            if (context == null) return;

            var cookie = context.Request.Cookies[FormsAuthentication.FormsCookieName];
            if (cookie == null || string.IsNullOrWhiteSpace(cookie.Value)) return;

            try
            {
                var ticket = FormsAuthentication.Decrypt(cookie.Value);
                if (ticket == null || ticket.Expired)
                {
                    FormsAuthentication.SignOut();
                    return;
                }

                int userId;
                if (!int.TryParse(ticket.UserData, out userId) || userId <= 0)
                {
                    FormsAuthentication.SignOut();
                    return;
                }

                var user = new UserRepository().GetById(userId);
                if (user == null)
                {
                    FormsAuthentication.SignOut();
                    return;
                }

                Session[KeyUserId] = user.Id;
                Session[KeyFullName] = user.FullName;
                Session[KeyUsername] = user.Username;
                Session[KeyEmail] = user.Email;
                Session[KeyRole] = user.Role;
            }
            catch
            {
                FormsAuthentication.SignOut();
                Session.Clear();
            }
        }

        public static void SignOut()
        {
            FormsAuthentication.SignOut();
            var session = Session;
            if (session != null)
            {
                session.Clear();
                session.Abandon();
            }
        }

        public static bool RequireLogin(Page page)
        {
            if (page == null) throw new ArgumentNullException("page");
            EnsureSessionIdentity();

            if (IsLoggedIn) return false;

            var returnUrl = page.Request.RawUrl;
            var loginUrl = page.ResolveUrl(LoginPage);

            if (IsLocalUrl(returnUrl))
                loginUrl += "?returnUrl=" + HttpUtility.UrlEncode(returnUrl);

            page.Response.Redirect(loginUrl, false);
            HttpContext.Current.ApplicationInstance.CompleteRequest();
            return true;
        }

        public static bool RequireAdmin(Page page)
        {
            if (RequireLogin(page)) return true;
            if (IsAdmin) return false;

            page.Response.Redirect(page.ResolveUrl(DeniedPage), false);
            HttpContext.Current.ApplicationInstance.CompleteRequest();
            return true;
        }

        public static bool RequireSuperAdmin(Page page)
        {
            if (RequireLogin(page)) return true;
            if (IsSuperAdmin) return false;

            page.Response.Redirect(page.ResolveUrl(DeniedPage), false);
            HttpContext.Current.ApplicationInstance.CompleteRequest();
            return true;
        }

        private static bool IsLocalUrl(string url)
        {
            return !string.IsNullOrEmpty(url)
                && url[0] == '/'
                && (url.Length == 1 || (url[1] != '/' && url[1] != '\\'));
        }
    }
}
