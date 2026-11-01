using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATLAsDCL.Migrations
{
    /// <inheritdoc />
    public partial class ScopeCourseOfferingUniquenessToInstitution : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CourseOfferings_InstitutionID",
                table: "CourseOfferings");

            migrationBuilder.RenameIndex(
                name: "UQ_Course_Semester",
                table: "CourseOfferings",
                newName: "IX_CourseOfferings_SemesterID");

            migrationBuilder.CreateIndex(
                name: "UQ_CourseOffering_Institution_Course_Semester",
                table: "CourseOfferings",
                columns: new[] { "InstitutionID", "CourseID", "SemesterID" },
                unique: true,
                filter: "[InstitutionID] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UQ_CourseOffering_Institution_Course_Semester",
                table: "CourseOfferings");

            migrationBuilder.RenameIndex(
                name: "IX_CourseOfferings_SemesterID",
                table: "CourseOfferings",
                newName: "UQ_Course_Semester");

            migrationBuilder.CreateIndex(
                name: "IX_CourseOfferings_InstitutionID",
                table: "CourseOfferings",
                column: "InstitutionID");
        }
    }
}
