using System.IO.Compression;
using System.Text;
using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces.Services;

namespace InfraFlowSculptor.Application.Common.Helpers;

/// <summary>
/// Shared helpers for downloading and reading generated artifacts from blob storage.
/// Used by download handlers (zip) and push-to-git handlers (file dictionary).
/// </summary>
internal static class BlobDownloadHelper
{
    /// <summary>
    /// Lists blobs under <paramref name="blobPrefix"/>, finds the latest timestamp folder,
    /// zips all matching files, and returns the byte array with a file name.
    /// </summary>
    /// <param name="blobService">The blob storage service.</param>
    /// <param name="blobPrefix">The storage prefix (e.g. <c>"bicep/project/{id}/"</c>).</param>
    /// <param name="prefixSegmentCount">
    /// The number of path segments that make up the timestamp prefix
    /// (e.g. 4 for <c>"bicep/project/{id}/{timestamp}"</c>).
    /// </param>
    /// <param name="fileNameSuffix">Suffix for the zip file name (e.g. <c>"bicep"</c>).</param>
    /// <param name="notFoundErrorFactory">Factory that produces the not-found error from the entity id.</param>
    /// <param name="entityId">The identifier of the entity whose artifacts are being downloaded.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The zip byte array and file name, or an error if no blobs were found.</returns>
    internal static async Task<ErrorOr<(byte[] Data, string FileName)>> DownloadLatestBlobsAsZipAsync(
        IBlobService blobService,
        string blobPrefix,
        int prefixSegmentCount,
        string fileNameSuffix,
        Func<Guid, Error> notFoundErrorFactory,
        Guid entityId,
        CancellationToken cancellationToken)
    {
        var allBlobs = await blobService.ListBlobsAsync(blobPrefix);

        if (allBlobs.Count == 0)
            return notFoundErrorFactory(entityId);

        var latestPrefix = allBlobs
            .Select(blobName => string.Join('/', blobName.Split('/').Take(prefixSegmentCount)))
            .Distinct()
            .OrderDescending()
            .FirstOrDefault();

        if (string.IsNullOrWhiteSpace(latestPrefix))
            return notFoundErrorFactory(entityId);

        var latestBlobs = allBlobs
            .Where(blobName => blobName.StartsWith(latestPrefix, StringComparison.Ordinal))
            .ToList();

        using var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var blobName in latestBlobs)
            {
                var content = await blobService.DownloadContentAsync(blobName);
                if (content is null)
                    continue;

                var relativePath = blobName[(latestPrefix.Length + 1)..];
                var entry = archive.CreateEntry(relativePath, CompressionLevel.Optimal);
                await using var entryStream = entry.Open();
                await entryStream.WriteAsync(Encoding.UTF8.GetBytes(content), cancellationToken);
            }
        }

        var fileName = $"project-{fileNameSuffix}-{entityId:N}.zip";
        return (memoryStream.ToArray(), fileName);
    }

    /// <summary>
    /// Lists blobs under <paramref name="blobPrefix"/>, finds the latest timestamp folder,
    /// reads their content, and returns a dictionary keyed by relative path.
    /// </summary>
    /// <param name="blobService">The blob storage service.</param>
    /// <param name="blobPrefix">The storage prefix (e.g. <c>"bicep/project/{id}/"</c>).</param>
    /// <param name="prefixSegmentCount">
    /// The number of path segments that make up the timestamp prefix
    /// (e.g. 4 for <c>"bicep/project/{id}/{timestamp}"</c>, 3 for <c>"bicep/{id}/{timestamp}"</c>).
    /// </param>
    /// <param name="notFoundErrorFactory">Factory that produces the not-found error from the entity id.</param>
    /// <param name="entityId">The identifier of the entity whose artifacts are being read.</param>
    /// <param name="subPrefix">
    /// Optional sub-prefix within the latest timestamp folder (e.g. <c>"infra/"</c> for
    /// SplitInfraCode layout). When provided, only blobs under this sub-prefix are included
    /// and the sub-prefix is stripped from relative paths.
    /// </param>
    /// <param name="postProcess">
    /// Optional post-processing function applied to the file dictionary before returning
    /// (e.g. <see cref="GeneratedPipelinePathNormalizer.Normalize"/>).
    /// </param>
    /// <returns>The file dictionary keyed by relative path, or an error if no blobs were found.</returns>
    internal static async Task<ErrorOr<IReadOnlyDictionary<string, string>>> GetLatestBlobFilesAsync(
        IBlobService blobService,
        string blobPrefix,
        int prefixSegmentCount,
        Func<Guid, Error> notFoundErrorFactory,
        Guid entityId,
        string? subPrefix = null,
        Func<Dictionary<string, string>, IReadOnlyDictionary<string, string>>? postProcess = null)
    {
        var latestFilesResult = await GetLatestBlobFilesCoreAsync(
            blobService,
            blobPrefix,
            prefixSegmentCount,
            notFoundErrorFactory,
            entityId);
        if (latestFilesResult.IsError)
            return latestFilesResult.Errors;

        var files = FilterFilesBySubPrefix(latestFilesResult.Value, subPrefix);

        if (files.Count == 0)
            return notFoundErrorFactory(entityId);

        IReadOnlyDictionary<string, string> result = postProcess is not null
            ? postProcess(files)
            : files;

        return result.ToErrorOr();
    }

    /// <summary>
    /// Lists blobs under <paramref name="blobPrefix"/>, finds the latest timestamp folder,
    /// and partitions the files into two named buckets inside that folder.
    /// </summary>
    internal static async Task<ErrorOr<(IReadOnlyDictionary<string, string> First, IReadOnlyDictionary<string, string> Second)>>
        GetLatestDualBucketBlobFilesAsync(
            IBlobService blobService,
            string blobPrefix,
            int prefixSegmentCount,
            Func<Guid, Error> notFoundErrorFactory,
            Guid entityId,
            string firstBucketName,
            string secondBucketName,
            string? legacyDefaultBucketName = null,
            Func<Dictionary<string, string>, IReadOnlyDictionary<string, string>>? firstPostProcess = null,
            Func<Dictionary<string, string>, IReadOnlyDictionary<string, string>>? secondPostProcess = null)
    {
        var latestFilesResult = await GetLatestBlobFilesCoreAsync(
            blobService,
            blobPrefix,
            prefixSegmentCount,
            notFoundErrorFactory,
            entityId);
        if (latestFilesResult.IsError)
            return latestFilesResult.Errors;

        var firstFiles = new Dictionary<string, string>(StringComparer.Ordinal);
        var secondFiles = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var (relativePath, content) in latestFilesResult.Value)
        {
            if (TryStripBucketPrefix(relativePath, firstBucketName, out var firstRelativePath))
            {
                firstFiles[firstRelativePath] = content;
                continue;
            }

            if (TryStripBucketPrefix(relativePath, secondBucketName, out var secondRelativePath))
            {
                secondFiles[secondRelativePath] = content;
                continue;
            }

            if (string.Equals(legacyDefaultBucketName, firstBucketName, StringComparison.Ordinal))
            {
                firstFiles[relativePath] = content;
                continue;
            }

            if (string.Equals(legacyDefaultBucketName, secondBucketName, StringComparison.Ordinal))
                secondFiles[relativePath] = content;
        }

        if (firstFiles.Count == 0 && secondFiles.Count == 0)
            return notFoundErrorFactory(entityId);

        IReadOnlyDictionary<string, string> firstResult = firstPostProcess is not null
            ? firstPostProcess(firstFiles)
            : firstFiles;
        IReadOnlyDictionary<string, string> secondResult = secondPostProcess is not null
            ? secondPostProcess(secondFiles)
            : secondFiles;

        return (firstResult, secondResult);
    }

    private static async Task<ErrorOr<Dictionary<string, string>>> GetLatestBlobFilesCoreAsync(
        IBlobService blobService,
        string blobPrefix,
        int prefixSegmentCount,
        Func<Guid, Error> notFoundErrorFactory,
        Guid entityId)
    {
        var allBlobs = await blobService.ListBlobsAsync(blobPrefix);

        if (allBlobs.Count == 0)
            return notFoundErrorFactory(entityId);

        var latestPrefix = allBlobs
            .Select(blobName => string.Join('/', blobName.Split('/').Take(prefixSegmentCount)))
            .Distinct()
            .OrderDescending()
            .FirstOrDefault();

        if (string.IsNullOrWhiteSpace(latestPrefix))
            return notFoundErrorFactory(entityId);

        var latestBlobs = allBlobs
            .Where(blobName => blobName.StartsWith($"{latestPrefix}/", StringComparison.Ordinal))
            .ToList();

        var files = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var blobName in latestBlobs)
        {
            var content = await blobService.DownloadContentAsync(blobName);
            if (content is null)
                continue;

            var relativePath = blobName[(latestPrefix.Length + 1)..];
            files[relativePath] = content;
        }

        if (files.Count == 0)
            return notFoundErrorFactory(entityId);

        return files;
    }

    private static Dictionary<string, string> FilterFilesBySubPrefix(
        IReadOnlyDictionary<string, string> files,
        string? subPrefix)
    {
        if (string.IsNullOrEmpty(subPrefix))
            return new Dictionary<string, string>(files, StringComparer.Ordinal);

        var trimmedPrefix = subPrefix.Trim('/');
        var prefix = $"{trimmedPrefix}/";
        var filteredFiles = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var (relativePath, content) in files)
        {
            if (!relativePath.StartsWith(prefix, StringComparison.Ordinal))
                continue;

            filteredFiles[relativePath[prefix.Length..]] = content;
        }

        return filteredFiles;
    }

    private static bool TryStripBucketPrefix(string relativePath, string bucketName, out string strippedRelativePath)
    {
        var normalizedBucketName = bucketName.Trim('/');
        var prefix = $"{normalizedBucketName}/";

        if (relativePath.StartsWith(prefix, StringComparison.Ordinal))
        {
            strippedRelativePath = relativePath[prefix.Length..];
            return true;
        }

        strippedRelativePath = string.Empty;
        return false;
    }
}
