IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [AuditLogs] (
    [Id] bigint NOT NULL IDENTITY,
    [UserId] int NULL,
    [Action] nvarchar(150) NOT NULL,
    [EntityType] nvarchar(100) NULL,
    [EntityId] nvarchar(100) NULL,
    [OldValues] nvarchar(max) NULL,
    [NewValues] nvarchar(max) NULL,
    [IpAddress] nvarchar(45) NULL,
    [CorrelationId] nvarchar(100) NULL,
    [Success] bit NOT NULL,
    [TimestampUtc] datetime2 NOT NULL,
    CONSTRAINT [PK_AuditLogs] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Courses] (
    [CourseID] int NOT NULL IDENTITY,
    [C_Name] nvarchar(100) NOT NULL,
    [Credit_Hours] int NOT NULL,
    [IsPractical] bit NOT NULL,
    CONSTRAINT [PK_Courses] PRIMARY KEY ([CourseID])
);
GO

CREATE TABLE [Institutions] (
    [InstitutionID] int NOT NULL IDENTITY,
    [name] nvarchar(100) NOT NULL,
    CONSTRAINT [PK_Institutions] PRIMARY KEY ([InstitutionID])
);
GO

CREATE TABLE [Semesters] (
    [SemesterID] int NOT NULL IDENTITY,
    [SemesterName] nvarchar(100) NOT NULL,
    [AcademicYear] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_Semesters] PRIMARY KEY ([SemesterID])
);
GO

CREATE TABLE [Departments] (
    [DepartmentId] int NOT NULL IDENTITY,
    [D_Name] nvarchar(100) NOT NULL,
    [InstitutionID] int NOT NULL,
    CONSTRAINT [PK_Departments] PRIMARY KEY ([DepartmentId]),
    CONSTRAINT [FK_Departments_Institutions_InstitutionID] FOREIGN KEY ([InstitutionID]) REFERENCES [Institutions] ([InstitutionID]) ON DELETE CASCADE
);
GO

CREATE TABLE [CourseOfferings] (
    [CourseOfferingID] int NOT NULL IDENTITY,
    [SemesterID] int NOT NULL,
    [CourseID] int NOT NULL,
    CONSTRAINT [PK_CourseOfferings] PRIMARY KEY ([CourseOfferingID]),
    CONSTRAINT [FK_CourseOfferings_Courses_CourseID] FOREIGN KEY ([CourseID]) REFERENCES [Courses] ([CourseID]) ON DELETE NO ACTION,
    CONSTRAINT [FK_CourseOfferings_Semesters_SemesterID] FOREIGN KEY ([SemesterID]) REFERENCES [Semesters] ([SemesterID]) ON DELETE NO ACTION
);
GO

CREATE TABLE [Users] (
    [UserId] int NOT NULL IDENTITY,
    [FullName] nvarchar(200) NOT NULL,
    [Password] nvarchar(255) NOT NULL,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETDATE()),
    [Role] nvarchar(50) NOT NULL DEFAULT N'Student',
    [AcademicId] nvarchar(max) NOT NULL,
    [InstitutionID] int NOT NULL,
    [DepartmentId] int NOT NULL,
    [RefreshTokenHash] nvarchar(max) NULL,
    [RefreshTokenExpiresAt] datetime2 NULL,
    [RefreshTokenRevokedAt] datetime2 NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([UserId]),
    CONSTRAINT [CK__Users__Role] CHECK ([Role] IN ('Student', 'Admin', 'SuperAdmin', 'Doctor', 'TeachingAssistant')),
    CONSTRAINT [FK_User_Insutition] FOREIGN KEY ([InstitutionID]) REFERENCES [Institutions] ([InstitutionID]) ON DELETE CASCADE,
    CONSTRAINT [FK_Users_Departments] FOREIGN KEY ([DepartmentId]) REFERENCES [Departments] ([DepartmentId]) ON DELETE CASCADE
);
GO

