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
        var localResourceGroupSymbols = resourceGroups
            .Select(rg => BicepIdentifierHelper.ToBicepIdentifier(rg.Name))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        sb.AppendLine("targetScope = 'subscription'");
        sb.AppendLine();

        // ── Imports ─────────────────────────────────────────────────────────
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
        var importedModuleTypeNames = BuildImportedModuleTypeNames(modules);
        var moduleTypeImports = modules
            .Where(m => m.ParameterTypeOverrides.Count > 0)
            .SelectMany(module => module.ParameterTypeOverrides.Values
                .Distinct(StringComparer.Ordinal)
                .Select(typeName => (module.ModuleFolderName, TypeName: typeName)))
            .Distinct()
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

        // ── Parameters ──────────────────────────────────────────────────────
        sb.AppendLine("@description('The target deployment environment')");
        sb.AppendLine("param environmentName EnvironmentName");
        sb.AppendLine();

        // Resource-specific parameter declarations (sku, capacity, etc.)
        foreach (var module in modules)
        {
            foreach (var (key, value) in module.Parameters)
            {
                var bicepType = module.ParameterTypeOverrides.TryGetValue(key, out var customType)
                    ? ResolveImportedTypeName(importedModuleTypeNames, module.ModuleFolderName, customType)
                    : BicepFormattingHelper.InferBicepType(value);
                sb.AppendLine($"param {module.ModuleName}{BicepFormattingHelper.Capitalize(key)} {bicepType}");
            }

            // Secure parameters (e.g. passwords) — @secure() string with no default
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

        // Static app setting parameter declarations
        foreach (var setting in appSettings.Where(s => s.EnvironmentValues is { Count: > 0 }))
        {
            var paramName = BicepNamingHelper.GetStaticAppSettingParamName(setting.TargetResourceName, setting.Name);
            sb.AppendLine($"param {paramName} string");
        }

        // Secure parameter declarations for ViaBicepparam Key Vault secrets
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

        sb.AppendLine();

        // ── Environment resolution ──────────────────────────────────────────
        sb.AppendLine("var env = environments[environmentName]");
        sb.AppendLine();

        // ── Tags merging (project → config → environment) ──────────────────
        var hasProjectTags = projectTags is { Count: > 0 };
        var hasConfigTags = configTags is { Count: > 0 };

        if (hasProjectTags || hasConfigTags)
        {
            if (hasProjectTags)
            {
                sb.AppendLine("var projectTags = {");
                foreach (var (tagKey, tagValue) in projectTags!)
                {
                    sb.AppendLine($"  {BicepFormattingHelper.FormatBicepObjectKey(tagKey)}: '{BicepFormattingHelper.EscapeBicepString(tagValue)}'");
                }
                sb.AppendLine("}");
                sb.AppendLine();
            }

            if (hasConfigTags)
            {
                sb.AppendLine("var configTags = {");
                foreach (var (tagKey, tagValue) in configTags!)
                {
                    sb.AppendLine($"  {BicepFormattingHelper.FormatBicepObjectKey(tagKey)}: '{BicepFormattingHelper.EscapeBicepString(tagValue)}'");
                }
                sb.AppendLine("}");
                sb.AppendLine();
            }

            // Merge: environment tags override config tags, which override project tags
            var unionParts = new List<string>();
            if (hasProjectTags) unionParts.Add("projectTags");
            if (hasConfigTags) unionParts.Add("configTags");
            unionParts.Add("env.tags");

            sb.AppendLine($"var tags = union({string.Join(", ", unionParts)})");
        }
        else
        {
            sb.AppendLine("var tags = env.tags");
        }
        sb.AppendLine();

        // ── Resource group declarations ─────────────────────────────────────
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

        // ── Existing resource declarations (cross-config references) ────────
        var externalRgs = existingResourceReferences
            .Select(r => r.ResourceGroupName)
            .Concat(roleAssignments
                .Where(ra => ra.IsTargetCrossConfig
                    || !localResourceGroupSymbols.Contains(BicepIdentifierHelper.ToBicepIdentifier(ra.TargetResourceGroupName)))
                .Select(ra => ra.TargetResourceGroupName))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (externalRgs.Count > 0)
        {
            sb.AppendLine("// ── Cross-configuration existing resource groups ──────────────────");
            foreach (var extRgName in externalRgs)
            {
                var extRgSymbol = $"existing_{BicepIdentifierHelper.ToBicepIdentifier(extRgName)}";
                var nameExprRg = BicepNamingHelper.BuildNamingExpression(extRgName, "rg", "ResourceGroup", namingContext);

                sb.AppendLine($"resource {extRgSymbol} 'Microsoft.Resources/resourceGroups@2024-07-01' existing = {{");
                sb.AppendLine($"  name: {nameExprRg}");
                sb.AppendLine("}");
                sb.AppendLine();
            }

            if (existingResourceReferences.Count > 0)
            {
                sb.AppendLine("// ── Cross-configuration existing resources ──────────────────────");
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
        }

        // ── Module declarations ─────────────────────────────────────────────
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

            foreach (var paramKey in module.Parameters.Keys)
            {
                sb.AppendLine($"    {paramKey}: {module.ModuleName}{BicepFormattingHelper.Capitalize(paramKey)}");
            }

            // ── Secure parameters (passwords, secrets) ──
            foreach (var secureParam in module.SecureParameters)
            {
                sb.AppendLine($"    {secureParam}: {module.ModuleName}{BicepFormattingHelper.Capitalize(secureParam)}");
            }

            // ── Parent module ID references ──
            foreach (var (paramName, (parentLogicalName, parentResourceType)) in module.ParentModuleIdReferences)
            {
                var parentModule = modules.FirstOrDefault(m =>
                    m.LogicalResourceName.Equals(parentLogicalName, StringComparison.OrdinalIgnoreCase)
                    && m.ResourceTypeName.Equals(parentResourceType, StringComparison.OrdinalIgnoreCase));
                if (parentModule is not null)
                {
                    var parentSymbol = $"{parentModule.ModuleName}Module";
                    tracker.RegisterUsage(parentSymbol, "id");
                    sb.AppendLine($"    {paramName}: {parentSymbol}.outputs.id");
                }
            }

            // ── Cross-config existing resource ID references ──
            foreach (var (paramName, existingResourceName) in module.ExistingResourceIdReferences)
            {
                var existingSymbol = $"existing_{BicepIdentifierHelper.ToBicepIdentifier(existingResourceName)}";
                sb.AppendLine($"    {paramName}: {existingSymbol}.id");
            }

            // ── Parent module name references ──
            foreach (var (paramName, (parentLogicalName, parentResourceType)) in module.ParentModuleNameReferences)
            {
                var parentModule = modules.FirstOrDefault(m =>
                    m.LogicalResourceName.Equals(parentLogicalName, StringComparison.OrdinalIgnoreCase)
                    && m.ResourceTypeName.Equals(parentResourceType, StringComparison.OrdinalIgnoreCase));
                if (parentModule is not null)
                {
                    var parentNameExpr = BicepNamingHelper.BuildNamingExpression(
                        parentModule.LogicalResourceName, parentModule.ResourceAbbreviation,
                        parentModule.ResourceTypeName, namingContext);
                    sb.AppendLine($"    {paramName}: {parentNameExpr}");
                }
            }

            // ── Identity params ─────────────────────────────────────────────
            var moduleKey = (module.LogicalResourceName, module.ResourceTypeName);

            if (module.UsesParameterizedIdentity)
            {
                var identityKind = module.IdentityKind ?? "SystemAssigned";
                sb.AppendLine($"    identityType: '{identityKind}'");

                if (module.ResourceTypeName != "UserAssignedIdentity"
                    && uaiBySourceResource.TryGetValue(moduleKey, out var uaiNamesParam))
                {
                    // Single generic param — use first UAI for this resource
                    var uaiName = uaiNamesParam[0];
                    var uaiModSym = modules.FirstOrDefault(m =>
                        m.ResourceTypeName == "UserAssignedIdentity"
                        && m.LogicalResourceName.Equals(uaiName, StringComparison.OrdinalIgnoreCase));
                    if (uaiModSym is not null)
                    {
                        var uaiSymbol = $"{uaiModSym.ModuleName}Module";
                        tracker.RegisterUsage(uaiSymbol, "resourceId");
                        sb.AppendLine($"    userAssignedIdentityId: {uaiSymbol}.outputs.resourceId");
                    }
                }
            }
            else
            {
                if (module.ResourceTypeName != "UserAssignedIdentity"
                    && uaiBySourceResource.TryGetValue(moduleKey, out var uaiNames))
                {
                    // Single generic param — use first UAI for this resource
                    var uaiName = uaiNames[0];
                    var uaiModuleSymbol = modules.FirstOrDefault(m =>
                        m.ResourceTypeName == "UserAssignedIdentity"
                        && m.LogicalResourceName.Equals(uaiName, StringComparison.OrdinalIgnoreCase));
                    if (uaiModuleSymbol is not null)
                    {
                        var uaiSymbol = $"{uaiModuleSymbol.ModuleName}Module";
                        tracker.RegisterUsage(uaiSymbol, "resourceId");
                        sb.AppendLine($"    userAssignedIdentityId: {uaiSymbol}.outputs.resourceId");
                    }
                }
            }

            // Inject appSettings / envVars param for compute modules only
            var isComputeModule = module.ResourceTypeName is AzureResourceTypes.WebApp or AzureResourceTypes.FunctionApp or AzureResourceTypes.ContainerApp;
            if (isComputeModule
                && appSettingsByTarget.TryGetValue(module.LogicalResourceName, out var resourceAppSettings))
            {
                var isContainerApp = module.ResourceTypeName == AzureResourceTypes.ContainerApp;
                var paramName2 = isContainerApp ? "envVars" : "appSettings";

                sb.AppendLine($"    {paramName2}: [");
                foreach (var setting in resourceAppSettings)
                {
                    sb.AppendLine("      {");
                    sb.AppendLine($"        name: '{setting.Name}'");

                    if (setting.IsSensitiveOutputExportedToKeyVault
                        && setting.KeyVaultResourceName is not null
                        && setting.SecretName is not null)
                    {
                        var kvIdentifier = BicepIdentifierHelper.ToBicepIdentifier(setting.KeyVaultResourceName);
                        var kvSecretsModuleSymbol = $"{kvIdentifier}KvSecretsModule";
                        tracker.RegisterUsage(kvSecretsModuleSymbol, "secretUris");
                        sb.AppendLine($"        value: '@Microsoft.KeyVault(SecretUri=${{{kvSecretsModuleSymbol}.outputs.secretUris.{BicepFormattingHelper.EscapeBicepString(setting.SecretName)}}})'");
                    }
                    else if (setting.IsKeyVaultReference && setting.KeyVaultResourceName is not null && setting.SecretName is not null)
                    {
                        if (setting.SecretValueAssignment == "ViaBicepparam")
                        {
                            var kvIdentifier = BicepIdentifierHelper.ToBicepIdentifier(setting.KeyVaultResourceName);
                            var kvSecretsModuleSymbol = $"{kvIdentifier}KvSecretsModule";
                            tracker.RegisterUsage(kvSecretsModuleSymbol, "secretUris");
                            sb.AppendLine($"        value: '@Microsoft.KeyVault(SecretUri=${{{kvSecretsModuleSymbol}.outputs.secretUris.{BicepFormattingHelper.EscapeBicepString(setting.SecretName)}}})'");
                        }
                        else
                        {
                            var kvModule = modules.FirstOrDefault(m =>
                                m.LogicalResourceName.Equals(setting.KeyVaultResourceName, StringComparison.OrdinalIgnoreCase)
                                && m.ResourceTypeName.Equals(AzureResourceTypes.KeyVault, StringComparison.OrdinalIgnoreCase));
                            if (kvModule is not null)
                            {
                                var kvSymbol = $"{kvModule.ModuleName}Module";
                                tracker.RegisterUsage(kvSymbol, "vaultUri");
                                sb.AppendLine($"        value: '@Microsoft.KeyVault(SecretUri=${{{kvSymbol}.outputs.vaultUri}}secrets/{BicepFormattingHelper.EscapeBicepString(setting.SecretName)})'");
                            }
                        }
                    }
                    else if (setting.IsOutputReference && setting.SourceResourceName is not null)
                    {
                        if (setting.IsSourceCrossConfig)
                        {
                            var extSymbol = $"existing_{BicepIdentifierHelper.ToBicepIdentifier(setting.SourceResourceName)}";
                            var propertyPath = BicepNamingHelper.StripResourceSymbolPrefix(setting.SourceOutputBicepExpression)
                                               ?? $"properties.{setting.SourceOutputName}";
                            sb.AppendLine($"        value: {extSymbol}.{propertyPath}");
                        }
                        else
                        {
                            var sourceModule = modules.FirstOrDefault(m =>
                                m.LogicalResourceName.Equals(setting.SourceResourceName, StringComparison.OrdinalIgnoreCase)
                                && (setting.SourceResourceTypeName is null
                                    || m.ResourceTypeName.Equals(setting.SourceResourceTypeName, StringComparison.OrdinalIgnoreCase)));
                            if (sourceModule is not null)
                            {
                                var sourceSymbol = $"{sourceModule.ModuleName}Module";
                                tracker.RegisterUsage(sourceSymbol, setting.SourceOutputName!);
                                sb.AppendLine($"        value: {sourceSymbol}.outputs.{setting.SourceOutputName}");
                            }
                        }
                    }
                    else if (setting.EnvironmentValues is { Count: > 0 })
                    {
                        var settingParamName = BicepNamingHelper.GetStaticAppSettingParamName(module.LogicalResourceName, setting.Name);
                        sb.AppendLine($"        value: {settingParamName}");
                    }
                    else if (setting.StaticValue is not null)
                    {
                        sb.AppendLine($"        value: '{BicepFormattingHelper.EscapeBicepString(setting.StaticValue)}'");
                    }

                    sb.AppendLine("      }");
                }
                sb.AppendLine("    ]");
            }

            sb.AppendLine("  }");
            sb.AppendLine("}");
            sb.AppendLine();

            foreach (var companion in module.CompanionModules)
            {
                StorageAccountCompanionHelper.AppendStorageAccountCompanionModule(sb, module, companion, rgSymbol, nameExpr);
            }
        }

        // ── Key Vault secrets (batch per Key Vault) ──────────────────────────
        var allKvSecrets = new List<(string KvResourceName, string SecretName, string ValueExpr)>();

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
            if (sourceModule is not null)
            {
                var sourceSymbol = $"{sourceModule.ModuleName}Module";
                tracker.RegisterUsage(sourceSymbol, export.SourceOutputName!);
                allKvSecrets.Add((
                    export.KeyVaultResourceName!,
                    export.SecretName!,
                    $"{sourceSymbol}.outputs.{export.SourceOutputName}"));
            }
        }

        foreach (var secret in appSettings
            .Where(s => s.IsKeyVaultReference && !s.IsSensitiveOutputExportedToKeyVault
                && s.SecretValueAssignment == "ViaBicepparam"
                && s.KeyVaultResourceName is not null
                && s.SecretName is not null))
        {
            var secureParamName = BicepNamingHelper.GetSecureAppSettingParamName(secret.TargetResourceName, secret.SecretName!);
            allKvSecrets.Add((secret.KeyVaultResourceName!, secret.SecretName!, secureParamName));
        }

        var secretsByKv = allKvSecrets
            .GroupBy(s => s.KvResourceName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (secretsByKv.Count > 0)
        {
            sb.AppendLine("// ── Key Vault secrets (batch per Key Vault) ────────────────────────────");
            sb.AppendLine();

            foreach (var kvGroup in secretsByKv)
            {
                var kvModule = modules.FirstOrDefault(m =>
                    m.LogicalResourceName.Equals(kvGroup.Key, StringComparison.OrdinalIgnoreCase)
                    && m.ResourceTypeName.Equals(AzureResourceTypes.KeyVault, StringComparison.OrdinalIgnoreCase));
                if (kvModule is null) continue;

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
        }

        // ── Role assignment module declarations ─────────────────────────────
        if (roleAssignments.Count > 0)
        {
            var grouped = RoleAssignmentAssembler.GroupRoleAssignments(roleAssignments);

            foreach (var group in grouped)
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
                    sb.AppendLine($"      RbacRoles.{role.ServiceCategory}['{role.RoleDefinitionName}']");
                }

                sb.AppendLine("    ]");
                sb.AppendLine("  }");
                sb.AppendLine("}");
                sb.AppendLine();
            }
        }

        return new MainBicepEmissionResult(sb.ToString(), tracker.Build());

        static IReadOnlyDictionary<string, string> BuildImportedModuleTypeNames(IReadOnlyCollection<GeneratedTypeModule> generatedModules)
        {
            var moduleTypeImports = generatedModules
                .Where(m => m.ParameterTypeOverrides.Count > 0)
                .SelectMany(module => module.ParameterTypeOverrides.Values
                    .Distinct(StringComparer.Ordinal)
                    .Select(typeName => (module.ModuleFolderName, TypeName: typeName)))
                .Distinct()
                .ToList();

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

        static string ResolveImportedTypeName(
            IReadOnlyDictionary<string, string> importedTypeNames,
            string moduleFolderName,
            string typeName)
        {
            return importedTypeNames.TryGetValue(BuildModuleTypeImportKey(moduleFolderName, typeName), out var importedTypeName)
                ? importedTypeName
                : typeName;
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
}
