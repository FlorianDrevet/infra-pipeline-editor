namespace InfraFlowSculptor.Contracts.InfrastructureConfig.Responses;

/// <summary>Represents a deployed environment definition belonging to an Infrastructure Configuration.</summary>
/// <param name="Id">Unique identifier of the environment definition.</param>
/// <param name="Name">Display name (e.g. "Production", "Staging").</param>
/// <param name="Prefix">Short prefix used in generated resource names.</param>
/// <param name="Suffix">Short suffix used in generated resource names.</param>
/// <param name="Location">Azure region where resources are deployed.</param>
/// <param name="TenantId">Azure AD tenant associated with this environment.</param>
/// <param name="SubscriptionId">Azure subscription where resources are created.</param>
/// <param name="Order">Deployment ordering index (lower = deployed first).</param>
/// <param name="RequiresApproval">Whether deployments to this environment require explicit approval.</param>
/// <param name="AzureResourceManagerConnection">Azure DevOps service connection name for ARM deployments.</param>
/// <param name="Tags">Azure resource tags applied to all resources in this environment.</param>
public record EnvironmentDefinitionResponse(
    string Id,
    string Name,
    string ShortName,
    string Prefix,
    string Suffix,
    string Location,
    Guid TenantId,
    Guid SubscriptionId,
    int Order,
    bool RequiresApproval,
    string? AzureResourceManagerConnection,
    IReadOnlyList<TagResponse> Tags);

/// <summary>A key/value Azure resource tag.</summary>
/// <param name="Name">Tag key.</param>
/// <param name="Value">Tag value.</param>
public record TagResponse(string Name, string Value);
