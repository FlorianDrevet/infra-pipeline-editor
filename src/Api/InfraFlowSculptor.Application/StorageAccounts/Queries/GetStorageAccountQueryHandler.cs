using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.StorageAccounts.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.StorageAccounts.Queries;

public class GetStorageAccountQueryHandler(IStorageAccountRepository storageAccountRepository, IMapper mapper)
    : IRequestHandler<GetStorageAccountQuery, ErrorOr<StorageAccountResult>>
{
    public async Task<ErrorOr<StorageAccountResult>> Handle(GetStorageAccountQuery query, CancellationToken cancellationToken)
    {
        var storageAccount = await storageAccountRepository.GetByIdWithSubResourcesAsync(query.Id, cancellationToken);

        if (storageAccount is null)
            return Errors.StorageAccount.NotFoundError(query.Id);

        return mapper.Map<StorageAccountResult>(storageAccount);
    }
}
