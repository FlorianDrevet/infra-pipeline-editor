using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace InfraFlowSculptor.Infrastructure.Persistence;

/// <summary>
/// Design-time factory used exclusively for EF Core tooling (migrations).
/// Not used at runtime.
/// </summary>
public class ProjectDbContextDesignTimeFactory : IDesignTimeDbContextFactory<ProjectDbContext>
{
    public ProjectDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("INFRA_DB_CONNECTION_STRING")
            ?? "Host=localhost;Database=infradb;Username=postgres;Password=postgres";

        var optionsBuilder = new DbContextOptionsBuilder<ProjectDbContext>();
        optionsBuilder.UseNpgsql(connectionString);
        return new ProjectDbContext(optionsBuilder.Options);
    }
}
