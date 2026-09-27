using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATLAsDCL.Migrations
{
    /// <inheritdoc />
    public partial class AddSemesterIsGradingClosed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsGradingClosed",
                table: "Semesters",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsGradingClosed",
                table: "Semesters");
        }
    }
}
