using ATLAS;
using ATLAS.Controllers.Authtenication;
using ATLAsBLL.Services.Auth;
using ATLAsBLL.Services.Auth.Queries;
using ATLASDomain.DTO.Login;
using ATLASDomain.Entities;
using ATLASDomain.InterFaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Xunit;

namespace ATLAS.Tests;

public class AuthenticationAndAuthorizationTests
{
    [Fact]
    public async Task StudentLogin_ValidCredentials_ReturnsTokenWithStudentClaims()
    {
        // Arrange
        var user = new User
        {
            UserId = 42,
            AcademicId = "A-100",
            IsActive = true,
            Role = enUserRole.Student,
            FullName = "Test Student",
            Password = BCrypt.Net.BCrypt.HashPassword("correct-password")
        };

        var services = new ServiceCollection();

        services.AddSingleton<IRequestHandler<GetUserByAcademicIdQuery, User?>>(
            new StudentLookupHandler(user));

        // The login path persists the refresh token through this command, so the
        // fake mediator needs a handler for it. Registering a no-op is enough: this
        // test asserts on the claims of the issued token, not on what was stored.
        services.AddSingleton<IRequestHandler<SaveRefreshTokenCommand, bool>>(
            new NoOpSaveRefreshTokenHandler());

        using var provider = services.BuildServiceProvider();

        var mediator = new FakeMediator(provider);

        var jwtOptions = Options.Create(new JwtOptions
        {
            Key = "test-signing-key-must-be-at-least-32-characters",
            Issuer = "ATLAS.Tests",
            Audience = "ATLAS.Tests",
            ExpiryMinutes = 30
        });

        // لو AuthController بيستخدم ISecurityEventService أثناء Login
        // استخدم implementation/fake مناسب للـ interface عندك.
        var securityEventService = default(ISecurityEventService)!;

        var controller = new AuthController(
            mediator,
            jwtOptions,
            securityEventService
        );

        var loginDto = new LoginDTO
        {
            AcademicId = "A-100",
            Password = "correct-password"
        };

        // Act
        var result = await controller.Login(loginDto);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);

        Assert.NotNull(ok.Value);

        var tokenResponse = Assert.IsType<TokenResponse>(ok.Value);
        var token = tokenResponse.AccessToken;

        Assert.False(string.IsNullOrWhiteSpace(token));

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.Contains(
            jwt.Claims,
            x => x.Type == ClaimTypes.NameIdentifier &&
                 x.Value == "42"
        );

        Assert.Contains(
            jwt.Claims,
            x => x.Type == ClaimTypes.Name &&
                 x.Value == "Test Student"
        );

        Assert.Contains(
            jwt.Claims,
            x => x.Type == ClaimTypes.Role &&
                 x.Value == "Student"
        );

        Assert.Contains(
            jwt.Claims,
            x => x.Type == "AcademicId" &&
                 x.Value == "A-100"
        );
    }

    [Fact]
    public async Task StudentLogin_MissingCredentials_ReturnsBadRequest()
    {
        // Arrange
        var services = new ServiceCollection();
        using var provider = services.BuildServiceProvider();

        var mediator = new FakeMediator(provider);

        var jwtOptions = Options.Create(new JwtOptions
        {
            Key = "test-signing-key-must-be-at-least-32-characters",
            Issuer = "ATLAS.Tests",
            Audience = "ATLAS.Tests",
            ExpiryMinutes = 30
        });

        var securityEventService = default(ISecurityEventService)!;

        var controller = new AuthController(
            mediator,
            jwtOptions,
            securityEventService
        );

        var loginDto = new LoginDTO();

        // Act
        var result = await controller.Login(loginDto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    private sealed class NoOpSaveRefreshTokenHandler
        : IRequestHandler<SaveRefreshTokenCommand, bool>
    {
        public Task<bool> Handle(
            SaveRefreshTokenCommand request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }
    }

    private sealed class StudentLookupHandler
        : IRequestHandler<GetUserByAcademicIdQuery, User?>
    {
        private readonly User _user;

        public StudentLookupHandler(User user)
        {
            _user = user;
        }

        public Task<User?> Handle(
            GetUserByAcademicIdQuery request,
            CancellationToken cancellationToken)
        {
            User? result = request.AcademicId == _user.AcademicId
                ? _user
                : null;

            return Task.FromResult(result);
        }
    }

    private sealed class FakeMediator : IMediator
    {
        private readonly IServiceProvider _provider;

        public FakeMediator(IServiceProvider provider)
        {
            _provider = provider;
        }

        public Task<TResponse> Send<TResponse>(
            IRequest<TResponse> request,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var handlerType = typeof(IRequestHandler<,>)
                .MakeGenericType(
                    request.GetType(),
                    typeof(TResponse)
                );

            var handler = _provider.GetService(handlerType);

            if (handler == null)
            {
                throw new InvalidOperationException(
                    $"No handler registered for {request.GetType().Name}"
                );
            }

            var method = handlerType.GetMethod("Handle");

            if (method == null)
            {
                throw new InvalidOperationException(
                    $"Handle method not found on {handlerType.Name}"
                );
            }

            return (Task<TResponse>)method.Invoke(
                handler,
                new object[]
                {
                    request,
                    cancellationToken
                }
            )!;
        }

        public Task Send<TRequest>(
            TRequest request,
            CancellationToken cancellationToken = default)
            where TRequest : IRequest
        {
            return Task.CompletedTask;
        }

        public Task<object?> Send(
            object request,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(
            IStreamRequest<TResponse> request,
            CancellationToken cancellationToken = default)
        {
            return EmptyStream<TResponse>();
        }

        public IAsyncEnumerable<object?> CreateStream(
            object request,
            CancellationToken cancellationToken = default)
        {
            return EmptyStream<object?>();
        }

        public Task Publish(
            object notification,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task Publish<TNotification>(
            TNotification notification,
            CancellationToken cancellationToken = default)
            where TNotification : INotification
        {
            return Task.CompletedTask;
        }

        private static async IAsyncEnumerable<T> EmptyStream<T>()
        {
            await Task.CompletedTask;
            yield break;
        }
    }
}
