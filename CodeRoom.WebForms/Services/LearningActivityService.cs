using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using CodeRoom.WebForms.Data;
using CodeRoom.WebForms.Models;

namespace CodeRoom.WebForms.Services
{
    /// <summary>
    /// Centralizes learner activity side effects so Web Forms pages preserve the source application's
    /// XP, notification, achievement and streak behavior without duplicating it in page code.
    /// </summary>
    public class LearningActivityService
    {
        private readonly UserRepository users = new UserRepository();
        private readonly AchievementRepository achievements = new AchievementRepository();
        private readonly NotificationRepository notifications = new NotificationRepository();

        public void Record(
            int userId,
            string activityType,
            string description,
            string notificationTitle = null,
            string notificationMessage = null,
            string linkUrl = null,
            string notificationType = "Activity",
            int? xpOverride = null)
        {
            var user = users.GetById(userId);
            if (user == null)
            {
                throw new InvalidOperationException("The learner account could not be found.");
            }

            using (var connection = DbConnectionFactory.Open())
            using (var transaction = connection.BeginTransaction())
            {
                var currentXp = RecordInsideTransaction(
                    connection, transaction, user, activityType, description,
                    notificationTitle, notificationMessage, linkUrl, notificationType, xpOverride);

                AwardEligibleAchievements(connection, transaction, userId, activityType, currentXp);
                transaction.Commit();
            }
        }

