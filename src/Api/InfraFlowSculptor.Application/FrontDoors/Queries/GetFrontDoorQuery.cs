using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.FrontDoors.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.FrontDoors.Queries;

/// <summary>Retrieves a single Front Door by identifier.</summary>
public record GetFrontDoorQuery(
    AzureResourceId Id
) : IQuery<FrontDoorResult>;
