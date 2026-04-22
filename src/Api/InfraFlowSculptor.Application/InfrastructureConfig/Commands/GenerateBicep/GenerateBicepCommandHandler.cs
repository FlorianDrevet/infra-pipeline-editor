using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.Application.InfrastructureConfig.Diagnostics;
using InfraFlowSculptor.BicepGeneration;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.BicepGeneration.Generators;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.GenerationCore.Models;
using InfraFlowSculptor.Domain.Common.AzureRoleDefinitions;
using InfraFlowSculptor.Domain.Common.ResourceOutputs;
using ErrorOr;
using MediatR;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.GenerateBicep;

public sealed class GenerateBicepCommandHandler(
    IInfrastructureConfigReadRepository configRepository,
    BicepGenerationEngine bicepGenerationEngine,
    IBlobService blobService,
    IConfigDiagnosticService diagnosticService)
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
        var config = await configRepository.GetByIdWithResourcesAsync(
            command.InfrastructureConfigId, cancellationToken);

        if (config is null)
            return Errors.InfrastructureConfig.NotFoundError(new InfrastructureConfigId(command.InfrastructureConfigId));

        var mergedAbbreviations = MergeAbbreviations(config.NamingContext.ResourceAbbreviations);

        var resources = config.ResourceGroups
            .SelectMany(rg => rg.Resources
                .Where(r => !r.IsExisting)
                .Select(r => new ResourceDefinition
            {
                ResourceId = r.Id,
                Name = r.Name,
                Type = r.ResourceType,
                ResourceGroupName = rg.Name,
                Sku = r.Properties.GetValueOrDefault("sku", string.Empty),
                Properties = r.Properties,
                ResourceAbbreviation = GetResourceAbbreviation(r.ResourceType, mergedAbbreviations),
                EnvironmentConfigs = r.EnvironmentConfigs
                    .ToDictionary(
                        ec => ec.EnvironmentName,
                        ec => (IReadOnlyDictionary<string, string>)ec.Properties),
                AssignedUserAssignedIdentityName = r.AssignedUserAssignedIdentityName,
                CustomDomains = (r.CustomDomains ?? [])
                    .Select(cd => new CustomDomainDefinition
                    {
                        EnvironmentName = cd.EnvironmentName,
                        DomainName = cd.DomainName,
                        BindingType = cd.BindingType,
                    })
                    .ToList(),
            }))
            .ToList();

        var resourceGroups = config.ResourceGroups
            .Select(rg => new ResourceGroupDefinition
            {
                Name = rg.Name,
                Location = rg.Location,
                ResourceAbbreviation = "rg"
            })
            .ToList();

        var environmentNames = config.Environments.Select(e => e.Name).ToList();

        var environments = config.Environments
            .Select(e => new EnvironmentDefinition
            {
                Name = e.Name,
                ShortName = e.ShortName,
                Location = e.Location,
                Prefix = e.Prefix,
                Suffix = e.Suffix,
                AzureResourceManagerConnection = e.AzureResourceManagerConnection,
                SubscriptionId = e.SubscriptionId,
                Tags = e.Tags,
            })
            .ToList();

        var namingContext = new NamingContext
        {
            DefaultTemplate = config.NamingContext.DefaultTemplate,
            ResourceTemplates = config.NamingContext.ResourceTemplates,
            ResourceAbbreviations = mergedAbbreviations,
        };

        var roleAssignments = config.RoleAssignments
            .Select(ra =>
            {
                var targetTypeName = GetResourceTypeName(ra.TargetResourceType);
                var sourceTypeName = GetResourceTypeName(ra.SourceResourceType);
                var roleDef = AzureRoleDefinitionCatalog.GetForResourceType(targetTypeName)
                    .FirstOrDefault(r => r.Id.Equals(ra.RoleDefinitionId, StringComparison.OrdinalIgnoreCase));

                return new RoleAssignmentDefinition
                {
                    SourceResourceName = ra.SourceResourceName,
                    SourceResourceType = ra.SourceResourceType,
                    SourceResourceTypeName = sourceTypeName,
                    SourceResourceGroupName = ra.SourceResourceGroupName,
                    TargetResourceName = ra.TargetResourceName,
                    TargetResourceType = ra.TargetResourceType,
                    TargetResourceGroupName = ra.TargetResourceGroupName,
                    ManagedIdentityType = ra.ManagedIdentityType,
                    RoleDefinitionId = ra.RoleDefinitionId,
                    RoleDefinitionName = roleDef?.Name ?? ra.RoleDefinitionId,
                    RoleDefinitionDescription = roleDef?.Description ?? string.Empty,
                    ServiceCategory = RoleAssignmentModuleTemplates.GetServiceCategory(targetTypeName),
                    TargetResourceTypeName = targetTypeName,
                    TargetResourceAbbreviation = GetResourceAbbreviation(ra.TargetResourceType, namingContext.ResourceAbbreviations),
                    UserAssignedIdentityName = ra.UserAssignedIdentityName,
                    UserAssignedIdentityResourceId = ra.UserAssignedIdentityResourceId,
                    UserAssignedIdentityResourceGroupName = ra.UserAssignedIdentityResourceGroupName,
                    IsTargetCrossConfig = ra.IsTargetCrossConfig,
                };
            })
            .ToList();

        var appSettingDefinitions = config.AppSettings
            .Select(s =>
            {
                var sourceTypeName = s.SourceResourceType is not null
                    ? GetResourceTypeName(s.SourceResourceType)
                    : null;

                string? bicepExpression = null;
                if (sourceTypeName is not null && s.SourceOutputName is not null)
                {
                    var outputDef = ResourceOutputCatalog.FindOutput(sourceTypeName, s.SourceOutputName);
                    bicepExpression = outputDef?.BicepExpression;
                }

                // Detect sensitive output exported to KV: has both source + KV references
                var isSensitiveExport = s.IsKeyVaultReference
                    && s.SourceResourceId is not null
                    && s.SourceOutputName is not null;

                return new AppSettingDefinition
                {
                    Name = s.Name,
                    StaticValue = null,
                    EnvironmentValues = s.EnvironmentValues,
                    SourceResourceName = s.SourceResourceName,
                    SourceOutputName = s.SourceOutputName,
                    SourceResourceTypeName = sourceTypeName,
                    TargetResourceName = s.ResourceName,
                    IsOutputReference = s.IsOutputReference,
                    SourceOutputBicepExpression = bicepExpression,
                    IsKeyVaultReference = s.IsKeyVaultReference,
                    KeyVaultResourceName = s.KeyVaultResourceName,
                    SecretName = s.SecretName,
                    IsSourceCrossConfig = s.IsSourceCrossConfig,
                    SourceResourceGroupName = s.SourceResourceGroupName,
                    IsSensitiveOutputExportedToKeyVault = isSensitiveExport,
                    SecretValueAssignment = s.SecretValueAssignment?.ToString(),
                    IsViaVariableGroup = s.IsViaVariableGroup,
                    PipelineVariableName = s.PipelineVariableName,
                    VariableGroupName = s.VariableGroupName,
                };
            })
            .ToList();

        var existingResourceReferences = config.CrossConfigReferences
            .Select(ccRef =>
            {
                var targetTypeName = GetResourceTypeName(ccRef.TargetResourceType);
                return new ExistingResourceReference
                {
                    ResourceName = ccRef.TargetResourceName,
                    ResourceTypeName = targetTypeName,
                    ResourceType = ccRef.TargetResourceType,
                    ResourceGroupName = ccRef.TargetResourceGroupName,
                    ResourceAbbreviation = ccRef.TargetResourceAbbreviation,
                    SourceConfigName = ccRef.TargetConfigName,
                };
            })
            .ToList();

        // Add local existing resources (IsExisting = true) as existing declarations
        var localExistingRefs = config.ResourceGroups
            .SelectMany(rg => rg.Resources
                .Where(r => r.IsExisting)
                .Select(r =>
                {
                    var typeName = GetResourceTypeName(r.ResourceType);
                    return new ExistingResourceReference
                    {
                        ResourceName = r.Name,
                        ResourceTypeName = typeName,
                        ResourceType = r.ResourceType,
                        ResourceGroupName = rg.Name,
                        ResourceAbbreviation = GetResourceAbbreviation(r.ResourceType, mergedAbbreviations),
                        SourceConfigName = string.Empty,
                    };
                }))
            .ToList();

        existingResourceReferences.AddRange(localExistingRefs);

        var generationRequest = new GenerationRequest
        {
            Resources = resources,
            ResourceGroups = resourceGroups,
            Environments = environments,
            EnvironmentNames = environmentNames,
            NamingContext = namingContext,
            RoleAssignments = roleAssignments,
            AppSettings = appSettingDefinitions,
            ExistingResourceReferences = existingResourceReferences,
            ProjectTags = config.ProjectTags,
            ConfigTags = config.ConfigTags,
        };

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

    /// <summary>
    /// Resolves the resource abbreviation from the Azure resource type string,
    /// preferring overrides from the merged abbreviation dictionary.
    /// </summary>
    private static string GetResourceAbbreviation(
        string azureResourceType,
        IReadOnlyDictionary<string, string> mergedAbbreviations)
    {
        var typeName = AzureResourceTypes.GetFriendlyName(azureResourceType);
        return mergedAbbreviations.TryGetValue(typeName, out var abbr)
            ? abbr
            : ResourceAbbreviationCatalog.GetAbbreviation(typeName);
    }

    /// <summary>
    /// Merges the catalog defaults with user overrides.
    /// Overrides take precedence over catalog entries.
    /// </summary>
    private static IReadOnlyDictionary<string, string> MergeAbbreviations(
        IReadOnlyDictionary<string, string> overrides)
    {
        var merged = new Dictionary<string, string>(ResourceAbbreviationCatalog.GetAll(), StringComparer.OrdinalIgnoreCase);
        foreach (var (key, value) in overrides)
        {
            merged[key] = value;
        }

        return merged;
    }

    /// <summary>
    /// Resolves the simple resource type name from the Azure resource type string (e.g. "Microsoft.KeyVault/vaults" → "KeyVault").
    /// </summary>
    private static string GetResourceTypeName(string azureResourceType) =>
        AzureResourceTypes.GetFriendlyName(azureResourceType);
}
