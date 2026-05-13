using System.Security.Cryptography;
using System.Text;
using InfraFlowSculptor.BicepGeneration.Assemblers;
using InfraFlowSculptor.BicepGeneration.Generators;
using InfraFlowSculptor.BicepGeneration.Helpers;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.BicepGeneration;

/// <summary>
/// Assembles all Bicep output files from generated resource modules and configuration data.
/// Produces: <c>types.bicep</c>, <c>functions.bicep</c>, <c>main.bicep</c>,
/// per-environment <c>.bicepparam</c> files, and resource module files.
/// </summary>
public static class BicepAssembler
{
    /// <summary>
    /// Assembles the complete Bicep output from generated modules and deployment context.
    /// </summary>
    public static GenerationResult Assemble(
        IReadOnlyCollection<GeneratedTypeModule> modules,
        GenerationRequest request)
    {
        var hasRoleAssignments = request.RoleAssignments.Count > 0;
        var normalizedModules = NormalizePrimaryModuleFileNames(modules);

        var typesBicep = TypesBicepAssembler.Generate(request.Environments, hasRoleAssignments);
        var functionsBicep = FunctionsBicepAssembler.Generate(request.NamingContext);
        var constantsBicep = hasRoleAssignments ? ConstantsBicepAssembler.Generate(request.RoleAssignments) : string.Empty;
        var mainEmission = MainBicepAssembler.Generate(
            normalizedModules,
            request.ResourceGroups,
            request.NamingContext,
            request.RoleAssignments,
            request.AppSettings,
            request.ExistingResourceReferences,
            (request.ProjectTags, request.ConfigTags));

        var environmentParameterFiles = ParameterFileAssembler.GenerateEnvironmentParameterFiles(
            normalizedModules,
            request.Environments,
            request.Resources,
            request.AppSettings);

        var moduleFiles = BuildModuleFiles(normalizedModules);
        AddRoleAssignmentModuleFiles(moduleFiles, request.RoleAssignments);
        AddKeyVaultSecretsModuleIfNeeded(moduleFiles, request.AppSettings);

        return new GenerationResult
        {
            MainBicep = mainEmission.Content,
            TypesBicep = typesBicep,
            FunctionsBicep = functionsBicep,
            RoleAssignments = request.RoleAssignments,
            ConstantsBicep = constantsBicep,
            EnvironmentParameterFiles = environmentParameterFiles,
            ModuleFiles = moduleFiles,
            UsedOutputsByModulePath = mainEmission.UsedOutputsByModulePath,
        };
    }

    private static Dictionary<string, string> BuildModuleFiles(IReadOnlyCollection<GeneratedTypeModule> normalizedModules)
    {
        var moduleFiles = new Dictionary<string, string>();

        foreach (var module in normalizedModules.DistinctBy(m => $"{m.ModuleFolderName}/{m.ModuleFileName}", StringComparer.OrdinalIgnoreCase))
        {
            AddPrimaryModuleFiles(moduleFiles, module);
            AddCompanionModuleFiles(moduleFiles, module);
        }

        return moduleFiles;
    }

    private static void AddPrimaryModuleFiles(IDictionary<string, string> moduleFiles, GeneratedTypeModule module)
    {
        var folder = module.ModuleFolderName;
        var bicepContentWithHeader = ModuleHeaderHelper.AddModuleHeader(
            module.ResourceTypeName,
            module.ModuleFileName,
            module.ModuleBicepContent);

        moduleFiles[$"modules/{folder}/{module.ModuleFileName}"] = bicepContentWithHeader;

        if (!string.IsNullOrEmpty(module.ModuleTypesBicepContent))
        {
            var typesPath = $"modules/{folder}/types.bicep";
            moduleFiles[typesPath] = moduleFiles.TryGetValue(typesPath, out var existingTypes)
                ? ModuleHeaderHelper.MergeTypesContent(existingTypes, module.ModuleTypesBicepContent)
                : module.ModuleTypesBicepContent;
        }
    }

