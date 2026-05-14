using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using ErrorOr;

namespace InfraFlowSculptor.Application.FrontDoors.Commands.DeleteFrontDoor;

/// <summary>Deletes a Front Door resource.</summary>
public record DeleteFrontDoorCommand(
    AzureResourceId Id
) : ICommand<Deleted>;
