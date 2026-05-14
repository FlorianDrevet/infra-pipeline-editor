using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using ErrorOr;

namespace InfraFlowSculptor.Application.NetworkSecurityGroups.Commands.DeleteNetworkSecurityGroup;

/// <summary>Deletes a Network Security Group resource.</summary>
public record DeleteNetworkSecurityGroupCommand(
    AzureResourceId Id
) : ICommand<Deleted>;
