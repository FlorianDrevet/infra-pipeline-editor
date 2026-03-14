using BicepGenerator.Application.Common.Interfaces.Persistence;
using BicepGenerator.Application.Common.Interfaces.Services;
using BicepGenerator.Domain;
using ErrorOr;
using MediatR;

namespace BicepGenerator.Application.Commands.GenerateBicep;

public class GenerateBicepCommandHandler(
    IInfrastructureConfigReadRepository configRepository,
    BicepGenerationEngine bicepGenerationEngine,
    IBlobService blobService)
    : IRequestHandler<GenerateBicepCommand, ErrorOr<GenerateBicepResult>>
{
    private const string DefaultLocation = "westeurope";

    public async Task<ErrorOr<GenerateBicepResult>> Handle(
        GenerateBicepCommand command,
        CancellationToken cancellationToken)
    {
        var config = await configRepository.GetByIdWithResourcesAsync(
            command.InfrastructureConfigId, cancellationToken);

        if (config is null)
            return Error.NotFound(
                "InfrastructureConfig.NotFound",
                $"Infrastructure configuration '{command.InfrastructureConfigId}' was not found.");

        var resources = config.ResourceGroups
            .SelectMany(rg => rg.Resources)
            .Select(r => new ResourceDefinition
            {
                Name = r.Name,
                Type = r.ResourceType,
                Sku = r.Properties.GetValueOrDefault("sku", string.Empty),
                Properties = r.Properties
            })
            .ToList();

        var defaultLocation = config.Environments.FirstOrDefault()?.Location
            ?? config.ResourceGroups.FirstOrDefault()?.Location
            ?? DefaultLocation;

        var generationRequest = new GenerationRequest
        {
            Resources = resources,
            Environment = new EnvironmentDefinition { Location = defaultLocation }
        };

        var result = bicepGenerationEngine.Generate(generationRequest);

        var prefix = $"bicep/{config.Id}/{DateTimeOffset.UtcNow:yyyyMMddHHmmss}";

        var mainBicepUri = await blobService.UploadContentAsync(
            $"{prefix}/main.bicep",
            result.MainBicep,
            "text/plain");

        var parametersUri = await blobService.UploadContentAsync(
            $"{prefix}/main.bicepparam",
            result.MainBicepParameters,
            "text/plain");

        var moduleUris = new Dictionary<string, Uri>();
        foreach (var (path, content) in result.ModuleFiles)
        {
            var moduleUri = await blobService.UploadContentAsync(
                $"{prefix}/{path}",
                content,
                "text/plain");
            moduleUris[path] = moduleUri;
        }

        return new GenerateBicepResult(mainBicepUri, parametersUri, moduleUris);
    }
}
