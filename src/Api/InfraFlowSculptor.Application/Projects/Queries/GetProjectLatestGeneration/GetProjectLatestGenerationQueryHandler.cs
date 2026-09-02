using ErrorOr;
using InfraFlowSculptor.Application.Common.Helpers;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Queries.GetProjectLatestGeneration;

/// <summary>
/// Lists blob paths for the latest Bicep, Pipeline and Bootstrap generations
/// without downloading file content. Returns null per artifact type when no blobs exist.
/// </summary>
public sealed class GetProjectLatestGenerationQueryHandler(
    IProjectAccessService accessService,
    IBlobService blobService)
    : IQueryHandler<GetProjectLatestGenerationQuery, GetProjectLatestGenerationResult>
{
    private const int ProjectBlobPrefixSegmentCount = 4;
    private const string BicepArtifactPrefix = "bicep/project/";
    private const string PipelineArtifactPrefix = "pipeline/project/";
    private const string BootstrapArtifactPrefix = "bootstrap/project/";
    private const string CommonPathSegment = "Common/";
    private const string InfraBucketPrefix = "infra/";
    private const string AppBucketPrefix = "app/";

    public async Task<ErrorOr<GetProjectLatestGenerationResult>> Handle(
        GetProjectLatestGenerationQuery query,
        CancellationToken cancellationToken)
    {
        var accessResult = await accessService.VerifyReadAccessAsync(
            new ProjectId(query.ProjectId), cancellationToken);
        if (accessResult.IsError)
            return accessResult.Errors;

        var bicepData = await BlobDownloadHelper.GetLatestBlobFolderAsync(
            blobService,
            $"{BicepArtifactPrefix}{query.ProjectId}/",
            ProjectBlobPrefixSegmentCount,
            cancellationToken);
        var pipelineData = await BlobDownloadHelper.GetLatestBlobFolderAsync(
            blobService,
            $"{PipelineArtifactPrefix}{query.ProjectId}/",
            ProjectBlobPrefixSegmentCount,
            cancellationToken);
        var bootstrapData = await BlobDownloadHelper.GetLatestBlobFolderAsync(
            blobService,
            $"{BootstrapArtifactPrefix}{query.ProjectId}/",
            ProjectBlobPrefixSegmentCount,
            cancellationToken);

        if (bicepData is null && pipelineData is null && bootstrapData is null)
            return new GetProjectLatestGenerationResult(null, null, null, null);

        var bicep = bicepData is not null ? BuildBicepFiles(bicepData.RelativePaths) : null;
        var pipeline = pipelineData is not null ? BuildPipelineFiles(pipelineData.RelativePaths) : null;
        var bootstrap = bootstrapData is not null ? BuildBootstrapFiles(bootstrapData.RelativePaths) : null;

        var generatedAt = bicepData?.Timestamp ?? pipelineData?.Timestamp ?? bootstrapData?.Timestamp;

        return new GetProjectLatestGenerationResult(bicep, pipeline, bootstrap, generatedAt);
    }

    private static LatestBicepFiles? BuildBicepFiles(IReadOnlyList<string> relativePaths)
    {

        var commonFiles = new Dictionary<string, string>(StringComparer.Ordinal);
        var configFiles = new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal);

        foreach (var path in relativePaths)
        {
            if (path.StartsWith(CommonPathSegment, StringComparison.Ordinal))
            {
                commonFiles[path] = path;
            }
            else
            {
                var slashIndex = path.IndexOf('/');
                if (slashIndex > 0)
                {
                    var configName = path[..slashIndex];
                    var filePath = path[(slashIndex + 1)..];
                    if (!configFiles.TryGetValue(configName, out var files))
                    {
                        files = new Dictionary<string, string>(StringComparer.Ordinal);
                        configFiles[configName] = files;
                    }
                    files[filePath] = filePath;
                }
            }
        }

        return new LatestBicepFiles(
            commonFiles,
            configFiles.ToDictionary(
                kvp => kvp.Key,
                kvp => (IReadOnlyDictionary<string, string>)kvp.Value));
    }

    private static LatestPipelineFiles? BuildPipelineFiles(IReadOnlyList<string> relativePaths)
    {

        var infraCommon = new Dictionary<string, string>(StringComparer.Ordinal);
        var appCommon = new Dictionary<string, string>(StringComparer.Ordinal);
        var infraConfig = new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal);
        var appConfig = new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal);

        foreach (var path in relativePaths)
        {
            if (path.StartsWith(InfraBucketPrefix, StringComparison.Ordinal))
            {
                var innerPath = path[InfraBucketPrefix.Length..];
                ClassifyPipelinePath(innerPath, infraCommon, infraConfig);
            }
            else if (path.StartsWith(AppBucketPrefix, StringComparison.Ordinal))
            {
                var innerPath = path[AppBucketPrefix.Length..];
                ClassifyPipelinePath(innerPath, appCommon, appConfig);
            }
            else
            {
                // Legacy project-level uploads used a single bucketless layout. Keep treating it as infra.
                ClassifyPipelinePath(path, infraCommon, infraConfig);
            }
        }

        if (infraCommon.Count == 0 && appCommon.Count == 0
            && infraConfig.Count == 0 && appConfig.Count == 0)
            return null;

        // Union for backward compatibility
        var unionCommon = new Dictionary<string, string>(infraCommon, StringComparer.Ordinal);
        foreach (var (key, value) in appCommon) unionCommon[key] = value;

        var unionConfig = new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.Ordinal);
        foreach (var (configName, files) in infraConfig)
        {
            var union = new Dictionary<string, string>(files, StringComparer.Ordinal);
            if (appConfig.TryGetValue(configName, out var appFiles))
                foreach (var (k, v) in appFiles) union[k] = v;
            unionConfig[configName] = union;
        }
        foreach (var (configName, files) in appConfig)
        {
            if (!unionConfig.ContainsKey(configName))
                unionConfig[configName] = files;
        }

        return new LatestPipelineFiles(
            unionCommon,
            unionConfig,
            infraCommon,
            appCommon,
            infraConfig.ToDictionary(kvp => kvp.Key, kvp => (IReadOnlyDictionary<string, string>)kvp.Value),
            appConfig.ToDictionary(kvp => kvp.Key, kvp => (IReadOnlyDictionary<string, string>)kvp.Value));
    }

    private static LatestBootstrapFiles? BuildBootstrapFiles(IReadOnlyList<string> relativePaths)
    {

        var allFiles = new Dictionary<string, string>(StringComparer.Ordinal);
        var infraFiles = new Dictionary<string, string>(StringComparer.Ordinal);
        var appFiles = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var path in relativePaths)
        {
            allFiles[path] = path;

            if (path.StartsWith(InfraBucketPrefix, StringComparison.Ordinal))
            {
                var innerPath = path[InfraBucketPrefix.Length..];
                infraFiles[innerPath] = innerPath;
            }
            else if (path.StartsWith(AppBucketPrefix, StringComparison.Ordinal))
            {
                var innerPath = path[AppBucketPrefix.Length..];
                appFiles[innerPath] = innerPath;
            }
            else
            {
                // AllInOne layout: no prefix, treat as infra
                infraFiles[path] = path;
            }
        }

        return new LatestBootstrapFiles(allFiles, infraFiles, appFiles);
    }

    private static void ClassifyPipelinePath(
        string innerPath,
        Dictionary<string, string> commonBucket,
        Dictionary<string, Dictionary<string, string>> configBucket)
    {
        // Pipeline paths: .azuredevops/Common/... or .azuredevops/{configName}/...
        const string azureDevOpsPrefix = ".azuredevops/";
        if (!innerPath.StartsWith(azureDevOpsPrefix, StringComparison.Ordinal))
        {
            // Non-standard path, treat as common
            var normalizedPath = NormalizePipelineRelativePath(innerPath);
            commonBucket[normalizedPath] = normalizedPath;
            return;
        }

        var afterAzDo = innerPath[azureDevOpsPrefix.Length..];
        if (afterAzDo.StartsWith(CommonPathSegment, StringComparison.Ordinal))
        {
            commonBucket[innerPath] = innerPath;
        }
        else
        {
            var slashIndex = afterAzDo.IndexOf('/');
            if (slashIndex > 0)
            {
                var configName = afterAzDo[..slashIndex];
                var filePath = NormalizePipelineRelativePath(afterAzDo[(slashIndex + 1)..]);
                if (!configBucket.TryGetValue(configName, out var files))
                {
                    files = new Dictionary<string, string>(StringComparer.Ordinal);
                    configBucket[configName] = files;
                }
                files[filePath] = filePath;
            }
            else
            {
                commonBucket[innerPath] = innerPath;
            }
        }
    }

    private static string NormalizePipelineRelativePath(string relativePath)
    {
        var normalizedFiles = GeneratedPipelinePathNormalizer.Normalize(
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [relativePath] = relativePath,
            });

        return normalizedFiles.Keys.First();
    }
}
