using System;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using CodeRoom.WebForms.Services;

namespace CodeRoom.WebForms.Helpers
{
    /// <summary>
    /// Session-backed identity for the migrated application. Keeps the source
    /// application's custom authentication model rather than ASP.NET Identity.
    /// Nothing populates the session until the login page is implemented in Phase 3,
    /// so every page currently resolves to the anonymous state.
    /// </summary>
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
            if (session == null)
            {
                return null;
            }

            return session[key];
        }

        public static bool IsLoggedIn
        {
            get
            {
                object value = Read(KeyUserId);
                return value != null && Convert.ToInt32(value) > 0;
            }
        }

        public static int CurrentUserId
        {
            get
            {
                object value = Read(KeyUserId);
                return value == null ? 0 : Convert.ToInt32(value);
            }
        }

        public static string CurrentFullName
        {
            get
            {
                object value = Read(KeyFullName);
                return value == null ? string.Empty : value.ToString();
            }
        }

        public static string CurrentUsername
        {
            get
            {
                object value = Read(KeyUsername);
                return value == null ? string.Empty : value.ToString();
            }
        }

        public static string CurrentEmail
        {
            get
            {
                object value = Read(KeyEmail);
                return value == null ? string.Empty : value.ToString();
            }
        }

        public static string CurrentRole
        {
            get
            {
                object value = Read(KeyRole);
                return value == null ? string.Empty : value.ToString();
            }
        }

        public static string FirstName
        {
            get
            {
                string name = CurrentFullName;
                return string.IsNullOrWhiteSpace(name) ? "Learner" : name.Split(' ')[0];
            }
        }

        public static string Initial
        {
            get
            {
                string name = FirstName;
                return string.IsNullOrWhiteSpace(name) ? "C" : name.Substring(0, 1).ToUpperInvariant();
            }
        }

        public static bool IsAdmin
        {
            get { return IsInRole(Roles.Admin) || IsInRole(Roles.SuperAdmin); }
        }

        public static bool IsSuperAdmin
        {
            get { return IsInRole(Roles.SuperAdmin); }
        }

        public static string RoleLabel
        {
            get
            {
                if (IsSuperAdmin)
                {
                    return "Super Administrator";
                }

                return IsInRole(Roles.Admin) ? "Administrator" : "Student";
            }
        }

        public static bool IsInRole(string role)
        {
            return string.Equals(CurrentRole, role, StringComparison.OrdinalIgnoreCase);
        }

        public static void SignIn(int userId, string fullName, string username, string email, string role)
        {
            var session = Session;
            if (session == null)
            {
                return;
            }

            session[KeyUserId] = userId;
            session[KeyFullName] = fullName;
            session[KeyUsername] = username;
            session[KeyEmail] = email;
            session[KeyRole] = role;
        }

        public static void SignOut()
        {
            var session = Session;
            if (session == null)
            {
                return;
            }

            session.Clear();
            session.Abandon();
        }

        /// <summary>Returns true when the request was redirected, so the caller must stop work.</summary>
        public static bool RequireLogin(Page page)
        {
            if (IsLoggedIn)
            {
                return false;
            }

            page.Response.Redirect(page.ResolveUrl(LoginPage));
            return true;
        }

        /// <summary>Returns true when the request was redirected, so the caller must stop work.</summary>
        public static bool RequireAdmin(Page page)
        {
            if (RequireLogin(page))
            {
                return true;
            }

            if (IsAdmin)
            {
                return false;
            }

            page.Response.Redirect(page.ResolveUrl(DeniedPage));
            return true;
        }

        /// <summary>Returns true when the request was redirected, so the caller must stop work.</summary>
        public static bool RequireSuperAdmin(Page page)
        {
            if (RequireLogin(page))
            {
                return true;
            }

            if (IsSuperAdmin)
            {
                return false;
            }

            page.Response.Redirect(page.ResolveUrl(DeniedPage));
            return true;
        }
    }
}
