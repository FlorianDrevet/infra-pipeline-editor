using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.GeneratePipeline;

/// <summary>Command to generate Azure DevOps pipeline YAML files for an infrastructure configuration.</summary>
public record GeneratePipelineCommand(
    Guid InfrastructureConfigId
) : ICommand<GeneratePipelineResult>;

/// <summary>Result of pipeline generation, containing URIs to the generated artifact files.</summary>
/// <param name="FileUris">Map of relative file paths to their blob URIs.</param>
public record GeneratePipelineResult(
    IReadOnlyDictionary<string, Uri> FileUris);
