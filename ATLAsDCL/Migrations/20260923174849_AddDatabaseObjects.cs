using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATLAsDCL.Migrations
{
    /// <summary>
    /// Creates the read models consumed by the application through the keyless
    /// view entities (<c>vw_SemesterStudentRanking</c>, <c>vw_StudentGradeMode</c>,
    /// <c>vw_CourseDetails</c>), the <c>sp_GetCourseAnalysis</c> stored procedure
    /// used by the course analysis feature, and the <c>TR_Students_GenerateCode</c>
    /// trigger declared on the Students table configuration.
    /// </summary>
    public partial class AddDatabaseObjects : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Ranking read model: one row per student per semester with the sum of
            // earned degrees and a dense rank inside the student's academic level.
            // Consumed by EnrollmentRepository.GetStudentSemesterRankAsync and
            // GetTopRankedStudentsInSemesterAsync (SemesterStudentRankingView).
            //
            // Wrapped in EXEC(...) because a CREATE/ALTER of a view or procedure
            // must start its own batch, and the generated deployment script puts
            // every statement inside an IF NOT EXISTS history check.
            migrationBuilder.Sql("""
                EXEC(N'CREATE OR ALTER VIEW dbo.vw_SemesterStudentRanking
                AS
                WITH StudentTotals AS
                (
                    SELECT
                        s.StudentId          AS StudentID,
                        co.SemesterID         AS SemesterID,
                        s.AcademicLevel      AS AcademicLevel,
                        s.StudentCode        AS StudentCode,
                        u.FullName           AS FullName,
                        s.DepartmentId       AS DepartmentID,
                        SUM(cg.StudentDegree) AS TotalDegree
                    FROM dbo.Enrollments e
                    INNER JOIN dbo.Students s
                        ON s.StudentId = e.StudentID
                    INNER JOIN dbo.Users u
                        ON u.UserId = s.UserId
                    INNER JOIN dbo.CourseOfferings co
                        ON co.CourseOfferingID = e.CourseOfferingID
                    LEFT JOIN dbo.CourseGrades cg
                        ON cg.EnrollmentID = e.EnrollmentID
                    GROUP BY
                        s.StudentId,
                        co.SemesterID,
                        s.AcademicLevel,
                        s.StudentCode,
                        u.FullName,
                        s.DepartmentId
                )
                SELECT
                    StudentID,
                    SemesterID,
                    AcademicLevel,
                    StudentCode,
                    FullName,
                    DepartmentID,
                    CAST(ISNULL(TotalDegree, 0) AS decimal(18, 2)) AS TotalDegree,
                    CAST(DENSE_RANK() OVER (
                        PARTITION BY SemesterID, AcademicLevel
                        ORDER BY TotalDegree DESC) AS bigint) AS [Rank]
                FROM StudentTotals;
                ');
                """);

            // Per-module grade read model for a course offering.
            // Consumed by StudentRepository.GetAllStudentWithModeAsync and
            // GetStudentWithModeAsync (StudentGradeModelView).
            migrationBuilder.Sql("""
                EXEC(N'CREATE OR ALTER VIEW dbo.vw_StudentGradeMode
                AS
                SELECT
                    am.ModuleID          AS ModuleID,
                    am.CourseOfferingID  AS CourseOfferingID,
                    s.StudentCode        AS StudentCode,
                    s.AcademicId         AS AcademicID,
                    u.FullName           AS FullName,
                    CAST(cg.StudentDegree AS decimal(18, 2)) AS StudentDegree
                FROM dbo.CourseGrades cg
                INNER JOIN dbo.Enrollments e
                    ON e.EnrollmentID = cg.EnrollmentID
                INNER JOIN dbo.Students s
                    ON s.StudentId = e.StudentID
                INNER JOIN dbo.Users u
                    ON u.UserId = s.UserId
                INNER JOIN dbo.AssessmentModules am
                    ON am.ModuleID = cg.ModuleID;
                ');
                """);

            // Student-facing course details read model.
            // Consumed by CourseOfferingRepository.GetDetailsCourseAsync and
            // GetMyCourseDetailsAsync (CourseDetailsView).
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

            // Course analysis read model. Returns three result sets consumed by
            // CourseOfferingRepository.GetCourseAnalysisAsync through Dapper:
            // module averages, grading progress and the degree distribution.
            // EXEC wrapper: CREATE PROCEDURE must start its own batch (see above).
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

                    -- 3) Distribution of total degrees over fixed ranges.
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

            // Safety net for inserts that explicitly supply NULL/'' StudentCode:
            // assigns the next sequence-backed code. Inserts that omit the column
            // already receive a code from the default constraint created in the
            // ProductionHardening migration. CREATE TRIGGER must be the first
            // statement of its batch, hence the EXEC wrapper.
            migrationBuilder.Sql("""
                IF OBJECT_ID('dbo.TR_Students_GenerateCode', 'TR') IS NULL
                EXEC('
                    CREATE TRIGGER dbo.TR_Students_GenerateCode
                    ON dbo.Students
                    AFTER INSERT
                    AS
                    BEGIN
                        SET NOCOUNT ON;
                        UPDATE s
                        SET s.StudentCode =
                            ''S'' + RIGHT(''00000000'' + CAST(NEXT VALUE FOR dbo.Seq_StudentCode AS nvarchar(10)), 8)
                        FROM dbo.Students s
                        INNER JOIN inserted i
                            ON i.StudentId = s.StudentId
                        WHERE i.StudentCode IS NULL OR i.StudentCode = N'''';
                    END
                ');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW IF EXISTS dbo.vw_CourseDetails;");
            migrationBuilder.Sql("DROP VIEW IF EXISTS dbo.vw_SemesterStudentRanking;");
            migrationBuilder.Sql("DROP VIEW IF EXISTS dbo.vw_StudentGradeMode;");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS dbo.sp_GetCourseAnalysis;");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS dbo.TR_Students_GenerateCode;");
        }
    }
}
