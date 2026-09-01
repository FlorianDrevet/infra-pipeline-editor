using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.DownloadBootstrap;

/// <summary>Handles the <see cref="DownloadBootstrapCommand"/>.</summary>
public sealed class DownloadBootstrapCommandHandler(
    IGeneratedArtifactService artifactService,
    IInfraConfigAccessService accessService)
    : ICommandHandler<DownloadBootstrapCommand, DownloadBootstrapResult>
{
    public async Task<ErrorOr<DownloadBootstrapResult>> Handle(
        DownloadBootstrapCommand command,
        CancellationToken cancellationToken)
    {
        var configId = new InfrastructureConfigId(command.InfrastructureConfigId);

        var accessResult = await accessService.VerifyReadAccessAsync(configId, cancellationToken);
        if (accessResult.IsError)
            return accessResult.Errors;

        var result = await artifactService.DownloadLatestAsync(
            "bootstrap", command.InfrastructureConfigId, cancellationToken);

        if (result is null)
            return Errors.InfrastructureConfig.BootstrapFilesNotFoundError(command.InfrastructureConfigId);

        return new DownloadBootstrapResult(result.Value.ZipContent, result.Value.FileName);
    }
}
