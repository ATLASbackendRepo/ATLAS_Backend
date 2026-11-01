using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATLAsDCL.Migrations
{
    /// <inheritdoc />
    public partial class ScopeSemesterUniquenessToInstitution : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Semesters_InstitutionID",
                table: "Semesters");

            migrationBuilder.DropIndex(
                name: "UQ_Semester_Name_AcademicYear",
                table: "Semesters");

            migrationBuilder.CreateIndex(
                name: "UQ_Semester_Institution_Name_AcademicYear",
                table: "Semesters",
                columns: new[] { "InstitutionID", "SemesterName", "AcademicYear" },
                unique: true,
                filter: "[InstitutionID] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UQ_Semester_Institution_Name_AcademicYear",
                table: "Semesters");

            migrationBuilder.CreateIndex(
                name: "IX_Semesters_InstitutionID",
                table: "Semesters",
                column: "InstitutionID");

            migrationBuilder.CreateIndex(
                name: "UQ_Semester_Name_AcademicYear",
                table: "Semesters",
                columns: new[] { "SemesterName", "AcademicYear" },
                unique: true);
        }
    }
}
