using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.StorageAccounts.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.StorageAccounts.Queries;

public record GetStorageAccountQuery(
    AzureResourceId Id
) : IQuery<StorageAccountResult>;
