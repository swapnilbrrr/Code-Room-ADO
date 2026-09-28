using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CodeRoom.WebForms.Models;
using CodeRoom.WebForms.Services;

namespace CodeRoom.WebForms.Data
{
    /// <summary>
    /// Writes the baseline content of the platform with plain ADO.NET. Every statement is parameterised,
    /// every step checks whether its rows already exist, and the whole run happens inside one
    /// SqlTransaction, so a failed start-up seed leaves the database exactly as it was.
    ///
    /// This is the ADO.NET replacement for the source application's DbSeeder + LearningPlatformSeeder
    /// pair. The source relied on EF change tracking to persist an object graph; here the graph is
    /// created explicitly in dependency order (course, then lessons, then modules, then quiz,
    /// then questions, then challenges) because the identity of each parent row is needed by its children.
    /// </summary>
    public static class DbSeeder
    {
        // The repositories hold no state, so the seeder can share one instance of each.
        private static readonly UserRepository UserRepo = new UserRepository();
        private static readonly CourseRepository CourseRepo = new CourseRepository();
        private static readonly QuizRepository QuizRepo = new QuizRepository();
        private static readonly CourseModuleRepository ModuleRepo = new CourseModuleRepository();
        private static readonly ChallengeRepository ChallengeRepo = new ChallengeRepository();
        private static readonly ResourceRepository ResourceRepo = new ResourceRepository();
        private static readonly AnnouncementRepository AnnouncementRepo = new AnnouncementRepository();
        private static readonly AchievementRepository AchievementRepo = new AchievementRepository();

        /// <summary>What the current run actually did, so start-up logging can report real numbers.</summary>
        public sealed class SeedReport
        {
            public int UsersCreated { get; set; }
            public int UsersRepaired { get; set; }
            public int CoursesCreated { get; set; }
            public int LessonsCreated { get; set; }
            public int LessonsRepaired { get; set; }
            public int ModulesCreated { get; set; }
            public int LessonsAssignedToModules { get; set; }
            public int QuizzesCreated { get; set; }
            public int QuestionsCreated { get; set; }
            public int ChallengesCreated { get; set; }
            public int ResourcesCreated { get; set; }
            public int AnnouncementsCreated { get; set; }
            public int AchievementsCreated { get; set; }

            public int TotalRowsCreated
            {
                get
                {
                    return UsersCreated + CoursesCreated + LessonsCreated + ModulesCreated + QuizzesCreated
                        + QuestionsCreated + ChallengesCreated + ResourcesCreated + AnnouncementsCreated
                        + AchievementsCreated;
                }
            }

            public bool NothingChanged { get { return TotalRowsCreated == 0 && UsersRepaired == 0 && LessonsRepaired == 0; } }
        }

        /// <summary>Run the seed in its own transaction on the default connection.</summary>
        public static SeedReport Seed()
        {
            return SqlHelper.WithTransaction<SeedReport>((connection, transaction) => Seed(connection, transaction));
        }

        /// <summary>Run the seed inside an existing unit of work.</summary>
        public static SeedReport Seed(SqlConnection connection, SqlTransaction transaction)
        {
            var report = new SeedReport();

            SeedDemoUsers(connection, transaction, report);
            SeedCatalogueCourses(connection, transaction, report);
            UpgradeNetworkingCourse(connection, transaction, report);
            SeedExpandedCourses(connection, transaction, report);
            EnsureModulesForEveryCourse(connection, transaction, report);
            BackfillLessonContentAndMedia(connection, transaction, report);
            MaterializeLessonResources(connection, transaction, report);
            SeedCuratedResources(connection, transaction, report);
            SeedAnnouncements(connection, transaction, report);
            SeedAchievements(connection, transaction, report);

            return report;
        }

        // ------------------------------------------------------------- demo users --

