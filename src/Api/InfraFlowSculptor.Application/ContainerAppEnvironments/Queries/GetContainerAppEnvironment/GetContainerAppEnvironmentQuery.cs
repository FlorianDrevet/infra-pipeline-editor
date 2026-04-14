using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Application.ContainerAppEnvironments.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.ContainerAppEnvironments.Queries.GetContainerAppEnvironment;

/// <summary>Query to retrieve a single Container App Environment resource by identifier.</summary>
public record GetContainerAppEnvironmentQuery(
    AzureResourceId Id
) : IQuery<ContainerAppEnvironmentResult>;
