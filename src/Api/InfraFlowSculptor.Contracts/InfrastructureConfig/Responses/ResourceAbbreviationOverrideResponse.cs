namespace InfraFlowSculptor.Contracts.InfrastructureConfig.Responses;

/// <summary>Represents a per-resource-type abbreviation override.</summary>
/// <param name="Id">Unique identifier of the abbreviation override entry.</param>
/// <param name="ResourceType">The Azure resource type this abbreviation applies to (e.g. "KeyVault", "StorageAccount").</param>
/// <param name="Abbreviation">The custom abbreviation value used in the <c>{resourceAbbr}</c> placeholder.</param>
public record ResourceAbbreviationOverrideResponse(
    string Id,
    string ResourceType,
    string Abbreviation);