        private static void SeedDemoUsers(SqlConnection connection, SqlTransaction transaction, SeedReport report)
        {
            foreach (var seed in DemoSeed.Users)
            {
                const string sql = "SELECT Id, FullName, Username, Email, Role, PasswordHash " +
                                   "FROM dbo.Users WHERE Email = @Email OR Username = @Username;";

                var existing = SqlHelper.ReadFirst(connection, transaction, sql, MapAccount,
                    new SqlParameter("@Email", SqlDbType.NVarChar, 120) { Value = seed.Email },
                    new SqlParameter("@Username", SqlDbType.NVarChar, 30) { Value = seed.Username });

                if (existing == null)
                {
                    var user = new User
                    {
                        FullName = seed.FullName,
                        Username = seed.Username,
                        Email = seed.Email,
                        PasswordHash = PasswordHasher.Hash(seed.InitialPassword),
                        Role = seed.Role
                    };

                    UserRepo.Insert(connection, transaction, user);
                    report.UsersCreated++;
                    continue;
                }

                // Keep the built-in accounts usable after an older seed version.
                var needsNewPassword = !PasswordHasher.Verify(seed.InitialPassword, existing.PasswordHash);
                var needsIdentityFix = existing.FullName != seed.FullName
                    || existing.Username != seed.Username
                    || existing.Email != seed.Email
                    || existing.Role != seed.Role;

                if (needsIdentityFix)
                {
                    const string update =
                        "UPDATE dbo.Users SET FullName = @FullName, Username = @Username, " +
                        "Email = @Email, Role = @Role WHERE Id = @Id;";

                    using (var command = SqlHelper.Prepare(connection, transaction, update))
                    {
                        SqlHelper.AddNVarChar(command, "@FullName", seed.FullName, 80);
                        SqlHelper.AddNVarChar(command, "@Username", seed.Username, 30);
                        SqlHelper.AddNVarChar(command, "@Email", seed.Email, 120);
                        SqlHelper.AddNVarChar(command, "@Role", seed.Role, 30);
                        SqlHelper.AddInt(command, "@Id", existing.Id);
                        command.ExecuteNonQuery();
                    }

                    report.UsersRepaired++;
                }

                if (needsNewPassword)
                {
                    UserRepo.UpdatePasswordHash(connection, transaction, existing.Id,
                        PasswordHasher.Hash(seed.InitialPassword));
                    report.UsersRepaired++;
                }
            }
        }

        private static Account MapAccount(SqlDataReader reader)
        {
            return new Account
            {
                Id = SqlHelper.GetInt(reader, "Id"),
                FullName = SqlHelper.GetString(reader, "FullName"),
                Username = SqlHelper.GetString(reader, "Username"),
                Email = SqlHelper.GetString(reader, "Email"),
                Role = SqlHelper.GetString(reader, "Role"),
                PasswordHash = SqlHelper.GetString(reader, "PasswordHash")
            };
        }

        private sealed class Account
        {
            public int Id;
            public string FullName;
            public string Username;
            public string Email;
            public string Role;
            public string PasswordHash;
        }

        // ------------------------------------------------------------- catalogue --

        /// <summary>
        /// The original ten-lesson paths. A course is only written when its slug is still free, which is
        /// what makes the whole seed safe to execute on every application start.
        /// </summary>
        private static void SeedCatalogueCourses(SqlConnection connection, SqlTransaction transaction, SeedReport report)
        {
            foreach (var seed in CatalogueSeed.Courses)
            {
                var existingCourseId = FindCourseId(connection, transaction, seed.Slug);

                // The networking catalogue row is renamed by UpgradeNetworkingCourse.
                // Once the permanent slug exists, never recreate the legacy row on later seed runs.
                if (existingCourseId != 0)
                {
                    continue;
                }

                if (seed.Slug == PlatformSeed.UpgradedNetworkingSlug
                    && FindCourseId(connection, transaction, PlatformSeed.UpgradedNetworkingNewSlug) != 0)
                {
                    continue;
                }

                var courseId = CourseRepo.Insert(connection, transaction, new Course
                {
                    Title = seed.Title,
                    Slug = seed.Slug,
                    Description = seed.Description,
                    Category = seed.Category,
                    Level = seed.Level,
                    IsPublished = true
                });
                report.CoursesCreated++;

                for (var i = 0; i < seed.Lessons.Length; i++)
                {
                    LessonRepository.Insert(connection, transaction, new Lesson
                    {
                        CourseId = courseId,
                        Title = seed.Lessons[i],
                        Content = LessonContentBuilder.Build(seed.Title, seed.Lessons[i], i + 1),
                        VideoUrl = i == 0 ? LessonMediaCatalog.VideoFor(seed.Title) : null,
                        ResourceUrl = i == 0 ? LessonMediaCatalog.ResourceFor(seed.Title) : null,
                        Order = i + 1,
                        IsPublished = true
                    });
                    report.LessonsCreated++;
                }

                var quizId = QuizRepo.Insert(connection, transaction, new Quiz
                {
                    CourseId = courseId,
                    Title = seed.QuizTitle,
                    Description = "Check your understanding of " + seed.Title + "."
                });
                report.QuizzesCreated++;

                SeedQuestions(connection, transaction, quizId, seed.Questions, report);
            }
        }

