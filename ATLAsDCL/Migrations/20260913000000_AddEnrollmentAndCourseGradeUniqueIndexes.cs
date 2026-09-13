using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATLAsDCL.Migrations;

public partial class AddEnrollmentAndCourseGradeUniqueIndexes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateIndex(
            name: "UX_CourseGrades_EnrollmentID_ModuleID",
            table: "CourseGrades",
            columns: new[] { "EnrollmentID", "ModuleID" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "UX_Enrollments_StudentID_CourseOfferingID",
            table: "Enrollments",
            columns: new[] { "StudentID", "CourseOfferingID" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "UX_CourseGrades_EnrollmentID_ModuleID", table: "CourseGrades");
        migrationBuilder.DropIndex(name: "UX_Enrollments_StudentID_CourseOfferingID", table: "Enrollments");
    }
}
