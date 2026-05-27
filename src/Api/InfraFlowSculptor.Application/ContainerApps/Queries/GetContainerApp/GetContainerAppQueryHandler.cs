using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.ContainerApps.Common;
using InfraFlowSculptor.Domain.Common.Errors;

namespace InfraFlowSculptor.Application.ContainerApps.Queries.GetContainerApp;

/// <summary>
/// Handles the <see cref="GetContainerAppQuery"/> request
/// and returns the matching Container App if the caller is a member.
/// </summary>
public sealed class GetContainerAppQueryHandler(
    IContainerAppReadRepository containerAppReadRepository,
    IInfraConfigAccessService accessService)
    : IQueryHandler<GetContainerAppQuery, ContainerAppResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<ContainerAppResult>> Handle(
        GetContainerAppQuery query,
        CancellationToken cancellationToken)
    {
        var readResult = await containerAppReadRepository.GetByIdAsync(query.Id, cancellationToken);
        if (readResult is null)
            return Errors.ContainerApp.NotFoundError(query.Id);

        var authResult = await accessService.VerifyReadAccessAsync(readResult.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return Errors.ContainerApp.NotFoundError(query.Id);

        return readResult.Result;
    }
}
