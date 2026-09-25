using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATLAsDCL.Migrations
{
    /// <inheritdoc />
    public partial class UniqueAcademicId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Data cleanup that must run before the column can be narrowed and
            // made unique. Over-long values are truncated, and duplicate
            // academic ids that predate the unique index keep their oldest row
            // untouched while the later rows are suffixed with their own primary
            // key, so no account is lost and the index can be created.
            migrationBuilder.Sql(
                "UPDATE Users SET AcademicId = LEFT(AcademicId, 50) WHERE LEN(AcademicId) > 50;");

            migrationBuilder.Sql(
                "UPDATE Students SET AcademicId = LEFT(AcademicId, 50) WHERE LEN(AcademicId) > 50;");

            migrationBuilder.Sql(
                "UPDATE target SET AcademicId = LEFT(target.AcademicId, 40) + '#' + CAST(target.UserId AS nvarchar(9)) " +
                "FROM Users AS target " +
                "WHERE EXISTS (SELECT 1 FROM Users AS older " +
                "WHERE older.AcademicId = target.AcademicId AND older.UserId < target.UserId);");

            migrationBuilder.Sql(
                "UPDATE target SET AcademicId = LEFT(target.AcademicId, 40) + '#' + CAST(target.StudentId AS nvarchar(9)) " +
                "FROM Students AS target " +
                "WHERE EXISTS (SELECT 1 FROM Students AS older " +
                "WHERE older.AcademicId = target.AcademicId AND older.StudentId < target.StudentId);");

            migrationBuilder.AlterColumn<string>(
                name: "AcademicId",
                table: "Users",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "AcademicId",
                table: "Students",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Users_AcademicId",
                table: "Users",
                column: "AcademicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Students_AcademicId",
                table: "Students",
                column: "AcademicId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_AcademicId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Students_AcademicId",
                table: "Students");

            migrationBuilder.AlterColumn<string>(
                name: "AcademicId",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "AcademicId",
                table: "Students",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);
        }
    }
}
