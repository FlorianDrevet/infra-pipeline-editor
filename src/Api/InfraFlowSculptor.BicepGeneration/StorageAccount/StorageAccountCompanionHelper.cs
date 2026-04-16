using System.Text;
using InfraFlowSculptor.BicepGeneration.Helpers;
using InfraFlowSculptor.BicepGeneration.Models;

namespace InfraFlowSculptor.BicepGeneration.StorageAccount;

/// <summary>
/// Helpers for rendering Storage Account companion modules (blob containers, CORS, lifecycle rules)
/// in main.bicep and .bicepparam files.
/// </summary>
internal static class StorageAccountCompanionHelper
{
    internal static IEnumerable<string> RenderCorsRules(IReadOnlyCollection<BlobCorsRuleData> corsRules, int indentLevel = 0)
    {
        if (corsRules.Count == 0)
        {
            yield return "[]";
            yield break;
        }

        var indent = new string(' ', indentLevel);
        var itemIndent = new string(' ', indentLevel + 2);
        var propertyIndent = new string(' ', indentLevel + 4);

        yield return "[";
        foreach (var rule in corsRules)
        {
            yield return $"{itemIndent}{{";
            yield return $"{propertyIndent}allowedOrigins: {BicepFormattingHelper.RenderBicepStringArray(rule.AllowedOrigins)}";
            yield return $"{propertyIndent}allowedMethods: {BicepFormattingHelper.RenderBicepStringArray(rule.AllowedMethods)}";
            yield return $"{propertyIndent}allowedHeaders: {BicepFormattingHelper.RenderBicepStringArray(rule.AllowedHeaders)}";
            yield return $"{propertyIndent}exposedHeaders: {BicepFormattingHelper.RenderBicepStringArray(rule.ExposedHeaders)}";
            yield return $"{propertyIndent}maxAgeInSeconds: {rule.MaxAgeInSeconds}";
            yield return $"{itemIndent}}}";
        }

        yield return $"{indent}]";
    }

    internal static string GetStorageAccountCorsParameterName(
        GeneratedTypeModule module,
        GeneratedCompanionModule companion) =>
        $"{module.ModuleName}{companion.ModuleSymbolSuffix}CorsRules";

    internal static IEnumerable<(string Name, string Description, IReadOnlyCollection<BlobCorsRuleData> Value)> GetStorageAccountCorsParameters(
        GeneratedTypeModule module)
    {
        foreach (var companion in module.CompanionModules)
        {
            if (companion.CorsRules.Count > 0)
            {
                yield return (
                    GetStorageAccountCorsParameterName(module, companion),
                    $"Blob service CORS rules for storage account '{module.LogicalResourceName}'",
                    companion.CorsRules);
            }

            if (companion.TableCorsRules.Count > 0)
            {
                yield return (
                    GetStorageAccountCorsParameterName(module, companion),
                    $"Table service CORS rules for storage account '{module.LogicalResourceName}'",
                    companion.TableCorsRules);
            }
        }
    }

    internal static void AppendCorsParameterAssignment(
        StringBuilder sb,
        string parameterName,
        IReadOnlyCollection<BlobCorsRuleData> corsRules)
    {
        if (corsRules.Count == 0)
        {
            sb.AppendLine($"param {parameterName} = []");
            return;
        }

        sb.AppendLine($"param {parameterName} = ");
        foreach (var line in RenderCorsRules(corsRules))
        {
            sb.AppendLine(line);
        }
    }

    internal static string GetStorageAccountLifecycleParameterName(
        GeneratedTypeModule module,
        GeneratedCompanionModule companion) =>
        $"{module.ModuleName}{companion.ModuleSymbolSuffix}LifecycleRules";

