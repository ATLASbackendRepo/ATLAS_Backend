using ATLAs.API.Middleware;
using ATLAS;
using ATLAS.Middleware.Alert;
using ATLAsBLL;
using ATLAsBLL.Behaviors;
using ATLAsBLL.Behaviour;
using ATLAsBLL.Services;
using ATLAsBLL.Services.Auth;
using ATLAsDCL.Data;
using ATLAsDCL.DependencyInjection;
using ATLASDomain.InterFaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using DALExt = ATLAsDCL.DependencyInjection.ServiceCollectionExtensions;

var builder = WebApplication.CreateBuilder(args);

// ======================================================
// Serilog (Logging)
// ======================================================
builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "ATLAS")
        .WriteTo.Console()
        .WriteTo.Seq("http://localhost:5341");
});

// ======================================================
// Services
// ======================================================

builder.Services.AddControllers();

builder.Services.AddScoped
   <ICourseOfferingAuthorizationService,
    CourseOfferingAuthorizationService>();

// ======================================================
// JWT Options
// ======================================================

builder.Services.AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
    .Validate(
        o => !string.IsNullOrWhiteSpace(o.Key),
        "Jwt:Key must be supplied through a secure configuration provider.")
    .Validate(
        o => !string.IsNullOrWhiteSpace(o.Issuer),
        "Jwt:Issuer is required.")
    .Validate(
        o => !string.IsNullOrWhiteSpace(o.Audience),
        "Jwt:Audience is required.")
    .Validate(
        o => o.ExpiryMinutes > 0,
        "Jwt:ExpiryMinutes must be greater than zero.")
    .ValidateOnStart();

// ======================================================
// Swagger
// ======================================================

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {your JWT token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ======================================================
// Business Logic & DAL
// ======================================================

builder.Services.AddBusinessLogicLayer();

DALExt.AddDAL(
    builder.Services,
    builder.Configuration);

// ======================================================
// JWT Configuration
// ======================================================

var jwtOptions =
    builder.Configuration
        .GetSection(JwtOptions.SectionName)
        .Get<JwtOptions>()
    ?? throw new InvalidOperationException(
        "JWT configuration is missing.");

// ======================================================
// CORS
// ======================================================

builder.Services.AddCors(options =>
{
    options.AddPolicy("StudentApiCorsPolicy", policy =>
    {
        policy
            .WithOrigins(
                "https://localhost:7133",
                "http://localhost:5056")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// ======================================================
// Authentication
// ======================================================

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            "ATLAS-Super-Secret-Key-2026-For-JWT-Signing!")), // ⚠️ لسه Hardcoded - متفق إننا مش بنلمسها دلوقتي

                ValidateIssuer = true,
                ValidIssuer = jwtOptions.Issuer,

                ValidateAudience = true,
                ValidAudience = jwtOptions.Audience,

                ValidateLifetime = true,

                ClockSkew = TimeSpan.Zero
            };
    });

