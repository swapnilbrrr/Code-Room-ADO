using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading;
using CodeRoom.WebForms.Data;
using CodeRoom.WebForms.Models;
using CodeRoom.WebForms.Services;

namespace CodeRoom.DataLayerTests
{
    /// <summary>
    /// Executes the Phase 3 acceptance checks against the real SQL Server instance configured in
    /// App.config: connection, database creation, schema, seeding, seeding again, CRUD, foreign keys,
    /// unique constraints, cascade behaviour, transaction commit, transaction rollback and the atomic
    /// XP update. Nothing here is mocked; every assertion is made by reading the database back.
    /// </summary>
    internal static class Program
    {
        private static int _passed;
        private static int _failed;

        private static int Main()
        {
            Console.WriteLine("Code-Room data layer tests");
            Console.WriteLine("Target: " + ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
            Console.WriteLine();

            Section("0. Reset test rows", Reset);
            Section("1. ADO.NET connection", Connection);
            Section("2. Database and schema", InitializationAndSchema);
            Section("2b. Application database connection", ApplicationDatabaseConnection);
            Section("3. Seed data", Seeding);
            Section("3b. Course aggregates", CourseAggregates);
            Section("4. Create, read, update, delete", Crud);
            Section("5. Unique constraints, foreign keys and cascade behaviour", ConstraintsAndForeignKeys);
            Section("6. Transactions", Transactions);
            Section("7. Atomic XP updates", AtomicXp);

            Console.WriteLine();
            Console.WriteLine("Passed: " + _passed + ", Failed: " + _failed);
            return _failed == 0 ? 0 : 1;
        }

        private static void Section(string name, Action body)
        {
            try
            {
                body();
            }
            catch (Exception ex)
            {
                Check(name + " aborted", false, ex.GetType().Name + ": " + ex.Message.Split('\n')[0]);
                var trace = (ex.StackTrace ?? "").Split('\n');
                for (var i = 0; i < Math.Min(4, trace.Length); i++)
                {
                    Console.WriteLine("          " + trace[i].Trim());
                }
            }
        }

        // ------------------------------------------------------- 0. reset leftovers --

        /// <summary>
        /// Remove rows a previously aborted run may have left behind, so the counts below describe this
        /// run only. Matches just the names this harness generates.
        /// </summary>
        private static void Reset()
        {
            Heading("0. Reset rows left by an earlier aborted run");

            if (!DatabaseInitializer.DatabaseExists("CodeRoomDb"))
            {
                Console.WriteLine("       CodeRoomDb does not exist yet - section 2 will create it, nothing to reset.");
                return;
            }

            var usersRemoved = SqlHelper.ExecuteNonQuery(
                "DELETE FROM dbo.Users WHERE Username LIKE N'test.learner.%' OR Username LIKE N'constraint.probe.%' " +
                "OR Username LIKE N'other.%' OR Email LIKE N'%@coderoom.test';");
            var coursesRemoved = SqlHelper.ExecuteNonQuery(
                "DELETE FROM dbo.Courses WHERE Slug LIKE N'scratch-course-%' OR Title LIKE N'Scratch Course %';");

            Console.WriteLine("       Test users removed: " + usersRemoved + ", scratch courses removed: " + coursesRemoved);

            using (var connection = DbConnectionFactory.Open())
            {
                Check("No test-owned rows remain",
                    Count(connection, "SELECT COUNT(1) FROM dbo.Users WHERE Email LIKE N'%@coderoom.test';") == 0);
            }
        }

        // ------------------------------------------------------------------ 1. connection --

        private static void Connection()
        {
            Heading("1. ADO.NET connection");

            using (var connection = DbConnectionFactory.OpenToInstance())
            {
                Check("SqlConnection opens against the instance", connection.State == ConnectionState.Open);

                var version = Scalar(connection, null,
                    "SELECT CAST(SERVERPROPERTY('ProductVersion') AS NVARCHAR(128)) + ' | ' + CAST(SERVERPROPERTY('Edition') AS NVARCHAR(128));") + "";
                Console.WriteLine("       " + version);
                var fullVersion = Scalar(connection, null, "SELECT @@VERSION;") + "";
                Console.WriteLine("       " + fullVersion.Split('\n')[0].Trim());
            }
        }

        /// <summary>
        /// Open the application database itself. Runs after initialization because a first run has no
        /// database to open until the initializer creates it.
        /// </summary>
        private static void ApplicationDatabaseConnection()
        {
            Heading("2b. SqlConnection against the application database");

            using (var connection = new SqlConnection(DbConnectionFactory.ConnectionString))
            {
                connection.Open();
                Check("SqlConnection opens against CodeRoomDb", connection.Database == "CodeRoomDb");
            }

            using (var connection = DbConnectionFactory.Open())
            {
                Check("DbConnectionFactory.Open returns an open connection",
                    connection.State == ConnectionState.Open);
            }
        }

        // ------------------------------------------------- 2. database and schema creation --

        private static void InitializationAndSchema()
        {
            Heading("2. Database and schema");

            var existed = DatabaseInitializer.DatabaseExists("CodeRoomDb");
            Console.WriteLine("       CodeRoomDb existed before this run: " + existed);

            var result = DatabaseInitializer.InitializeWithSeedData();
            Check("Initialization ran", result.BatchesExecuted > 0);
            Console.WriteLine("       Database created by this run: " + result.DatabaseCreated
                              + ", schema batches executed: " + result.BatchesExecuted
                              + ", seed rows created: " + result.SeedRowsCreated);

            DatabaseInitializer.ResetForTests();

            ProbeFreshDatabase();

            var tables = DatabaseInitializer.GetTables("CodeRoomDb");
            Check("18 tables present", tables.Count == 18, "found " + tables.Count);

            var expected = new[]
            {
                "Achievements", "AdminAuditLogs", "Announcements", "Certificates", "Challenges",
                "CourseModules", "Courses", "Enrollments", "Lessons", "Notifications", "Progress",
                "Questions", "QuizAttempts", "Quizzes", "Resources", "UserAchievements", "UserActivities", "Users"
            };

            var missing = expected.Where(name => !tables.Contains(name)).ToArray();
            Check("Every expected table exists", missing.Length == 0, missing.Length == 0 ? "" : string.Join(", ", missing));

            using (var connection = DbConnectionFactory.Open())
            {
                var foreignKeys = Convert.ToInt32(Scalar(connection, null, "SELECT COUNT(1) FROM sys.foreign_keys;"));
                Check("22 foreign key relationships", foreignKeys == 22, "found " + foreignKeys);

                var untrusted = Convert.ToInt32(Scalar(connection, null, "SELECT COUNT(1) FROM sys.foreign_keys WHERE is_not_trusted = 1;"));
                Check("No untrusted foreign keys", untrusted == 0, "found " + untrusted);

                var checks = Convert.ToInt32(Scalar(connection, null, "SELECT COUNT(1) FROM sys.check_constraints;"));
                Console.WriteLine("       CHECK constraints: " + checks);

                var indexes = Convert.ToInt32(Scalar(connection, null,
                    "SELECT COUNT(1) FROM sys.indexes WHERE is_primary_key = 0 AND is_unique_constraint = 0 AND name IS NOT NULL;"));
                Console.WriteLine("       Non-primary indexes: " + indexes);

                var cascades = Scalar(connection, null,
                    "SELECT STRING_AGG(name, ', ') FROM sys.foreign_keys WHERE delete_referential_action = 1;");
                Console.WriteLine("       ON DELETE CASCADE: " + cascades);

                var restrict = Scalar(connection, null,
                    "SELECT STRING_AGG(name, ', ') FROM sys.foreign_keys WHERE delete_referential_action = 0;");
                Console.WriteLine("       NO ACTION (multiple cascade paths handled in code): " + restrict);
            }
        }

        /// <summary>
        /// Prove the create-database and schema-build paths of the initializer on a throwaway database
        /// name, so the checks describe a server that had nothing. The database-level statements run on a
        /// master connection; the schema script itself carries a "USE [CodeRoomDb];" batch, which is
        /// retargeted at the probe database for this run only. The probe is dropped afterwards.
        /// </summary>
        private static void ProbeFreshDatabase()
        {
            const string probeDatabase = "CodeRoomDb_SchemaProbe";

            using (var master = DbConnectionFactory.OpenToInstance())
            {
                SqlHelper.ExecuteNonQuery(master, null,
                    "IF DB_ID(@Name) IS NOT NULL BEGIN ALTER DATABASE [" + probeDatabase + "] " +
                    "SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [" + probeDatabase + "]; END;",
                    new SqlParameter("@Name", SqlDbType.NVarChar, 128) { Value = probeDatabase });
            }

            Check("Initialization detects a missing database", !DatabaseInitializer.DatabaseExists(probeDatabase));
            DatabaseInitializer.CreateDatabase(probeDatabase);
            Check("DatabaseInitializer.CreateDatabase creates it", DatabaseInitializer.DatabaseExists(probeDatabase));

            var probeScript = File.ReadAllText(DatabaseInitializer.MapPath(DatabaseInitializer.SchemaScript))
                .Replace("USE [CodeRoomDb];", "USE [" + probeDatabase + "];");

            var probeBatches = DatabaseInitializer.ExecuteScript(probeScript, probeDatabase);
            Check("Schema script executes against a new database", probeBatches > 0, probeBatches + " batches");

            var probeTables = DatabaseInitializer.GetTables(probeDatabase);
            Check("Schema script builds all 18 tables in the probe database", probeTables.Count == 18,
                probeTables.Count + " tables");

            using (var master = DbConnectionFactory.OpenToInstance())
            {
                SqlHelper.ExecuteNonQuery(master, null,
                    "ALTER DATABASE [" + probeDatabase + "] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; " +
                    "DROP DATABASE [" + probeDatabase + "];");
            }

            Check("Probe database dropped after the test", !DatabaseInitializer.DatabaseExists(probeDatabase));
        }

        // ------------------------------------------------------------------ 3. seeding --

        private static void Seeding()
        {
            Heading("3. Seed data");

            using (var connection = DbConnectionFactory.Open())
            {
                Check("8 demo users seeded", Count(connection, "SELECT COUNT(1) FROM dbo.Users;") == 8,
                    Count(connection, "SELECT COUNT(1) FROM dbo.Users;") + " users");
                Check("1 SuperAdmin, 1 Admin, 6 Student",
                    Count(connection, "SELECT COUNT(1) FROM dbo.Users WHERE Role = N'SuperAdmin';") == 1
                    && Count(connection, "SELECT COUNT(1) FROM dbo.Users WHERE Role = N'Admin';") == 1
                    && Count(connection, "SELECT COUNT(1) FROM dbo.Users WHERE Role = N'Student';") == 6);
                Check("20 courses seeded (8 catalogue + 12 expanded)",
                    Count(connection, "SELECT COUNT(1) FROM dbo.Courses;") == 20,
                    Count(connection, "SELECT COUNT(1) FROM dbo.Courses;") + " courses");
                Check("Lessons, modules, quizzes, questions and challenges seeded",
                    Count(connection, "SELECT COUNT(1) FROM dbo.Lessons;") > 0
                    && Count(connection, "SELECT COUNT(1) FROM dbo.CourseModules;") > 0
                    && Count(connection, "SELECT COUNT(1) FROM dbo.Quizzes;") == 20
                    && Count(connection, "SELECT COUNT(1) FROM dbo.Questions;") == 100
                    && Count(connection, "SELECT COUNT(1) FROM dbo.Challenges;") > 0);
                Check("9 achievements and 15 resources seeded",
                    Count(connection, "SELECT COUNT(1) FROM dbo.Achievements;") == 9
                    && Count(connection, "SELECT COUNT(1) FROM dbo.Resources;") >= 15,
                    Count(connection, "SELECT COUNT(1) FROM dbo.Resources;") + " resources");
                Check("2 announcements seeded", Count(connection, "SELECT COUNT(1) FROM dbo.Announcements;") == 2);
                Check("Networking course carries the CCNA-style content",
                    Count(connection, "SELECT COUNT(1) FROM dbo.Courses WHERE Slug = N'ccna-style-networking-exam-prep';") == 1);

                var hash = Scalar(connection, null,
                    "SELECT TOP 1 PasswordHash FROM dbo.Users WHERE Username = N'swapnil';") + "";
                Check("Demo password hashes use the PBKDF2 format", hash.Split('.').Length == 3 && hash.StartsWith("100000."),
                    hash.Substring(0, Math.Min(24, hash.Length)) + "...");
            }

            Console.WriteLine("       Running the seed a second time to prove idempotency...");
            int resourceCountBeforeReseed;
            using (var connection = DbConnectionFactory.Open())
            {
                resourceCountBeforeReseed = Count(connection, "SELECT COUNT(1) FROM dbo.Resources;");
            }

            var second = DbSeeder.Seed();
            Check("Second seed run creates no rows", second.TotalRowsCreated == 0, second.TotalRowsCreated + " rows");
            Check("Second seed run repairs no rows", second.UsersRepaired == 0 && second.LessonsRepaired == 0);

            using (var connection = DbConnectionFactory.Open())
            {
                Check("Course count unchanged after re-seeding", Count(connection, "SELECT COUNT(1) FROM dbo.Courses;") == 20);
                Check("Question count unchanged after re-seeding", Count(connection, "SELECT COUNT(1) FROM dbo.Questions;") == 100);
                Check("Resource count unchanged after re-seeding",
                    Count(connection, "SELECT COUNT(1) FROM dbo.Resources;") == resourceCountBeforeReseed);
            }
        }

        // ----------------------------------------------------------- 3b. course aggregates --

        private static void CourseAggregates()
        {
            Heading("3b. Course aggregates");

            var courses = new CourseRepository();
            var resources = new ResourceRepository();
            var published = courses.GetPublished();

            Check("Published catalogue courses include their lessons",
                published.Count > 0 && published.All(c => c.Lessons != null && c.Lessons.Count > 0),
                published.Count + " published courses");

            foreach (var course in published)
            {
                var detail = courses.GetDetails(course.Id);
                Check("Course aggregate loads modules and module lessons: " + course.Title,
                    detail != null
                    && detail.Modules != null
                    && detail.Modules.All(m => m.Lessons != null),
                    detail == null ? "course not found" : detail.Modules.Count + " modules");

                Check("Course has database-backed resources: " + course.Title,
                    resources.GetByCourse(course.Id, 8).Count > 0);
            }
        }

        // -------------------------------------------------------------------- 4. CRUD --

        private static void Crud()
        {
            Heading("4. Create, read, update, delete");

            var users = new UserRepository();
            var courses = new CourseRepository();
            var lessons = new LessonRepository();
            var enrollments = new EnrollmentRepository();
            var progress = new ProgressRepository();

            var userId = users.Insert(new User
            {
                FullName = "Test Learner",
                Username = "test.learner." + Guid.NewGuid().ToString("N").Substring(0, 8),
                Email = "test." + Guid.NewGuid().ToString("N").Substring(0, 8) + "@coderoom.test",
                PasswordHash = PasswordHasher.Hash("Test.Student@2026"),
                Role = "Student",
                Bio = "Created by the data layer test run."
            });
            Check("INSERT returns the IDENTITY id", userId > 0, "id " + userId);

            var read = users.GetById(userId);
            Check("SELECT maps the row back", read != null && read.FullName == "Test Learner");
            Check("Nullable Bio round-trips", read != null && read.Bio == "Created by the data layer test run.");
            Check("Nullable AvatarUrl stays null", read != null && read.AvatarUrl == null);
            Check("Default Role and theme mapped", read != null && read.Role == "Student" && read.ThemePreference == "system");

            read.FullName = "Test Learner Renamed";
            users.Update(read);
            Check("UPDATE persists", users.GetById(userId).FullName == "Test Learner Renamed");

            var courseId = courses.GetPublished().Select(c => c.Id).First();
            enrollments.Insert(userId, courseId);
            Check("Enrollment created", enrollments.IsEnrolled(userId, courseId));

            var lessonId = LessonRepository.GetByCourse(courseId).First().Id;
            Check("First completion reports true", progress.MarkCompleted(userId, lessonId));
            Check("Repeat completion reports false", !progress.MarkCompleted(userId, lessonId));
            Check("Progress row readable", progress.Get(userId, lessonId) != null);
            Check("Completed lesson appears in the completed set", progress.GetCompletedLessonIds(userId, courseId).Contains(lessonId));

            users.Delete(userId);
            Check("DELETE removes the user", users.GetById(userId) == null);
            Check("Cascade removed the enrollment", !enrollments.IsEnrolled(userId, courseId));
            Check("Cascade removed the progress row", progress.Get(userId, lessonId) == null);
        }

        // ------------------------------------------- 5. constraints and foreign keys --

        private static void ConstraintsAndForeignKeys()
        {
            Heading("5. Unique constraints, foreign keys and cascade behaviour");

            var users = new UserRepository();
            var email = "duplicate." + Guid.NewGuid().ToString("N").Substring(0, 8) + "@coderoom.test";

            var firstId = users.Insert(new User
            {
                FullName = "Constraint Probe",
                Username = "constraint.probe." + Guid.NewGuid().ToString("N").Substring(0, 6),
                Email = email,
                PasswordHash = PasswordHasher.Hash("Probe.Student@2026")
            });

            ExpectSqlError("Users.Username unique index is enforced", new[] { 2601, 2627 }, () =>
                users.Insert(new User
                {
                    FullName = "Duplicate Username",
                    Username = users.GetById(firstId).Username,
                    Email = "other." + Guid.NewGuid().ToString("N").Substring(0, 8) + "@coderoom.test",
                    PasswordHash = PasswordHasher.Hash("Probe.Student@2026")
                }));

            ExpectSqlError("Users.Email unique index is enforced", new[] { 2601, 2627 }, () =>
                users.Insert(new User
                {
                    FullName = "Duplicate Email",
                    Username = "other." + Guid.NewGuid().ToString("N").Substring(0, 8),
                    Email = email,
                    PasswordHash = PasswordHasher.Hash("Probe.Student@2026")
                }));

            ExpectSqlError("Progress.UserId foreign key is enforced", new[] { 547 }, () =>
                SqlHelper.ExecuteNonQuery(
                    "INSERT INTO dbo.Progress (UserId, LessonId, IsCompleted, CompletedAt) VALUES (@UserId, @LessonId, 1, SYSUTCDATETIME());",
                    new SqlParameter("@UserId", SqlDbType.Int) { Value = 999999 },
                    new SqlParameter("@LessonId", SqlDbType.Int) { Value = 1 }));

            using (var connection = DbConnectionFactory.Open())
            {
                ExpectSqlError("Enrollments (UserId, CourseId) unique index is enforced", new[] { 2601, 2627 }, () =>
                {
                    var courseId = Convert.ToInt32(Scalar(connection, null, "SELECT MIN(Id) FROM dbo.Courses;"));
                    SqlHelper.ExecuteNonQuery(connection, null,
                        "INSERT INTO dbo.Enrollments (UserId, CourseId, EnrolledAt) VALUES (@UserId, @CourseId, SYSUTCDATETIME());",
                        new SqlParameter("@UserId", SqlDbType.Int) { Value = firstId },
                        new SqlParameter("@CourseId", SqlDbType.Int) { Value = courseId });
                    SqlHelper.ExecuteNonQuery(connection, null,
                        "INSERT INTO dbo.Enrollments (UserId, CourseId, EnrolledAt) VALUES (@UserId, @CourseId, SYSUTCDATETIME());",
                        new SqlParameter("@UserId", SqlDbType.Int) { Value = firstId },
                        new SqlParameter("@CourseId", SqlDbType.Int) { Value = courseId });
                });

                ExpectSqlError("Notifications.Type CHECK constraint is enforced", new[] { 547 }, () =>
                    SqlHelper.ExecuteNonQuery(connection, null,
                        "INSERT INTO dbo.Notifications (UserId, [Type], Title, Message, LinkUrl, IsRead, CreatedAt) " +
                        "VALUES (@UserId, N'NotAType', N'Title', N'Message', NULL, 0, SYSUTCDATETIME());",
                        new SqlParameter("@UserId", SqlDbType.Int) { Value = firstId }));
            }

            // Course deletion must cascade to lessons (and the repository ordering must handle the
            // NO ACTION relationships that SQL Server could not express as cascades).
            var courses = new CourseRepository();
            var modules = new CourseModuleRepository();
            var lessons = new LessonRepository();
            var challenges = new ChallengeRepository();

            var scratchCourse = courses.Insert(new Course
            {
                Title = "Scratch Course " + Guid.NewGuid().ToString("N").Substring(0, 6),
                Slug = "scratch-course-" + Guid.NewGuid().ToString("N").Substring(0, 8),
                Description = "Temporary course created by the data layer tests.",
                Category = "Programming",
                Level = "Beginner"
            });

            var scratchLesson = lessons.Insert(new Lesson
            {
                CourseId = scratchCourse,
                Title = "Scratch Lesson",
                Content = new string('x', 800),
                Order = 1
            });

            var scratchModule = modules.Insert(new CourseModule
            {
                CourseId = scratchCourse,
                Title = "Scratch Module",
                Description = "Temporary module created by the data layer tests.",
                Order = 1
            });

            lessons.Update(GetLesson(lessons, scratchLesson, scratchModule));
            var scratchChallenge = challenges.Insert(new Challenge
            {
                CourseId = scratchCourse,
                LessonId = scratchLesson,
                Title = "Scratch Challenge",
                Instructions = "Type the answer.",
                ExpectedAnswer = "answer",
                ValidationMode = "Exact",
                Points = 10
            });

            using (var connection = DbConnectionFactory.Open())
            {
                ExpectSqlError("FK_Lessons_CourseModules is NO ACTION, so a module with lessons cannot be deleted directly",
                    new[] { 547 }, () => SqlHelper.ExecuteNonQuery(connection, null,
                        "DELETE FROM dbo.CourseModules WHERE Id = @Id;",
                        new SqlParameter("@Id", SqlDbType.Int) { Value = scratchModule }));

                ExpectSqlError("FK_Challenges_Lessons is NO ACTION, so a lesson with challenges cannot be deleted directly",
                    new[] { 547 }, () => SqlHelper.ExecuteNonQuery(connection, null,
                        "DELETE FROM dbo.Lessons WHERE Id = @Id;",
                        new SqlParameter("@Id", SqlDbType.Int) { Value = scratchLesson }));
            }

            // The repository methods do the explicit null-first ordering the schema comment promises.
            modules.Delete(scratchModule);
            Check("CourseModuleRepository.Delete cleared the lesson link first",
                GetLesson(lessons, scratchLesson, null).CourseModuleId == null);

            challenges.Delete(scratchChallenge);
            courses.Delete(scratchCourse);

            using (var connection = DbConnectionFactory.Open())
            {
                Check("CourseRepository.Delete removed the course",
                    Count(connection, "SELECT COUNT(1) FROM dbo.Courses WHERE Id = " + scratchCourse + ";") == 0);
                Check("ON DELETE CASCADE removed the lesson",
                    Count(connection, "SELECT COUNT(1) FROM dbo.Lessons WHERE Id = " + scratchLesson + ";") == 0);
            }

            users.Delete(firstId);
            Check("Probe user cleaned up", users.GetById(firstId) == null);
        }

        private static Lesson GetLesson(LessonRepository lessons, int lessonId, int? expectedModuleId)
        {
            var all = lessons.GetAllByCourse(lessons.GetDetailedById(lessonId).CourseId);
            var lesson = all.FirstOrDefault(l => l.Id == lessonId) ?? lessons.GetDetailedById(lessonId);
            if (expectedModuleId.HasValue)
            {
                lesson.CourseModuleId = expectedModuleId.Value;
            }
            return lesson;
        }

        // ---------------------------------------------------------------- 6. transactions --

        private static void Transactions()
        {
            Heading("6. Transactions");

            var notifications = new NotificationRepository();
            var userId = new UserRepository().GetByUsername("bijay").Id;

            // Commit path.
            var committed = SqlHelper.WithTransaction<int>((connection, transaction) =>
                notifications.Insert(connection, transaction, new Notification
                {
                    UserId = userId,
                    Type = "Activity",
                    Title = "Committed by test",
                    Message = "This notification row was written inside a committed transaction.",
                    IsRead = false
                }));

            Check("Transaction commit persisted the row",
                notifications.GetById(committed, userId) != null, "id " + committed);

            // Rollback path: the same work, deliberately abandoned.
            int abandoned = 0;
            try
            {
                SqlHelper.WithTransaction((connection, transaction) =>
                {
                    abandoned = notifications.Insert(connection, transaction, new Notification
                    {
                        UserId = userId,
                        Type = "Activity",
                        Title = "Rolled back by test",
                        Message = "This row must not survive.",
                        IsRead = false
                    });

                    throw new InvalidOperationException("Forced failure to prove the rollback.");
                });
            }
            catch (InvalidOperationException)
            {
                Console.WriteLine("       Forced failure raised as expected.");
            }

            Check("Transaction rollback removed the row", notifications.GetById(abandoned, userId) == null, "id " + abandoned);

            using (var connection = DbConnectionFactory.Open())
            {
                Check("Rolled-back title is absent",
                    Count(connection, "SELECT COUNT(1) FROM dbo.Notifications WHERE Title = N'Rolled back by test';") == 0);
            }

            notifications.MarkRead(userId, committed);
            SqlHelper.ExecuteNonQuery("DELETE FROM dbo.Notifications WHERE Id = @Id;",
                new SqlParameter("@Id", SqlDbType.Int) { Value = committed });
        }

        // ------------------------------------------------------------------- 7. XP --

        private static void AtomicXp()
        {
            Heading("7. Atomic XP updates");

            var users = new UserRepository();
            var userId = users.GetByUsername("anisha").Id;
            var start = users.GetById(userId).Xp;

            var threads = Enumerable.Range(0, 8)
                .Select(index => new Thread(() => users.AddXp(userId, 5)))
                .ToArray();

            foreach (var thread in threads)
            {
                thread.Start();
            }

            foreach (var thread in threads)
            {
                thread.Join();
            }

            var after = users.GetById(userId).Xp;
            Check("8 concurrent AddXp(+5) calls add exactly 40 XP", after == start + 40,
                start + " -> " + after);

            SqlHelper.ExecuteNonQuery("UPDATE dbo.Users SET Xp = @Xp WHERE Id = @UserId;",
                new SqlParameter("@Xp", SqlDbType.Int) { Value = start },
                new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
            Check("XP returned to its starting value", users.GetById(userId).Xp == start);

            Console.WriteLine("       (The increment is written as 'Xp = Xp + @Amount' in SQL, not read and rewritten in C#.)");
        }

        // ------------------------------------------------------------------- plumbing --

        private static void Heading(string text)
        {
            Console.WriteLine();
            Console.WriteLine(text);
        }

        private static void Check(string name, bool condition)
        {
            Check(name, condition, null);
        }

        private static void Check(string name, bool condition, string detail)
        {
            if (condition)
            {
                _passed++;
                Console.WriteLine("  PASS  " + name);
            }
            else
            {
                _failed++;
                Console.WriteLine("  FAIL  " + name + (string.IsNullOrEmpty(detail) ? "" : "  [" + detail + "]"));
            }
        }

        private static void ExpectSqlError(string name, int[] numbers, Action action)
        {
            try
            {
                action();
                Check(name, false, "no error was raised");
            }
            catch (SqlException ex)
            {
                Check(name, numbers.Contains(ex.Number), "SQL error " + ex.Number + ": " + ex.Message.Split('\n')[0]);
            }
            catch (Exception ex)
            {
                Check(name, false, ex.GetType().Name + ": " + ex.Message);
            }
        }

        private static object Scalar(SqlConnection connection, SqlTransaction transaction, string sql)
        {
            using (var command = SqlHelper.Prepare(connection, transaction, sql))
            {
                return command.ExecuteScalar();
            }
        }

        private static int Count(SqlConnection connection, string sql)
        {
            var value = Scalar(connection, null, sql);
            return value == null || value == DBNull.Value ? 0 : Convert.ToInt32(value);
        }
    }
}
