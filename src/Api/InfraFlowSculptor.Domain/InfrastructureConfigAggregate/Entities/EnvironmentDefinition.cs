using BicepGenerator.Domain.Common.Models;
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
    public IEnumerable<Tag> Tags { get; set; } // Tags Azure spécifiques
    
    private readonly List<EnvironmentParameterValue> _parameterValues = new();
    public IReadOnlyCollection<EnvironmentParameterValue> ParameterValues => _parameterValues;


    private EnvironmentDefinition() { }

    internal EnvironmentDefinition(InfrastructureConfigId infraConfigId,
        Name name,
        Prefix prefix,
        Suffix suffix,
        Location location,
        TenantId tenantId,
        SubscriptionId subscriptionId,
        Order order,
        RequiresApproval requiresApproval,
        IEnumerable<Tag> tags)
        : base(EnvironmentDefinitionId.CreateUnique())
    {
        InfraConfigId = infraConfigId;
        Name = name;
        Prefix = prefix;
        Suffix = suffix;
        Location = location;
        TenantId = tenantId;
        SubscriptionId = subscriptionId;
        Order = order;
        RequiresApproval = requiresApproval;
        Tags = tags;
    }
}