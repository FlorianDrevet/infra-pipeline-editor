using System.IO.Compression;
using System.Text;
using InfraFlowSculptor.Application.Common.Interfaces.Services;

namespace InfraFlowSculptor.Infrastructure.Services;

/// <summary>
/// Manages generated artifact files (Bicep, Pipeline, etc.) in blob storage.
/// Blob path pattern: <c>{artifactType}/{configId}/{timestamp}/...</c>
/// </summary>
public sealed class GeneratedArtifactService(IBlobService blobService) : IGeneratedArtifactService
{
    public async Task<(byte[] ZipContent, string FileName)?> DownloadLatestAsync(
        string artifactType, Guid configId, CancellationToken cancellationToken = default)
    {
        var latestBlobs = await GetLatestBlobsAsync(artifactType, configId);
        if (latestBlobs is null)
            return null;

        var (latestPrefix, blobNames) = latestBlobs.Value;

        using var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var blobName in blobNames)
            {
                var content = await blobService.DownloadContentAsync(blobName);
                if (content is null) continue;

                var relativePath = blobName[(latestPrefix.Length + 1)..];
                var entry = archive.CreateEntry(relativePath, CompressionLevel.Optimal);
                await using var entryStream = entry.Open();
                await entryStream.WriteAsync(Encoding.UTF8.GetBytes(content), cancellationToken);
            }
        }

        var fileName = $"{artifactType}-{configId:N}.zip";
        return (memoryStream.ToArray(), fileName);
    }

    public async Task<string?> GetFileContentAsync(
        string artifactType, Guid configId, string filePath, CancellationToken cancellationToken = default)
    {
        var latestBlobs = await GetLatestBlobsAsync(artifactType, configId);
        if (latestBlobs is null)
            return null;

        var (latestPrefix, _) = latestBlobs.Value;
        var blobName = $"{latestPrefix}/{filePath}";
        return await blobService.DownloadContentAsync(blobName);
    }

    public async Task<IReadOnlyDictionary<string, string>?> GetLatestFilesAsync(
        string artifactType, Guid configId, CancellationToken cancellationToken = default)
    {
        var latestBlobs = await GetLatestBlobsAsync(artifactType, configId);
        if (latestBlobs is null)
            return null;

        var (latestPrefix, blobNames) = latestBlobs.Value;
        var files = new Dictionary<string, string>();

        foreach (var blobName in blobNames)
        {
            var content = await blobService.DownloadContentAsync(blobName);
            if (content is null) continue;

            var relativePath = blobName[(latestPrefix.Length + 1)..];
            files[relativePath] = content;
        }

        return files.Count == 0 ? null : files;
    }

    public async Task<Uri> UploadArtifactAsync(
        string artifactType, Guid configId, string timestamp, string relativePath, string content)
    {
        var blobPath = $"{artifactType}/{configId}/{timestamp}/{relativePath}";
        return await blobService.UploadContentAsync(blobPath, content, "text/plain");
    }

    private async Task<(string LatestPrefix, List<string> Blobs)?> GetLatestBlobsAsync(
        string artifactType, Guid configId)
    {
        var prefix = $"{artifactType}/{configId}/";
        var allBlobs = await blobService.ListBlobsAsync(prefix);

        if (allBlobs.Count == 0)
            return null;

        var latestPrefix = allBlobs
            .Select(b => string.Join('/', b.Split('/').Take(3)))
            .Distinct()
            .OrderDescending()
            .First();

        var latestBlobs = allBlobs
            .Where(b => b.StartsWith(latestPrefix, StringComparison.Ordinal))
            .ToList();

        return (latestPrefix, latestBlobs);
    }
}
