using ErrorOr;
using InfraFlowSculptor.Application.ContainerApps.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.ContainerApps.Queries.GetContainerApp;

/// <summary>Query to retrieve a single Container App resource by identifier.</summary>
public record GetContainerAppQuery(
    AzureResourceId Id
) : IRequest<ErrorOr<ContainerAppResult>>;
