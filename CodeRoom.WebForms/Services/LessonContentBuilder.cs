using System;

namespace CodeRoom.WebForms.Services
{
    /// <summary>
    /// Produces the long-form lesson body used by every seeded lesson. The text is deterministic, so the
    /// same course and lesson always generate the same content and re-seeding never drifts.
    /// Ported from the source application's LessonContentBuilder; the two fall-through arms of the
    /// original switch expressions were emitted as literal "_ =>" text and are written as real fallbacks here.
    /// </summary>
    public static class LessonContentBuilder
    {
        public static string Build(string courseTitle, string lessonTitle, int lessonNumber)
        {
            return EnsureRichContent(null, courseTitle, lessonTitle, lessonNumber);
        }

        public static string EnsureRichContent(string existingContent, string courseTitle, string lessonTitle, int lessonNumber)
        {
            if (!string.IsNullOrWhiteSpace(existingContent) && existingContent.Trim().Length >= 260)
            {
                return existingContent.Trim();
            }

            var focus = GetFocus(courseTitle, lessonTitle);
            var practice = GetPractice(lessonTitle);
            var application = GetApplication(courseTitle, lessonTitle);
            var mistakes = GetCommonMistakes(lessonTitle);

            return
                "Lesson " + lessonNumber + " — " + lessonTitle + ": " + focus + "\n\n" +
                "WHY IT MATTERS: " + courseTitle + " relies on this concept because it helps you make predictable technical decisions instead of memorising isolated commands or definitions. " +
                "The goal is to understand what the concept does, when it is useful, and what evidence you can use to verify your result.\n\n" +
                "KEY CONCEPTS: Break the topic into three questions: what is it, how does it work, and what changes when the inputs change? " +
                "Keep the terms precise, identify the important boundaries, and connect each rule to at least one concrete example. This makes the lesson easier to recall during troubleshooting and assessment questions.\n\n" +
                "HOW TO THINK ABOUT IT: Start by identifying the inputs, the expected behaviour and the result you need. " +
                "Then work through the process one step at a time. If the result differs from your expectation, compare the actual evidence with your assumption and isolate the smallest part that could explain the difference.\n\n" +
                "WORKED EXAMPLE: " + application + " " +
                "Write down the expected result before you run the example, then compare it with the actual output. If the result is different, change one variable at a time rather than changing the whole example at once.\n\n" +
                "COMMON MISTAKES: " + mistakes + "\n\n" +
                "PRACTICE FOCUS: " + practice + " Reproduce a small example yourself, change one part, and record what changed. " +
                "For a stronger test, include one normal case and one edge case, then explain why the two results differ.\n\n" +
                "CHECK YOURSELF: Can you define the concept, describe one realistic use case, predict the result before execution, and explain one failure mode? " +
                "If not, revisit the key concepts before moving on.\n\n" +
                "KEY TAKEAWAY: Write one sentence in your own words, one command or pattern you would use in practice, and one question you would ask during a real technical investigation or project.";
        }

        private static string GetApplication(string courseTitle, string lessonTitle)
        {
            return "Create a small example centred on " + lessonTitle + ". First write down what you expect the example to do, then run it and compare the actual result. " +
                   "For " + courseTitle + ", try to relate the example to a realistic task such as building a feature, analysing input, troubleshooting a problem or protecting a system.";
        }

