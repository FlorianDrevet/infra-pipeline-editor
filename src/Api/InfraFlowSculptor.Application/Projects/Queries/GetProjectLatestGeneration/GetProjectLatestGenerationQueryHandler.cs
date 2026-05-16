using ErrorOr;
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

        var bicepData = await GetLatestRelativePathsAsync($"bicep/project/{query.ProjectId}/");
        var pipelineData = await GetLatestRelativePathsAsync($"pipeline/project/{query.ProjectId}/");
        var bootstrapData = await GetLatestRelativePathsAsync($"bootstrap/project/{query.ProjectId}/");

        if (bicepData is null && pipelineData is null && bootstrapData is null)
            return new GetProjectLatestGenerationResult(null, null, null, null);

        var bicep = bicepData is not null ? BuildBicepFiles(bicepData.Value.Paths) : null;
        var pipeline = pipelineData is not null ? BuildPipelineFiles(pipelineData.Value.Paths) : null;
        var bootstrap = bootstrapData is not null ? BuildBootstrapFiles(bootstrapData.Value.Paths) : null;

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

    private async Task<(IReadOnlyList<string> Paths, string Timestamp)?> GetLatestRelativePathsAsync(string blobPrefix)
    {
        var allBlobs = await blobService.ListBlobsAsync(blobPrefix);
        if (allBlobs.Count == 0) return null;

        var latestPrefix = allBlobs
            .Select(blobName => string.Join('/', blobName.Split('/').Take(ProjectBlobPrefixSegmentCount)))
            .Distinct()
            .OrderDescending()
            .First();

        if (string.IsNullOrWhiteSpace(latestPrefix)) return null;

        var fullPrefix = $"{latestPrefix}/";
        var paths = allBlobs
            .Where(blobName => blobName.StartsWith(fullPrefix, StringComparison.Ordinal))
            .Select(blobName => blobName[fullPrefix.Length..])
            .ToList();

        var segments = latestPrefix.Split('/');
        var timestamp = segments.Length >= ProjectBlobPrefixSegmentCount
            ? segments[ProjectBlobPrefixSegmentCount - 1]
            : string.Empty;

        return (paths, timestamp);
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
            commonBucket[innerPath] = innerPath;
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
                if (!configBucket.TryGetValue(configName, out var files))
                {
                    files = new Dictionary<string, string>(StringComparer.Ordinal);
                    configBucket[configName] = files;
                }
                files[innerPath] = innerPath;
            }
            else
            {
                commonBucket[innerPath] = innerPath;
            }
        }
    }
}
