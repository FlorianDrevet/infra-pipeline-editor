using Microsoft.EntityFrameworkCore;

namespace BicepGenerator.Infrastructure.Persistence;

public class ProjectDbContext(DbContextOptions<ProjectDbContext> options) : DbContext(options)
{

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .ApplyConfigurationsFromAssembly(typeof(ProjectDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}