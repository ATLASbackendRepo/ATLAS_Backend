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
    /// The connection string is only used to select the SQL Server provider
    /// while scaffolding; no database connection is opened at design time.
    /// </summary>
    public class AtLASDbContextDesignTimeFactory : IDesignTimeDbContextFactory<AtLASDbContext>
    {
        /// <summary>
        /// Creates the <see cref="AtLASDbContext"/> instance the EF Core tooling
        /// uses to diff the current model against the migrations snapshot.
        /// </summary>
        /// <param name="args">Arguments passed by the EF Core command-line tool. Not used.</param>
        /// <returns>A <see cref="AtLASDbContext"/> configured for the SQL Server provider.</returns>
        public AtLASDbContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<AtLASDbContext>()
                .UseSqlServer("Server=localhost,1435;Database=ATLAS;User Id=sa;Password=YourStrongPassword123!;TrustServerCertificate=True;")
                .Options;

            return new AtLASDbContext(options);
        }
    }
}
