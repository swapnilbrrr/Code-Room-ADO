using System;

namespace CodeRoom.WebForms.Helpers
{
    /// <summary>
    /// Guards media and link URLs that are stored by administrators and later rendered into
    /// learner-facing pages, so schemes such as javascript: and data: cannot execute in the browser.
    /// </summary>
    public static class SafeUrl
    {
        public static bool IsAllowed(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return true;

            var trimmed = value.Trim();
            if (trimmed.Length > 500) return false;

            if (trimmed.StartsWith("/", StringComparison.Ordinal))
                return trimmed.Length == 1 || (trimmed[1] != '/' && trimmed[1] != '\\');

            Uri uri;
            return Uri.TryCreate(trimmed, UriKind.Absolute, out uri) &&
                   (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
        }
    }
}
