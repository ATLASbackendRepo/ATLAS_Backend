using ATLAsBLL.Services.profile.Staff.Query;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace ATLAS.Tests
{
    public class StaffProfilesControllerTests
    {
        [Fact]
        public async Task GetByUserId_ReturnsBadRequest_OnInvalidUserId()
        {
            var mediatorMock = new Mock<IMediator>();
            var controller = new ATLAS.API.Controllers.Profile.StaffProfilesController(mediatorMock.Object);

            var result = await controller.GetByUserId(0);

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetByUserId_ReturnsNotFound_WhenNoProfile()
        {
            var mediatorMock = new Mock<IMediator>();
            mediatorMock.Setup(m => m.Send(It.IsAny<object>(), default)).ReturnsAsync((object?)null);

            var controller = new ATLAS.API.Controllers.Profile.StaffProfilesController(mediatorMock.Object);

            var result = await controller.GetByUserId(1);

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

            var controller =
                new ATLAS.API.Controllers.Profile.StaffProfilesController(
                    mediatorMock.Object);

            var result = await controller.GetByUserId(1);

            var ok = Assert.IsType<OkObjectResult>(result.Result);

            Assert.Equal(dto, ok.Value);
        }
    }
}
