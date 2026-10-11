using ATLAS.Claims;
using ATLAS.Controllers.CourseOffering;
using ATLAS.Controllers.Student;
using ATLASDomain.DTO.CourseOffring;
using ATLASDomain.DTO.Staff;
using ATLASDomain.Entities;
using ATLAsBLL.Features.Students.Commands.CreateStudentsRange;
using ATLAsBLL.Features.Users.Commands.UpdateUser;
using ATLAsBLL.Services;
using ATLAsDCL.Data;
using ATLAsDCL.Repositories;
using ATLAS.Persistence.Repositories;
using ATLAsDCL.UnitOfWork;
using ClosedXML.Excel;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ATLAS.Tests
{
    public class TenantIsolationAndSecurityAuditTests
    {
        private static IUnitOfWork CreateUnitOfWork(AtLASDbContext context) => new UnitOfWork(
            context,
            new AssessmentModuleRepository(context),
            new CourseGradeRepository(context),
            new CourseOfferingDoctorRepository(context),
            new CourseOfferingRepository(context),
            new CourseOfferingTARepository(context),
            new CourseRepository(context),
            new DepartmentRepository(context),
            new EnrollmentRepository(context),
            new SemesterRepository(context),
            new StudentRepository(context),
            new UserRepository(context),
            new CourseGroupMessageRepository(context),
            new NotificationRepository(context),
            new StudentProfileRepository(context),
            new StaffProfileRepository(context),
            new CourseGroupMessageReadRepository(context),
            new GradeAppealRequestRepository(context),
            new institutionRepository(context)
        );

        #region ATLAS-SEC-001: Admin Course Lookup Tenant Isolation Tests

        private static CourseOffringController CreateControllerWithAdminContext(
            Mock<IMediator> mediatorMock,
            IUnitOfWork unitOfWork,
            string role,
            int institutionId)
        {
            var controller = new CourseOffringController(mediatorMock.Object, NullLogger<CourseOffringController>.Instance, unitOfWork);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "999"),
                new Claim(ClaimTypes.Role, role),
                new Claim(AtlasClaims.InstitutionId, institutionId.ToString())
            };

            var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            return controller;
        }

        [Fact]
        public async Task ATLAS_SEC_001_Admin_SameInstitution_StudentCourses_ReturnsOk()
        {
            using var db = TestDbContextFactory.Create();
            var targetStudent = new User
            {
                UserId = 10,
                AcademicId = "STU_INST1",
                FullName = "Student Inst1",
                Password = "hash",
                InstitutionID = 1,
                Role = enUserRole.Student,
                IsActive = true
            };
            db.Users.Add(targetStudent);
            await db.SaveChangesAsync();

            var uow = CreateUnitOfWork(db);
            var mediatorMock = new Mock<IMediator>();
            mediatorMock.Setup(m => m.Send(It.IsAny<ATLAsBLL.Services.CourseOffering.Queries.GetmyCourse.Student.GetMyCourseQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CourseOfferingDTO> { new() });

            var controller = CreateControllerWithAdminContext(mediatorMock, uow, "Admin", institutionId: 1);

            var result = await controller.GetAdminStudentMyCourses("STU_INST1");

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task ATLAS_SEC_001_Admin_CrossInstitution_StudentCourses_ReturnsForbid()
        {
            using var db = TestDbContextFactory.Create();
            var targetStudent = new User
            {
                UserId = 20,
                AcademicId = "STU_INST2",
                FullName = "Student Inst2",
                Password = "hash",
                InstitutionID = 2,
                Role = enUserRole.Student,
                IsActive = true
            };
            db.Users.Add(targetStudent);
            await db.SaveChangesAsync();

            var uow = CreateUnitOfWork(db);
            var mediatorMock = new Mock<IMediator>();
            var controller = CreateControllerWithAdminContext(mediatorMock, uow, "Admin", institutionId: 1);

            var result = await controller.GetAdminStudentMyCourses("STU_INST2");

            Assert.IsType<ForbidResult>(result.Result);
        }

        [Fact]
        public async Task ATLAS_SEC_001_Admin_CrossInstitution_StaffCourses_ReturnsForbid()
        {
            using var db = TestDbContextFactory.Create();
            var targetDoctor = new User
            {
                UserId = 30,
                AcademicId = "DOC_INST2",
                FullName = "Doctor Inst2",
                Password = "hash",
                InstitutionID = 2,
                Role = enUserRole.Doctor,
                IsActive = true
            };
            db.Users.Add(targetDoctor);
            await db.SaveChangesAsync();

            var uow = CreateUnitOfWork(db);
            var mediatorMock = new Mock<IMediator>();
            var controller = CreateControllerWithAdminContext(mediatorMock, uow, "Admin", institutionId: 1);

            var result = await controller.GetAdminStaffMyCourses("DOC_INST2");

            Assert.IsType<ForbidResult>(result.Result);
        }

        [Fact]
        public async Task ATLAS_SEC_001_SAdmin_CrossInstitution_StudentCourses_ReturnsOk()
        {
            using var db = TestDbContextFactory.Create();
            var targetStudent = new User
            {
                UserId = 40,
                AcademicId = "STU_INST2",
                FullName = "Student Inst2",
                Password = "hash",
                InstitutionID = 2,
                Role = enUserRole.Student,
                IsActive = true
            };
            db.Users.Add(targetStudent);
            await db.SaveChangesAsync();

            var uow = CreateUnitOfWork(db);
            var mediatorMock = new Mock<IMediator>();
            mediatorMock.Setup(m => m.Send(It.IsAny<ATLAsBLL.Services.CourseOffering.Queries.GetmyCourse.Student.GetMyCourseQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CourseOfferingDTO> { new() });

            // SAdmin is global (can query any institution)
            var controller = CreateControllerWithAdminContext(mediatorMock, uow, "SAdmin", institutionId: 1);

            var result = await controller.GetAdminStudentMyCourses("STU_INST2");

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task ATLAS_SEC_001_SAdmin_CrossInstitution_StaffCourses_ReturnsOk()
        {
            using var db = TestDbContextFactory.Create();
            var targetDoctor = new User
            {
                UserId = 45,
                AcademicId = "DOC_INST2",
                FullName = "Doctor Inst2",
                Password = "hash",
                InstitutionID = 2,
                Role = enUserRole.Doctor,
                IsActive = true
            };
            db.Users.Add(targetDoctor);
            await db.SaveChangesAsync();

            var uow = CreateUnitOfWork(db);
            var mediatorMock = new Mock<IMediator>();
            mediatorMock.Setup(m => m.Send(It.IsAny<ATLAsBLL.Services.CourseOffering.Queries.GetmyCourse.Doctor.GetStaffMyCourseQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<DoctorCourseOfferingDTO> { new() });

            // SAdmin is global (can query any institution's staff)
            var controller = CreateControllerWithAdminContext(mediatorMock, uow, "SAdmin", institutionId: 1);

            var result = await controller.GetAdminStaffMyCourses("DOC_INST2");

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task ATLAS_SEC_001_Admin_SameInstitution_StaffCourses_ReturnsOk()
        {
            using var db = TestDbContextFactory.Create();
            var targetDoctor = new User
            {
                UserId = 46,
                AcademicId = "DOC_INST1",
                FullName = "Doctor Inst1",
                Password = "hash",
                InstitutionID = 1,
                Role = enUserRole.Doctor,
                IsActive = true
            };
            db.Users.Add(targetDoctor);
            await db.SaveChangesAsync();

            var uow = CreateUnitOfWork(db);
            var mediatorMock = new Mock<IMediator>();
            mediatorMock.Setup(m => m.Send(It.IsAny<ATLAsBLL.Services.CourseOffering.Queries.GetmyCourse.Doctor.GetStaffMyCourseQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<DoctorCourseOfferingDTO> { new() });

            // Admin can query staff within their own institution
            var controller = CreateControllerWithAdminContext(mediatorMock, uow, "Admin", institutionId: 1);

            var result = await controller.GetAdminStaffMyCourses("DOC_INST1");

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task ATLAS_SEC_001_Admin_UnknownAcademicId_ReturnsBadRequest()
        {
            using var db = TestDbContextFactory.Create();
            var uow = CreateUnitOfWork(db);
            var mediatorMock = new Mock<IMediator>();
            var controller = CreateControllerWithAdminContext(mediatorMock, uow, "Admin", institutionId: 1);

            var result = await controller.GetAdminStudentMyCourses("NON_EXISTENT");

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        #endregion

        #region ATLAS-SEC-002: Invalidation of Stale JWTs on Institution Change

        [Fact]
        public async Task ATLAS_SEC_002_UpdateUser_WhenInstitutionChanges_BumpsSecurityStamp()
        {
            using var db = TestDbContextFactory.Create();
            var initialStamp = Guid.NewGuid();
            var user = new User
            {
                UserId = 50,
                AcademicId = "ADMIN_TRANSFER",
                FullName = "Admin To Transfer",
                Password = "hash",
                InstitutionID = 1,
                Role = enUserRole.Admin,
                IsActive = true,
                SecurityStamp = initialStamp
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var uow = CreateUnitOfWork(db);
            var handler = new UpdateUserCommandHandler(uow);

            var command = new UpdateUserCommand(new Create_Update_StaffDto
            {
                AcademicId = "ADMIN_TRANSFER",
                InstituteId = 2, // Institution changes from 1 to 2
                Role = enUserRole.Admin // Role remains Admin
            });

            var result = await handler.Handle(command, CancellationToken.None);

            Assert.True(result);
            var updatedUser = await db.Users.FirstAsync(u => u.UserId == 50);
            Assert.Equal(2, updatedUser.InstitutionID);
            Assert.NotEqual(initialStamp, updatedUser.SecurityStamp);
        }

        [Fact]
        public async Task ATLAS_SEC_002_UpdateUser_WhenOnlyRoleChanges_BumpsSecurityStamp()
        {
            using var db = TestDbContextFactory.Create();
            var initialStamp = Guid.NewGuid();
            var user = new User
            {
                UserId = 51,
                AcademicId = "STAFF_ROLE_CHANGE",
                FullName = "Doctor to Admin",
                Password = "hash",
                InstitutionID = 1,
                Role = enUserRole.Doctor,
                IsActive = true,
                SecurityStamp = initialStamp
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var uow = CreateUnitOfWork(db);
            var handler = new UpdateUserCommandHandler(uow);

            var command = new UpdateUserCommand(new Create_Update_StaffDto
            {
                AcademicId = "STAFF_ROLE_CHANGE",
                InstituteId = 1, // Institution remains 1
                Role = enUserRole.Admin // Role changes
            });

            var result = await handler.Handle(command, CancellationToken.None);

            Assert.True(result);
            var updatedUser = await db.Users.FirstAsync(u => u.UserId == 51);
            Assert.Equal(enUserRole.Admin, updatedUser.Role);
            Assert.NotEqual(initialStamp, updatedUser.SecurityStamp);
        }

        [Fact]
        public async Task ATLAS_SEC_002_UpdateUser_WhenNeitherChanges_PreservesSecurityStamp()
        {
            using var db = TestDbContextFactory.Create();
            var initialStamp = Guid.NewGuid();
            var user = new User
            {
                UserId = 52,
                AcademicId = "STAFF_NO_CHANGE",
                FullName = "Original Name",
                Password = "hash",
                InstitutionID = 1,
                Role = enUserRole.Admin,
                IsActive = true,
                SecurityStamp = initialStamp
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var uow = CreateUnitOfWork(db);
            var handler = new UpdateUserCommandHandler(uow);

            var command = new UpdateUserCommand(new Create_Update_StaffDto
            {
                AcademicId = "STAFF_NO_CHANGE",
                FullName = "Updated Name", // Only name changes
                InstituteId = 1,
                Role = enUserRole.Admin
            });

            var result = await handler.Handle(command, CancellationToken.None);

            Assert.True(result);
            var updatedUser = await db.Users.FirstAsync(u => u.UserId == 52);
            Assert.Equal("Updated Name", updatedUser.FullName);
            Assert.Equal(initialStamp, updatedUser.SecurityStamp);
        }

        [Fact]
        public async Task ATLAS_SEC_002_TokenLevel_InstitutionChange_InvalidatesOldTokenStamp()
        {
            using var db = TestDbContextFactory.Create();
            var initialStamp = Guid.NewGuid();
            var user = new User
            {
                UserId = 60,
                AcademicId = "ADMIN_OLD_TOKEN",
                FullName = "Admin Old Token",
                Password = "hash",
                InstitutionID = 1,
                Role = enUserRole.Admin,
                IsActive = true,
                SecurityStamp = initialStamp
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            // Simulate issuing a token that carries the initialStamp claim
            var tokenStampClaim = initialStamp.ToString();

            // Move user to institution 2
            var uow = CreateUnitOfWork(db);
            var handler = new UpdateUserCommandHandler(uow);
            var command = new UpdateUserCommand(new Create_Update_StaffDto
            {
                AcademicId = "ADMIN_OLD_TOKEN",
                InstituteId = 2,
                Role = enUserRole.Admin
            });

            var updateSuccess = await handler.Handle(command, CancellationToken.None);
            Assert.True(updateSuccess);

            // Fetch updated user from database
            var updatedUser = await db.Users.FirstAsync(u => u.UserId == 60);

            // Replicate the exact token validation check from Program.cs:
            // CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(stampClaim.ToLowerInvariant()), Encoding.UTF8.GetBytes(account.SecurityStamp.ToString()))
            var stampMatches = System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(
                System.Text.Encoding.UTF8.GetBytes(tokenStampClaim.ToLowerInvariant()),
                System.Text.Encoding.UTF8.GetBytes(updatedUser.SecurityStamp.ToString().ToLowerInvariant()));

            // The old token stamp MUST NOT match the updated database stamp (token is rejected)
            Assert.False(stampMatches);
        }

        #endregion

        #region ATLAS-AUTH-001: Distributed Failed Logins Do Not Block Other IPs

        private sealed class SingleInstanceDbContextFactory<TContext> : IDbContextFactory<TContext>
            where TContext : DbContext
        {
            private readonly DbContextOptions<TContext> _options;
            public SingleInstanceDbContextFactory(DbContextOptions<TContext> options) => _options = options;
            public TContext CreateDbContext() => (TContext)Activator.CreateInstance(typeof(TContext), _options)!;
            public Task<TContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
                => Task.FromResult(CreateDbContext());
        }

        [Fact]
        public async Task ATLAS_AUTH_001_FailedLogins_PartitionedByIp_DoesNotBlockDifferentIp()
        {
            var options = new DbContextOptionsBuilder<AtLASDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            var factory = new SingleInstanceDbContextFactory<AtLASDbContext>(options);
            var service = new SecurityEventService(factory, NullLogger<SecurityEventService>.Instance);

            // Log 5 failed attempts from Attacker IP 192.168.1.100 for academic ID "STUDENT_42"
            for (int i = 0; i < 5; i++)
            {
                await service.LogAsync(new ATLASDomain.InterFaces.SecurityEventEntry
                {
                    EventType = "FailedLogin",
                    AttemptedAcademicId = "STUDENT_42",
                    IpAddress = "192.168.1.100",
                    Severity = "High"
                });
            }

            // Check failure count for attacker IP
            var attackerCount = await service.CountRecentFailedLoginsAsync("STUDENT_42", "192.168.1.100", TimeSpan.FromMinutes(10));
            Assert.Equal(5, attackerCount);

            // Check failure count for legitimate user's IP (e.g. 10.0.0.1)
            var legitCount = await service.CountRecentFailedLoginsAsync("STUDENT_42", "10.0.0.1", TimeSpan.FromMinutes(10));
            Assert.Equal(0, legitCount);
        }

        [Fact]
        public async Task ATLAS_AUTH_001_FailedLogins_NullIp_ExplicitFallback_DoesNotAggregateWithRealIps()
        {
            var options = new DbContextOptionsBuilder<AtLASDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            var factory = new SingleInstanceDbContextFactory<AtLASDbContext>(options);
            var service = new SecurityEventService(factory, NullLogger<SecurityEventService>.Instance);

            // Log 5 failed attempts with "unknown" IP for academic ID "STUDENT_99"
            for (int i = 0; i < 5; i++)
            {
                await service.LogAsync(new ATLASDomain.InterFaces.SecurityEventEntry
                {
                    EventType = "FailedLogin",
                    AttemptedAcademicId = "STUDENT_99",
                    IpAddress = "unknown",
                    Severity = "High"
                });
            }

            // Check count for "unknown"
            var unknownCount = await service.CountRecentFailedLoginsAsync("STUDENT_99", "unknown", TimeSpan.FromMinutes(10));
            Assert.Equal(5, unknownCount);

            // Check count for a legitimate IP "192.168.1.50"
            var realIpCount = await service.CountRecentFailedLoginsAsync("STUDENT_99", "192.168.1.50", TimeSpan.FromMinutes(10));
            Assert.Equal(0, realIpCount);
        }

        [Fact]
        public async Task ATLAS_AUTH_001_DistributedBruteForce_AccumulatesAccountWideCount()
        {
            var options = new DbContextOptionsBuilder<AtLASDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            var factory = new SingleInstanceDbContextFactory<AtLASDbContext>(options);
            var service = new SecurityEventService(factory, NullLogger<SecurityEventService>.Instance);

            // Simulate distributed attack: 12 failed attempts from 6 different IP addresses (2 attempts each)
            for (int ip = 1; ip <= 6; ip++)
            {
                for (int attempt = 0; attempt < 2; attempt++)
                {
                    await service.LogAsync(new ATLASDomain.InterFaces.SecurityEventEntry
                    {
                        EventType = "FailedLogin",
                        AttemptedAcademicId = "VICTIM_ACCOUNT",
                        IpAddress = $"10.0.0.{ip}",
                        Severity = "Low"
                    });
                }
            }

            // Individual IPs only have 2 failures each (below the 5 failure threshold)
            var singleIpCount = await service.CountRecentFailedLoginsAsync("VICTIM_ACCOUNT", "10.0.0.1", TimeSpan.FromMinutes(10));
            Assert.Equal(2, singleIpCount);

            // Account-wide count detects all 12 failures, enabling layered distributed attack detection
            var totalAccountCount = await service.CountRecentFailedLoginsAsync("VICTIM_ACCOUNT", TimeSpan.FromMinutes(10));
            Assert.Equal(12, totalAccountCount);
        }

        #endregion

        #region ATLAS-FUNC-001: Empty Student-Import Workbook Rejection and Controller Contract

        [Fact]
        public async Task ATLAS_FUNC_001_EmptyWorkbook_ThrowsInvalidDataException()
        {
            var tempPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.xlsx");
            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("Students");
                // Header row only, no student records
                ws.Cell(1, 1).Value = "AcademicID";
                ws.Cell(1, 2).Value = "FullName";
                ws.Cell(1, 3).Value = "Institution";
                ws.Cell(1, 4).Value = "Department";
                workbook.SaveAs(tempPath);
            }

            using var db = TestDbContextFactory.Create();
            var uow = CreateUnitOfWork(db);
            var handler = new CreateStudentsRangeCommandHandler(uow);

            var ex = await Assert.ThrowsAsync<InvalidDataException>(() =>
                handler.Handle(new CreateStudentsRangeCommand(tempPath), CancellationToken.None));

            Assert.Equal("The uploaded Excel file contains no student data rows.", ex.Message);
            Assert.False(File.Exists(tempPath)); // Cleaned up in finally
        }

        private static IFormFile CreateDummyExcelFormFile()
        {
            var ms = new MemoryStream();
            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("Students");
                ws.Cell(1, 1).Value = "Header";
                workbook.SaveAs(ms);
            }
            var bytes = ms.ToArray();
            var stream = new MemoryStream(bytes);
            return new FormFile(stream, 0, bytes.Length, "file", "students.xlsx")
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            };
        }

        [Fact]
        public async Task ATLAS_FUNC_001_StudentController_Upload_ReturnsBadRequest_WhenHandlerReturnsFalse()
        {
            var mediatorMock = new Mock<IMediator>();
            mediatorMock.Setup(m => m.Send(It.IsAny<CreateStudentsRangeCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var controller = new StudentController(mediatorMock.Object, Mock.Of<ICourseOfferingAuthorizationService>());
            var result = await controller.CreateStudents(CreateDummyExcelFormFile());

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Failed to process student records from the uploaded file.", badRequest.Value);
        }

        [Fact]
        public async Task ATLAS_FUNC_001_StudentController_Upload_ReturnsOk_WhenHandlerReturnsTrue()
        {
            var mediatorMock = new Mock<IMediator>();
            mediatorMock.Setup(m => m.Send(It.IsAny<CreateStudentsRangeCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var controller = new StudentController(mediatorMock.Object, Mock.Of<ICourseOfferingAuthorizationService>());
            var result = await controller.CreateStudents(CreateDummyExcelFormFile());

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        #endregion
    }
}