CREATE TABLE [CourseGroupMessages] (
    [MessageID] int NOT NULL IDENTITY,
    [CourseOfferingID] int NOT NULL,
    [SenderUserID] int NOT NULL,
    [Content] nvarchar(max) NOT NULL,
    [SentAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    [IsPinned] bit NOT NULL DEFAULT CAST(0 AS bit),
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    CONSTRAINT [PK_CourseGroupMessages] PRIMARY KEY ([MessageID]),
    CONSTRAINT [FK_CourseGroupMessages_CourseOfferings_CourseOfferingID] FOREIGN KEY ([CourseOfferingID]) REFERENCES [CourseOfferings] ([CourseOfferingID]) ON DELETE CASCADE,
    CONSTRAINT [FK_CourseGroupMessages_Users_SenderUserID] FOREIGN KEY ([SenderUserID]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION
);
GO

CREATE TABLE [CourseOfferingDoctors] (
    [CourseOfferingDoctorID] int NOT NULL IDENTITY,
    [CourseOfferingID] int NOT NULL,
    [DoctorID] int NOT NULL,
    CONSTRAINT [PK_CourseOfferingDoctors] PRIMARY KEY ([CourseOfferingDoctorID]),
    CONSTRAINT [FK_CourseOfferingDoctors_CourseOfferings_CourseOfferingID] FOREIGN KEY ([CourseOfferingID]) REFERENCES [CourseOfferings] ([CourseOfferingID]) ON DELETE NO ACTION,
    CONSTRAINT [FK_CourseOfferingDoctors_Users_DoctorID] FOREIGN KEY ([DoctorID]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION
);
GO

CREATE TABLE [CourseOfferingTAs] (
    [CourseOfferingTAID] int NOT NULL IDENTITY,
    [CourseOfferingID] int NOT NULL,
    [TeacherAssistID] int NOT NULL,
    CONSTRAINT [PK_CourseOfferingTAs] PRIMARY KEY ([CourseOfferingTAID]),
    CONSTRAINT [FK_CourseOfferingTAs_CourseOfferings_CourseOfferingID] FOREIGN KEY ([CourseOfferingID]) REFERENCES [CourseOfferings] ([CourseOfferingID]) ON DELETE NO ACTION,
    CONSTRAINT [FK_CourseOfferingTAs_Users_TeacherAssistID] FOREIGN KEY ([TeacherAssistID]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION
);
GO

CREATE TABLE [StaffProfiles] (
    [StaffProfileID] int NOT NULL IDENTITY,
    [UserID] int NOT NULL,
    [AcademicId] nvarchar(max) NOT NULL,
    [Role] nvarchar(50) NOT NULL,
    [DepartmentID] int NULL,
    [TeachingYears] nvarchar(100) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_StaffProfiles] PRIMARY KEY ([StaffProfileID]),
    CONSTRAINT [FK_StaffProfiles_Departments_DepartmentID] FOREIGN KEY ([DepartmentID]) REFERENCES [Departments] ([DepartmentId]) ON DELETE CASCADE,
    CONSTRAINT [FK_StaffProfiles_Users_UserID] FOREIGN KEY ([UserID]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE
);
GO

CREATE TABLE [Students] (
    [StudentId] int NOT NULL IDENTITY,
    [StudentCode] nvarchar(20) NOT NULL,
    [UserId] int NOT NULL,
    [AcademicLevel] int NOT NULL,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [AcademicId] nvarchar(max) NOT NULL,
    [InstitutionID] int NOT NULL,
    [DepartmentId] int NOT NULL,
    CONSTRAINT [PK_Students] PRIMARY KEY ([StudentId]),
    CONSTRAINT [FK_Students_Departments_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [Departments] ([DepartmentId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Students_Institutions_InstitutionID] FOREIGN KEY ([InstitutionID]) REFERENCES [Institutions] ([InstitutionID]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Students_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION
);
GO

CREATE TABLE [CourseGroupMessageReads] (
    [MessageID] int NOT NULL,
    [UserID] int NOT NULL,
    [ReadAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    CONSTRAINT [PK_CourseGroupMessageReads] PRIMARY KEY ([MessageID], [UserID]),
    CONSTRAINT [FK_CourseGroupMessageReads_CourseGroupMessages_MessageID] FOREIGN KEY ([MessageID]) REFERENCES [CourseGroupMessages] ([MessageID]) ON DELETE CASCADE,
    CONSTRAINT [FK_CourseGroupMessageReads_Users_UserID] FOREIGN KEY ([UserID]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE
);
GO

CREATE TABLE [Notifications] (
    [NotificationID] int NOT NULL IDENTITY,
    [RecipientUserID] int NOT NULL,
    [Type] nvarchar(50) NOT NULL,
    [Title] nvarchar(200) NOT NULL,
    [Body] nvarchar(500) NOT NULL,
    [RelatedMessageID] int NULL,
    [IsRead] bit NOT NULL DEFAULT CAST(0 AS bit),
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    CONSTRAINT [PK_Notifications] PRIMARY KEY ([NotificationID]),
    CONSTRAINT [FK_Notifications_CourseGroupMessages_RelatedMessageID] FOREIGN KEY ([RelatedMessageID]) REFERENCES [CourseGroupMessages] ([MessageID]) ON DELETE SET NULL,
    CONSTRAINT [FK_Notifications_Users_RecipientUserID] FOREIGN KEY ([RecipientUserID]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION
);
GO

CREATE TABLE [Enrollments] (
    [EnrollmentID] int NOT NULL IDENTITY,
    [StudentID] int NOT NULL,
    [CreatedByUserID] int NOT NULL,
    [EnrollmentDate] datetime2 NOT NULL,
    [CourseOfferingID] int NOT NULL,
    CONSTRAINT [PK_Enrollments] PRIMARY KEY ([EnrollmentID]),
    CONSTRAINT [FK_Enrollments_CourseOfferings_CourseOfferingID] FOREIGN KEY ([CourseOfferingID]) REFERENCES [CourseOfferings] ([CourseOfferingID]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Enrollments_Students_StudentID] FOREIGN KEY ([StudentID]) REFERENCES [Students] ([StudentId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Enrollments_Users_CreatedByUserID] FOREIGN KEY ([CreatedByUserID]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION
);
GO

CREATE TABLE [StudentProfiles] (
    [StudentProfileID] int NOT NULL IDENTITY,
    [StudentID] int NOT NULL,
    [LinkedInUrl] nvarchar(300) NULL,
    [SectionNumber] int NULL,
    [Faculty] nvarchar(max) NOT NULL,
    [QRCodeUrl] nvarchar(300) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_StudentProfiles] PRIMARY KEY ([StudentProfileID]),
    CONSTRAINT [FK_StudentProfiles_Students_StudentID] FOREIGN KEY ([StudentID]) REFERENCES [Students] ([StudentId]) ON DELETE CASCADE
);
GO

CREATE TABLE [AssessmentModules] (
    [ModuleID] int NOT NULL IDENTITY,
    [CourseOfferingID] int NOT NULL,
    [ModuleName] nvarchar(150) NOT NULL,
    [MaxDegree] decimal(5,2) NOT NULL,
    [CourseGradeCG_ID] int NULL,
    CONSTRAINT [PK_AssessmentModules] PRIMARY KEY ([ModuleID]),
    CONSTRAINT [FK_AssessmentModules_CourseOfferings_CourseOfferingID] FOREIGN KEY ([CourseOfferingID]) REFERENCES [CourseOfferings] ([CourseOfferingID]) ON DELETE NO ACTION
);
GO

CREATE TABLE [CourseGrades] (
    [CG_ID] int NOT NULL IDENTITY,
    [EnrollmentID] int NOT NULL,
    [ModuleID] int NOT NULL,
    [StudentDegree] decimal(5,2) NOT NULL,
    [CreatedBy] int NOT NULL,
    [UpdatedBy] int NULL,
    CONSTRAINT [PK_CourseGrades] PRIMARY KEY ([CG_ID]),
    CONSTRAINT [FK_CourseGrades_AssessmentModules_ModuleID] FOREIGN KEY ([ModuleID]) REFERENCES [AssessmentModules] ([ModuleID]) ON DELETE NO ACTION,
    CONSTRAINT [FK_CourseGrades_Enrollments_EnrollmentID] FOREIGN KEY ([EnrollmentID]) REFERENCES [Enrollments] ([EnrollmentID]) ON DELETE NO ACTION
);
GO

CREATE TABLE [GradeAppealRequests] (
    [RequestID] int NOT NULL IDENTITY,
    [StudentID] int NOT NULL,
    [CourseOfferingID] int NOT NULL,
    [AssessmentModuleID] int NULL,
    [DoctorUserID] int NOT NULL,
    [CurrentGrade] decimal(5,2) NULL,
    [ProblemDescription] nvarchar(max) NOT NULL,
    [Status] nvarchar(20) NOT NULL DEFAULT N'Pending',
    [DoctorResponse] nvarchar(max) NULL,
    [SubmittedAt] datetime2 NOT NULL DEFAULT (GETDATE()),
    [ReviewedAt] datetime2 NULL,
    CONSTRAINT [PK_GradeAppealRequests] PRIMARY KEY ([RequestID]),
    CONSTRAINT [CK_GradeAppealRequests_Status] CHECK ([Status] IN ('Pending', 'UnderReview', 'NeedMoreInfo', 'Approved', 'Rejected', 'Closed')),
    CONSTRAINT [FK_GradeAppealRequests_AssessmentModules] FOREIGN KEY ([AssessmentModuleID]) REFERENCES [AssessmentModules] ([ModuleID]) ON DELETE NO ACTION,
    CONSTRAINT [FK_GradeAppealRequests_CourseOfferings] FOREIGN KEY ([CourseOfferingID]) REFERENCES [CourseOfferings] ([CourseOfferingID]) ON DELETE NO ACTION,
    CONSTRAINT [FK_GradeAppealRequests_Students] FOREIGN KEY ([StudentID]) REFERENCES [Students] ([StudentId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_GradeAppealRequests_Users] FOREIGN KEY ([DoctorUserID]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION
);
GO

CREATE INDEX [IX_AssessmentModules_CourseGradeCG_ID] ON [AssessmentModules] ([CourseGradeCG_ID]);
GO

CREATE INDEX [IX_AssessmentModules_CourseOfferingID] ON [AssessmentModules] ([CourseOfferingID]);
GO

CREATE INDEX [IX_CourseGrades_EnrollmentID] ON [CourseGrades] ([EnrollmentID]);
GO

CREATE INDEX [IX_CourseGrades_ModuleID] ON [CourseGrades] ([ModuleID]);
GO

CREATE INDEX [IX_CourseGroupMessageReads_UserID] ON [CourseGroupMessageReads] ([UserID]);
GO

CREATE INDEX [IX_CourseGroupMessages_CourseOfferingID] ON [CourseGroupMessages] ([CourseOfferingID]);
GO

CREATE INDEX [IX_CourseGroupMessages_SenderUserID] ON [CourseGroupMessages] ([SenderUserID]);
GO

CREATE INDEX [IX_CourseOfferingDoctors_CourseOfferingID] ON [CourseOfferingDoctors] ([CourseOfferingID]);
GO

CREATE INDEX [IX_CourseOfferingDoctors_DoctorID] ON [CourseOfferingDoctors] ([DoctorID]);
GO

CREATE INDEX [IX_CourseOfferings_CourseID] ON [CourseOfferings] ([CourseID]);
GO

CREATE INDEX [UQ_Course_Semester] ON [CourseOfferings] ([SemesterID]);
GO

CREATE INDEX [IX_CourseOfferingTAs_CourseOfferingID] ON [CourseOfferingTAs] ([CourseOfferingID]);
GO

CREATE INDEX [IX_CourseOfferingTAs_TeacherAssistID] ON [CourseOfferingTAs] ([TeacherAssistID]);
GO

CREATE INDEX [IX_Departments_InstitutionID] ON [Departments] ([InstitutionID]);
GO

CREATE INDEX [IX_Enrollments_CourseOfferingID] ON [Enrollments] ([CourseOfferingID]);
GO

CREATE INDEX [IX_Enrollments_CreatedByUserID] ON [Enrollments] ([CreatedByUserID]);
GO

CREATE INDEX [IX_Enrollments_StudentID] ON [Enrollments] ([StudentID]);
GO

CREATE INDEX [IX_GradeAppealRequests_AssessmentModuleID] ON [GradeAppealRequests] ([AssessmentModuleID]);
GO

CREATE INDEX [IX_GradeAppealRequests_CourseOfferingID] ON [GradeAppealRequests] ([CourseOfferingID]);
GO

CREATE INDEX [IX_GradeAppealRequests_DoctorUserID] ON [GradeAppealRequests] ([DoctorUserID]);
GO

CREATE INDEX [IX_GradeAppealRequests_StudentID] ON [GradeAppealRequests] ([StudentID]);
GO

CREATE INDEX [IX_Notifications_RecipientUserID] ON [Notifications] ([RecipientUserID]);
GO

CREATE INDEX [IX_Notifications_RelatedMessageID] ON [Notifications] ([RelatedMessageID]);
GO

CREATE UNIQUE INDEX [UQ_Semester_Name_AcademicYear] ON [Semesters] ([SemesterName], [AcademicYear]);
GO

CREATE INDEX [IX_StaffProfiles_DepartmentID] ON [StaffProfiles] ([DepartmentID]);
GO

CREATE UNIQUE INDEX [IX_StaffProfiles_UserID] ON [StaffProfiles] ([UserID]);
GO

CREATE UNIQUE INDEX [IX_StudentProfiles_StudentID] ON [StudentProfiles] ([StudentID]);
GO

CREATE INDEX [IX_Students_DepartmentId] ON [Students] ([DepartmentId]);
GO

CREATE INDEX [IX_Students_InstitutionID] ON [Students] ([InstitutionID]);
GO

CREATE UNIQUE INDEX [IX_Students_StudentCode] ON [Students] ([StudentCode]);
GO

CREATE UNIQUE INDEX [IX_Students_UserId] ON [Students] ([UserId]);
GO

CREATE INDEX [IX_Users_DepartmentId] ON [Users] ([DepartmentId]);
GO

CREATE INDEX [IX_Users_InstitutionID] ON [Users] ([InstitutionID]);
GO

ALTER TABLE [AssessmentModules] ADD CONSTRAINT [FK_AssessmentModules_CourseGrades_CourseGradeCG_ID] FOREIGN KEY ([CourseGradeCG_ID]) REFERENCES [CourseGrades] ([CG_ID]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260828185129_InitialCreate', N'8.0.10');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [SecurityEvents] (
    [Id] bigint NOT NULL IDENTITY,
    [EventType] nvarchar(max) NOT NULL,
    [UserId] int NULL,
    [AttemptedAcademicId] nvarchar(max) NULL,
    [Severity] nvarchar(max) NOT NULL,
    [IpAddress] nvarchar(max) NULL,
    [Description] nvarchar(max) NULL,
    [CorrelationId] nvarchar(max) NULL,
    [TimestampUtc] datetime2 NOT NULL,
    [IsAlerted] bit NOT NULL,
    [AlertedAtUtc] datetime2 NULL,
    CONSTRAINT [PK_SecurityEvents] PRIMARY KEY ([Id])
);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260829155120_AddSecurityEventsTable', N'8.0.10');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DROP INDEX [IX_StaffProfiles_UserID] ON [StaffProfiles];
GO

DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'Role');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT [' + @var0 + '];');
GO

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[StaffProfiles]') AND [c].[name] = N'UserID');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [StaffProfiles] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [StaffProfiles] ALTER COLUMN [UserID] int NULL;
GO

DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[StaffProfiles]') AND [c].[name] = N'Role');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [StaffProfiles] DROP CONSTRAINT [' + @var2 + '];');
ALTER TABLE [StaffProfiles] ALTER COLUMN [Role] nvarchar(50) NULL;
GO

DECLARE @var3 sysname;
SELECT @var3 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[StaffProfiles]') AND [c].[name] = N'AcademicId');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [StaffProfiles] DROP CONSTRAINT [' + @var3 + '];');
ALTER TABLE [StaffProfiles] ALTER COLUMN [AcademicId] nvarchar(max) NULL;
GO

CREATE UNIQUE INDEX [IX_StaffProfiles_UserID] ON [StaffProfiles] ([UserID]) WHERE [UserID] IS NOT NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260907233338_FixAssessmentModuleCourseGradeForeignKey', N'8.0.10');
GO

COMMIT;
GO