// ======================================================
// Rate Limiting
// ======================================================

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode =
        StatusCodes.Status429TooManyRequests;

    // ==================================================
    // GLOBAL RATE LIMITER
    // ==================================================

    options.GlobalLimiter =
        PartitionedRateLimiter.Create<HttpContext, string>(
            httpContext =>
            {
                var userId =
                    httpContext.User
                        .FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? httpContext.User
                        .FindFirst("sub")?.Value;

                var partitionKey =
                    !string.IsNullOrEmpty(userId)
                        ? $"user:{userId}"
                        : $"ip:{httpContext.Connection.RemoteIpAddress}";

                var permitLimit =
                    !string.IsNullOrEmpty(userId)
                        ? 100
                        : 20;

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey,
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = permitLimit,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    });
            });

    // ==================================================
    // AUTH
    // ==================================================

    options.AddPolicy("AuthLimiter", httpContext =>
    {
        var ip =
            httpContext.Connection.RemoteIpAddress?.ToString()
            ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(
            $"auth:ip:{ip}",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 3,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            });
    });

    // ==================================================
    // MESSAGES
    // ==================================================

    options.AddPolicy("MessagesPolicy", httpContext =>
    {
        var userId =
            httpContext.User
                .FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? httpContext.User
                .FindFirst("sub")?.Value;

        var partitionKey =
            !string.IsNullOrEmpty(userId)
                ? $"messages:user:{userId}"
                : $"messages:ip:{httpContext.Connection.RemoteIpAddress}";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            });
    });

    // ==================================================
    // NOTIFICATIONS
    // ==================================================

    options.AddPolicy("NotificationsPolicy", httpContext =>
    {
        var userId =
            httpContext.User
                .FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? httpContext.User
                .FindFirst("sub")?.Value;

        var partitionKey =
            !string.IsNullOrEmpty(userId)
                ? $"notif:user:{userId}"
                : $"notif:ip:{httpContext.Connection.RemoteIpAddress}";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 30,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            });
    });

    // ==================================================
    // NOTIFICATIONS - READ ALL
    // ==================================================

    options.AddPolicy("NotificationsReadAllPolicy", httpContext =>
    {
        var userId =
            httpContext.User
                .FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? httpContext.User
                .FindFirst("sub")?.Value;

        var partitionKey =
            !string.IsNullOrEmpty(userId)
                ? $"notif-read-all:user:{userId}"
                : $"notif-read-all:ip:{httpContext.Connection.RemoteIpAddress}";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            });
    });

    // ==================================================
    // COURSE QUERY
    // ==================================================

    options.AddPolicy("CourseQueryPolicy", httpContext =>
    {
        var userId =
            httpContext.User
                .FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? httpContext.User
                .FindFirst("sub")?.Value;

        var partitionKey =
            !string.IsNullOrEmpty(userId)
                ? $"courses:user:{userId}"
                : $"courses:ip:{httpContext.Connection.RemoteIpAddress}";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 30,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            });
    });

    // ==================================================
    // COURSE DETAILS
    // ==================================================

    options.AddPolicy("CourseDetailsPolicy", httpContext =>
    {
        var userId =
            httpContext.User
                .FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? httpContext.User
                .FindFirst("sub")?.Value;

        var partitionKey =
            !string.IsNullOrEmpty(userId)
                ? $"course-details:user:{userId}"
                : $"course-details:ip:{httpContext.Connection.RemoteIpAddress}";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 15,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            });
    });

    // ==================================================
    // ANALYSIS
    // ==================================================

    options.AddPolicy("AnalysisPolicy", httpContext =>
    {
        var userId =
            httpContext.User
                .FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? httpContext.User
                .FindFirst("sub")?.Value;

        var partitionKey =
            !string.IsNullOrEmpty(userId)
                ? $"analysis:user:{userId}"
                : $"analysis:ip:{httpContext.Connection.RemoteIpAddress}";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            });
    });

    // ==================================================
    // REQUEST WRITE
    // ==================================================

    options.AddPolicy("RequestWritePolicy", httpContext =>
    {
        var userId =
            httpContext.User
                .FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? httpContext.User
                .FindFirst("sub")?.Value;

        var partitionKey =
            !string.IsNullOrEmpty(userId)
                ? $"req-write:user:{userId}"
                : $"req-write:ip:{httpContext.Connection.RemoteIpAddress}";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            });
    });

    // ==================================================
    // REQUEST READ
    // ==================================================

    options.AddPolicy("RequestReadPolicy", httpContext =>
    {
        var userId =
            httpContext.User
                .FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? httpContext.User
                .FindFirst("sub")?.Value;

        var partitionKey =
            !string.IsNullOrEmpty(userId)
                ? $"req-read:user:{userId}"
                : $"req-read:ip:{httpContext.Connection.RemoteIpAddress}";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 30,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            });
    });

    // ==================================================
    // COURSE GRADE READ
    // ==================================================

    options.AddPolicy("CourseGradeReadPolicy", httpContext =>
    {
        var userId =
            httpContext.User
                .FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? httpContext.User
                .FindFirst("sub")?.Value;

        var partitionKey =
            !string.IsNullOrEmpty(userId)
                ? $"grades-read:user:{userId}"
                : $"grades-read:ip:{httpContext.Connection.RemoteIpAddress}";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 20,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            });
    });

    // ==================================================
    // COURSE GRADE ASSIGN
    // ==================================================

    options.AddPolicy("CourseGradeAssignPolicy", httpContext =>
    {
        var userId =
            httpContext.User
                .FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? httpContext.User
                .FindFirst("sub")?.Value;

        var partitionKey =
            !string.IsNullOrEmpty(userId)
                ? $"grades-assign:user:{userId}"
                : $"grades-assign:ip:{httpContext.Connection.RemoteIpAddress}";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            });
    });

    // ==================================================
    // RANKINGS
    // ==================================================

    options.AddPolicy("RankingsPolicy", httpContext =>
    {
        var userId =
            httpContext.User
                .FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? httpContext.User
                .FindFirst("sub")?.Value;

        var partitionKey =
            !string.IsNullOrEmpty(userId)
                ? $"rankings:user:{userId}"
                : $"rankings:ip:{httpContext.Connection.RemoteIpAddress}";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 20,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            });
    });

    // ==================================================
    // PROFILE READ
    // ==================================================

    options.AddPolicy("ProfileReadPolicy", httpContext =>
    {
        var userId =
            httpContext.User
                .FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? httpContext.User
                .FindFirst("sub")?.Value;

        var partitionKey =
            !string.IsNullOrEmpty(userId)
                ? $"profile-read:user:{userId}"
                : $"profile-read:ip:{httpContext.Connection.RemoteIpAddress}";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 15,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            });
    });

    // ==================================================
    // PROFILE UPDATE
    // ==================================================

    options.AddPolicy("ProfileUpdatePolicy", httpContext =>
    {
        var userId =
            httpContext.User
                .FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? httpContext.User
                .FindFirst("sub")?.Value;

        var partitionKey =
            !string.IsNullOrEmpty(userId)
                ? $"profile-update:user:{userId}"
                : $"profile-update:ip:{httpContext.Connection.RemoteIpAddress}";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 9,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            });
    });

    // ==================================================
    // REJECTION RESPONSE
    // ==================================================

    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode =
            StatusCodes.Status429TooManyRequests;

        context.HttpContext.Response.ContentType =
            "application/json";

        await context.HttpContext.Response.WriteAsJsonAsync(
            new
            {
                statusCode = 429,
                message = "Too many requests. Please try again later."
            },
            cancellationToken);
    };
});


