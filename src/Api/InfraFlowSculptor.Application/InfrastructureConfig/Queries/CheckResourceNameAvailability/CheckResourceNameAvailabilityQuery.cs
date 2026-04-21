using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Queries.CheckResourceNameAvailability;

/// <summary>
/// Query to check the availability of an Azure resource name across every environment
/// defined on the project. The raw name is run through the resolved naming template per env.
/// </summary>
/// <param name="ProjectId">Project owning the environments and naming templates.</param>
/// <param name="ConfigId">Optional infrastructure config whose templates may override the project templates.</param>
/// <param name="ResourceType">Friendly resource type (e.g. <c>"ContainerRegistry"</c>).</param>
/// <param name="Name">User-entered raw resource name.</param>
public sealed record CheckResourceNameAvailabilityQuery(
    ProjectId ProjectId,
    InfrastructureConfigId? ConfigId,
    string ResourceType,
    string Name) : IQuery<CheckResourceNameAvailabilityResult>;
