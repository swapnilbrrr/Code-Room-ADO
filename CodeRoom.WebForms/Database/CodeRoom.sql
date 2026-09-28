/* ============================================================================
   Code-Room - Microsoft SQL Server database
   Migration target for the ASP.NET Web Forms / .NET Framework 4.7.2 / ADO.NET
   application. Derived from the Entity Core model in
   CodeRoom.Web/Data/ApplicationDbContext.cs and the runtime MySQL schema in
   CodeRoom.Web/Services/DatabaseSchemaUpdater.cs, which is the deployed
   shape of the database.

   Type conversion applied throughout:
     AUTO_INCREMENT      -> IDENTITY(1,1)
     varchar/longtext    -> NVARCHAR / NVARCHAR(MAX)  (utf8mb4 emoji support)
     tinyint(1)          -> BIT
     datetime(6)         -> DATETIME2
     backticks           -> square brackets
     ENGINE=InnoDB       -> (removed, SQL Server has no table engine clause)

   Every statement is guarded so the script is safe to run more than once.
   Nothing here drops or alters existing data.
   ============================================================================ */

/* Filtered indexes require QUOTED_IDENTIFIER ON; some clients (sqlcmd) default
   it OFF, so the script sets it explicitly instead of relying on the client. */
SET QUOTED_IDENTIFIER ON;
GO

IF DB_ID(N'CodeRoomDb') IS NULL
BEGIN
    CREATE DATABASE [CodeRoomDb];
END
GO

USE [CodeRoomDb];
GO

