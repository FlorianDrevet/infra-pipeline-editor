using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Domain.Common.Errors;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Queries.GetBootstrapFileContent;

/// <summary>Handles the <see cref="GetBootstrapFileContentQuery"/>.</summary>
public sealed class GetBootstrapFileContentQueryHandler(IGeneratedArtifactService artifactService)
    : IQueryHandler<GetBootstrapFileContentQuery, GetBootstrapFileContentResult>
{
    public async Task<ErrorOr<GetBootstrapFileContentResult>> Handle(
        GetBootstrapFileContentQuery query,
        CancellationToken cancellationToken)
    {
        var content = await artifactService.GetFileContentAsync(
            "bootstrap", query.InfrastructureConfigId, query.FilePath, cancellationToken);

        if (content is null)
            return Errors.InfrastructureConfig.BootstrapFileNotFoundError(query.FilePath);

        return new GetBootstrapFileContentResult(content);
    }
}
