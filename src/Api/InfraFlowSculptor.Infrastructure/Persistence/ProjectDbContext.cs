using InfraFlowSculptor.Domain.AppConfigurationAggregate;
using InfraFlowSculptor.Domain.AppConfigurationAggregate.Entities;
using InfraFlowSculptor.Domain.AppServicePlanAggregate;
using InfraFlowSculptor.Domain.AppServicePlanAggregate.Entities;
using InfraFlowSculptor.Domain.Common.BaseModels.Entites;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.Entities;
using InfraFlowSculptor.Domain.KeyVaultAggregate;
using InfraFlowSculptor.Domain.KeyVaultAggregate.Entities;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.Entities;
using InfraFlowSculptor.Domain.RedisCacheAggregate;
using InfraFlowSculptor.Domain.RedisCacheAggregate.Entities;
using InfraFlowSculptor.Domain.ResourceGroupAggregate;
using InfraFlowSculptor.Domain.StorageAccountAggregate;
using InfraFlowSculptor.Domain.StorageAccountAggregate.Entities;
using InfraFlowSculptor.Domain.UserAggregate;
using InfraFlowSculptor.Domain.UserAssignedIdentityAggregate;
using InfraFlowSculptor.Domain.WebAppAggregate;
using InfraFlowSculptor.Domain.WebAppAggregate.Entities;
using InfraFlowSculptor.Domain.FunctionAppAggregate;
using InfraFlowSculptor.Domain.FunctionAppAggregate.Entities;
using InfraFlowSculptor.Domain.ContainerAppEnvironmentAggregate;
using InfraFlowSculptor.Domain.ContainerAppEnvironmentAggregate.Entities;
using InfraFlowSculptor.Domain.ContainerAppAggregate;
using InfraFlowSculptor.Domain.ContainerAppAggregate.Entities;
using InfraFlowSculptor.Domain.LogAnalyticsWorkspaceAggregate;
using InfraFlowSculptor.Domain.LogAnalyticsWorkspaceAggregate.Entities;
using InfraFlowSculptor.Domain.ApplicationInsightsAggregate;
using InfraFlowSculptor.Domain.ApplicationInsightsAggregate.Entities;
using Microsoft.EntityFrameworkCore;

namespace InfraFlowSculptor.Infrastructure.Persistence;

public class ProjectDbContext(DbContextOptions<ProjectDbContext> options) : DbContext(options)
{
    public DbSet<Project> Projects { get; set; } = null!;
    public DbSet<ProjectMember> ProjectMembers { get; set; } = null!;
    public DbSet<ProjectResourceNamingTemplate> ProjectResourceNamingTemplates { get; set; } = null!;
    public DbSet<InfrastructureConfig> InfrastructureConfigs { get; set; } = null!;
    public DbSet<ResourceGroup> ResourceGroups { get; set; } = null!;
    public DbSet<KeyVault> KeyVaults { get; set; } = null!;
    public DbSet<RedisCache> RedisCaches { get; set; } = null!;
    public DbSet<StorageAccount> StorageAccounts { get; set; } = null!;
    public DbSet<BlobContainer> BlobContainers { get; set; } = null!;
    public DbSet<StorageQueue> StorageQueues { get; set; } = null!;
    public DbSet<StorageTable> StorageTables { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<ParameterDefinition> ParameterDefinitions { get; set; } = null!;
    public DbSet<ResourceParameterUsage> ResourceParameterUsage { get; set; } = null!;
    public DbSet<InputOutputLink> InputOutputLinks { get; set; } = null!;
    public DbSet<ResourceNamingTemplate> ResourceNamingTemplates { get; set; } = null!;
    public DbSet<RoleAssignment> RoleAssignments { get; set; } = null!;
    public DbSet<KeyVaultEnvironmentSettings> KeyVaultEnvironmentSettings { get; set; } = null!;
    public DbSet<RedisCacheEnvironmentSettings> RedisCacheEnvironmentSettings { get; set; } = null!;
    public DbSet<StorageAccountEnvironmentSettings> StorageAccountEnvironmentSettings { get; set; } = null!;
    public DbSet<AppServicePlan> AppServicePlans { get; set; } = null!;
    public DbSet<AppServicePlanEnvironmentSettings> AppServicePlanEnvironmentSettings { get; set; } = null!;
    public DbSet<WebApp> WebApps { get; set; } = null!;
    public DbSet<WebAppEnvironmentSettings> WebAppEnvironmentSettings { get; set; } = null!;
    public DbSet<FunctionApp> FunctionApps { get; set; } = null!;
    public DbSet<FunctionAppEnvironmentSettings> FunctionAppEnvironmentSettings { get; set; } = null!;
    public DbSet<UserAssignedIdentity> UserAssignedIdentities { get; set; } = null!;
    public DbSet<AppConfiguration> AppConfigurations { get; set; } = null!;
    public DbSet<AppConfigurationEnvironmentSettings> AppConfigurationEnvironmentSettings { get; set; } = null!;
    public DbSet<ContainerAppEnvironment> ContainerAppEnvironments { get; set; } = null!;
    public DbSet<ContainerAppEnvironmentEnvironmentSettings> ContainerAppEnvironmentEnvironmentSettings { get; set; } = null!;
    public DbSet<ContainerApp> ContainerApps { get; set; } = null!;
    public DbSet<ContainerAppEnvironmentSettings> ContainerAppEnvironmentSettings { get; set; } = null!;
    public DbSet<LogAnalyticsWorkspace> LogAnalyticsWorkspaces { get; set; } = null!;
    public DbSet<LogAnalyticsWorkspaceEnvironmentSettings> LogAnalyticsWorkspaceEnvironmentSettings { get; set; } = null!;
    public DbSet<ApplicationInsights> ApplicationInsightsResources { get; set; } = null!;
    public DbSet<ApplicationInsightsEnvironmentSettings> ApplicationInsightsEnvironmentSettings { get; set; } = null!;


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .ApplyConfigurationsFromAssembly(typeof(ProjectDbContext).Assembly);
        
        base.OnModelCreating(modelBuilder);
    }
}