using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using ATLAS.Controllers.CourseOffering;
using ATLASDomain.DTO.CourseOffring;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using ATLAsBLL.Services.CourseGrade.Queries;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace ATLAS.Tests
{
    public class CourseOfferingControllerTests
    {
        private readonly ILogger<CourseOffringController> _logger;

        private CourseOffringController CreateControllerWithStudent(
          Mock<IMediator> mediatorMock,
          int studentId)
        {
            var controller = new CourseOffringController(mediatorMock.Object, _logger);

            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new[]
                    {
                new Claim(ClaimTypes.NameIdentifier, studentId.ToString())
                    }));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = user
                }
            };

            return controller;
        }

        [Fact]
        public async Task GetStudentMyCourses_ReturnsUnauthorized_WhenNoStudentClaim()
        {
            var mediatorMock = new Mock<IMediator>();

            var controller = new CourseOffringController(mediatorMock.Object, _logger);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var result = await controller.GetStudentMyCourses();

            Assert.IsType<UnauthorizedResult>(result.Result);
        }

        [Fact]
        public async Task GetStudentMyCourses_ReturnsNotFound_WhenNoCoursesFound()
        {
            var mediatorMock = new Mock<IMediator>();

            var controller = CreateControllerWithStudent(
                mediatorMock,
                2);

            var result = await controller.GetStudentMyCourses();

            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

   
        [Fact]
        public async Task GetStudentMyCourses_ReturnsOk_WhenCoursesExist()
        {
            var mediatorMock = new Mock<IMediator>();

            var list = new List<CourseOfferingDTO>
    {
        new CourseOfferingDTO()
    };

            mediatorMock
                .Setup(m => m.Send(
                    It.IsAny<ATLAsBLL.Services.CourseOffering.Queries.GetmyCourse.Student.GetMyCourseQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(list);

            var controller = CreateControllerWithStudent(
                mediatorMock,
                1);

            var result = await controller.GetStudentMyCourses();

            var ok = Assert.IsType<OkObjectResult>(result.Result);

            Assert.Equal(list, ok.Value);
        }
    }
}
