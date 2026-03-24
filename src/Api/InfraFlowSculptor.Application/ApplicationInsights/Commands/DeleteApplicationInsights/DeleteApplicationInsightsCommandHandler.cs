using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;
using MediatR;

namespace InfraFlowSculptor.Application.ApplicationInsights.Commands.DeleteApplicationInsights;

/// <summary>
/// Handles the <see cref="DeleteApplicationInsightsCommand"/> request.
/// </summary>
public sealed class DeleteApplicationInsightsCommandHandler(
    IApplicationInsightsRepository applicationInsightsRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService)
    : IRequestHandler<DeleteApplicationInsightsCommand, ErrorOr<Deleted>>
{
    /// <inheritdoc />
    public async Task<ErrorOr<Deleted>> Handle(
        DeleteApplicationInsightsCommand request,
        CancellationToken cancellationToken)
    {
        var applicationInsights = await applicationInsightsRepository.GetByIdAsync(request.Id, cancellationToken);
        if (applicationInsights is null)
            return Errors.ApplicationInsights.NotFoundError(request.Id);

        var resourceGroup = await resourceGroupRepository.GetByIdAsync(applicationInsights.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.ApplicationInsights.NotFoundError(request.Id);

        var authResult = await accessService.VerifyWriteAccessAsync(resourceGroup.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        await applicationInsightsRepository.DeleteAsync(request.Id);

        return Result.Deleted;
    }
}