        private static void SeedQuestions(
            SqlConnection connection,
            SqlTransaction transaction,
            int quizId,
            QuestionSeed[] questions,
            SeedReport report)
        {
            foreach (var seed in questions)
            {
                QuizRepo.InsertQuestion(connection, transaction, new Question
                {
                    QuizId = quizId,
                    QuestionText = seed.Text,
                    OptionA = seed.A,
                    OptionB = seed.B,
                    OptionC = seed.C,
                    OptionD = seed.D,
                    CorrectOption = seed.Correct
                });
                report.QuestionsCreated++;
            }
        }

        // ----------------------------------------------- networking course upgrade --

        /// <summary>
        /// Mirrors UpgradeNetworkingCourseAsync from the source application: the "Networking Fundamentals"
        /// path is rewritten in place into the CCNA-style path. Keyed on the original slug, so once the
        /// rename has happened this step is a permanent no-op.
        /// </summary>
        private static void UpgradeNetworkingCourse(SqlConnection connection, SqlTransaction transaction, SeedReport report)
        {
            var courseId = FindCourseId(connection, transaction, PlatformSeed.UpgradedNetworkingSlug);
            if (courseId == 0)
            {
                return;
            }

            const string updateCourse =
                "UPDATE dbo.Courses SET Title = @Title, Slug = @Slug, Description = @Description, " +
                "Category = @Category, Level = @Level, EstimatedMinutes = @EstimatedMinutes, " +
                "IsCertification = @IsCertification, CertificateName = @CertificateName, " +
                "PassingScorePercent = @PassingScorePercent, IsPublished = 1 WHERE Id = @Id;";

            using (var command = SqlHelper.Prepare(connection, transaction, updateCourse))
            {
                SqlHelper.AddNVarChar(command, "@Title", PlatformSeed.UpgradedNetworkingTitle, 120);
                SqlHelper.AddNVarChar(command, "@Slug", PlatformSeed.UpgradedNetworkingNewSlug, 120);
                SqlHelper.AddText(command, "@Description", PlatformSeed.UpgradedNetworkingDescription);
                SqlHelper.AddNVarChar(command, "@Category", "Networking", 60);
                SqlHelper.AddNVarChar(command, "@Level", "Intermediate", 30);
                SqlHelper.AddInt(command, "@EstimatedMinutes", 420);
                SqlHelper.AddBool(command, "@IsCertification", true);
                SqlHelper.AddNVarChar(command, "@CertificateName", PlatformSeed.UpgradedNetworkingCertificateName, 120);
                SqlHelper.AddInt(command, "@PassingScorePercent", 70);
                SqlHelper.AddInt(command, "@Id", courseId);
                command.ExecuteNonQuery();
            }

            var lessons = ReadLessons(connection, transaction, courseId);
            var rewriteCount = Math.Min(lessons.Count, PlatformSeed.UpgradedNetworkingLessons.Length);

            for (var i = 0; i < rewriteCount; i++)
            {
                var lesson = lessons[i];
                var title = PlatformSeed.UpgradedNetworkingLessons[i];

                lesson.Title = title;
                lesson.Summary = "Core concept, worked example, practical checks and exam-style reasoning.";
                lesson.Content = LessonContentBuilder.Build(PlatformSeed.UpgradedNetworkingTitle, title, i + 1);
                lesson.ContentType = i % 3 == 0 ? "Video" : "Reading";
                lesson.DurationMinutes = 30 + (i * 3);
                lesson.VideoUrl = i < 2 ? "https://www.youtube.com/embed/QKfk7YFILws" : null;

                LessonRepository.Update(connection, transaction, lesson);
                report.LessonsRepaired++;
            }

            const string updateQuiz =
                "UPDATE dbo.Quizzes SET Title = @Title, AssessmentType = @AssessmentType, " +
                "TimeLimitMinutes = @TimeLimitMinutes, PassingScorePercent = @PassingScorePercent, " +
                "IsCertificationExam = @IsCertificationExam WHERE CourseId = @CourseId;";

            using (var command = SqlHelper.Prepare(connection, transaction, updateQuiz))
            {
                SqlHelper.AddNVarChar(command, "@Title", PlatformSeed.UpgradedNetworkingQuizTitle, 160);
                SqlHelper.AddNVarChar(command, "@AssessmentType", "Final Exam", 30);
                SqlHelper.AddInt(command, "@TimeLimitMinutes", 30);
                SqlHelper.AddInt(command, "@PassingScorePercent", 70);
                SqlHelper.AddBool(command, "@IsCertificationExam", true);
                SqlHelper.AddInt(command, "@CourseId", courseId);
                command.ExecuteNonQuery();
            }
        }

