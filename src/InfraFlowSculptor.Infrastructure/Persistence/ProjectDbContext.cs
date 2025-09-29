using InfraFlowSculptor.Domain.InfrastructureConfigAggregate;
using InfraFlowSculptor.Domain.KeyVaultAggregate;
using InfraFlowSculptor.Domain.ResourceGroupAggregate;
using Microsoft.EntityFrameworkCore;
using InfraFlowSculptor.Domain.UserAggregate;

namespace InfraFlowSculptor.Infrastructure.Persistence;

public class ProjectDbContext(DbContextOptions<ProjectDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<InfrastructureConfig> InfrastructureConfigs { get; set; } = null!;
    public DbSet<ResourceGroup> ResourceGroups { get; set; } = null!;
    public DbSet<KeyVault> KeyVaults { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .ApplyConfigurationsFromAssembly(typeof(ProjectDbContext).Assembly);
        
        base.OnModelCreating(modelBuilder);
    }
}