using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Projects.Common;

/// <summary>Application-layer result for a project-level environment definition.</summary>
public record ProjectEnvironmentDefinitionResult(
    ProjectEnvironmentDefinitionId Id,
    Name Name,
    string ShortName,
    string Prefix,
    string Suffix,
    string Location,
    Guid SubscriptionId,
    int Order,
    bool RequiresApproval,
    string? AzureResourceManagerConnection,
    IReadOnlyList<TagResult> Tags);
