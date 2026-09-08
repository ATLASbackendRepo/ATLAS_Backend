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

