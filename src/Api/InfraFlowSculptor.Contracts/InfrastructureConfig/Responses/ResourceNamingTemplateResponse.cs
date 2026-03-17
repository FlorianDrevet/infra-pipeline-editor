namespace InfraFlowSculptor.Contracts.InfrastructureConfig.Responses;

/// <summary>Represents a per-resource-type naming template override.</summary>
/// <param name="Id">Unique identifier of the naming template entry.</param>
/// <param name="ResourceType">The Azure resource type this template applies to (e.g. "KeyVault", "StorageAccount").</param>
/// <param name="Template">
/// The naming template string with optional placeholders:
/// {name}, {prefix}, {suffix}, {env}, {resourceType}, {location}.
/// </param>
public record ResourceNamingTemplateResponse(
    string Id,
    string ResourceType,
    string Template);
