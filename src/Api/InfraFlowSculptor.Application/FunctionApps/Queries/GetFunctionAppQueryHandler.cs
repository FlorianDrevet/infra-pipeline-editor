using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.FunctionApps.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using MapsterMapper;

namespace InfraFlowSculptor.Application.FunctionApps.Queries;

/// <summary>Handles the <see cref="GetFunctionAppQuery"/> request.</summary>
public sealed class GetFunctionAppQueryHandler(
    IFunctionAppRepository functionAppRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : IQueryHandler<GetFunctionAppQuery, FunctionAppResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<FunctionAppResult>> Handle(
        GetFunctionAppQuery query,
        CancellationToken cancellationToken)
    {
        var functionApp = await functionAppRepository.GetByIdReadOnlyAsync(query.Id, cancellationToken);
        if (functionApp is null)
            return Errors.FunctionApp.NotFoundError(query.Id);

        var authResult = await accessService.VerifyReadAccessAsync(functionApp.ResourceGroup!.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return Errors.FunctionApp.NotFoundError(query.Id);

        return mapper.Map<FunctionAppResult>(functionApp);
    }
}