        // ---------------------------------------------------- expanded platform --

        private static void SeedExpandedCourses(SqlConnection connection, SqlTransaction transaction, SeedReport report)
        {
            foreach (var seed in PlatformSeed.Courses)
            {
                var courseId = FindCourseId(connection, transaction, seed.Slug);

                if (courseId == 0)
                {
                    courseId = CourseRepo.Insert(connection, transaction, ToCourse(seed));
                    report.CoursesCreated++;
                }
                else
                {
                    // The source seeder refreshes the metadata of an existing course on every start.
                    CourseRepository.Update(connection, transaction, ToCourse(seed, courseId));
                }

                var lessons = ReadLessons(connection, transaction, courseId);
                if (lessons.Count == 0)
                {
                    for (var i = 0; i < seed.Lessons.Length; i++)
                    {
                        var item = seed.Lessons[i];

                        LessonRepository.Insert(connection, transaction, new Lesson
                        {
                            CourseId = courseId,
                            Title = item.Title,
                            Summary = item.Summary,
                            Content = LessonContentBuilder.Build(seed.Title, item.Title, i + 1),
                            ContentType = item.Type,
                            VideoUrl = item.Video,
                            ResourceUrl = item.Resource,
                            DurationMinutes = item.Duration,
                            Order = i + 1,
                            IsPublished = true
                        });
                        report.LessonsCreated++;
                    }

                    lessons = ReadLessons(connection, transaction, courseId);
                }

                SeedModules(connection, transaction, courseId, seed.Category, lessons, report);

                var quizId = FindQuizId(connection, transaction, courseId);
                if (quizId == 0)
                {
                    quizId = QuizRepo.Insert(connection, transaction, new Quiz
                    {
                        CourseId = courseId,
                        Title = seed.Certification ? seed.Title + " Final Exam" : seed.Title + " Knowledge Check",
                        Description = "Test your understanding of " + seed.Title + ".",
                        AssessmentType = seed.Certification ? "Final Exam" : "Quiz",
                        TimeLimitMinutes = seed.Certification ? 30 : 0,
                        PassingScorePercent = seed.PassScore,
                        IsCertificationExam = seed.Certification
                    });
                    report.QuizzesCreated++;
                    SeedQuestions(connection, transaction, quizId, seed.Questions, report);
                }
                else if (CountRows(connection, transaction, "SELECT COUNT(1) FROM dbo.Questions WHERE QuizId = @CourseId;", quizId) == 0)
                {
                    SeedQuestions(connection, transaction, quizId, seed.Questions, report);
                }

                if (seed.Challenges.Length > 0
                    && CountRows(connection, transaction, "SELECT COUNT(1) FROM dbo.Challenges WHERE CourseId = @CourseId;", courseId) == 0)
                {
                    foreach (var challenge in seed.Challenges)
                    {
                        var index = Math.Min(challenge.LessonOffset, lessons.Count - 1);
                        var lessonId = lessons.Count == 0 ? (int?)null : lessons[Math.Max(index, 0)].Id;

                        ChallengeRepo.Insert(connection, transaction, new Challenge
                        {
                            CourseId = courseId,
                            LessonId = lessonId,
                            Title = challenge.Title,
                            Instructions = challenge.Instructions,
                            StarterCode = challenge.StarterCode,
                            Hint = challenge.Hint,
                            ExpectedAnswer = challenge.ExpectedAnswer,
                            ValidationMode = challenge.Mode,
                            Points = challenge.Points
                        });
                        report.ChallengesCreated++;
                    }
                }
            }
        }