// ======================================================
// Audit and Correlation ID Services
// ======================================================

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICorrelationIdAccessor, CorrelationIdAccessor>();
//builder.Services.AddScoped<IAuditService, AuditService>();
//builder.Services.AddScoped<ISecurityEventService, SecurityEventService>(); // ← كانت ناقصة، ده اللي كان بيكسر أي Login فاشل
builder.Services.AddAuthorization();
builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, SecurityAuditAuthorizationHandler>();


builder.Services.AddDbContextFactory<AtLASDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
    , ServiceLifetime.Scoped);


// ======================================================
// Discord & Email Alerts
// ======================================================

builder.Services
    .AddOptions<DiscordOptions>()
    .Bind(builder.Configuration.GetSection(DiscordOptions.SectionName))
    .ValidateOnStart();

builder.Services.AddHttpClient<DiscordNotificationProvider>();

builder.Services.AddScoped<INotificationProvider, DiscordNotificationProvider>();
builder.Services.AddScoped<INotificationProvider, EmailNotificationProvider>();

builder.Services.AddHostedService<AlertBackgroundService>();

// ======================================================
// Build
// ======================================================

var app = builder.Build();

// ======================================================
// Swagger
// ======================================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ======================================================
// Middleware Pipeline
// ======================================================

app.UseHttpsRedirection();

app.UseCors("StudentApiCorsPolicy");

app.UseMiddleware<CorrelationIdMiddleware>();

app.UseAuthentication();

app.UseRateLimiter();

app.UseAuthorization();

app.MapControllers();

// ======================================================
// Run
// ======================================================

app.Run();