        private static string GetCommonMistakes(string title)
        {
            switch (title)
            {
                case "Variables & Data Types":
                    return "Using a type without considering the kind of value it represents, converting data without checking the result, or changing values in more places than necessary.";
                case "Loops":
                case "Conditional Statements":
                case "Conditions":
                    return "Making the condition too broad, forgetting an exit condition, or assuming every iteration or branch will run.";
                case "Methods":
                case "Functions":
                    return "Making one method responsible for too many unrelated tasks, using unclear parameter names, or duplicating logic instead of reusing a well-defined function.";
                case "Classes & Objects":
                    return "Putting unrelated responsibilities into one class or exposing data without considering how the object should control its own state.";
                case "Inheritance & Polymorphism":
                    return "Using inheritance simply for code reuse when composition or a shared interface would communicate the design more clearly.";
                case "HTML Structure":
                case "Semantic HTML":
                    return "Choosing elements based only on visual appearance instead of the meaning of the content, or skipping labels and document structure that users and assistive technology rely on.";
                case "CSS Fundamentals":
                case "Box Model":
                case "Flexbox":
                case "Grid":
                    return "Changing several layout properties at once without checking which property is actually controlling the result.";
                case "Responsive Design":
                    return "Designing only for one screen width and trying to patch smaller screens with lots of fixed dimensions.";
                case "Forms & Accessibility":
                case "Forms & Validation":
                    return "Relying only on browser-side checks and assuming submitted data is trustworthy.";
                case "Routing":
                    return "Creating links that depend on hard-coded URLs or changing a route without checking the navigation paths that depend on it.";
                case "Entity Framework Core":
                case "Database Design":
                    return "Ignoring relationships and constraints because the application appears to work with a small sample of data.";
                case "Authentication":
                case "Authentication & Authorization":
                    return "Confusing identity verification with permission checks or granting broader access than the user actually needs.";
                case "OSI Model":
                case "TCP/IP Model":
                    return "Treating the models as seven or four exact physical boxes rather than conceptual ways of organising networking responsibilities.";
                case "TCP & UDP":
                    return "Choosing a transport protocol from habit without considering reliability, latency and application behaviour.";
                case "DNS":
                    return "Assuming every name lookup is a simple one-to-one mapping and overlooking caching, record types and the possibility of failures at different points.";
                case "HTTP/HTTPS":
                    return "Looking only at the page content instead of checking the request method, status code, headers and transport security.";
                case "Permissions":
                    return "Granting broad access because it makes a test work instead of applying the least privilege required.";
                case "Security Monitoring":
                    return "Treating a single event as proof of compromise without checking surrounding context, timing, source and related activity.";
                case "Phishing":
                    return "Trusting urgency, display names or familiar branding without independently verifying the sender and destination.";
                case "Normalization":
                    return "Removing duplication mechanically without first understanding the relationships and access patterns the application needs.";
                default:
                    return "Skipping the small verification step and assuming the first result proves that the concept is understood.";
            }
        }