/* ---------------------------------------------------------------- Users -- */
IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users
    (
        Id                      INT            IDENTITY(1,1) NOT NULL,
        FullName                NVARCHAR(80)   NOT NULL,
        Username                NVARCHAR(60)   NOT NULL,
        Email                   NVARCHAR(120)  NOT NULL,
        PasswordHash            NVARCHAR(MAX)  NOT NULL,
        Bio                     NVARCHAR(500)  NULL,
        AvatarUrl               NVARCHAR(300)  NULL,
        Role                    NVARCHAR(30)   NOT NULL
            CONSTRAINT DF_Users_Role DEFAULT (N'Student'),
        Xp                      INT            NOT NULL
            CONSTRAINT DF_Users_Xp DEFAULT (0),
        ThemePreference         NVARCHAR(20)   NOT NULL
            CONSTRAINT DF_Users_ThemePreference DEFAULT (N'system'),
        ProfileVisibility       NVARCHAR(20)   NOT NULL
            CONSTRAINT DF_Users_ProfileVisibility DEFAULT (N'Public'),
        EmailNotificationsEnabled BIT          NOT NULL
            CONSTRAINT DF_Users_EmailNotificationsEnabled DEFAULT (1),
        CreatedAt               DATETIME2      NOT NULL
            CONSTRAINT DF_Users_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_Users PRIMARY KEY (Id),
        CONSTRAINT CK_Users_Role CHECK (Role IN (N'Student', N'Admin', N'SuperAdmin')),
        CONSTRAINT CK_Users_ThemePreference CHECK (ThemePreference IN (N'system', N'light', N'dark')),
        CONSTRAINT CK_Users_ProfileVisibility CHECK (ProfileVisibility IN (N'Public', N'Members')),
        CONSTRAINT CK_Users_Xp CHECK (Xp >= 0)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Users_Email' AND object_id = OBJECT_ID(N'dbo.Users'))
    CREATE UNIQUE INDEX IX_Users_Email ON dbo.Users (Email);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Users_Username' AND object_id = OBJECT_ID(N'dbo.Users'))
    CREATE UNIQUE INDEX IX_Users_Username ON dbo.Users (Username);
GO

/* -------------------------------------------------------------- Courses -- */
IF OBJECT_ID(N'dbo.Courses', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Courses
    (
        Id                  INT            IDENTITY(1,1) NOT NULL,
        Title               NVARCHAR(120)  NOT NULL,
        Slug                NVARCHAR(140)  NOT NULL,
        Description         NVARCHAR(1000) NOT NULL,
        Category            NVARCHAR(60)   NOT NULL,
        Level               NVARCHAR(30)   NOT NULL
            CONSTRAINT DF_Courses_Level DEFAULT (N'Beginner'),
        EstimatedMinutes    INT            NOT NULL
            CONSTRAINT DF_Courses_EstimatedMinutes DEFAULT (120),
        IsCertification     BIT            NOT NULL
            CONSTRAINT DF_Courses_IsCertification DEFAULT (0),
        CertificateName     NVARCHAR(160)  NULL,
        PassingScorePercent INT            NOT NULL
            CONSTRAINT DF_Courses_PassingScorePercent DEFAULT (70),
        ThumbnailUrl        NVARCHAR(300)  NULL,
        IsPublished         BIT            NOT NULL
            CONSTRAINT DF_Courses_IsPublished DEFAULT (1),
        CreatedAt           DATETIME2      NOT NULL
            CONSTRAINT DF_Courses_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_Courses PRIMARY KEY (Id),
        CONSTRAINT CK_Courses_Level CHECK (Level IN (N'Beginner', N'Intermediate')),
        CONSTRAINT CK_Courses_PassingScore CHECK (PassingScorePercent BETWEEN 50 AND 100),
        CONSTRAINT CK_Courses_EstimatedMinutes CHECK (EstimatedMinutes BETWEEN 10 AND 1000)
    );
END
GO

/* Slug drives every course lookup (catalogue, details, seeding) but was never
   indexed in the MySQL schema. Non-unique because the model allows duplicates. */
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Courses_Slug' AND object_id = OBJECT_ID(N'dbo.Courses'))
    CREATE INDEX IX_Courses_Slug ON dbo.Courses (Slug);
GO

/* --------------------------------------------------------- CourseModules -- */
IF OBJECT_ID(N'dbo.CourseModules', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.CourseModules
    (
        Id          INT            IDENTITY(1,1) NOT NULL,
        CourseId    INT            NOT NULL,
        Title       NVARCHAR(120)  NOT NULL,
        Description NVARCHAR(500)  NOT NULL
            CONSTRAINT DF_CourseModules_Description DEFAULT (N''),
        ModuleOrder INT            NOT NULL,
        CONSTRAINT PK_CourseModules PRIMARY KEY (Id)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CourseModules_CourseId' AND object_id = OBJECT_ID(N'dbo.CourseModules'))
    CREATE INDEX IX_CourseModules_CourseId ON dbo.CourseModules (CourseId);
GO

/* --------------------------------------------------------------- Lessons -- */
IF OBJECT_ID(N'dbo.Lessons', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Lessons
    (
        Id              INT            IDENTITY(1,1) NOT NULL,
        CourseId        INT            NOT NULL,
        CourseModuleId  INT            NULL,
        Title           NVARCHAR(150)  NOT NULL,
        Summary         NVARCHAR(180)  NULL,
        Content         NVARCHAR(MAX)  NOT NULL,
        ContentType     NVARCHAR(30)   NOT NULL
            CONSTRAINT DF_Lessons_ContentType DEFAULT (N'Reading'),
        VideoUrl        NVARCHAR(300)  NULL,
        AudioUrl        NVARCHAR(300)  NULL,
        ResourceUrl     NVARCHAR(300)  NULL,
        [Order]         INT            NOT NULL,
        DurationMinutes INT            NOT NULL
            CONSTRAINT DF_Lessons_DurationMinutes DEFAULT (10),
        IsPublished     BIT            NOT NULL
            CONSTRAINT DF_Lessons_IsPublished DEFAULT (1),
        CONSTRAINT PK_Lessons PRIMARY KEY (Id),
        CONSTRAINT CK_Lessons_ContentType CHECK (ContentType IN (N'Reading', N'Video', N'Audio', N'Challenge', N'Assessment', N'Final Exam')),
        CONSTRAINT CK_Lessons_Order CHECK ([Order] BETWEEN 1 AND 999),
        CONSTRAINT CK_Lessons_Duration CHECK (DurationMinutes BETWEEN 1 AND 600)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Lessons_CourseId' AND object_id = OBJECT_ID(N'dbo.Lessons'))
    CREATE INDEX IX_Lessons_CourseId ON dbo.Lessons (CourseId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Lessons_CourseModuleId' AND object_id = OBJECT_ID(N'dbo.Lessons'))
    CREATE INDEX IX_Lessons_CourseModuleId ON dbo.Lessons (CourseModuleId);
GO

/* ----------------------------------------------------------- Enrollments -- */
IF OBJECT_ID(N'dbo.Enrollments', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Enrollments
    (
        Id         INT       IDENTITY(1,1) NOT NULL,
        UserId     INT       NOT NULL,
        CourseId   INT       NOT NULL,
        EnrolledAt DATETIME2 NOT NULL
            CONSTRAINT DF_Enrollments_EnrolledAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_Enrollments PRIMARY KEY (Id)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Enrollments_UserId_CourseId' AND object_id = OBJECT_ID(N'dbo.Enrollments'))
    CREATE UNIQUE INDEX IX_Enrollments_UserId_CourseId ON dbo.Enrollments (UserId, CourseId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Enrollments_CourseId' AND object_id = OBJECT_ID(N'dbo.Enrollments'))
    CREATE INDEX IX_Enrollments_CourseId ON dbo.Enrollments (CourseId);
GO

/* --------------------------------------------------------------- Quizzes -- */
IF OBJECT_ID(N'dbo.Quizzes', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Quizzes
    (
        Id                  INT            IDENTITY(1,1) NOT NULL,
        CourseId            INT            NOT NULL,
        Title               NVARCHAR(150)  NOT NULL,
        Description         NVARCHAR(500)  NOT NULL
            CONSTRAINT DF_Quizzes_Description DEFAULT (N''),
        AssessmentType      NVARCHAR(30)   NOT NULL
            CONSTRAINT DF_Quizzes_AssessmentType DEFAULT (N'Quiz'),
        TimeLimitMinutes    INT            NOT NULL
            CONSTRAINT DF_Quizzes_TimeLimitMinutes DEFAULT (0),
        PassingScorePercent INT            NOT NULL
            CONSTRAINT DF_Quizzes_PassingScorePercent DEFAULT (70),
        IsCertificationExam BIT            NOT NULL
            CONSTRAINT DF_Quizzes_IsCertificationExam DEFAULT (0),
        CONSTRAINT PK_Quizzes PRIMARY KEY (Id),
        CONSTRAINT CK_Quizzes_AssessmentType CHECK (AssessmentType IN (N'Quiz', N'Final Exam')),
        CONSTRAINT CK_Quizzes_TimeLimit CHECK (TimeLimitMinutes BETWEEN 0 AND 180),
        CONSTRAINT CK_Quizzes_PassingScore CHECK (PassingScorePercent BETWEEN 0 AND 100)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Quizzes_CourseId' AND object_id = OBJECT_ID(N'dbo.Quizzes'))
    CREATE INDEX IX_Quizzes_CourseId ON dbo.Quizzes (CourseId);
GO

/* ------------------------------------------------------------- Questions -- */
IF OBJECT_ID(N'dbo.Questions', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Questions
    (
        Id           INT            IDENTITY(1,1) NOT NULL,
        QuizId       INT            NOT NULL,
        QuestionText NVARCHAR(400)  NOT NULL,
        OptionA      NVARCHAR(200)  NOT NULL,
        OptionB      NVARCHAR(200)  NOT NULL,
        OptionC      NVARCHAR(200)  NOT NULL,
        OptionD      NVARCHAR(200)  NOT NULL,
        CorrectOption NVARCHAR(1)   NOT NULL
            CONSTRAINT DF_Questions_CorrectOption DEFAULT (N'A'),
        CONSTRAINT PK_Questions PRIMARY KEY (Id),
        CONSTRAINT CK_Questions_CorrectOption CHECK (CorrectOption IN (N'A', N'B', N'C', N'D'))
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Questions_QuizId' AND object_id = OBJECT_ID(N'dbo.Questions'))
    CREATE INDEX IX_Questions_QuizId ON dbo.Questions (QuizId);
GO

/* ---------------------------------------------------------- QuizAttempts -- */
IF OBJECT_ID(N'dbo.QuizAttempts', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.QuizAttempts
    (
        Id            INT       IDENTITY(1,1) NOT NULL,
        UserId        INT       NOT NULL,
        QuizId        INT       NOT NULL,
        Score         INT       NOT NULL,
        TotalQuestions INT      NOT NULL,
        AttemptedAt   DATETIME2 NOT NULL
            CONSTRAINT DF_QuizAttempts_AttemptedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_QuizAttempts PRIMARY KEY (Id)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_QuizAttempts_UserId' AND object_id = OBJECT_ID(N'dbo.QuizAttempts'))
    CREATE INDEX IX_QuizAttempts_UserId ON dbo.QuizAttempts (UserId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_QuizAttempts_QuizId' AND object_id = OBJECT_ID(N'dbo.QuizAttempts'))
    CREATE INDEX IX_QuizAttempts_QuizId ON dbo.QuizAttempts (QuizId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_QuizAttempts_UserId_AttemptedAt' AND object_id = OBJECT_ID(N'dbo.QuizAttempts'))
    CREATE INDEX IX_QuizAttempts_UserId_AttemptedAt ON dbo.QuizAttempts (UserId, AttemptedAt DESC);
GO

/* -------------------------------------------------------------- Progress -- */
IF OBJECT_ID(N'dbo.Progress', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Progress
    (
        Id          INT       IDENTITY(1,1) NOT NULL,
        UserId      INT       NOT NULL,
        LessonId    INT       NOT NULL,
        IsCompleted BIT       NOT NULL
            CONSTRAINT DF_Progress_IsCompleted DEFAULT (0),
        CompletedAt DATETIME2 NULL,
        CONSTRAINT PK_Progress PRIMARY KEY (Id)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Progress_UserId_LessonId' AND object_id = OBJECT_ID(N'dbo.Progress'))
    CREATE UNIQUE INDEX IX_Progress_UserId_LessonId ON dbo.Progress (UserId, LessonId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Progress_LessonId' AND object_id = OBJECT_ID(N'dbo.Progress'))
    CREATE INDEX IX_Progress_LessonId ON dbo.Progress (LessonId);
GO

/* ------------------------------------------------------------- Resources -- */
IF OBJECT_ID(N'dbo.Resources', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Resources
    (
        Id      INT           IDENTITY(1,1) NOT NULL,
        CourseId INT          NULL,
        Title   NVARCHAR(150) NOT NULL,
        Url     NVARCHAR(400) NOT NULL,
        [Type]  NVARCHAR(40)  NOT NULL
            CONSTRAINT DF_Resources_Type DEFAULT (N'Document'),
        CONSTRAINT PK_Resources PRIMARY KEY (Id),
        CONSTRAINT CK_Resources_Type CHECK ([Type] IN (N'Document', N'Link', N'Video', N'Audio', N'Reference', N'Documentation', N'Learning', N'Security', N'Exam Prep'))
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Resources_CourseId' AND object_id = OBJECT_ID(N'dbo.Resources'))
    CREATE INDEX IX_Resources_CourseId ON dbo.Resources (CourseId);
GO

/* ---------------------------------------------------------- Announcements -- */
IF OBJECT_ID(N'dbo.Announcements', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Announcements
    (
        Id          INT            IDENTITY(1,1) NOT NULL,
        Title       NVARCHAR(150)  NOT NULL,
        Message     NVARCHAR(1000) NOT NULL,
        PublishedAt DATETIME2      NOT NULL
            CONSTRAINT DF_Announcements_PublishedAt DEFAULT (SYSUTCDATETIME()),
        IsPublished BIT            NOT NULL
            CONSTRAINT DF_Announcements_IsPublished DEFAULT (1),
        CONSTRAINT PK_Announcements PRIMARY KEY (Id)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Announcements_PublishedAt' AND object_id = OBJECT_ID(N'dbo.Announcements'))
    CREATE INDEX IX_Announcements_PublishedAt ON dbo.Announcements (PublishedAt DESC)
        WHERE IsPublished = 1;
GO

/* -------------------------------------------------------- UserActivities -- */
IF OBJECT_ID(N'dbo.UserActivities', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.UserActivities
    (
        Id           INT           IDENTITY(1,1) NOT NULL,
        UserId       INT           NOT NULL,
        ActivityType NVARCHAR(40)  NOT NULL,
        Description  NVARCHAR(220) NOT NULL,
        CreatedAt    DATETIME2     NOT NULL
            CONSTRAINT DF_UserActivities_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_UserActivities PRIMARY KEY (Id)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_UserActivities_UserId_CreatedAt' AND object_id = OBJECT_ID(N'dbo.UserActivities'))
    CREATE INDEX IX_UserActivities_UserId_CreatedAt ON dbo.UserActivities (UserId, CreatedAt);
GO

/* --------------------------------------------------------- Notifications -- */
IF OBJECT_ID(N'dbo.Notifications', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Notifications
    (
        Id        INT           IDENTITY(1,1) NOT NULL,
        UserId    INT           NOT NULL,
        [Type]    NVARCHAR(40)  NOT NULL,
        Title     NVARCHAR(150) NOT NULL,
        Message   NVARCHAR(500) NOT NULL,
        LinkUrl   NVARCHAR(300) NULL,
        CreatedAt DATETIME2     NOT NULL
            CONSTRAINT DF_Notifications_CreatedAt DEFAULT (SYSUTCDATETIME()),
        IsRead    BIT           NOT NULL
            CONSTRAINT DF_Notifications_IsRead DEFAULT (0),
        CONSTRAINT PK_Notifications PRIMARY KEY (Id),
        CONSTRAINT CK_Notifications_Type CHECK ([Type] IN (N'Announcement', N'CourseEnrollment', N'Activity', N'StreakMilestone', N'Achievement', N'Certificate', N'CourseCompleted', N'LessonCompleted', N'ExamPassed', N'QuizCompleted', N'Challenge'))
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Notifications_UserId_IsRead_CreatedAt' AND object_id = OBJECT_ID(N'dbo.Notifications'))
    CREATE INDEX IX_Notifications_UserId_IsRead_CreatedAt ON dbo.Notifications (UserId, IsRead, CreatedAt);
GO

/* LearningActivityService writes notification types from the learner workflow.
   Keep the database constraint aligned with the source application's values.
   Repair a constraint left behind by an older run of this script. */
IF EXISTS (SELECT 1 FROM sys.check_constraints
           WHERE name = N'CK_Notifications_Type'
             AND parent_object_id = OBJECT_ID(N'dbo.Notifications')
             AND (definition NOT LIKE N'%CourseEnrollment%' OR definition NOT LIKE N'%LessonCompleted%' OR definition NOT LIKE N'%Challenge%'))
BEGIN
    ALTER TABLE dbo.Notifications DROP CONSTRAINT CK_Notifications_Type;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_Notifications_Type' AND parent_object_id = OBJECT_ID(N'dbo.Notifications'))
BEGIN
    ALTER TABLE dbo.Notifications ADD CONSTRAINT CK_Notifications_Type
        CHECK ([Type] IN (N'Announcement', N'CourseEnrollment', N'Activity', N'StreakMilestone', N'Achievement', N'Certificate', N'CourseCompleted', N'LessonCompleted', N'ExamPassed', N'QuizCompleted', N'Challenge'));
END
GO

/* ----------------------------------------------------------- Challenges -- */
IF OBJECT_ID(N'dbo.Challenges', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Challenges
    (
        Id             INT            IDENTITY(1,1) NOT NULL,
        CourseId       INT            NOT NULL,
        LessonId       INT            NULL,
        Title          NVARCHAR(160)  NOT NULL,
        Instructions   NVARCHAR(1800) NOT NULL,
        StarterCode    NVARCHAR(2000) NULL,
        Hint           NVARCHAR(600)  NULL,
        ExpectedAnswer NVARCHAR(1000) NOT NULL,
        ValidationMode NVARCHAR(30)   NOT NULL
            CONSTRAINT DF_Challenges_ValidationMode DEFAULT (N'Exact'),
        Points         INT            NOT NULL
            CONSTRAINT DF_Challenges_Points DEFAULT (50),
        CONSTRAINT PK_Challenges PRIMARY KEY (Id),
        CONSTRAINT CK_Challenges_ValidationMode CHECK (ValidationMode IN (N'Exact', N'Contains')),
        CONSTRAINT CK_Challenges_Points CHECK (Points BETWEEN 5 AND 500)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Challenges_CourseId' AND object_id = OBJECT_ID(N'dbo.Challenges'))
    CREATE INDEX IX_Challenges_CourseId ON dbo.Challenges (CourseId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Challenges_LessonId' AND object_id = OBJECT_ID(N'dbo.Challenges'))
    CREATE INDEX IX_Challenges_LessonId ON dbo.Challenges (LessonId);
GO

/* ---------------------------------------------------------- Achievements -- */
IF OBJECT_ID(N'dbo.Achievements', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Achievements
    (
        Id          INT           IDENTITY(1,1) NOT NULL,
        Name        NVARCHAR(100) NOT NULL,
        Description NVARCHAR(500) NOT NULL,
        Code        NVARCHAR(40)  NOT NULL,
        Icon        NVARCHAR(20)  NOT NULL
            CONSTRAINT DF_Achievements_Icon DEFAULT (N'★'),
        XpReward    INT           NOT NULL
            CONSTRAINT DF_Achievements_XpReward DEFAULT (0),
        CONSTRAINT PK_Achievements PRIMARY KEY (Id)
    );
END
GO

/* The UNIQUE Code index only existed in the runtime MySQL updater, never in the
   EF model. Every achievement lookup (TryAwardAchievementAsync) uses Code. */
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Achievements_Code' AND object_id = OBJECT_ID(N'dbo.Achievements'))
    CREATE UNIQUE INDEX IX_Achievements_Code ON dbo.Achievements (Code);
GO

/* ------------------------------------------------------ UserAchievements -- */
IF OBJECT_ID(N'dbo.UserAchievements', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.UserAchievements
    (
        Id            INT       IDENTITY(1,1) NOT NULL,
        UserId        INT       NOT NULL,
        AchievementId INT       NOT NULL,
        EarnedAt      DATETIME2 NOT NULL
            CONSTRAINT DF_UserAchievements_EarnedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_UserAchievements PRIMARY KEY (Id)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_UserAchievements_UserId_AchievementId' AND object_id = OBJECT_ID(N'dbo.UserAchievements'))
    CREATE UNIQUE INDEX IX_UserAchievements_UserId_AchievementId ON dbo.UserAchievements (UserId, AchievementId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_UserAchievements_AchievementId' AND object_id = OBJECT_ID(N'dbo.UserAchievements'))
    CREATE INDEX IX_UserAchievements_AchievementId ON dbo.UserAchievements (AchievementId);
GO

/* ---------------------------------------------------------- Certificates -- */
IF OBJECT_ID(N'dbo.Certificates', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Certificates
    (
        Id               INT           IDENTITY(1,1) NOT NULL,
        UserId           INT           NOT NULL,
        CourseId         INT           NOT NULL,
        QuizAttemptId    INT           NOT NULL,
        CertificateNumber NVARCHAR(40) NOT NULL,
        Title            NVARCHAR(160) NOT NULL,
        IssuedAt         DATETIME2     NOT NULL
            CONSTRAINT DF_Certificates_IssuedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_Certificates PRIMARY KEY (Id)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Certificates_CertificateNumber' AND object_id = OBJECT_ID(N'dbo.Certificates'))
    CREATE UNIQUE INDEX IX_Certificates_CertificateNumber ON dbo.Certificates (CertificateNumber);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Certificates_UserId' AND object_id = OBJECT_ID(N'dbo.Certificates'))
    CREATE INDEX IX_Certificates_UserId ON dbo.Certificates (UserId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Certificates_CourseId' AND object_id = OBJECT_ID(N'dbo.Certificates'))
    CREATE INDEX IX_Certificates_CourseId ON dbo.Certificates (CourseId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Certificates_QuizAttemptId' AND object_id = OBJECT_ID(N'dbo.Certificates'))
    CREATE INDEX IX_Certificates_QuizAttemptId ON dbo.Certificates (QuizAttemptId);
GO

/* ------------------------------------------------------- AdminAuditLogs -- */
IF OBJECT_ID(N'dbo.AdminAuditLogs', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AdminAuditLogs
    (
        Id          INT           IDENTITY(1,1) NOT NULL,
        UserId      INT           NOT NULL,
        [Action]    NVARCHAR(60)  NOT NULL,
        EntityType  NVARCHAR(80)  NOT NULL,
        EntityName  NVARCHAR(120) NULL,
        Description NVARCHAR(500) NOT NULL
            CONSTRAINT DF_AdminAuditLogs_Description DEFAULT (N''),
        CreatedAt   DATETIME2     NOT NULL
            CONSTRAINT DF_AdminAuditLogs_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_AdminAuditLogs PRIMARY KEY (Id),
        CONSTRAINT CK_AdminAuditLogs_Action CHECK ([Action] IN (N'Created', N'Updated', N'Deleted'))
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AdminAuditLogs_UserId_CreatedAt' AND object_id = OBJECT_ID(N'dbo.AdminAuditLogs'))
    CREATE INDEX IX_AdminAuditLogs_UserId_CreatedAt ON dbo.AdminAuditLogs (UserId, CreatedAt);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AdminAuditLogs_CreatedAt' AND object_id = OBJECT_ID(N'dbo.AdminAuditLogs'))
    CREATE INDEX IX_AdminAuditLogs_CreatedAt ON dbo.AdminAuditLogs (CreatedAt DESC);
GO

/* ============================================================================
   FOREIGN KEYS

   Delete behaviour follows ApplicationDbContext.cs:
     Cascade            - the entity is owned by its parent row
     SetNull            - the reference is advisory, the row survives

   SQL Server counts ON DELETE SET NULL as a cascade action and rejects any
   table that can be reached by more than one cascade path from a single DELETE
   (error 1785). The MySQL model has three such conflicts:

     Certificates  <- Users -> Certificates
                      Users -> QuizAttempts -> Certificates
                      Courses -> Certificates
                      Courses -> Quizzes -> QuizAttempts -> Certificates
     Lessons       <- Courses -> Lessons (CASCADE)
                      Courses -> CourseModules -> Lessons (SET NULL)
     Challenges    <- Courses -> Challenges (CASCADE)
                      Courses -> Lessons -> Challenges (SET NULL)

   Those three foreign keys (FK_Certificates_QuizAttempts,
   FK_Lessons_CourseModules, FK_Challenges_Lessons) are therefore declared
   WITHOUT an ON DELETE clause, i.e. RESTRICT / NO ACTION. Referential integrity
   is fully preserved - it simply has to be honoured in the right order, so the
   data layer performs the SET NULL / dependent delete explicitly inside one
   transaction:
     LessonRepository  - nulls Lessons.CourseModuleId before a module is deleted
     CourseRepository  - nulls Challenges.LessonId before a lesson is deleted
     CertificateRepository / QuizRepository / UserRepository / CourseRepository
                       - removes dependent certificates before their attempt,
                         user or course row.
   ============================================================================ */

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_CourseModules_Courses')
    ALTER TABLE dbo.CourseModules ADD CONSTRAINT FK_CourseModules_Courses
        FOREIGN KEY (CourseId) REFERENCES dbo.Courses (Id) ON DELETE CASCADE;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Lessons_Courses')
    ALTER TABLE dbo.Lessons ADD CONSTRAINT FK_Lessons_Courses
        FOREIGN KEY (CourseId) REFERENCES dbo.Courses (Id) ON DELETE CASCADE;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Lessons_CourseModules')
    ALTER TABLE dbo.Lessons ADD CONSTRAINT FK_Lessons_CourseModules
        FOREIGN KEY (CourseModuleId) REFERENCES dbo.CourseModules (Id);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Enrollments_Users')
    ALTER TABLE dbo.Enrollments ADD CONSTRAINT FK_Enrollments_Users
        FOREIGN KEY (UserId) REFERENCES dbo.Users (Id) ON DELETE CASCADE;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Enrollments_Courses')
    ALTER TABLE dbo.Enrollments ADD CONSTRAINT FK_Enrollments_Courses
        FOREIGN KEY (CourseId) REFERENCES dbo.Courses (Id) ON DELETE CASCADE;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Quizzes_Courses')
    ALTER TABLE dbo.Quizzes ADD CONSTRAINT FK_Quizzes_Courses
        FOREIGN KEY (CourseId) REFERENCES dbo.Courses (Id) ON DELETE CASCADE;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Questions_Quizzes')
    ALTER TABLE dbo.Questions ADD CONSTRAINT FK_Questions_Quizzes
        FOREIGN KEY (QuizId) REFERENCES dbo.Quizzes (Id) ON DELETE CASCADE;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_QuizAttempts_Users')
    ALTER TABLE dbo.QuizAttempts ADD CONSTRAINT FK_QuizAttempts_Users
        FOREIGN KEY (UserId) REFERENCES dbo.Users (Id) ON DELETE CASCADE;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_QuizAttempts_Quizzes')
    ALTER TABLE dbo.QuizAttempts ADD CONSTRAINT FK_QuizAttempts_Quizzes
        FOREIGN KEY (QuizId) REFERENCES dbo.Quizzes (Id) ON DELETE CASCADE;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Progress_Users')
    ALTER TABLE dbo.Progress ADD CONSTRAINT FK_Progress_Users
        FOREIGN KEY (UserId) REFERENCES dbo.Users (Id) ON DELETE CASCADE;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Progress_Lessons')
    ALTER TABLE dbo.Progress ADD CONSTRAINT FK_Progress_Lessons
        FOREIGN KEY (LessonId) REFERENCES dbo.Lessons (Id) ON DELETE CASCADE;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Resources_Courses')
    ALTER TABLE dbo.Resources ADD CONSTRAINT FK_Resources_Courses
        FOREIGN KEY (CourseId) REFERENCES dbo.Courses (Id) ON DELETE SET NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_UserActivities_Users')
    ALTER TABLE dbo.UserActivities ADD CONSTRAINT FK_UserActivities_Users
        FOREIGN KEY (UserId) REFERENCES dbo.Users (Id) ON DELETE CASCADE;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Notifications_Users')
    ALTER TABLE dbo.Notifications ADD CONSTRAINT FK_Notifications_Users
        FOREIGN KEY (UserId) REFERENCES dbo.Users (Id) ON DELETE CASCADE;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Challenges_Courses')
    ALTER TABLE dbo.Challenges ADD CONSTRAINT FK_Challenges_Courses
        FOREIGN KEY (CourseId) REFERENCES dbo.Courses (Id) ON DELETE CASCADE;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Challenges_Lessons')
    ALTER TABLE dbo.Challenges ADD CONSTRAINT FK_Challenges_Lessons
        FOREIGN KEY (LessonId) REFERENCES dbo.Lessons (Id);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_UserAchievements_Users')
    ALTER TABLE dbo.UserAchievements ADD CONSTRAINT FK_UserAchievements_Users
        FOREIGN KEY (UserId) REFERENCES dbo.Users (Id) ON DELETE CASCADE;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_UserAchievements_Achievements')
    ALTER TABLE dbo.UserAchievements ADD CONSTRAINT FK_UserAchievements_Achievements
        FOREIGN KEY (AchievementId) REFERENCES dbo.Achievements (Id) ON DELETE CASCADE;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Certificates_Users')
    ALTER TABLE dbo.Certificates ADD CONSTRAINT FK_Certificates_Users
        FOREIGN KEY (UserId) REFERENCES dbo.Users (Id) ON DELETE CASCADE;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Certificates_Courses')
    ALTER TABLE dbo.Certificates ADD CONSTRAINT FK_Certificates_Courses
        FOREIGN KEY (CourseId) REFERENCES dbo.Courses (Id) ON DELETE CASCADE;
GO

/* RESTRICT - see the note above. Deleting the attempt requires the certificate
   to be removed first, which the data layer does inside one transaction. */
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Certificates_QuizAttempts')
    ALTER TABLE dbo.Certificates ADD CONSTRAINT FK_Certificates_QuizAttempts
        FOREIGN KEY (QuizAttemptId) REFERENCES dbo.QuizAttempts (Id);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_AdminAuditLogs_Users')
    ALTER TABLE dbo.AdminAuditLogs ADD CONSTRAINT FK_AdminAuditLogs_Users
        FOREIGN KEY (UserId) REFERENCES dbo.Users (Id) ON DELETE CASCADE;
GO

/* Foreign keys created by this script are trusted immediately, so nothing has
   to be re-enabled in the normal case. If an earlier interrupted run left any
   constraint untrusted, re-validate and enable it now. The identifiers come
   from the catalog, never from application input. */
DECLARE @sql NVARCHAR(MAX) = N'';

SELECT @sql = @sql +
    N'ALTER TABLE ' + QUOTENAME(OBJECT_SCHEMA_NAME(fk.parent_object_id)) + N'.' +
                 QUOTENAME(OBJECT_NAME(fk.parent_object_id)) +
    N' WITH CHECK CHECK CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)
FROM sys.foreign_keys AS fk
WHERE fk.is_not_trusted = 1;

IF @sql <> N''
BEGIN
    EXEC sys.sp_executesql @sql;
END
GO