        private static Course ToCourse(CourseSeed seed, int id = 0)
        {
            return new Course
            {
                Id = id,
                Title = seed.Title,
                Slug = seed.Slug,
                Description = seed.Description,
                Category = seed.Category,
                Level = seed.Level,
                EstimatedMinutes = seed.Minutes,
                IsCertification = seed.Certification,
                CertificateName = seed.CertificateName,
                PassingScorePercent = seed.PassScore,
                IsPublished = true
            };
        }

        // ---------------------------------------------------------------- modules --

        /// <summary>
        /// Lessons are grouped two per module, with the module heading taken from the course category.
        /// Shared by the expanded courses and by EnsureModulesForEveryCourse.
        /// </summary>
        private static void SeedModules(
            SqlConnection connection,
            SqlTransaction transaction,
            int courseId,
            string category,
            List<Lesson> lessons,
            SeedReport report)
        {
            if (lessons.Count == 0)
            {
                return;
            }

            var modules = ReadModules(connection, transaction, courseId);

            if (modules.Count == 0)
            {
                var moduleCount = (int)Math.Ceiling(lessons.Count / 2.0);

                for (var i = 0; i < moduleCount; i++)
                {
                    var heading = PlatformSeed.ModuleTitle(category, i);

                    var moduleId = ModuleRepo.Insert(connection, transaction, new CourseModule
                    {
                        CourseId = courseId,
                        Title = "Module " + (i + 1) + " \u2014 " + heading,
                        Description = "Learn and practise the " + heading.ToLowerInvariant() + " stage of this path.",
                        Order = i + 1
                    });
                    report.ModulesCreated++;

                    modules.Add(new CourseModule { Id = moduleId, CourseId = courseId, Order = i + 1 });
                }
            }

            for (var i = 0; i < lessons.Count; i++)
            {
                if (lessons[i].CourseModuleId.HasValue || modules.Count == 0)
                {
                    continue;
                }

                var target = modules[Math.Min(i / 2, modules.Count - 1)];

                const string sql = "UPDATE dbo.Lessons SET CourseModuleId = @ModuleId WHERE Id = @Id;";
                using (var command = SqlHelper.Prepare(connection, transaction, sql))
                {
                    SqlHelper.AddInt(command, "@ModuleId", target.Id);
                    SqlHelper.AddInt(command, "@Id", lessons[i].Id);
                    command.ExecuteNonQuery();
                }

                lessons[i].CourseModuleId = target.Id;
                report.LessonsAssignedToModules++;
            }
        }

        /// <summary>
        /// The source application runs this as a separate pass so catalogue courses (which are created
        /// before modules existed) end up with the same structure as the expanded paths.
        /// </summary>
        private static void EnsureModulesForEveryCourse(SqlConnection connection, SqlTransaction transaction, SeedReport report)
        {
            const string sql = "SELECT Id, Slug, Category FROM dbo.Courses ORDER BY Id;";

            var courses = SqlHelper.ReadList(connection, transaction, sql, reader => new
            {
                Id = SqlHelper.GetInt(reader, "Id"),
                Category = SqlHelper.GetString(reader, "Category")
            });

            foreach (var course in courses)
            {
                var lessons = ReadLessons(connection, transaction, course.Id);
                SeedModules(connection, transaction, course.Id, course.Category, lessons, report);
            }
        }

        // ------------------------------------------------- content and media repair --

