using ErrorOr;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.StorageAccounts.Commands.DeleteStorageAccount;

public record DeleteStorageAccountCommand(
    AzureResourceId Id
) : IRequest<ErrorOr<Deleted>>;
