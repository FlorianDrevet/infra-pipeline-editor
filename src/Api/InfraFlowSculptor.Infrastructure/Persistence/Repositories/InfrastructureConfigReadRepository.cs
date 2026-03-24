using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.InfrastructureConfig.ReadModels;
using InfraFlowSculptor.Domain.AppConfigurationAggregate;
using InfraFlowSculptor.Domain.AppConfigurationAggregate.Entities;
using InfraFlowSculptor.Domain.AppServicePlanAggregate;
using InfraFlowSculptor.Domain.AppServicePlanAggregate.Entities;
using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.KeyVaultAggregate;
using InfraFlowSculptor.Domain.KeyVaultAggregate.Entities;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.RedisCacheAggregate;
using InfraFlowSculptor.Domain.RedisCacheAggregate.Entities;
using InfraFlowSculptor.Domain.StorageAccountAggregate;
using InfraFlowSculptor.Domain.StorageAccountAggregate.Entities;
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
using InfraFlowSculptor.Domain.CosmosDbAggregate;
using InfraFlowSculptor.Domain.CosmosDbAggregate.Entities;
using InfraFlowSculptor.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InfraFlowSculptor.Infrastructure.Persistence.Repositories;

public sealed class InfrastructureConfigReadRepository(ProjectDbContext dbContext)
    : IInfrastructureConfigReadRepository
{
    public async Task<InfrastructureConfigReadModel?> GetByIdWithResourcesAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var configId = new InfrastructureConfigId(id);

        var config = await dbContext.InfrastructureConfigs
            .Include(c => c.ResourceGroups)
                .ThenInclude(rg => rg.Resources)
            .Include(c => c.ResourceNamingTemplates)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == configId, cancellationToken);

        if (config is null)
            return null;

        // Eager-load typed environment settings for each resource type
        var allResourceIds = config.ResourceGroups
            .SelectMany(rg => rg.Resources)
            .Select(r => r.Id)
            .ToList();

        var kvSettings = await dbContext.KeyVaultEnvironmentSettings
            .Where(es => allResourceIds.Contains(es.KeyVaultId))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var rcSettings = await dbContext.RedisCacheEnvironmentSettings
            .Where(es => allResourceIds.Contains(es.RedisCacheId))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var saSettings = await dbContext.StorageAccountEnvironmentSettings
            .Where(es => allResourceIds.Contains(es.StorageAccountId))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var aspSettings = await dbContext.AppServicePlanEnvironmentSettings
            .Where(es => allResourceIds.Contains(es.AppServicePlanId))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var waSettings = await dbContext.WebAppEnvironmentSettings
            .Where(es => allResourceIds.Contains(es.WebAppId))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var faSettings = await dbContext.FunctionAppEnvironmentSettings
            .Where(es => allResourceIds.Contains(es.FunctionAppId))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var acSettings = await dbContext.AppConfigurationEnvironmentSettings
            .Where(es => allResourceIds.Contains(es.AppConfigurationId))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var caeSettings = await dbContext.ContainerAppEnvironmentEnvironmentSettings
            .Where(es => allResourceIds.Contains(es.ContainerAppEnvironmentId))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var caSettings = await dbContext.ContainerAppEnvironmentSettings
            .Where(es => allResourceIds.Contains(es.ContainerAppId))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var lawSettings = await dbContext.LogAnalyticsWorkspaceEnvironmentSettings
            .Where(es => allResourceIds.Contains(es.LogAnalyticsWorkspaceId))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var aiSettings = await dbContext.ApplicationInsightsEnvironmentSettings
            .Where(es => allResourceIds.Contains(es.ApplicationInsightsId))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var cosmosSettings = await dbContext.CosmosDbEnvironmentSettings
            .Where(es => allResourceIds.Contains(es.CosmosDbId))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var resourceGroups = config.ResourceGroups.Select(rg =>
        {
            var resources = rg.Resources
                .Select(r => MapResource(r, kvSettings, rcSettings, saSettings, aspSettings, waSettings, faSettings, acSettings, caeSettings, caSettings, lawSettings, aiSettings, cosmosSettings))
                .OfType<AzureResourceReadModel>()
                .ToList();

            return new ResourceGroupReadModel(
                rg.Id.Value,
                rg.Name.Value,
                MapLocation(rg.Location),
                resources);
        }).ToList();

        // ── Load parent project for environments and naming context ─────────
        var project = await dbContext.Projects
            .Include(p => p.ResourceNamingTemplates)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == config.ProjectId, cancellationToken);

        var environments = BuildEnvironmentList(config, project);
        var namingContext = BuildNamingContext(config, project);

        return new InfrastructureConfigReadModel(
            config.Id.Value,
            config.Name.Value,
            resourceGroups,
            environments,
            namingContext);
    }

    /// <summary>
    /// Resolves the effective environment list based on inheritance settings.
    /// </summary>
    private static List<EnvironmentDefinitionReadModel> BuildEnvironmentList(
        InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig config,
        Project? project)
    {
        if (config.UseProjectEnvironments && project is not null)
        {
            return project.EnvironmentDefinitions.Select(e =>
                new EnvironmentDefinitionReadModel(
                    e.Id.Value,
                    e.Name.Value,
                    MapLocation(e.Location),
                    e.Prefix.Value,
                    e.Suffix.Value)).ToList();
        }

        return config.EnvironmentDefinitions.Select(e =>
            new EnvironmentDefinitionReadModel(
                e.Id.Value,
                e.Name.Value,
                MapLocation(e.Location),
                e.Prefix.Value,
                e.Suffix.Value)).ToList();
    }

    /// <summary>
    /// Resolves the effective naming context based on inheritance settings.
    /// </summary>
    private static NamingContextReadModel BuildNamingContext(
        InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig config,
        Project? project)
    {
        // When using project naming conventions (default), read from project
        if (config.UseProjectNamingConventions && project is not null)
        {
            return new NamingContextReadModel(
                project.DefaultNamingTemplate?.Value,
                project.ResourceNamingTemplates.ToDictionary(
                    t => t.ResourceType,
                    t => t.Template.Value));
        }

        // Otherwise, read from the config itself (if overridden)
        return new NamingContextReadModel(
            config.DefaultNamingTemplate?.Value,
            config.ResourceNamingTemplates.ToDictionary(
                t => t.ResourceType,
                t => t.Template.Value));
    }

    /// <summary>
    /// Maps an <see cref="AzureResource"/> to its read model using typed environment settings.
    /// Returns <c>null</c> for resource types that are not yet supported by the generator.
    /// </summary>
    private static AzureResourceReadModel? MapResource(
        AzureResource resource,
        IReadOnlyList<KeyVaultEnvironmentSettings> kvSettings,
        IReadOnlyList<RedisCacheEnvironmentSettings> rcSettings,
        IReadOnlyList<StorageAccountEnvironmentSettings> saSettings,
        IReadOnlyList<AppServicePlanEnvironmentSettings> aspSettings,
        IReadOnlyList<WebAppEnvironmentSettings> waSettings,
        IReadOnlyList<FunctionAppEnvironmentSettings> faSettings,
        IReadOnlyList<AppConfigurationEnvironmentSettings> acSettings,
        IReadOnlyList<ContainerAppEnvironmentEnvironmentSettings> caeSettings,
        IReadOnlyList<ContainerAppEnvironmentSettings> caSettings,
        IReadOnlyList<LogAnalyticsWorkspaceEnvironmentSettings> lawSettings,
        IReadOnlyList<ApplicationInsightsEnvironmentSettings> aiSettings,
        IReadOnlyList<CosmosDbEnvironmentSettings> cosmosSettings)
    {
        return resource switch
        {
            KeyVault kv => new AzureResourceReadModel(
                kv.Id.Value,
                kv.Name.Value,
                MapLocation(kv.Location),
                "Microsoft.KeyVault/vaults",
                new Dictionary<string, string>(),
                kvSettings
                    .Where(es => es.KeyVaultId == kv.Id)
                    .Select(es => new ResourceEnvironmentConfigReadModel(es.EnvironmentName, es.ToDictionary()))
                    .ToList()),
            RedisCache rc => new AzureResourceReadModel(
                rc.Id.Value,
                rc.Name.Value,
                MapLocation(rc.Location),
                "Microsoft.Cache/Redis",
                new Dictionary<string, string>(),
                rcSettings
                    .Where(es => es.RedisCacheId == rc.Id)
                    .Select(es => new ResourceEnvironmentConfigReadModel(es.EnvironmentName, es.ToDictionary()))
                    .ToList()),
            StorageAccount sa => new AzureResourceReadModel(
                sa.Id.Value,
                sa.Name.Value,
                MapLocation(sa.Location),
                "Microsoft.Storage/storageAccounts",
                new Dictionary<string, string>(),
                saSettings
                    .Where(es => es.StorageAccountId == sa.Id)
                    .Select(es => new ResourceEnvironmentConfigReadModel(es.EnvironmentName, es.ToDictionary()))
                    .ToList()),
            AppServicePlan asp => new AzureResourceReadModel(
                asp.Id.Value,
                asp.Name.Value,
                MapLocation(asp.Location),
                "Microsoft.Web/serverfarms",
                new Dictionary<string, string>
                {
                    ["osType"] = asp.OsType.Value.ToString()
                },
                aspSettings
                    .Where(es => es.AppServicePlanId == asp.Id)
                    .Select(es => new ResourceEnvironmentConfigReadModel(es.EnvironmentName, es.ToDictionary()))
                    .ToList()),
            WebApp wa => new AzureResourceReadModel(
                wa.Id.Value,
                wa.Name.Value,
                MapLocation(wa.Location),
                "Microsoft.Web/sites",
                new Dictionary<string, string>
                {
                    ["runtimeStack"] = wa.RuntimeStack.Value.ToString().ToLower(),
                    ["runtimeVersion"] = wa.RuntimeVersion,
                    ["alwaysOn"] = wa.AlwaysOn.ToString().ToLower(),
                    ["httpsOnly"] = wa.HttpsOnly.ToString().ToLower(),
                    ["appServicePlanId"] = wa.AppServicePlanId.Value.ToString()
                },
                waSettings
                    .Where(es => es.WebAppId == wa.Id)
                    .Select(es => new ResourceEnvironmentConfigReadModel(es.EnvironmentName, es.ToDictionary()))
                    .ToList()),
            FunctionApp fa => new AzureResourceReadModel(
                fa.Id.Value,
                fa.Name.Value,
                MapLocation(fa.Location),
                "Microsoft.Web/sites/functionapp",
                new Dictionary<string, string>
                {
                    ["runtimeStack"] = fa.RuntimeStack.Value.ToString().ToLower(),
                    ["runtimeVersion"] = fa.RuntimeVersion,
                    ["httpsOnly"] = fa.HttpsOnly.ToString().ToLower(),
                    ["appServicePlanId"] = fa.AppServicePlanId.Value.ToString()
                },
                faSettings
                    .Where(es => es.FunctionAppId == fa.Id)
                    .Select(es => new ResourceEnvironmentConfigReadModel(es.EnvironmentName, es.ToDictionary()))
                    .ToList()),
            UserAssignedIdentity uai => new AzureResourceReadModel(
                uai.Id.Value,
                uai.Name.Value,
                MapLocation(uai.Location),
                "Microsoft.ManagedIdentity/userAssignedIdentities",
                new Dictionary<string, string>(),
                new List<ResourceEnvironmentConfigReadModel>()),
            AppConfiguration ac => new AzureResourceReadModel(
                ac.Id.Value,
                ac.Name.Value,
                MapLocation(ac.Location),
                "Microsoft.AppConfiguration/configurationStores",
                new Dictionary<string, string>(),
                acSettings
                    .Where(es => es.AppConfigurationId == ac.Id)
                    .Select(es => new ResourceEnvironmentConfigReadModel(es.EnvironmentName, es.ToDictionary()))
                    .ToList()),
            ContainerAppEnvironment cae => new AzureResourceReadModel(
                cae.Id.Value,
                cae.Name.Value,
                MapLocation(cae.Location),
                "Microsoft.App/managedEnvironments",
                new Dictionary<string, string>(),
                caeSettings
                    .Where(es => es.ContainerAppEnvironmentId == cae.Id)
                    .Select(es => new ResourceEnvironmentConfigReadModel(es.EnvironmentName, es.ToDictionary()))
                    .ToList()),
            ContainerApp ca => new AzureResourceReadModel(
                ca.Id.Value,
                ca.Name.Value,
                MapLocation(ca.Location),
                "Microsoft.App/containerApps",
                new Dictionary<string, string>
                {
                    ["containerAppEnvironmentId"] = ca.ContainerAppEnvironmentId.Value.ToString()
                },
                caSettings
                    .Where(es => es.ContainerAppId == ca.Id)
                    .Select(es => new ResourceEnvironmentConfigReadModel(es.EnvironmentName, es.ToDictionary()))
                    .ToList()),
            LogAnalyticsWorkspace law => new AzureResourceReadModel(
                law.Id.Value,
                law.Name.Value,
                MapLocation(law.Location),
                "Microsoft.OperationalInsights/workspaces",
                new Dictionary<string, string>(),
                lawSettings
                    .Where(es => es.LogAnalyticsWorkspaceId == law.Id)
                    .Select(es => new ResourceEnvironmentConfigReadModel(es.EnvironmentName, es.ToDictionary()))
                    .ToList()),
            Domain.ApplicationInsightsAggregate.ApplicationInsights ai => new AzureResourceReadModel(
                ai.Id.Value,
                ai.Name.Value,
                MapLocation(ai.Location),
                "Microsoft.Insights/components",
                new Dictionary<string, string>
                {
                    ["logAnalyticsWorkspaceId"] = ai.LogAnalyticsWorkspaceId.Value.ToString()
                },
                aiSettings
                    .Where(es => es.ApplicationInsightsId == ai.Id)
                    .Select(es => new ResourceEnvironmentConfigReadModel(es.EnvironmentName, es.ToDictionary()))
                    .ToList()),
            CosmosDb cosmos => new AzureResourceReadModel(
                cosmos.Id.Value,
                cosmos.Name.Value,
                MapLocation(cosmos.Location),
                "Microsoft.DocumentDB/databaseAccounts",
                new Dictionary<string, string>(),
                cosmosSettings
                    .Where(es => es.CosmosDbId == cosmos.Id)
                    .Select(es => new ResourceEnvironmentConfigReadModel(es.EnvironmentName, es.ToDictionary()))
                    .ToList()),
            _ => null
        };
    }

    private static string MapLocation(Location location)
    {
        return location.Value switch
        {
            Location.LocationEnum.EastUS => "eastus",
            Location.LocationEnum.WestUS => "westus",
            Location.LocationEnum.CentralUS => "centralus",
            Location.LocationEnum.NorthEurope => "northeurope",
            Location.LocationEnum.WestEurope => "westeurope",
            Location.LocationEnum.SoutheastAsia => "southeastasia",
            Location.LocationEnum.EastAsia => "eastasia",
            Location.LocationEnum.AustraliaEast => "australiaeast",
            Location.LocationEnum.JapanEast => "japaneast",
            _ => "westeurope"
        };
    }
}
