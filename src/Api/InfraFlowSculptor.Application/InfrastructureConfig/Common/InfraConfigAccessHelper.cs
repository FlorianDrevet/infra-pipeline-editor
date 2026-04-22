using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Common;

/// <summary>
/// Enforces access control for InfrastructureConfig by checking project-level membership.
/// Registered as <c>Scoped</c> — caches results per HTTP request to avoid redundant DB queries
/// when multiple handlers verify access to the same configuration within one request scope.
/// </summary>
internal sealed class InfraConfigAccessService(
    IInfrastructureConfigRepository configRepository,
    IProjectAccessService projectAccessService)
    : IInfraConfigAccessService
{
    private readonly Dictionary<InfrastructureConfigId, ErrorOr<Domain.InfrastructureConfigAggregate.InfrastructureConfig>> _readCache = new();
    private readonly Dictionary<InfrastructureConfigId, ErrorOr<Domain.InfrastructureConfigAggregate.InfrastructureConfig>> _writeCache = new();

    public async Task<ErrorOr<Domain.InfrastructureConfigAggregate.InfrastructureConfig>> VerifyReadAccessAsync(
        InfrastructureConfigId infraConfigId,
        CancellationToken cancellationToken = default)
    {
        if (_readCache.TryGetValue(infraConfigId, out var cached))
            return cached;

        var infraConfig = await configRepository.GetByIdAsync(infraConfigId, cancellationToken);

        if (infraConfig is null)
        {
            var error = Errors.InfrastructureConfig.NotFoundError(infraConfigId);
            _readCache[infraConfigId] = error;
            return error;
        }

        var projectAccess = await projectAccessService.VerifyReadAccessAsync(
            infraConfig.ProjectId, cancellationToken);

        if (projectAccess.IsError)
        {
            var error = Errors.InfrastructureConfig.NotFoundError(infraConfigId);
            _readCache[infraConfigId] = error;
            return error;
        }

        _readCache[infraConfigId] = infraConfig;
        return infraConfig;
    }

    public async Task<ErrorOr<Domain.InfrastructureConfigAggregate.InfrastructureConfig>> VerifyWriteAccessAsync(
        InfrastructureConfigId infraConfigId,
        CancellationToken cancellationToken = default)
    {
        if (_writeCache.TryGetValue(infraConfigId, out var cached))
            return cached;

        var infraConfig = await configRepository.GetByIdAsync(infraConfigId, cancellationToken);

        if (infraConfig is null)
        {
            var error = Errors.InfrastructureConfig.NotFoundError(infraConfigId);
            _writeCache[infraConfigId] = error;
            return error;
        }

        var projectAccess = await projectAccessService.VerifyWriteAccessAsync(
            infraConfig.ProjectId, cancellationToken);

        if (projectAccess.IsError)
        {
            _writeCache[infraConfigId] = projectAccess.Errors;
            return projectAccess.Errors;
        }

        _writeCache[infraConfigId] = infraConfig;
        return infraConfig;
    }
}
