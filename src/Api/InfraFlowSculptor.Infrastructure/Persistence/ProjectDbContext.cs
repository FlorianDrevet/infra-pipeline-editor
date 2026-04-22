using InfraFlowSculptor.Domain.AppConfigurationAggregate;
using InfraFlowSculptor.Domain.AppConfigurationAggregate.Entities;
using InfraFlowSculptor.Domain.AppServicePlanAggregate;
using InfraFlowSculptor.Domain.AppServicePlanAggregate.Entities;
using InfraFlowSculptor.Domain.Common.BaseModels;
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
using InfraFlowSculptor.Domain.SqlServerAggregate;
using InfraFlowSculptor.Domain.SqlServerAggregate.Entities;
using InfraFlowSculptor.Domain.SqlDatabaseAggregate;
using InfraFlowSculptor.Domain.SqlDatabaseAggregate.Entities;
using InfraFlowSculptor.Domain.ServiceBusNamespaceAggregate;
using InfraFlowSculptor.Domain.ServiceBusNamespaceAggregate.Entities;
using InfraFlowSculptor.Domain.ContainerRegistryAggregate;
using InfraFlowSculptor.Domain.ContainerRegistryAggregate.Entities;
using InfraFlowSculptor.Domain.EventHubNamespaceAggregate;
using InfraFlowSculptor.Domain.EventHubNamespaceAggregate.Entities;
using InfraFlowSculptor.Infrastructure.Persistence.Views;
using Microsoft.EntityFrameworkCore;

namespace InfraFlowSculptor.Infrastructure.Persistence;