    internal static IEnumerable<(string Name, string Description, IReadOnlyCollection<ContainerLifecycleRuleData> Value)> GetStorageAccountLifecycleParameters(
        GeneratedTypeModule module)
    {
        foreach (var companion in module.CompanionModules)
        {
            if (companion.LifecycleRules.Count > 0)
            {
                yield return (
                    GetStorageAccountLifecycleParameterName(module, companion),
                    $"Blob lifecycle management rules for storage account '{module.LogicalResourceName}'",
                    companion.LifecycleRules);
            }
        }
    }

    internal static IEnumerable<string> RenderLifecycleRules(IReadOnlyCollection<ContainerLifecycleRuleData> rules, int indentLevel = 0)
    {
        if (rules.Count == 0)
        {
            yield return "[]";
            yield break;
        }

        var indent = new string(' ', indentLevel);
        var itemIndent = new string(' ', indentLevel + 2);
        var propertyIndent = new string(' ', indentLevel + 4);

        yield return "[";
        foreach (var rule in rules)
        {
            yield return $"{itemIndent}{{";
            yield return $"{propertyIndent}ruleName: '{BicepFormattingHelper.EscapeBicepString(rule.RuleName)}'";
            yield return $"{propertyIndent}containerNames: {BicepFormattingHelper.RenderBicepStringArray(rule.ContainerNames)}";
            yield return $"{propertyIndent}timeToLiveInDays: {rule.TimeToLiveInDays}";
            yield return $"{itemIndent}}}";
        }

        yield return $"{indent}]";
    }

    internal static void AppendLifecycleParameterAssignment(
        StringBuilder sb,
        string parameterName,
        IReadOnlyCollection<ContainerLifecycleRuleData> rules)
    {
        if (rules.Count == 0)
        {
            sb.AppendLine($"param {parameterName} = []");
            return;
        }

        sb.AppendLine($"param {parameterName} = ");
        foreach (var line in RenderLifecycleRules(rules))
        {
            sb.AppendLine(line);
        }
    }

    internal static void AppendStorageAccountCompanionModule(
        StringBuilder sb,
        GeneratedTypeModule module,
        GeneratedCompanionModule companion,
        string resourceGroupSymbol,
        string storageAccountNameExpression)
    {
        var companionSymbol = $"{module.ModuleName}{companion.ModuleSymbolSuffix}Module";
        var deploymentName = $"{module.ModuleName}{companion.DeploymentNameSuffix}";

        sb.AppendLine($"module {companionSymbol} './modules/{companion.FolderName}/{companion.FileName}' = {{");
        sb.AppendLine($"  name: '{deploymentName}'");
        sb.AppendLine($"  scope: {resourceGroupSymbol}");
        sb.AppendLine("  params: {");
        sb.AppendLine($"    storageAccountName: {storageAccountNameExpression}");

        if (companion.BlobContainerNames.Count > 0)
        {
            sb.AppendLine($"    blobContainerNames: {BicepFormattingHelper.RenderBicepStringArray(companion.BlobContainerNames)}");
        }

        if (companion.CorsRules.Count > 0)
        {
            sb.AppendLine($"    corsRules: {GetStorageAccountCorsParameterName(module, companion)}");
        }

        if (companion.StorageTableNames.Count > 0)
        {
            sb.AppendLine($"    tableNames: {BicepFormattingHelper.RenderBicepStringArray(companion.StorageTableNames)}");
        }

        if (companion.TableCorsRules.Count > 0)
        {
            sb.AppendLine($"    corsRules: {GetStorageAccountCorsParameterName(module, companion)}");
        }

        if (companion.QueueNames.Count > 0)
        {
            sb.AppendLine($"    queueNames: {BicepFormattingHelper.RenderBicepStringArray(companion.QueueNames)}");
        }

        if (companion.LifecycleRules.Count > 0)
        {
            sb.AppendLine($"    containerLifecycleRules: {GetStorageAccountLifecycleParameterName(module, companion)}");
        }

        sb.AppendLine("  }");
        sb.AppendLine("}");
        sb.AppendLine();
    }
}
