using System.Collections.Generic;

namespace CodeRoom.WebForms.Services
{
    /// <summary>
    /// The baseline content of the platform, carried over from the source application's DbSeeder and
    /// LearningPlatformSeeder. These are plain data holders: ADO.NET writes them in PlatformSeeder and the
    /// database never sees a C# object.
    /// </summary>
    public sealed class QuestionSeed
    {
        public QuestionSeed(string text, string a, string b, string c, string d, string correct)
        {
            Text = text;
            A = a;
            B = b;
            C = c;
            D = d;
            Correct = correct;
        }

        public string Text { get; private set; }
        public string A { get; private set; }
        public string B { get; private set; }
        public string C { get; private set; }
        public string D { get; private set; }
        public string Correct { get; private set; }
    }

    public sealed class LessonSeed
    {
        public LessonSeed(string title, string summary, string type, string video, string resource, int duration)
        {
            Title = title;
            Summary = summary;
            Type = type;
            Video = video;
            Resource = resource;
            Duration = duration;
        }

        public string Title { get; private set; }
        public string Summary { get; private set; }
        public string Type { get; private set; }
        public string Video { get; private set; }
        public string Resource { get; private set; }
        public int Duration { get; private set; }
    }

    public sealed class ChallengeSeed
    {
        public ChallengeSeed(string title, string instructions, string starterCode, string hint,
            string expectedAnswer, string mode, int points, int lessonOffset)
        {
            Title = title;
            Instructions = instructions;
            StarterCode = starterCode;
            Hint = hint;
            ExpectedAnswer = expectedAnswer;
            Mode = mode;
            Points = points;
            LessonOffset = lessonOffset;
        }

        public string Title { get; private set; }
        public string Instructions { get; private set; }
        public string StarterCode { get; private set; }
        public string Hint { get; private set; }
        public string ExpectedAnswer { get; private set; }
        public string Mode { get; private set; }
        public int Points { get; private set; }
        public int LessonOffset { get; private set; }
    }

    public sealed class CourseSeed
    {
        public CourseSeed(string title, string slug, string description, string category, string level,
            int minutes, bool certification, string certificateName, int passScore,
            LessonSeed[] lessons, QuestionSeed[] questions, ChallengeSeed[] challenges)
        {
            Title = title;
            Slug = slug;
            Description = description;
            Category = category;
            Level = level;
            Minutes = minutes;
            Certification = certification;
            CertificateName = certificateName;
            PassScore = passScore;
            Lessons = lessons;
            Questions = questions;
            Challenges = challenges;
        }

        public string Title { get; private set; }
        public string Slug { get; private set; }
        public string Description { get; private set; }
        public string Category { get; private set; }
        public string Level { get; private set; }
        public int Minutes { get; private set; }
        public bool Certification { get; private set; }
        public string CertificateName { get; private set; }
        public int PassScore { get; private set; }
        public LessonSeed[] Lessons { get; private set; }
        public QuestionSeed[] Questions { get; private set; }
        public ChallengeSeed[] Challenges { get; private set; }
    }

    /// <summary>The original catalogue: ten lessons each, one knowledge-check quiz each.</summary>
    public static class CatalogueSeed
    {
        public static readonly string[] NetworkingLessonTitles =
        {
            "What is a Network?", "OSI Model", "TCP/IP Model", "IPv4 & IPv6", "MAC & ARP",
            "TCP & UDP", "Ports & Protocols", "DNS", "HTTP/HTTPS", "Network Troubleshooting"
        };

        public sealed class Entry
        {
            public Entry(string title, string slug, string description, string category, string level,
                string[] lessons, string quizTitle, QuestionSeed[] questions)
            {
                Title = title;
                Slug = slug;
                Description = description;
                Category = category;
                Level = level;
                Lessons = lessons;
                QuizTitle = quizTitle;
                Questions = questions;
            }

            public string Title { get; private set; }
            public string Slug { get; private set; }
            public string Description { get; private set; }
            public string Category { get; private set; }
            public string Level { get; private set; }
            public string[] Lessons { get; private set; }
            public string QuizTitle { get; private set; }
            public QuestionSeed[] Questions { get; private set; }
        }

