using Microsoft.EntityFrameworkCore.Migrations;

public partial class AddDatabaseObjects : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            CREATE OR ALTER VIEW dbo.vw_CourseDetails
            AS
            SELECT ...
            """);

        migrationBuilder.Sql("""
            CREATE OR ALTER VIEW dbo.vw_SemesterStudentRanking
            AS
            SELECT ...
            """);

        migrationBuilder.Sql("""
            CREATE OR ALTER PROCEDURE dbo.sp_GetCourseAnalysis
                @CourseOfferingID INT
            AS
            BEGIN
                SELECT ...
                WHERE CourseOfferingID = @CourseOfferingID;
            END
            """);

        migrationBuilder.Sql("""
            IF OBJECT_ID(
                'dbo.TR_Students_GenerateCode',
                'TR') IS NULL
            EXEC('
                CREATE TRIGGER dbo.TR_Students_GenerateCode
                ON dbo.Students
                AFTER INSERT
                AS
                BEGIN
                    -- توليد StudentCode
                END
            ');
            """);
    }

    protected override void Down(
        MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            "DROP VIEW IF EXISTS dbo.vw_CourseDetails;");

        // احذف باقي الـ Views والـ Procedures والـ Trigger
    }
}