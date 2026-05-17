using System.Text;
using InfraFlowSculptor.BicepGeneration.Assemblers.MainBicep;
using InfraFlowSculptor.BicepGeneration.Helpers;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.BicepGeneration.StorageAccount;

namespace InfraFlowSculptor.BicepGeneration.Assemblers;

/// <summary>
/// Generates <c>main.bicep</c> with environment-aware naming and resource deployment.
/// </summary>
internal static class MainBicepAssembler
{
    /// <summary>
    /// Generates the <c>main.bicep</c> content for the given modules and deployment context.
    /// Returns both the generated text and the map of module outputs referenced during emission,
    /// keyed by module file path. The map is consumed by the IR output pruner to remove unused outputs
    /// without re-parsing the generated text.
    /// </summary>
    internal static MainBicepEmissionResult Generate(
        IReadOnlyCollection<GeneratedTypeModule> modules,
        IReadOnlyList<ResourceGroupDefinition> resourceGroups,
        NamingContext namingContext,
        IReadOnlyList<RoleAssignmentDefinition> roleAssignments,
        IReadOnlyList<AppSettingDefinition> appSettings,
        IReadOnlyList<ExistingResourceReference> existingResourceReferences,
        (IReadOnlyDictionary<string, string>? ProjectTags, IReadOnlyDictionary<string, string>? ConfigTags) tagSets = default)
    {
        var sb = new StringBuilder();
        var tracker = new OutputUsageTracker();
        MainBicepModuleSectionAssembler.ValidateKeyVaultSecretNames(appSettings);
        var localResourceGroupSymbols = resourceGroups
            .Select(resourceGroup => BicepIdentifierHelper.ToBicepIdentifier(resourceGroup.Name))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        sb.AppendLine("targetScope = 'subscription'");
        sb.AppendLine();

        sb.AppendLine("import { EnvironmentName, environments } from 'types.bicep'");

        var functionImports = BicepNamingHelper.BuildFunctionImportList(namingContext, modules, resourceGroups);
        foreach (var existingReference in existingResourceReferences)
        {
            AddNamingImport(existingReference.ResourceTypeName);
        }

        foreach (var roleAssignment in roleAssignments)
        {
            AddNamingImport(roleAssignment.TargetResourceTypeName);
        }

        if (functionImports.Count > 0)
        {
            sb.Append("import { ");
            sb.AppendJoin(", ", functionImports);
            sb.AppendLine(" } from 'functions.bicep'");
        }

        if (roleAssignments.Count > 0)
        {
            sb.AppendLine("import { RbacRoles } from 'constants.bicep'");
        }

        var usedModuleTypeImports = GetUsedModuleTypeImports(modules);
        var importedModuleTypeNames = BuildImportedModuleTypeNames(usedModuleTypeImports);
        var moduleTypeImports = usedModuleTypeImports
            .GroupBy(importValue => importValue.ModuleFolderName)
            .ToList();
        foreach (var group in moduleTypeImports)
        {
            var typeNames = group
                .OrderBy(importValue => importValue.TypeName, StringComparer.Ordinal)
                .Select(importValue => FormatImportedTypeSymbol(
                    importValue.TypeName,
                    importedModuleTypeNames[BuildModuleTypeImportKey(importValue.ModuleFolderName, importValue.TypeName)]));
            sb.Append("import { ");
            sb.AppendJoin(", ", typeNames);
            sb.AppendLine($" }} from './modules/{group.Key}/types.bicep'");
        }

        sb.AppendLine();
        sb.AppendLine("@description('The target deployment environment')");
        sb.AppendLine("param environmentName EnvironmentName");
        sb.AppendLine();

        AppendResourceParameterDeclarations(sb, modules, importedModuleTypeNames);
        AppendAppSettingParameterDeclarations(sb, appSettings);

        sb.AppendLine();
        sb.AppendLine("var env = environments[environmentName]");
        sb.AppendLine();

        AppendTagsMergingBlock(sb, tagSets.ProjectTags, tagSets.ConfigTags);
        MainBicepDeploymentSectionAssembler.AppendResourceGroupDeclarations(sb, resourceGroups, namingContext);
        MainBicepDeploymentSectionAssembler.AppendExistingResourceDeclarations(
            sb,
            existingResourceReferences,
            roleAssignments,
            namingContext,
            localResourceGroupSymbols);

        var appSettingsByTarget = appSettings
            .GroupBy(setting => setting.TargetResourceName, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.ToList(), StringComparer.OrdinalIgnoreCase);

        var uaiBySourceResource = roleAssignments
            .Where(roleAssignment => roleAssignment.ManagedIdentityType == "UserAssigned" && roleAssignment.UserAssignedIdentityName is not null)
            .GroupBy(roleAssignment => (roleAssignment.SourceResourceName, roleAssignment.SourceResourceTypeName))
            .ToDictionary(
                group => group.Key,
                group => group.Select(roleAssignment => roleAssignment.UserAssignedIdentityName!)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList());

        foreach (var module in modules)
        {
            MainBicepModuleSectionAssembler.AppendModuleDeclaration(
                sb,
                tracker,
                module,
                modules,
                namingContext,
                uaiBySourceResource,
                appSettingsByTarget);
        }

        MainBicepDeploymentSectionAssembler.AppendKeyVaultSecretsSection(sb, tracker, modules, appSettings, namingContext);
        MainBicepDeploymentSectionAssembler.AppendRoleAssignmentModules(
            sb,
            tracker,
            modules,
            roleAssignments,
            namingContext,
            localResourceGroupSymbols);

        return new MainBicepEmissionResult(sb.ToString(), tracker.Build());

        static IReadOnlyList<(string ModuleFolderName, string TypeName)> GetUsedModuleTypeImports(
            IReadOnlyCollection<GeneratedTypeModule> generatedModules)
        {
            return generatedModules
                .Where(module => module.ParameterTypeOverrides.Count > 0)
                .SelectMany(module => module.ParameterTypeOverrides
                    .Where(parameterTypeOverride =>
                        module.Parameters.ContainsKey(parameterTypeOverride.Key)
                        && !IsDerivedParameter(module, parameterTypeOverride.Key))
                    .Select(parameterTypeOverride => (module.ModuleFolderName, TypeName: parameterTypeOverride.Value)))
                .Distinct()
                .ToList();
        }

        static IReadOnlyDictionary<string, string> BuildImportedModuleTypeNames(
            IReadOnlyList<(string ModuleFolderName, string TypeName)> moduleTypeImports)
        {
            var collidingTypeNames = moduleTypeImports
                .GroupBy(importValue => importValue.TypeName, StringComparer.Ordinal)
                .Where(group => group.Select(importValue => importValue.ModuleFolderName).Distinct(StringComparer.OrdinalIgnoreCase).Count() > 1)
                .Select(group => group.Key)
                .ToHashSet(StringComparer.Ordinal);

            return moduleTypeImports.ToDictionary(
                importValue => BuildModuleTypeImportKey(importValue.ModuleFolderName, importValue.TypeName),
                importValue => collidingTypeNames.Contains(importValue.TypeName)
                    ? $"{importValue.ModuleFolderName}{importValue.TypeName}"
                    : importValue.TypeName,
                StringComparer.OrdinalIgnoreCase);
        }

        static string FormatImportedTypeSymbol(string typeName, string importedTypeName)
        {
            return string.Equals(typeName, importedTypeName, StringComparison.Ordinal)
                ? typeName
                : $"{typeName} as {importedTypeName}";
        }

        static string BuildModuleTypeImportKey(string moduleFolderName, string typeName)
        {
            return $"{moduleFolderName}|{typeName}";
        }

        void AddNamingImport(string resourceTypeName)
        {
            if (string.IsNullOrWhiteSpace(resourceTypeName))
            {
                return;
            }

            if (namingContext.ResourceTemplates.ContainsKey(resourceTypeName))
            {
                functionImports.Add(NamingTemplateTranslator.GetFunctionName(resourceTypeName));
                return;
            }

            if (!string.IsNullOrEmpty(namingContext.DefaultTemplate))
            {
                functionImports.Add("BuildResourceName");
            }
        }
    }

