using System;

namespace CodeRoom.WebForms.Services
{
    /// <summary>
    /// The expanded platform catalogue carried over from the source application's LearningPlatformSeeder:
    /// twelve additional learning paths with explicit lessons, knowledge checks, practice challenges,
    /// per-course resources and the achievement definitions. These are plain data holders only; the ADO.NET
    /// seeder turns them into parameterised INSERT statements.
    /// </summary>
    public static class PlatformSeed
    {
        public static readonly CourseSeed[] Courses =
        {
            new CourseSeed("Python Automation", "python-automation",
                "Automate repetitive tasks with Python, files, APIs, parsing and safe command execution patterns.",
                "Programming", "Intermediate", 300, false, null, 70,
                new[]
                {
                    new LessonSeed("Automation mindset", "Choose tasks that benefit from repeatable, testable scripts.", "Reading", "https://www.youtube.com/embed/rfscVS0vtbw", "https://docs.python.org/3/library/", 25),
                    new LessonSeed("Files and directories", "Work with paths and files using pathlib and context managers.", "Video", null, "https://docs.python.org/3/library/pathlib.html", 30),
                    new LessonSeed("CSV and JSON", "Parse structured data and produce useful outputs.", "Reading", null, "https://docs.python.org/3/library/json.html", 30),
                    new LessonSeed("HTTP APIs", "Send requests, inspect responses and handle failures safely.", "Video", null, "https://developer.mozilla.org/en-US/docs/Web/HTTP", 35),
                    new LessonSeed("Logging and errors", "Make automation scripts observable and debuggable.", "Reading", null, "https://docs.python.org/3/library/logging.html", 30),
                    new LessonSeed("Automation mini-project", "Combine input, processing, output and logging in one small tool.", "Challenge", null, null, 45)
                },
                new[]
                {
                    new QuestionSeed("Which Python module is designed for filesystem paths?", "path", "pathlib", "filepaths", "ospath", "B"),
                    new QuestionSeed("Which format is commonly used for structured API data?", "JSON", "BMP", "WAV", "EXE", "A"),
                    new QuestionSeed("What should automation scripts do when an external request fails?", "Ignore it", "Crash silently", "Handle and report the failure", "Retry forever", "C"),
                    new QuestionSeed("Which module provides application logging?", "logging", "debug", "trace", "report", "A"),
                    new QuestionSeed("What is a good automation property?", "Repeatable", "Random", "Hidden", "Unlogged", "A")
                },
                new[]
                {
                    new ChallengeSeed("Normalize a file extension", "Return the lowercase extension for a filename such as report.CSV.", "filename = 'report.CSV'", "Remember the extension includes the dot.", ".csv", "Contains", 50, 1)
                }),

            new CourseSeed("Python for Cybersecurity", "python-for-cybersecurity",
                "Use Python to reason about security logs, indicators, sockets, parsing and defensive automation.",
                "Cybersecurity", "Intermediate", 330, false, null, 70,
                new[]
                {
                    new LessonSeed("Python in defensive security", "Understand where scripting accelerates repetitive analyst tasks.", "Video", "https://www.youtube.com/embed/rfscVS0vtbw", "https://docs.python.org/3/", 25),
                    new LessonSeed("Log parsing", "Extract timestamps, usernames, IPs and event fields from raw logs.", "Reading", null, "https://docs.python.org/3/library/re.html", 35),
                    new LessonSeed("IP and port validation", "Validate network indicators before using them in analysis.", "Reading", null, "https://docs.python.org/3/library/ipaddress.html", 30),
                    new LessonSeed("Socket basics", "Understand client/server communication and safe socket experiments.", "Video", null, "https://docs.python.org/3/library/socket.html", 35),
                    new LessonSeed("Threat intel enrichment", "Normalise indicators and prepare data for enrichment workflows.", "Reading", null, "https://www.cisa.gov/topics/cyber-threats-and-advisories", 35),
                    new LessonSeed("SOC triage mini-project", "Turn a small log set into a structured triage summary.", "Challenge", null, null, 45)
                },
                new[]
                {
                    new QuestionSeed("Which Python module validates IP addresses?", "socket", "ipaddress", "network", "netaddr", "B"),
                    new QuestionSeed("Why parse logs into fields?", "To remove evidence", "To make analysis consistent", "To hide timestamps", "To slow investigations", "B"),
                    new QuestionSeed("What should be done before enriching an indicator?", "Normalise and validate it", "Delete it", "Publish it", "Encrypt the hostname only", "A"),
                    new QuestionSeed("Which data is especially useful in SOC triage?", "Timestamp and source", "Font size", "Screen brightness", "Wallpaper", "A"),
                    new QuestionSeed("What does a socket provide?", "A communication endpoint", "A database table", "A password vault", "A web template", "A")
                },
                new[]
                {
                    new ChallengeSeed("Extract an IP from text", "Return the IP address from 'source=10.10.10.5'.", "text = 'source=10.10.10.5'", "Keep the answer as the IPv4 address only.", "10.10.10.5", "Exact", 50, 1),
                    new ChallengeSeed("Count failed logins", "Return the count of the word 'failed' in a short log string.", "log = 'failed ok failed'", null, "2", "Exact", 50, 2)
                }),

            new CourseSeed("Bash Fundamentals", "bash-fundamentals",
                "Master Bash navigation, variables, pipes, redirection, loops and defensive shell habits.",
                "Linux", "Beginner", 280, false, null, 70,
                new[]
                {
                    new LessonSeed("Shell and terminal basics", "Understand commands, prompts, paths and exit codes.", "Video", "https://www.youtube.com/embed/pkZEKIXe3u4", "https://www.gnu.org/software/bash/manual/", 25),
                    new LessonSeed("Variables and quoting", "Use variables safely and understand quoting rules.", "Reading", null, "https://www.gnu.org/software/bash/manual/bash.html#Shell-Parameters", 30),
                    new LessonSeed("Pipes and redirection", "Build useful command pipelines and capture output.", "Video", null, "https://www.gnu.org/software/bash/manual/bash.html#Pipelines", 30),
                    new LessonSeed("Conditions and loops", "Automate repeated shell operations with checks.", "Reading", null, "https://www.gnu.org/software/bash/manual/bash.html#Conditional-Constructs", 35),
                    new LessonSeed("Permissions and processes", "Inspect permissions and running processes from the shell.", "Reading", null, "https://man7.org/linux/man-pages/", 35),
                    new LessonSeed("Bash automation challenge", "Combine variables, loops and command output in a safe mini-script.", "Challenge", null, null, 40)
                },
                new[]
                {
                    new QuestionSeed("Which command prints the current directory?", "cd", "pwd", "ls", "dir", "B"),
                    new QuestionSeed("Which symbol redirects standard output?", ">", "|", "&", "#", "A"),
                    new QuestionSeed("Which command lists files?", "pwd", "ls", "mkdir", "whoami", "B"),
                    new QuestionSeed("Why quote variables?", "To preserve intended argument boundaries", "To make them random", "To disable scripts", "To hide output", "A"),
                    new QuestionSeed("Which command changes file permissions?", "chown", "chmod", "ps", "grep", "B")
                },
                new[]
                {
                    new ChallengeSeed("Print the working directory", "Give the Bash command that prints the current working directory.", null, null, "pwd", "Exact", 30, 0)
                }),

            new CourseSeed("AWS Cloud Essentials", "aws-cloud-essentials",
                "Build an exam-oriented AWS foundation around cloud concepts, core services, IAM, networking, security and billing.",
                "Cloud", "Beginner", 360, true, "Code-Room AWS Cloud Essentials Certificate", 70,
                new[]
                {
                    new LessonSeed("Cloud concepts", "Understand elasticity, regions, availability zones and shared responsibility.", "Video", "https://www.youtube.com/embed/3hLmDS179YE", "https://aws.amazon.com/what-is-cloud-computing/", 35),
                    new LessonSeed("AWS compute", "Compare EC2, Lambda and container-oriented workloads.", "Video", null, "https://aws.amazon.com/ec2/", 40),
                    new LessonSeed("AWS storage and databases", "Choose between S3, EBS, RDS and other storage patterns.", "Reading", null, "https://aws.amazon.com/products/storage/", 40),
                    new LessonSeed("IAM and security", "Apply least privilege, MFA and role-based access.", "Video", null, "https://docs.aws.amazon.com/IAM/latest/UserGuide/introduction.html", 45),
                    new LessonSeed("VPC networking", "Reason about subnets, routing, security groups and network boundaries.", "Reading", null, "https://docs.aws.amazon.com/vpc/latest/userguide/what-is-amazon-vpc.html", 45),
                    new LessonSeed("Cloud billing & exam practice", "Use pricing concepts and exam-style scenario questions.", "Assessment", null, "https://aws.amazon.com/pricing/", 35)
                },
                new[]
                {
                    new QuestionSeed("Which AWS service is object storage?", "EC2", "S3", "RDS", "Lambda", "B"),
                    new QuestionSeed("What does IAM primarily control?", "Identity and permissions", "Video encoding", "DNS only", "CPU scheduling", "A"),
                    new QuestionSeed("Which service runs code without managing servers directly?", "Lambda", "EBS", "VPC", "Route 53", "A"),
                    new QuestionSeed("What is a security group?", "A stateful virtual firewall for resources", "A billing invoice", "A storage bucket", "A DNS record", "A"),
                    new QuestionSeed("Which AWS concept isolates failures geographically?", "Availability Zones", "Tags", "Accounts only", "Queues", "A")
                },
                new[]
                {
                    new ChallengeSeed("Choose object storage", "Return the AWS service used for object storage.", null, null, "S3", "Exact", 40, 0)
                }),

            new CourseSeed("Azure Cloud Fundamentals", "azure-cloud-fundamentals",
                "Explore Azure services, resource groups, identity, networking, storage, monitoring and cloud security.",
                "Cloud", "Beginner", 330, true, "Code-Room Azure Cloud Foundations Certificate", 70,
                new[]
                {
                    new LessonSeed("Azure fundamentals", "Understand subscriptions, regions, availability zones and resource groups.", "Reading", null, "https://learn.microsoft.com/azure/cloud-adoption-framework/", 35),
                    new LessonSeed("Compute services", "Compare virtual machines, App Service, containers and serverless options.", "Video", null, "https://learn.microsoft.com/azure/architecture/guide/technology-choices/compute-decision-tree", 40),
                    new LessonSeed("Storage", "Understand Blob, Files and managed storage patterns.", "Reading", null, "https://learn.microsoft.com/azure/storage/common/storage-introduction", 35),
                    new LessonSeed("Microsoft Entra ID", "Learn cloud identity, roles and least privilege.", "Video", null, "https://learn.microsoft.com/entra/fundamentals/whatis", 40),
                    new LessonSeed("Virtual networking", "Reason about VNets, subnets and network security.", "Reading", null, "https://learn.microsoft.com/azure/virtual-network/virtual-networks-overview", 45),
                    new LessonSeed("Azure security & exam practice", "Apply shared responsibility and scenario-based reasoning.", "Assessment", null, "https://learn.microsoft.com/security/", 35)
                },
                new[]
                {
                    new QuestionSeed("What groups Azure resources logically?", "Resource groups", "Availability Zones", "VNets", "Tenants only", "A"),
                    new QuestionSeed("Which service provides cloud identity?", "Microsoft Entra ID", "Blob Storage", "VM Scale Sets", "Azure DNS", "A"),
                    new QuestionSeed("What is a VNet?", "A virtual network boundary", "A database engine", "A billing plan", "A user account", "A"),
                    new QuestionSeed("Blob Storage is primarily for what?", "Object data", "CPU scheduling", "Identity tokens", "DNS", "A"),
                    new QuestionSeed("What principle should guide cloud permissions?", "Least privilege", "Everyone admin", "Shared passwords", "Permanent access", "A")
                },
                new[]
                {
                    new ChallengeSeed("Name the Azure identity service", "Provide the current Microsoft cloud identity service name.", null, null, "Microsoft Entra ID", "Contains", 40, 3)
                }),

            new CourseSeed("Cloud Security Fundamentals", "cloud-security-fundamentals",
                "Apply IAM, network segmentation, logging, shared responsibility and secure cloud architecture principles.",
                "Cloud", "Intermediate", 300, false, null, 70,
                new[]
                {
                    new LessonSeed("Shared responsibility", "Separate provider responsibilities from customer responsibilities.", "Reading", null, "https://cloud.google.com/learn/what-is-cloud-computing", 30),
                    new LessonSeed("Cloud IAM", "Apply role-based access, MFA and least privilege.", "Video", null, "https://docs.aws.amazon.com/IAM/latest/UserGuide/best-practices.html", 35),
                    new LessonSeed("Network controls", "Use segmentation, security groups and private connectivity concepts.", "Reading", null, "https://learn.microsoft.com/security/benchmark/azure/introduction", 35),
                    new LessonSeed("Logging and monitoring", "Design useful audit trails and alert signals.", "Video", null, "https://aws.amazon.com/cloudtrail/", 35),
                    new LessonSeed("Secrets and data protection", "Protect keys, secrets and sensitive information at rest and in transit.", "Reading", null, "https://owasp.org/www-project-top-ten/", 35),
                    new LessonSeed("Cloud incident response", "Use evidence and containment thinking in cloud incidents.", "Challenge", null, null, 40)
                },
                new[]
                {
                    new QuestionSeed("Which principle limits excessive permissions?", "Least privilege", "Shared admin", "Default public", "Flat access", "A"),
                    new QuestionSeed("Why centralise cloud logs?", "For investigation and accountability", "To hide attacks", "To reduce timestamps", "To remove evidence", "A"),
                    new QuestionSeed("What should secrets be stored in?", "A managed secret store", "Source code", "Public README", "Browser local storage", "A"),
                    new QuestionSeed("Which control reduces lateral movement?", "Network segmentation", "Shared passwords", "Open security groups", "Public buckets", "A"),
                    new QuestionSeed("What should incident response preserve?", "Evidence", "Only screenshots", "Nothing", "Credentials", "A")
                },
                new[]
                {
                    new ChallengeSeed("Identify the security principle", "Name the principle that says a user should receive only the access needed for the task.", null, null, "least privilege", "Contains", 50, 1)
                }),

            new CourseSeed("SOC Analyst Foundations", "soc-analyst-foundations",
                "Learn alert triage, log analysis, detection thinking, investigation workflow and incident documentation.",
                "Cybersecurity", "Intermediate", 340, true, "Code-Room SOC Analyst Foundations Certificate", 70,
                new[]
                {
                    new LessonSeed("SOC workflow", "Understand triage, scoping, escalation and documentation.", "Reading", null, "https://attack.mitre.org/", 30),
                    new LessonSeed("Windows and Linux logs", "Identify useful authentication, process and system events.", "Video", null, "https://learn.microsoft.com/windows/security/threat-protection/auditing/basic-audit-policy-settings", 40),
                    new LessonSeed("Network evidence", "Read IPs, ports, protocols and packet context.", "Video", "https://www.youtube.com/embed/7_G0cY9Y5V8", "https://www.wireshark.org/docs/", 40),
                    new LessonSeed("Detection and false positives", "Separate suspicious signals from benign activity using context.", "Reading", null, "https://attack.mitre.org/matrices/enterprise/", 35),
                    new LessonSeed("Incident response notes", "Document who, what, when, where and why.", "Reading", null, "https://csrc.nist.gov/projects/incident-response", 35),
                    new LessonSeed("SOC case simulation", "Complete an analyst-style alert investigation and assessment.", "Assessment", null, null, 45)
                },
                new[]
                {
                    new QuestionSeed("What is alert triage?", "Initial analysis and prioritisation", "Deleting alerts", "Patching servers", "Writing code only", "A"),
                    new QuestionSeed("Why correlate multiple log sources?", "To add context", "To remove evidence", "To create noise", "To avoid timelines", "A"),
                    new QuestionSeed("What is a false positive?", "A benign event incorrectly alerted as suspicious", "A confirmed attack", "A deleted log", "A blocked port", "A"),
                    new QuestionSeed("Which field anchors an investigation timeline?", "Timestamp", "Font", "Window size", "Theme", "A"),
                    new QuestionSeed("What should an analyst document?", "Evidence and actions", "Only their opinion", "Passwords", "Nothing", "A")
                },
                new[]
                {
                    new ChallengeSeed("Name the first triage question", "Provide the investigation question that identifies when the event occurred.", null, null, "when", "Contains", 35, 1)
                }),

            new CourseSeed("Web Application Security", "web-application-security",
                "Practise secure web development using authentication, validation, session protection, XSS, CSRF and common web attack concepts.",
                "Cybersecurity", "Intermediate", 320, false, null, 70,
                new[]
                {
                    new LessonSeed("Web attack surface", "Map inputs, sessions, APIs and trust boundaries.", "Reading", null, "https://owasp.org/www-project-top-ten/", 30),
                    new LessonSeed("Authentication & sessions", "Protect identity, cookies and session state.", "Video", null, "https://cheatsheetseries.owasp.org/cheatsheets/Session_Management_Cheat_Sheet.html", 40),
                    new LessonSeed("Input validation & XSS", "Treat user input as untrusted and encode output correctly.", "Reading", null, "https://owasp.org/www-community/attacks/xss/", 40),
                    new LessonSeed("CSRF", "Understand browser-based request forgery and anti-forgery tokens.", "Reading", null, "https://owasp.org/www-community/attacks/csrf", 35),
                    new LessonSeed("SQL injection", "Understand why parameterised database access matters.", "Video", null, "https://owasp.org/www-community/attacks/SQL_Injection", 40),
                    new LessonSeed("Secure coding review", "Review a small MVC flow and identify security improvements.", "Challenge", null, null, 45)
                },
                new[]
                {
                    new QuestionSeed("What does XSS target?", "Browser-executed script injection", "Database backup", "DNS caching", "CPU load", "A"),
                    new QuestionSeed("What protects state-changing forms from CSRF?", "Anti-forgery tokens", "Long URLs", "HTML comments", "More CSS", "A"),
                    new QuestionSeed("What helps prevent SQL injection?", "Parameterised queries", "String concatenation", "Public SQL", "Client-only validation", "A"),
                    new QuestionSeed("Should browser input be trusted?", "No", "Always", "Only at night", "Only on localhost", "A"),
                    new QuestionSeed("What is authentication?", "Verifying identity", "Granting every permission", "Rendering HTML", "Logging out", "A")
                },
                new[]
                {
                    new ChallengeSeed("Choose the safer database access pattern", "Name the approach that binds user input as parameters instead of concatenating SQL strings.", null, null, "parameterised query", "Contains", 50, 4)
                }),

            new CourseSeed("SQL & MySQL Foundations", "sql-mysql-foundations",
                "Learn relational modelling, SQL CRUD, joins, constraints, indexing and application database thinking.",
                "Databases", "Beginner", 300, false, null, 70,
                new[]
                {
                    new LessonSeed("Relational thinking", "Model entities, keys and relationships.", "Reading", null, "https://dev.mysql.com/doc/refman/8.0/en/", 30),
                    new LessonSeed("SELECT & filtering", "Retrieve exactly the rows an application needs.", "Video", "https://www.youtube.com/embed/HXV3zeQKqGY", "https://dev.mysql.com/doc/refman/8.0/en/select.html", 35),
                    new LessonSeed("INSERT UPDATE DELETE", "Perform database CRUD carefully and safely.", "Reading", null, "https://dev.mysql.com/doc/refman/8.0/en/insert.html", 35),
                    new LessonSeed("JOINs", "Combine related data across tables.", "Reading", null, "https://dev.mysql.com/doc/refman/8.0/en/join.html", 35),
                    new LessonSeed("Constraints & indexes", "Protect data integrity and improve common lookups.", "Video", null, "https://dev.mysql.com/doc/refman/8.0/en/create-table-foreign-keys.html", 35),
                    new LessonSeed("Database design challenge", "Turn a learning requirement into a small relational schema.", "Challenge", null, null, 45)
                },
                new[]
                {
                    new QuestionSeed("Which SQL command reads rows?", "SELECT", "READ", "GET", "FETCHALL", "A"),
                    new QuestionSeed("What uniquely identifies a row?", "Primary key", "Foreign key", "Comment", "Trigger only", "A"),
                    new QuestionSeed("Which JOIN keeps only matching rows?", "INNER JOIN", "LEFT JOIN", "RIGHT JOIN", "CROSS JOIN", "A"),
                    new QuestionSeed("What do foreign keys enforce?", "Relationships", "CSS", "Passwords", "File permissions", "A"),
                    new QuestionSeed("Why use indexes?", "Faster common lookups", "To remove data", "To encrypt rows", "To replace keys", "A")
                },
                new[]
                {
                    new ChallengeSeed("Write a filter clause", "Return the SQL clause used to filter rows by a condition.", null, null, "WHERE", "Exact", 35, 1)
                }),

            new CourseSeed("Certification Practice: Python", "certification-practice-python",
                "Exam-style Python practice course with timed assessment preparation, scenario questions and a certificate path.",
                "Programming", "Intermediate", 260, true, "Code-Room Python Foundations Certificate", 70,
                new[]
                {
                    new LessonSeed("Python exam map", "Review syntax, functions, collections, exceptions and files.", "Reading", "https://www.youtube.com/embed/rfscVS0vtbw", "https://docs.python.org/3/tutorial/", 30),
                    new LessonSeed("Functions & collections", "Practise core Python patterns likely to appear in assessments.", "Video", null, "https://docs.python.org/3/tutorial/controlflow.html", 35),
                    new LessonSeed("Errors & files", "Review exception handling and safe file workflows.", "Reading", null, "https://docs.python.org/3/tutorial/errors.html", 30),
                    new LessonSeed("Data processing", "Combine dictionaries, lists and comprehensions in practical tasks.", "Reading", null, "https://docs.python.org/3/tutorial/datastructures.html", 30),
                    new LessonSeed("Timed practice set", "Attempt scenario questions under a countdown.", "Assessment", null, null, 35),
                    new LessonSeed("Final certification examination", "Complete the final assessment to qualify for a certificate.", "Final Exam", null, null, 60)
                },
                new[]
                {
                    new QuestionSeed("Function keyword", "def", "class", "func", "lambda-only", "A"),
                    new QuestionSeed("Immutable sequence", "list", "dict", "tuple", "set-only", "C"),
                    new QuestionSeed("Exception handler keyword", "catch", "except", "handle", "rescue", "B"),
                    new QuestionSeed("Dictionary access uses", "keys", "indexes only", "key-value lookup", "row ids", "C"),
                    new QuestionSeed("Which function returns the number of items?", "size", "len", "countall", "items", "B")
                },
                new[]
                {
                    new ChallengeSeed("Return a Python list length", "Give the expression that returns the number of items in [1, 2, 3].", null, null, "len([1,2,3])", "Contains", 50, 2)
                }),

            new CourseSeed("Certification Practice: Cloud", "certification-practice-cloud",
                "Cross-cloud certification practice covering core concepts, identity, networking, security and scenario reasoning.",
                "Cloud", "Intermediate", 300, true, "Code-Room Cloud Foundations Certificate", 70,
                new[]
                {
                    new LessonSeed("Cloud exam map", "Compare service models, regions, availability and shared responsibility.", "Video", "https://www.youtube.com/embed/3hLmDS179YE", "https://aws.amazon.com/what-is-cloud-computing/", 35),
                    new LessonSeed("Identity & access", "Practise cloud IAM and least privilege scenarios.", "Reading", null, "https://learn.microsoft.com/security/zero-trust/develop/identity", 35),
                    new LessonSeed("Networking & compute", "Compare virtual networks, subnets and compute choices.", "Reading", null, "https://docs.aws.amazon.com/vpc/latest/userguide/what-is-amazon-vpc.html", 40),
                    new LessonSeed("Storage & databases", "Match workloads to cloud storage and database services.", "Video", null, "https://aws.amazon.com/products/storage/", 35),
                    new LessonSeed("Security scenarios", "Work through shared responsibility and incident examples.", "Assessment", null, "https://cloudsecurityalliance.org/", 40),
                    new LessonSeed("Final cloud certification examination", "Complete the final cross-cloud assessment.", "Final Exam", null, null, 60)
                },
                new[]
                {
                    new QuestionSeed("Object storage example", "S3", "EC2", "VPC", "IAM", "A"),
                    new QuestionSeed("Identity principle", "Least privilege", "Public access", "Shared password", "Permanent root", "A"),
                    new QuestionSeed("Network segmentation uses", "Subnets", "Fonts", "Users only", "PDFs", "A"),
                    new QuestionSeed("Shared responsibility means", "Security duties are divided by layer", "Provider does everything", "Customer does nothing", "No security needed", "A"),
                    new QuestionSeed("Serverless compute example", "Lambda", "S3", "EBS", "VPC", "A")
                },
                new[]
                {
                    new ChallengeSeed("Name the least privilege principle", "State the cloud security principle for granting only required permissions.", null, null, "least privilege", "Contains", 40, 1)
                }),

            new CourseSeed("Certification Practice: Networking", "certification-practice-networking",
                "Exam preparation for routing, switching, subnetting, services and network troubleshooting scenarios.",
                "Networking", "Intermediate", 320, true, "Code-Room Networking Foundations Certificate", 70,
                new[]
                {
                    new LessonSeed("Subnetting practice", "Convert prefixes to usable address ranges.", "Reading", "https://www.youtube.com/embed/QKfk7YFILws", "https://www.cisco.com/c/en/us/support/docs/ip/routing-information-protocol-rip/13788-3.html", 45),
                    new LessonSeed("Switching & VLANs", "Practise Ethernet forwarding, VLANs and trunk concepts.", "Video", null, "https://www.cisco.com/c/en/us/support/docs/lan-switching/virtual-lans-vlan/10023-3.html", 40),
                    new LessonSeed("Routing", "Compare connected, static and dynamic routing ideas.", "Reading", null, "https://www.cisco.com/c/en/us/support/docs/ip/enhanced-interior-gateway-routing-protocol-eigrp/16406-eigrp-toc.html", 45),
                    new LessonSeed("Network services", "Review DNS, DHCP, NAT and common transport ports.", "Reading", null, "https://www.cisco.com/c/en/us/support/docs/ip/domain-name-system-dns/116167-technote-dns-00.html", 40),
                    new LessonSeed("Troubleshooting", "Use ping, traceroute, ARP, routes and interface evidence.", "Video", null, "https://www.cisco.com/c/en/us/support/docs/ip/ip-addressing-services/13788-3.html", 40),
                    new LessonSeed("Final networking examination", "Solve scenario-based questions and subnetting checks.", "Final Exam", null, null, 60)
                },
                new[]
                {
                    new QuestionSeed("OSI layers", "5", "6", "7", "8", "C"),
                    new QuestionSeed("TCP is", "Connection-oriented", "Connectionless", "Only encrypted", "A routing protocol", "A"),
                    new QuestionSeed("HTTPS default port", "21", "80", "443", "25", "C"),
                    new QuestionSeed("Private address", "8.8.8.8", "192.168.1.10", "1.1.1.1", "9.9.9.9", "B"),
                    new QuestionSeed("DNS resolves", "Names to addresses", "MACs to passwords", "Ports to files", "Users to groups", "A")
                },
                new[]
                {
                    new ChallengeSeed("Private IPv4 address", "Return a private IPv4 address from a common RFC1918 range.", null, null, "192.168.1.10", "Exact", 40, 1)
                })
        };

