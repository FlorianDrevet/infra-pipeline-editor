using ErrorOr;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.Projects.Commands.GenerateProjectPipeline;

/// <summary>Command to generate pipeline YAML files for an entire project in mono-repo mode.</summary>
public record GenerateProjectPipelineCommand(
    ProjectId ProjectId
) : IRequest<ErrorOr<GenerateProjectPipelineResult>>;

/// <summary>Result of mono-repo pipeline generation, containing URIs organized by common and per-config folders.</summary>
/// <param name="CommonFileUris">Shared template files under the .azuredevops/ directory.</param>
/// <param name="ConfigFileUris">Per-configuration files keyed by config name, then relative path.</param>
public record GenerateProjectPipelineResult(
    IReadOnlyDictionary<string, Uri> CommonFileUris,
    IReadOnlyDictionary<string, IReadOnlyDictionary<string, Uri>> ConfigFileUris);
