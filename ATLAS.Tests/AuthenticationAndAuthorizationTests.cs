using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ATLAS;
using ATLAS.Controllers.Authtenication;
using ATLAsBLL.Services.Auth.Queries;
using ATLASDomain.DTO.Login;
using ATLASDomain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ATLAS.Tests;

public class AuthenticationAndAuthorizationTests
{
    [Fact]
    public async Task StudentLogin_ValidCredentials_ReturnsTokenWithStudentClaims()
    {
        var student = new Student
        {
            StudentId = 17,
            UserId = 42,
            AcademicId = "A-100",
            StudentCode = "S-17",
            AcademicLevel = 3,
            IsActive = true,
            User = new User
            {
                UserId = 42,
                FullName = "Test Student",
                IsActive = true,
                Password = BCrypt.Net.BCrypt.HashPassword("correct-password")
            }
        };

        var services = new ServiceCollection();
        services.AddSingleton<IRequestHandler<GetStudentByAcademicIdQuery, Student?>>(new StudentLookupHandler(student));
        using var provider = services.BuildServiceProvider();
        var controller = new AuthController(
            new Mediator(provider),
            Options.Create(new JwtOptions
            {
                Key = "test-signing-key-must-be-at-least-32-characters",
                Issuer = "ATLAS.Tests",
                Audience = "ATLAS.Tests",
                ExpiryMinutes = 30
            }));

        var result = await controller.StudentLogin(new StudentLoginDTO
        {
            AcademicId = "A-100",
            Password = "correct-password"
        });

        var ok = Assert.IsType<OkObjectResult>(result);
        var token = (string)ok.Value!.GetType().GetProperty("token")!.GetValue(ok.Value)!;
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.Contains(jwt.Claims, x => x.Type == ClaimTypes.NameIdentifier && x.Value == "42");
        Assert.Contains(jwt.Claims, x => x.Type == "StudentId" && x.Value == "17");
        Assert.Contains(jwt.Claims, x => x.Type == ClaimTypes.Role && x.Value == "Student");
        Assert.Contains(jwt.Claims, x => x.Type == "AcademicId" && x.Value == "A-100");
    }

    [Fact]
    public async Task StudentLogin_MissingCredentials_ReturnsBadRequest()
    {
        var controller = new AuthController(
            new Mediator(new ServiceCollection().BuildServiceProvider()),
            Options.Create(new JwtOptions()));

        var result = await controller.StudentLogin(new StudentLoginDTO());

        Assert.IsType<BadRequestObjectResult>(result);
    }

    private sealed class StudentLookupHandler(Student student)
        : IRequestHandler<GetStudentByAcademicIdQuery, Student?>
    {
        public Task<Student?> Handle(GetStudentByAcademicIdQuery request, CancellationToken cancellationToken) =>
            Task.FromResult<Student?>(request.AcademicId == student.AcademicId ? student : null);
    }
}
