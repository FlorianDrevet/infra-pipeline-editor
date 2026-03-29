using ErrorOr;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.Projects.Commands.DownloadProjectPipeline;

/// <summary>Command to download the latest generated pipeline files for an entire project.</summary>
public record DownloadProjectPipelineCommand(
    ProjectId ProjectId
) : IRequest<ErrorOr<DownloadProjectPipelineResult>>;

/// <summary>Result of a project-level pipeline ZIP download.</summary>
public record DownloadProjectPipelineResult(
    byte[] ZipContent,
    string FileName);