        /// <summary>One curated resource attached to each expanded course, matched by course slug.</summary>
        public static readonly ResourceSeed[] CourseResources =
        {
            new ResourceSeed("python-automation", "Python Standard Library Reference", "https://docs.python.org/3/library/", "Documentation"),
            new ResourceSeed("python-for-cybersecurity", "Python ipaddress Module", "https://docs.python.org/3/library/ipaddress.html", "Documentation"),
            new ResourceSeed("bash-fundamentals", "GNU Bash Reference", "https://www.gnu.org/software/bash/manual/", "Documentation"),
            new ResourceSeed("aws-cloud-essentials", "AWS Skill Builder", "https://skillbuilder.aws/", "Learning"),
            new ResourceSeed("azure-cloud-fundamentals", "Microsoft Learn \u2014 Azure", "https://learn.microsoft.com/training/azure/", "Learning"),
            new ResourceSeed("cloud-security-fundamentals", "OWASP Top 10", "https://owasp.org/www-project-top-ten/", "Security"),
            new ResourceSeed("soc-analyst-foundations", "MITRE ATT&CK", "https://attack.mitre.org/", "Security"),
            new ResourceSeed("web-application-security", "OWASP Web Security Testing Guide", "https://owasp.org/www-project-web-security-testing-guide/", "Security"),
            new ResourceSeed("sql-mysql-foundations", "MySQL 8.0 Reference Manual", "https://dev.mysql.com/doc/refman/8.0/en/", "Documentation"),
            new ResourceSeed("certification-practice-python", "Python 3 Tutorial", "https://docs.python.org/3/tutorial/", "Exam Prep"),
            new ResourceSeed("certification-practice-cloud", "AWS Cloud Practitioner Essentials", "https://aws.amazon.com/training/learn-about/cloud-practitioner/", "Exam Prep"),
            new ResourceSeed("certification-practice-networking", "Cisco Learning Network", "https://learningnetwork.cisco.com/", "Exam Prep")
        };

