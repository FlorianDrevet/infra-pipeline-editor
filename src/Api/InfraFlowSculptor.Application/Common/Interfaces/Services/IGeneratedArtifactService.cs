namespace InfraFlowSculptor.Application.Common.Interfaces.Services;

/// <summary>
/// Service for managing generated artifact files (Bicep, Pipeline, etc.) in blob storage.
/// Provides common operations: download as zip, get file content, list latest files.
/// </summary>
public interface IGeneratedArtifactService
{
    /// <summary>
    /// Downloads the latest generated artifacts as a zip archive.
    /// </summary>
    /// <param name="artifactType">The artifact type prefix (e.g. "bicep", "pipeline").</param>
    /// <param name="configId">The infrastructure configuration ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Zip content and file name, or null if no artifacts found.</returns>
    Task<(byte[] ZipContent, string FileName)?> DownloadLatestAsync(
        string artifactType, Guid configId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the content of a specific file from the latest generated artifacts.
    /// </summary>
    /// <param name="artifactType">The artifact type prefix (e.g. "bicep", "pipeline").</param>
    /// <param name="configId">The infrastructure configuration ID.</param>
    /// <param name="filePath">The relative file path within the artifact.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The file content, or null if not found.</returns>
    Task<string?> GetFileContentAsync(
        string artifactType, Guid configId, string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all files from the latest generated artifacts as a dictionary.
    /// </summary>
    /// <param name="artifactType">The artifact type prefix (e.g. "bicep", "pipeline").</param>
    /// <param name="configId">The infrastructure configuration ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Dictionary of relative path → file content, or null if no artifacts found.</returns>
    Task<IReadOnlyDictionary<string, string>?> GetLatestFilesAsync(
        string artifactType, Guid configId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Uploads a generated artifact file to blob storage.
    /// </summary>
    /// <param name="artifactType">The artifact type prefix (e.g. "bicep", "pipeline").</param>
    /// <param name="configId">The infrastructure configuration ID.</param>
    /// <param name="timestamp">The generation timestamp prefix.</param>
    /// <param name="relativePath">The relative file path within the artifact.</param>
    /// <param name="content">The file content.</param>
    /// <returns>The URI of the uploaded blob.</returns>
    Task<Uri> UploadArtifactAsync(
        string artifactType, Guid configId, string timestamp, string relativePath, string content);
}
