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
/// <param name="FileUris">
/// Flat union of all generated bootstrap pipeline files keyed by relative path.
/// In <c>SplitInfraCode</c>, paths are prefixed with <c>infra/</c> and <c>app/</c>.
/// In <c>AllInOne</c>, paths are root-level.
/// </param>
/// <param name="InfraFileUris">Bootstrap files targeted at the infra-flagged repository, keyed by repo-relative path.</param>
/// <param name="AppFileUris">Bootstrap files targeted at the application-code repository, keyed by repo-relative path. Empty in <c>AllInOne</c>.</param>
public record GenerateProjectBootstrapPipelineResult(
    IReadOnlyDictionary<string, Uri> FileUris,
    IReadOnlyDictionary<string, Uri> InfraFileUris,
    IReadOnlyDictionary<string, Uri> AppFileUris);
