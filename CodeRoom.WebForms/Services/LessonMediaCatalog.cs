namespace CodeRoom.WebForms.Services
{
    /// <summary>
    /// Introductory video and reference link for each catalogue course, keyed by course title exactly as in
    /// the source application. Only the first lesson of a course carries these links.
    /// </summary>
    public static class LessonMediaCatalog
    {
        public static string VideoFor(string courseTitle)
        {
            switch (courseTitle)
            {
                case "C# Fundamentals": return "https://www.youtube.com/embed/GhQdlIFylQ8";
                case "Python Programming": return "https://www.youtube.com/embed/rfscVS0vtbw";
                case "HTML & CSS Foundations": return "https://www.youtube.com/embed/a_iQb1lnAEQ";
                case "ASP.NET Core MVC": return "https://www.youtube.com/embed/6SAFgcMie4U";
                case "Linux Fundamentals": return "https://www.youtube.com/embed/pkZEKIXe3u4";
                case "Cybersecurity Foundations": return "https://www.youtube.com/embed/Q_hwxazyXQY";
                case "Database Fundamentals": return "https://www.youtube.com/embed/HXV3zeQKqGY";
                default: return null;
            }
        }

        public static string ResourceFor(string courseTitle)
        {
            switch (courseTitle)
            {
                case "C# Fundamentals": return "https://learn.microsoft.com/dotnet/csharp/";
                case "Python Programming": return "https://docs.python.org/3/tutorial/";
                case "HTML & CSS Foundations": return "https://developer.mozilla.org/en-US/docs/Learn";
                case "ASP.NET Core MVC": return "https://learn.microsoft.com/aspnet/core/";
                case "Linux Fundamentals": return "https://linuxjourney.com/";
                case "Cybersecurity Foundations": return "https://owasp.org/www-project-top-ten/";
                case "Database Fundamentals": return "https://dev.mysql.com/doc/";
                default: return null;
            }
        }
    }
}
