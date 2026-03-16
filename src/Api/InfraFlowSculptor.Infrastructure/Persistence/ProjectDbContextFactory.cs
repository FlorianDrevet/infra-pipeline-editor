using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace InfraFlowSculptor.Infrastructure.Persistence;

/// <summary>
/// Design-time factory used by EF Core tooling (migrations) when no running host is available.
/// </summary>
public class ProjectDbContextFactory : IDesignTimeDbContextFactory<ProjectDbContext>
{
    public ProjectDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ProjectDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=infraflow_design;Username=postgres;Password=postgres");
        return new ProjectDbContext(optionsBuilder.Options);
    }
}
