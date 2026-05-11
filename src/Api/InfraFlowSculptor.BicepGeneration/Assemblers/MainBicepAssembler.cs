using System.Text;
using InfraFlowSculptor.BicepGeneration.Generators;
using InfraFlowSculptor.BicepGeneration.Helpers;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.BicepGeneration.StorageAccount;
using InfraFlowSculptor.GenerationCore;

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
        IReadOnlyDictionary<string, string>? projectTags = null,
        IReadOnlyDictionary<string, string>? configTags = null)
    {
        var sb = new StringBuilder();
        var tracker = new OutputUsageTracker();
        ValidateKeyVaultSecretNames(appSettings);
        var localResourceGroupSymbols = resourceGroups
            .Select(rg => BicepIdentifierHelper.ToBicepIdentifier(rg.Name))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        sb.AppendLine("targetScope = 'subscription'");
        sb.AppendLine();

        // â”€â”€ Imports â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
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

        // Module-level type imports (for structured parameter types)
        var usedModuleTypeImports = GetUsedModuleTypeImports(modules);
        var importedModuleTypeNames = BuildImportedModuleTypeNames(usedModuleTypeImports);
        var moduleTypeImports = usedModuleTypeImports
            .GroupBy(x => x.ModuleFolderName)
            .ToList();
        foreach (var group in moduleTypeImports)
        {
            var typeNames = group
                .OrderBy(x => x.TypeName, StringComparer.Ordinal)
                .Select(x => FormatImportedTypeSymbol(
                    x.TypeName,
                    importedModuleTypeNames[BuildModuleTypeImportKey(x.ModuleFolderName, x.TypeName)]));
            sb.Append("import { ");
            sb.AppendJoin(", ", typeNames);
            sb.AppendLine($" }} from './modules/{group.Key}/types.bicep'");
        }

        sb.AppendLine();

        // â”€â”€ Parameters â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        sb.AppendLine("@description('The target deployment environment')");
        sb.AppendLine("param environmentName EnvironmentName");
        sb.AppendLine();

        // Resource-specific parameter declarations (sku, capacity, etc.)
        AppendResourceParameterDeclarations(sb, modules, importedModuleTypeNames);

        // Static app setting parameter declarations
        AppendAppSettingParameterDeclarations(sb, appSettings);

        sb.AppendLine();

        // â”€â”€ Environment resolution â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        sb.AppendLine("var env = environments[environmentName]");
        sb.AppendLine();

        // â”€â”€ Tags merging (project â†’ config â†’ environment) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        AppendTagsMergingBlock(sb, projectTags, configTags);

        // â”€â”€ Resource group declarations â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        AppendResourceGroupDeclarations(sb, resourceGroups, namingContext);

        // â”€â”€ Existing resource declarations (cross-config references) â”€â”€â”€â”€â”€â”€â”€â”€
        AppendExistingResourceDeclarations(sb, existingResourceReferences, roleAssignments, namingContext, localResourceGroupSymbols);

        // â”€â”€ Module declarations â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        // Group app settings by target resource name for quick lookup
        var appSettingsByTarget = appSettings
            .GroupBy(s => s.TargetResourceName, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

        // Group user-assigned identity references by (source name, source type name).
        var uaiBySourceResource = roleAssignments
            .Where(ra => ra.ManagedIdentityType == "UserAssigned" && ra.UserAssignedIdentityName is not null)
            .GroupBy(ra => (ra.SourceResourceName, ra.SourceResourceTypeName))
            .ToDictionary(
                g => g.Key,
                g => g.Select(ra => ra.UserAssignedIdentityName!)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList());

        foreach (var module in modules)
        {
            AppendModuleDeclaration(sb, tracker, module, modules, namingContext, uaiBySourceResource, appSettingsByTarget);
        }

        // â”€â”€ Key Vault secrets (batch per Key Vault) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        AppendKeyVaultSecretsSection(sb, tracker, modules, appSettings, namingContext);

        // â”€â”€ Role assignment module declarations â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        AppendRoleAssignmentModules(sb, tracker, modules, roleAssignments, namingContext, localResourceGroupSymbols);

        return new MainBicepEmissionResult(sb.ToString(), tracker.Build());

        static IReadOnlyList<(string ModuleFolderName, string TypeName)> GetUsedModuleTypeImports(
            IReadOnlyCollection<GeneratedTypeModule> generatedModules)
        {
            return generatedModules
                .Where(module => module.ParameterTypeOverrides.Count > 0)
                .SelectMany(module => module.ParameterTypeOverrides
                    .Where(parameterTypeOverride => module.Parameters.ContainsKey(parameterTypeOverride.Key))
                    .Select(parameterTypeOverride => (module.ModuleFolderName, TypeName: parameterTypeOverride.Value)))
                .Distinct()
                .ToList();
        }

        static IReadOnlyDictionary<string, string> BuildImportedModuleTypeNames(
            IReadOnlyList<(string ModuleFolderName, string TypeName)> moduleTypeImports)
        {
            var collidingTypeNames = moduleTypeImports
                .GroupBy(x => x.TypeName, StringComparer.Ordinal)
                .Where(group => group.Select(x => x.ModuleFolderName).Distinct(StringComparer.OrdinalIgnoreCase).Count() > 1)
                .Select(group => group.Key)
                .ToHashSet(StringComparer.Ordinal);

            return moduleTypeImports.ToDictionary(
                x => BuildModuleTypeImportKey(x.ModuleFolderName, x.TypeName),
                x => collidingTypeNames.Contains(x.TypeName)
                    ? $"{x.ModuleFolderName}{x.TypeName}"
                    : x.TypeName,
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

    private const string UserAssignedIdentityTypeName = "UserAssignedIdentity";

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
            foreach (var (key, value) in module.Parameters)
            {
                var bicepType = module.ParameterTypeOverrides.TryGetValue(key, out var customType)
                    ? ResolveImportedTypeName(importedModuleTypeNames, module.ModuleFolderName, customType)
                    : BicepFormattingHelper.InferBicepType(value);
                sb.AppendLine($"param {module.ModuleName}{BicepFormattingHelper.Capitalize(key)} {bicepType}");
            }

            foreach (var secureParam in module.SecureParameters)
            {
                sb.AppendLine();
                sb.AppendLine("@secure()");
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
        foreach (var setting in appSettings.Where(s => s.EnvironmentValues is { Count: > 0 }))
        {
            var paramName = BicepNamingHelper.GetStaticAppSettingParamName(setting.TargetResourceName, setting.Name);
            sb.AppendLine($"param {paramName} string");
        }

        foreach (var setting in appSettings.Where(s =>
            s.IsKeyVaultReference && !s.IsSensitiveOutputExportedToKeyVault
            && s.SecretValueAssignment == "ViaBicepparam"))
        {
            var paramName = BicepNamingHelper.GetSecureAppSettingParamName(setting.TargetResourceName, setting.SecretName!);
            sb.AppendLine();
            sb.AppendLine("@secure()");
            sb.AppendLine($"@description('Secret value for Key Vault secret \\'{BicepFormattingHelper.EscapeBicepString(setting.SecretName!)}\\' used by {setting.TargetResourceName}')");
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

        var unionParts = new List<string>();
        if (hasProjectTags) unionParts.Add("projectTags");
        if (hasConfigTags) unionParts.Add("configTags");
        unionParts.Add("env.tags");

        sb.AppendLine($"var tags = union({string.Join(", ", unionParts)})");
        sb.AppendLine();
    }

    private static void AppendTagsVariable(
        StringBuilder sb,
        string variableName,
        IReadOnlyDictionary<string, string> tags)
    {
        sb.AppendLine($"var {variableName} = {{");
        foreach (var (tagKey, tagValue) in tags)
        {
            sb.AppendLine($"  {BicepFormattingHelper.FormatBicepObjectKey(tagKey)}: '{BicepFormattingHelper.EscapeBicepString(tagValue)}'");
        }
        sb.AppendLine("}");
        sb.AppendLine();
    }

    private static void AppendResourceGroupDeclarations(
        StringBuilder sb,
        IReadOnlyList<ResourceGroupDefinition> resourceGroups,
        NamingContext namingContext)
    {
        foreach (var rg in resourceGroups)
        {
            var rgSymbol = BicepIdentifierHelper.ToBicepIdentifier(rg.Name);
            var nameExpr = BicepNamingHelper.BuildNamingExpression(
                rg.Name, rg.ResourceAbbreviation, "ResourceGroup", namingContext);

            sb.AppendLine($"resource {rgSymbol} 'Microsoft.Resources/resourceGroups@2024-07-01' = {{");
            sb.AppendLine($"  name: {nameExpr}");
            sb.AppendLine("  location: env.location");
            sb.AppendLine("  tags: tags");
            sb.AppendLine("}");
            sb.AppendLine();
        }
    }

    private static void AppendExistingResourceDeclarations(
        StringBuilder sb,
        IReadOnlyList<ExistingResourceReference> existingResourceReferences,
        IReadOnlyList<RoleAssignmentDefinition> roleAssignments,
        NamingContext namingContext,
        HashSet<string> localResourceGroupSymbols)
    {
        var externalRgs = existingResourceReferences
            .Select(r => r.ResourceGroupName)
            .Concat(roleAssignments
                .Where(ra => ra.IsTargetCrossConfig
                    || !localResourceGroupSymbols.Contains(BicepIdentifierHelper.ToBicepIdentifier(ra.TargetResourceGroupName)))
                .Select(ra => ra.TargetResourceGroupName))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (externalRgs.Count == 0)
            return;

        sb.AppendLine("// â”€â”€ Cross-configuration existing resource groups â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€");
        foreach (var extRgName in externalRgs)
        {
            var extRgSymbol = $"existing_{BicepIdentifierHelper.ToBicepIdentifier(extRgName)}";
            var nameExprRg = BicepNamingHelper.BuildNamingExpression(extRgName, "rg", "ResourceGroup", namingContext);

            sb.AppendLine($"resource {extRgSymbol} 'Microsoft.Resources/resourceGroups@2024-07-01' existing = {{");
            sb.AppendLine($"  name: {nameExprRg}");
            sb.AppendLine("}");
            sb.AppendLine();
        }

        if (existingResourceReferences.Count == 0)
            return;

        sb.AppendLine("// â”€â”€ Cross-configuration existing resources â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€");
        foreach (var extRef in existingResourceReferences)
        {
            var extSymbol = $"existing_{BicepIdentifierHelper.ToBicepIdentifier(extRef.ResourceName)}";
            var extRgSymbol = $"existing_{BicepIdentifierHelper.ToBicepIdentifier(extRef.ResourceGroupName)}";
            var nameExprRes = BicepNamingHelper.BuildNamingExpression(
                extRef.ResourceName, extRef.ResourceAbbreviation,
                extRef.ResourceTypeName, namingContext);

            var apiVersion = ResourceTypeMetadata.GetExistingResourceApiVersion(extRef.ResourceType);

            sb.AppendLine($"resource {extSymbol} '{extRef.ResourceType}@{apiVersion}' existing = {{");
            sb.AppendLine($"  name: {nameExprRes}");
            sb.AppendLine($"  scope: {extRgSymbol}");
            sb.AppendLine("}");
            sb.AppendLine();
        }
    }

    private static void AppendKeyVaultSecretsSection(
        StringBuilder sb,
        OutputUsageTracker tracker,
        IReadOnlyCollection<GeneratedTypeModule> modules,
        IReadOnlyList<AppSettingDefinition> appSettings,
        NamingContext namingContext)
    {
        var allKvSecrets = new List<(string KvResourceName, string SecretName, string ValueExpr)>();
        CollectSensitiveOutputKvSecrets(tracker, modules, appSettings, allKvSecrets);
        CollectViaBicepparamKvSecrets(appSettings, allKvSecrets);

        var secretsByKv = allKvSecrets
            .GroupBy(s => s.KvResourceName, StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (secretsByKv.Count == 0)
            return;

        sb.AppendLine("// â”€â”€ Key Vault secrets (batch per Key Vault) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€");
        sb.AppendLine();

        foreach (var kvGroup in secretsByKv)
        {
            AppendKeyVaultSecretsModule(sb, tracker, modules, kvGroup, namingContext);
        }
    }

    private static void CollectSensitiveOutputKvSecrets(
        OutputUsageTracker tracker,
        IReadOnlyCollection<GeneratedTypeModule> modules,
        IReadOnlyList<AppSettingDefinition> appSettings,
        List<(string KvResourceName, string SecretName, string ValueExpr)> target)
    {
        foreach (var export in appSettings
            .Where(s => s.IsSensitiveOutputExportedToKeyVault
                && s.KeyVaultResourceName is not null
                && s.SecretName is not null
                && s.SourceResourceName is not null
                && s.SourceOutputBicepExpression is not null))
        {
            var sourceModule = modules.FirstOrDefault(m =>
                m.LogicalResourceName.Equals(export.SourceResourceName!, StringComparison.OrdinalIgnoreCase)
                && (export.SourceResourceTypeName is null
                    || m.ResourceTypeName.Equals(export.SourceResourceTypeName, StringComparison.OrdinalIgnoreCase)));
            if (sourceModule is null)
                continue;

            var sourceSymbol = $"{sourceModule.ModuleName}Module";
            tracker.RegisterUsage(sourceSymbol, export.SourceOutputName!);
            target.Add((
                export.KeyVaultResourceName!,
                export.SecretName!,
                $"{sourceSymbol}.outputs.{export.SourceOutputName}"));
        }
    }

    private static void CollectViaBicepparamKvSecrets(
        IReadOnlyList<AppSettingDefinition> appSettings,
        List<(string KvResourceName, string SecretName, string ValueExpr)> target)
    {
        foreach (var secret in appSettings
            .Where(s => s.IsKeyVaultReference && !s.IsSensitiveOutputExportedToKeyVault
                && s.SecretValueAssignment == "ViaBicepparam"
                && s.KeyVaultResourceName is not null
                && s.SecretName is not null))
        {
            var secureParamName = BicepNamingHelper.GetSecureAppSettingParamName(secret.TargetResourceName, secret.SecretName!);
            target.Add((secret.KeyVaultResourceName!, secret.SecretName!, secureParamName));
        }
    }

    private static void AppendKeyVaultSecretsModule(
        StringBuilder sb,
        OutputUsageTracker tracker,
        IReadOnlyCollection<GeneratedTypeModule> modules,
        IGrouping<string, (string KvResourceName, string SecretName, string ValueExpr)> kvGroup,
        NamingContext namingContext)
    {
        var kvModule = modules.FirstOrDefault(m =>
            m.LogicalResourceName.Equals(kvGroup.Key, StringComparison.OrdinalIgnoreCase)
            && m.ResourceTypeName.Equals(AzureResourceTypes.KeyVault, StringComparison.OrdinalIgnoreCase));
        if (kvModule is null) return;

        var kvIdentifier = BicepIdentifierHelper.ToBicepIdentifier(kvGroup.Key);
        var kvSecretsModuleSymbol = $"{kvIdentifier}KvSecretsModule";
        const string KvSecretsModulePath = "modules/KeyVault/kvSecrets.module.bicep";
        tracker.RegisterModulePath(kvSecretsModuleSymbol, KvSecretsModulePath);
        var kvRgSymbol = BicepIdentifierHelper.ToBicepIdentifier(kvModule.ResourceGroupName);
        var kvNameExpr = BicepNamingHelper.BuildNamingExpression(
            kvModule.LogicalResourceName, kvModule.ResourceAbbreviation,
            kvModule.ResourceTypeName, namingContext);

        sb.AppendLine($"module {kvSecretsModuleSymbol} './{KvSecretsModulePath}' = {{");
        sb.AppendLine($"  name: '{kvIdentifier}-kv-secrets'");
        sb.AppendLine($"  scope: {kvRgSymbol}");
        sb.AppendLine("  params: {");
        sb.AppendLine($"    keyVaultName: {kvNameExpr}");
        sb.AppendLine("    secrets: [");
        foreach (var (_, secretName, valueExpr) in kvGroup)
        {
            sb.AppendLine("      {");
            sb.AppendLine($"        name: '{BicepFormattingHelper.EscapeBicepString(secretName)}'");
            sb.AppendLine($"        value: {valueExpr}");
            sb.AppendLine("      }");
        }
        sb.AppendLine("    ]");
        sb.AppendLine("  }");
        sb.AppendLine("  dependsOn: [");
        sb.AppendLine($"    {kvModule.ModuleName}Module");
        sb.AppendLine("  ]");
        sb.AppendLine("}");
        sb.AppendLine();
    }

    private static void AppendRoleAssignmentModules(
        StringBuilder sb,
        OutputUsageTracker tracker,
        IReadOnlyCollection<GeneratedTypeModule> modules,
        IReadOnlyList<RoleAssignmentDefinition> roleAssignments,
        NamingContext namingContext,
        HashSet<string> localResourceGroupSymbols)
    {
        if (roleAssignments.Count == 0)
            return;

        var grouped = RoleAssignmentAssembler.GroupRoleAssignments(roleAssignments);

        foreach (var group in grouped)
        {
            AppendRoleAssignmentModule(sb, tracker, modules, group, namingContext, localResourceGroupSymbols);
        }
    }

    private static void AppendRoleAssignmentModule(
        StringBuilder sb,
        OutputUsageTracker tracker,
        IReadOnlyCollection<GeneratedTypeModule> modules,
        GroupedRoleAssignment group,
        NamingContext namingContext,
        HashSet<string> localResourceGroupSymbols)
    {
        var sourceIdentifier = BicepIdentifierHelper.ToBicepIdentifier(group.SourceResourceName);
        var targetIdentifier = BicepIdentifierHelper.ToBicepIdentifier(group.TargetResourceName);
        var moduleSymbol = $"{ResourceTypeMetadata.GetBaseModuleName(group.SourceResourceType)}{BicepFormattingHelper.Capitalize(sourceIdentifier)}" +
            $"{ResourceTypeMetadata.GetBaseModuleName(group.TargetResourceType)}{BicepFormattingHelper.Capitalize(targetIdentifier)}Roles";

        var targetFolder = ResourceTypeMetadata.GetModuleFolderName(group.TargetResourceTypeName);
        var moduleFileName = RoleAssignmentModuleTemplates.GetModuleFileName(group.TargetResourceTypeName);

        var targetRgIdentifier = BicepIdentifierHelper.ToBicepIdentifier(group.TargetResourceGroupName);
        var usesExistingTargetScope = group.IsTargetCrossConfig
            || !localResourceGroupSymbols.Contains(targetRgIdentifier);
        var targetRgSymbol = usesExistingTargetScope
            ? $"existing_{targetRgIdentifier}"
            : targetRgIdentifier;

        var targetNameExpr = BicepNamingHelper.BuildNamingExpression(
            group.TargetResourceName, group.TargetResourceAbbreviation,
            group.TargetResourceTypeName, namingContext);

        var principalIdExpr = RoleAssignmentAssembler.ResolvePrincipalIdExpression(group, modules, tracker);

        var roleModulePath = $"modules/{targetFolder}/{moduleFileName}";
        tracker.RegisterModulePath(moduleSymbol, roleModulePath);
        sb.AppendLine($"module {moduleSymbol} './{roleModulePath}' = {{");
        sb.AppendLine($"  name: '{moduleSymbol}'");
        sb.AppendLine($"  scope: {targetRgSymbol}");
        sb.AppendLine("  params: {");
        sb.AppendLine($"    name: {targetNameExpr}");
        sb.AppendLine($"    principalId: {principalIdExpr}");
        sb.AppendLine("    roles: [");

        foreach (var role in group.Roles)
        {
            var rolePropertyAccess = BicepFormattingHelper.FormatBicepPropertyAccess(role.RoleDefinitionName);
            sb.AppendLine($"      RbacRoles.{role.ServiceCategory}{rolePropertyAccess}");
        }

        sb.AppendLine("    ]");
        sb.AppendLine("  }");
        sb.AppendLine("}");
        sb.AppendLine();
    }

    private static void AppendModuleDeclaration(
        StringBuilder sb,
        OutputUsageTracker tracker,
        GeneratedTypeModule module,
        IReadOnlyCollection<GeneratedTypeModule> modules,
        NamingContext namingContext,
        Dictionary<(string SourceResourceName, string SourceResourceTypeName), List<string>> uaiBySourceResource,
        Dictionary<string, List<AppSettingDefinition>> appSettingsByTarget)
    {
        var rgSymbol = BicepIdentifierHelper.ToBicepIdentifier(module.ResourceGroupName);
        var moduleSymbol = $"{module.ModuleName}Module";
        var modulePath = $"modules/{module.ModuleFolderName}/{module.ModuleFileName}";
        tracker.RegisterModulePath(moduleSymbol, modulePath);
        var nameExpr = BicepNamingHelper.BuildNamingExpression(
            module.LogicalResourceName, module.ResourceAbbreviation,
            module.ResourceTypeName, namingContext);

        sb.AppendLine($"module {moduleSymbol} './{modulePath}' = {{");
        sb.AppendLine($"  name: '{module.ModuleName}'");
        sb.AppendLine($"  scope: {rgSymbol}");
        sb.AppendLine("  params: {");
        sb.AppendLine("    location: env.location");
        sb.AppendLine($"    name: {nameExpr}");
        sb.AppendLine("    tags: tags");

        AppendModuleScalarParameters(sb, module);
        AppendModuleSecureParameters(sb, module);
        AppendModuleParentIdReferences(sb, tracker, module, modules);
        AppendModuleExistingResourceIdReferences(sb, module);
        AppendModuleParentNameReferences(sb, module, modules, namingContext);
        AppendModuleIdentityParameters(sb, tracker, module, modules, uaiBySourceResource);
        AppendModuleAppSettingsBlock(sb, tracker, module, modules, appSettingsByTarget);

        sb.AppendLine("  }");
        sb.AppendLine("}");
        sb.AppendLine();

        foreach (var companion in module.CompanionModules)
        {
            StorageAccountCompanionHelper.AppendStorageAccountCompanionModule(sb, module, companion, rgSymbol, nameExpr);
        }
    }

    private static void AppendModuleScalarParameters(StringBuilder sb, GeneratedTypeModule module)
    {
        foreach (var paramKey in module.Parameters.Keys)
        {
            sb.AppendLine($"    {paramKey}: {module.ModuleName}{BicepFormattingHelper.Capitalize(paramKey)}");
        }
    }

    private static void AppendModuleSecureParameters(StringBuilder sb, GeneratedTypeModule module)
    {
        foreach (var secureParam in module.SecureParameters)
        {
            sb.AppendLine($"    {secureParam}: {module.ModuleName}{BicepFormattingHelper.Capitalize(secureParam)}");
        }
    }

    private static void AppendModuleParentIdReferences(
        StringBuilder sb,
        OutputUsageTracker tracker,
        GeneratedTypeModule module,
        IReadOnlyCollection<GeneratedTypeModule> modules)
    {
        foreach (var (paramName, (parentLogicalName, parentResourceType)) in module.ParentModuleIdReferences)
        {
            var parentModule = modules.FirstOrDefault(m =>
                m.LogicalResourceName.Equals(parentLogicalName, StringComparison.OrdinalIgnoreCase)
                && m.ResourceTypeName.Equals(parentResourceType, StringComparison.OrdinalIgnoreCase));
            if (parentModule is null)
                continue;

            var parentSymbol = $"{parentModule.ModuleName}Module";
            tracker.RegisterUsage(parentSymbol, "id");
            sb.AppendLine($"    {paramName}: {parentSymbol}.outputs.id");
        }
    }

    private static void AppendModuleExistingResourceIdReferences(StringBuilder sb, GeneratedTypeModule module)
    {
        foreach (var (paramName, existingResourceName) in module.ExistingResourceIdReferences)
        {
            var existingSymbol = $"existing_{BicepIdentifierHelper.ToBicepIdentifier(existingResourceName)}";
            sb.AppendLine($"    {paramName}: {existingSymbol}.id");
        }
    }

    private static void AppendModuleParentNameReferences(
        StringBuilder sb,
        GeneratedTypeModule module,
        IReadOnlyCollection<GeneratedTypeModule> modules,
        NamingContext namingContext)
    {
        foreach (var (paramName, (parentLogicalName, parentResourceType)) in module.ParentModuleNameReferences)
        {
            var parentModule = modules.FirstOrDefault(m =>
                m.LogicalResourceName.Equals(parentLogicalName, StringComparison.OrdinalIgnoreCase)
                && m.ResourceTypeName.Equals(parentResourceType, StringComparison.OrdinalIgnoreCase));
            if (parentModule is null)
                continue;

            var parentNameExpr = BicepNamingHelper.BuildNamingExpression(
                parentModule.LogicalResourceName, parentModule.ResourceAbbreviation,
                parentModule.ResourceTypeName, namingContext);
            sb.AppendLine($"    {paramName}: {parentNameExpr}");
        }
    }

    private static void AppendModuleIdentityParameters(
        StringBuilder sb,
        OutputUsageTracker tracker,
        GeneratedTypeModule module,
        IReadOnlyCollection<GeneratedTypeModule> modules,
        Dictionary<(string SourceResourceName, string SourceResourceTypeName), List<string>> uaiBySourceResource)
    {
        var moduleKey = (module.LogicalResourceName, module.ResourceTypeName);

        if (module.UsesParameterizedIdentity)
        {
            var identityKind = module.IdentityKind ?? "SystemAssigned";
            sb.AppendLine($"    identityType: '{identityKind}'");
        }

        if (module.ResourceTypeName == UserAssignedIdentityTypeName)
            return;

        if (!uaiBySourceResource.TryGetValue(moduleKey, out var uaiNames))
            return;

        var uaiName = uaiNames[0];
        var uaiModule = modules.FirstOrDefault(m =>
            m.ResourceTypeName == UserAssignedIdentityTypeName
            && m.LogicalResourceName.Equals(uaiName, StringComparison.OrdinalIgnoreCase));
        if (uaiModule is null)
            return;

        var uaiSymbol = $"{uaiModule.ModuleName}Module";
        tracker.RegisterUsage(uaiSymbol, "resourceId");
        sb.AppendLine($"    userAssignedIdentityId: {uaiSymbol}.outputs.resourceId");
    }

    private static void AppendModuleAppSettingsBlock(
        StringBuilder sb,
        OutputUsageTracker tracker,
        GeneratedTypeModule module,
        IReadOnlyCollection<GeneratedTypeModule> modules,
        Dictionary<string, List<AppSettingDefinition>> appSettingsByTarget)
    {
        var isComputeModule = module.ResourceTypeName is AzureResourceTypes.WebApp or AzureResourceTypes.FunctionApp or AzureResourceTypes.ContainerApp;
        if (!isComputeModule)
            return;

        if (!appSettingsByTarget.TryGetValue(module.LogicalResourceName, out var resourceAppSettings))
            return;

        var isContainerApp = module.ResourceTypeName == AzureResourceTypes.ContainerApp;
        var paramName = isContainerApp ? "envVars" : "appSettings";

        sb.AppendLine($"    {paramName}: [");
        foreach (var setting in resourceAppSettings)
        {
            sb.AppendLine("      {");
            sb.AppendLine($"        name: '{setting.Name}'");
            AppendAppSettingValueLine(sb, tracker, setting, module, modules);
            sb.AppendLine("      }");
        }
        sb.AppendLine("    ]");
    }

    private static void ValidateKeyVaultSecretNames(IReadOnlyList<AppSettingDefinition> appSettings)
    {
        foreach (var setting in appSettings.Where(RequiresKeyVaultSecretValidation))
        {
            if (KeyVaultSecretNameRules.IsValid(setting.SecretName!))
            {
                continue;
            }

            throw new InvalidOperationException(
                $"Key Vault secret name '{setting.SecretName}' for resource '{setting.TargetResourceName}' is invalid. {KeyVaultSecretNameRules.ValidationMessage}");
        }
    }

    private static bool RequiresKeyVaultSecretValidation(AppSettingDefinition setting)
    {
        return setting.SecretName is not null
            && (setting.IsKeyVaultReference || setting.IsSensitiveOutputExportedToKeyVault);
    }

    private static void AppendAppSettingValueLine(
        StringBuilder sb,
        OutputUsageTracker tracker,
        AppSettingDefinition setting,
        GeneratedTypeModule module,
        IReadOnlyCollection<GeneratedTypeModule> modules)
    {
        if (TryAppendSensitiveKeyVaultExportValue(sb, tracker, setting))
            return;
        if (TryAppendKeyVaultReferenceValue(sb, tracker, setting, modules))
            return;
        if (TryAppendOutputReferenceValue(sb, tracker, setting, modules))
            return;
        if (setting.EnvironmentValues is { Count: > 0 })
        {
            var settingParamName = BicepNamingHelper.GetStaticAppSettingParamName(module.LogicalResourceName, setting.Name);
            sb.AppendLine($"        value: {settingParamName}");
            return;
        }
        if (setting.StaticValue is not null)
        {
            sb.AppendLine($"        value: '{BicepFormattingHelper.EscapeBicepString(setting.StaticValue)}'");
        }
    }

    private static bool TryAppendSensitiveKeyVaultExportValue(
        StringBuilder sb,
        OutputUsageTracker tracker,
        AppSettingDefinition setting)
    {
        if (!setting.IsSensitiveOutputExportedToKeyVault
            || setting.KeyVaultResourceName is null
            || setting.SecretName is null)
        {
            return false;
        }

        var kvIdentifier = BicepIdentifierHelper.ToBicepIdentifier(setting.KeyVaultResourceName);
        var kvSecretsModuleSymbol = $"{kvIdentifier}KvSecretsModule";
        var secretPropertyAccess = BicepFormattingHelper.FormatBicepPropertyAccess(setting.SecretName);
        tracker.RegisterUsage(kvSecretsModuleSymbol, "secretUris");
        sb.AppendLine($"        value: '@Microsoft.KeyVault(SecretUri=${{{kvSecretsModuleSymbol}.outputs.secretUris{secretPropertyAccess}}})'");
        return true;
    }

    private static bool TryAppendKeyVaultReferenceValue(
        StringBuilder sb,
        OutputUsageTracker tracker,
        AppSettingDefinition setting,
        IReadOnlyCollection<GeneratedTypeModule> modules)
    {
        if (!setting.IsKeyVaultReference
            || setting.KeyVaultResourceName is null
            || setting.SecretName is null)
        {
            return false;
        }

        if (setting.SecretValueAssignment == "ViaBicepparam")
        {
            var kvIdentifier = BicepIdentifierHelper.ToBicepIdentifier(setting.KeyVaultResourceName);
            var kvSecretsModuleSymbol = $"{kvIdentifier}KvSecretsModule";
            var secretPropertyAccess = BicepFormattingHelper.FormatBicepPropertyAccess(setting.SecretName);
            tracker.RegisterUsage(kvSecretsModuleSymbol, "secretUris");
            sb.AppendLine($"        value: '@Microsoft.KeyVault(SecretUri=${{{kvSecretsModuleSymbol}.outputs.secretUris{secretPropertyAccess}}})'");
            return true;
        }

        var kvModule = modules.FirstOrDefault(m =>
            m.LogicalResourceName.Equals(setting.KeyVaultResourceName, StringComparison.OrdinalIgnoreCase)
            && m.ResourceTypeName.Equals(AzureResourceTypes.KeyVault, StringComparison.OrdinalIgnoreCase));
        if (kvModule is null)
            return true;

        var kvSymbol = $"{kvModule.ModuleName}Module";
        tracker.RegisterUsage(kvSymbol, "vaultUri");
        sb.AppendLine($"        value: '@Microsoft.KeyVault(SecretUri=${{{kvSymbol}.outputs.vaultUri}}secrets/{BicepFormattingHelper.EscapeBicepString(setting.SecretName)})'");
        return true;
    }

    private static bool TryAppendOutputReferenceValue(
        StringBuilder sb,
        OutputUsageTracker tracker,
        AppSettingDefinition setting,
        IReadOnlyCollection<GeneratedTypeModule> modules)
    {
        if (!setting.IsOutputReference || setting.SourceResourceName is null)
            return false;

        if (setting.IsSourceCrossConfig)
        {
            var extSymbol = $"existing_{BicepIdentifierHelper.ToBicepIdentifier(setting.SourceResourceName)}";
            var propertyPath = BicepNamingHelper.StripResourceSymbolPrefix(setting.SourceOutputBicepExpression)
                               ?? $"properties.{setting.SourceOutputName}";
            sb.AppendLine($"        value: {extSymbol}.{propertyPath}");
            return true;
        }

        var sourceModule = modules.FirstOrDefault(m =>
            m.LogicalResourceName.Equals(setting.SourceResourceName, StringComparison.OrdinalIgnoreCase)
            && (setting.SourceResourceTypeName is null
                || m.ResourceTypeName.Equals(setting.SourceResourceTypeName, StringComparison.OrdinalIgnoreCase)));
        if (sourceModule is null)
            return true;

        var sourceSymbol = $"{sourceModule.ModuleName}Module";
        tracker.RegisterUsage(sourceSymbol, setting.SourceOutputName!);
        sb.AppendLine($"        value: {sourceSymbol}.outputs.{setting.SourceOutputName}");
        return true;
    }
}