        private static string GetFocus(string courseTitle, string title)
        {
            switch (title)
            {
                case "Introduction to C#":
                    return "C# is a statically typed, object-oriented language used on the .NET platform. This lesson establishes the role of source files, namespaces, statements, methods and the program entry point.";
                case "Variables & Data Types":
                    return "Variables hold values that a program can work with, while data types describe the kind of value being stored. In C#, choosing an appropriate type helps the compiler catch mistakes early and makes the program's intent clearer.";
                case "Operators & Expressions":
                    return "Operators let a program perform arithmetic, comparisons and logical decisions. Expressions combine values and operators into results that can be assigned, checked or passed into methods.";
                case "Conditional Statements":
                    return "Conditional statements allow a program to choose different actions based on a condition. The common building blocks are if, else if, else and switch, each useful for a different style of decision logic.";
                case "Loops":
                    return "Loops repeat a block of work while a condition or sequence requires it. for, while, do-while and foreach loops are useful in different situations, especially when processing collections or repeating a known operation.";
                case "Methods":
                    return "Methods package reusable behaviour behind a clear name, parameters and return type. Breaking a larger task into methods makes code easier to test, understand and maintain.";
                case "Arrays & Collections":
                    return "Collections organise multiple values so a program can process related data together. Arrays provide a fixed-size structure, while collection types such as List and Dictionary are more flexible for everyday applications.";
                case "Classes & Objects":
                    return "Classes define the structure and behaviour of objects. This is the foundation of object-oriented programming in C#, where data and the operations that work on that data can be modelled together.";
                case "Inheritance & Polymorphism":
                    return "Inheritance lets a derived class reuse members from a base class, while polymorphism allows code to work with a shared abstraction and still use the specialised behaviour of a derived type.";
                case "Python Basics":
                    return "Python emphasises readable syntax and a small amount of ceremony around common programming tasks. This lesson introduces the interpreter, scripts, indentation and the basic shape of a Python program.";
                case "Conditions":
                    return "Python uses if, elif and else to branch execution. Conditions are expressions that evaluate to true or false and can be combined to create more precise decision rules.";
                case "Functions":
                    return "Functions group reusable behaviour and can accept parameters and return values. Good functions reduce repetition and give a program a clearer structure.";
                case "Lists, Tuples & Dictionaries":
                    return "Python provides several built-in collection types for different needs. Lists are mutable sequences, tuples are immutable sequences, and dictionaries store key-value pairs for direct lookup.";
                case "Modules":
                    return "Modules let Python code be split into reusable files and imported where needed. Working with modules makes larger programs easier to organise and helps keep individual files focused.";
                case "File Handling":
                    return "File handling allows programs to read information from and write information to persistent storage. The with statement is commonly used so files are closed correctly even when an operation fails.";
                case "Exceptions":
                    return "Exceptions represent problems that occur while a program is running. try, except, else and finally provide a structured way to handle expected failures without hiding the underlying problem.";
                case "How the Web Works":
                    return "Web applications rely on a client-server model in which a browser sends HTTP requests and a web server returns responses. Understanding URLs, requests, responses and status codes provides the foundation for everything built later in the course.";
                case "HTML Structure":
                    return "HTML describes the structure and meaning of a web page. Elements, attributes, headings, paragraphs, lists, links and media combine to create a document that browsers and assistive technologies can understand.";
                case "Semantic HTML":
                    return "Semantic elements communicate the role of page content, such as navigation, main content, articles and footers. Clear semantics improve maintainability, accessibility and the way content is interpreted by tools.";
                case "CSS Fundamentals":
                    return "CSS controls how HTML content is presented. Selectors, properties and values work together to manage typography, spacing, colour, borders and layout without mixing presentation into the document structure.";
                case "Box Model":
                    return "Every normal HTML element can be understood through the CSS box model: content, padding, border and margin. Knowing how those layers affect size and spacing makes layout problems much easier to diagnose.";
                case "Flexbox":
                    return "Flexbox is designed for arranging items along one main axis while giving control over alignment, spacing and distribution. It is especially useful for navigation bars, toolbars and component rows.";
                case "Grid":
                    return "CSS Grid provides a two-dimensional layout system for rows and columns. It is useful for page sections, dashboards and card layouts where both horizontal and vertical relationships matter.";
                case "Responsive Design":
                    return "Responsive design adapts a page to different screen sizes and input conditions. Flexible units, media queries and responsive layouts allow the same interface to remain usable on phones, tablets and desktops.";
                case "Forms & Accessibility":
                    return "Forms are the main mechanism for collecting user input on the web. Good labels, meaningful controls, validation messages, keyboard support and sensible focus order make forms easier for everyone to use.";
                case "Introduction to ASP.NET Core":
                    return "ASP.NET Core is a cross-platform web framework for building server-side applications on .NET. This lesson introduces the application lifecycle, dependency injection, middleware and the role of the host.";
                case "MVC Architecture":
                    return "The MVC pattern separates request handling, application data and presentation into controllers, models and views. This separation makes a web application easier to reason about and change as it grows.";
                case "Controllers":
                    return "Controllers receive HTTP requests, coordinate application work and return an appropriate response. In MVC applications, action methods usually fetch or update data and then choose a view or redirect.";
                case "Models":
                    return "Models represent the application's data and the rules that apply to it. Data annotations can describe validation requirements, display names and database-facing constraints.";
                case "Razor Views":
                    return "Razor combines HTML with C# expressions so server-side data can be rendered into a page. Keeping views focused on presentation helps avoid putting business logic in the UI layer.";
                case "Routing":
                    return "Routing maps an incoming URL to a controller action or endpoint. Clean routes make an application easier to navigate and allow links to remain meaningful as the system grows.";
                case "Forms & Validation":
                    return "Server-side validation ensures that data cannot be trusted simply because it came from a browser. ASP.NET Core validation works alongside client-side validation to give users fast feedback while keeping the server authoritative.";
                case "Entity Framework Core":
                    return "Entity Framework Core provides object-relational mapping between C# classes and relational database tables. DbContext, DbSet and LINQ queries let the application read and update database data through strongly typed code.";
                case "Authentication":
                    return "Authentication establishes who the current user is, while authorization determines what that user is allowed to do. Cookie authentication and role checks provide the foundation for protected student and administrator areas.";
                case "Building an MVC Application":
                    return "A complete MVC application brings together models, views, controllers, routing, validation, dependency injection and persistence. The aim is to connect these pieces into a maintainable end-to-end workflow.";
                case "What is a Network?":
                    return "A computer network allows devices to communicate and share resources using agreed protocols. The lesson introduces endpoints, links, addressing and the basic purpose of switches, routers and other network components.";
                case "OSI Model":
                    return "The OSI model divides network communication into seven conceptual layers. It is a troubleshooting and learning framework that helps relate application behaviour to transport, network, data-link and physical processes.";
                case "TCP/IP Model":
                    return "The TCP/IP model groups networking functions into layers used by real Internet protocols. Understanding how application, transport, Internet and link functions interact makes packet captures easier to interpret.";
                case "IPv4 & IPv6":
                    return "IP addressing identifies interfaces so packets can be delivered across networks. IPv4 uses 32-bit addresses, while IPv6 expands the address space and introduces a different notation and feature set.";
                case "MAC & ARP":
                    return "MAC addresses identify interfaces at the local network level, while ARP resolves an IPv4 address to a local MAC address. Together they explain an important part of how an Ethernet host reaches a neighbour.";
                case "TCP & UDP":
                    return "TCP provides a connection-oriented transport with sequencing, acknowledgements and retransmission, while UDP provides a lightweight datagram service. Choosing between them depends on the application's delivery requirements.";
                case "Ports & Protocols":
                    return "Ports distinguish services on a host, while protocols define how those services communicate. Recognising common port and protocol combinations is fundamental to network troubleshooting and security monitoring.";
                case "DNS":
                    return "DNS translates human-readable names into records such as IP addresses. Clients use DNS so applications can refer to services by names instead of hard-coding addresses.";
                case "HTTP/HTTPS":
                    return "HTTP defines the request-response exchange used by the web, while HTTPS protects that exchange with TLS. Methods, headers, status codes and encrypted transport all matter when analysing web traffic.";
                case "Network Troubleshooting":
                    return "Troubleshooting follows a structured process: define the symptom, isolate the layer or component involved, test a hypothesis and verify the result. Tools such as ping, traceroute, ipconfig or Wireshark support that process with evidence.";
                case "Linux Basics":
                    return "Linux is a family of Unix-like operating systems built around the kernel and a collection of user-space tools. This lesson introduces distributions, shells and the basic workflow of working from a terminal.";
                case "Filesystem":
                    return "The Linux filesystem is organised as a hierarchy starting from the root directory. Important paths such as /etc, /var, /home and /usr have different purposes and help administrators locate configuration, logs, user data and software.";
                case "Navigation & CLI":
                    return "Command-line navigation uses a small set of tools such as pwd, ls and cd to locate and inspect files. Learning these commands first makes later administration and troubleshooting much faster.";
                case "Users & Groups":
                    return "Linux uses users and groups to control identity and access. Group membership provides a convenient way to assign permissions to a set of users without changing each account individually.";
                case "Permissions":
                    return "Linux permissions control read, write and execute access for an owner, a group and other users. Understanding chmod, chown and permission notation is essential for safe system administration.";
                case "Processes":
                    return "A process is a running instance of a program. Linux tools such as ps, top and kill help administrators inspect process activity and manage programs that are consuming resources or behaving unexpectedly.";
                case "Networking Commands":
                    return "Linux provides command-line tools for inspecting interfaces, routes, sockets and DNS. These utilities provide quick evidence when diagnosing connectivity or suspicious network activity.";
                case "Package Management":
                    return "Package managers install, update and remove software while handling dependencies. Learning the package workflow makes system maintenance more consistent than manually copying application files.";
                case "Shell Basics":
                    return "A shell interprets commands and provides scripting features such as variables, conditions, loops and pipelines. These capabilities make repetitive administration tasks easier to automate.";
                case "Linux Security Basics":
                    return "Basic Linux security combines least privilege, strong permissions, timely updates, secure services and useful logging. These controls reduce the chance that a compromised process can affect the entire system.";
                case "Introduction to Cybersecurity":
                    return "Cybersecurity protects information, systems and services from unauthorised access, misuse and disruption. This lesson introduces defensive thinking, risk and the relationship between people, processes and technology.";
                case "CIA Triad":
                    return "The confidentiality, integrity and availability triad provides a simple way to reason about security objectives. Different incidents affect these properties in different ways, so controls should be matched to the risk.";
                case "Threats & Vulnerabilities":
                    return "A threat is something capable of causing harm, while a vulnerability is a weakness that can be exploited. Security work connects the two through risk analysis and prioritised remediation.";
                case "Authentication & Authorization":
                    return "Authentication verifies identity, whereas authorization evaluates permissions after identity has been established. Separating these concerns helps systems enforce least privilege and safer access decisions.";
                case "Malware":
                    return "Malware is software designed to perform unwanted or harmful actions. Common categories include trojans, ransomware, spyware and worms, each with different behaviours and defensive indicators.";
                case "Phishing":
                    return "Phishing uses deceptive messages or interfaces to manipulate users into revealing information or taking unsafe actions. Defensive awareness focuses on sender identity, links, urgency cues and verification through trusted channels.";
                case "Firewalls":
                    return "Firewalls enforce traffic rules between networks or hosts. A useful rule set is explicit about sources, destinations, protocols and ports while following least privilege.";
                case "Endpoint Security":
                    return "Endpoint security combines controls such as secure configuration, patching, anti-malware, application control and monitoring. The goal is to reduce attack surface while generating useful evidence when something abnormal occurs.";
                case "Security Monitoring":
                    return "Security monitoring collects events and telemetry so defenders can recognise suspicious activity and investigate it. Good monitoring depends on useful log sources, timestamps, context and alert quality.";
                case "Incident Response Basics":
                    return "Incident response provides a repeatable way to prepare for, identify, contain, eradicate and recover from security incidents. Clear evidence and documented actions help teams reduce impact and learn from each event.";
                case "Introduction to Databases":
                    return "Databases provide structured storage and controlled access to information. This lesson introduces tables, records, relationships and the role of a database management system in an application.";
                case "Relational Databases":
                    return "Relational databases organise data into tables that can be linked through keys. The relational model supports consistency and powerful queries while keeping related data structured.";
                case "Tables & Relationships":
                    return "Tables store rows of related records, while relationships connect records across tables. One-to-many relationships are especially common in applications such as courses with many lessons.";
                case "Primary & Foreign Keys":
                    return "A primary key uniquely identifies a record, while a foreign key references a record in another table. Together they create reliable relationships and prevent ambiguous links between data.";
                case "SQL SELECT":
                    return "SELECT retrieves data from one or more tables. Filtering with WHERE, ordering with ORDER BY and combining rows with expressions lets an application retrieve exactly the information it needs.";
                case "INSERT / UPDATE / DELETE":
                    return "INSERT creates records, UPDATE changes existing records and DELETE removes them. These operations must be used carefully, especially when constraints or user-generated data are involved.";
                case "JOINs":
                    return "JOINs combine related rows from multiple tables based on a relationship. INNER JOIN, LEFT JOIN and other variants answer different questions about matching and missing records.";
                case "Constraints":
                    return "Constraints enforce data rules at the database level. Primary keys, unique constraints, foreign keys and required values help protect integrity even when multiple parts of an application write to the same database.";
                case "Normalization":
                    return "Normalization reduces unnecessary duplication by separating data into well-structured related tables. The aim is to improve consistency and make updates less error-prone.";
                case "Database Design":
                    return "Good database design starts from application requirements and turns them into entities, attributes and relationships. Thinking carefully about keys, constraints and expected queries makes the final schema easier to maintain.";
            }

            if (Contains(courseTitle, "Python"))
            {
                return "This Python lesson focuses on " + Lower(title) + ". Pay attention to syntax, data flow, error handling and the small verification steps that make a script predictable and reusable.";
            }

            if (Contains(courseTitle, "Bash") || Contains(courseTitle, "Linux"))
            {
                return "This Linux and shell lesson focuses on " + Lower(title) + ". Connect each command to the system state it changes or observes, and practise safely with a test file, process or directory.";
            }

            if (Contains(courseTitle, "AWS") || Contains(courseTitle, "Azure") || Contains(courseTitle, "Cloud"))
            {
                return "This cloud lesson focuses on " + Lower(title) + ". Compare the architectural goal, access boundary, operational responsibility and security trade-off before choosing a service or control.";
            }

            if (Contains(courseTitle, "SOC"))
            {
                return "This SOC lesson focuses on " + Lower(title) + ". Build an evidence-based timeline, distinguish observations from assumptions and identify the next data source that would reduce uncertainty.";
            }

            if (Contains(courseTitle, "Security") || Contains(courseTitle, "Cyber"))
            {
                return "This security lesson focuses on " + Lower(title) + ". Think in terms of attack surface, trust boundaries, defensive controls, observable evidence and the least-privilege action that reduces risk.";
            }

            if (Contains(courseTitle, "Networking") || Contains(courseTitle, "CCNA"))
            {
                return "This networking lesson focuses on " + Lower(title) + ". Draw the traffic path, identify the address or protocol involved, and verify your reasoning with a concrete packet, command or topology example.";
            }

            if (Contains(courseTitle, "SQL") || Contains(courseTitle, "MySQL") || Contains(courseTitle, "Database"))
            {
                return "This database lesson focuses on " + Lower(title) + ". Relate the concept to tables, keys, constraints and the query or application operation that would use it.";
            }

            return "This topic explains an important part of " + courseTitle + ". Focus on the terminology, the purpose of the concept, the conditions in which it is used and the mistakes that commonly lead to incorrect results.";
        }