        public static readonly Entry[] Courses =
        {
            new Entry("C# Fundamentals", "csharp-fundamentals",
                "Learn C# syntax, control flow and object-oriented programming fundamentals.",
                "Programming", "Beginner",
                new[] { "Introduction to C#", "Variables & Data Types", "Operators & Expressions", "Conditional Statements", "Loops", "Methods", "Arrays & Collections", "Classes & Objects", "Inheritance & Polymorphism", "Mini Project" },
                "C# Fundamentals Quiz",
                new[]
                {
                    new QuestionSeed("Which keyword defines a class in C#?", "class", "struct", "def", "function", "A"),
                    new QuestionSeed("Which type stores whole numbers?", "string", "int", "bool", "double", "B"),
                    new QuestionSeed("What is the entry point of a C# console app?", "Start()", "Run()", "Main()", "Init()", "C"),
                    new QuestionSeed("Which loop runs at least once?", "for", "while", "foreach", "do-while", "D"),
                    new QuestionSeed("Which concept lets a class reuse another class's members?", "Inheritance", "Encapsulation", "Overloading", "Casting", "A")
                }),
            new Entry("Python Programming", "python-programming",
                "Build a practical Python foundation through syntax, data structures, functions and file handling.",
                "Programming", "Beginner",
                new[] { "Python Basics", "Variables & Data Types", "Conditions", "Loops", "Functions", "Lists, Tuples & Dictionaries", "Modules", "File Handling", "Exceptions", "Mini Project" },
                "Python Programming Quiz",
                new[]
                {
                    new QuestionSeed("How do you define a function in Python?", "func", "def", "function", "define", "B"),
                    new QuestionSeed("Which data structure uses key-value pairs?", "list", "tuple", "dictionary", "set", "C"),
                    new QuestionSeed("Which symbol starts a comment in Python?", "//", "#", "--", "/*", "B"),
                    new QuestionSeed("Which keyword handles exceptions?", "catch", "except", "rescue", "trap", "B"),
                    new QuestionSeed("Which is an immutable sequence?", "list", "dict", "set", "tuple", "D")
                }),
            new Entry("HTML & CSS Foundations", "html-css-foundations",
                "Learn how to structure accessible pages and build responsive layouts with HTML and CSS.",
                "Web Development", "Beginner",
                new[] { "How the Web Works", "HTML Structure", "Semantic HTML", "CSS Fundamentals", "Box Model", "Flexbox", "Grid", "Responsive Design", "Forms & Accessibility", "Mini Project" },
                "HTML & CSS Quiz",
                new[]
                {
                    new QuestionSeed("Which tag defines the largest heading?", "<h6>", "<head>", "<h1>", "<header>", "C"),
                    new QuestionSeed("Which CSS property changes text colour?", "font-color", "color", "text-style", "fill", "B"),
                    new QuestionSeed("Which layout module is one-dimensional?", "Grid", "Flexbox", "Table", "Float", "B"),
                    new QuestionSeed("Which tag is semantic for navigation?", "<div>", "<nav>", "<menu-bar>", "<links>", "B"),
                    new QuestionSeed("Which unit is relative to the root font size?", "px", "pt", "rem", "cm", "C")
                }),
            new Entry("ASP.NET Core MVC", "aspnet-core-mvc",
                "Understand MVC architecture, routing, Razor views, validation and data access in ASP.NET Core.",
                "Web Development", "Intermediate",
                new[] { "Introduction to ASP.NET Core", "MVC Architecture", "Controllers", "Models", "Razor Views", "Routing", "Forms & Validation", "Entity Framework Core", "Authentication", "Building an MVC Application" },
                "ASP.NET Core MVC Quiz",
                new[]
                {
                    new QuestionSeed("Which component handles incoming HTTP requests in MVC?", "Model", "View", "Controller", "Database", "C"),
                    new QuestionSeed("Which file configures the request pipeline in .NET 8?", "Startup.cs", "Program.cs", "Web.config", "App.cs", "B"),
                    new QuestionSeed("What does EF Core provide?", "Styling", "Object-relational mapping", "Routing", "Logging", "B"),
                    new QuestionSeed("Which attribute validates a required field?", "[Key]", "[Required]", "[Bind]", "[Route]", "B"),
                    new QuestionSeed("Razor view files use which extension?", ".razor", ".cshtml", ".html", ".vbhtml", "B")
                }),
            new Entry("Networking Fundamentals", "networking-fundamentals",
                "Build a practical foundation in network models, addressing, protocols and basic troubleshooting.",
                "Cybersecurity", "Beginner",
                NetworkingLessonTitles,
                "Networking Fundamentals Quiz",
                new[]
                {
                    new QuestionSeed("How many layers are in the OSI model?", "5", "6", "7", "8", "C"),
                    new QuestionSeed("Which protocol is connection-oriented?", "UDP", "TCP", "ICMP", "ARP", "B"),
                    new QuestionSeed("What does DNS resolve?", "IP to MAC", "Domain names to IP addresses", "Ports to services", "Files to folders", "B"),
                    new QuestionSeed("Which port does HTTPS use by default?", "21", "80", "443", "25", "C"),
                    new QuestionSeed("Which address is a private IPv4 range?", "8.8.8.8", "192.168.1.1", "172.15.0.1", "11.0.0.1", "B")
                }),
            new Entry("Linux Fundamentals", "linux-fundamentals",
                "Learn Linux filesystem navigation, permissions, processes, networking commands and basic security.",
                "Cybersecurity", "Beginner",
                new[] { "Linux Basics", "Filesystem", "Navigation & CLI", "Users & Groups", "Permissions", "Processes", "Networking Commands", "Package Management", "Shell Basics", "Linux Security Basics" },
                "Linux Fundamentals Quiz",
                new[]
                {
                    new QuestionSeed("Which command lists directory contents?", "ls", "cd", "pwd", "mv", "A"),
                    new QuestionSeed("Which command shows the current directory?", "dir", "pwd", "cwd", "loc", "B"),
                    new QuestionSeed("What does chmod change?", "Ownership", "Permissions", "Filename", "Size", "B"),
                    new QuestionSeed("Which file lists user accounts?", "/etc/passwd", "/etc/hosts", "/var/log", "/home", "A"),
                    new QuestionSeed("Which command displays running processes?", "jobs", "ps", "run", "top-list", "B")
                }),
            new Entry("Cybersecurity Foundations", "cybersecurity-foundations",
                "Explore core defensive security concepts, common threats, controls and incident response.",
                "Cybersecurity", "Intermediate",
                new[] { "Introduction to Cybersecurity", "CIA Triad", "Threats & Vulnerabilities", "Authentication & Authorization", "Malware", "Phishing", "Firewalls", "Endpoint Security", "Security Monitoring", "Incident Response Basics" },
                "Cybersecurity Foundations Quiz",
                new[]
                {
                    new QuestionSeed("What does the 'C' in the CIA triad stand for?", "Control", "Confidentiality", "Compliance", "Continuity", "B"),
                    new QuestionSeed("Which attack tricks users via fake emails?", "DDoS", "Phishing", "SQL Injection", "Spoofing", "B"),
                    new QuestionSeed("What does a firewall primarily do?", "Encrypt files", "Filter network traffic", "Back up data", "Scan disks", "B"),
                    new QuestionSeed("Which is a strong authentication factor combination?", "Password only", "Two-factor authentication", "Username only", "Email only", "B"),
                    new QuestionSeed("What is the first phase of incident response?", "Recovery", "Preparation", "Eradication", "Reporting", "B")
                }),
            new Entry("Database Fundamentals", "database-fundamentals",
                "Understand relational databases, SQL operations, relationships, constraints and basic design.",
                "Databases", "Beginner",
                new[] { "Introduction to Databases", "Relational Databases", "Tables & Relationships", "Primary & Foreign Keys", "SQL SELECT", "INSERT / UPDATE / DELETE", "JOINs", "Constraints", "Normalization", "Database Design" },
                "Database Fundamentals Quiz",
                new[]
                {
                    new QuestionSeed("Which SQL statement retrieves data?", "GET", "SELECT", "FETCH", "READ", "B"),
                    new QuestionSeed("What uniquely identifies a row in a table?", "Foreign key", "Primary key", "Index", "View", "B"),
                    new QuestionSeed("Which clause filters rows in a query?", "ORDER BY", "WHERE", "GROUP BY", "HAVING", "B"),
                    new QuestionSeed("Which JOIN returns only matching rows?", "LEFT JOIN", "RIGHT JOIN", "INNER JOIN", "FULL JOIN", "C"),
                    new QuestionSeed("What does normalization reduce?", "Speed", "Data redundancy", "Security", "Storage cost only", "B")
                })
        };

