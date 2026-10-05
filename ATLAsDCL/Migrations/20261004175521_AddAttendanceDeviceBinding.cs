using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATLAsDCL.Migrations
{
    /// <inheritdoc />
    public partial class AddAttendanceDeviceBinding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RegisteredDevices",
                columns: table => new
                {
                    RegisteredDeviceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    DeviceId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    DeviceIdHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    DeviceSecret = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    DeviceName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PairedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegisteredDevices", x => x.RegisteredDeviceId);
                    table.ForeignKey(
                        name: "FK_RegisteredDevices_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "UX_AttendanceSessions_OneActivePerOffering",
                table: "AttendanceSessions",
                column: "CourseOfferingId",
                unique: true,
                filter: "[Status] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_RegisteredDevices_DeviceIdHash",
                table: "RegisteredDevices",
                column: "DeviceIdHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegisteredDevices_UserId",
                table: "RegisteredDevices",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RegisteredDevices");

            migrationBuilder.DropIndex(
                name: "UX_AttendanceSessions_OneActivePerOffering",
                table: "AttendanceSessions");
        }
    }
}
