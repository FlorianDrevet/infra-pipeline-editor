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
    /// Options for partitioning the latest blob folder into two logical buckets.
    /// </summary>
    internal sealed record DualBucketBlobFilesOptions(
        string FirstBucketName,
        string SecondBucketName,
        string? LegacyDefaultBucketName = null,
        Func<Dictionary<string, string>, IReadOnlyDictionary<string, string>>? FirstPostProcess = null,
        Func<Dictionary<string, string>, IReadOnlyDictionary<string, string>>? SecondPostProcess = null);

    /// <summary>
    /// Options for reading one requested file from the latest blob folder.
    /// </summary>
    internal sealed record LatestBlobContentOptions(
        Func<string, Error> FileNotFoundErrorFactory,
        string RequestedFilePath,
        IReadOnlyList<string> CandidateRelativePaths);

    /// <summary>
    /// Options for reading the latest generated files from blob storage.
    /// </summary>
    internal sealed record LatestBlobFilesOptions(
        string? SubPrefix = null,
        Func<Dictionary<string, string>, IReadOnlyDictionary<string, string>>? PostProcess = null,
        CancellationToken CancellationToken = default);

    /// <summary>
    /// Describes the latest timestamp folder found under a blob prefix.
    /// </summary>
    internal sealed record LatestBlobFolder(
        string Prefix,
        string Timestamp,
        IReadOnlyList<string> RelativePaths);

    /// <summary>
    /// Lists blobs under <paramref name="blobPrefix"/>, finds the latest timestamp folder,
    /// and returns its relative paths without downloading file content.
    /// </summary>
    /// <param name="blobService">The blob storage service.</param>
    /// <param name="blobPrefix">The storage prefix (e.g. <c>"bicep/project/{id}/"</c>).</param>
    /// <param name="prefixSegmentCount">
    /// The number of path segments that make up the timestamp prefix
    /// (e.g. 4 for <c>"bicep/project/{id}/{timestamp}"</c>).
    /// </param>
    /// <returns>
    /// A <see cref="LatestBlobFolder"/> describing the latest folder, or <c>null</c> when no blobs exist.
    /// </returns>
    internal static async Task<LatestBlobFolder?> GetLatestBlobFolderAsync(
        IBlobService blobService,
        string blobPrefix,
        int prefixSegmentCount,
        CancellationToken cancellationToken = default)
    {
        var allBlobs = await blobService.ListBlobsAsync(blobPrefix, cancellationToken);

        if (allBlobs.Count == 0)
            return null;

        var latestPrefix = allBlobs
            .Select(blobName => string.Join('/', blobName.Split('/').Take(prefixSegmentCount)))
            .Distinct()
            .OrderDescending()
            .First();

        if (string.IsNullOrWhiteSpace(latestPrefix))
            return null;

        var fullPrefix = $"{latestPrefix}/";
        var relativePaths = allBlobs
            .Where(blobName => blobName.StartsWith(fullPrefix, StringComparison.Ordinal))
            .Select(blobName => blobName[fullPrefix.Length..])
            .ToList();

        if (relativePaths.Count == 0)
            return null;

        var segments = latestPrefix.Split('/');
        var timestamp = segments.Length >= prefixSegmentCount
            ? segments[prefixSegmentCount - 1]
            : string.Empty;

        return new LatestBlobFolder(latestPrefix, timestamp, relativePaths);
    }

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
        var allBlobs = await blobService.ListBlobsAsync(blobPrefix, cancellationToken);

        if (allBlobs.Count == 0)
            return notFoundErrorFactory(entityId);

        var latestPrefix = allBlobs
            .Select(blobName => string.Join('/', blobName.Split('/').Take(prefixSegmentCount)))
            .Distinct()
            .OrderDescending()
            .First();

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
                var content = await blobService.DownloadContentAsync(blobName, cancellationToken);
                if (content is null)
                    continue;

                var relativePath = blobName[(latestPrefix.Length + 1)..];
                var entry = archive.CreateEntry(relativePath, CompressionLevel.Optimal);
                await using var entryStream = await entry.OpenAsync(cancellationToken);
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
        return await GetLatestBlobFilesAsync(
            blobService,
            blobPrefix,
            prefixSegmentCount,
            notFoundErrorFactory,
            entityId,
            new LatestBlobFilesOptions(subPrefix, postProcess));
    }

    internal static async Task<ErrorOr<IReadOnlyDictionary<string, string>>> GetLatestBlobFilesAsync(
        IBlobService blobService,
        string blobPrefix,
        int prefixSegmentCount,
        Func<Guid, Error> notFoundErrorFactory,
        Guid entityId,
        LatestBlobFilesOptions options)
    {
        var latestFilesResult = await GetLatestBlobFilesCoreAsync(
            blobService,
            blobPrefix,
            prefixSegmentCount,
            notFoundErrorFactory,
            entityId,
            options.CancellationToken);
        if (latestFilesResult.IsError)
            return latestFilesResult.Errors;

        var files = FilterFilesBySubPrefix(latestFilesResult.Value, options.SubPrefix);

        if (files.Count == 0)
            return notFoundErrorFactory(entityId);

        IReadOnlyDictionary<string, string> result = options.PostProcess is not null
            ? options.PostProcess(files)
            : files;

        return result.ToErrorOr();
    }

    /// <summary>
    /// Lists blobs under <paramref name="blobPrefix"/>, finds the latest timestamp folder,
    /// and partitions the files into two named buckets inside that folder.
    /// </summary>
    /// <param name="options">Options describing the two bucket names and optional post-processing.</param>
    internal static async Task<ErrorOr<(IReadOnlyDictionary<string, string> First, IReadOnlyDictionary<string, string> Second)>>
        GetLatestDualBucketBlobFilesAsync(
            IBlobService blobService,
            string blobPrefix,
            int prefixSegmentCount,
            Func<Guid, Error> notFoundErrorFactory,
            Guid entityId,
            DualBucketBlobFilesOptions options,
            CancellationToken cancellationToken = default)
    {
        var latestFilesResult = await GetLatestBlobFilesCoreAsync(
            blobService,
            blobPrefix,
            prefixSegmentCount,
            notFoundErrorFactory,
            entityId,
            cancellationToken);
        if (latestFilesResult.IsError)
            return latestFilesResult.Errors;

        var firstFiles = new Dictionary<string, string>(StringComparer.Ordinal);
        var secondFiles = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var (relativePath, content) in latestFilesResult.Value)
        {
            if (TryStripBucketPrefix(relativePath, options.FirstBucketName, out var firstRelativePath))
            {
                firstFiles[firstRelativePath] = content;
                continue;
            }

            if (TryStripBucketPrefix(relativePath, options.SecondBucketName, out var secondRelativePath))
            {
                secondFiles[secondRelativePath] = content;
                continue;
            }

            if (string.Equals(options.LegacyDefaultBucketName, options.FirstBucketName, StringComparison.Ordinal))
            {
                firstFiles[relativePath] = content;
                continue;
            }

            if (string.Equals(options.LegacyDefaultBucketName, options.SecondBucketName, StringComparison.Ordinal))
                secondFiles[relativePath] = content;
        }

        if (firstFiles.Count == 0 && secondFiles.Count == 0)
            return notFoundErrorFactory(entityId);

        IReadOnlyDictionary<string, string> firstResult = options.FirstPostProcess is not null
            ? options.FirstPostProcess(firstFiles)
            : firstFiles;
        IReadOnlyDictionary<string, string> secondResult = options.SecondPostProcess is not null
            ? options.SecondPostProcess(secondFiles)
            : secondFiles;

        return (firstResult, secondResult);
    }

    internal static async Task<ErrorOr<string>> GetLatestBlobContentAsync(
        IBlobService blobService,
        string blobPrefix,
        int prefixSegmentCount,
        Func<Guid, Error> notFoundErrorFactory,
        Guid entityId,
        LatestBlobContentOptions options,
        CancellationToken cancellationToken = default)
    {
        var latestFilesResult = await GetLatestBlobFilesCoreAsync(
            blobService,
            blobPrefix,
            prefixSegmentCount,
            notFoundErrorFactory,
            entityId,
            cancellationToken);
        if (latestFilesResult.IsError)
            return latestFilesResult.Errors;

        foreach (var candidateRelativePath in options.CandidateRelativePaths)
        {
            if (latestFilesResult.Value.TryGetValue(candidateRelativePath, out var content))
                return content;
        }

        return options.FileNotFoundErrorFactory(options.RequestedFilePath);
    }

    private static async Task<ErrorOr<Dictionary<string, string>>> GetLatestBlobFilesCoreAsync(
        IBlobService blobService,
        string blobPrefix,
        int prefixSegmentCount,
        Func<Guid, Error> notFoundErrorFactory,
        Guid entityId,
        CancellationToken cancellationToken)
    {
        var allBlobs = await blobService.ListBlobsAsync(blobPrefix, cancellationToken);

        if (allBlobs.Count == 0)
            return notFoundErrorFactory(entityId);

        var latestPrefix = allBlobs
            .Select(blobName => string.Join('/', blobName.Split('/').Take(prefixSegmentCount)))
            .Distinct()
            .OrderDescending()
            .First();

        if (string.IsNullOrWhiteSpace(latestPrefix))
            return notFoundErrorFactory(entityId);

        var latestBlobs = allBlobs
            .Where(blobName => blobName.StartsWith($"{latestPrefix}/", StringComparison.Ordinal))
            .ToList();

        var files = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var blobName in latestBlobs)
        {
            var content = await blobService.DownloadContentAsync(blobName, cancellationToken);
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