    private static string ResolveImportedTypeName(
        IReadOnlyDictionary<string, string> importedTypeNames,
        string moduleFolderName,
        string typeName)
    {
        return importedTypeNames.TryGetValue($"{moduleFolderName}|{typeName}", out var importedTypeName)
            ? importedTypeName
            : typeName;
    }

    private static void AppendResourceParameterDeclarations(
        StringBuilder sb,
        IReadOnlyCollection<GeneratedTypeModule> modules,
        IReadOnlyDictionary<string, string> importedModuleTypeNames)
    {
        foreach (var module in modules)
        {
            foreach (var (key, value) in GetDeclaredParameters(module))
            {
                var bicepType = module.ParameterTypeOverrides.TryGetValue(key, out var customType)
                    ? ResolveImportedTypeName(importedModuleTypeNames, module.ModuleFolderName, customType)
                    : BicepFormattingHelper.InferBicepType(value);
                AppendDescriptionDecorator(sb, BuildResourceParameterDescription(module, key, isSecure: false));
                sb.AppendLine($"param {module.ModuleName}{BicepFormattingHelper.Capitalize(key)} {bicepType}");
            }

            foreach (var secureParam in module.SecureParameters)
            {
                sb.AppendLine();
                sb.AppendLine("@secure()");
                AppendDescriptionDecorator(sb, BuildResourceParameterDescription(module, secureParam, isSecure: true));
                sb.AppendLine($"param {module.ModuleName}{BicepFormattingHelper.Capitalize(secureParam)} string");
            }

            foreach (var (name, description, _) in StorageAccountCompanionHelper.GetStorageAccountCorsParameters(module))
            {
                sb.AppendLine($"@description('{BicepFormattingHelper.EscapeBicepString(description)}')");
                sb.AppendLine($"param {name} array = []");
            }

            foreach (var (name, description, _) in StorageAccountCompanionHelper.GetStorageAccountLifecycleParameters(module))
            {
                sb.AppendLine($"@description('{BicepFormattingHelper.EscapeBicepString(description)}')");
                sb.AppendLine($"param {name} array = []");
            }
        }
    }

