using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;

namespace InfraFlowSculptor.Application.Projects.Queries.GetProjectBootstrapPipelineFileContent;

/// <summary>Query to retrieve the content of a single generated bootstrap pipeline file for a project.</summary>
/// <param name="ProjectId">The unique identifier of the project.</param>
/// <param name="FilePath">The relative file path within the latest bootstrap generation (e.g. <c>bootstrap.pipeline.yml</c>).</param>
public record GetProjectBootstrapPipelineFileContentQuery(
    Guid ProjectId,
    string FilePath
) : IQuery<GetProjectBootstrapPipelineFileContentResult>;

/// <summary>Result containing the raw YAML content of the requested bootstrap pipeline file.</summary>
/// <param name="Content">The raw YAML content of the bootstrap pipeline file.</param>
public record GetProjectBootstrapPipelineFileContentResult(string Content);
