using InfraFlowSculptor.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InfraFlowSculptor.Infrastructure.Tests.Persistence.Repositories;

/// <summary>
/// Builds isolated <see cref="ProjectDbContext"/> instances backed by the EF Core in-memory provider.
/// Each call returns a context bound to a unique database so tests are fully isolated.
/// </summary>
internal static class InMemoryDbContextFactory
{
    /// <summary>Creates a new <see cref="ProjectDbContext"/> with a unique in-memory database.</summary>
    public static ProjectDbContext Create()
    {
        var options = new DbContextOptionsBuilder<ProjectDbContext>()
            .UseInMemoryDatabase(databaseName: $"test_{Guid.NewGuid()}")
            .ConfigureWarnings(b => b.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new ProjectDbContext(options);
    }
}
