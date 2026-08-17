using ATLASAPI.Controllers;
using ATLAsBLL.Services.CourseGrade.Queries;
using ATLASDomain.DTO.CourseGrade;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace ATLAS.Tests
{
    public class CourseGradeControllerTests
    {
        [Fact]
        public async Task GetCourseStudentsPaged_ReturnsBadRequest_OnInvalidParams()
        {
            var mediatorMock = new Mock<IMediator>();
            var courseAuth = Mock.Of<ATLAS.ICourseOfferingAuthorizationService>();
            var controller = new CourseGradeController(mediatorMock.Object, courseAuth);

            var result = await controller.GetCourseStudentsPaged(0);

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetCourseStudentsPaged_ReturnsForbid_WhenNotAuthorized()
        {
            var mediatorMock = new Mock<IMediator>();
            var courseAuthMock = new Mock<ATLAS.ICourseOfferingAuthorizationService>();
            courseAuthMock.Setup(c => c.CanAccessOfferingAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>(), It.IsAny<int>(), default)).ReturnsAsync(false);

            var controller = new CourseGradeController(mediatorMock.Object, courseAuthMock.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

            var result = await controller.GetCourseStudentsPaged(1);

            Assert.IsType<ForbidResult>(result.Result);
        }

        [Fact]
       public async Task GetCourseStudentsPaged_ReturnsOk_WhenAuthorized()
{
    var mediatorMock = new Mock<IMediator>();
    var courseAuthMock = new Mock<ATLAS.ICourseOfferingAuthorizationService>();

    courseAuthMock
        .Setup(c => c.CanAccessOfferingAsync(
            It.IsAny<ClaimsPrincipal>(),
            It.IsAny<int>(),
            default))
        .ReturnsAsync(true);

    var paged = new PagedResultDto<StudentCardDto>
    {
        Items = new List<StudentCardDto>(),
        TotalCount = 0
    };

    mediatorMock
        .Setup(m => m.Send(
            It.IsAny<GetCourseStudentsQuery>(),
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(paged);

    var controller = new CourseGradeController(
        mediatorMock.Object,
        courseAuthMock.Object);

    controller.ControllerContext = new ControllerContext
    {
        HttpContext = new DefaultHttpContext()
    };

    var result = await controller.GetCourseStudentsPaged(1);

    var ok = Assert.IsType<OkObjectResult>(result.Result);

    Assert.Equal(paged, ok.Value);
}
    }
}
