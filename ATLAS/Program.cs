using ATLAs.API.Middleware;
using ATLAS;
using ATLAS.Claims;
using ATLAS.Middleware;
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
        .Enrich.WithProperty("Application", "ATLAS");

    // MED-10: the Seq URL was hard-coded to http://seq:80 in appsettings.json, so
    // the Seq__ServerUrl variable that docker-compose already sets had no effect
    // and every other environment tried to reach a host named "seq" that does not
    // exist there. It is now bound from configuration, and the sink is attached
    // from code so the value can come from either the Serilog section or the
    // compose variable.
    var seqServerUrl = context.Configuration["Serilog:WriteTo:1:Args:serverUrl"]
        ?? context.Configuration["Seq__ServerUrl"]
        ?? context.Configuration["Seq:ServerUrl"];

    if (!string.IsNullOrWhiteSpace(seqServerUrl))
    {
        // Only add the sink when the configuration actually supplies a URL. An
        // unreachable sink is worse than no sink: Serilog swallows and retries,
        // which slows every write path.
        configuration.WriteTo.Seq(seqServerUrl);
    }
});

// MED-03: the rate limiter partitions by remote IP, and behind a reverse proxy
// that address is the proxy's own. Without ForwardedHeaders every request looks
// like it comes from the same host, so a per-IP limit silently becomes a global
// one and the whole campus shares a single allowance.
builder.Services.Configure<Microsoft.AspNetCore.Builder.ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor |
                               Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto;

    // Only the proxies this deployment actually sits behind may set these
    // headers. Leaving the default "any known network" in place would let a
    // client that can reach Kestrel directly spoof its own address.
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();

    var configuredProxies = builder.Configuration
        .GetSection("ForwardedHeaders:KnownProxies")
        .Get<string[]>() ?? Array.Empty<string>();

    foreach (var proxy in configuredProxies)
    {
        if (System.Net.IPAddress.TryParse(proxy, out var address))
            options.KnownProxies.Add(address);
    }

    // Default to loopback proxies when no explicit list is configured so external clients cannot spoof X-Forwarded-For headers (HIGH-01).
    if (options.KnownProxies.Count == 0)
    {
        options.KnownProxies.Add(System.Net.IPAddress.Loopback);
        options.KnownProxies.Add(System.Net.IPAddress.IPv6Loopback);
    }
});

// ======================================================
// Services
// ======================================================

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddScoped
   <ICourseOfferingAuthorizationService,
    CourseOfferingAuthorizationService>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<ATLAS.Attendance.IAttendanceService, ATLAS.Attendance.AttendanceService>();

// MED-17: attendance scans are only accepted from a device the server has paired.
// The options are validated on start so a misconfigured lifetime or an
// accidentally short key is caught at boot rather than on the first scan.
builder.Services.AddOptions<ATLAS.Attendance.DeviceBindingOptions>()
    .Bind(builder.Configuration.GetSection(ATLAS.Attendance.DeviceBindingOptions.SectionName))
    .Validate(
        o => string.IsNullOrWhiteSpace(o.Key) || o.Key.Length >= 64,
        "Attendance:DeviceBinding:Key must be empty (derived from Jwt:Key) or at least 64 characters long.")
    .Validate(
        o => o.PairingTokenLifetimeDays is > 0 and <= 730,
        "Attendance:DeviceBinding:PairingTokenLifetimeDays must be between 1 and 730.")
    .Validate(
        o => o.MaxDevicesPerStudent is >= 1 and <= 20,
        "Attendance:DeviceBinding:MaxDevicesPerStudent must be between 1 and 20.")
    .ValidateOnStart();
builder.Services.AddScoped<ATLAS.Attendance.IDeviceBindingService, ATLAS.Attendance.DeviceBindingService>();

// ======================================================
// JWT Options
// ======================================================

