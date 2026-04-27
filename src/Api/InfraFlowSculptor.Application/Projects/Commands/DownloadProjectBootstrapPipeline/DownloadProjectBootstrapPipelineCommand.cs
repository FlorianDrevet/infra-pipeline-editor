using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Projects.Commands.DownloadProjectPipeline;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Commands.DownloadProjectBootstrapPipeline;

/// <summary>Command to download the latest generated bootstrap pipeline files for an entire project as a ZIP archive.</summary>
/// <param name="ProjectId">The unique identifier of the project.</param>
public record DownloadProjectBootstrapPipelineCommand(
    ProjectId ProjectId
) : ICommand<DownloadProjectPipelineResult>;
