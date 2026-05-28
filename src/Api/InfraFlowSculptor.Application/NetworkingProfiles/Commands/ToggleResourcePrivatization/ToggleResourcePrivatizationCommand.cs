using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.NetworkingProfiles.Commands.ToggleResourcePrivatization;

/// <summary>Toggles the privatization state of a specific Azure resource.</summary>
public record ToggleResourcePrivatizationCommand(
    InfrastructureConfigId InfraConfigId,
    AzureResourceId ResourceId,
    bool IsPrivatized
) : ICommand<Success>;
