using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Application.KeyVaults.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.KeyVaults.Queries;

public record GetKeyVaultQuery(
    AzureResourceId Id
) : IQuery<KeyVaultResult>;