using Xunit;
using ATLAsDCL.Data;
using ATLAsDCL.Services;
using ATLASDomain.Entities;
using ATLAS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ATLAS.Tests;

/// <summary>
/// Unit tests for CourseAccessService authorization logic.
/// </summary>
public class CourseAccessServiceTests
{
    private AtLASDbContext CreateDbContext()
    {
        return TestDbContextFactory.Create();
    }

    [Fact]
    public async Task CanSendAsync_DoctorAssignedToCourse_ReturnsTrue()
    {
        // Arrange
        using var context = CreateDbContext();
        var courseAccessService = new CourseAccessService(context);

        var doctor = new User { UserId = 1, FullName = "Dr. Smith", IsActive = true };
        var semester = new Semester { SemesterID = 1, SemesterName = "Spring 2024" };
        var course = new Course { CourseID = 1, C_Name = "Math 101", Credit_Hours = 3 };
        var courseOffering = new CourseOffering { CourseOfferingID = 1, CourseID = 1, SemesterID = 1 };
        var courseOfferingDoctor = new CourseOfferingDoctor { CourseOfferingID = 1, DoctorID = 1 };

        context.Users.Add(doctor);
        context.Semesters.Add(semester);
        context.Courses.Add(course);
        context.CourseOfferings.Add(courseOffering);
        context.CourseOfferingDoctors.Add(courseOfferingDoctor);
        await context.SaveChangesAsync();

        // Act
        var result = await courseAccessService.CanSendAsync(1, 1);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task CanSendAsync_TAAssignedToCourse_ReturnsTrue()
    {
        // Arrange
        using var context = CreateDbContext();
        var courseAccessService = new CourseAccessService(context);

        var ta = new User { UserId = 2, FullName = "TA Smith", IsActive = true };
        var semester = new Semester { SemesterID = 2, SemesterName = "Spring 2024" };
        var course = new Course { CourseID = 2, C_Name = "Physics 101", Credit_Hours = 3 };
        var courseOffering = new CourseOffering { CourseOfferingID = 2, CourseID = 2, SemesterID = 2 };
        var courseOfferingTA = new CourseOfferingTA { CourseOfferingID = 2, TAID = 2 };

        context.Users.Add(ta);
        context.Semesters.Add(semester);
        context.Courses.Add(course);
        context.CourseOfferings.Add(courseOffering);
        context.CourseOfferingTAs.Add(courseOfferingTA);
        await context.SaveChangesAsync();

        // Act
        var result = await courseAccessService.CanSendAsync(2, 2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task CanSendAsync_StudentNotAssigned_ReturnsFalse()
    {
        // Arrange
        using var context = CreateDbContext();
        var courseAccessService = new CourseAccessService(context);

        var student = new User { UserId = 3, FullName = "Student John", IsActive = true };
        var semester = new Semester { SemesterID = 3, SemesterName = "Spring 2024" };
        var course = new Course { CourseID = 3, C_Name = "Chemistry 101", Credit_Hours = 3 };
        var courseOffering = new CourseOffering { CourseOfferingID = 3, CourseID = 3, SemesterID = 3 };
        var studentEntity = new Student { StudentId = 1, UserId = 3, StudentCode = "A50" };
        var enrollment = new Enrollment { EnrollmentID = 1, StudentID = 1, CourseOfferingID = 3 };

        context.Users.Add(student);
        context.Semesters.Add(semester);
        context.Courses.Add(course);
        context.CourseOfferings.Add(courseOffering);
        context.Students.Add(studentEntity);
        context.Enrollments.Add(enrollment);
        await context.SaveChangesAsync();

        // Act
        var result = await courseAccessService.CanSendAsync(3, 3);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CanReadAsync_DoctorAssignedToCourse_ReturnsTrue()
    {
        // Arrange
        using var context = CreateDbContext();
        var courseAccessService = new CourseAccessService(context);

        var doctor = new User { UserId = 4, FullName = "Dr. Johnson", IsActive = true };
        var semester = new Semester { SemesterID = 4, SemesterName = "Spring 2024" };
        var course = new Course { CourseID = 4, C_Name = "History 101", Credit_Hours = 3 };
        var courseOffering = new CourseOffering { CourseOfferingID = 4, CourseID = 4, SemesterID = 4 };
        var courseOfferingDoctor = new CourseOfferingDoctor { CourseOfferingID = 4, DoctorID = 4 };

        context.Users.Add(doctor);
        context.Semesters.Add(semester);
        context.Courses.Add(course);
        context.CourseOfferings.Add(courseOffering);
        context.CourseOfferingDoctors.Add(courseOfferingDoctor);
        await context.SaveChangesAsync();

        // Act
        var result = await courseAccessService.CanReadAsync(4, 4);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task CanReadAsync_TAAssignedToCourse_ReturnsTrue()
    {
        // Arrange
        using var context = CreateDbContext();
        var courseAccessService = new CourseAccessService(context);

        var ta = new User { UserId = 5, FullName = "TA Johnson", IsActive = true };
        var semester = new Semester { SemesterID = 5, SemesterName = "Spring 2024" };
        var course = new Course { CourseID = 5, C_Name = "Art 101", Credit_Hours = 3 };
        var courseOffering = new CourseOffering { CourseOfferingID = 5, CourseID = 5, SemesterID = 5 };
        var courseOfferingTA = new CourseOfferingTA { CourseOfferingID = 5, TAID = 5 };

        context.Users.Add(ta);
        context.Semesters.Add(semester);
        context.Courses.Add(course);
        context.CourseOfferings.Add(courseOffering);
        context.CourseOfferingTAs.Add(courseOfferingTA);
        await context.SaveChangesAsync();

        // Act
        var result = await courseAccessService.CanReadAsync(5, 5);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task CanReadAsync_EnrolledStudentInCourse_ReturnsTrue()
    {
        // Arrange
        using var context = CreateDbContext();
        var courseAccessService = new CourseAccessService(context);

        var student = new User { UserId = 6, FullName = "Student Jane", IsActive = true };
        var semester = new Semester { SemesterID = 6, SemesterName = "Spring 2024" };
        var course = new Course { CourseID = 6, C_Name = "Biology 101", Credit_Hours = 3 };
        var courseOffering = new CourseOffering { CourseOfferingID = 6, CourseID = 6, SemesterID = 6 };
        var studentEntity = new Student { StudentId = 2, UserId = 6, StudentCode = "STU002" };
        var enrollment = new Enrollment { EnrollmentID = 2, StudentID = 2, CourseOfferingID = 6 };

        context.Users.Add(student);
        context.Semesters.Add(semester);
        context.Courses.Add(course);
        context.CourseOfferings.Add(courseOffering);
        context.Students.Add(studentEntity);
        context.Enrollments.Add(enrollment);
        await context.SaveChangesAsync();

        // Act
        var result = await courseAccessService.CanReadAsync(6, 6);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task CanReadAsync_NonEnrolledStudent_ReturnsFalse()
    {
        // Arrange
        using var context = CreateDbContext();
        var courseAccessService = new CourseAccessService(context);

        var student1 = new User { UserId = 7, FullName = "Student NoAccess", IsActive = true };
        var student2 = new User { UserId = 8, FullName = "Student Enrolled", IsActive = true };
        var semester = new Semester { SemesterID = 7, SemesterName = "Spring 2024" };
        var course = new Course { CourseID = 7, C_Name = "Geography 101", Credit_Hours = 3 };
        var courseOffering = new CourseOffering { CourseOfferingID = 7, CourseID = 7, SemesterID = 7 };
        var student1Entity = new Student { StudentId = 3, UserId = 7, StudentCode = "STU003" };
        var student2Entity = new Student { StudentId = 4, UserId = 8, StudentCode = "STU004" };
        var enrollment = new Enrollment { EnrollmentID = 3, StudentID    = 4, CourseOfferingID = 7 };

        context.Users.Add(student1);
        context.Users.Add(student2);
        context.Semesters.Add(semester);
        context.Courses.Add(course);
        context.CourseOfferings.Add(courseOffering);
        context.Students.Add(student1Entity);
        context.Students.Add(student2Entity);
        context.Enrollments.Add(enrollment);
        await context.SaveChangesAsync();

        // Act - student1 (UserID 7) is not enrolled
        var result = await courseAccessService.CanReadAsync(7, 7);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CanReadAsync_UserNotExist_ReturnsFalse()
    {
        // Arrange
        using var context = CreateDbContext();
        var courseAccessService = new CourseAccessService(context);

        var semester = new Semester { SemesterID = 8, SemesterName = "Spring 2024" };
        var course = new Course { CourseID = 8, C_Name = "English 101", Credit_Hours = 3 };
        var courseOffering = new CourseOffering { CourseOfferingID = 8, CourseID = 8, SemesterID = 8 };

        context.Semesters.Add(semester);
        context.Courses.Add(course);
        context.CourseOfferings.Add(courseOffering);
        await context.SaveChangesAsync();

        // Act - non-existent user
        var result = await courseAccessService.CanReadAsync(999, 8);

        // Assert
        Assert.False(result);
    }
}
