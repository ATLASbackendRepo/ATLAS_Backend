using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATLAsDCL.Migrations
{
    /// <inheritdoc />
    public partial class MediumLowHardening : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // MED-11: the columns below are narrowed from nvarchar(max) to a
            // bounded length so the new indexes fit inside the 900-byte key limit
            // and the poller can seek instead of scanning. A plain ALTER COLUMN
            // aborts the whole migration if a single existing row is longer than
            // the new limit, so the data is truncated first. The truncation is
            // deliberate and safe: an event type, severity label, IP or academic id
            // beyond these lengths is malformed, and the audit trail value that
            // actually matters (the description) is left untouched.
            migrationBuilder.Sql(@"
UPDATE SecurityEvents SET EventType           = LEFT(EventType, 100)           WHERE LEN(EventType)           > 100;
UPDATE SecurityEvents SET Severity            = LEFT(Severity, 20)             WHERE LEN(Severity)            > 20;
UPDATE SecurityEvents SET IpAddress           = LEFT(IpAddress, 45)            WHERE LEN(IpAddress)           > 45;
UPDATE SecurityEvents SET CorrelationId       = LEFT(CorrelationId, 100)       WHERE LEN(CorrelationId)       > 100;
UPDATE SecurityEvents SET AttemptedAcademicId = LEFT(AttemptedAcademicId, 50)  WHERE LEN(AttemptedAcademicId) > 50;");

            migrationBuilder.DropIndex(
                name: "IX_GradeAppealRequests_StudentID",
                table: "GradeAppealRequests");

            migrationBuilder.AddColumn<Guid>(
                name: "SecurityStamp",
                table: "Users",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "TimestampUtc",
                table: "SecurityEvents",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "Severity",
                table: "SecurityEvents",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Low",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "IpAddress",
                table: "SecurityEvents",
                type: "nvarchar(45)",
                maxLength: 45,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EventType",
                table: "SecurityEvents",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "CorrelationId",
                table: "SecurityEvents",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AttemptedAcademicId",
                table: "SecurityEvents",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeliveryAttempts",
                table: "SecurityEvents",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "LastDeliveryError",
                table: "SecurityEvents",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextAttemptAtUtc",
                table: "SecurityEvents",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "GradeAppealRequests",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "CourseGrades",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.CreateTable(
                name: "CourseGradeHistory",
                columns: table => new
                {
                    HistoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CG_ID = table.Column<int>(type: "int", nullable: false),
                    EnrollmentID = table.Column<int>(type: "int", nullable: false),
                    ModuleID = table.Column<int>(type: "int", nullable: false),
                    PreviousDegree = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    NewDegree = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    ChangedBy = table.Column<int>(type: "int", nullable: false),
                    ChangedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseGradeHistory", x => x.HistoryId);
                    table.ForeignKey(
                        name: "FK_CourseGradeHistory_CourseGrades_CG_ID",
                        column: x => x.CG_ID,
                        principalTable: "CourseGrades",
                        principalColumn: "CG_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SecurityEvents_FailedLoginByAccount",
                table: "SecurityEvents",
                columns: new[] { "EventType", "AttemptedAcademicId", "TimestampUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_SecurityEvents_PendingAlerts",
                table: "SecurityEvents",
                columns: new[] { "IsAlerted", "Severity", "TimestampUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_SecurityEvents_TimestampUtc",
                table: "SecurityEvents",
                column: "TimestampUtc");

            migrationBuilder.CreateIndex(
                name: "UX_GradeAppealRequests_OpenAppealPerStudentModule",
                table: "GradeAppealRequests",
                columns: new[] { "StudentID", "AssessmentModuleID" },
                unique: true,
                filter: "[Status] IN ('Pending', 'UnderReview')");

            migrationBuilder.CreateIndex(
                name: "IX_CourseGradeHistory_CG_ID",
                table: "CourseGradeHistory",
                columns: new[] { "CG_ID", "ChangedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CourseGradeHistory");

            migrationBuilder.DropIndex(
                name: "IX_SecurityEvents_FailedLoginByAccount",
                table: "SecurityEvents");

            migrationBuilder.DropIndex(
                name: "IX_SecurityEvents_PendingAlerts",
                table: "SecurityEvents");

            migrationBuilder.DropIndex(
                name: "IX_SecurityEvents_TimestampUtc",
                table: "SecurityEvents");

            migrationBuilder.DropIndex(
                name: "UX_GradeAppealRequests_OpenAppealPerStudentModule",
                table: "GradeAppealRequests");

            migrationBuilder.DropColumn(
                name: "SecurityStamp",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DeliveryAttempts",
                table: "SecurityEvents");

            migrationBuilder.DropColumn(
                name: "LastDeliveryError",
                table: "SecurityEvents");

            migrationBuilder.DropColumn(
                name: "NextAttemptAtUtc",
                table: "SecurityEvents");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "GradeAppealRequests");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "CourseGrades");

            migrationBuilder.AlterColumn<DateTime>(
                name: "TimestampUtc",
                table: "SecurityEvents",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETDATE()");

            migrationBuilder.AlterColumn<string>(
                name: "Severity",
                table: "SecurityEvents",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "Low");

            migrationBuilder.AlterColumn<string>(
                name: "IpAddress",
                table: "SecurityEvents",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(45)",
                oldMaxLength: 45,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EventType",
                table: "SecurityEvents",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "CorrelationId",
                table: "SecurityEvents",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AttemptedAcademicId",
                table: "SecurityEvents",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GradeAppealRequests_StudentID",
                table: "GradeAppealRequests",
                column: "StudentID");
        }
    }
}
