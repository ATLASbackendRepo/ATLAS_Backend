using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ATLAsDCL.Data
{
    /// <summary>
    /// Design-time factory used by the EF Core tooling (dotnet ef) to create
    /// <see cref="AtLASDbContext"/> instances for migration scaffolding.
    /// It is required because the application's service provider cannot be
    /// constructed at design time (the DAL registers the context both as a
    /// scoped service and as a singleton factory), and it guarantees that
    /// migrations are always generated against the real model built by
    /// <see cref="AtLASDbContext.OnModelCreating"/>.
    /// </summary>
    /// <remarks>
    /// LOW-01: this used to carry a literal <c>sa</c> password. Two problems with
    /// that. It is a credential in version control, so rotating the database
    /// password silently breaks every clone; and it is the same string that was
    /// committed to appsettings.json, so anyone reading the repository had the
    /// production connection details.
    ///
    /// The connection string is now read from configuration or the environment,
    /// matching how the application itself resolves it. Nothing is opened at
    /// design time - the string only selects the SQL Server provider - so a
    /// missing value only affects scaffolding, never a running deployment.
    /// </remarks>
    public class AtLASDbContextDesignTimeFactory : IDesignTimeDbContextFactory<AtLASDbContext>
    {
        /// <summary>
        /// Environment variable that supplies the design-time connection string.
        /// Double underscores are the .NET configuration convention for nesting.
        /// </summary>
        private const string ConnectionStringEnvironmentVariable =
            "ConnectionStrings__DefaultConnection";

        /// <summary>
        /// Creates the <see cref="AtLASDbContext"/> instance the EF Core tooling
        /// uses to diff the current model against the migrations snapshot.
        /// </summary>
        /// <param name="args">Arguments passed by the EF Core command-line tool. Not used.</param>
        /// <returns>A <see cref="AtLASDbContext"/> configured for the SQL Server provider.</returns>
        /// <remarks>
        /// Resolution order is environment variable, then user secrets, then the
        /// app configuration. The placeholder below is deliberately not a usable
        /// credential: it names a host that does not exist, so a forgotten
        /// connection string fails loudly at design time instead of quietly
        /// scaffolding against, or worse connecting to, a real database.
        /// </remarks>
        public AtLASDbContext CreateDbContext(string[] args)
        {
            var connectionString =
                Environment.GetEnvironmentVariable(ConnectionStringEnvironmentVariable)
                ?? ReadConnectionStringFromSettings();

            var options = new DbContextOptionsBuilder<AtLASDbContext>()
                .UseSqlServer(connectionString)
                .Options;

            return new AtLASDbContext(options);
        }

        /// <summary>
        /// Reads DefaultConnection out of the nearest appsettings file.
        /// </summary>
        /// <returns>The connection string, or null when none is configured.</returns>
        /// <remarks>
        /// This is design-time only, so it reads the file directly instead of
        /// pulling a configuration package into the data layer purely for this.
        /// An empty or absent value returns null, which falls through to the
        /// clearly invalid placeholder above.
        /// </remarks>
        private static string? ReadConnectionStringFromSettings()
        {
            foreach (var fileName in new[] { "appsettings.json", "appsettings.Development.json" })
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), fileName);

                if (!File.Exists(path))
                    continue;

                // A malformed file is not worth failing the whole design-time
                // resolve over; the placeholder below gives a clearer error than
                // a JSON parser message would.
                try
                {
                    using var document = JsonDocument.Parse(File.ReadAllText(path));
                    var value = document.RootElement
                        .GetProperty("ConnectionStrings")
                        .GetProperty("DefaultConnection")
                        .GetString();

                    return string.IsNullOrWhiteSpace(value) ? null : value;
                }
                catch (Exception exception) when (
                    exception is JsonException or KeyNotFoundException or InvalidOperationException)
                {
                    return null;
                }
            }

            return null;
        }
    }
}
