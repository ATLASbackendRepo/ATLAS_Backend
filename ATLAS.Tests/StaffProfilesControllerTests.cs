using ATLAsBLL.Services.profile.Staff.Query;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace ATLAS.Tests
{
    public class StaffProfilesControllerTests
    {
        private static ATLAS.API.Controllers.Profile.ProfilesController CreateController(IMediator mediator, int userId = 1)
        {
            var controller = new ATLAS.API.Controllers.Profile.ProfilesController(mediator);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                        [new Claim(ClaimTypes.NameIdentifier, userId.ToString())], "test"))
                }
            };
            return controller;
        }

        [Fact]
        public async Task GetByUserId_ThrowsUnauthorized_OnMissingUserClaim()
        {
            var mediatorMock = new Mock<IMediator>();
            var controller = new ATLAS.API.Controllers.Profile.ProfilesController(mediatorMock.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
            };

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => controller.GetByUserId());
        }

        [Fact]
        public async Task GetByUserId_ReturnsNotFound_WhenNoProfile()
        {
            var mediatorMock = new Mock<IMediator>();
            mediatorMock.Setup(m => m.Send(It.IsAny<object>(), default)).ReturnsAsync((object?)null);

            var controller = CreateController(mediatorMock.Object);

            var result = await controller.GetByUserId();

            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetByUserId_ReturnsOk_WhenProfileFound()
        {
            var mediatorMock = new Mock<IMediator>();

            var dto = new ATLASDomain.DTO.Profile.StaffProfileResponseDto();

            mediatorMock
                .Setup(m => m.Send(
                    It.IsAny<GetStaffProfileByUserIdQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);

            var controller = CreateController(mediatorMock.Object);

            var result = await controller.GetByUserId();

            var ok = Assert.IsType<OkObjectResult>(result.Result);

            Assert.Equal(dto, ok.Value);
        }
    }
}