        private static string GetPractice(string title)
        {
            switch (title)
            {
                case "Variables & Data Types":
                case "SQL SELECT":
                    return "Create a small example, change one value or condition, and predict the result before running it.";
                case "Loops":
                case "Conditions":
                case "Conditional Statements":
                    return "Write a small decision or repetition problem such as filtering a list of values or counting matching items.";
                case "Classes & Objects":
                case "Models":
                    return "Design a small class with two or three properties and one behaviour that uses those properties.";
                case "Authentication":
                case "Authentication & Authorization":
                    return "Trace a login request and identify where identity is established and where access permissions are checked.";
                case "HTTP/HTTPS":
                    return "Inspect a request in browser developer tools and identify the method, URL, status code and important headers.";
                case "OSI Model":
                case "TCP/IP Model":
                    return "Take a common web request and describe which networking layer or function is responsible for each stage.";
                case "Permissions":
                    return "Create a test file and inspect its permission bits, then explain which users should be allowed to read or modify it.";
                case "Security Monitoring":
                    return "Pick a security event and list the log fields you would need to determine who acted, what happened, when it happened and from where.";
                case "Normalization":
                case "Database Design":
                    return "Sketch the entities involved in a simple learning system and decide which relationships should be represented with foreign keys.";
                case "JOINs":
                    return "Write a query that combines two related tables and compare the result of INNER JOIN with LEFT JOIN.";
                case "Mini Project":
                    return "Combine the ideas from the preceding lessons into a small working exercise. Keep the scope narrow, test the happy path, then deliberately try an invalid input.";
            }

            if (Contains(title, "Automation"))
            {
                return "Automate one repetitive step, add a safe input check, log the result and test one failure case.";
            }

            if (Contains(title, "Cloud") || Contains(title, "IAM") || Contains(title, "VPC") || Contains(title, "Azure"))
            {
                return "Take a small workload and map identity, network boundary, compute/storage choice and one monitoring signal before selecting the service.";
            }

            if (Contains(title, "Log") || Contains(title, "SOC") || Contains(title, "Monitoring"))
            {
                return "Build a mini timeline from a sample event and list one fact, one hypothesis and one additional evidence source.";
            }

            if (Contains(title, "Bash") || Contains(title, "Shell"))
            {
                return "Run the command safely in a test location, capture its output, then repeat it with an edge case such as an empty input or missing file.";
            }

            if (Contains(title, "Networking") || Contains(title, "Routing") || Contains(title, "Subnet"))
            {
                return "Draw the packet path or subnet range first, then verify it using a small CLI command or worked example.";
            }

            if (Contains(title, "Security") || Contains(title, "XSS") || Contains(title, "CSRF") || Contains(title, "SQL injection"))
            {
                return "Identify the untrusted input and trust boundary, then name the specific server-side control that should protect it.";
            }

            if (Contains(title, "Database") || Contains(title, "SQL"))
            {
                return "Create a tiny schema or query, inspect the result, then deliberately test a missing row, duplicate value or invalid condition.";
            }

            return "Create a tiny example that demonstrates the idea, test it with a normal case and one edge case, and write down what you learned from the result.";
        }

        private static bool Contains(string source, string value)
        {
            return source != null && value != null &&
                   source.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string Lower(string value)
        {
            return value == null ? string.Empty : value.ToLowerInvariant();
        }
    }
}
