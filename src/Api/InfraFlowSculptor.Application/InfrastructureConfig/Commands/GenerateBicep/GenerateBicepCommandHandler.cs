using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.Application.InfrastructureConfig.Diagnostics;
using InfraFlowSculptor.BicepGeneration;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using ErrorOr;
using MediatR;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.GenerateBicep;

public sealed class GenerateBicepCommandHandler(
    IInfrastructureConfigReadRepository configRepository,
    BicepGenerationEngine bicepGenerationEngine,
    IBlobService blobService,
    IConfigDiagnosticService diagnosticService,
    IInfraConfigAccessService accessService)
    : ICommandHandler<GenerateBicepCommand, GenerateBicepResult>
{
    /// <summary>
    /// The subdirectory name where Bicep parameter files are stored.
    /// </summary>
    private const string ParametersDirectory = "parameters";

    /// <summary>
    /// The file extension for Bicep parameter files.
    /// </summary>
    private const string BicepParameterExtension = ".bicepparam";



    public async Task<ErrorOr<GenerateBicepResult>> Handle(
        GenerateBicepCommand command,
        CancellationToken cancellationToken)
    {
        var configId = new InfrastructureConfigId(command.InfrastructureConfigId);

        // Audit APP-002 (2026-04-23): enforce write access before generating artifacts.
        var accessResult = await accessService.VerifyWriteAccessAsync(configId, cancellationToken);
        if (accessResult.IsError)
            return accessResult.Errors;

        var config = await configRepository.GetByIdWithResourcesAsync(
            command.InfrastructureConfigId, cancellationToken);

        if (config is null)
            return Errors.InfrastructureConfig.NotFoundError(configId);

        var generationRequest = GenerationRequestBuilder.Build(config);

        var result = bicepGenerationEngine.Generate(generationRequest);

        var prefix = $"bicep/{config.Id}/{DateTimeOffset.UtcNow:yyyyMMddHHmmss}";

        // Upload types.bicep
        await blobService.UploadContentAsync(
            $"{prefix}/types.bicep",
            result.TypesBicep,
            "text/plain");

        // Upload functions.bicep
        await blobService.UploadContentAsync(
            $"{prefix}/functions.bicep",
            result.FunctionsBicep,
            "text/plain");

        // Upload constants.bicep (only when role assignments exist)
        Uri? constantsBicepUri = null;
        if (!string.IsNullOrEmpty(result.ConstantsBicep))
        {
            constantsBicepUri = await blobService.UploadContentAsync(
                $"{prefix}/constants.bicep",
                result.ConstantsBicep,
                "text/plain");
        }

        // Upload main.bicep
        var mainBicepUri = await blobService.UploadContentAsync(
            $"{prefix}/main.bicep",
            result.MainBicep,
            "text/plain");

        var parameterUris = new Dictionary<string, Uri>();
        foreach (var (fileName, content) in result.EnvironmentParameterFiles)
        {
            var destinationPath = ResolveArtifactPath(prefix, fileName);
            var paramUri = await blobService.UploadContentAsync(
                destinationPath,
                content,
                "text/plain");
            parameterUris[$"{ParametersDirectory}/{fileName}"] = paramUri;
        }

        var moduleUris = new Dictionary<string, Uri>();
        foreach (var (path, content) in result.ModuleFiles)
        {
            var moduleUri = await blobService.UploadContentAsync(
                $"{prefix}/{path}",
                content,
                "text/plain");
            moduleUris[path] = moduleUri;
        }

        var warnings = await diagnosticService.EvaluateAsync(config, cancellationToken).ConfigureAwait(false);

        return new GenerateBicepResult(mainBicepUri, constantsBicepUri, parameterUris, moduleUris, warnings);
    }

    /// <summary>
    /// Resolves the destination storage path for a Bicep artifact based on its file type.
    /// Parameter files (.bicepparam) are placed in a <c>parameters/</c> subdirectory;
    /// other artifacts remain in the base prefix directory.
    /// </summary>
    /// <param name="prefix">The base storage prefix (e.g., "bicep/{configId}/{timestamp}").</param>
    /// <param name="fileName">The name of the file to upload.</param>
    /// <returns>The full destination path for the artifact in blob storage.</returns>
    private static string ResolveArtifactPath(string prefix, string fileName) =>
        fileName.EndsWith(BicepParameterExtension, StringComparison.OrdinalIgnoreCase)
            ? $"{prefix}/{ParametersDirectory}/{fileName}"
            : $"{prefix}/{fileName}";
}