        /// <summary>Global resources shown on the platform resources page.</summary>
        public static readonly ResourceSeed[] Resources =
        {
            new ResourceSeed(null, "C# Coding Standards (PDF)", "https://learn.microsoft.com/dotnet/csharp/", "Document"),
            new ResourceSeed(null, "MDN Web Docs", "https://developer.mozilla.org/", "Link"),
            new ResourceSeed(null, "OWASP Top 10", "https://owasp.org/www-project-top-ten/", "Link")
        };

        public static readonly AnnouncementSeed[] Announcements =
        {
            new AnnouncementSeed("Welcome to Code-Room",
                "Explore our technology courses, track your progress and test yourself with quizzes.", -7),
            new AnnouncementSeed("New cybersecurity path available",
                "Networking, Linux and Cybersecurity Foundations are now live in the catalogue.", -2)
        };
    }

    public sealed class ResourceSeed
    {
        public ResourceSeed(string slug, string title, string url, string type)
        {
            Slug = slug;
            Title = title;
            Url = url;
            Type = type;
        }

        public string Slug { get; private set; }
        public string Title { get; private set; }
        public string Url { get; private set; }
        public string Type { get; private set; }
    }

    public sealed class AnnouncementSeed
    {
        public AnnouncementSeed(string title, string message, int publishedDaysAgo)
        {
            Title = title;
            Message = message;
            PublishedDaysAgo = publishedDaysAgo;
        }

