using System.Security.Claims;
using ATLAsBLL.Exceptions;
using ATLAsBLL.Services.Notifications.Command.MarkAllNotificationsAsRead;
using ATLAsBLL.Services.Notifications.Command.MarkNotificationAsRead;
using ATLAsBLL.Services.Notifications.DTOs;
using ATLAsBLL.Services.Notifications.Queries.GetMyNotifications;
using ATLAS.Controllers;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace ATLAS.Tests
{
    public class NotificationsControllerTests
    {
        private NotificationsController CreateControllerWithUser(
            Mock<IMediator> mediatorMock,
            int userId)
        {
            var controller = new NotificationsController(mediatorMock.Object);

            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new[]
                    {
                        new Claim(
                            ClaimTypes.NameIdentifier,
                            userId.ToString())
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
        public async Task GetNotifications_ReturnsOk_WhenNotificationsExist()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>();

            var notifications = new List<NotificationDto>
            {
                new NotificationDto
                {
                    NotificationID = 1,
                    Title = "Test"
                }
            };

            mediatorMock
                .Setup(m => m.Send(
                    It.IsAny<GetMyNotificationsQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(notifications);

            var controller = CreateControllerWithUser(mediatorMock, 1);

            // Act
            var result = await controller.GetNotifications(false);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);

            Assert.Equal(notifications, ok.Value);
        }

        [Fact]
        public async Task GetNotifications_ReturnsOk_WhenNoNotificationsExist()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>();

            mediatorMock
                .Setup(m => m.Send(
                    It.IsAny<GetMyNotificationsQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<NotificationDto>());

            var controller = CreateControllerWithUser(mediatorMock, 1);

            // Act
            var result = await controller.GetNotifications(false);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);

            var notifications =
                Assert.IsType<List<NotificationDto>>(ok.Value);

            Assert.Empty(notifications);
        }

        [Fact]
        public async Task MarkAsRead_ReturnsOk_OnSuccess()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>();

            mediatorMock
                .Setup(m => m.Send(
                    It.IsAny<MarkNotificationAsReadCommand>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(0);

            var controller = CreateControllerWithUser(mediatorMock, 1);

            // Act
            var result = await controller.MarkAsRead(5);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task MarkAsRead_ReturnsForbid_OnForbidden()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>();

            mediatorMock
                .Setup(m => m.Send(
                    It.IsAny<MarkNotificationAsReadCommand>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(
                    new ForbiddenAccessException("Forbidden"));

            var controller = CreateControllerWithUser(mediatorMock, 1);

            // Act
            var result = await controller.MarkAsRead(5);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task MarkAsRead_ReturnsNotFound_OnKeyNotFound()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>();

            mediatorMock
                .Setup(m => m.Send(
                    It.IsAny<MarkNotificationAsReadCommand>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(
                    new KeyNotFoundException("not found"));

            var controller = CreateControllerWithUser(mediatorMock, 1);

            // Act
            var result = await controller.MarkAsRead(5);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task MarkAsRead_ReturnsProblemDetails_OnUnexpectedException()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>();

            mediatorMock
                .Setup(m => m.Send(
                    It.IsAny<MarkNotificationAsReadCommand>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(
                    new System.Exception("boom"));

            var controller = CreateControllerWithUser(mediatorMock, 1);

            // Act
            var result = await controller.MarkAsRead(5);

            // Assert
            var problem = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, problem.StatusCode);
        }

        [Fact]
        public async Task MarkAllAsRead_ReturnsOk_OnSuccess()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>();

            mediatorMock
                .Setup(m => m.Send(
                    It.IsAny<MarkAllNotificationsAsReadCommand>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(3);

            var controller = CreateControllerWithUser(mediatorMock, 1);

            // Act
            var result = await controller.MarkAllAsRead();

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task MarkAllAsRead_ReturnsProblemDetails_OnUnexpectedException()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>();

            mediatorMock
                .Setup(m => m.Send(
                    It.IsAny<MarkAllNotificationsAsReadCommand>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(
                    new System.Exception("boom"));

            var controller = CreateControllerWithUser(mediatorMock, 1);

            // Act
            var result = await controller.MarkAllAsRead();

            // Assert
            var problem = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, problem.StatusCode);
        }
    }
}