        /// <summary>
        /// Mirrors UpgradeLegacyDemoDataAsync's lesson pass: rebuild thin content and give the first
        /// lesson of every course its starter video and reference link.
        /// </summary>
        private static void BackfillLessonContentAndMedia(SqlConnection connection, SqlTransaction transaction, SeedReport report)
        {
            const string sql =
                "SELECT l.Id AS LessonId, l.CourseId, l.Title, l.Summary, l.Content, l.ContentType, " +
                "       l.VideoUrl, l.AudioUrl, l.ResourceUrl, l.[Order], l.DurationMinutes, l.IsPublished, " +
                "       l.CourseModuleId, c.Title AS CourseTitle " +
                "FROM dbo.Lessons AS l " +
                "INNER JOIN dbo.Courses AS c ON c.Id = l.CourseId " +
                "WHERE ISNULL(l.Content, N'') = N'' " +
                "   OR LEN(ISNULL(l.Content, N'')) < 700 " +
                "   OR (l.[Order] = 1 AND (l.VideoUrl IS NULL OR l.ResourceUrl IS NULL)) " +
                "ORDER BY l.Id;";

            var rows = SqlHelper.ReadList(connection, transaction, sql, MapLessonWithCourse);

            foreach (var row in rows)
            {
                var lesson = row.Lesson;
                var changed = false;

                if (string.IsNullOrEmpty(lesson.Content) || lesson.Content.Trim().Length < 700)
                {
                    lesson.Content = LessonContentBuilder.Build(row.CourseTitle, lesson.Title, lesson.Order);
                    changed = true;
                }

                if (lesson.Order == 1 && string.IsNullOrWhiteSpace(lesson.VideoUrl))
                {
                    lesson.VideoUrl = LessonMediaCatalog.VideoFor(row.CourseTitle);
                    changed = true;
                }

                if (lesson.Order == 1 && string.IsNullOrWhiteSpace(lesson.ResourceUrl))
                {
                    lesson.ResourceUrl = LessonMediaCatalog.ResourceFor(row.CourseTitle);
                    changed = true;
                }

                if (!changed)
                {
                    continue;
                }

                LessonRepository.Update(connection, transaction, lesson);
                report.LessonsRepaired++;
            }
        }

        // -------------------------------------------------------------- resources --

        /// <summary>
        /// Every media URL a lesson carries also becomes a catalogue resource, which is how the source
        /// application keeps the resources page populated from lesson content.
        /// </summary>
        private static void MaterializeLessonResources(SqlConnection connection, SqlTransaction transaction, SeedReport report)
        {
            const string sql =
                "SELECT l.CourseId, l.Title, l.VideoUrl, l.AudioUrl, l.ResourceUrl " +
                "FROM dbo.Lessons AS l " +
                "WHERE l.VideoUrl IS NOT NULL OR l.AudioUrl IS NOT NULL OR l.ResourceUrl IS NOT NULL " +
                "ORDER BY l.Id;";

            var lessons = SqlHelper.ReadList(connection, transaction, sql, reader => new LessonMedia
            {
                CourseId = SqlHelper.GetInt(reader, "CourseId"),
                Title = SqlHelper.GetString(reader, "Title"),
                VideoUrl = SqlHelper.GetNullableString(reader, "VideoUrl"),
                AudioUrl = SqlHelper.GetNullableString(reader, "AudioUrl"),
                ResourceUrl = SqlHelper.GetNullableString(reader, "ResourceUrl")
            });

            foreach (var lesson in lessons)
            {
                AddResourceIfMissing(connection, transaction, lesson.CourseId, lesson.VideoUrl,
                    "Video \u2014 " + lesson.Title, "Video", report);
                AddResourceIfMissing(connection, transaction, lesson.CourseId, lesson.AudioUrl,
                    "Audio \u2014 " + lesson.Title, "Audio", report);
                AddResourceIfMissing(connection, transaction, lesson.CourseId, lesson.ResourceUrl,
                    "Reference \u2014 " + lesson.Title, "Reference", report);
            }
        }

        private sealed class LessonMedia
        {
            public int CourseId;
            public string Title;
            public string VideoUrl;
            public string AudioUrl;
            public string ResourceUrl;
        }

