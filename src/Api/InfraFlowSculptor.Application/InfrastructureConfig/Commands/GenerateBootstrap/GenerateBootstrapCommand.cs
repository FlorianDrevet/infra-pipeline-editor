using InfraFlowSculptor.Application.Common.Interfaces;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.GenerateBootstrap;

/// <summary>
/// Command to generate the bootstrap pipeline YAML file for a single infrastructure configuration.
/// Config-level counterpart of <c>GenerateProjectBootstrapPipelineCommand</c>, used when the project
/// declares a <c>MultiRepo</c> layout (each configuration owns its own repository).
/// </summary>
public record GenerateBootstrapCommand(
    Guid InfrastructureConfigId
) : IGenerateCommand<GenerateBootstrapResult>;

/// <summary>Result of bootstrap generation, containing URIs to the generated artifact files.</summary>
/// <param name="FileUris">Map of relative file paths to their blob URIs.</param>
public record GenerateBootstrapResult(
    IReadOnlyDictionary<string, Uri> FileUris);
