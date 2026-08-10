using ATLAsDCL.Data;
using Microsoft.EntityFrameworkCore;

namespace ATLAS.Tests;

internal static class TestDbContextFactory
{
    public static AtLASDbContext Create()
    {
        var options = new DbContextOptionsBuilder<AtLASDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AtLASDbContext(options);
    }
}
