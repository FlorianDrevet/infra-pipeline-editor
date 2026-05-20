using InfraFlowSculptor.Application.Common.Generation;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.PipelineGeneration;
using InfraFlowSculptor.PipelineGeneration.Models;

namespace InfraFlowSculptor.Application.Projects.Common.Storage;

/// <summary>
/// Uploads project-level mono-repo Bicep and pipeline artifacts to blob storage.
/// </summary>
public sealed class MonoRepoBlobUploadOrchestrator(IBlobService blobService) : IMonoRepoBlobUploadOrchestrator
{
    private const string PlainTextContentType = "text/plain";
    private const string CommonPathSegment = "Common/";
    private const string AzureDevOpsPrefix = ".azuredevops/";

    /// <inheritdoc />
    public async Task<ProjectBicepBlobUploadResult> UploadBicepAsync(
        string prefix,
        MonoRepoGenerationResult generationResult,
        CancellationToken cancellationToken = default)
    {
        var commonFileUris = new Dictionary<string, Uri>(StringComparer.Ordinal);
        foreach (var (path, content) in generationResult.CommonFiles)
        {
            var commonPath = $"{CommonPathSegment}{path}";
            var uri = await blobService.UploadContentAsync(
                    $"{prefix}/{commonPath}",
                    content,
                    PlainTextContentType)
                .ConfigureAwait(false);
            commonFileUris[commonPath] = uri;
        }

        var configFileUris = new Dictionary<string, IReadOnlyDictionary<string, Uri>>(StringComparer.Ordinal);
        foreach (var (configName, files) in generationResult.ConfigFiles)
        {
            var uploadedFiles = new Dictionary<string, Uri>(StringComparer.Ordinal);
            foreach (var (path, content) in files)
            {
                var uri = await blobService.UploadContentAsync(
                        $"{prefix}/{configName}/{path}",
                        content,
                        PlainTextContentType)
                    .ConfigureAwait(false);
                uploadedFiles[path] = uri;
            }

            configFileUris[configName] = uploadedFiles;
        }

        return new ProjectBicepBlobUploadResult(commonFileUris, configFileUris);
    }

    /// <inheritdoc />
    public async Task<ProjectPipelineBlobUploadResult> UploadPipelineAsync(
        string prefix,
        MonoRepoPipelineResult generationResult,
        CancellationToken cancellationToken = default)
    {
        var infraCommonUris = new Dictionary<string, Uri>(StringComparer.Ordinal);
        var appCommonUris = new Dictionary<string, Uri>(StringComparer.Ordinal);
        foreach (var (path, content) in generationResult.CommonFiles)
        {
            var repoRelativePath = $".azuredevops/Common/{path}";
            var uri = await blobService.UploadContentAsync(
                    $"{prefix}/infra/{repoRelativePath}",
                    content,
                    PlainTextContentType)
                .ConfigureAwait(false);
            infraCommonUris[repoRelativePath] = uri;
        }

        var hasAppPipelines = generationResult.ConfigFiles.Values
            .Any(files => files.Keys.Any(path => path.StartsWith("apps/", StringComparison.Ordinal)));

        if (hasAppPipelines)
        {
            foreach (var (path, content) in AppPipelineGenerationEngine.GenerateSharedTemplates())
            {
                var repoRelativePath = ToCommonAzureDevOpsPath(path);
                var uri = await blobService.UploadContentAsync(
                        $"{prefix}/app/{repoRelativePath}",
                        content,
                        PlainTextContentType)
                    .ConfigureAwait(false);
                appCommonUris[repoRelativePath] = uri;
            }

            // Include environment variables files in the app bucket — app CI pipelines reference them
            // via template includes resolved relative to the pipeline template path.
            foreach (var (path, content) in generationResult.CommonFiles)
            {
                if (!path.StartsWith("variables/", StringComparison.Ordinal))
                    continue;

                var repoRelativePath = $".azuredevops/Common/{path}";
                var uri = await blobService.UploadContentAsync(
                        $"{prefix}/app/{repoRelativePath}",
                        content,
                        PlainTextContentType)
                    .ConfigureAwait(false);
                appCommonUris[repoRelativePath] = uri;
            }
        }

        var infraConfigUris = new Dictionary<string, IReadOnlyDictionary<string, Uri>>(StringComparer.Ordinal);
        var appConfigUris = new Dictionary<string, IReadOnlyDictionary<string, Uri>>(StringComparer.Ordinal);
        var unionConfigUris = new Dictionary<string, IReadOnlyDictionary<string, Uri>>(StringComparer.Ordinal);

        foreach (var (configName, files) in generationResult.ConfigFiles)
        {
            var (infraFiles, appFiles) = AppPipelineFileClassifier.Split(files);

            var infraUris = new Dictionary<string, Uri>(StringComparer.Ordinal);
            foreach (var (path, content) in infraFiles)
            {
                var repoRelativePath = $".azuredevops/{configName}/{path}";
                var uri = await blobService.UploadContentAsync(
                        $"{prefix}/infra/{repoRelativePath}",
                        content,
                        PlainTextContentType)
                    .ConfigureAwait(false);
                infraUris[repoRelativePath] = uri;
            }

            var appUris = new Dictionary<string, Uri>(StringComparer.Ordinal);
            foreach (var (path, content) in appFiles)
            {
                var repoRelativePath = $".azuredevops/{configName}/{path}";
                var uri = await blobService.UploadContentAsync(
                        $"{prefix}/app/{repoRelativePath}",
                        content,
                        PlainTextContentType)
                    .ConfigureAwait(false);
                appUris[repoRelativePath] = uri;
            }

            infraConfigUris[configName] = infraUris;
            appConfigUris[configName] = appUris;

            var union = new Dictionary<string, Uri>(infraUris, StringComparer.Ordinal);
            foreach (var (path, uri) in appUris)
                union[path] = uri;
            unionConfigUris[configName] = union;
        }

        var unionCommonUris = new Dictionary<string, Uri>(infraCommonUris, StringComparer.Ordinal);
        foreach (var (path, uri) in appCommonUris)
            unionCommonUris[path] = uri;

        return new ProjectPipelineBlobUploadResult(
            unionCommonUris,
            unionConfigUris,
            infraCommonUris,
            appCommonUris,
            infraConfigUris,
            appConfigUris);
    }

    private static string ToCommonAzureDevOpsPath(string path)
    {
        return path.StartsWith(AzureDevOpsPrefix, StringComparison.Ordinal)
            ? $".azuredevops/Common/{path[AzureDevOpsPrefix.Length..]}"
            : $".azuredevops/Common/{path}";
    }
}
