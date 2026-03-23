using System.Diagnostics.CodeAnalysis;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Domain.InfrastructureConfigAggregate.Entities;

public sealed class EnvironmentDefinition : Entity<EnvironmentDefinitionId>
{
    public InfrastructureConfigId InfraConfigId { get; set; } = null!;
    public InfrastructureConfig InfraConfig { get; set; } = null!;

    public required Name Name { get; set; } // dev, staging, prod
    public required Prefix Prefix { get; set; } // "dev-", "stg-", "prd-"
    public required Suffix Suffix { get; set; } // "-dev", "-staging", "-prod"
    public required Location Location { get; set; } // Location par défaut
    public required TenantId TenantId { get; set; }
    public required SubscriptionId SubscriptionId { get; set; }
    public Order Order { get; set; } // Ordre de déploiement (dev=1, staging=2, prod=3)
    public RequiresApproval RequiresApproval { get; set; } // Pour les pipelines

    private readonly List<Tag> _tags = new();
    public IReadOnlyCollection<Tag> Tags => _tags; // Tags Azure spécifiques

    public void SetTags(IEnumerable<Tag> tags)
    {
        _tags.Clear();
        _tags.AddRange(tags);
    }

    private readonly List<EnvironmentParameterValue> _parameterValues = new();
    public IReadOnlyCollection<EnvironmentParameterValue> ParameterValues => _parameterValues;


    private EnvironmentDefinition() { }

    [SetsRequiredMembers]
    internal EnvironmentDefinition(InfrastructureConfigId infraConfigId, EnvironmentDefinitionData data)
        : base(EnvironmentDefinitionId.CreateUnique())
    {
        InfraConfigId = infraConfigId;
        Name = data.Name;
        Prefix = data.Prefix;
        Suffix = data.Suffix;
        Location = data.Location;
        TenantId = data.TenantId;
        SubscriptionId = data.SubscriptionId;
        Order = data.Order;
        RequiresApproval = data.RequiresApproval;
        _tags.AddRange(data.Tags);
    }
}