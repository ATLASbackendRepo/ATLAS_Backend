using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATLAsDCL.Migrations
{
    /// <inheritdoc />
    public partial class AddInstitutionToCourseOfferingSemester : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InstitutionID",
                table: "Semesters",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InstitutionID",
                table: "Courses",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InstitutionID",
                table: "CourseOfferings",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Semesters_InstitutionID",
                table: "Semesters",
                column: "InstitutionID");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_InstitutionID",
                table: "Courses",
                column: "InstitutionID");

            migrationBuilder.CreateIndex(
                name: "IX_CourseOfferings_InstitutionID",
                table: "CourseOfferings",
                column: "InstitutionID");

            migrationBuilder.AddForeignKey(
                name: "FK_CourseOfferings_Institutions_InstitutionID",
                table: "CourseOfferings",
                column: "InstitutionID",
                principalTable: "Institutions",
                principalColumn: "InstitutionID");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Institutions_InstitutionID",
                table: "Courses",
                column: "InstitutionID",
                principalTable: "Institutions",
                principalColumn: "InstitutionID");

            migrationBuilder.AddForeignKey(
                name: "FK_Semesters_Institutions_InstitutionID",
                table: "Semesters",
                column: "InstitutionID",
                principalTable: "Institutions",
                principalColumn: "InstitutionID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CourseOfferings_Institutions_InstitutionID",
                table: "CourseOfferings");

            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Institutions_InstitutionID",
                table: "Courses");

            migrationBuilder.DropForeignKey(
                name: "FK_Semesters_Institutions_InstitutionID",
                table: "Semesters");

            migrationBuilder.DropIndex(
                name: "IX_Semesters_InstitutionID",
                table: "Semesters");

            migrationBuilder.DropIndex(
                name: "IX_Courses_InstitutionID",
                table: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_CourseOfferings_InstitutionID",
                table: "CourseOfferings");

            migrationBuilder.DropColumn(
                name: "InstitutionID",
                table: "Semesters");

            migrationBuilder.DropColumn(
                name: "InstitutionID",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "InstitutionID",
                table: "CourseOfferings");
        }
    }
}
