using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATLAsDCL.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    Action = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EntityId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    OldValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    CorrelationId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Success = table.Column<bool>(type: "bit", nullable: false),
                    TimestampUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Courses",
                columns: table => new
                {
                    CourseID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    C_Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Credit_Hours = table.Column<int>(type: "int", nullable: false),
                    IsPractical = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Courses", x => x.CourseID);
                });

            migrationBuilder.CreateTable(
                name: "Institutions",
                columns: table => new
                {
                    InstitutionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Institutions", x => x.InstitutionID);
                });

            migrationBuilder.CreateTable(
                name: "Semesters",
                columns: table => new
                {
                    SemesterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SemesterName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AcademicYear = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Semesters", x => x.SemesterID);
                });

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    DepartmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    D_Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    InstitutionID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.DepartmentId);
                    table.ForeignKey(
                        name: "FK_Departments_Institutions_InstitutionID",
                        column: x => x.InstitutionID,
                        principalTable: "Institutions",
                        principalColumn: "InstitutionID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CourseOfferings",
                columns: table => new
                {
                    CourseOfferingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SemesterID = table.Column<int>(type: "int", nullable: false),
                    CourseID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseOfferings", x => x.CourseOfferingID);
                    table.ForeignKey(
                        name: "FK_CourseOfferings_Courses_CourseID",
                        column: x => x.CourseID,
                        principalTable: "Courses",
                        principalColumn: "CourseID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CourseOfferings_Semesters_SemesterID",
                        column: x => x.SemesterID,
                        principalTable: "Semesters",
                        principalColumn: "SemesterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Student"),
                    AcademicId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InstitutionID = table.Column<int>(type: "int", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    RefreshTokenHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RefreshTokenExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RefreshTokenRevokedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                    table.CheckConstraint("CK__Users__Role", "[Role] IN ('Student', 'Admin', 'SuperAdmin', 'Doctor', 'TeachingAssistant')");
                    table.ForeignKey(
                        name: "FK_User_Insutition",
                        column: x => x.InstitutionID,
                        principalTable: "Institutions",
                        principalColumn: "InstitutionID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Users_Departments",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "DepartmentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CourseGroupMessages",
                columns: table => new
                {
                    MessageID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CourseOfferingID = table.Column<int>(type: "int", nullable: false),
                    SenderUserID = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    IsPinned = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseGroupMessages", x => x.MessageID);
                    table.ForeignKey(
                        name: "FK_CourseGroupMessages_CourseOfferings_CourseOfferingID",
                        column: x => x.CourseOfferingID,
                        principalTable: "CourseOfferings",
                        principalColumn: "CourseOfferingID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CourseGroupMessages_Users_SenderUserID",
                        column: x => x.SenderUserID,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CourseOfferingDoctors",
                columns: table => new
                {
                    CourseOfferingDoctorID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CourseOfferingID = table.Column<int>(type: "int", nullable: false),
                    DoctorID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseOfferingDoctors", x => x.CourseOfferingDoctorID);
                    table.ForeignKey(
                        name: "FK_CourseOfferingDoctors_CourseOfferings_CourseOfferingID",
                        column: x => x.CourseOfferingID,
                        principalTable: "CourseOfferings",
                        principalColumn: "CourseOfferingID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CourseOfferingDoctors_Users_DoctorID",
                        column: x => x.DoctorID,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CourseOfferingTAs",
                columns: table => new
                {
                    CourseOfferingTAID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CourseOfferingID = table.Column<int>(type: "int", nullable: false),
                    TeacherAssistID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseOfferingTAs", x => x.CourseOfferingTAID);
                    table.ForeignKey(
                        name: "FK_CourseOfferingTAs_CourseOfferings_CourseOfferingID",
                        column: x => x.CourseOfferingID,
                        principalTable: "CourseOfferings",
                        principalColumn: "CourseOfferingID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CourseOfferingTAs_Users_TeacherAssistID",
                        column: x => x.TeacherAssistID,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StaffProfiles",
                columns: table => new
                {
                    StaffProfileID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    AcademicId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DepartmentID = table.Column<int>(type: "int", nullable: true),
                    TeachingYears = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffProfiles", x => x.StaffProfileID);
                    table.ForeignKey(
                        name: "FK_StaffProfiles_Departments_DepartmentID",
                        column: x => x.DepartmentID,
                        principalTable: "Departments",
                        principalColumn: "DepartmentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StaffProfiles_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    StudentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    AcademicLevel = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    AcademicId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InstitutionID = table.Column<int>(type: "int", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.StudentId);
                    table.ForeignKey(
                        name: "FK_Students_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "DepartmentId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Students_Institutions_InstitutionID",
                        column: x => x.InstitutionID,
                        principalTable: "Institutions",
                        principalColumn: "InstitutionID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Students_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CourseGroupMessageReads",
                columns: table => new
                {
                    MessageID = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseGroupMessageReads", x => new { x.MessageID, x.UserID });
                    table.ForeignKey(
                        name: "FK_CourseGroupMessageReads_CourseGroupMessages_MessageID",
                        column: x => x.MessageID,
                        principalTable: "CourseGroupMessages",
                        principalColumn: "MessageID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CourseGroupMessageReads_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    NotificationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RecipientUserID = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Body = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    RelatedMessageID = table.Column<int>(type: "int", nullable: true),
                    IsRead = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.NotificationID);
                    table.ForeignKey(
                        name: "FK_Notifications_CourseGroupMessages_RelatedMessageID",
                        column: x => x.RelatedMessageID,
                        principalTable: "CourseGroupMessages",
                        principalColumn: "MessageID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_RecipientUserID",
                        column: x => x.RecipientUserID,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Enrollments",
                columns: table => new
                {
                    EnrollmentID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentID = table.Column<int>(type: "int", nullable: false),
                    CreatedByUserID = table.Column<int>(type: "int", nullable: false),
                    EnrollmentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CourseOfferingID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enrollments", x => x.EnrollmentID);
                    table.ForeignKey(
                        name: "FK_Enrollments_CourseOfferings_CourseOfferingID",
                        column: x => x.CourseOfferingID,
                        principalTable: "CourseOfferings",
                        principalColumn: "CourseOfferingID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Enrollments_Students_StudentID",
                        column: x => x.StudentID,
                        principalTable: "Students",
                        principalColumn: "StudentId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Enrollments_Users_CreatedByUserID",
                        column: x => x.CreatedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StudentProfiles",
                columns: table => new
                {
                    StudentProfileID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentID = table.Column<int>(type: "int", nullable: false),
                    LinkedInUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    SectionNumber = table.Column<int>(type: "int", nullable: true),
                    Faculty = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QRCodeUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentProfiles", x => x.StudentProfileID);
                    table.ForeignKey(
                        name: "FK_StudentProfiles_Students_StudentID",
                        column: x => x.StudentID,
                        principalTable: "Students",
                        principalColumn: "StudentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssessmentModules",
                columns: table => new
                {
                    ModuleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CourseOfferingID = table.Column<int>(type: "int", nullable: false),
                    ModuleName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    MaxDegree = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    CourseGradeCG_ID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentModules", x => x.ModuleID);
                    table.ForeignKey(
                        name: "FK_AssessmentModules_CourseOfferings_CourseOfferingID",
                        column: x => x.CourseOfferingID,
                        principalTable: "CourseOfferings",
                        principalColumn: "CourseOfferingID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CourseGrades",
                columns: table => new
                {
                    CG_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EnrollmentID = table.Column<int>(type: "int", nullable: false),
                    ModuleID = table.Column<int>(type: "int", nullable: false),
                    StudentDegree = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseGrades", x => x.CG_ID);
                    table.ForeignKey(
                        name: "FK_CourseGrades_AssessmentModules_ModuleID",
                        column: x => x.ModuleID,
                        principalTable: "AssessmentModules",
                        principalColumn: "ModuleID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CourseGrades_Enrollments_EnrollmentID",
                        column: x => x.EnrollmentID,
                        principalTable: "Enrollments",
                        principalColumn: "EnrollmentID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GradeAppealRequests",
                columns: table => new
                {
                    RequestID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentID = table.Column<int>(type: "int", nullable: false),
                    CourseOfferingID = table.Column<int>(type: "int", nullable: false),
                    AssessmentModuleID = table.Column<int>(type: "int", nullable: true),
                    DoctorUserID = table.Column<int>(type: "int", nullable: false),
                    CurrentGrade = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    ProblemDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Pending"),
                    DoctorResponse = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GradeAppealRequests", x => x.RequestID);
                    table.CheckConstraint("CK_GradeAppealRequests_Status", "[Status] IN ('Pending', 'UnderReview', 'NeedMoreInfo', 'Approved', 'Rejected', 'Closed')");
                    table.ForeignKey(
                        name: "FK_GradeAppealRequests_AssessmentModules",
                        column: x => x.AssessmentModuleID,
                        principalTable: "AssessmentModules",
                        principalColumn: "ModuleID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GradeAppealRequests_CourseOfferings",
                        column: x => x.CourseOfferingID,
                        principalTable: "CourseOfferings",
                        principalColumn: "CourseOfferingID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GradeAppealRequests_Students",
                        column: x => x.StudentID,
                        principalTable: "Students",
                        principalColumn: "StudentId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GradeAppealRequests_Users",
                        column: x => x.DoctorUserID,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentModules_CourseGradeCG_ID",
                table: "AssessmentModules",
                column: "CourseGradeCG_ID");

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentModules_CourseOfferingID",
                table: "AssessmentModules",
                column: "CourseOfferingID");

            migrationBuilder.CreateIndex(
                name: "IX_CourseGrades_EnrollmentID",
                table: "CourseGrades",
                column: "EnrollmentID");

            migrationBuilder.CreateIndex(
                name: "IX_CourseGrades_ModuleID",
                table: "CourseGrades",
                column: "ModuleID");

            migrationBuilder.CreateIndex(
                name: "IX_CourseGroupMessageReads_UserID",
                table: "CourseGroupMessageReads",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_CourseGroupMessages_CourseOfferingID",
                table: "CourseGroupMessages",
                column: "CourseOfferingID");

            migrationBuilder.CreateIndex(
                name: "IX_CourseGroupMessages_SenderUserID",
                table: "CourseGroupMessages",
                column: "SenderUserID");

            migrationBuilder.CreateIndex(
                name: "IX_CourseOfferingDoctors_CourseOfferingID",
                table: "CourseOfferingDoctors",
                column: "CourseOfferingID");

            migrationBuilder.CreateIndex(
                name: "IX_CourseOfferingDoctors_DoctorID",
                table: "CourseOfferingDoctors",
                column: "DoctorID");

            migrationBuilder.CreateIndex(
                name: "IX_CourseOfferings_CourseID",
                table: "CourseOfferings",
                column: "CourseID");

            migrationBuilder.CreateIndex(
                name: "UQ_Course_Semester",
                table: "CourseOfferings",
                column: "SemesterID");

            migrationBuilder.CreateIndex(
                name: "IX_CourseOfferingTAs_CourseOfferingID",
                table: "CourseOfferingTAs",
                column: "CourseOfferingID");

            migrationBuilder.CreateIndex(
                name: "IX_CourseOfferingTAs_TeacherAssistID",
                table: "CourseOfferingTAs",
                column: "TeacherAssistID");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_InstitutionID",
                table: "Departments",
                column: "InstitutionID");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_CourseOfferingID",
                table: "Enrollments",
                column: "CourseOfferingID");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_CreatedByUserID",
                table: "Enrollments",
                column: "CreatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_StudentID",
                table: "Enrollments",
                column: "StudentID");

            migrationBuilder.CreateIndex(
                name: "IX_GradeAppealRequests_AssessmentModuleID",
                table: "GradeAppealRequests",
                column: "AssessmentModuleID");

            migrationBuilder.CreateIndex(
                name: "IX_GradeAppealRequests_CourseOfferingID",
                table: "GradeAppealRequests",
                column: "CourseOfferingID");

            migrationBuilder.CreateIndex(
                name: "IX_GradeAppealRequests_DoctorUserID",
                table: "GradeAppealRequests",
                column: "DoctorUserID");

            migrationBuilder.CreateIndex(
                name: "IX_GradeAppealRequests_StudentID",
                table: "GradeAppealRequests",
                column: "StudentID");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RecipientUserID",
                table: "Notifications",
                column: "RecipientUserID");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RelatedMessageID",
                table: "Notifications",
                column: "RelatedMessageID");

            migrationBuilder.CreateIndex(
                name: "UQ_Semester_Name_AcademicYear",
                table: "Semesters",
                columns: new[] { "SemesterName", "AcademicYear" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StaffProfiles_DepartmentID",
                table: "StaffProfiles",
                column: "DepartmentID");

            migrationBuilder.CreateIndex(
                name: "IX_StaffProfiles_UserID",
                table: "StaffProfiles",
                column: "UserID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentProfiles_StudentID",
                table: "StudentProfiles",
                column: "StudentID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Students_DepartmentId",
                table: "Students",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Students_InstitutionID",
                table: "Students",
                column: "InstitutionID");

            migrationBuilder.CreateIndex(
                name: "IX_Students_StudentCode",
                table: "Students",
                column: "StudentCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Students_UserId",
                table: "Students",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_DepartmentId",
                table: "Users",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_InstitutionID",
                table: "Users",
                column: "InstitutionID");

            migrationBuilder.AddForeignKey(
                name: "FK_AssessmentModules_CourseGrades_CourseGradeCG_ID",
                table: "AssessmentModules",
                column: "CourseGradeCG_ID",
                principalTable: "CourseGrades",
                principalColumn: "CG_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssessmentModules_CourseGrades_CourseGradeCG_ID",
                table: "AssessmentModules");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "CourseGroupMessageReads");

            migrationBuilder.DropTable(
                name: "CourseOfferingDoctors");

            migrationBuilder.DropTable(
                name: "CourseOfferingTAs");

            migrationBuilder.DropTable(
                name: "GradeAppealRequests");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "StaffProfiles");

            migrationBuilder.DropTable(
                name: "StudentProfiles");

            migrationBuilder.DropTable(
                name: "CourseGroupMessages");

            migrationBuilder.DropTable(
                name: "CourseGrades");

            migrationBuilder.DropTable(
                name: "AssessmentModules");

            migrationBuilder.DropTable(
                name: "Enrollments");

            migrationBuilder.DropTable(
                name: "CourseOfferings");

            migrationBuilder.DropTable(
                name: "Students");

            migrationBuilder.DropTable(
                name: "Courses");

            migrationBuilder.DropTable(
                name: "Semesters");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropTable(
                name: "Institutions");
        }
    }
}
