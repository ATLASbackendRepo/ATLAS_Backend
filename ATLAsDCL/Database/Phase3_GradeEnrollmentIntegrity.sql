/*
   Apply once to each existing SQL Server ATLAS database.
   This project has no EF Core migrations, so this script keeps the physical schema
   aligned with the EF model. It deliberately stops if existing duplicate data would
   make a unique constraint unsafe to create.
*/
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF EXISTS (
    SELECT 1
    FROM dbo.CourseGrades
    GROUP BY EnrollmentID, ModuleID
    HAVING COUNT(*) > 1)
    ;THROW 51000, 'Cannot add UX_CourseGrades_EnrollmentID_ModuleID: duplicate CourseGrades exist.', 1;

IF EXISTS (
    SELECT 1
    FROM dbo.Enrollments
    GROUP BY StudentID, CourseOfferingID
    HAVING COUNT(*) > 1)
    ;THROW 51001, 'Cannot add UX_Enrollments_StudentID_CourseOfferingID: duplicate Enrollments exist.', 1;

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.CourseGrades')
      AND name = N'UX_CourseGrades_EnrollmentID_ModuleID')
    CREATE UNIQUE INDEX UX_CourseGrades_EnrollmentID_ModuleID
        ON dbo.CourseGrades (EnrollmentID, ModuleID);

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.Enrollments')
      AND name = N'UX_Enrollments_StudentID_CourseOfferingID')
    CREATE UNIQUE INDEX UX_Enrollments_StudentID_CourseOfferingID
        ON dbo.Enrollments (StudentID, CourseOfferingID);

IF COL_LENGTH(N'dbo.CourseGrades', N'RowVersion') IS NULL
    ALTER TABLE dbo.CourseGrades ADD RowVersion rowversion NOT NULL;

IF COL_LENGTH(N'dbo.GradeAppealRequests', N'RowVersion') IS NULL
    ALTER TABLE dbo.GradeAppealRequests ADD RowVersion rowversion NOT NULL;

IF EXISTS (
    SELECT 1 FROM sys.check_constraints
    WHERE parent_object_id = OBJECT_ID(N'dbo.GradeAppealRequests')
      AND name = N'CK_GradeAppealRequests_Status')
    ALTER TABLE dbo.GradeAppealRequests DROP CONSTRAINT CK_GradeAppealRequests_Status;

ALTER TABLE dbo.GradeAppealRequests WITH CHECK ADD CONSTRAINT CK_GradeAppealRequests_Status
    CHECK ([Status] IN ('Pending', 'UnderReview', 'NeedMoreInfo', 'Approved', 'Rejected', 'Closed'));

COMMIT TRANSACTION;
