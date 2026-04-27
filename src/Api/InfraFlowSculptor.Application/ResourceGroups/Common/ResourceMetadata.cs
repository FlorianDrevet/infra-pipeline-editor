namespace InfraFlowSculptor.Application.ResourceGroups.Common;

/// <summary>
/// Lightweight projection of an Azure resource with its containing resource group name.
/// Used to batch-resolve cross-config reference targets without N+1 queries or TPT resolution.
/// </summary>
/// <param name="ResourceId">Unique identifier of the Azure resource.</param>
/// <param name="ResourceName">Display name of the resource.</param>
/// <param name="ResourceType">Concrete type discriminator (e.g. "KeyVault", "WebApp").</param>
/// <param name="ResourceGroupName">Name of the containing resource group.</param>
public sealed record ResourceMetadata(
    Guid ResourceId,
    string ResourceName,
    string ResourceType,
    string ResourceGroupName);
