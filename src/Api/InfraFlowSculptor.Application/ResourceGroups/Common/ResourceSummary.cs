namespace InfraFlowSculptor.Application.ResourceGroups.Common;

/// <summary>
/// Lightweight projection of an Azure resource for listing purposes.
/// Queried directly from the <c>AzureResource</c> base table using the persisted
/// <c>ResourceType</c> column, avoiding TPT resolution and its 18-table LEFT JOIN overhead.
/// </summary>
/// <param name="Id">Unique identifier of the Azure resource.</param>
/// <param name="Name">Display name of the resource.</param>
/// <param name="ResourceType">Concrete type discriminator (e.g. "KeyVault", "WebApp").</param>
/// <param name="Location">Azure region as a string (e.g. "WestEurope").</param>
/// <param name="IsExisting">Whether this resource already exists in Azure.</param>
/// <param name="CustomNameOverride">Optional explicit name override.</param>
public sealed record ResourceSummary(
    Guid Id,
    string Name,
    string ResourceType,
    string Location,
    bool IsExisting,
    string? CustomNameOverride);
