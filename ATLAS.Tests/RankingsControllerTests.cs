using ATLAsBLL.Services.CourseGrade.Queries.GetMySemesterRank;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ATLAS.Tests
{
    public class RankingsControllerTests
    {
        private ATLAS.API.Controllers.RankingsController CreateControllerWithStudent(int studentId, Mock<IMediator>? mediatorMock = null)
        {
            var mediator = mediatorMock?.Object ?? new Mock<IMediator>().Object;
            var controller = new ATLAS.API.Controllers.RankingsController(mediator);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, studentId.ToString()) }));
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };
            return controller;
        }

        [Fact]
        public async Task GetMyRank_ReturnsUnauthorized_WhenNoStudentClaim()
        {
            var controller = new ATLAS.API.Controllers.RankingsController(new Mock<IMediator>().Object);
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

            var result = await controller.GetMyRank( 1, CancellationToken.None);

            Assert.IsType<UnauthorizedResult>(result.Result);
        }

        [Fact]
        public async Task GetMyRank_ReturnsNotFound_WhenNoEnrollmentsFound()
        {
            var controller = CreateControllerWithStudent(2);

            var result = await controller.GetMyRank( 1, CancellationToken.None);

            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetMyRank_ReturnsBadRequest_OnInvalidIds()
        {
            var controller = CreateControllerWithStudent(1);

            var res1 = await controller.GetMyRank(
               0,
                CancellationToken.None);

            Assert.IsType<BadRequestObjectResult>(res1.Result);

            var res2 = await controller.GetMyRank(
                -1,
                CancellationToken.None);

            Assert.IsType<BadRequestObjectResult>(res2.Result);
        }

        [Fact]
        public async Task GetMyRank_ReturnsOk_WhenFound()
        {
            var mediatorMock = new Mock<IMediator>();

            var dto = new ATLASDomain.DTO.CourseGrade.MyRankDto();

            mediatorMock
                .Setup(m => m.Send(
                    It.IsAny<GetMySemesterRankQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);

            var controller = CreateControllerWithStudent(1, mediatorMock);

            var result = await controller.GetMyRank(
                1,
                CancellationToken.None);

            var ok = Assert.IsType<OkObjectResult>(result.Result);

            Assert.Equal(dto, ok.Value);
        }
    }
}
