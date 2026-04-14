namespace InfraFlowSculptor.Application.Projects.Queries.ListProjectResources;

/// <summary>
/// Result representing an Azure resource within a project, including its configuration context.
/// </summary>
/// <param name="ResourceId">The Azure resource identifier.</param>
/// <param name="ResourceName">The resource name.</param>
/// <param name="ResourceType">The logical resource type name (e.g. "KeyVault", "RedisCache", "StorageAccount").</param>
/// <param name="ResourceGroupName">The parent resource group name.</param>
/// <param name="ConfigId">The infrastructure configuration identifier that owns this resource.</param>
/// <param name="ConfigName">The infrastructure configuration name.</param>
public record ProjectResourceResult(
    Guid ResourceId,
    string ResourceName,
    string ResourceType,
    string ResourceGroupName,
    Guid ConfigId,
    string ConfigName);
