using ErrorOr;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.Projects.Commands.GenerateProjectPipeline;

/// <summary>Command to generate pipeline YAML files for an entire project in mono-repo mode.</summary>
public record GenerateProjectPipelineCommand(
    ProjectId ProjectId
) : IRequest<ErrorOr<GenerateProjectPipelineResult>>;

/// <summary>Result of mono-repo pipeline generation, containing URIs organized by per-config folders.</summary>
/// <param name="ConfigFileUris">Per-configuration files keyed by config name, then relative path.</param>
public record GenerateProjectPipelineResult(
    IReadOnlyDictionary<string, IReadOnlyDictionary<string, Uri>> ConfigFileUris);