    private static void AddCompanionModuleFiles(IDictionary<string, string> moduleFiles, GeneratedTypeModule module)
    {
        foreach (var companion in module.CompanionModules)
        {
            var companionPath = $"modules/{companion.FolderName}/{companion.FileName}";
            moduleFiles[companionPath] = ModuleHeaderHelper.AddModuleHeader(
                module.ResourceTypeName,
                companion.FileName,
                companion.BicepContent);

            if (!string.IsNullOrWhiteSpace(companion.TypesBicepContent))
            {
                var typesPath = $"modules/{companion.FolderName}/types.bicep";
                moduleFiles[typesPath] = moduleFiles.TryGetValue(typesPath, out var existingTypes)
                    ? ModuleHeaderHelper.MergeTypesContent(existingTypes, companion.TypesBicepContent)
                    : companion.TypesBicepContent;
            }
        }
    }

    private static void AddRoleAssignmentModuleFiles(
        IDictionary<string, string> moduleFiles,
        IReadOnlyList<RoleAssignmentDefinition> roleAssignments)
    {
        var targetResourceTypes = roleAssignments
            .Select(ra => ra.TargetResourceTypeName)
            .Distinct()
            .ToList();

        foreach (var typeName in targetResourceTypes)
        {
            var meta = RoleAssignmentModuleTemplates.GetMetadata(typeName);
            if (meta is null) continue;

            var folder = ResourceTypeMetadata.GetModuleFolderName(typeName);
            var fileName = RoleAssignmentModuleTemplates.GetModuleFileName(typeName);
            moduleFiles[$"modules/{folder}/{fileName}"] = RoleAssignmentModuleTemplates.GenerateModule(typeName);
        }
    }

    private static void AddKeyVaultSecretsModuleIfNeeded(
        IDictionary<string, string> moduleFiles,
        IReadOnlyList<AppSettingDefinition> appSettings)
    {
        var hasSensitiveExports = appSettings.Any(s => s.IsSensitiveOutputExportedToKeyVault);
        var hasViaBicepparamSecrets = appSettings.Any(s =>
            s.IsKeyVaultReference && !s.IsSensitiveOutputExportedToKeyVault
            && s.SecretValueAssignment == "ViaBicepparam");
        if (hasSensitiveExports || hasViaBicepparamSecrets)
        {
            moduleFiles["modules/KeyVault/kvSecrets.module.bicep"] = KvSecretsModuleAssembler.Generate();
        }
    }

    /// <summary>
    /// Disambiguates primary module file names when multiple resources would otherwise emit
    /// different Bicep content to the same module path.
    /// </summary>
    private static IReadOnlyCollection<GeneratedTypeModule> NormalizePrimaryModuleFileNames(
        IReadOnlyCollection<GeneratedTypeModule> modules)
    {
        var normalizedModules = new List<GeneratedTypeModule>(modules.Count);

        foreach (var group in modules.GroupBy(m => $"{m.ModuleFolderName}/{m.ModuleFileName}", StringComparer.OrdinalIgnoreCase))
        {
            var groupedModules = group.ToList();
            var distinctContents = groupedModules
                .Select(m => m.ModuleBicepContent)
                .Distinct(StringComparer.Ordinal)
                .ToList();

            if (distinctContents.Count <= 1)
            {
                normalizedModules.AddRange(groupedModules);
                continue;
            }

            var representativeModule = groupedModules[0];
            var fileNameByContent = distinctContents.ToDictionary(
                content => content,
                content => AppendContentHash(representativeModule.ModuleFileName, content),
                StringComparer.Ordinal);

            normalizedModules.AddRange(groupedModules.Select(module => module with
            {
                ModuleFileName = fileNameByContent[module.ModuleBicepContent]
            }));
        }

        return normalizedModules;
    }

    private static string AppendContentHash(string moduleFileName, string content)
    {
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(content));
        var hash = Convert.ToHexString(hashBytes)[..8].ToLowerInvariant();
        const string suffix = ".module.bicep";

        if (moduleFileName.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
        {
            return $"{moduleFileName[..^suffix.Length]}.{hash}{suffix}";
        }

        return $"{moduleFileName}.{hash}";
    }
}
