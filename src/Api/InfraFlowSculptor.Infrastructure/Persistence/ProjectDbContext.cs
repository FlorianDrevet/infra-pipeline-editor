using InfraFlowSculptor.Domain.Common.BaseModels.Entites;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.Entities;
using InfraFlowSculptor.Domain.KeyVaultAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.Entities;
using InfraFlowSculptor.Domain.RedisCacheAggregate;
using InfraFlowSculptor.Domain.ResourceGroupAggregate;
using InfraFlowSculptor.Domain.StorageAccountAggregate;
using InfraFlowSculptor.Domain.StorageAccountAggregate.Entities;
using InfraFlowSculptor.Domain.UserAggregate;
using Microsoft.EntityFrameworkCore;

namespace InfraFlowSculptor.Infrastructure.Persistence;

public class ProjectDbContext(DbContextOptions<ProjectDbContext> options) : DbContext(options)
{
    public DbSet<InfrastructureConfig> InfrastructureConfigs { get; set; } = null!;
    public DbSet<ResourceGroup> ResourceGroups { get; set; } = null!;
    public DbSet<KeyVault> KeyVaults { get; set; } = null!;
    public DbSet<RedisCache> RedisCaches { get; set; } = null!;
    public DbSet<StorageAccount> StorageAccounts { get; set; } = null!;
    public DbSet<BlobContainer> BlobContainers { get; set; } = null!;
    public DbSet<StorageQueue> StorageQueues { get; set; } = null!;
    public DbSet<StorageTable> StorageTables { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Member> Members { get; set; } = null!;
    public DbSet<ParameterDefinition> ParameterDefinitions { get; set; } = null!;
    public DbSet<ResourceParameterUsage> ResourceParameterUsage { get; set; } = null!;
    public DbSet<InputOutputLink> InputOutputLinks { get; set; } = null!;
    public DbSet<ResourceNamingTemplate> ResourceNamingTemplates { get; set; } = null!;
    public DbSet<RoleAssignment> RoleAssignments { get; set; } = null!;
    public DbSet<Project> Projects { get; set; } = null!;
    public DbSet<ProjectMember> ProjectMembers { get; set; } = null!;


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .ApplyConfigurationsFromAssembly(typeof(ProjectDbContext).Assembly);
        
        base.OnModelCreating(modelBuilder);
    }
}