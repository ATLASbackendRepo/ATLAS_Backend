using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATLAsDCL.Migrations
{
    /// <inheritdoc />
    public partial class FixCourseDetailsDecimalPrecision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // MED-15: the view cast both grade columns to int, so a module worth
            // 7.50 marks was reported as 7 to the student. Both underlying
            // columns are decimal(5,2), so the view now keeps that precision all
            // the way to CourseofferingDetailsDTO.
            //
            // The DDL is wrapped in EXEC(...) because SQL Server requires CREATE
            // OR ALTER VIEW to be the first statement of its batch, and the
            // generated deployment script wraps every statement in an
            // IF NOT EXISTS history check.
            migrationBuilder.Sql("""
                EXEC(N'CREATE OR ALTER VIEW dbo.vw_CourseDetails
                AS
                SELECT
                    s.StudentId          AS studentId,
                    am.CourseOfferingID  AS CourseOfferingID,
                    c.C_Name             AS C_Name,
                    c.Credit_Hours       AS Credit_Hours,
                    am.ModuleID          AS ModuleId,
                    cg.StudentDegree     AS studentdegree,
                    am.MaxDegree         AS maxDegree,
                    am.ModuleName        AS ModuleName
                FROM dbo.CourseGrades cg
                INNER JOIN dbo.Enrollments e
                    ON e.EnrollmentID = cg.EnrollmentID
                INNER JOIN dbo.Students s
                    ON s.StudentId = e.StudentID
                INNER JOIN dbo.AssessmentModules am
                    ON am.ModuleID = cg.ModuleID
                INNER JOIN dbo.CourseOfferings co
                    ON co.CourseOfferingID = am.CourseOfferingID
                INNER JOIN dbo.Courses c
                    ON c.CourseID = co.CourseID;
                ');
                """);

            // MED-15: the distribution used inclusive integer ranges, so a total
            // of 10.5 fell into no bucket at all. The ranges are now half-open
            // ([0,10), [10,20), [20,30)) and the last bucket is open-ended, so
            // every student lands in exactly one bucket; a student with no grades
            // yet counts as zero.
            migrationBuilder.Sql("""
                EXEC(N'CREATE OR ALTER PROCEDURE dbo.sp_GetCourseAnalysis
                    @CourseOfferingID INT
                AS
                BEGIN
                    SET NOCOUNT ON;

                    -- 1) Average earned degree per assessment module.
                    SELECT
                        am.ModuleID,
                        am.ModuleName,
                        AVG(CAST(cg.StudentDegree AS float)) AS ModuleAvg,
                        CAST(am.MaxDegree AS float) AS MaxDegree
                    FROM dbo.AssessmentModules am
                    LEFT JOIN dbo.CourseGrades cg
                        ON cg.ModuleID = am.ModuleID
                    WHERE am.CourseOfferingID = @CourseOfferingID
                    GROUP BY am.ModuleID, am.ModuleName, am.MaxDegree;

                    -- 2) Grading progress per assessment module.
                    SELECT
                        am.ModuleID,
                        am.ModuleName,
                        COUNT(cg.CG_ID) AS GradedCount,
                        (SELECT COUNT(*)
                         FROM dbo.Enrollments e2
                         WHERE e2.CourseOfferingID = @CourseOfferingID) AS TotalStudents
                    FROM dbo.AssessmentModules am
                    LEFT JOIN dbo.CourseGrades cg
                        ON cg.ModuleID = am.ModuleID
                    WHERE am.CourseOfferingID = @CourseOfferingID
                    GROUP BY am.ModuleID, am.ModuleName;

                    -- 3) Distribution of total degrees over half-open ranges.
                    WITH StudentTotals AS
                    (
                        SELECT
                            e.StudentID,
                            SUM(cg.StudentDegree) AS TotalDegree
                        FROM dbo.Enrollments e
                        LEFT JOIN dbo.CourseGrades cg
                            ON cg.EnrollmentID = e.EnrollmentID
                        WHERE e.CourseOfferingID = @CourseOfferingID
                        GROUP BY e.StudentID
                    )
                    SELECT
                        ISNULL(SUM(CASE WHEN ISNULL(TotalDegree, 0) >= 0  AND ISNULL(TotalDegree, 0) < 10 THEN 1 ELSE 0 END), 0) AS Range0_10,
                        ISNULL(SUM(CASE WHEN ISNULL(TotalDegree, 0) >= 10 AND ISNULL(TotalDegree, 0) < 20 THEN 1 ELSE 0 END), 0) AS Range11_20,
                        ISNULL(SUM(CASE WHEN ISNULL(TotalDegree, 0) >= 20 AND ISNULL(TotalDegree, 0) < 30 THEN 1 ELSE 0 END), 0) AS Range21_30,
                        ISNULL(SUM(CASE WHEN ISNULL(TotalDegree, 0) >= 30 THEN 1 ELSE 0 END), 0) AS Range31_40,
                        AVG(CAST(TotalDegree AS float)) AS ClassAvg
                    FROM StudentTotals;
                END
                ');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Restores the definitions that truncated the degrees to int and left
            // gaps between the analysis buckets.
            migrationBuilder.Sql("""
                EXEC(N'CREATE OR ALTER VIEW dbo.vw_CourseDetails
                AS
                SELECT
                    s.StudentId          AS studentId,
                    am.CourseOfferingID  AS CourseOfferingID,
                    c.C_Name             AS C_Name,
                    c.Credit_Hours       AS Credit_Hours,
                    am.ModuleID          AS ModuleId,
                    CAST(cg.StudentDegree AS int) AS studentdegree,
                    CAST(am.MaxDegree    AS int) AS maxDegree,
                    am.ModuleName        AS ModuleName
                FROM dbo.CourseGrades cg
                INNER JOIN dbo.Enrollments e
                    ON e.EnrollmentID = cg.EnrollmentID
                INNER JOIN dbo.Students s
                    ON s.StudentId = e.StudentID
                INNER JOIN dbo.AssessmentModules am
                    ON am.ModuleID = cg.ModuleID
                INNER JOIN dbo.CourseOfferings co
                    ON co.CourseOfferingID = am.CourseOfferingID
                INNER JOIN dbo.Courses c
                    ON c.CourseID = co.CourseID;
                ');
                """);

            migrationBuilder.Sql("""
                EXEC(N'CREATE OR ALTER PROCEDURE dbo.sp_GetCourseAnalysis
                    @CourseOfferingID INT
                AS
                BEGIN
                    SET NOCOUNT ON;

                    SELECT
                        am.ModuleID,
                        am.ModuleName,
                        AVG(CAST(cg.StudentDegree AS float)) AS ModuleAvg,
                        CAST(am.MaxDegree AS float) AS MaxDegree
                    FROM dbo.AssessmentModules am
                    LEFT JOIN dbo.CourseGrades cg
                        ON cg.ModuleID = am.ModuleID
                    WHERE am.CourseOfferingID = @CourseOfferingID
                    GROUP BY am.ModuleID, am.ModuleName, am.MaxDegree;

                    SELECT
                        am.ModuleID,
                        am.ModuleName,
                        COUNT(cg.CG_ID) AS GradedCount,
                        (SELECT COUNT(*)
                         FROM dbo.Enrollments e2
                         WHERE e2.CourseOfferingID = @CourseOfferingID) AS TotalStudents
                    FROM dbo.AssessmentModules am
                    LEFT JOIN dbo.CourseGrades cg
                        ON cg.ModuleID = am.ModuleID
                    WHERE am.CourseOfferingID = @CourseOfferingID
                    GROUP BY am.ModuleID, am.ModuleName;

                    WITH StudentTotals AS
                    (
                        SELECT
                            e.StudentID,
                            SUM(cg.StudentDegree) AS TotalDegree
                        FROM dbo.Enrollments e
                        LEFT JOIN dbo.CourseGrades cg
                            ON cg.EnrollmentID = e.EnrollmentID
                        WHERE e.CourseOfferingID = @CourseOfferingID
                        GROUP BY e.StudentID
                    )
                    SELECT
                        ISNULL(SUM(CASE WHEN TotalDegree BETWEEN 0  AND 10 THEN 1 ELSE 0 END), 0) AS Range0_10,
                        ISNULL(SUM(CASE WHEN TotalDegree BETWEEN 11 AND 20 THEN 1 ELSE 0 END), 0) AS Range11_20,
                        ISNULL(SUM(CASE WHEN TotalDegree BETWEEN 21 AND 30 THEN 1 ELSE 0 END), 0) AS Range21_30,
                        ISNULL(SUM(CASE WHEN TotalDegree BETWEEN 31 AND 40 THEN 1 ELSE 0 END), 0) AS Range31_40,
                        AVG(CAST(TotalDegree AS float)) AS ClassAvg
                    FROM StudentTotals;
                END
                ');
                """);
        }
    }
}
