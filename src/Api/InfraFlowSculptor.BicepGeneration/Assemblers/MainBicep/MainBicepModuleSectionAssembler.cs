using System.Text;
using InfraFlowSculptor.BicepGeneration.Helpers;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.BicepGeneration.StorageAccount;
using InfraFlowSculptor.GenerationCore;

namespace InfraFlowSculptor.BicepGeneration.Assemblers.MainBicep;

/// <summary>
/// Emits module declarations and app-setting value resolution for <c>main.bicep</c>.
/// </summary>
internal static class MainBicepModuleSectionAssembler
{
    private const string IdOutputName = "id";
    private const string UserAssignedIdentityTypeName = "UserAssignedIdentity";

    internal static void ValidateKeyVaultSecretNames(IReadOnlyList<AppSettingDefinition> appSettings)
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

    internal static void AppendModuleDeclaration(
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
            module.LogicalResourceName,
            module.ResourceAbbreviation,
            module.ResourceTypeName,
            namingContext);

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
        AppendModuleParentOutputReferences(sb, tracker, module, modules);
        AppendModuleExistingResourceIdReferences(sb, module);
        AppendModuleExistingResourcePropertyReferences(sb, module);
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
        foreach (var paramKey in GetDeclaredParameters(module).Select(parameter => parameter.Key))
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
            var parentModule = modules.FirstOrDefault(candidate =>
                candidate.LogicalResourceName.Equals(parentLogicalName, StringComparison.OrdinalIgnoreCase)
                && candidate.ResourceTypeName.Equals(parentResourceType, StringComparison.OrdinalIgnoreCase));
            if (parentModule is null)
            {
                continue;
            }

            var parentSymbol = $"{parentModule.ModuleName}Module";
            tracker.RegisterUsage(parentSymbol, IdOutputName);
            sb.AppendLine($"    {paramName}: {parentSymbol}.outputs.{IdOutputName}");
        }
    }

    private static void AppendModuleParentOutputReferences(
        StringBuilder sb,
        OutputUsageTracker tracker,
        GeneratedTypeModule module,
        IReadOnlyCollection<GeneratedTypeModule> modules)
    {
        foreach (var (paramName, (parentLogicalName, parentResourceType, outputName)) in module.ParentModuleOutputReferences)
        {
            var parentModule = modules.FirstOrDefault(candidate =>
                candidate.LogicalResourceName.Equals(parentLogicalName, StringComparison.OrdinalIgnoreCase)
                && candidate.ResourceTypeName.Equals(parentResourceType, StringComparison.OrdinalIgnoreCase));
            if (parentModule is null)
            {
                continue;
            }

            var parentSymbol = $"{parentModule.ModuleName}Module";
            tracker.RegisterUsage(parentSymbol, outputName);
            sb.AppendLine($"    {paramName}: {parentSymbol}.outputs.{outputName}");
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

    private static void AppendModuleExistingResourcePropertyReferences(StringBuilder sb, GeneratedTypeModule module)
    {
        foreach (var (paramName, (resourceName, propertyPath)) in module.ExistingResourcePropertyReferences)
        {
            var existingSymbol = $"existing_{BicepIdentifierHelper.ToBicepIdentifier(resourceName)}";
            sb.AppendLine($"    {paramName}: {existingSymbol}.{propertyPath}");
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
            var parentModule = modules.FirstOrDefault(candidate =>
                candidate.LogicalResourceName.Equals(parentLogicalName, StringComparison.OrdinalIgnoreCase)
                && candidate.ResourceTypeName.Equals(parentResourceType, StringComparison.OrdinalIgnoreCase));
            if (parentModule is null)
            {
                continue;
            }

            var parentNameExpr = BicepNamingHelper.BuildNamingExpression(
                parentModule.LogicalResourceName,
                parentModule.ResourceAbbreviation,
                parentModule.ResourceTypeName,
                namingContext);
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
        {
            return;
        }

        if (!uaiBySourceResource.TryGetValue(moduleKey, out var uaiNames))
        {
            return;
        }

        var uaiName = uaiNames[0];
        var uaiModule = modules.FirstOrDefault(candidate =>
            candidate.ResourceTypeName == UserAssignedIdentityTypeName
            && candidate.LogicalResourceName.Equals(uaiName, StringComparison.OrdinalIgnoreCase));
        if (uaiModule is null)
        {
            return;
        }

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
        {
            return;
        }

        if (!appSettingsByTarget.TryGetValue(module.LogicalResourceName, out var resourceAppSettings))
        {
            return;
        }

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
        {
            return;
        }

        if (TryAppendKeyVaultReferenceValue(sb, tracker, setting, modules))
        {
            return;
        }

        if (TryAppendOutputReferenceValue(sb, tracker, setting, modules))
        {
            return;
        }

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

        var kvModule = modules.FirstOrDefault(candidate =>
            candidate.LogicalResourceName.Equals(setting.KeyVaultResourceName, StringComparison.OrdinalIgnoreCase)
            && candidate.ResourceTypeName.Equals(AzureResourceTypes.KeyVault, StringComparison.OrdinalIgnoreCase));
        if (kvModule is null)
        {
            return true;
        }

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
        {
            return false;
        }

        if (setting.IsSourceCrossConfig)
        {
            var extSymbol = $"existing_{BicepIdentifierHelper.ToBicepIdentifier(setting.SourceResourceName)}";
            var propertyPath = BicepNamingHelper.StripResourceSymbolPrefix(setting.SourceOutputBicepExpression)
                ?? $"properties.{setting.SourceOutputName}";
            sb.AppendLine($"        value: {extSymbol}.{propertyPath}");
            return true;
        }

        var sourceModule = modules.FirstOrDefault(candidate =>
            candidate.LogicalResourceName.Equals(setting.SourceResourceName, StringComparison.OrdinalIgnoreCase)
            && (setting.SourceResourceTypeName is null
                || candidate.ResourceTypeName.Equals(setting.SourceResourceTypeName, StringComparison.OrdinalIgnoreCase)));
        if (sourceModule is null)
        {
            return true;
        }

        var sourceSymbol = $"{sourceModule.ModuleName}Module";
        tracker.RegisterUsage(sourceSymbol, setting.SourceOutputName!);
        sb.AppendLine($"        value: {sourceSymbol}.outputs.{setting.SourceOutputName}");
        return true;
    }
}