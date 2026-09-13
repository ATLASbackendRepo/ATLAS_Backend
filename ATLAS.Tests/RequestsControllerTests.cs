using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ATLAS.Tests
{
    public class RequestsControllerTests
    {
        private ControllerBase CreateControllerWithStudentUser(Moq.Mock<IMediator> mediatorMock, int studentId, out ATLAS.Controllers.Requests.RequestController controller)
        {
            controller = new ATLAS.Controllers.Requests.RequestController(mediatorMock.Object);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, studentId.ToString()) }));
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
            return controller;
        }

        [Fact]
        public async Task Create_ReturnsUnauthorized_WhenNoStudentClaim()
        {
            var mediatorMock = new Mock<IMediator>();
            var controller = new ATLAS.Controllers.Requests.RequestController(mediatorMock.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

            var dto = new ATLASDomain.DTO.Request.CreateGradeAppealRequestDto
            {
                ProblemDescription = "Test problem description"
            };

            var result = await controller.Create(dto, CancellationToken.None);

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task Create_ReturnsCreated_OnSuccess()
        {
            var mediatorMock = new Mock<IMediator>();
            mediatorMock.Setup(m => m.Send(It.IsAny<object>(), It.IsAny<CancellationToken>())).ReturnsAsync(5);

            var controller = new ATLAS.Controllers.Requests.RequestController(mediatorMock.Object);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1") }));
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

            var result = await controller.Create(
                new ATLASDomain.DTO.Request.CreateGradeAppealRequestDto
                {
                    ProblemDescription = "Test problem description"
                },
                CancellationToken.None);

            Assert.IsType<CreatedAtActionResult>(result);
        }

        [Fact]
        public async Task Update_ReturnsUnauthorized_WhenNoDoctorClaim()
        {
            var mediatorMock = new Mock<IMediator>();
            var controller = new ATLAS.Controllers.Requests.RequestController(mediatorMock.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

            var result = await controller.Update(1, new ATLASDomain.DTO.Request.UpdateGradeAppealRequestDto(), CancellationToken.None);

            Assert.IsType<UnauthorizedObjectResult>(result);
        }
        [Fact]
        public async Task Update_ReturnsNoContent_WhenUpdated()
        {
            var mediatorMock = new Mock<IMediator>();

            mediatorMock
                .Setup(m => m.Send(
                    It.IsAny<ATLAsBLL.Services.Request.Command.UpdateGradeAppealRequest.UpdateGradeAppealRequestCommand>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var controller =
                new ATLAS.Controllers.Requests.RequestController(
                    mediatorMock.Object);

            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new[]
                    {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    "1")
                    }));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = user
                }
            };

            var result = await controller.Update(
                1,
                new ATLASDomain.DTO.Request.UpdateGradeAppealRequestDto(),
                CancellationToken.None);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Update_ReturnsNotFound_WhenNotUpdated()
        {
            var mediatorMock = new Mock<IMediator>();
            mediatorMock.Setup(m => m.Send(It.IsAny<object>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var controller = new ATLAS.Controllers.Requests.RequestController(mediatorMock.Object);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1") }));
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

            var result = await controller.Update(1, new ATLASDomain.DTO.Request.UpdateGradeAppealRequestDto(), CancellationToken.None);

            Assert.IsType<NotFoundResult>(result);
        }
    }
}
