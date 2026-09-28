using System;
using System.Security.Cryptography;
using System.Text;
using System.Web.UI.WebControls;

namespace CodeRoom.WebForms.Helpers
{
    /// <summary>
    /// Synchronizer-token helper for state-changing Web Forms posts.
    /// ViewState MAC protects Web Forms state, while this token protects cookie-authenticated posts from CSRF.
    /// </summary>
    public static class Csrf
    {
        private const string SessionKey = "CR_CsrfToken";

        public static void EnsureToken(HiddenField field)
        {
            if (field == null) throw new ArgumentNullException("field");
            field.Value = GetOrCreateToken();
        }

        public static void Validate(HiddenField field)
        {
            if (field == null) throw new ArgumentNullException("field");

            var expected = GetOrCreateToken();
            var actual = field.Value ?? string.Empty;

            if (!SecureEquals(expected, actual))
            {
                throw new InvalidOperationException("The security token is invalid or has expired.");
            }
        }

        private static string GetOrCreateToken()
        {
            var context = System.Web.HttpContext.Current;
            var session = context == null ? null : context.Session;
            if (session == null)
                throw new InvalidOperationException("Session state is required for form security.");

            var token = session[SessionKey] as string;
            if (!string.IsNullOrEmpty(token)) return token;

            var bytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create()) rng.GetBytes(bytes);

            token = Convert.ToBase64String(bytes);
            session[SessionKey] = token;
            return token;
        }

        private static bool SecureEquals(string left, string right)
        {
            var a = Encoding.UTF8.GetBytes(left ?? string.Empty);
            var b = Encoding.UTF8.GetBytes(right ?? string.Empty);
            var diff = a.Length ^ b.Length;
            var length = Math.Max(a.Length, b.Length);

            for (var i = 0; i < length; i++)
            {
                var x = i < a.Length ? a[i] : (byte)0;
                var y = i < b.Length ? b[i] : (byte)0;
                diff |= x ^ y;
            }

            return diff == 0;
        }
    }
}
