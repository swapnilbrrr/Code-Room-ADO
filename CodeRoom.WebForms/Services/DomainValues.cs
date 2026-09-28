namespace CodeRoom.WebForms.Services
{
    /// <summary>
    /// The platform's string backed domains. These mirror the values already stored by the
    /// source application, so no numeric enums are introduced by the migration.
    /// </summary>
    public static class DomainValues
    {
        public static class CourseLevel
        {
            public const string Beginner = "Beginner";
            public const string Intermediate = "Intermediate";
        }

        public static class LessonContentType
        {
            public const string Reading = "Reading";
            public const string Video = "Video";
            public const string Audio = "Audio";
            public const string Challenge = "Challenge";
            public const string Assessment = "Assessment";
            public const string FinalExam = "Final Exam";
        }

        public static class AssessmentType
        {
            public const string Quiz = "Quiz";
            public const string FinalExam = "Final Exam";
        }

        public static class ValidationMode
        {
            public const string Exact = "Exact";
            public const string Contains = "Contains";
        }

        public static class CorrectOption
        {
            public const string A = "A";
            public const string B = "B";
            public const string C = "C";
            public const string D = "D";
        }

        public static class ResourceType
        {
            public const string Document = "Document";
            public const string Link = "Link";
            public const string Video = "Video";
            public const string Audio = "Audio";
            public const string Reference = "Reference";
        }

        public static class Theme
        {
            public const string System = "system";
            public const string Light = "light";
            public const string Dark = "dark";
        }

        public static class Visibility
        {
            public const string Public = "Public";
            public const string Members = "Members";
        }

        public static class AuditAction
        {
            public const string Created = "Created";
            public const string Updated = "Updated";
            public const string Deleted = "Deleted";
        }

        /// <summary>Notification.Type values written by the source application.</summary>
        public static class NotificationType
        {
            public const string Announcement = "Announcement";
            public const string CourseEnrollment = "CourseEnrollment";
            public const string Activity = "Activity";
            public const string StreakMilestone = "StreakMilestone";
            public const string Achievement = "Achievement";
            public const string Certificate = "Certificate";
            public const string CourseCompleted = "CourseCompleted";
            public const string ExamPassed = "ExamPassed";
            public const string QuizCompleted = "QuizCompleted";
        }

        /// <summary>UserActivity.ActivityType values written by the source application.</summary>
        public static class ActivityType
        {
            public const string AccountCreated = "AccountCreated";
            public const string ProfileUpdated = "ProfileUpdated";
            public const string CourseEnrolled = "CourseEnrolled";
            public const string LessonCompleted = "LessonCompleted";
            public const string QuizAttempted = "QuizAttempted";
            public const string ChallengeCompleted = "ChallengeCompleted";
            public const string CourseCompleted = "CourseCompleted";
            public const string ExamPassed = "ExamPassed";
            public const string CertificateEarned = "CertificateEarned";
        }
    }
}
