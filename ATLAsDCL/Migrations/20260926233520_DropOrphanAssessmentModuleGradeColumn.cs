using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATLAsDCL.Migrations
{
    /// <inheritdoc />
    public partial class DropOrphanAssessmentModuleGradeColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // MED-09: the grade of an assessment module used to be modelled as one
            // CourseGrade per module (AssessmentModules.CourseGradeCG_ID). The model
            // later moved to one CourseGrade per enrollment and module, and the
            // column and its indexes were trimmed from the model by hand, so no
            // migration ever removed them from a database that was created earlier.
            // Every object is dropped only when it is still present.
            //
            // The index must go first: SQL Server refuses to drop a column that an
            // index still depends on (error 5074).
            migrationBuilder.Sql("""
                IF EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = N'IX_AssessmentModules_CourseGradeCG_ID'
                      AND object_id = OBJECT_ID(N'dbo.AssessmentModules'))
                    DROP INDEX IX_AssessmentModules_CourseGradeCG_ID ON dbo.AssessmentModules;

                IF EXISTS (
                    SELECT 1
                    FROM sys.columns
                    WHERE object_id = OBJECT_ID(N'dbo.AssessmentModules')
                      AND name = N'CourseGradeCG_ID')
                BEGIN
                    DECLARE @foreignKey sysname;

                    SELECT TOP (1) @foreignKey = fk.name
                    FROM sys.foreign_keys fk
                    INNER JOIN sys.foreign_key_columns fkc
                        ON fkc.constraint_object_id = fk.object_id
                    INNER JOIN sys.columns c
                        ON c.object_id = fkc.parent_object_id
                       AND c.column_id = fkc.parent_column_id
                    WHERE fk.parent_object_id = OBJECT_ID(N'dbo.AssessmentModules')
                      AND c.name = N'CourseGradeCG_ID';

                    IF @foreignKey IS NOT NULL
                    BEGIN
                        DECLARE @dropForeignKey nvarchar(300) =
                            N'ALTER TABLE dbo.AssessmentModules DROP CONSTRAINT ' + QUOTENAME(@foreignKey);

                        EXEC sp_executesql @dropForeignKey;
                    END

                    ALTER TABLE dbo.AssessmentModules DROP COLUMN CourseGradeCG_ID;
                END

                -- Redundant: UX_CourseGrades_EnrollmentID_ModuleID already starts
                -- with EnrollmentID.
                IF EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = N'IX_CourseGrades_EnrollmentID'
                      AND object_id = OBJECT_ID(N'dbo.CourseGrades'))
                    DROP INDEX IX_CourseGrades_EnrollmentID ON dbo.CourseGrades;

                -- Redundant: UX_Enrollments_StudentID_CourseOfferingID already
                -- starts with StudentID.
                IF EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = N'IX_Enrollments_StudentID'
                      AND object_id = OBJECT_ID(N'dbo.Enrollments'))
                    DROP INDEX IX_Enrollments_StudentID ON dbo.Enrollments;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // The dropped column and indexes belong to a model the application no
            // longer has, so there is nothing meaningful to restore. Recreating
            // them would only put the dead schema back.
            migrationBuilder.Sql("""
                IF NOT EXISTS (
                    SELECT 1
                    FROM sys.columns
                    WHERE object_id = OBJECT_ID(N'dbo.AssessmentModules')
                      AND name = N'CourseGradeCG_ID')
                BEGIN
                    ALTER TABLE dbo.AssessmentModules ADD CourseGradeCG_ID int NULL;

                    CREATE INDEX IX_AssessmentModules_CourseGradeCG_ID
                        ON dbo.AssessmentModules (CourseGradeCG_ID);
                END

                IF NOT EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = N'IX_CourseGrades_EnrollmentID'
                      AND object_id = OBJECT_ID(N'dbo.CourseGrades'))
                    CREATE INDEX IX_CourseGrades_EnrollmentID ON dbo.CourseGrades (EnrollmentID);

                IF NOT EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = N'IX_Enrollments_StudentID'
                      AND object_id = OBJECT_ID(N'dbo.Enrollments'))
                    CREATE INDEX IX_Enrollments_StudentID ON dbo.Enrollments (StudentID);
                """);
        }
    }
}
