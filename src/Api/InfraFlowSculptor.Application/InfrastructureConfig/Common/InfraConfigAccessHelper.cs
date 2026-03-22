using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Common;

/// <summary>
/// Enforces access control for InfrastructureConfig by checking project-level membership.
/// </summary>
internal sealed class InfraConfigAccessService(
    IInfrastructureConfigRepository configRepository,
    IProjectAccessService projectAccessService)
    : IInfraConfigAccessService
{
    public async Task<ErrorOr<Domain.InfrastructureConfigAggregate.InfrastructureConfig>> VerifyReadAccessAsync(
        InfrastructureConfigId infraConfigId,
        CancellationToken cancellationToken = default)
    {
        var infraConfig = await configRepository.GetByIdAsync(infraConfigId, cancellationToken);

        if (infraConfig is null)
            return Errors.InfrastructureConfig.NotFoundError(infraConfigId);

        var projectAccess = await projectAccessService.VerifyReadAccessAsync(
            infraConfig.ProjectId, cancellationToken);

        if (projectAccess.IsError)
            return Errors.InfrastructureConfig.NotFoundError(infraConfigId);

        return infraConfig;
    }

    public async Task<ErrorOr<Domain.InfrastructureConfigAggregate.InfrastructureConfig>> VerifyWriteAccessAsync(
        InfrastructureConfigId infraConfigId,
        CancellationToken cancellationToken = default)
    {
        var infraConfig = await configRepository.GetByIdAsync(infraConfigId, cancellationToken);

        if (infraConfig is null)
            return Errors.InfrastructureConfig.NotFoundError(infraConfigId);

        var projectAccess = await projectAccessService.VerifyWriteAccessAsync(
            infraConfig.ProjectId, cancellationToken);

        if (projectAccess.IsError)
            return projectAccess.Errors;

        return infraConfig;
    }
}