        public static readonly AchievementSeed[] Achievements =
        {
            new AchievementSeed("first-course", "First Course", "Enrol in your first learning path.", "\U0001F9ED", 30),
            new AchievementSeed("first-lesson", "First Steps", "Complete your first lesson.", "\U0001F680", 25),
            new AchievementSeed("quiz-starter", "Quiz Starter", "Submit your first quiz.", "\U0001F9E0", 30),
            new AchievementSeed("challenge-starter", "Challenge Accepted", "Complete your first practice challenge.", "\u26A1", 50),
            new AchievementSeed("course-finisher", "Course Finisher", "Complete your first full course.", "\U0001F3C6", 100),
            new AchievementSeed("streak-7", "Week on Fire", "Maintain a 7-day learning streak.", "\U0001F525", 100),
            new AchievementSeed("xp-500", "Five Hundred Club", "Reach 500 XP.", "\U0001F48E", 150),
            new AchievementSeed("certificate", "Certified Learner", "Earn your first Code-Room certificate.", "\U0001F393", 200),
            new AchievementSeed("cloud-path", "Cloud Explorer", "Enroll in a cloud learning path.", "\u2601\uFE0F", 50)
        };

        /// <summary>
        /// The source application rewrites the original "Networking Fundamentals" path in place into the
        /// CCNA-style path (UpgradeNetworkingCourseAsync). The end state is seeded directly instead of
        /// replaying the two-stage upgrade, so the data lands identical on a fresh database.
        /// </summary>
        public const string UpgradedNetworkingSlug = "networking-fundamentals";
        public const string UpgradedNetworkingTitle = "CCNA-Style Networking & Exam Prep";
        public const string UpgradedNetworkingNewSlug = "ccna-style-networking-exam-prep";
        public const string UpgradedNetworkingDescription =
            "A practical, exam-focused networking path covering IPv4, switching, routing, VLANs, wireless, security and troubleshooting. " +
            "This is an independent CCNA-style study path, not an official Cisco course.";
        public const string UpgradedNetworkingCertificateName = "Code-Room Networking Foundations Certificate";
        public const string UpgradedNetworkingQuizTitle = "Networking Foundations Final Examination";

