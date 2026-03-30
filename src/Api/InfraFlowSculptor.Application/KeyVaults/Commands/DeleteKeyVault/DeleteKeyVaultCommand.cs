using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.KeyVaults.Commands.DeleteKeyVault;

public record DeleteKeyVaultCommand(
    AzureResourceId Id
) : ICommand<Deleted>;