        public string Title { get; private set; }
        public string Message { get; private set; }
        public int PublishedDaysAgo { get; private set; }
    }

    public sealed class AchievementSeed
    {
        public AchievementSeed(string code, string name, string description, string icon, int xpReward)
        {
            Code = code;
            Name = name;
            Description = description;
            Icon = icon;
            XpReward = xpReward;
        }

        public string Code { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public string Icon { get; private set; }
        public int XpReward { get; private set; }
    }

    /// <summary>
    /// Built-in demonstration accounts. No plaintext password is stored in source control: the
    /// initial password is derived from the source application's naming convention at seed time and
    /// is only ever passed straight into the PBKDF2 hasher.
    /// </summary>
    public sealed class DemoUserSeed
    {
        public DemoUserSeed(string fullName, string username, string email, string role, string passwordLabel)
        {
            FullName = fullName;
            Username = username;
            Email = email;
            Role = role;
            PasswordLabel = passwordLabel;
        }

        public string FullName { get; private set; }
        public string Username { get; private set; }
        public string Email { get; private set; }
        public string Role { get; private set; }
        public string PasswordLabel { get; private set; }

        /// <summary>First name + role label + year, e.g. "Swapnil.Admin@2026".</summary>
        public string InitialPassword
        {
            get
            {
                var firstName = FullName.Split(' ')[0];
                return firstName + "." + PasswordLabel + "@2026";
            }
        }
    }

    public static class DemoSeed
    {
        public static readonly DemoUserSeed[] Users =
        {
            new DemoUserSeed("Swapnil Katuwal", "swapnil", "swapnil.katuwal@coderoom.com", "SuperAdmin", "Admin"),
            new DemoUserSeed("Chandra Bhatta", "chandra", "chandra.bhatta@coderoom.com", "Admin", "Admin"),
            new DemoUserSeed("Bijay Khadka", "bijay", "bijay.khadka@coderoom.com", "Student", "Student"),
            new DemoUserSeed("Anisha Gurung", "anisha", "anisha.gurung@coderoom.com", "Student", "Student"),
            new DemoUserSeed("Nischal Bhandari", "nischal", "nischal.bhandari@coderoom.com", "Student", "Student"),
            new DemoUserSeed("Suman Adhikari", "suman", "suman.adhikari@coderoom.com", "Student", "Student"),
            new DemoUserSeed("Prerana Rai", "prerana", "prerana.rai@coderoom.com", "Student", "Student"),
            new DemoUserSeed("Babin Aryal", "babin", "babin.aryal@coderoom.com", "Student", "Student")
        };
    }
}
