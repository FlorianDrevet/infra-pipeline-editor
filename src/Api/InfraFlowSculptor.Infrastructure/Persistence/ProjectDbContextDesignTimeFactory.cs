using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace InfraFlowSculptor.Infrastructure.Persistence;

public class ProjectDbContextDesignTimeFactory : IDesignTimeDbContextFactory<ProjectDbContext>
{
    public ProjectDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ProjectDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=infra_design_time;Username=postgres;Password=postgres");
        return new ProjectDbContext(optionsBuilder.Options);
    }
}