builder.Services.AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
    .Validate(
        o => !string.IsNullOrWhiteSpace(o.Key) && o.Key.Length >= 64 &&
             !string.Equals(o.Key, "YOUR_SUPER_SECRET_KEY_THAT_IS_AT_LEAST_32_CHARACTERS_LONG!", StringComparison.Ordinal),
        "Jwt:Key must be supplied through a secure configuration provider, must not be the sample value, and must be at least 64 characters long.")
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
            .WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>())
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
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),

                ValidateIssuer = true,
                ValidIssuer = jwtOptions.Issuer,

                ValidateAudience = true,
                ValidAudience = jwtOptions.Audience,

                ValidateLifetime = true,

                ClockSkew = TimeSpan.Zero
            };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                // MED-01 needs a scoped DbContext to read the account, and the JWT
                // middleware runs before the request scope the endpoint would get,
                // so a scope factory is captured here and used per request.
                var stampValidationScopeFactory =
                    context.HttpContext.RequestServices
                        .GetRequiredService<IServiceScopeFactory>();
                // A token issued for the mandatory first password change is
                // only accepted by the endpoints that complete that change.
                var restricted = context.Principal?
                    .HasClaim(AtlasClaims.PasswordChangeOnly, "true") == true;

                if (restricted)
                {
                    var path = context.HttpContext.Request.Path;

                    var allowed =
                        path.StartsWithSegments("/api/Auth/ChangePassword") ||
                        path.StartsWithSegments("/api/Auth/logout");

                    if (!allowed)
                    {
                        context.Fail(
                            "This token may only be used to change the password.");
                    }

                    return;
                }

                // MED-01: the signature proves the token was issued by this server
                // and has not expired, but it says nothing about the account since.
                // A token captured before a logout, a deactivation or a role change
                // would otherwise keep working until it expired, because none of
                // those events can alter an already-signed token. The security
                // stamp is what closes that window: it is minted into the token at
                // login, it is compared against the live row on every request, and
                // it is rotated whenever the account's state changes in a way that
                // must take effect immediately.
                var stampClaim = context.Principal?.FindFirst(AtlasClaims.SecurityStamp)?.Value;

                if (string.IsNullOrWhiteSpace(stampClaim))
                {
                    // A token without a stamp predates the column, so its validity
                    // cannot be re-checked. Refusing it forces a re-login rather
                    // than granting an unverifiable lifetime.
                    context.Fail("The token carries no security stamp.");
                    return;
                }

                var callerId = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);

                if (!int.TryParse(callerId, out var userId) || userId <= 0)
                {
                    context.Fail("The token carries no usable account identifier.");
                    return;
                }

                // The stamp and the active flag are read straight from the
                // database rather than from a cache: a cache would reintroduce the
                // very window MED-01 exists to close, because a deactivated account
                // would stay valid for as long as the entry lived. One indexed
                // primary-key lookup per request is a fair price for that.
                using var stampScope = stampValidationScopeFactory.CreateScope();

                var users = stampScope.ServiceProvider
                    .GetRequiredService<ATLASDomain.Interfaces.Repositories.IUserRepository>();

                var account = await users.GetByUserIdAsync(userId);

                if (account is null)
                {
                    context.Fail("The account no longer exists.");
                    return;
                }

                // A deactivated account must lose access on the very next request,
                // not when its token happens to expire.
                if (!account.IsActive)
                {
                    context.Fail("The account has been deactivated.");
                    return;
                }

                // Constant-time comparison so a token cannot be used as an oracle to
                // recover the stamp value and forge a matching token.
                if (!System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(
                        Encoding.UTF8.GetBytes(stampClaim.ToLowerInvariant()),
                        Encoding.UTF8.GetBytes(account.SecurityStamp.ToString())))
                {
                    context.Fail("The token has been invalidated.");
                    return;
                }

                return;
            }
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
    // DEVICE PAIRING
    // ==================================================

    // MED-17: pairing is what mints a device secret, so it is the one attendance
    // endpoint worth rate limiting tightly. A student only pairs a handful of
    // devices in their time at the institution.
    options.AddPolicy("DevicePairPolicy", httpContext =>
    {
        var userId =
            httpContext.User
                .FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? httpContext.User
                .FindFirst("sub")?.Value;

        var partitionKey =
            !string.IsNullOrEmpty(userId)
                ? $"device-pair:user:{userId}"
                : $"device-pair:ip:{httpContext.Connection.RemoteIpAddress}";

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
    // ADMIN - CREATE RANGE
    // ==================================================

    options.AddPolicy("AdminCreateRangePolicy", httpContext =>
    {
        var userId =
            httpContext.User
                .FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? httpContext.User
                .FindFirst("sub")?.Value;

        var partitionKey =
            !string.IsNullOrEmpty(userId)
                ? $"admin-create-range:user:{userId}"
                : $"admin-create-range:ip:{httpContext.Connection.RemoteIpAddress}";

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
    // ADMIN - UPDATE
    // ==================================================

    options.AddPolicy("AdminUpdatePolicy", httpContext =>
    {
        var userId =
            httpContext.User
                .FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? httpContext.User
                .FindFirst("sub")?.Value;

        var partitionKey =
            !string.IsNullOrEmpty(userId)
                ? $"admin-update:user:{userId}"
                : $"admin-update:ip:{httpContext.Connection.RemoteIpAddress}";

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
    // ADMIN - SOFT DELETE
    // ==================================================

    options.AddPolicy("AdminSoftDeletePolicy", httpContext =>
    {
        var userId =
            httpContext.User
                .FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? httpContext.User
                .FindFirst("sub")?.Value;

        var partitionKey =
            !string.IsNullOrEmpty(userId)
                ? $"admin-soft-delete:user:{userId}"
                : $"admin-soft-delete:ip:{httpContext.Connection.RemoteIpAddress}";

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
    // ADMIN - GET ALL
    // ==================================================

    options.AddPolicy("AdminGetAllPolicy", httpContext =>
    {
        var userId =
            httpContext.User
                .FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? httpContext.User
                .FindFirst("sub")?.Value;

        var partitionKey =
            !string.IsNullOrEmpty(userId)
                ? $"admin-get-all:user:{userId}"
                : $"admin-get-all:ip:{httpContext.Connection.RemoteIpAddress}";

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
builder.Services.AddHealthChecks();
builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, SecurityAuditAuthorizationHandler>();


//builder.Services.AddDbContextFactory<AtLASDbContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
//    , ServiceLifetime.Scoped);

// ======================================================
// Discord & Email Alerts
// ======================================================

builder.Services
    .AddOptions<DiscordOptions>()
    .Bind(builder.Configuration.GetSection(DiscordOptions.SectionName))
    .ValidateOnStart();

// MED-11: the typed HttpClient was registered with AddHttpClient, but the
// provider was then re-registered with AddScoped<INotificationProvider, ...>.
// AddScoped does not understand IHttpClientFactory, so it built the provider
// through the container's default HttpClient - which has no timeout at all and
// lives for the whole application lifetime.
//
// That combination is the worst of both: a hung Discord webhook would block the
// alert poller forever, and because there is no timeout the poll loop would never
// advance past that row, so every other pending alert would stall with it.
//
// Both registrations below resolve the provider through IHttpClientFactory, so
// the named client - and its timeout - is actually honoured.
builder.Services
    .AddHttpClient<DiscordNotificationProvider>(client =>
    {
        // The poller runs on a 15-second cadence. A webhook that has not answered
        // within a few seconds is not going to answer at all, and holding the poll
        // open for it delays every other alert.
        client.Timeout = TimeSpan.FromSeconds(10);
    })
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        AutomaticDecompression = System.Net.DecompressionMethods.All
    });

// Resolving the provider from its own typed-client registration is what makes the
// configured HttpClient - and therefore the timeout above - the one actually
// injected. It is registered as transient by AddHttpClient; resolving it from
// within the poller's scope gives one instance per scope.
builder.Services.AddScoped<INotificationProvider>(serviceProvider =>
    serviceProvider.GetRequiredService<DiscordNotificationProvider>());

builder.Services.AddScoped<INotificationProvider, EmailNotificationProvider>();

builder.Services.AddHostedService<AlertBackgroundService>();

// ======================================================
// Build
// ======================================================

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var database = scope.ServiceProvider.GetRequiredService<AtLASDbContext>();
    await database.Database.MigrateAsync();

    // Bootstrap the first administrator.
    // A brand new database has no institution and no department, and the
    // endpoints that create them already require an administrator, so the seed
    // creates both of them itself before it creates the SAdmin account.
    var adminAcademicId = app.Configuration["Seed:FirstAdmin:AcademicId"];
    var adminPassword = app.Configuration["Seed:FirstAdmin:Password"];

    if (!string.IsNullOrWhiteSpace(adminAcademicId) &&
        !string.IsNullOrWhiteSpace(adminPassword) &&
        !await database.Users.AnyAsync(user => user.Role == ATLASDomain.Entities.enUserRole.SAdmin))
    {
        var institutionName =
            app.Configuration["Seed:Institution:Name"] ?? "ATLAS University";

        var departmentName =
            app.Configuration["Seed:Department:Name"] ?? "General";

        var configuredInstitutionId = int.TryParse(
            app.Configuration["Seed:FirstAdmin:InstitutionId"],
            out var institutionId)
            ? institutionId
            : 0;

        var institution = configuredInstitutionId > 0
            ? await database.Institutions.FirstOrDefaultAsync(
                row => row.InstitutionID == configuredInstitutionId)
            : await database.Institutions.FirstOrDefaultAsync(
                row => row.name == institutionName);

        if (institution is null)
        {
            institution = new ATLASDomain.Entities.Institution
            {
                name = institutionName
            };

            database.Institutions.Add(institution);
            await database.SaveChangesAsync();
        }

        var configuredDepartmentId = int.TryParse(
            app.Configuration["Seed:FirstAdmin:DepartmentId"],
            out var departmentId)
            ? departmentId
            : 0;

        var department = configuredDepartmentId > 0
            ? await database.Departments.FirstOrDefaultAsync(
                row => row.DepartmentId == configuredDepartmentId)
            : await database.Departments.FirstOrDefaultAsync(
                row => row.D_Name == departmentName &&
                       row.InstitutionID == institution.InstitutionID);

        if (department is null)
        {
            department = new ATLASDomain.Entities.Department
            {
                D_Name = departmentName,
                InstitutionID = institution.InstitutionID
            };

            database.Departments.Add(department);
            await database.SaveChangesAsync();
        }

        database.Users.Add(new ATLASDomain.Entities.User
        {
            AcademicId = adminAcademicId,
            FullName = app.Configuration["Seed:FirstAdmin:FullName"] ?? "ATLAS Administrator",
            Password = BCrypt.Net.BCrypt.HashPassword(adminPassword),
            Role = ATLASDomain.Entities.enUserRole.SAdmin,
            IsActive = true,
            MustChangePassword = false,
            InstitutionID = institution.InstitutionID,
            DepartmentId = department.DepartmentId
        });
        await database.SaveChangesAsync();
    }
}

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

// First in the pipeline so every failure raised further down is translated
// into one consistent problem response.
app.UseExceptionHandler();

// LOW-04: this must precede HttpsRedirection, CORS and the correlation-id
// middleware. Placed after them, a failure raised by one of those never reaches
// the handler and escapes as an unstructured 500.
app.UseForwardedHeaders();

// LOW-03: the responses advertised the server implementation and carried no
// anti-framing or content-sniffing protection at all. The Server header is
// suppressed by Kestrel's own option below; these are set here so they also
// apply to responses produced by middleware that runs before the endpoints.
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers.Remove("Server");
    context.Response.Headers.Remove("X-Powered-By");
    await next();
});

app.UseHttpsRedirection();

app.UseCors("StudentApiCorsPolicy");

// MED-10: there was no request logging at all, so nothing recorded which
// endpoints were actually being called, how long they took, or their status
// codes. This emits one structured line per request. It sits before the
// correlation-id middleware on purpose so that the correlation id it logs is
// the one the handlers below it also use.
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate =
        "{RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";

    // Health and readiness probes fire every few seconds and would otherwise
    // dominate the log volume and bury real traffic.
    options.GetLevel = (httpContext, _, exception) =>
        exception is not null
            ? Serilog.Events.LogEventLevel.Error
            : httpContext.Request.Path.StartsWithSegments("/healthz") ||
              httpContext.Request.Path.StartsWithSegments("/readyz")
                ? Serilog.Events.LogEventLevel.Verbose
                : Serilog.Events.LogEventLevel.Information;
});

app.UseMiddleware<CorrelationIdMiddleware>();

app.UseAuthentication();

app.UseRateLimiter();

app.UseAuthorization();

app.MapHealthChecks("/healthz");
app.MapGet("/readyz", async (AtLASDbContext database, CancellationToken cancellationToken) =>
    await database.Database.CanConnectAsync(cancellationToken)
        ? Results.Ok(new { status = "ready" })
        : Results.StatusCode(StatusCodes.Status503ServiceUnavailable));
app.MapControllers();

// ======================================================
// Run
// ======================================================

app.Run();
