using ErrorOr;
using InfraFlowSculptor.Application.StorageAccounts.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.StorageAccounts.Queries;

public record GetStorageAccountQuery(
    AzureResourceId Id
) : IRequest<ErrorOr<StorageAccountResult>>;
