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
        .WriteTo.Console();
});

// ======================================================
// Services
// ======================================================

builder.Services.AddControllers();
builder.Services.AddProblemDetails();

builder.Services.AddScoped
   <ICourseOfferingAuthorizationService,
    CourseOfferingAuthorizationService>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<ATLAS.Attendance.IAttendanceService, ATLAS.Attendance.AttendanceService>();

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

builder.Services.AddHttpClient<DiscordNotificationProvider>();

builder.Services.AddScoped<INotificationProvider, DiscordNotificationProvider>();
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

app.UseHttpsRedirection();

app.UseCors("StudentApiCorsPolicy");

app.UseMiddleware<CorrelationIdMiddleware>();

app.UseExceptionHandler();

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
