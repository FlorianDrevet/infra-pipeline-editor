using ErrorOr;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.Projects.Commands.DownloadProjectBicep;

/// <summary>Command to download the latest generated Bicep files for an entire project.</summary>
public record DownloadProjectBicepCommand(
    ProjectId ProjectId
) : IRequest<ErrorOr<DownloadProjectBicepResult>>;

/// <summary>Result of a project-level Bicep ZIP download.</summary>
public record DownloadProjectBicepResult(
    byte[] ZipContent,
    string FileName);