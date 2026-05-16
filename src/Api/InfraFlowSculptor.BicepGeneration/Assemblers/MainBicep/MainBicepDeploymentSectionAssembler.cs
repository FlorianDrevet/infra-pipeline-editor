using System.Text;
using InfraFlowSculptor.BicepGeneration.Constants;
using InfraFlowSculptor.BicepGeneration.Generators;
using InfraFlowSculptor.BicepGeneration.Helpers;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.GenerationCore;

namespace InfraFlowSculptor.BicepGeneration.Assemblers.MainBicep;

/// <summary>
/// Emits the deployment-oriented sections of <c>main.bicep</c> outside module declarations.
/// </summary>
internal static class MainBicepDeploymentSectionAssembler
{
    private const string CrossConfigurationExistingResourceGroupsHeader =
        "// -- Cross-configuration existing resource groups --------------------------";

    private const string CrossConfigurationExistingResourcesHeader =
        "// -- Cross-configuration existing resources --------------------------------";

    private const string KeyVaultSecretsHeader =
        "// -- Key Vault secrets (batch per Key Vault) ------------------------------";

    internal static void AppendResourceGroupDeclarations(
        StringBuilder sb,
        IReadOnlyList<ResourceGroupDefinition> resourceGroups,
        NamingContext namingContext)
    {
        foreach (var rg in resourceGroups)
        {
            var rgSymbol = BicepIdentifierHelper.ToBicepIdentifier(rg.Name);
            var nameExpr = BicepNamingHelper.BuildNamingExpression(
                rg.Name, rg.ResourceAbbreviation, "ResourceGroup", namingContext);

            sb.AppendLine($"resource {rgSymbol} '{BicepArmTypeCatalog.ResourceGroupsArmType}' = {{");
            sb.AppendLine($"  name: {nameExpr}");
            sb.AppendLine("  location: env.location");
            sb.AppendLine("  tags: tags");
            sb.AppendLine("}");
            sb.AppendLine();
        }
    }

    internal static void AppendExistingResourceDeclarations(
        StringBuilder sb,
        IReadOnlyList<ExistingResourceReference> existingResourceReferences,
        IReadOnlyList<RoleAssignmentDefinition> roleAssignments,
        NamingContext namingContext,
        HashSet<string> localResourceGroupSymbols)
    {
        var externalRgs = existingResourceReferences
            .Select(reference => reference.ResourceGroupName)
            .Concat(roleAssignments
                .Where(roleAssignment => roleAssignment.IsTargetCrossConfig
                    || !localResourceGroupSymbols.Contains(BicepIdentifierHelper.ToBicepIdentifier(roleAssignment.TargetResourceGroupName)))
                .Select(roleAssignment => roleAssignment.TargetResourceGroupName))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (externalRgs.Count == 0)
        {
            return;
        }

        sb.AppendLine(CrossConfigurationExistingResourceGroupsHeader);
        foreach (var extRgName in externalRgs)
        {
            var extRgSymbol = $"existing_{BicepIdentifierHelper.ToBicepIdentifier(extRgName)}";
            var nameExprRg = BicepNamingHelper.BuildNamingExpression(extRgName, "rg", "ResourceGroup", namingContext);

            sb.AppendLine($"resource {extRgSymbol} '{BicepArmTypeCatalog.ResourceGroupsArmType}' existing = {{");
            sb.AppendLine($"  name: {nameExprRg}");
            sb.AppendLine("}");
            sb.AppendLine();
        }

        if (existingResourceReferences.Count == 0)
        {
            return;
        }

        sb.AppendLine(CrossConfigurationExistingResourcesHeader);
        foreach (var extRef in existingResourceReferences)
        {
            var extSymbol = $"existing_{BicepIdentifierHelper.ToBicepIdentifier(extRef.ResourceName)}";
            var extRgSymbol = $"existing_{BicepIdentifierHelper.ToBicepIdentifier(extRef.ResourceGroupName)}";
            var nameExprRes = BicepNamingHelper.BuildNamingExpression(
                extRef.ResourceName,
                extRef.ResourceAbbreviation,
                extRef.ResourceTypeName,
                namingContext);

            var apiVersion = ResourceTypeMetadata.GetExistingResourceApiVersion(extRef.ResourceType);

            sb.AppendLine($"resource {extSymbol} '{extRef.ResourceType}@{apiVersion}' existing = {{");
            sb.AppendLine($"  name: {nameExprRes}");
            sb.AppendLine($"  scope: {extRgSymbol}");
            sb.AppendLine("}");
            sb.AppendLine();
        }
    }