    private static void AppendAppSettingParameterDeclarations(
        StringBuilder sb,
        IReadOnlyList<AppSettingDefinition> appSettings)
    {
        foreach (var settingGroup in appSettings
                     .Where(setting => setting.EnvironmentValues is { Count: > 0 })
                     .GroupBy(setting => setting.TargetResourceName, StringComparer.OrdinalIgnoreCase))
        {
            sb.AppendLine($"// The following inputs are used as environment variables for application {settingGroup.Key}.");

            foreach (var setting in settingGroup)
            {
                var paramName = BicepNamingHelper.GetStaticAppSettingParamName(setting.TargetResourceName, setting.Name);
                sb.AppendLine($"param {paramName} string");
            }
        }

        foreach (var setting in appSettings.Where(setting =>
            setting.IsKeyVaultReference && !setting.IsSensitiveOutputExportedToKeyVault
            && setting.SecretValueAssignment == "ViaBicepparam"))
        {
            var paramName = BicepNamingHelper.GetSecureAppSettingParamName(setting.TargetResourceName, setting.SecretName!);
            sb.AppendLine();
            sb.AppendLine("@secure()");
            AppendDescriptionDecorator(sb, BuildKeyVaultSecretValueDescription(setting));
            sb.AppendLine($"param {paramName} string");
        }
    }

    private static void AppendTagsMergingBlock(
        StringBuilder sb,
        IReadOnlyDictionary<string, string>? projectTags,
        IReadOnlyDictionary<string, string>? configTags)
    {
        var hasProjectTags = projectTags is { Count: > 0 };
        var hasConfigTags = configTags is { Count: > 0 };

        if (!hasProjectTags && !hasConfigTags)
        {
            sb.AppendLine("var tags = env.tags");
            sb.AppendLine();
            return;
        }

        if (hasProjectTags)
        {
            AppendTagsVariable(sb, "projectTags", projectTags!);
        }

        if (hasConfigTags)
        {
            AppendTagsVariable(sb, "configTags", configTags!);
        }

        if (hasProjectTags && hasConfigTags)
        {
            sb.AppendLine("var tags = union(projectTags, configTags, env.tags)");
        }
        else if (hasProjectTags)
        {
            sb.AppendLine("var tags = union(projectTags, env.tags)");
        }
        else
        {
            sb.AppendLine("var tags = union(configTags, env.tags)");
        }

        sb.AppendLine();
    }

    private static void AppendTagsVariable(
        StringBuilder sb,
        string variableName,
        IReadOnlyDictionary<string, string> tags)
    {
        sb.AppendLine($"var {variableName} = {{");
        foreach (var (key, value) in tags)
        {
            sb.AppendLine($"  '{BicepFormattingHelper.EscapeBicepString(key)}': '{BicepFormattingHelper.EscapeBicepString(value)}'");
        }

        sb.AppendLine("}");
        sb.AppendLine();
    }

    private static IEnumerable<KeyValuePair<string, object>> GetDeclaredParameters(GeneratedTypeModule module)
    {
        return module.Parameters.Where(parameter => !IsDerivedParameter(module, parameter.Key));
    }

    private static bool IsDerivedParameter(GeneratedTypeModule module, string parameterName)
    {
        return module.ParentModuleIdReferences.ContainsKey(parameterName)
            || module.ParentModuleNameReferences.ContainsKey(parameterName)
            || module.ParentModuleOutputReferences.ContainsKey(parameterName)
            || module.ExistingResourceIdReferences.ContainsKey(parameterName)
            || module.ExistingResourcePropertyReferences.ContainsKey(parameterName);
    }

    private static void AppendDescriptionDecorator(StringBuilder sb, string description)
    {
        sb.AppendLine($"@description('{BicepFormattingHelper.EscapeBicepString(description)}')");
    }

    private static string BuildResourceParameterDescription(
        GeneratedTypeModule module,
        string parameterName,
        bool isSecure)
    {
        var resourceTypeName = string.IsNullOrWhiteSpace(module.ResourceTypeName)
            ? module.ModuleFolderName
            : module.ResourceTypeName;
        var resourceName = string.IsNullOrWhiteSpace(module.LogicalResourceName)
            ? module.ModuleName
            : module.LogicalResourceName;
        var valueQualifier = isSecure ? "Secure value" : "Value";

        return $"{valueQualifier} for {parameterName} of {resourceTypeName} resource {resourceName}.";
    }

    private static string BuildKeyVaultSecretValueDescription(AppSettingDefinition setting)
    {
        return $"Secret value for Key Vault secret {setting.SecretName!} used by {setting.TargetResourceName}.";
    }
}
