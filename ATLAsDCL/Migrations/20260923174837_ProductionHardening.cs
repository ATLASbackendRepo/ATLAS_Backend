using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATLAsDCL.Migrations
{
    /// <inheritdoc />
    public partial class ProductionHardening : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Sequence backing the Students.StudentCode default constraint below.
            // It must exist before the constraint is created; the consuming
            // trigger TR_Students_GenerateCode is created in AddDatabaseObjects.
            migrationBuilder.Sql(
                "IF OBJECT_ID('dbo.Seq_StudentCode', 'SO') IS NULL CREATE SEQUENCE dbo.Seq_StudentCode AS int START WITH 1 INCREMENT BY 1;");

            migrationBuilder.DropForeignKey(
                name: "FK_User_Insutition",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Departments",
                table: "Users");

            migrationBuilder.DropCheckConstraint(
                name: "CK__Users__Role",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_CourseOfferingTAs_CourseOfferingID",
                table: "CourseOfferingTAs");

            migrationBuilder.DropIndex(
                name: "IX_CourseOfferingDoctors_CourseOfferingID",
                table: "CourseOfferingDoctors");

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

            migrationBuilder.AlterColumn<string>(
                name: "StudentCode",
                table: "Students",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValueSql: "('S' + RIGHT('00000000' + CAST(NEXT VALUE FOR dbo.Seq_StudentCode AS nvarchar(10)), 8))",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AddCheckConstraint(
                name: "CK__Users__Role",
                table: "Users",
                sql: "[Role] IN ('Student', 'Admin', 'SAdmin', 'Doctor', 'TeachingAssistant')");

            // These unique indexes are part of the model snapshot since
            // AddAttendanceModule, but that migration never created them in the
            // database; they are created here so fresh databases match the model.
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

            migrationBuilder.CreateIndex(
                name: "IX_CourseOfferingTAs_CourseOfferingID_TeacherAssistID",
                table: "CourseOfferingTAs",
                columns: new[] { "CourseOfferingID", "TeacherAssistID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CourseOfferingDoctors_CourseOfferingID_DoctorID",
                table: "CourseOfferingDoctors",
                columns: new[] { "CourseOfferingID", "DoctorID" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_CourseGrades_StudentDegree_NonNegative",
                table: "CourseGrades",
                sql: "[StudentDegree] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AssessmentModules_MaxDegree_Range",
                table: "AssessmentModules",
                sql: "[MaxDegree] >= 1 AND [MaxDegree] <= 40");

            migrationBuilder.AddForeignKey(
                name: "FK_User_Insutition",
                table: "Users",
                column: "InstitutionID",
                principalTable: "Institutions",
                principalColumn: "InstitutionID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Departments",
                table: "Users",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "DepartmentId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_User_Insutition",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Departments",
                table: "Users");

            migrationBuilder.DropCheckConstraint(
                name: "CK__Users__Role",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_CourseOfferingTAs_CourseOfferingID_TeacherAssistID",
                table: "CourseOfferingTAs");

            migrationBuilder.DropIndex(
                name: "IX_CourseOfferingDoctors_CourseOfferingID_DoctorID",
                table: "CourseOfferingDoctors");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CourseGrades_StudentDegree_NonNegative",
                table: "CourseGrades");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AssessmentModules_MaxDegree_Range",
                table: "AssessmentModules");

            migrationBuilder.DropIndex(
                name: "UX_CourseGrades_EnrollmentID_ModuleID",
                table: "CourseGrades");

            migrationBuilder.DropIndex(
                name: "UX_Enrollments_StudentID_CourseOfferingID",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "MustChangePassword",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PreviousRefreshTokenHash",
                table: "Users");

            migrationBuilder.AlterColumn<string>(
                name: "StudentCode",
                table: "Students",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValueSql: "('S' + RIGHT('00000000' + CAST(NEXT VALUE FOR dbo.Seq_StudentCode AS nvarchar(10)), 8))");

            migrationBuilder.AddCheckConstraint(
                name: "CK__Users__Role",
                table: "Users",
                sql: "[Role] IN ('Student', 'Admin', 'SuperAdmin', 'Doctor', 'TeachingAssistant')");

            migrationBuilder.CreateIndex(
                name: "IX_CourseOfferingTAs_CourseOfferingID",
                table: "CourseOfferingTAs",
                column: "CourseOfferingID");

            migrationBuilder.CreateIndex(
                name: "IX_CourseOfferingDoctors_CourseOfferingID",
                table: "CourseOfferingDoctors",
                column: "CourseOfferingID");

            migrationBuilder.AddForeignKey(
                name: "FK_User_Insutition",
                table: "Users",
                column: "InstitutionID",
                principalTable: "Institutions",
                principalColumn: "InstitutionID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Departments",
                table: "Users",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "DepartmentId",
                onDelete: ReferentialAction.Cascade);

            // Dropping the sequence is only safe after the AlterColumn above has
            // removed the default constraint that references it.
            migrationBuilder.Sql(
                "IF OBJECT_ID('dbo.Seq_StudentCode', 'SO') IS NOT NULL DROP SEQUENCE dbo.Seq_StudentCode;");
        }
    }
}
