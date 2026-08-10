/* Apply once to existing SQL Server ATLAS databases. The project does not use EF migrations. */
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.Enrollments') AND name = N'IX_Enrollments_CourseOfferingID')
    CREATE INDEX IX_Enrollments_CourseOfferingID ON dbo.Enrollments (CourseOfferingID);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.AssessmentModules') AND name = N'IX_AssessmentModules_CourseOfferingID')
    CREATE INDEX IX_AssessmentModules_CourseOfferingID ON dbo.AssessmentModules (CourseOfferingID);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.CourseOfferings') AND name = N'IX_CourseOfferings_SemesterID')
    CREATE INDEX IX_CourseOfferings_SemesterID ON dbo.CourseOfferings (SemesterID);
