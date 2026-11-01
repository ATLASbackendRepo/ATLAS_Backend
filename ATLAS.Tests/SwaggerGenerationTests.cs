using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using Moq;
using Swashbuckle.AspNetCore.Swagger;
using System.IO;
using Xunit;

namespace ATLAS.Tests
{
    public class SwaggerGenerationTests
    {
        [Fact]
        public void ExportLiveSwaggerJson()
        {
            var services = new ServiceCollection();

            var hostEnv = new Mock<IHostEnvironment>();
            hostEnv.Setup(e => e.EnvironmentName).Returns("Development");
            hostEnv.Setup(e => e.ApplicationName).Returns("ATLAS");
            services.AddSingleton(hostEnv.Object);

            var webHostEnv = new Mock<IWebHostEnvironment>();
            webHostEnv.Setup(e => e.EnvironmentName).Returns("Development");
            webHostEnv.Setup(e => e.ApplicationName).Returns("ATLAS");
            services.AddSingleton(webHostEnv.Object);

            services.AddLogging();
            services.AddControllers()
                .AddApplicationPart(typeof(ATLAS.Controllers.Student.StudentController).Assembly);
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
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
                        System.Array.Empty<string>()
                    }
                });
            });

            var sp = services.BuildServiceProvider();
            var swaggerProvider = sp.GetRequiredService<ISwaggerProvider>();
            var doc = swaggerProvider.GetSwagger("v1");

            Assert.NotNull(doc);
            Assert.NotEmpty(doc.Paths);

            using var stringWriter = new StringWriter();
            var jsonWriter = new Microsoft.OpenApi.Writers.OpenApiJsonWriter(stringWriter);
            doc.SerializeAsV3(jsonWriter);
            var json = stringWriter.ToString();

            Assert.False(string.IsNullOrWhiteSpace(json));

            var outputPath = Path.Combine(Directory.GetCurrentDirectory(), "live-swagger.json");
            File.WriteAllText(outputPath, json);

            // Also write to solution root
            var current = new DirectoryInfo(Directory.GetCurrentDirectory());
            while (current != null && !File.Exists(Path.Combine(current.FullName, "ATLAS.slnx")))
            {
                current = current.Parent;
            }
            if (current != null)
            {
                File.WriteAllText(Path.Combine(current.FullName, "live-swagger.json"), json);
            }
        }
    }
}
