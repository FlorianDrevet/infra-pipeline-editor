using System.ComponentModel;
using System.Text.Json;
using InfraFlowSculptor.Mcp.Common;
using ModelContextProtocol.Server;

namespace InfraFlowSculptor.Mcp.Tools;

/// <summary>
/// Provides MCP tools for discovering Azure role definitions available for role assignments.
/// </summary>
[McpServerToolType]
public sealed class RoleDefinitionTools
{
    private RoleDefinitionTools() { }

    /// <summary>
    /// Lists well-known Azure RBAC role definitions that can be used in role assignments.
    /// </summary>
    [McpServerTool(Name = "list_available_role_definitions")]
    [Description(
        "Lists well-known Azure RBAC role definitions that can be used with 'add_role_assignment'. " +
        "Returns the role name, description, and the role definition GUID to pass as roleDefinitionId. " +
        "Filter by category (e.g. 'storage', 'container', 'keyvault') or leave empty for all.")]
    public static string ListAvailableRoleDefinitions(
        [Description("Optional category filter: 'storage', 'container', 'keyvault', 'monitoring', 'messaging', 'identity', 'sql', 'network'.")] string? category = null)
    {
        var roles = string.IsNullOrWhiteSpace(category)
            ? WellKnownRoles
            : WellKnownRoles.Where(r => r.Category.Contains(category, StringComparison.OrdinalIgnoreCase)).ToArray();

        return JsonSerializer.Serialize(new
        {
            totalRoles = roles.Length,
            roles = roles.Select(r => new
            {
                r.RoleDefinitionId,
                r.Name,
                r.Description,
                r.Category,
                r.TypicalUsage,
            }),
        }, McpJsonDefaults.SerializerOptions);
    }

    private static readonly RoleInfo[] WellKnownRoles =
    [
        // Container
        new("7f951dda-4ed3-4680-a7ca-43fe172d538d", "AcrPull", "Pull artifacts from a container registry", "container", "ContainerApp → ContainerRegistry"),
        new("8311e382-0749-4cb8-b61a-304f252e45ec", "AcrPush", "Push artifacts to a container registry", "container", "CI/CD pipeline → ContainerRegistry"),
        new("c2f4ef07-c644-48eb-af81-4b1b4947fb11", "AcrDelete", "Delete artifacts from a container registry", "container", "Cleanup pipeline → ContainerRegistry"),

        // Key Vault
        new("4633458b-17de-408a-b874-0445c86b69e6", "Key Vault Secrets User", "Read secret contents", "keyvault", "ContainerApp/WebApp → KeyVault"),
        new("a4417e6f-fecd-4de8-b567-7b0420556985", "Key Vault Certificates Officer", "Manage certificates", "keyvault", "Infra pipeline → KeyVault"),
        new("14b46e9e-c2b7-41b4-b07b-48a6ebf60603", "Key Vault Crypto Officer", "Manage keys", "keyvault", "Infra pipeline → KeyVault"),
        new("21090545-7ca7-4776-b22c-e363652d74d2", "Key Vault Reader", "Read vault metadata (not secrets)", "keyvault", "Monitoring → KeyVault"),

        // Storage
        new("ba92f5b4-2d11-453d-a403-e96b0029c9fe", "Storage Blob Data Contributor", "Read, write, and delete blob data", "storage", "App → StorageAccount"),
        new("2a2b9908-6ea1-4ae2-8e65-a410df84e7d1", "Storage Blob Data Reader", "Read blob data only", "storage", "App → StorageAccount (read-only)"),
        new("974c5e8b-45b9-4653-ba55-5f855dd0fb88", "Storage Queue Data Contributor", "Read, write, and delete queue messages", "storage", "App → StorageAccount queues"),
        new("b7e6dc6d-f1e8-4753-8033-0f276bb0955b", "Storage Table Data Contributor", "Read, write, and delete table data", "storage", "App → StorageAccount tables"),

        // Monitoring
        new("73c42c96-874c-492b-b04d-ab87d138a893", "Log Analytics Reader", "Read Log Analytics data", "monitoring", "App → LogAnalyticsWorkspace"),
        new("92aaf0da-9dab-42b6-94a3-d43ce8d16293", "Log Analytics Contributor", "Manage Log Analytics workspace", "monitoring", "Infra → LogAnalyticsWorkspace"),
        new("3913510d-42f4-4e42-8a64-420c390055eb", "Monitoring Metrics Publisher", "Publish metrics to Azure Monitor", "monitoring", "App → ApplicationInsights"),

        // Service Bus
        new("4f6d3b9b-027b-4f4c-9142-0e5a2a2247e0", "Azure Service Bus Data Owner", "Full access to Service Bus", "messaging", "App → ServiceBusNamespace"),
        new("69a216fc-b8fb-44d8-bc22-1f3c2cd27a39", "Azure Service Bus Data Receiver", "Receive messages", "messaging", "Consumer → ServiceBusNamespace"),
        new("56ae325e-5073-4008-99d0-48d9c1c3c92a", "Azure Service Bus Data Sender", "Send messages", "messaging", "Producer → ServiceBusNamespace"),

        // Event Hub
        new("a638d3c7-ab3a-418d-83e6-5f17a39d4fde", "Azure Event Hubs Data Owner", "Full access to Event Hubs", "messaging", "App → EventHubNamespace"),
        new("2b629674-e913-4c01-ae53-ef4638d8f975", "Azure Event Hubs Data Receiver", "Receive events", "messaging", "Consumer → EventHubNamespace"),
        new("f526a384-b230-433a-b45c-95f59c4a2dec", "Azure Event Hubs Data Sender", "Send events", "messaging", "Producer → EventHubNamespace"),

        // SQL
        new("9b7fa17d-e63e-47b0-bb0a-15c516ac86ec", "SQL DB Contributor", "Manage SQL databases but not access", "sql", "Infra → SqlServer"),

        // Identity
        new("f1a07417-d97a-45cb-824c-7a7467783830", "Managed Identity Operator", "Read and assign user assigned identities", "identity", "ContainerApp → UserAssignedIdentity"),

        // App Configuration
        new("516239f1-63e1-4d78-a4de-a74fb236a071", "App Configuration Data Reader", "Read app configuration", "configuration", "App → AppConfiguration"),
        new("5ae67dd6-50cb-40e7-96ff-dc2bfa4b606b", "App Configuration Data Owner", "Full access to app configuration", "configuration", "App → AppConfiguration"),

        // Cosmos DB
        new("00000000-0000-0000-0000-000000000002", "Cosmos DB Built-in Data Contributor", "Read/write Cosmos DB data plane", "sql", "App → CosmosDb"),

        // Redis
        new("e0f68234-74aa-48ed-b826-c38b57376e17", "Redis Cache Contributor", "Manage Redis caches", "data", "Infra → RedisCache"),
    ];

    private sealed record RoleInfo(
        string RoleDefinitionId,
        string Name,
        string Description,
        string Category,
        string TypicalUsage);
}
