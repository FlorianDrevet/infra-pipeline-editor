using System.ComponentModel;
using System.Text.Json;
using InfraFlowSculptor.Application.Imports.Common.Creation;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using InfraFlowSculptor.Mcp.Common;
using MediatR;
using ModelContextProtocol.Server;

namespace InfraFlowSculptor.Mcp.Tools;

/// <summary>
/// Provides the MCP tool for creating individual resources within an existing resource group.
/// Delegates to <see cref="ResourceCreationCoordinator"/> for actual resource creation.
/// </summary>
[McpServerToolType]
public sealed class ResourceCreationTools
{
    private ResourceCreationTools() { }

    /// <summary>
    /// Creates one or more resources in an existing resource group.
    /// Uses the resource creation coordinator to handle dependencies between resources.
    /// </summary>
    [McpServerTool(Name = "create_resource")]
    [Description(
        "Creates one or more resources in an existing resource group. " +
        "Provide a JSON array of resource definitions. Each resource must have: resourceType, name. " +
        "Optional fields: location (defaults to resource group location), properties (type-specific JSON object), dependsOn (array of resource names for ordering). " +
        "Supported types: KeyVault, StorageAccount, SqlServer, SqlDatabase, ContainerRegistry, " +
        "LogAnalyticsWorkspace, ApplicationInsights, ContainerAppEnvironment, ContainerApp, " +
        "UserAssignedIdentity, AppServicePlan, WebApp, FunctionApp, CosmosDb, VirtualNetwork, " +
        "ServiceBusNamespace, EventHubNamespace, RedisCache, AppConfiguration, NetworkSecurityGroup, PrivateDnsZone. " +
        "Properties vary by type — examples: " +
        "SqlServer: {\"version\": \"V12\", \"administratorLogin\": \"sqladmin\"}, " +
        "SqlDatabase: {\"sqlServerId\": \"<guid>\", \"collation\": \"SQL_Latin1_General_CP1_CI_AS\"}, " +
        "ContainerApp: {\"containerAppEnvironmentId\": \"<guid>\", \"containerRegistryId\": \"<guid>\"}, " +
        "ApplicationInsights: {\"logAnalyticsWorkspaceId\": \"<guid>\"}, " +
        "ContainerAppEnvironment: {\"logAnalyticsWorkspaceId\": \"<guid>\"}, " +
        "KeyVault: {\"enableRbacAuthorization\": true, \"enablePurgeProtection\": true, \"enableSoftDelete\": true}, " +
        "StorageAccount: {\"kind\": \"StorageV2\", \"accessTier\": \"Hot\", \"minimumTlsVersion\": \"Tls12\"}.")]
    public static async Task<string> CreateResource(
        ISender mediator,
        [Description("The resource group ID (GUID) where the resource(s) will be created.")] string resourceGroupId,
        [Description("JSON array of resource definitions: [{\"resourceType\": \"KeyVault\", \"name\": \"my-kv\", \"location\": \"FranceCentral\", \"properties\": {...}, \"dependsOn\": [...]}]")] string resources,
        CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(resourceGroupId, out var rgId))
        {
            return McpJsonDefaults.Error("invalid_resource_group_id", "The resourceGroupId must be a valid GUID.");
        }

        var resourceDefs = ParseResourceDefinitions(resources);
        if (resourceDefs is null)
        {
            return McpJsonDefaults.Error("invalid_resources", "Failed to parse resources JSON array. Expected array of {resourceType, name, location?, properties?, dependsOn?}.");
        }

        var inputs = resourceDefs.Select(r => new ResourceCreationInput
        {
            ResourceType = r.ResourceType,
            Name = r.Name,
            Location = r.Location,
            DependencyResourceNames = r.DependsOn,
            ExtractedProperties = r.Properties,
        }).ToList();

        var (created, skipped) = await ResourceCreationCoordinator.CreateResourcesAsync(
            mediator,
            ResourceGroupId.Create(rgId),
            inputs,
            logger: null,
            cancellationToken);

        return JsonSerializer.Serialize(new
        {
            status = "completed",
            resourceGroupId,
            createdResources = created.Select(c => new
            {
                c.ResourceType,
                c.ResourceId,
                c.Name,
            }),
            skippedResources = skipped.Select(s => new
            {
                s.ResourceType,
                s.Name,
                s.Reason,
            }),
            totalCreated = created.Count,
            totalSkipped = skipped.Count,
        }, McpJsonDefaults.SerializerOptions);
    }

    private static List<ResourceDefinition>? ParseResourceDefinitions(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<List<ResourceDefinition>>(json, ParseOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static readonly JsonSerializerOptions ParseOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private sealed class ResourceDefinition
    {
        public string ResourceType { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Location { get; set; }
        public IReadOnlyList<string>? DependsOn { get; set; }
        public Dictionary<string, object?>? Properties { get; set; }
    }
}
