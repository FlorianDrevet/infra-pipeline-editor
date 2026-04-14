using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.InfrastructureConfig.Diagnostics;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Queries.GetConfigDiagnostics;

/// <summary>
/// Handles the <see cref="GetConfigDiagnosticsQuery"/> by loading the configuration read model
/// and running all registered diagnostic rules against it.
/// </summary>
public sealed class GetConfigDiagnosticsQueryHandler(
    IInfrastructureConfigReadRepository configRepository,
    IConfigDiagnosticService diagnosticService)
    : IQueryHandler<GetConfigDiagnosticsQuery, GetConfigDiagnosticsResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<GetConfigDiagnosticsResult>> Handle(
        GetConfigDiagnosticsQuery query,
        CancellationToken cancellationToken)
    {
        var config = await configRepository.GetByIdWithResourcesAsync(
            query.InfrastructureConfigId, cancellationToken).ConfigureAwait(false);

        if (config is null)
            return Errors.InfrastructureConfig.NotFoundError(
                new InfrastructureConfigId(query.InfrastructureConfigId));

        var diagnostics = diagnosticService.Evaluate(config);

        return new GetConfigDiagnosticsResult(diagnostics);
    }
}
