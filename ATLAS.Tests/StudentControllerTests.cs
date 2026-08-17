using ATLAS.Controllers.Student;
using ATLAsBLL.Services.Student.Queries.GetallStudent;
using ATLASDomain.DTO.Student;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace ATLAS.Tests
{
    public class StudentControllerTests
    {
        [Fact]
        public async Task GetAllStudent_ReturnsNotFound_WhenNoStudents()
        {
            var mediatorMock = new Mock<IMediator>();
            mediatorMock.Setup(m => m.Send(It.IsAny<object>(), default))
                .ReturnsAsync((List<StudentDTO>?)null);

            var controller = new StudentController(mediatorMock.Object, Mock.Of<ATLAS.ICourseOfferingAuthorizationService>());

            var result = await controller.GetAllStudent();

            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetAllStudent_ReturnsOk_WhenStudentsExist()
        {
            var mediatorMock = new Mock<IMediator>();

            var list = new List<StudentDTO>
    {
        new StudentDTO { StudentCode = "A1" }
    };

            mediatorMock
                .Setup(m => m.Send(
                    It.IsAny<GetAllStudentsQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(list);

            var controller = new StudentController(
                mediatorMock.Object,
                Mock.Of<ATLAS.ICourseOfferingAuthorizationService>());

            var result = await controller.GetAllStudent();

            var ok = Assert.IsType<OkObjectResult>(result.Result);

            Assert.Equal(list, ok.Value);
        }
        [Fact]
        public async Task GetAllStudentWithLevel_ReturnsBadRequest_OnInvalidLevel()
        {
            var mediatorMock = new Mock<IMediator>();
            var controller = new StudentController(mediatorMock.Object, Mock.Of<ATLAS.ICourseOfferingAuthorizationService>());

            var result = await controller.GetAllStudentWithLevel(0);

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }
    }
}
