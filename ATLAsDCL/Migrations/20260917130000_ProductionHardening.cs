using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATLAsDCL.Migrations;

public partial class ProductionHardening : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "MustChangePassword",
            table: "Users",
            type: "bit",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<string>(
            name: "PreviousRefreshTokenHash",
            table: "Users",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.DropCheckConstraint(name: "CK__Users__Role", table: "Users");
        migrationBuilder.AddCheckConstraint(
            name: "CK__Users__Role",
            table: "Users",
            sql: "[Role] IN ('Student', 'Admin', 'SAdmin', 'Doctor', 'TeachingAssistant')");

        migrationBuilder.DropForeignKey(name: "FK_User_Insutition", table: "Users");
        migrationBuilder.DropForeignKey(name: "FK_Users_Departments", table: "Users");
        migrationBuilder.AddForeignKey(
            name: "FK_User_Insutition", table: "Users", column: "InstitutionID",
            principalTable: "Institutions", principalColumn: "InstitutionID",
            onDelete: ReferentialAction.Restrict);
        migrationBuilder.AddForeignKey(
            name: "FK_Users_Departments", table: "Users", column: "DepartmentId",
            principalTable: "Departments", principalColumn: "DepartmentId",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.CreateIndex(
            name: "UX_CourseOfferingDoctors_Offering_Doctor",
            table: "CourseOfferingDoctors",
            columns: new[] { "CourseOfferingID", "DoctorID" }, unique: true);
        migrationBuilder.CreateIndex(
            name: "UX_CourseOfferingTAs_Offering_TA",
            table: "CourseOfferingTAs",
            columns: new[] { "CourseOfferingID", "TeacherAssistID" }, unique: true);
        migrationBuilder.CreateIndex(
            name: "IX_Enrollments_CourseOfferingID_StudentID",
            table: "Enrollments",
            columns: new[] { "CourseOfferingID", "StudentID" });
        migrationBuilder.CreateIndex(
            name: "IX_CourseGrades_ModuleID_EnrollmentID",
            table: "CourseGrades",
            columns: new[] { "ModuleID", "EnrollmentID" });
        migrationBuilder.CreateIndex(
            name: "IX_SecurityEvents_EventType_AcademicId_Timestamp",
            table: "SecurityEvents",
            columns: new[] { "EventType", "AttemptedAcademicId", "TimestampUtc" });
        migrationBuilder.CreateIndex(
            name: "IX_SecurityEvents_IsAlerted_Severity_Timestamp",
            table: "SecurityEvents",
            columns: new[] { "IsAlerted", "Severity", "TimestampUtc" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "UX_CourseOfferingDoctors_Offering_Doctor", table: "CourseOfferingDoctors");
        migrationBuilder.DropIndex(name: "UX_CourseOfferingTAs_Offering_TA", table: "CourseOfferingTAs");
        migrationBuilder.DropIndex(name: "IX_Enrollments_CourseOfferingID_StudentID", table: "Enrollments");
        migrationBuilder.DropIndex(name: "IX_CourseGrades_ModuleID_EnrollmentID", table: "CourseGrades");
        migrationBuilder.DropIndex(name: "IX_SecurityEvents_EventType_AcademicId_Timestamp", table: "SecurityEvents");
        migrationBuilder.DropIndex(name: "IX_SecurityEvents_IsAlerted_Severity_Timestamp", table: "SecurityEvents");
        migrationBuilder.DropForeignKey(name: "FK_User_Insutition", table: "Users");
        migrationBuilder.DropForeignKey(name: "FK_Users_Departments", table: "Users");
        migrationBuilder.AddForeignKey(name: "FK_User_Insutition", table: "Users", column: "InstitutionID", principalTable: "Institutions", principalColumn: "InstitutionID", onDelete: ReferentialAction.Cascade);
        migrationBuilder.AddForeignKey(name: "FK_Users_Departments", table: "Users", column: "DepartmentId", principalTable: "Departments", principalColumn: "DepartmentId", onDelete: ReferentialAction.Cascade);
        migrationBuilder.DropCheckConstraint(name: "CK__Users__Role", table: "Users");
        migrationBuilder.AddCheckConstraint(name: "CK__Users__Role", table: "Users", sql: "[Role] IN ('Student', 'Admin', 'SuperAdmin', 'Doctor', 'TeachingAssistant')");
        migrationBuilder.DropColumn(name: "MustChangePassword", table: "Users");
        migrationBuilder.DropColumn(name: "PreviousRefreshTokenHash", table: "Users");
    }
}
