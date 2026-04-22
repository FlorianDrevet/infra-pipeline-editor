using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Commands.GenerateProjectBootstrapPipeline;

/// <summary>Command to generate the bootstrap pipeline YAML file for an entire project.</summary>
/// <param name="ProjectId">The unique identifier of the project.</param>
public record GenerateProjectBootstrapPipelineCommand(
    ProjectId ProjectId
) : ICommand<GenerateProjectBootstrapPipelineResult>;

/// <summary>Result of bootstrap pipeline generation containing blob storage URIs keyed by relative file path.</summary>
/// <param name="FileUris">Bootstrap pipeline files keyed by relative path (e.g. <c>bootstrap.pipeline.yml</c>).</param>
public record GenerateProjectBootstrapPipelineResult(
    IReadOnlyDictionary<string, Uri> FileUris);