public class ProjectDbContext(DbContextOptions<ProjectDbContext> options) : DbContext(options)
{
    public DbSet<AzureResource> AzureResources { get; set; } = null!;
    public DbSet<Project> Projects { get; set; } = null!;
    public DbSet<ProjectMember> ProjectMembers { get; set; } = null!;
    public DbSet<ProjectResourceNamingTemplate> ProjectResourceNamingTemplates { get; set; } = null!;
    public DbSet<ProjectResourceAbbreviation> ProjectResourceAbbreviations { get; set; } = null!;
    public DbSet<InfrastructureConfig> InfrastructureConfigs { get; set; } = null!;
    public DbSet<ResourceGroup> ResourceGroups { get; set; } = null!;
    public DbSet<KeyVault> KeyVaults { get; set; } = null!;
    public DbSet<RedisCache> RedisCaches { get; set; } = null!;
    public DbSet<StorageAccount> StorageAccounts { get; set; } = null!;
    public DbSet<BlobContainer> BlobContainers { get; set; } = null!;
    public DbSet<CorsRule> StorageAccountCorsRules { get; set; } = null!;
    public DbSet<BlobLifecycleRule> BlobLifecycleRules { get; set; } = null!;
    public DbSet<StorageQueue> StorageQueues { get; set; } = null!;
    public DbSet<StorageTable> StorageTables { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<ParameterDefinition> ParameterDefinitions { get; set; } = null!;
    public DbSet<ResourceParameterUsage> ResourceParameterUsage { get; set; } = null!;
    public DbSet<InputOutputLink> InputOutputLinks { get; set; } = null!;
    public DbSet<ResourceNamingTemplate> ResourceNamingTemplates { get; set; } = null!;
    public DbSet<ResourceAbbreviationOverride> ResourceAbbreviationOverrides { get; set; } = null!;
    public DbSet<RoleAssignment> RoleAssignments { get; set; } = null!;
    public DbSet<AppSetting> AppSettings { get; set; } = null!;
    public DbSet<AppSettingEnvironmentValue> AppSettingEnvironmentValues { get; set; } = null!;
    public DbSet<SecureParameterMapping> SecureParameterMappings { get; set; } = null!;
    public DbSet<CustomDomain> CustomDomains { get; set; } = null!;
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
    public DbSet<AppConfigurationKey> AppConfigurationKeys { get; set; } = null!;
    public DbSet<AppConfigurationKeyEnvironmentValue> AppConfigurationKeyEnvironmentValues { get; set; } = null!;
    public DbSet<ContainerAppEnvironment> ContainerAppEnvironments { get; set; } = null!;
    public DbSet<ContainerAppEnvironmentEnvironmentSettings> ContainerAppEnvironmentEnvironmentSettings { get; set; } = null!;
    public DbSet<ContainerApp> ContainerApps { get; set; } = null!;
    public DbSet<ContainerAppEnvironmentSettings> ContainerAppEnvironmentSettings { get; set; } = null!;
    public DbSet<LogAnalyticsWorkspace> LogAnalyticsWorkspaces { get; set; } = null!;
    public DbSet<LogAnalyticsWorkspaceEnvironmentSettings> LogAnalyticsWorkspaceEnvironmentSettings { get; set; } = null!;
    public DbSet<ApplicationInsights> ApplicationInsightsResources { get; set; } = null!;
    public DbSet<ApplicationInsightsEnvironmentSettings> ApplicationInsightsEnvironmentSettings { get; set; } = null!;
    public DbSet<Domain.CosmosDbAggregate.CosmosDb> CosmosDbAccounts { get; set; } = null!;
    public DbSet<Domain.CosmosDbAggregate.Entities.CosmosDbEnvironmentSettings> CosmosDbEnvironmentSettings { get; set; } = null!;
    public DbSet<SqlServer> SqlServers { get; set; } = null!;
    public DbSet<SqlServerEnvironmentSettings> SqlServerEnvironmentSettings { get; set; } = null!;
    public DbSet<SqlDatabase> SqlDatabases { get; set; } = null!;
    public DbSet<SqlDatabaseEnvironmentSettings> SqlDatabaseEnvironmentSettings { get; set; } = null!;
    public DbSet<ServiceBusNamespace> ServiceBusNamespaces { get; set; } = null!;
    public DbSet<ServiceBusNamespaceEnvironmentSettings> ServiceBusNamespaceEnvironmentSettings { get; set; } = null!;
    public DbSet<ServiceBusQueue> ServiceBusQueues { get; set; } = null!;
    public DbSet<ServiceBusTopicSubscription> ServiceBusTopicSubscriptions { get; set; } = null!;
    public DbSet<ContainerRegistry> ContainerRegistries { get; set; } = null!;
    public DbSet<ContainerRegistryEnvironmentSettings> ContainerRegistryEnvironmentSettings { get; set; } = null!;
    public DbSet<EventHubNamespace> EventHubNamespaces { get; set; } = null!;
    public DbSet<EventHubNamespaceEnvironmentSettings> EventHubNamespaceEnvironmentSettings { get; set; } = null!;
    public DbSet<Domain.EventHubNamespaceAggregate.Entities.EventHub> EventHubs { get; set; } = null!;
    public DbSet<EventHubConsumerGroup> EventHubConsumerGroups { get; set; } = null!;
    public DbSet<GitRepositoryConfiguration> GitRepositoryConfigurations { get; set; } = null!;
    public DbSet<CrossConfigResourceReference> CrossConfigResourceReferences { get; set; } = null!;
    public DbSet<ProjectPipelineVariableGroup> ProjectPipelineVariableGroups { get; set; } = null!;

    /// <summary>Keyless entity mapped to the <c>vw_ResourceEnvironmentEntries</c> PostgreSQL view.</summary>
    public DbSet<ResourceEnvironmentEntryView> ResourceEnvironmentEntryViews { get; set; } = null!;

    /// <summary>Keyless entity mapped to the <c>vw_ChildToParentLinks</c> PostgreSQL view.</summary>
    public DbSet<ChildToParentLinkView> ChildToParentLinkViews { get; set; } = null!;

    /// <inheritdoc />
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<AzureResource>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property(nameof(AzureResource.ResourceType)).CurrentValue =
                    entry.Entity.GetType().Name;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .ApplyConfigurationsFromAssembly(typeof(ProjectDbContext).Assembly);

        modelBuilder.Entity<ResourceEnvironmentEntryView>(entity =>
        {
            entity.HasNoKey();
            entity.ToView("vw_ResourceEnvironmentEntries");
        });

        modelBuilder.Entity<ChildToParentLinkView>(entity =>
        {
            entity.HasNoKey();
            entity.ToView("vw_ChildToParentLinks");
        });

        base.OnModelCreating(modelBuilder);
    }
}