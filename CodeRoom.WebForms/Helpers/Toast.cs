using System.Web;

namespace CodeRoom.WebForms.Helpers
{
    /// <summary>
    /// Replaces the source application's TempData toast. Web Forms has no TempData,
    /// so the toast crosses a single redirect through Session and is cleared on read.
    /// </summary>
    public static class Toast
    {
        private const string KeyTitle = "CR_ToastTitle";
        private const string KeyMessage = "CR_ToastMessage";
        private const string KeyIcon = "CR_ToastIcon";

        public static void Set(string title, string message, string icon)
        {
            var session = HttpContext.Current != null ? HttpContext.Current.Session : null;
            if (session == null)
            {
                return;
            }

            session[KeyTitle] = title;
            session[KeyMessage] = message;
            session[KeyIcon] = icon;
        }

        public static void Success(string title, string message) { Set(title, message, "✓"); }
        public static void Error(string title, string message) { Set(title, message, "!"); }
        public static void Info(string title, string message) { Set(title, message, "i"); }

        public static bool HasTitle
        {
            get { return Read(KeyTitle) != null; }
        }

        public static string Title
        {
            get { return Read(KeyTitle) ?? string.Empty; }
        }

        public static string Message
        {
            get { return Read(KeyMessage) ?? string.Empty; }
        }

        public static string Icon
        {
            get
            {
                string icon = Read(KeyIcon);
                return string.IsNullOrEmpty(icon) ? "✓" : icon;
            }
        }

        public static void Clear()
        {
            var session = HttpContext.Current != null ? HttpContext.Current.Session : null;
            if (session == null)
            {
                return;
            }

            session.Remove(KeyTitle);
            session.Remove(KeyMessage);
            session.Remove(KeyIcon);
        }

        private static string Read(string key)
        {
            var session = HttpContext.Current != null ? HttpContext.Current.Session : null;
            if (session == null || session[key] == null)
            {
                return null;
            }

            return session[key].ToString();
        }
    }
}
