using System.Security.Claims;
using ATLAsBLL.Services.Notifications.Command.MarkAllNotificationsAsRead;
using ATLAsBLL.Services.Notifications.Command.MarkNotificationAsRead;
using ATLAsBLL.Services.Notifications.DTOs;
using ATLAsBLL.Services.Notifications.Queries.GetMyNotifications;
using ATLAsBLL.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using ATLAS.Controllers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ATLAS.Tests
{
    public class NotificationsControllerTests
    {
        private NotificationsController CreateControllerWithUser(Mock<IMediator> mediatorMock, int userId)
        {
            var controller = new NotificationsController(mediatorMock.Object);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) }));
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
            return controller;
        }

        [Fact]
        public async Task GetNotifications_ReturnsOk_WhenNotificationsExist()
        {
            var mediatorMock = new Mock<IMediator>();
            var notifications = new List<NotificationDto> { new NotificationDto { NotificationID = 1, Title = "Test" } };
            mediatorMock
                .Setup(m => m.Send(It.IsAny<GetMyNotificationsQuery>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(notifications));

            var controller = CreateControllerWithUser(mediatorMock, 1);

            var result = await controller.GetNotifications(false);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(notifications, ok.Value);
        }

        [Fact]
        public async Task GetNotifications_ReturnsNotFound_WhenNullReturned()
        {
            var mediatorMock = new Mock<IMediator>();
            mediatorMock
                .Setup(m => m.Send(It.IsAny<GetMyNotificationsQuery>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<List<NotificationDto>?>(null));

            var controller = CreateControllerWithUser(mediatorMock, 1);

            var result = await controller.GetNotifications(false);

            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task MarkAsRead_ReturnsOk_OnSuccess()
        {
            var mediatorMock = new Mock<IMediator>();
            mediatorMock
                .Setup(m => m.Send(It.IsAny<MarkNotificationAsReadCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(0)); // <-- return an int, not Unit

            var controller = CreateControllerWithUser(mediatorMock, 1);

            var result = await controller.MarkAsRead(5);

            var ok = Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task MarkAsRead_ReturnsForbid_OnForbidden()
        {
            var mediatorMock = new Mock<IMediator>();
            mediatorMock
                .Setup(m => m.Send(It.IsAny<MarkNotificationAsReadCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ForbiddenAccessException("Forbidden"));

            var controller = CreateControllerWithUser(mediatorMock, 1);

            var result = await controller.MarkAsRead(5);

            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task MarkAsRead_ReturnsNotFound_OnKeyNotFound()
        {
            var mediatorMock = new Mock<IMediator>();
            mediatorMock
                .Setup(m => m.Send(It.IsAny<MarkNotificationAsReadCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new KeyNotFoundException("not found"));

            var controller = CreateControllerWithUser(mediatorMock, 1);

            var result = await controller.MarkAsRead(5);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task MarkAsRead_ReturnsBadRequest_OnException()
        {
            var mediatorMock = new Mock<IMediator>();
            mediatorMock
                .Setup(m => m.Send(It.IsAny<MarkNotificationAsReadCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new System.Exception("boom"));

            var controller = CreateControllerWithUser(mediatorMock, 1);

            var result = await controller.MarkAsRead(5);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task MarkAllAsRead_ReturnsOk_OnSuccess()
        {
            var mediatorMock = new Mock<IMediator>();
            mediatorMock
                .Setup(m => m.Send(It.IsAny<MarkAllNotificationsAsReadCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(3));

            var controller = CreateControllerWithUser(mediatorMock, 1);

            var result = await controller.MarkAllAsRead();

            var ok = Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task MarkAllAsRead_ReturnsBadRequest_OnException()
        {
            var mediatorMock = new Mock<IMediator>();
            mediatorMock
                .Setup(m => m.Send(It.IsAny<MarkAllNotificationsAsReadCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new System.Exception("boom"));

            var controller = CreateControllerWithUser(mediatorMock, 1);

            var result = await controller.MarkAllAsRead();

            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}
