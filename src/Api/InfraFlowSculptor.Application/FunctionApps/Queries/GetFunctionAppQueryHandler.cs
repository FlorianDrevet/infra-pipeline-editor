using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.FunctionApps.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.FunctionApps.Queries;

/// <summary>Handles the <see cref="GetFunctionAppQuery"/> request.</summary>
public sealed class GetFunctionAppQueryHandler(
    IFunctionAppRepository functionAppRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : IQueryHandler<GetFunctionAppQuery, FunctionAppResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<FunctionAppResult>> Handle(
        GetFunctionAppQuery query,
        CancellationToken cancellationToken)
    {
        var functionApp = await functionAppRepository.GetByIdAsync(query.Id, cancellationToken);
        if (functionApp is null)
            return Errors.FunctionApp.NotFoundError(query.Id);

        var resourceGroup = await resourceGroupRepository.GetByIdAsync(functionApp.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.FunctionApp.NotFoundError(query.Id);

        var authResult = await accessService.VerifyReadAccessAsync(resourceGroup.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        return mapper.Map<FunctionAppResult>(functionApp);
    }
}
