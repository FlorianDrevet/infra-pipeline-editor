using System.Text;
using InfraFlowSculptor.BicepGeneration.Assemblers;
using InfraFlowSculptor.BicepGeneration.Generators;
using InfraFlowSculptor.BicepGeneration.Helpers;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.GenerationCore;

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
        IReadOnlyList<ResourceGroupDefinition> resourceGroups,
        IReadOnlyList<EnvironmentDefinition> environments,
        IReadOnlyList<string> environmentNames,
        IEnumerable<ResourceDefinition> resources,
        NamingContext namingContext,
        IReadOnlyList<RoleAssignmentDefinition> roleAssignments,
        IReadOnlyList<AppSettingDefinition> appSettings,
        IReadOnlyList<ExistingResourceReference>? existingResourceReferences = null,
        IReadOnlyDictionary<string, string>? projectTags = null,
        IReadOnlyDictionary<string, string>? configTags = null)
    {
        var hasRoleAssignments = roleAssignments.Count > 0;

        var typesBicep = TypesBicepAssembler.Generate(environments, hasRoleAssignments);
        var functionsBicep = FunctionsBicepAssembler.Generate(namingContext);
        var constantsBicep = hasRoleAssignments ? ConstantsBicepAssembler.Generate(roleAssignments) : string.Empty;
        var main = MainBicepAssembler.Generate(modules, resourceGroups, namingContext, roleAssignments, appSettings, existingResourceReferences ?? [], projectTags, configTags);

        var environmentParameterFiles = ParameterFileAssembler.GenerateEnvironmentParameterFiles(
            modules, environments, resources, appSettings);

        var moduleFiles = new Dictionary<string, string>();

        foreach (var module in modules.DistinctBy(m => m.ModuleFileName))
        {
            var folder = module.ModuleFolderName;
            var bicepContentWithHeader = ModuleHeaderHelper.AddModuleHeader(
                module.ResourceTypeName,
                module.ModuleFileName,
                module.ModuleBicepContent);

            moduleFiles[$"modules/{folder}/{module.ModuleFileName}"] = bicepContentWithHeader;

            if (!string.IsNullOrEmpty(module.ModuleTypesBicepContent))
            {
                moduleFiles[$"modules/{folder}/types.bicep"] = module.ModuleTypesBicepContent;
            }

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

        // Generate role assignment modules only for target resource types that have assignments
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

        // Generate Key Vault secrets batch module if sensitive outputs are exported to KV or ViaBicepparam secrets exist
        var hasSensitiveExports = appSettings.Any(s => s.IsSensitiveOutputExportedToKeyVault);
        var hasViaBicepparamSecrets = appSettings.Any(s =>
            s.IsKeyVaultReference && !s.IsSensitiveOutputExportedToKeyVault
            && s.SecretValueAssignment == "ViaBicepparam");
        if (hasSensitiveExports || hasViaBicepparamSecrets)
        {
            moduleFiles["modules/KeyVault/kvSecrets.module.bicep"] = KvSecretsModuleAssembler.Generate();
        }

        return new GenerationResult
        {
            MainBicep = main,
            TypesBicep = typesBicep,
            FunctionsBicep = functionsBicep,
            ConstantsBicep = constantsBicep,
            EnvironmentParameterFiles = environmentParameterFiles,
            ModuleFiles = moduleFiles
        };
    }

    /// <summary>
    /// Removes <c>output</c> declarations from a Bicep file whose names are not in
    /// <paramref name="usedOutputs" />.  Preserves all other content verbatim.
    /// </summary>
    /// <param name="bicepContent">The full content of a generated Bicep module file.</param>
    /// <param name="usedOutputs">The set of output names that are actually referenced.</param>
    /// <returns>The Bicep content with unused output declarations stripped.</returns>
    public static string PruneUnusedOutputs(string bicepContent, HashSet<string> usedOutputs)
    {
        if (string.IsNullOrEmpty(bicepContent))
            return bicepContent;

        var lines = bicepContent.Split('\n');
        var result = new StringBuilder(bicepContent.Length);
        var pendingAnnotations = new List<string>();

        foreach (var rawLine in lines)
        {
            var line = rawLine.TrimEnd('\r');

            // Buffer @description / @secure / @export annotations — they belong to the next declaration.
            if (line.TrimStart().StartsWith('@'))
            {
                pendingAnnotations.Add(line);
                continue;
            }

            // Detect an output declaration: output <name> ...
            var trimmed = line.TrimStart();
            if (trimmed.StartsWith("output ", StringComparison.Ordinal))
            {
                var parts = trimmed.Split(' ', 3, StringSplitOptions.RemoveEmptyEntries);
                var outputName = parts.Length >= 2 ? parts[1] : string.Empty;

                if (!usedOutputs.Contains(outputName))
                {
                    pendingAnnotations.Clear();
                    continue;
                }
            }

            // Flush buffered annotations before any non-output line.
            foreach (var annotation in pendingAnnotations)
            {
                result.Append(annotation).Append('\n');
            }
            pendingAnnotations.Clear();

            result.Append(line).Append('\n');
        }

        // Flush any trailing annotations (edge case).
        foreach (var annotation in pendingAnnotations)
        {
            result.Append(annotation).Append('\n');
        }

        // Remove trailing newline added by the loop if the original didn't end with one.
        var output = result.ToString();
        if (!bicepContent.EndsWith('\n') && output.EndsWith('\n'))
            output = output[..^1];

        return output;
    }
}
