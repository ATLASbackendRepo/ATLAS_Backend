using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using ATLASDomain.Entities;
using ATLAsBLL.Exceptions;
using ATLAsBLL.Features.CourseOfferingDoctors.Commands.CreateCourseOfferingDoctor;
using ATLAsBLL.Features.CourseOfferingTAs.Commands.CreateCourseOfferingTA;
using ATLAsDCL.UnitOfWork;
using Moq;
using Xunit;

namespace ATLAS.Tests
{
    public class CourseOfferingDoctorTaAssignmentTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ICurrentUserService> _currentUserServiceMock;

        public CourseOfferingDoctorTaAssignmentTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _currentUserServiceMock = new Mock<ICurrentUserService>();
        }

        [Fact]
        public async Task AssignDoctor_CrossInstitutionOffering_ThrowsForbiddenAccessException()
        {
            // Caller is Admin of Institution 1
            _currentUserServiceMock.Setup(s => s.IsSAdmin).Returns(false);
            _currentUserServiceMock.Setup(s => s.InstitutionId).Returns(1);

            // Offering belongs to Institution 2
            var offering = new CourseOffering { CourseOfferingID = 10, InstitutionID = 2 };
            _unitOfWorkMock.Setup(u => u.CourseOfferings.GetByIdAsync(10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(offering);

            var handler = new CreateCourseOfferingDoctorCommandHandler(_unitOfWorkMock.Object, _currentUserServiceMock.Object);
            var command = new CreateCourseOfferingDoctorCommand(10, 5);

            await Assert.ThrowsAsync<ForbiddenAccessException>(() => handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task AssignDoctor_OfferingWithNullInstitution_NonSAdmin_ThrowsForbiddenAccessException()
        {
            // Caller is Admin of Institution 1
            _currentUserServiceMock.Setup(s => s.IsSAdmin).Returns(false);
            _currentUserServiceMock.Setup(s => s.InstitutionId).Returns(1);

            // Offering has null InstitutionID
            var offering = new CourseOffering { CourseOfferingID = 10, InstitutionID = null };
            _unitOfWorkMock.Setup(u => u.CourseOfferings.GetByIdAsync(10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(offering);

            var handler = new CreateCourseOfferingDoctorCommandHandler(_unitOfWorkMock.Object, _currentUserServiceMock.Object);
            var command = new CreateCourseOfferingDoctorCommand(10, 5);

            await Assert.ThrowsAsync<ForbiddenAccessException>(() => handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task AssignDoctor_TargetUserNotDoctor_ThrowsArgumentException()
        {
            _currentUserServiceMock.Setup(s => s.IsSAdmin).Returns(false);
            _currentUserServiceMock.Setup(s => s.InstitutionId).Returns(1);

            var offering = new CourseOffering { CourseOfferingID = 10, InstitutionID = 1 };
            _unitOfWorkMock.Setup(u => u.CourseOfferings.GetByIdAsync(10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(offering);

            // Target user is a Student
            var studentUser = new User
            {
                UserId = 5,
                AcademicId = "ST001",
                FullName = "Student John",
                Role = enUserRole.Student,
                IsActive = true,
                InstitutionID = 1
            };
            _unitOfWorkMock.Setup(u => u.Users.GetByIdAsync(5, It.IsAny<CancellationToken>()))
                .ReturnsAsync(studentUser);

            var handler = new CreateCourseOfferingDoctorCommandHandler(_unitOfWorkMock.Object, _currentUserServiceMock.Object);
            var command = new CreateCourseOfferingDoctorCommand(10, 5);

            var ex = await Assert.ThrowsAsync<ArgumentException>(() => handler.Handle(command, CancellationToken.None));
            Assert.Contains("does not have the Doctor role", ex.Message);
        }

        [Fact]
        public async Task AssignDoctor_InactiveUser_ThrowsArgumentException()
        {
            _currentUserServiceMock.Setup(s => s.IsSAdmin).Returns(false);
            _currentUserServiceMock.Setup(s => s.InstitutionId).Returns(1);

            var offering = new CourseOffering { CourseOfferingID = 10, InstitutionID = 1 };
            _unitOfWorkMock.Setup(u => u.CourseOfferings.GetByIdAsync(10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(offering);

            // Target user is Doctor but Inactive
            var inactiveDoctor = new User
            {
                UserId = 5,
                AcademicId = "DR001",
                FullName = "Dr. Inactive",
                Role = enUserRole.Doctor,
                IsActive = false,
                InstitutionID = 1
            };
            _unitOfWorkMock.Setup(u => u.Users.GetByIdAsync(5, It.IsAny<CancellationToken>()))
                .ReturnsAsync(inactiveDoctor);

            var handler = new CreateCourseOfferingDoctorCommandHandler(_unitOfWorkMock.Object, _currentUserServiceMock.Object);
            var command = new CreateCourseOfferingDoctorCommand(10, 5);

            var ex = await Assert.ThrowsAsync<ArgumentException>(() => handler.Handle(command, CancellationToken.None));
            Assert.Contains("inactive", ex.Message);
        }

        [Fact]
        public async Task AssignDoctor_DoctorBelongsToDifferentInstitution_ThrowsForbiddenAccessException()
        {
            _currentUserServiceMock.Setup(s => s.IsSAdmin).Returns(false);
            _currentUserServiceMock.Setup(s => s.InstitutionId).Returns(1);

            var offering = new CourseOffering { CourseOfferingID = 10, InstitutionID = 1 };
            _unitOfWorkMock.Setup(u => u.CourseOfferings.GetByIdAsync(10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(offering);

            // Doctor belongs to Institution 2
            var doctorInst2 = new User
            {
                UserId = 5,
                AcademicId = "DR002",
                FullName = "Dr. External",
                Role = enUserRole.Doctor,
                IsActive = true,
                InstitutionID = 2
            };
            _unitOfWorkMock.Setup(u => u.Users.GetByIdAsync(5, It.IsAny<CancellationToken>()))
                .ReturnsAsync(doctorInst2);

            var handler = new CreateCourseOfferingDoctorCommandHandler(_unitOfWorkMock.Object, _currentUserServiceMock.Object);
            var command = new CreateCourseOfferingDoctorCommand(10, 5);

            await Assert.ThrowsAsync<ForbiddenAccessException>(() => handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task AssignDoctor_DuplicateAssignment_ThrowsInvalidOperationException()
        {
            _currentUserServiceMock.Setup(s => s.IsSAdmin).Returns(false);
            _currentUserServiceMock.Setup(s => s.InstitutionId).Returns(1);

            var offering = new CourseOffering { CourseOfferingID = 10, InstitutionID = 1 };
            _unitOfWorkMock.Setup(u => u.CourseOfferings.GetByIdAsync(10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(offering);

            var doctor = new User
            {
                UserId = 5,
                AcademicId = "DR001",
                FullName = "Dr. Same",
                Role = enUserRole.Doctor,
                IsActive = true,
                InstitutionID = 1
            };
            _unitOfWorkMock.Setup(u => u.Users.GetByIdAsync(5, It.IsAny<CancellationToken>()))
                .ReturnsAsync(doctor);

            // Already assigned
            var existing = new List<CourseOfferingDoctor>
            {
                new CourseOfferingDoctor { CourseOfferingDoctorID = 1, CourseOfferingID = 10, DoctorID = 5 }
            };
            _unitOfWorkMock.Setup(u => u.CourseOfferingDoctors.FindAsync(
                It.IsAny<Expression<Func<CourseOfferingDoctor, bool>>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(existing);

            var handler = new CreateCourseOfferingDoctorCommandHandler(_unitOfWorkMock.Object, _currentUserServiceMock.Object);
            var command = new CreateCourseOfferingDoctorCommand(10, 5);

            await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task AssignDoctor_ValidSameInstitution_Succeeds()
        {
            _currentUserServiceMock.Setup(s => s.IsSAdmin).Returns(false);
            _currentUserServiceMock.Setup(s => s.InstitutionId).Returns(1);

            var offering = new CourseOffering { CourseOfferingID = 10, InstitutionID = 1 };
            _unitOfWorkMock.Setup(u => u.CourseOfferings.GetByIdAsync(10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(offering);

            var doctor = new User
            {
                UserId = 5,
                AcademicId = "DR001",
                FullName = "Dr. Same",
                Role = enUserRole.Doctor,
                IsActive = true,
                InstitutionID = 1
            };
            _unitOfWorkMock.Setup(u => u.Users.GetByIdAsync(5, It.IsAny<CancellationToken>()))
                .ReturnsAsync(doctor);

            _unitOfWorkMock.Setup(u => u.CourseOfferingDoctors.FindAsync(
                It.IsAny<Expression<Func<CourseOfferingDoctor, bool>>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CourseOfferingDoctor>());

            _unitOfWorkMock.Setup(u => u.CourseOfferingDoctors.AddAsync(
                It.IsAny<CourseOfferingDoctor>(), It.IsAny<CancellationToken>()))
                .Callback<CourseOfferingDoctor, CancellationToken>((cod, ct) => cod.CourseOfferingDoctorID = 42)
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var handler = new CreateCourseOfferingDoctorCommandHandler(_unitOfWorkMock.Object, _currentUserServiceMock.Object);
            var command = new CreateCourseOfferingDoctorCommand(10, 5);

            var result = await handler.Handle(command, CancellationToken.None);

            Assert.Equal(42, result);
        }

        [Fact]
        public async Task AssignTA_CrossInstitution_ThrowsForbiddenAccessException()
        {
            _currentUserServiceMock.Setup(s => s.IsSAdmin).Returns(false);
            _currentUserServiceMock.Setup(s => s.InstitutionId).Returns(1);

            var offering = new CourseOffering { CourseOfferingID = 10, InstitutionID = 2 };
            _unitOfWorkMock.Setup(u => u.CourseOfferings.GetByIdAsync(10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(offering);

            var handler = new CreateCourseOfferingTACommandHandler(_unitOfWorkMock.Object, _currentUserServiceMock.Object);
            var command = new CreateCourseOfferingTACommand(10, 8);

            await Assert.ThrowsAsync<ForbiddenAccessException>(() => handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task AssignTA_TargetUserNotTA_ThrowsArgumentException()
        {
            _currentUserServiceMock.Setup(s => s.IsSAdmin).Returns(false);
            _currentUserServiceMock.Setup(s => s.InstitutionId).Returns(1);

            var offering = new CourseOffering { CourseOfferingID = 10, InstitutionID = 1 };
            _unitOfWorkMock.Setup(u => u.CourseOfferings.GetByIdAsync(10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(offering);

            // User is a Doctor, not a TA
            var doctorUser = new User
            {
                UserId = 8,
                AcademicId = "DR008",
                FullName = "Dr. NotTA",
                Role = enUserRole.Doctor,
                IsActive = true,
                InstitutionID = 1
            };
            _unitOfWorkMock.Setup(u => u.Users.GetByIdAsync(8, It.IsAny<CancellationToken>()))
                .ReturnsAsync(doctorUser);

            var handler = new CreateCourseOfferingTACommandHandler(_unitOfWorkMock.Object, _currentUserServiceMock.Object);
            var command = new CreateCourseOfferingTACommand(10, 8);

            var ex = await Assert.ThrowsAsync<ArgumentException>(() => handler.Handle(command, CancellationToken.None));
            Assert.Contains("does not have the TeachingAssistant role", ex.Message);
        }

        [Fact]
        public async Task AssignTA_ValidSameInstitution_Succeeds()
        {
            _currentUserServiceMock.Setup(s => s.IsSAdmin).Returns(false);
            _currentUserServiceMock.Setup(s => s.InstitutionId).Returns(1);

            var offering = new CourseOffering { CourseOfferingID = 10, InstitutionID = 1 };
            _unitOfWorkMock.Setup(u => u.CourseOfferings.GetByIdAsync(10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(offering);

            var ta = new User
            {
                UserId = 8,
                AcademicId = "TA001",
                FullName = "TA Jane",
                Role = enUserRole.TeachingAssistant,
                IsActive = true,
                InstitutionID = 1
            };
            _unitOfWorkMock.Setup(u => u.Users.GetByIdAsync(8, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ta);

            _unitOfWorkMock.Setup(u => u.CourseOfferingTAs.FindAsync(
                It.IsAny<Expression<Func<CourseOfferingTA, bool>>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CourseOfferingTA>());

            _unitOfWorkMock.Setup(u => u.CourseOfferingTAs.AddAsync(
                It.IsAny<CourseOfferingTA>(), It.IsAny<CancellationToken>()))
                .Callback<CourseOfferingTA, CancellationToken>((cot, ct) => cot.CourseOfferingTAID = 99)
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var handler = new CreateCourseOfferingTACommandHandler(_unitOfWorkMock.Object, _currentUserServiceMock.Object);
            var command = new CreateCourseOfferingTACommand(10, 8);

            var result = await handler.Handle(command, CancellationToken.None);

            Assert.Equal(99, result);
        }

        [Fact]
        public void Validators_EnforcePositiveIds()
        {
            var doctorValidator = new CreateCourseOfferingDoctorCommandValidator();
            var taValidator = new CreateCourseOfferingTACommandValidator();

            var invalidDoctorCmd = new CreateCourseOfferingDoctorCommand(0, -1);
            var doctorResult = doctorValidator.Validate(invalidDoctorCmd);
            Assert.False(doctorResult.IsValid);
            Assert.Equal(2, doctorResult.Errors.Count);

            var invalidTaCmd = new CreateCourseOfferingTACommand(-5, 0);
            var taResult = taValidator.Validate(invalidTaCmd);
            Assert.False(taResult.IsValid);
            Assert.Equal(2, taResult.Errors.Count);
        }
    }
}