    internal static void AppendKeyVaultSecretsSection(
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
            .GroupBy(secret => secret.KvResourceName, StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (secretsByKv.Count == 0)
        {
            return;
        }

        sb.AppendLine(KeyVaultSecretsHeader);
        sb.AppendLine();

        foreach (var kvGroup in secretsByKv)
        {
            AppendKeyVaultSecretsModule(sb, tracker, modules, kvGroup, namingContext);
        }
    }

    internal static void AppendRoleAssignmentModules(
        StringBuilder sb,
        OutputUsageTracker tracker,
        IReadOnlyCollection<GeneratedTypeModule> modules,
        IReadOnlyList<RoleAssignmentDefinition> roleAssignments,
        NamingContext namingContext,
        HashSet<string> localResourceGroupSymbols)
    {
        if (roleAssignments.Count == 0)
        {
            return;
        }

        var groupedAssignments = RoleAssignmentAssembler.GroupRoleAssignments(roleAssignments);

        foreach (var group in groupedAssignments)
        {
            AppendRoleAssignmentModule(sb, tracker, modules, group, namingContext, localResourceGroupSymbols);
        }
    }

    private static void CollectSensitiveOutputKvSecrets(
        OutputUsageTracker tracker,
        IReadOnlyCollection<GeneratedTypeModule> modules,
        IReadOnlyList<AppSettingDefinition> appSettings,
        List<(string KvResourceName, string SecretName, string ValueExpr)> target)
    {
        foreach (var export in appSettings
            .Where(setting => setting.IsSensitiveOutputExportedToKeyVault
                && setting.KeyVaultResourceName is not null
                && setting.SecretName is not null
                && setting.SourceResourceName is not null
                && setting.SourceOutputBicepExpression is not null))
        {
            var sourceModule = modules.FirstOrDefault(module =>
                module.LogicalResourceName.Equals(export.SourceResourceName!, StringComparison.OrdinalIgnoreCase)
                && (export.SourceResourceTypeName is null
                    || module.ResourceTypeName.Equals(export.SourceResourceTypeName, StringComparison.OrdinalIgnoreCase)));
            if (sourceModule is null)
            {
                continue;
            }

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
            .Where(setting => setting.IsKeyVaultReference
                && !setting.IsSensitiveOutputExportedToKeyVault
                && setting.SecretValueAssignment == "ViaBicepparam"
                && setting.KeyVaultResourceName is not null
                && setting.SecretName is not null))
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
        var kvModule = modules.FirstOrDefault(module =>
            module.LogicalResourceName.Equals(kvGroup.Key, StringComparison.OrdinalIgnoreCase)
            && module.ResourceTypeName.Equals(AzureResourceTypes.KeyVault, StringComparison.OrdinalIgnoreCase));
        if (kvModule is null)
        {
            return;
        }

        var kvIdentifier = BicepIdentifierHelper.ToBicepIdentifier(kvGroup.Key);
        var kvSecretsModuleSymbol = $"{kvIdentifier}KvSecretsModule";
        const string kvSecretsModulePath = "modules/KeyVault/kvSecrets.module.bicep";
        tracker.RegisterModulePath(kvSecretsModuleSymbol, kvSecretsModulePath);
        var kvRgSymbol = BicepIdentifierHelper.ToBicepIdentifier(kvModule.ResourceGroupName);
        var kvNameExpr = BicepNamingHelper.BuildNamingExpression(
            kvModule.LogicalResourceName,
            kvModule.ResourceAbbreviation,
            kvModule.ResourceTypeName,
            namingContext);

        sb.AppendLine($"module {kvSecretsModuleSymbol} './{kvSecretsModulePath}' = {{");
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
        var moduleSymbol =
            $"{ResourceTypeMetadata.GetBaseModuleName(group.SourceResourceType)}{BicepFormattingHelper.Capitalize(sourceIdentifier)}" +
            $"{ResourceTypeMetadata.GetBaseModuleName(group.TargetResourceType)}{BicepFormattingHelper.Capitalize(targetIdentifier)}Roles";

        var targetFolder = ResourceTypeMetadata.GetModuleFolderName(group.TargetResourceTypeName);
        var moduleFileName = RoleAssignmentModuleTemplates.GetModuleFileName(group.TargetResourceTypeName);

        var targetRgIdentifier = BicepIdentifierHelper.ToBicepIdentifier(group.TargetResourceGroupName);
        var usesExistingTargetScope = group.IsTargetCrossConfig || !localResourceGroupSymbols.Contains(targetRgIdentifier);
        var targetRgSymbol = usesExistingTargetScope
            ? $"existing_{targetRgIdentifier}"
            : targetRgIdentifier;

        var targetNameExpr = BicepNamingHelper.BuildNamingExpression(
            group.TargetResourceName,
            group.TargetResourceAbbreviation,
            group.TargetResourceTypeName,
            namingContext);

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
}