using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.PipelineGeneration.Models;

namespace InfraFlowSculptor.Application.Projects.Common.Storage;

/// <summary>
/// Uploads mono-repo generation artifacts to blob storage and returns the URI maps consumed by project handlers.
/// </summary>
public interface IMonoRepoBlobUploadOrchestrator
{
    /// <summary>
    /// Uploads mono-repo Bicep artifacts to blob storage.
    /// </summary>
    /// <param name="prefix">The timestamped blob prefix for the generation batch.</param>
    /// <param name="generationResult">The generated mono-repo Bicep artifacts.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The uploaded Bicep artifact URI maps.</returns>
    Task<ProjectBicepBlobUploadResult> UploadBicepAsync(
        string prefix,
        MonoRepoGenerationResult generationResult,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Uploads mono-repo pipeline artifacts to blob storage.
    /// </summary>
    /// <param name="prefix">The timestamped blob prefix for the generation batch.</param>
    /// <param name="generationResult">The generated mono-repo pipeline artifacts.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The uploaded pipeline artifact URI maps.</returns>
    Task<ProjectPipelineBlobUploadResult> UploadPipelineAsync(
        string prefix,
        MonoRepoPipelineResult generationResult,
        CancellationToken cancellationToken = default);
}