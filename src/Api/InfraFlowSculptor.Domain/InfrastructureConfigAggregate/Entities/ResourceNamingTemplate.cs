using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.InfrastructureConfigAggregate.Entities;

/// <summary>
/// Stores a per-resource-type naming template that overrides the
/// InfrastructureConfig default naming template.
/// </summary>
public sealed class ResourceNamingTemplate : Entity<ResourceNamingTemplateId>
{
    public InfrastructureConfigId InfraConfigId { get; private set; } = null!;
    public InfrastructureConfig InfraConfig { get; private set; } = null!;

    /// <summary>
    /// The Azure resource type this template applies to (e.g. "KeyVault", "RedisCache", "StorageAccount").
    /// </summary>
    public string ResourceType { get; private set; } = null!;

    /// <summary>
    /// The naming template, e.g. "{prefix}-{name}-kv-{env}".
    /// </summary>
    public NamingTemplate Template { get; private set; } = null!;

    private ResourceNamingTemplate() { }

    internal ResourceNamingTemplate(
        InfrastructureConfigId infraConfigId,
        string resourceType,
        NamingTemplate template)
        : base(ResourceNamingTemplateId.CreateUnique())
    {
        InfraConfigId = infraConfigId;
        ResourceType = resourceType;
        Template = template;
    }

    internal void Update(NamingTemplate template)
    {
        Template = template;
    }
}