        public static readonly string[] UpgradedNetworkingLessons =
        {
            "Network Fundamentals & Topologies",
            "IPv4 Addressing & Subnetting",
            "Ethernet, Switching & VLANs",
            "Routing & Static Routes",
            "Wireless, NAT & Network Services",
            "Access Control & Network Security",
            "Troubleshooting with CLI Tools",
            "Exam Scenarios & Packet Reasoning"
        };

        /// <summary>Module headings used when a course's lessons are grouped into modules.</summary>
        public static string ModuleTitle(string category, int index)
        {
            string[] titles;
            switch (category)
            {
                case "Cloud":
                    titles = new[] { "Cloud concepts", "Core services", "Architecture & security", "Exam practice" };
                    break;
                case "Programming":
                    titles = new[] { "Foundations", "Core language", "Practical development", "Practice & projects" };
                    break;
                case "Linux":
                    titles = new[] { "CLI foundations", "System administration", "Automation", "Security & troubleshooting" };
                    break;
                case "Cybersecurity":
                    titles = new[] { "Foundations", "Defensive controls", "Monitoring", "Incident response & practice" };
                    break;
                case "Networking":
                    titles = new[] { "Network fundamentals", "Switching & routing", "Services & security", "Troubleshooting & exam practice" };
                    break;
                default:
                    titles = new[] { "Foundations", "Core concepts", "Applied practice", "Assessment" };
                    break;
            }

            return titles[Math.Min(index, titles.Length - 1)];
        }
    }
}