        public bool CompleteLesson(int userId, int lessonId, int courseId, string lessonTitle, string courseTitle)
        {
            var user = users.GetById(userId);
            if (user == null)
            {
                throw new InvalidOperationException("The learner account could not be found.");
            }

            using (var connection = DbConnectionFactory.Open())
            using (var transaction = connection.BeginTransaction(IsolationLevel.Serializable))
            {
                var courseExists = ExistsByCourse(connection, transaction,
                    "SELECT COUNT(1) FROM dbo.Courses WHERE Id = @CourseId;", courseId);
                var lessonBelongs = ExistsLessonInCourse(connection, transaction, lessonId, courseId);

                if (!courseExists || !lessonBelongs)
                {
                    throw new InvalidOperationException("The requested lesson could not be found.");
                }

                if (!EnrollmentExists(connection, transaction, userId, courseId))
                {
                    throw new InvalidOperationException("Enroll in this course before completing lessons.");
                }

                var firstCompletion = new ProgressRepository().MarkCompleted(connection, transaction, userId, lessonId);
                if (!firstCompletion)
                {
                    transaction.Commit();
                    return false;
                }

                var currentXp = RecordInsideTransaction(
                    connection, transaction, user,
                    DomainValues.ActivityType.LessonCompleted,
                    "Completed " + lessonTitle,
                    "Lesson completed",
                    "Nice work. " + lessonTitle + " is now marked complete.",
                    "/Lessons/Index.aspx?id=" + courseId + "&lessonId=" + lessonId,
                    DomainValues.NotificationType.LessonCompletion,
                    null);

                AwardEligibleAchievements(connection, transaction, userId,
                    DomainValues.ActivityType.LessonCompleted, currentXp);

                var completeCount = CountCompletedLessons(connection, transaction, userId, courseId);
                var totalCount = CountLessons(connection, transaction, courseId);
                if (totalCount > 0 && completeCount == totalCount)
                {
                    const string title = "Course completed";
                    var alreadyNotified = NotificationExists(connection, transaction, userId,
                        DomainValues.NotificationType.CourseCompleted, title, "/Courses/Details.aspx?id=" + courseId);

                    if (!alreadyNotified)
                    {
                        notifications.Insert(connection, transaction, new Notification
                        {
                            UserId = userId,
                            Type = DomainValues.NotificationType.CourseCompleted,
                            Title = title,
                            Message = "You completed every lesson in " + courseTitle + ".",
                            LinkUrl = "/Courses/Details.aspx?id=" + courseId,
                            CreatedAt = DateTime.UtcNow,
                            IsRead = false
                        });

                        ActivityRepository.Insert(connection, transaction, new UserActivity
                        {
                            UserId = userId,
                            ActivityType = DomainValues.ActivityType.CourseCompleted,
                            Description = "Completed " + courseTitle,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }

                transaction.Commit();
            }

            TryRecordStreakMilestone(userId);
            return true;
        }

        public bool Enroll(int userId, int courseId, string courseTitle, string category, string lessonUrl)
        {
            var user = users.GetById(userId);
            if (user == null)
            {
                throw new InvalidOperationException("The learner account could not be found.");
            }

            using (var connection = DbConnectionFactory.Open())
            using (var transaction = connection.BeginTransaction(IsolationLevel.Serializable))
            {
                if (EnrollmentExists(connection, transaction, userId, courseId))
                {
                    transaction.Commit();
                    return false;
                }

                EnrollmentRepository.Insert(connection, transaction, userId, courseId);

                var currentXp = RecordInsideTransaction(
                    connection, transaction, user,
                    DomainValues.ActivityType.CourseEnrolled,
                    "Enrolled in " + courseTitle,
                    "Course enrolled",
                    "You are now enrolled in " + courseTitle + ". Your learning journey starts here.",
                    lessonUrl,
                    DomainValues.NotificationType.CourseEnrollment,
                    null);

                var enrollmentCount = CountEnrollments(connection, transaction, userId);
                if (enrollmentCount == 1)
                {
                    AwardAchievement(connection, transaction, userId, "first-course");
                }

                if (string.Equals(category, "Cloud", StringComparison.OrdinalIgnoreCase))
                {
                    AwardAchievement(connection, transaction, userId, "cloud-path");
                }

                if (currentXp >= 500)
                {
                    AwardAchievement(connection, transaction, userId, "xp-500");
                }

                transaction.Commit();
            }

            TryRecordStreakMilestone(userId);
            return true;
        }

        public QuizAttempt SubmitQuiz(int userId, Quiz quiz, Dictionary<int, string> answers)
        {
            if (quiz == null || quiz.Questions == null || quiz.Questions.Count == 0)
                throw new InvalidOperationException("The requested assessment could not be found.");

            var normalizedAnswers = answers ?? new Dictionary<int, string>();
            var score = quiz.Questions.Count(question =>
            {
                string chosen;
                return normalizedAnswers.TryGetValue(question.Id, out chosen) &&
                       string.Equals(chosen, question.CorrectOption, StringComparison.OrdinalIgnoreCase);
            });
            var percentage = (int)Math.Round(score * 100.0 / quiz.Questions.Count, MidpointRounding.AwayFromZero);
            var passed = new QuizRepository().HasPassed(quiz, percentage);
            var user = users.GetById(userId);
            if (user == null) throw new InvalidOperationException("The learner account could not be found.");

            using (var connection = DbConnectionFactory.Open())
            using (var transaction = connection.BeginTransaction(IsolationLevel.Serializable))
            {
                if (!EnrollmentExists(connection, transaction, userId, quiz.CourseId))
                    throw new InvalidOperationException("Enroll in this course before taking the assessment.");

                var attempt = new QuizAttempt
                {
                    UserId = userId, QuizId = quiz.Id, Score = score,
                    TotalQuestions = quiz.Questions.Count, AttemptedAt = DateTime.UtcNow, Quiz = quiz
                };
                attempt.Id = new QuizAttemptRepository().Insert(connection, transaction, attempt);

                var activityType = passed && quiz.IsCertificationExam ? DomainValues.ActivityType.ExamPassed : DomainValues.ActivityType.QuizAttempted;
                var notificationType = passed && quiz.IsCertificationExam ? DomainValues.NotificationType.ExamPassed : DomainValues.NotificationType.QuizCompleted;
                var currentXp = RecordInsideTransaction(
                    connection, transaction, user, activityType,
                    "Completed " + quiz.Title + " with " + percentage + "%",
                    passed ? "Assessment passed" : "Quiz completed",
                    "You scored " + score + "/" + quiz.Questions.Count + " (" + percentage + "%) in " + quiz.Title + ".",
                    "/Quiz/Results.aspx?id=" + attempt.Id, notificationType,
                    passed && quiz.IsCertificationExam ? (int?)150 : null);
                AwardEligibleAchievements(connection, transaction, userId, activityType, currentXp);

                if (passed && quiz.IsCertificationExam)
                {
                    var certificates = new CertificateRepository();
                    if (!certificates.ExistsForUserAndCourse(userId, quiz.CourseId))
                    {
                        var course = new CourseRepository().GetById(quiz.CourseId);
                        var certificate = new Certificate
                        {
                            UserId = userId, CourseId = quiz.CourseId, QuizAttemptId = attempt.Id,
                            CertificateNumber = CertificateRepository.FormatNumber(quiz.CourseId, userId, attempt.Id),
                            Title = course != null && !string.IsNullOrWhiteSpace(course.CertificateName) ? course.CertificateName : quiz.Title + " Certificate",
                            IssuedAt = DateTime.UtcNow
                        };
                        certificate.Id = certificates.Insert(connection, transaction, certificate);
                        var certificateXp = RecordInsideTransaction(
                            connection, transaction, user, DomainValues.ActivityType.CertificateEarned,
                            "Earned " + certificate.Title, "Certificate unlocked",
                            "You passed the certification examination and earned a Code-Room certificate.",
                            "/Certificates/Details.aspx?id=" + certificate.Id,
                            DomainValues.NotificationType.Certificate, 200);
                        AwardEligibleAchievements(connection, transaction, userId, DomainValues.ActivityType.CertificateEarned, certificateXp);
                    }
                }

                transaction.Commit();
                TryRecordStreakMilestone(userId);
                return attempt;
            }
        }

        private int RecordInsideTransaction(
            SqlConnection connection,
            SqlTransaction transaction,
            User user,
            string activityType,
            string description,
            string notificationTitle,
            string notificationMessage,
            string linkUrl,
            string notificationType,
            int? xpOverride)
        {
            var currentXp = UserRepository.AddXp(
                connection, transaction, user.Id,
                Math.Max(0, xpOverride ?? GetXp(activityType)));

            ActivityRepository.Insert(connection, transaction, new UserActivity
            {
                UserId = user.Id,
                ActivityType = activityType,
                Description = description,
                CreatedAt = DateTime.UtcNow
            });

            if (user.EmailNotificationsEnabled &&
                !string.IsNullOrWhiteSpace(notificationTitle) &&
                !string.IsNullOrWhiteSpace(notificationMessage))
            {
                notifications.Insert(connection, transaction, new Notification
                {
                    UserId = user.Id,
                    Type = notificationType,
                    Title = notificationTitle,
                    Message = notificationMessage,
                    LinkUrl = linkUrl,
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false
                });
            }

            return currentXp;
        }

        public void AwardAchievement(int userId, string code)
        {
            using (var connection = DbConnectionFactory.Open())
            using (var transaction = connection.BeginTransaction())
            {
                AwardAchievement(connection, transaction, userId, code);
                transaction.Commit();
            }
        }

        public void TryRecordStreakMilestone(int userId)
        {
            var activities = new ActivityRepository().GetRecentForStreak(userId, 400);
            var streak = CalculateStreak(activities);
            var milestones = new[] { 3, 7, 14, 30 };

            foreach (var milestone in milestones)
            {
                if (streak < milestone)
                {
                    continue;
                }

                var title = milestone + "-day learning streak";
                if (notifications.Exists(userId, DomainValues.NotificationType.StreakMilestone, title, DateTime.MinValue))
                {
                    continue;
                }

                if (users.GetById(userId) == null)
                {
                    continue;
                }

                notifications.Insert(new Notification
                {
                    UserId = userId,
                    Type = DomainValues.NotificationType.StreakMilestone,
                    Title = title,
                    Message = "You have learned on " + milestone + " consecutive days. Keep the momentum going!",
                    LinkUrl = "/Dashboard/MyDashboard.aspx",
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false
                });
            }

            if (streak >= 7)
            {
                AwardAchievement(userId, "streak-7");
            }
        }

        public static int GetXp(string activityType)
        {
            switch (activityType)
            {
                case "AccountCreated": return 25;
                case "ProfileUpdated": return 5;
                case "CourseEnrolled": return 20;
                case "LessonCompleted": return 20;
                case "QuizAttempted": return 30;
                case "ChallengeCompleted": return 50;
                case "CourseCompleted": return 100;
                case "ExamPassed": return 150;
                case "CertificateEarned": return 200;
                default: return 10;
            }
        }

        public static int GetLevel(int xp)
        {
            return Math.Max(1, (xp / 250) + 1);
        }

        public static int GetLevelProgress(int xp)
        {
            return xp % 250;
        }

        public static int CalculateStreak(IEnumerable<UserActivity> activities, DateTime? today = null)
        {
            var qualifyingTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "CourseEnrolled",
                "LessonCompleted",
                "QuizAttempted",
                "ChallengeCompleted",
                "CourseCompleted",
                "ExamPassed"
            };

            var activeDays = activities
                .Where(a => qualifyingTypes.Contains(a.ActivityType))
                .Select(a => a.CreatedAt.Date)
                .Distinct()
                .ToList();

            var cursor = (today ?? DateTime.UtcNow).Date;
            if (!activeDays.Contains(cursor))
            {
                cursor = cursor.AddDays(-1);
            }

            var streak = 0;
            while (activeDays.Contains(cursor))
            {
                streak++;
                cursor = cursor.AddDays(-1);
            }

            return streak;
        }

        private static bool ExistsByCourse(SqlConnection connection, SqlTransaction transaction, string sql, int courseId)
        {
            using (var command = SqlHelper.Prepare(connection, transaction, sql))
            {
                SqlHelper.AddInt(command, "@CourseId", courseId);
                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
        }

        private static bool ExistsLessonInCourse(SqlConnection connection, SqlTransaction transaction, int lessonId, int courseId)
        {
            const string sql =
                "SELECT COUNT(1) FROM dbo.Lessons WHERE Id = @LessonId AND CourseId = @CourseId;";

            using (var command = SqlHelper.Prepare(connection, transaction, sql))
            {
                SqlHelper.AddInt(command, "@LessonId", lessonId);
                SqlHelper.AddInt(command, "@CourseId", courseId);
                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
        }

        private static int CountCompletedLessons(SqlConnection connection, SqlTransaction transaction, int userId, int courseId)
        {
            const string sql =
                "SELECT COUNT(1) FROM dbo.Progress AS p INNER JOIN dbo.Lessons AS l ON l.Id = p.LessonId " +
                "WHERE p.UserId = @UserId AND l.CourseId = @CourseId AND p.IsCompleted = 1;";
            using (var command = SqlHelper.Prepare(connection, transaction, sql))
            {
                SqlHelper.AddInt(command, "@UserId", userId);
                SqlHelper.AddInt(command, "@CourseId", courseId);
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        private static int CountLessons(SqlConnection connection, SqlTransaction transaction, int courseId)
        {
            const string sql = "SELECT COUNT(1) FROM dbo.Lessons WHERE CourseId = @CourseId;";
            using (var command = SqlHelper.Prepare(connection, transaction, sql))
            {
                SqlHelper.AddInt(command, "@CourseId", courseId);
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        private static bool NotificationExists(SqlConnection connection, SqlTransaction transaction, int userId, string type, string title, string linkUrl)
        {
            const string sql =
                "SELECT COUNT(1) FROM dbo.Notifications WHERE UserId = @UserId AND [Type] = @Type " +
                "AND Title = @Title AND LinkUrl = @LinkUrl;";
            using (var command = SqlHelper.Prepare(connection, transaction, sql))
            {
                SqlHelper.AddInt(command, "@UserId", userId);
                SqlHelper.AddNVarChar(command, "@Type", type, 40);
                SqlHelper.AddNVarChar(command, "@Title", title, 160);
                SqlHelper.AddNVarChar(command, "@LinkUrl", linkUrl, 300);
                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
        }

        private static bool EnrollmentExists(SqlConnection connection, SqlTransaction transaction, int userId, int courseId)
        {
            const string sql = "SELECT COUNT(1) FROM dbo.Enrollments WHERE UserId = @UserId AND CourseId = @CourseId;";
            using (var command = SqlHelper.Prepare(connection, transaction, sql))
            {
                SqlHelper.AddInt(command, "@UserId", userId);
                SqlHelper.AddInt(command, "@CourseId", courseId);
                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
        }

        private static int CountEnrollments(SqlConnection connection, SqlTransaction transaction, int userId)
        {
            const string sql = "SELECT COUNT(1) FROM dbo.Enrollments WHERE UserId = @UserId;";
            using (var command = SqlHelper.Prepare(connection, transaction, sql))
            {
                SqlHelper.AddInt(command, "@UserId", userId);
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        private void AwardEligibleAchievements(
            SqlConnection connection,
            SqlTransaction transaction,
            int userId,
            string activityType,
            int currentXp)
        {
            switch (activityType)
            {
                case "LessonCompleted":
                    AwardAchievement(connection, transaction, userId, "first-lesson");
                    break;
                case "QuizAttempted":
                    AwardAchievement(connection, transaction, userId, "quiz-starter");
                    break;
                case "ChallengeCompleted":
                    AwardAchievement(connection, transaction, userId, "challenge-starter");
                    break;
                case "CourseCompleted":
                    AwardAchievement(connection, transaction, userId, "course-finisher");
                    break;
                case "CertificateEarned":
                    AwardAchievement(connection, transaction, userId, "certificate");
                    break;
            }

            if (currentXp >= 500)
            {
                AwardAchievement(connection, transaction, userId, "xp-500");
            }
        }

        private void AwardAchievement(
            SqlConnection connection,
            SqlTransaction transaction,
            int userId,
            string code)
        {
            var achievement = achievements.Award(connection, transaction, userId, code);
            if (achievement == null)
            {
                return;
            }

            if (achievement.XpReward > 0)
            {
                UserRepository.AddXp(connection, transaction, userId, achievement.XpReward);
            }

            notifications.Insert(connection, transaction, new Notification
            {
                UserId = userId,
                Type = DomainValues.NotificationType.Achievement,
                Title = "Achievement unlocked: " + achievement.Name,
                Message = achievement.Description,
                LinkUrl = "/Profile/Index.aspx",
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            });
        }
    }
}