        /// <summary>The curated per-course links plus the three global resources.</summary>
        private static void SeedCuratedResources(SqlConnection connection, SqlTransaction transaction, SeedReport report)
        {
            foreach (var seed in CatalogueSeed.Resources)
            {
                AddResourceIfMissing(connection, transaction, null, seed.Url, seed.Title, seed.Type, report);
            }

            foreach (var seed in PlatformSeed.CourseResources)
            {
                var courseId = FindCourseId(connection, transaction, seed.Slug);
                AddResourceIfMissing(connection, transaction, courseId == 0 ? (int?)null : courseId,
                    seed.Url, seed.Title, seed.Type, report);
            }
        }

        private static void AddResourceIfMissing(
            SqlConnection connection,
            SqlTransaction transaction,
            int? courseId,
            string url,
            string title,
            string type,
            SeedReport report)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return;
            }

            const string exists = "SELECT COUNT(1) FROM dbo.Resources WHERE Url = @Url;";
            if (CountRows(connection, transaction, exists, null,
                new SqlParameter("@Url", SqlDbType.NVarChar, 300) { Value = url }) > 0)
            {
                return;
            }

            ResourceRepo.Insert(connection, transaction, new Resource
            {
                CourseId = courseId,
                Title = title,
                Url = url,
                Type = type
            });
            report.ResourcesCreated++;
        }

        // ----------------------------------------------------------- announcements --

        private static void SeedAnnouncements(SqlConnection connection, SqlTransaction transaction, SeedReport report)
        {
            foreach (var seed in CatalogueSeed.Announcements)
            {
                const string exists = "SELECT COUNT(1) FROM dbo.Announcements WHERE Title = @Title;";
                if (CountRows(connection, transaction, exists, null,
                    new SqlParameter("@Title", SqlDbType.NVarChar, 160) { Value = seed.Title }) > 0)
                {
                    continue;
                }

                AnnouncementRepo.Insert(connection, transaction, new Announcement
                {
                    Title = seed.Title,
                    Message = seed.Message,
                    PublishedAt = DateTime.UtcNow.AddDays(seed.PublishedDaysAgo),
                    IsPublished = true
                });
                report.AnnouncementsCreated++;
            }
        }

        // ------------------------------------------------------------ achievements --

        private static void SeedAchievements(SqlConnection connection, SqlTransaction transaction, SeedReport report)
        {

            foreach (var seed in PlatformSeed.Achievements)
            {
                var achievement = new Achievement
                {
                    Code = seed.Code,
                    Name = seed.Name,
                    Description = seed.Description,
                    Icon = seed.Icon,
                    XpReward = seed.XpReward
                };

                if (AchievementRepo.InsertIfMissing(connection, transaction, achievement) != 0)
                {
                    report.AchievementsCreated++;
                }
            }
        }

        // ------------------------------------------------------------------ reads --

        private static int FindCourseId(SqlConnection connection, SqlTransaction transaction, string slug)
        {
            if (string.IsNullOrEmpty(slug))
            {
                return 0;
            }

            const string sql = "SELECT Id FROM dbo.Courses WHERE Slug = @Slug;";
            return CountRows(connection, transaction, sql, null,
                new SqlParameter("@Slug", SqlDbType.NVarChar, 120) { Value = slug });
        }

        private static int FindQuizId(SqlConnection connection, SqlTransaction transaction, int courseId)
        {
            const string sql = "SELECT MIN(Id) FROM dbo.Quizzes WHERE CourseId = @CourseId;";
            return CountRows(connection, transaction, sql, courseId);
        }

        /// <summary>Scalar COUNT/MIN reader; the extra parameters are for the lookups keyed by text.</summary>
        private static int CountRows(
            SqlConnection connection,
            SqlTransaction transaction,
            string sql,
            int? courseId,
            params SqlParameter[] extra)
        {
            using (var command = SqlHelper.Prepare(connection, transaction, sql))
            {
                if (courseId.HasValue)
                {
                    SqlHelper.AddInt(command, "@CourseId", courseId.Value);
                }

                if (extra != null)
                {
                    command.Parameters.AddRange(extra);
                }

                var result = command.ExecuteScalar();
                return result == null || result == SqlHelper.Null ? 0 : Convert.ToInt32(result);
            }
        }

        /// <summary>Lessons of one course, in display order, fully populated so Update can round-trip them.</summary>
        private static List<Lesson> ReadLessons(SqlConnection connection, SqlTransaction transaction, int courseId)
        {
            const string sql =
                "SELECT Id, CourseId, CourseModuleId, Title, Summary, Content, ContentType, VideoUrl, " +
                "       AudioUrl, ResourceUrl, [Order], DurationMinutes, IsPublished " +
                "FROM dbo.Lessons WHERE CourseId = @CourseId ORDER BY [Order], Id;";

            return SqlHelper.ReadList(connection, transaction, sql, MapLesson,
                new SqlParameter("@CourseId", SqlDbType.Int) { Value = courseId });
        }

        private static List<CourseModule> ReadModules(SqlConnection connection, SqlTransaction transaction, int courseId)
        {
            const string sql = "SELECT Id, CourseId, ModuleOrder FROM dbo.CourseModules " +
                               "WHERE CourseId = @CourseId ORDER BY ModuleOrder, Id;";

            return SqlHelper.ReadList(connection, transaction, sql, reader => new CourseModule
            {
                Id = SqlHelper.GetInt(reader, "Id"),
                CourseId = SqlHelper.GetInt(reader, "CourseId"),
                Order = SqlHelper.GetInt(reader, "ModuleOrder")
            }, new SqlParameter("@CourseId", SqlDbType.Int) { Value = courseId });
        }

        private static Lesson MapLesson(SqlDataReader reader)
        {
            return new Lesson
            {
                Id = SqlHelper.GetInt(reader, "Id"),
                CourseId = SqlHelper.GetInt(reader, "CourseId"),
                CourseModuleId = SqlHelper.GetNullableInt(reader, "CourseModuleId"),
                Title = SqlHelper.GetString(reader, "Title"),
                Summary = SqlHelper.GetNullableString(reader, "Summary"),
                Content = SqlHelper.GetString(reader, "Content"),
                ContentType = SqlHelper.GetString(reader, "ContentType"),
                VideoUrl = SqlHelper.GetNullableString(reader, "VideoUrl"),
                AudioUrl = SqlHelper.GetNullableString(reader, "AudioUrl"),
                ResourceUrl = SqlHelper.GetNullableString(reader, "ResourceUrl"),
                Order = SqlHelper.GetInt(reader, "Order"),
                DurationMinutes = SqlHelper.GetInt(reader, "DurationMinutes"),
                IsPublished = SqlHelper.GetBool(reader, "IsPublished")
            };
        }

        private static LessonWithCourse MapLessonWithCourse(SqlDataReader reader)
        {
            return new LessonWithCourse
            {
                CourseTitle = SqlHelper.GetString(reader, "CourseTitle"),
                Lesson = new Lesson
                {
                    Id = SqlHelper.GetInt(reader, "LessonId"),
                    CourseId = SqlHelper.GetInt(reader, "CourseId"),
                    CourseModuleId = SqlHelper.GetNullableInt(reader, "CourseModuleId"),
                    Title = SqlHelper.GetString(reader, "Title"),
                    Summary = SqlHelper.GetNullableString(reader, "Summary"),
                    Content = SqlHelper.GetString(reader, "Content"),
                    ContentType = SqlHelper.GetString(reader, "ContentType"),
                    VideoUrl = SqlHelper.GetNullableString(reader, "VideoUrl"),
                    AudioUrl = SqlHelper.GetNullableString(reader, "AudioUrl"),
                    ResourceUrl = SqlHelper.GetNullableString(reader, "ResourceUrl"),
                    Order = SqlHelper.GetInt(reader, "Order"),
                    DurationMinutes = SqlHelper.GetInt(reader, "DurationMinutes"),
                    IsPublished = SqlHelper.GetBool(reader, "IsPublished")
                }
            };
        }

        private sealed class LessonWithCourse
        {
            public Lesson Lesson;
            public string CourseTitle;
        }
    }
}
