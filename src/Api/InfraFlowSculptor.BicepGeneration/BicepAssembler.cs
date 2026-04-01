using System.Text;
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
        var typesBicep = GenerateTypesBicep(environments, hasRoleAssignments);
        var functionsBicep = GenerateFunctionsBicep(namingContext);
        var constantsBicep = hasRoleAssignments ? GenerateConstantsBicep(roleAssignments) : string.Empty;
        var main = GenerateMainBicep(modules, resourceGroups, namingContext, roleAssignments, appSettings, existingResourceReferences ?? [], projectTags, configTags);

        var environmentParameterFiles = GenerateEnvironmentParameterFiles(
            modules, environmentNames, resources, appSettings);

        var moduleFiles = new Dictionary<string, string>();

        foreach (var module in modules.DistinctBy(m => m.ModuleFileName))
        {
            var folder = module.ModuleFolderName;
            var bicepContentWithHeader = AddModuleHeader(
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
                moduleFiles[companionPath] = AddModuleHeader(
                    module.ResourceTypeName,
                    companion.FileName,
                    companion.BicepContent);

                if (!string.IsNullOrWhiteSpace(companion.TypesBicepContent))
                {
                    var typesPath = $"modules/{companion.FolderName}/types.bicep";
                    moduleFiles[typesPath] = moduleFiles.TryGetValue(typesPath, out var existingTypes)
                        ? MergeTypesContent(existingTypes, companion.TypesBicepContent)
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

            var folder = GetModuleFolderName(typeName);
            var fileName = RoleAssignmentModuleTemplates.GetModuleFileName(typeName);
            moduleFiles[$"modules/{folder}/{fileName}"] = RoleAssignmentModuleTemplates.GenerateModule(typeName);
        }

        // Generate Key Vault secret module if sensitive outputs are exported to KV or ViaBicepparam secrets exist
        var hasSensitiveExports = appSettings.Any(s => s.IsSensitiveOutputExportedToKeyVault);
        var hasViaBicepparamSecrets = appSettings.Any(s =>
            s.IsKeyVaultReference && !s.IsSensitiveOutputExportedToKeyVault
            && s.SecretValueAssignment == "ViaBicepparam");
        if (hasSensitiveExports || hasViaBicepparamSecrets)
        {
            moduleFiles["modules/KeyVault/kvSecret.module.bicep"] = GenerateKvSecretModule();
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
    /// Returns the module folder name for a given resource type name (matches existing generator conventions).
    /// </summary>
    private static string GetModuleFolderName(string resourceTypeName) =>
        resourceTypeName switch
        {
            AzureResourceTypes.KeyVault => AzureResourceTypes.KeyVault,
            AzureResourceTypes.RedisCache => AzureResourceTypes.RedisCache,
            AzureResourceTypes.StorageAccount => AzureResourceTypes.StorageAccount,
            AzureResourceTypes.AppServicePlan => AzureResourceTypes.AppServicePlan,
            AzureResourceTypes.WebApp => AzureResourceTypes.WebApp,
            AzureResourceTypes.FunctionApp => AzureResourceTypes.FunctionApp,
            AzureResourceTypes.UserAssignedIdentity => AzureResourceTypes.UserAssignedIdentity,
            AzureResourceTypes.AppConfiguration => AzureResourceTypes.AppConfiguration,
            AzureResourceTypes.ContainerAppEnvironment => AzureResourceTypes.ContainerAppEnvironment,
            AzureResourceTypes.ContainerApp => AzureResourceTypes.ContainerApp,
            AzureResourceTypes.LogAnalyticsWorkspace => AzureResourceTypes.LogAnalyticsWorkspace,
            AzureResourceTypes.ApplicationInsights => AzureResourceTypes.ApplicationInsights,
            AzureResourceTypes.CosmosDb => AzureResourceTypes.CosmosDb,
            AzureResourceTypes.SqlServer => AzureResourceTypes.SqlServer,
            AzureResourceTypes.SqlDatabase => AzureResourceTypes.SqlDatabase,
            _ => resourceTypeName
        };

    /// <summary>
    /// Returns a human-readable display name for the resource type.
    /// </summary>
    private static string GetResourceTypeDisplayName(string resourceTypeName) =>
        resourceTypeName switch
        {
            AzureResourceTypes.KeyVault => "Key Vault",
            AzureResourceTypes.RedisCache => "Redis Cache",
            AzureResourceTypes.StorageAccount => "Storage Account",
            AzureResourceTypes.AppServicePlan => "App Service Plan",
            AzureResourceTypes.WebApp => "Web App",
            AzureResourceTypes.FunctionApp => "Function App",
            AzureResourceTypes.UserAssignedIdentity => "User Assigned Identity",
            AzureResourceTypes.AppConfiguration => "App Configuration",
            AzureResourceTypes.ContainerAppEnvironment => "Container App Environment",
            AzureResourceTypes.ContainerApp => "Container App",
            AzureResourceTypes.LogAnalyticsWorkspace => "Log Analytics Workspace",
            AzureResourceTypes.ApplicationInsights => "Application Insights",
            AzureResourceTypes.CosmosDb => "Cosmos DB",
            AzureResourceTypes.SqlServer => "SQL Server",
            AzureResourceTypes.SqlDatabase => "SQL Database",
            AzureResourceTypes.ServiceBusNamespace => "Service Bus Namespace",
            _ => resourceTypeName
        };

    /// <summary>
    /// Returns the Microsoft Learn documentation URL for the resource type.
    /// </summary>
    private static string GetResourceTypeDocumentationUrl(string resourceTypeName) =>
        resourceTypeName switch
        {
            AzureResourceTypes.KeyVault => "https://learn.microsoft.com/en-us/azure/templates/microsoft.keyvault/vaults",
            AzureResourceTypes.RedisCache => "https://learn.microsoft.com/en-us/azure/templates/microsoft.cache/redis",
            AzureResourceTypes.StorageAccount => "https://learn.microsoft.com/en-us/azure/templates/microsoft.storage/storageaccounts",
            AzureResourceTypes.AppServicePlan => "https://learn.microsoft.com/en-us/azure/templates/microsoft.web/serverfarms",
            AzureResourceTypes.WebApp => "https://learn.microsoft.com/en-us/azure/templates/microsoft.web/sites",
            AzureResourceTypes.FunctionApp => "https://learn.microsoft.com/en-us/azure/templates/microsoft.web/sites",
            AzureResourceTypes.UserAssignedIdentity => "https://learn.microsoft.com/en-us/azure/templates/microsoft.managedidentity/userassignedidentities",
            AzureResourceTypes.AppConfiguration => "https://learn.microsoft.com/en-us/azure/templates/microsoft.appconfiguration/configurationstores",
            AzureResourceTypes.ContainerAppEnvironment => "https://learn.microsoft.com/en-us/azure/templates/microsoft.app/managedenvironments",
            AzureResourceTypes.ContainerApp => "https://learn.microsoft.com/en-us/azure/templates/microsoft.app/containerapps",
            AzureResourceTypes.LogAnalyticsWorkspace => "https://learn.microsoft.com/en-us/azure/templates/microsoft.operationalinsights/workspaces",
            AzureResourceTypes.ApplicationInsights => "https://learn.microsoft.com/en-us/azure/templates/microsoft.insights/components",
            AzureResourceTypes.CosmosDb => "https://learn.microsoft.com/en-us/azure/templates/microsoft.documentdb/databaseaccounts",
            AzureResourceTypes.SqlServer => "https://learn.microsoft.com/en-us/azure/templates/microsoft.sql/servers",
            AzureResourceTypes.SqlDatabase => "https://learn.microsoft.com/en-us/azure/templates/microsoft.sql/servers/databases",
            AzureResourceTypes.ServiceBusNamespace => "https://learn.microsoft.com/en-us/azure/templates/microsoft.servicebus/namespaces",
            _ => string.Empty
        };

    /// <summary>
    /// Generates a formatted module header comment with description and documentation link.
    /// </summary>
    private static string GenerateModuleHeader(
        string resourceTypeName,
        string moduleFileName)
    {
        var displayName = GetResourceTypeDisplayName(resourceTypeName);
        var docUrl = GetResourceTypeDocumentationUrl(resourceTypeName);

        var sb = new StringBuilder();
        sb.AppendLine("// =======================================================================");
        sb.AppendLine($"// {displayName} Module");
        sb.AppendLine("// -----------------------------------------------------------------------");
        sb.AppendLine($"// Module: {moduleFileName}");
        sb.AppendLine($"// Description: Deploys an Azure {displayName} resource");
        if (!string.IsNullOrEmpty(docUrl))
        {
            sb.AppendLine($"// See: {docUrl}");
        }
        sb.AppendLine("// =======================================================================");
        sb.AppendLine();

        return sb.ToString();
    }

    /// <summary>
    /// Adds a formatted header to the module Bicep content.
    /// </summary>
    private static string AddModuleHeader(
        string resourceTypeName,
        string moduleFileName,
        string bicepContent)
    {
        var header = GenerateModuleHeader(resourceTypeName, moduleFileName);
        return header + bicepContent;
    }

    private static string MergeTypesContent(string existingContent, string additionalContent)
    {
        if (string.IsNullOrWhiteSpace(additionalContent) || existingContent.Contains(additionalContent, StringComparison.Ordinal))
        {
            return existingContent;
        }

        return $"{existingContent.TrimEnd()}\n\n{additionalContent.Trim()}\n";
    }

    private static string RenderBicepStringArray(IReadOnlyList<string> values)
    {
        if (values.Count == 0)
        {
            return "[]";
        }

        return $"[ {string.Join(", ", values.Select(v => $"'{EscapeBicepString(v)}'"))} ]";
    }

    private static IEnumerable<string> RenderCorsRules(IReadOnlyList<BlobCorsRuleData> corsRules, int indentLevel = 0)
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
            yield return $"{propertyIndent}allowedOrigins: {RenderBicepStringArray(rule.AllowedOrigins)}";
            yield return $"{propertyIndent}allowedMethods: {RenderBicepStringArray(rule.AllowedMethods)}";
            yield return $"{propertyIndent}allowedHeaders: {RenderBicepStringArray(rule.AllowedHeaders)}";
            yield return $"{propertyIndent}exposedHeaders: {RenderBicepStringArray(rule.ExposedHeaders)}";
            yield return $"{propertyIndent}maxAgeInSeconds: {rule.MaxAgeInSeconds}";
            yield return $"{itemIndent}}}";
        }

        yield return $"{indent}]";
    }

    private static string GetStorageAccountCorsParameterName(
        GeneratedTypeModule module,
        GeneratedCompanionModule companion) =>
        $"{module.ModuleName}{companion.ModuleSymbolSuffix}CorsRules";

    private static IEnumerable<(string Name, string Description, IReadOnlyList<BlobCorsRuleData> Value)> GetStorageAccountCorsParameters(
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

    private static void AppendCorsParameterAssignment(
        StringBuilder sb,
        string parameterName,
        IReadOnlyList<BlobCorsRuleData> corsRules)
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

    private static string GetStorageAccountLifecycleParameterName(
        GeneratedTypeModule module,
        GeneratedCompanionModule companion) =>
        $"{module.ModuleName}{companion.ModuleSymbolSuffix}LifecycleRules";

    private static IEnumerable<(string Name, string Description, IReadOnlyList<ContainerLifecycleRuleData> Value)> GetStorageAccountLifecycleParameters(
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

    private static IEnumerable<string> RenderLifecycleRules(IReadOnlyList<ContainerLifecycleRuleData> rules, int indentLevel = 0)
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
            yield return $"{propertyIndent}ruleName: '{EscapeBicepString(rule.RuleName)}'";
            yield return $"{propertyIndent}containerNames: {RenderBicepStringArray(rule.ContainerNames)}";
            yield return $"{propertyIndent}timeToLiveInDays: {rule.TimeToLiveInDays}";
            yield return $"{itemIndent}}}";
        }

        yield return $"{indent}]";
    }

    private static void AppendLifecycleParameterAssignment(
        StringBuilder sb,
        string parameterName,
        IReadOnlyList<ContainerLifecycleRuleData> rules)
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

    private static void AppendStorageAccountCompanionModule(
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
            sb.AppendLine($"    blobContainerNames: {RenderBicepStringArray(companion.BlobContainerNames)}");
        }

        if (companion.CorsRules.Count > 0)
        {
            sb.AppendLine($"    corsRules: {GetStorageAccountCorsParameterName(module, companion)}");
        }

        if (companion.StorageTableNames.Count > 0)
        {
            sb.AppendLine($"    tableNames: {RenderBicepStringArray(companion.StorageTableNames)}");
        }

        if (companion.TableCorsRules.Count > 0)
        {
            sb.AppendLine($"    corsRules: {GetStorageAccountCorsParameterName(module, companion)}");
        }

        if (companion.QueueNames.Count > 0)
        {
            sb.AppendLine($"    queueNames: {RenderBicepStringArray(companion.QueueNames)}");
        }

        if (companion.LifecycleRules.Count > 0)
        {
            sb.AppendLine($"    containerLifecycleRules: {GetStorageAccountLifecycleParameterName(module, companion)}");
        }

        sb.AppendLine("  }");
        sb.AppendLine("}");
        sb.AppendLine();
    }

    // ────────────────────────────────────────────────────────────────────────
    // types.bicep
    // ────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Generates <c>types.bicep</c> with exported environment types, variable map, and optional RBAC types.
    /// </summary>
    internal static string GenerateTypesBicep(IReadOnlyList<EnvironmentDefinition> environments, bool includeRbacType)
    {
        var sb = new StringBuilder();

        // ── EnvironmentName union type ──────────────────────────────────────
        if (environments.Count > 0)
        {
            sb.AppendLine("@export()");
            sb.Append("type EnvironmentName = ");
            sb.AppendJoin(" | ", environments.Select(e => $"'{SanitizeBicepKey(e.Name)}'"));
            sb.AppendLine();
        }
        else
        {
            sb.AppendLine("@export()");
            sb.AppendLine("type EnvironmentName = string");
        }

        sb.AppendLine();

        // ── EnvironmentVariables object type ────────────────────────────────
        sb.AppendLine("@export()");
        sb.AppendLine("type EnvironmentVariables = {");
        sb.AppendLine("  envName: string");
        sb.AppendLine("  envShort: string");
        sb.AppendLine("  envSuffix: string");
        sb.AppendLine("  envPrefix: string");
        sb.AppendLine("  location: string");
        sb.AppendLine("  tags: object");
        sb.AppendLine("}");
        sb.AppendLine();

        // ── environments variable map ───────────────────────────────────────
        sb.AppendLine("@export()");
        sb.AppendLine("var environments = {");
        foreach (var env in environments)
        {
            var key = SanitizeBicepKey(env.Name);
            var envSuffix = env.Suffix ?? string.Empty;
            var envPrefix = env.Prefix ?? string.Empty;

            sb.AppendLine($"  {key}: {{");
            sb.AppendLine($"    envName: '{env.Name}'");
            sb.AppendLine($"    envShort: '{env.ShortName}'");
            sb.AppendLine($"    envSuffix: '{envSuffix}'");
            sb.AppendLine($"    envPrefix: '{envPrefix}'");
            sb.AppendLine($"    location: '{env.Location}'");
            sb.AppendLine("    tags: {");
            foreach (var (tagKey, tagValue) in env.Tags)
            {
                sb.AppendLine($"      {EscapeBicepString(tagKey)}: '{EscapeBicepString(tagValue)}'");
            }
            sb.AppendLine("    }");
            sb.AppendLine("  }");
        }

        sb.AppendLine("}");

        // ── RbacRoleType (only when role assignments exist) ─────────────────
        if (includeRbacType)
        {
            sb.AppendLine();
            sb.AppendLine("@description('Rbac Role Type')");
            sb.AppendLine("@export()");
            sb.AppendLine("type RbacRoleType = {");
            sb.AppendLine("  @description('Identifier of the role')");
            sb.AppendLine("  id: string");
            sb.AppendLine();
            sb.AppendLine("  @description('Name of the role')");
            sb.AppendLine("  description: string");
            sb.AppendLine("}");
        }

        return sb.ToString();
    }

    // ────────────────────────────────────────────────────────────────────────
    // functions.bicep
    // ────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Generates <c>functions.bicep</c> with exported naming functions
    /// derived from the project's naming templates.
    /// </summary>
    internal static string GenerateFunctionsBicep(NamingContext namingContext)
    {
        var sb = new StringBuilder();

        sb.AppendLine("import { EnvironmentVariables } from 'types.bicep'");
        sb.AppendLine();

        // ── Default naming function ─────────────────────────────────────────
        if (!string.IsNullOrEmpty(namingContext.DefaultTemplate))
        {
            var interpolation = NamingTemplateTranslator.ToBicepInterpolation(namingContext.DefaultTemplate);
            var usesResourceType = NamingTemplateTranslator.UsesResourceType(namingContext.DefaultTemplate);
            var extraParam = usesResourceType ? ", resourceType string" : "";

            sb.AppendLine($"@description('Builds the default resource name from template: {namingContext.DefaultTemplate}')");
            sb.AppendLine("@export()");
            sb.AppendLine($"func BuildResourceName(name string, resourceAbbr string{extraParam}, env EnvironmentVariables) string =>");
            sb.AppendLine($"  {interpolation}");
            sb.AppendLine();
        }

        // ── Per-resource-type override functions ────────────────────────────
        foreach (var (resourceType, template) in namingContext.ResourceTemplates)
        {
            var functionName = NamingTemplateTranslator.GetFunctionName(resourceType);
            var interpolation = NamingTemplateTranslator.ToBicepInterpolation(template);
            var usesResourceType = NamingTemplateTranslator.UsesResourceType(template);
            var extraParam = usesResourceType ? ", resourceType string" : "";

            sb.AppendLine($"@description('Builds a {resourceType} name from template: {template}')");
            sb.AppendLine("@export()");
            sb.AppendLine($"func {functionName}(name string, resourceAbbr string{extraParam}, env EnvironmentVariables) string =>");
            sb.AppendLine($"  {interpolation}");
            sb.AppendLine();
        }

        return sb.ToString();
    }

    // ────────────────────────────────────────────────────────────────────────
    // constants.bicep
    // ────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Generates <c>constants.bicep</c> with exported RBAC role definitions
    /// grouped by Azure service. Only includes roles that are actually used in role assignments.
    /// </summary>
    internal static string GenerateConstantsBicep(IReadOnlyList<RoleAssignmentDefinition> roleAssignments)
    {
        var sb = new StringBuilder();

        // Group used roles by service category, then by role name (deduplicated)
        var rolesByService = roleAssignments
            .GroupBy(ra => ra.ServiceCategory)
            .OrderBy(g => g.Key)
            .ToDictionary(
                g => g.Key,
                g => g.DistinctBy(ra => ra.RoleDefinitionId)
                    .OrderBy(ra => ra.RoleDefinitionName)
                    .ToList());

        sb.AppendLine("// Role definitions id");
        sb.AppendLine("@export()");
        sb.AppendLine("@description('RBAC roles grouped by Azure service')");
        sb.AppendLine("var RbacRoles = {");

        foreach (var (service, roles) in rolesByService)
        {
            sb.AppendLine($"  {service}: {{");

            foreach (var role in roles)
            {
                sb.AppendLine($"    '{role.RoleDefinitionName}': {{");
                sb.AppendLine($"      id: '{role.RoleDefinitionId}'");
                sb.AppendLine($"      description: '{EscapeBicepString(role.RoleDefinitionDescription)}'");
                sb.AppendLine("    }");
            }

            sb.AppendLine("  }");
        }

        sb.AppendLine("}");

        return sb.ToString();
    }

    // ────────────────────────────────────────────────────────────────────────
    // main.bicep
    // ────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Generates <c>main.bicep</c> with environment-aware naming and resource deployment.
    /// </summary>
    private static string GenerateMainBicep(
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

        sb.AppendLine("targetScope = 'subscription'");
        sb.AppendLine();

        // ── Imports ─────────────────────────────────────────────────────────
        sb.AppendLine("import { EnvironmentName, environments } from 'types.bicep'");

        var functionImports = BuildFunctionImportList(namingContext, modules, resourceGroups);
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
                var bicepType = InferBicepType(value);
                sb.AppendLine($"param {module.ModuleName}{Capitalize(key)} {bicepType}");
            }

            foreach (var (name, description, _) in GetStorageAccountCorsParameters(module))
            {
                sb.AppendLine($"@description('{EscapeBicepString(description)}')");
                sb.AppendLine($"param {name} array = []");
            }

            foreach (var (name, description, _) in GetStorageAccountLifecycleParameters(module))
            {
                sb.AppendLine($"@description('{EscapeBicepString(description)}')");
                sb.AppendLine($"param {name} array = []");
            }
        }

        // Static app setting parameter declarations
        foreach (var setting in appSettings.Where(s => s.EnvironmentValues is { Count: > 0 }))
        {
            var paramName = GetStaticAppSettingParamName(setting.TargetResourceName, setting.Name);
            sb.AppendLine($"param {paramName} string");
        }

        // Secure parameter declarations for ViaBicepparam Key Vault secrets
        foreach (var setting in appSettings.Where(s =>
            s.IsKeyVaultReference && !s.IsSensitiveOutputExportedToKeyVault
            && s.SecretValueAssignment == "ViaBicepparam"))
        {
            var paramName = GetSecureAppSettingParamName(setting.TargetResourceName, setting.SecretName!);
            sb.AppendLine();
            sb.AppendLine("@secure()");
            sb.AppendLine($"@description('Secret value for Key Vault secret \\'{EscapeBicepString(setting.SecretName!)}\\' used by {setting.TargetResourceName}')");
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
                    sb.AppendLine($"  '{EscapeBicepString(tagKey)}': '{EscapeBicepString(tagValue)}'");
                }
                sb.AppendLine("}");
                sb.AppendLine();
            }

            if (hasConfigTags)
            {
                sb.AppendLine("var configTags = {");
                foreach (var (tagKey, tagValue) in configTags!)
                {
                    sb.AppendLine($"  '{EscapeBicepString(tagKey)}': '{EscapeBicepString(tagValue)}'");
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
            var nameExpr = BuildNamingExpression(
                rg.Name, rg.ResourceAbbreviation, "ResourceGroup", namingContext);

            sb.AppendLine($"resource {rgSymbol} 'Microsoft.Resources/resourceGroups@2024-07-01' = {{");
            sb.AppendLine($"  name: {nameExpr}");
            sb.AppendLine("  location: env.location");
            sb.AppendLine("  tags: tags");
            sb.AppendLine("}");
            sb.AppendLine();
        }

        // ── Existing resource declarations (cross-config references) ────────
        if (existingResourceReferences.Count > 0)
        {
            // Deduplicate external resource groups (multiple resources may share the same RG)
            var externalRgs = existingResourceReferences
                .Select(r => r.ResourceGroupName)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            sb.AppendLine("// ── Cross-configuration existing resource groups ──────────────────");
            foreach (var extRgName in externalRgs)
            {
                var extRgSymbol = $"existing_{BicepIdentifierHelper.ToBicepIdentifier(extRgName)}";
                var nameExprRg = BuildNamingExpression(extRgName, "rg", "ResourceGroup", namingContext);

                sb.AppendLine($"resource {extRgSymbol} 'Microsoft.Resources/resourceGroups@2024-07-01' existing = {{");
                sb.AppendLine($"  name: {nameExprRg}");
                sb.AppendLine("}");
                sb.AppendLine();
            }

            sb.AppendLine("// ── Cross-configuration existing resources ──────────────────────");
            foreach (var extRef in existingResourceReferences)
            {
                var extSymbol = $"existing_{BicepIdentifierHelper.ToBicepIdentifier(extRef.ResourceName)}";
                var extRgSymbol = $"existing_{BicepIdentifierHelper.ToBicepIdentifier(extRef.ResourceGroupName)}";
                var nameExprRes = BuildNamingExpression(
                    extRef.ResourceName, extRef.ResourceAbbreviation,
                    extRef.ResourceTypeName, namingContext);

                var apiVersion = GetExistingResourceApiVersion(extRef.ResourceType);

                sb.AppendLine($"resource {extSymbol} '{extRef.ResourceType}@{apiVersion}' existing = {{");
                sb.AppendLine($"  name: {nameExprRes}");
                sb.AppendLine($"  scope: {extRgSymbol}");
                sb.AppendLine("}");
                sb.AppendLine();
            }
        }

        // ── Module declarations ─────────────────────────────────────────────
        // Group app settings by target resource name for quick lookup
        var appSettingsByTarget = appSettings
            .GroupBy(s => s.TargetResourceName, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

        // Group user-assigned identity references by (source name, source type name).
        // Uses composite key to avoid ambiguity when multiple resources share the same name.
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
            var nameExpr = BuildNamingExpression(
                module.LogicalResourceName, module.ResourceAbbreviation,
                module.ResourceTypeName, namingContext);

            sb.AppendLine($"module {moduleSymbol} './modules/{module.ModuleFolderName}/{module.ModuleFileName}' = {{");
            sb.AppendLine($"  name: '{module.ModuleName}'");
            sb.AppendLine($"  scope: {rgSymbol}");
            sb.AppendLine("  params: {");
            sb.AppendLine("    location: env.location");
            sb.AppendLine($"    name: {nameExpr}");
            sb.AppendLine("    tags: tags");

            foreach (var paramKey in module.Parameters.Keys)
            {
                sb.AppendLine($"    {paramKey}: {module.ModuleName}{Capitalize(paramKey)}");
            }

            // ── Parent module ID references (e.g. appServicePlanId → aspModule.outputs.id) ──
            foreach (var (paramName, parentLogicalName) in module.ParentModuleIdReferences)
            {
                var parentModule = modules.FirstOrDefault(m =>
                    m.LogicalResourceName.Equals(parentLogicalName, StringComparison.OrdinalIgnoreCase));
                if (parentModule is not null)
                {
                    sb.AppendLine($"    {paramName}: {parentModule.ModuleName}Module.outputs.id");
                }
            }

            // ── Parent module name references (e.g. sqlServerName → naming expression) ──
            foreach (var (paramName, parentLogicalName) in module.ParentModuleNameReferences)
            {
                var parentModule = modules.FirstOrDefault(m =>
                    m.LogicalResourceName.Equals(parentLogicalName, StringComparison.OrdinalIgnoreCase));
                if (parentModule is not null)
                {
                    var parentNameExpr = BuildNamingExpression(
                        parentModule.LogicalResourceName, parentModule.ResourceAbbreviation,
                        parentModule.ResourceTypeName, namingContext);
                    sb.AppendLine($"    {paramName}: {parentNameExpr}");
                }
            }

            // ── Identity params ─────────────────────────────────────────────
            var moduleKey = (module.LogicalResourceName, module.ResourceTypeName);

            if (module.UsesParameterizedIdentity)
            {
                // Parameterized identity: pass identityType and UAI resource IDs.
                var identityKind = module.IdentityKind ?? "SystemAssigned";
                sb.AppendLine($"    identityType: '{identityKind}'");

                // Pass UAI resource IDs if this resource uses UserAssigned identity
                if (module.ResourceTypeName != "UserAssignedIdentity"
                    && uaiBySourceResource.TryGetValue(moduleKey, out var uaiNamesParam))
                {
                    foreach (var uaiName in uaiNamesParam)
                    {
                        var uaiId = BicepIdentifierHelper.ToBicepIdentifier(uaiName);
                        var pName = $"userAssignedIdentity{Capitalize(uaiId)}Id";
                        var uaiModSym = modules.FirstOrDefault(m =>
                            m.ResourceTypeName == "UserAssignedIdentity"
                            && m.LogicalResourceName.Equals(uaiName, StringComparison.OrdinalIgnoreCase));
                        if (uaiModSym is not null)
                        {
                            sb.AppendLine($"    {pName}: {uaiModSym.ModuleName}Module.outputs.resourceId");
                        }
                    }
                }
            }
            else
            {
                // Hardcoded identity: pass UAI resource IDs to the module (existing behavior).
                // Skip UAI modules themselves — they don't consume their own resource ID.
                if (module.ResourceTypeName != "UserAssignedIdentity"
                    && uaiBySourceResource.TryGetValue(moduleKey, out var uaiNames))
                {
                    foreach (var uaiName in uaiNames)
                    {
                        var uaiId = BicepIdentifierHelper.ToBicepIdentifier(uaiName);
                        var paramName = $"userAssignedIdentity{Capitalize(uaiId)}Id";
                        var uaiModuleSymbol = modules.FirstOrDefault(m =>
                            m.ResourceTypeName == "UserAssignedIdentity"
                            && m.LogicalResourceName.Equals(uaiName, StringComparison.OrdinalIgnoreCase));
                        if (uaiModuleSymbol is not null)
                        {
                            sb.AppendLine($"    {paramName}: {uaiModuleSymbol.ModuleName}Module.outputs.resourceId");
                        }
                    }
                }
            }

            // Inject appSettings / envVars param for compute modules only
            var isComputeModule = module.ResourceTypeName is AzureResourceTypes.WebApp or AzureResourceTypes.FunctionApp or AzureResourceTypes.ContainerApp;
            if (isComputeModule
                && appSettingsByTarget.TryGetValue(module.LogicalResourceName, out var resourceAppSettings))
            {
                var isContainerApp = module.ResourceTypeName == AzureResourceTypes.ContainerApp;
                var paramName = isContainerApp ? "envVars" : "appSettings";

                sb.AppendLine($"    {paramName}: [");
                foreach (var setting in resourceAppSettings)
                {
                    sb.AppendLine("      {");
                    sb.AppendLine($"        name: '{setting.Name}'");

                    if (setting.IsSensitiveOutputExportedToKeyVault
                        && setting.SourceResourceName is not null
                        && setting.SecretName is not null)
                    {
                        // Sensitive output exported to KV — reference the generated kvSecret module's secretUri output
                        var sourceModule = modules.FirstOrDefault(m =>
                            m.LogicalResourceName.Equals(setting.SourceResourceName, StringComparison.OrdinalIgnoreCase));

                        if (sourceModule is not null)
                        {
                            var secretIdentifier = BicepIdentifierHelper.ToBicepIdentifier(setting.SecretName);
                            var secretModuleSymbol = $"{sourceModule.ModuleName}{Capitalize(secretIdentifier)}SecretModule";

                            sb.AppendLine($"        value: '@Microsoft.KeyVault(SecretUri=${{{secretModuleSymbol}.outputs.secretUri}})'");
                        }
                    }
                    else if (setting.IsKeyVaultReference && setting.KeyVaultResourceName is not null && setting.SecretName is not null)
                    {
                        if (setting.SecretValueAssignment == "ViaBicepparam")
                        {
                            // ViaBicepparam secret — reference the generated kvSecret module's secretUri output
                            var targetIdentifier = BicepIdentifierHelper.ToBicepIdentifier(setting.TargetResourceName);
                            var secretIdentifier = BicepIdentifierHelper.ToBicepIdentifier(setting.SecretName);
                            var secretModuleSymbol = $"{targetIdentifier}{Capitalize(secretIdentifier)}SecretModule";

                            sb.AppendLine($"        value: '@Microsoft.KeyVault(SecretUri=${{{secretModuleSymbol}.outputs.secretUri}})'");
                        }
                        else
                        {
                            // Manual Key Vault secret reference (DirectInKeyVault — user-specified secret name)
                            var kvModule = modules.FirstOrDefault(m =>
                                m.LogicalResourceName.Equals(setting.KeyVaultResourceName, StringComparison.OrdinalIgnoreCase));

                            if (kvModule is not null)
                            {
                                sb.AppendLine($"        value: '@Microsoft.KeyVault(SecretUri=${{{kvModule.ModuleName}Module.outputs.vaultUri}}secrets/{EscapeBicepString(setting.SecretName)})'");
                            }
                        }
                    }
                    else if (setting.IsOutputReference && setting.SourceResourceName is not null)
                    {
                        if (setting.IsSourceCrossConfig)
                        {
                            // Cross-config source: use existing resource property
                            var extSymbol = $"existing_{BicepIdentifierHelper.ToBicepIdentifier(setting.SourceResourceName)}";
                            // Strip the resource symbolic name prefix from catalog BicepExpression
                            // e.g. "kv.properties.vaultUri" → "properties.vaultUri"
                            var propertyPath = StripResourceSymbolPrefix(setting.SourceOutputBicepExpression)
                                               ?? $"properties.{setting.SourceOutputName}";
                            sb.AppendLine($"        value: {extSymbol}.{propertyPath}");
                        }
                        else
                        {
                            // Same-config source: use module output
                            var sourceModule = modules.FirstOrDefault(m =>
                                m.LogicalResourceName.Equals(setting.SourceResourceName, StringComparison.OrdinalIgnoreCase));

                            if (sourceModule is not null)
                            {
                                sb.AppendLine($"        value: {sourceModule.ModuleName}Module.outputs.{setting.SourceOutputName}");
                            }
                        }
                    }
                    else if (setting.EnvironmentValues is { Count: > 0 })
                    {
                        var settingParamName = GetStaticAppSettingParamName(module.LogicalResourceName, setting.Name);
                        sb.AppendLine($"        value: {settingParamName}");
                    }
                    else if (setting.StaticValue is not null)
                    {
                        sb.AppendLine($"        value: '{EscapeBicepString(setting.StaticValue)}'");
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
                AppendStorageAccountCompanionModule(sb, module, companion, rgSymbol, nameExpr);
            }
        }

        // ── Key Vault secrets for sensitive output exports ────────────────
        var sensitiveExports = appSettings
            .Where(s => s.IsSensitiveOutputExportedToKeyVault
                && s.KeyVaultResourceName is not null
                && s.SecretName is not null
                && s.SourceResourceName is not null
                && s.SourceOutputBicepExpression is not null)
            .ToList();

        if (sensitiveExports.Count > 0)
        {
            sb.AppendLine("// ── Key Vault secrets for sensitive resource outputs ──────────────────");
            sb.AppendLine();

            foreach (var export in sensitiveExports)
            {
                var kvModule = modules.FirstOrDefault(m =>
                    m.LogicalResourceName.Equals(export.KeyVaultResourceName!, StringComparison.OrdinalIgnoreCase));
                var sourceModule = modules.FirstOrDefault(m =>
                    m.LogicalResourceName.Equals(export.SourceResourceName!, StringComparison.OrdinalIgnoreCase));

                if (kvModule is null || sourceModule is null)
                    continue;

                var kvRgSymbol = BicepIdentifierHelper.ToBicepIdentifier(kvModule.ResourceGroupName);
                var secretIdentifier = BicepIdentifierHelper.ToBicepIdentifier(export.SecretName!);
                var secretModuleSymbol = $"{sourceModule.ModuleName}{Capitalize(secretIdentifier)}SecretModule";
                var kvNameExpr = BuildNamingExpression(
                    kvModule.LogicalResourceName, kvModule.ResourceAbbreviation,
                    kvModule.ResourceTypeName, namingContext);

                sb.AppendLine($"module {secretModuleSymbol} './modules/KeyVault/kvSecret.module.bicep' = {{");
                sb.AppendLine($"  name: '{sourceModule.ModuleName}-{EscapeBicepString(export.SecretName!).ToLowerInvariant()}-secret'");
                sb.AppendLine($"  scope: {kvRgSymbol}");
                sb.AppendLine("  params: {");
                sb.AppendLine($"    keyVaultName: {kvNameExpr}");
                sb.AppendLine($"    secretName: '{EscapeBicepString(export.SecretName!)}'");
                sb.AppendLine($"    secretValue: {sourceModule.ModuleName}Module.outputs.{export.SourceOutputName}");
                sb.AppendLine("  }");
                sb.AppendLine("}");
                sb.AppendLine();
            }
        }

        // ── Key Vault secrets for ViaBicepparam static secrets ────────────
        var viaBicepparamSecrets = appSettings
            .Where(s => s.IsKeyVaultReference && !s.IsSensitiveOutputExportedToKeyVault
                && s.SecretValueAssignment == "ViaBicepparam"
                && s.KeyVaultResourceName is not null
                && s.SecretName is not null)
            .ToList();

        if (viaBicepparamSecrets.Count > 0)
        {
            sb.AppendLine("// ── Key Vault secrets for ViaBicepparam static secrets ──────────────────");
            sb.AppendLine();

            foreach (var secret in viaBicepparamSecrets)
            {
                var kvModule = modules.FirstOrDefault(m =>
                    m.LogicalResourceName.Equals(secret.KeyVaultResourceName!, StringComparison.OrdinalIgnoreCase));

                if (kvModule is null)
                    continue;

                var kvRgSymbol = BicepIdentifierHelper.ToBicepIdentifier(kvModule.ResourceGroupName);
                var secretIdentifier = BicepIdentifierHelper.ToBicepIdentifier(secret.SecretName!);
                var targetIdentifier = BicepIdentifierHelper.ToBicepIdentifier(secret.TargetResourceName);
                var secretModuleSymbol = $"{targetIdentifier}{Capitalize(secretIdentifier)}SecretModule";
                var kvNameExpr = BuildNamingExpression(
                    kvModule.LogicalResourceName, kvModule.ResourceAbbreviation,
                    kvModule.ResourceTypeName, namingContext);
                var secureParamName = GetSecureAppSettingParamName(secret.TargetResourceName, secret.SecretName!);

                sb.AppendLine($"module {secretModuleSymbol} './modules/KeyVault/kvSecret.module.bicep' = {{");
                sb.AppendLine($"  name: '{targetIdentifier}-{EscapeBicepString(secret.SecretName!).ToLowerInvariant()}-secret'");
                sb.AppendLine($"  scope: {kvRgSymbol}");
                sb.AppendLine("  params: {");
                sb.AppendLine($"    keyVaultName: {kvNameExpr}");
                sb.AppendLine($"    secretName: '{EscapeBicepString(secret.SecretName!)}'");
                sb.AppendLine($"    secretValue: {secureParamName}");
                sb.AppendLine("  }");
                sb.AppendLine("}");
                sb.AppendLine();
            }
        }

        // ── Role assignment module declarations ─────────────────────────────
        if (roleAssignments.Count > 0)
        {
            var grouped = GroupRoleAssignments(roleAssignments);

            foreach (var group in grouped)
            {
                var sourceIdentifier = BicepIdentifierHelper.ToBicepIdentifier(group.SourceResourceName);
                var targetIdentifier = BicepIdentifierHelper.ToBicepIdentifier(group.TargetResourceName);
                var moduleSymbol = $"{GetBaseModuleName(group.SourceResourceType)}{Capitalize(sourceIdentifier)}" +
                    $"{GetBaseModuleName(group.TargetResourceType)}{Capitalize(targetIdentifier)}Roles";

                var targetFolder = GetModuleFolderName(group.TargetResourceTypeName);
                var moduleFileName = RoleAssignmentModuleTemplates.GetModuleFileName(group.TargetResourceTypeName);

                // Cross-config targets use existing_ prefixed symbols
                var targetRgSymbol = group.IsTargetCrossConfig
                    ? $"existing_{BicepIdentifierHelper.ToBicepIdentifier(group.TargetResourceGroupName)}"
                    : BicepIdentifierHelper.ToBicepIdentifier(group.TargetResourceGroupName);

                var targetNameExpr = BuildNamingExpression(
                    group.TargetResourceName, group.TargetResourceAbbreviation,
                    group.TargetResourceTypeName, namingContext);

                var principalIdExpr = ResolvePrincipalIdExpression(group, modules);

                sb.AppendLine($"module {moduleSymbol} './modules/{targetFolder}/{moduleFileName}' = {{");
                sb.AppendLine($"  name: '{moduleSymbol}'");
                sb.AppendLine($"  scope: {targetRgSymbol}");
                sb.AppendLine("  params: {");
                sb.AppendLine($"    name: {targetNameExpr}");
                sb.AppendLine($"    principalId: {principalIdExpr}");
                sb.AppendLine("    roles: [");

                foreach (var role in group.Roles)
                {
                    sb.AppendLine($"      RbacRoles.{group.ServiceCategory}['{role.RoleDefinitionName}']");
                }

                sb.AppendLine("    ]");
                sb.AppendLine("  }");
                sb.AppendLine("}");
                sb.AppendLine();
            }
        }

        return sb.ToString();
    }

    // ────────────────────────────────────────────────────────────────────────
    // Role assignment helpers
    // ────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Groups role assignments by (source, target, identity type) and collects
    /// the assigned roles into a single entry for each group.
    /// </summary>
    private static List<GroupedRoleAssignment> GroupRoleAssignments(
        IReadOnlyList<RoleAssignmentDefinition> roleAssignments)
    {
        return roleAssignments
            .GroupBy(ra => (ra.SourceResourceName, ra.TargetResourceName, ra.ManagedIdentityType,
                ra.UserAssignedIdentityResourceId))
            .Select(g =>
            {
                var first = g.First();
                return new GroupedRoleAssignment
                {
                    SourceResourceName = first.SourceResourceName,
                    SourceResourceType = first.SourceResourceType,
                    TargetResourceName = first.TargetResourceName,
                    TargetResourceType = first.TargetResourceType,
                    TargetResourceTypeName = first.TargetResourceTypeName,
                    TargetResourceGroupName = first.TargetResourceGroupName,
                    TargetResourceAbbreviation = first.TargetResourceAbbreviation,
                    ServiceCategory = first.ServiceCategory,
                    ManagedIdentityType = first.ManagedIdentityType,
                    UserAssignedIdentityName = first.UserAssignedIdentityName,
                    IsTargetCrossConfig = first.IsTargetCrossConfig,
                    Roles = g.Select(ra => new RoleRef(ra.RoleDefinitionName)).ToList()
                };
            })
            .ToList();
    }

    /// <summary>
    /// Resolves the Bicep expression for the <c>principalId</c> parameter
    /// of a role assignment module based on the managed identity type.
    /// </summary>
    private static string ResolvePrincipalIdExpression(
        GroupedRoleAssignment group,
        IReadOnlyCollection<GeneratedTypeModule> modules)
    {
        if (group.ManagedIdentityType == "UserAssigned" && group.UserAssignedIdentityName is not null)
        {
            var uaiIdentifier = BicepIdentifierHelper.ToBicepIdentifier(group.UserAssignedIdentityName);
            return $"userAssignedIdentity{Capitalize(uaiIdentifier)}Module.outputs.principalId";
        }

        // SystemAssigned: reference the source resource module's principalId output
        var sourceIdentifier = BicepIdentifierHelper.ToBicepIdentifier(group.SourceResourceName);
        var sourceBaseName = GetBaseModuleName(group.SourceResourceType);
        return $"{sourceBaseName}{Capitalize(sourceIdentifier)}Module.outputs.principalId";
    }

    private sealed record RoleRef(string RoleDefinitionName);

    private sealed class GroupedRoleAssignment
    {
        public string SourceResourceName { get; init; } = string.Empty;
        public string SourceResourceType { get; init; } = string.Empty;
        public string TargetResourceName { get; init; } = string.Empty;
        public string TargetResourceType { get; init; } = string.Empty;
        public string TargetResourceTypeName { get; init; } = string.Empty;
        public string TargetResourceGroupName { get; init; } = string.Empty;
        public string TargetResourceAbbreviation { get; init; } = string.Empty;
        public string ServiceCategory { get; init; } = string.Empty;
        public string ManagedIdentityType { get; init; } = string.Empty;
        public string? UserAssignedIdentityName { get; init; }
        public bool IsTargetCrossConfig { get; init; }
        public List<RoleRef> Roles { get; init; } = [];
    }

    // ────────────────────────────────────────────────────────────────────────
    // .bicepparam files
    // ────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Generates one <c>.bicepparam</c> file per environment.
    /// Each file sets <c>environmentName</c> and the resource-specific parameter overrides.
    /// </summary>
    private static Dictionary<string, string> GenerateEnvironmentParameterFiles(
        IReadOnlyCollection<GeneratedTypeModule> modules,
        IReadOnlyList<string> environmentNames,
        IEnumerable<ResourceDefinition> resources,
        IReadOnlyList<AppSettingDefinition> appSettings)
    {
        var resourceList = resources.ToList();
        var result = new Dictionary<string, string>();

        foreach (var envName in environmentNames)
        {
            var envModules = ApplyEnvironmentOverrides(modules, envName, resourceList);
            var paramContent = GenerateMainParameters(envModules, envName, appSettings);
            var fileName = $"main.{envName.ToLowerInvariant()}.bicepparam";
            result[fileName] = paramContent;
        }

        return result;
    }

    /// <summary>
    /// Clones modules and replaces matching parameter values with environment-specific overrides.
    /// </summary>
    private static IReadOnlyCollection<GeneratedTypeModule> ApplyEnvironmentOverrides(
        IReadOnlyCollection<GeneratedTypeModule> modules,
        string environmentName,
        IReadOnlyList<ResourceDefinition> resources)
    {
        return modules.Select(module =>
        {
            var matchingResource = resources.FirstOrDefault(r =>
            {
                var resourceIdentifier = BicepIdentifierHelper.ToBicepIdentifier(r.Name);
                var expectedModuleName = GetBaseModuleName(r.Type) +
                    (resourceIdentifier.Length == 0
                        ? resourceIdentifier
                        : char.ToUpperInvariant(resourceIdentifier[0]) + resourceIdentifier[1..]);
                return module.ModuleName == expectedModuleName;
            });

            if (matchingResource is null ||
                !matchingResource.EnvironmentConfigs.TryGetValue(environmentName, out var envOverrides) ||
                envOverrides.Count == 0)
            {
                return module;
            }

            var mergedParams = new Dictionary<string, object>(module.Parameters);
            foreach (var (key, value) in envOverrides)
            {
                if (mergedParams.ContainsKey(key))
                {
                    mergedParams[key] = value;
                }
            }

            return module with { Parameters = mergedParams };
        }).ToList();
    }

    private static string GetBaseModuleName(string resourceType)
    {
        return resourceType switch
        {
            AzureResourceTypes.ArmTypes.KeyVault => "keyVault",
            AzureResourceTypes.ArmTypes.RedisCache => "redisCache",
            AzureResourceTypes.ArmTypes.StorageAccount => "storageAccount",
            AzureResourceTypes.ArmTypes.AppServicePlan => "appServicePlan",
            AzureResourceTypes.ArmTypes.WebApp => "webApp",
            AzureResourceTypes.ArmTypes.FunctionApp => "functionApp",
            AzureResourceTypes.ArmTypes.UserAssignedIdentity => "userAssignedIdentity",
            AzureResourceTypes.ArmTypes.AppConfiguration => "appConfiguration",
            AzureResourceTypes.ArmTypes.ContainerAppEnvironment => "containerAppEnvironment",
            AzureResourceTypes.ArmTypes.ContainerApp => "containerApp",
            AzureResourceTypes.ArmTypes.LogAnalyticsWorkspace => "logAnalyticsWorkspace",
            AzureResourceTypes.ArmTypes.ApplicationInsights => "applicationInsights",
            AzureResourceTypes.ArmTypes.CosmosDb => "cosmosDb",
            AzureResourceTypes.ArmTypes.SqlServer => "sqlServer",
            AzureResourceTypes.ArmTypes.SqlDatabase => "sqlDatabase",
            _ => "unknown"
        };
    }

    /// <summary>
    /// Generates a single <c>.bicepparam</c> file setting <c>environmentName</c>
    /// and all resource-specific parameters.
    /// </summary>
    private static string GenerateMainParameters(
        IReadOnlyCollection<GeneratedTypeModule> modules,
        string environmentName,
        IReadOnlyList<AppSettingDefinition> appSettings)
    {
        var sb = new StringBuilder();
        var normalizedEnvironmentName = SanitizeBicepKey(environmentName);

        sb.AppendLine("using 'main.bicep'");
        sb.AppendLine();
        sb.AppendLine($"param environmentName = '{normalizedEnvironmentName}'");
        sb.AppendLine();

        foreach (var module in modules)
        {
            foreach (var (key, value) in module.Parameters)
            {
                sb.AppendLine($"param {module.ModuleName}{Capitalize(key)} = {SerializeToBicep(value)}");
            }

            foreach (var (name, _, value) in GetStorageAccountCorsParameters(module))
            {
                AppendCorsParameterAssignment(sb, name, value);
            }

            foreach (var (name, _, value) in GetStorageAccountLifecycleParameters(module))
            {
                AppendLifecycleParameterAssignment(sb, name, value);
            }

            sb.AppendLine();
        }

        // ── Static app setting params (per-environment values) ──────────
        foreach (var setting in appSettings.Where(s => s.EnvironmentValues is { Count: > 0 }))
        {
            var paramName = GetStaticAppSettingParamName(setting.TargetResourceName, setting.Name);
            var value = setting.EnvironmentValues!.TryGetValue(environmentName, out var envValue)
                ? envValue
                : string.Empty;
            sb.AppendLine($"param {paramName} = '{EscapeBicepString(value)}'");
        }

        // ── ViaBicepparam secure params (placeholder — to be filled at deployment time) ──
        foreach (var setting in appSettings.Where(s =>
            s.IsKeyVaultReference && !s.IsSensitiveOutputExportedToKeyVault
            && s.SecretValueAssignment == "ViaBicepparam"))
        {
            var paramName = GetSecureAppSettingParamName(setting.TargetResourceName, setting.SecretName!);
            sb.AppendLine($"param {paramName} = ''");
        }

        return sb.ToString();
    }

    // ────────────────────────────────────────────────────────────────────────
    // Helpers
    // ────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Builds a Bicep naming function call expression for a given resource.
    /// Selects the per-resource-type override function if one exists, otherwise the default.
    /// Falls back to a literal string when no naming template is configured.
    /// </summary>
    private static string BuildNamingExpression(
        string logicalName,
        string resourceAbbreviation,
        string resourceTypeName,
        NamingContext namingContext)
    {
        if (namingContext.ResourceTemplates.ContainsKey(resourceTypeName))
        {
            var funcName = NamingTemplateTranslator.GetFunctionName(resourceTypeName);
            return $"{funcName}('{logicalName}', '{resourceAbbreviation}', env)";
        }

        if (!string.IsNullOrEmpty(namingContext.DefaultTemplate))
        {
            return $"BuildResourceName('{logicalName}', '{resourceAbbreviation}', env)";
        }

        return $"'{logicalName}'";
    }

    /// <summary>
    /// Collects the set of naming functions actually referenced in <c>main.bicep</c>
    /// to build the import statement.
    /// </summary>
    private static HashSet<string> BuildFunctionImportList(
        NamingContext namingContext,
        IReadOnlyCollection<GeneratedTypeModule> modules,
        IReadOnlyList<ResourceGroupDefinition> resourceGroups)
    {
        var imports = new HashSet<string>();

        if (string.IsNullOrEmpty(namingContext.DefaultTemplate) &&
            namingContext.ResourceTemplates.Count == 0)
        {
            return imports;
        }

        var usedResourceTypes = modules
            .Select(m => m.ResourceTypeName)
            .Concat(resourceGroups.Select(_ => "ResourceGroup"))
            .Distinct();

        var hasDefault = false;

        foreach (var typeName in usedResourceTypes)
        {
            if (namingContext.ResourceTemplates.ContainsKey(typeName))
            {
                imports.Add(NamingTemplateTranslator.GetFunctionName(typeName));
            }
            else if (!string.IsNullOrEmpty(namingContext.DefaultTemplate))
            {
                hasDefault = true;
            }
        }

        if (hasDefault)
        {
            imports.Add("BuildResourceName");
        }

        return imports;
    }

    /// <summary>
    /// Sanitizes a string for use as a Bicep object key.
    /// </summary>
    private static string SanitizeBicepKey(string name)
    {
        if (string.IsNullOrEmpty(name)) return "unknown";
        return name.Replace(' ', '_').Replace('-', '_').ToLowerInvariant();
    }

    /// <summary>
    /// Checks whether any tag source has tags defined.
    /// </summary>
    private static bool HasAnyTags(
        IReadOnlyList<EnvironmentDefinition> environments,
        IReadOnlyDictionary<string, string>? projectTags,
        IReadOnlyDictionary<string, string>? configTags)
    {
        if (projectTags is { Count: > 0 }) return true;
        if (configTags is { Count: > 0 }) return true;
        return environments.Any(e => e.Tags.Count > 0);
    }

    private static string Capitalize(string s) =>
        s.Length == 0 ? s : char.ToUpperInvariant(s[0]) + s[1..];

    /// <summary>
    /// Builds a deterministic Bicep param name for a static app setting.
    /// Convention: <c>{moduleName}{PascalCase(settingName)}</c>.
    /// </summary>
    private static string GetStaticAppSettingParamName(string targetResourceName, string settingName)
    {
        var moduleName = BicepIdentifierHelper.ToBicepIdentifier(targetResourceName);
        var pascalName = ToPascalCaseFromEnvVar(settingName);
        return $"{moduleName}{pascalName}";
    }

    /// <summary>
    /// Builds a deterministic Bicep param name for a secure Key Vault secret value.
    /// Convention: <c>{moduleName}{PascalCase(secretName)}SecretValue</c>.
    /// </summary>
    private static string GetSecureAppSettingParamName(string targetResourceName, string secretName)
    {
        var moduleName = BicepIdentifierHelper.ToBicepIdentifier(targetResourceName);
        var pascalName = ToPascalCaseFromEnvVar(secretName);
        return $"{moduleName}{pascalName}SecretValue";
    }

    /// <summary>
    /// Converts an environment variable name (SNAKE_CASE) to PascalCase.
    /// E.g. "MY_API_URL" → "MyApiUrl", "APP__HOST" → "AppHost".
    /// </summary>
    private static string ToPascalCaseFromEnvVar(string envVarName)
    {
        var sb = new StringBuilder();
        var capitalizeNext = true;
        foreach (var c in envVarName)
        {
            if (c is '_' or '-' or '.')
            {
                capitalizeNext = true;
                continue;
            }

            if (capitalizeNext)
            {
                sb.Append(char.ToUpperInvariant(c));
                capitalizeNext = false;
            }
            else
            {
                sb.Append(char.ToLowerInvariant(c));
            }
        }

        return sb.ToString();
    }

    private static string InferBicepType(object value)
    {
        return value switch
        {
            string => "string",
            int or long or double => "int",
            bool => "bool",
            _ => "object"
        };
    }

    private static string SerializeToBicep(object value)
    {
        return value switch
        {
            string s => $"'{s}'",
            bool b => b ? "true" : "false",
            int or long or double => value.ToString()!,
            _ => SerializeObject(value)
        };
    }

    private static string SerializeObject(object obj)
    {
        var props = obj.GetType().GetProperties();

        var sb = new StringBuilder();
        sb.AppendLine("{");

        foreach (var p in props)
        {
            var propValue = p.GetValue(obj);
            if (propValue is not null)
                sb.AppendLine($"  {p.Name}: {SerializeToBicep(propValue)}");
        }

        sb.Append('}');
        return sb.ToString();
    }

    /// <summary>
    /// Escapes single quotes in a string for use within a Bicep string literal.
    /// </summary>
    private static string EscapeBicepString(string value) =>
        value.Replace("'", "\\'");

    /// <summary>
    /// Strips the leading resource symbolic name from a catalog BicepExpression.
    /// For example, <c>kv.properties.vaultUri</c> becomes <c>properties.vaultUri</c>.
    /// Returns <c>null</c> if the expression is null or cannot be stripped (e.g. string interpolation).
    /// </summary>
    private static string? StripResourceSymbolPrefix(string? bicepExpression)
    {
        if (bicepExpression is null)
            return null;

        var firstDot = bicepExpression.IndexOf('.');
        if (firstDot < 0)
            return bicepExpression;

        // Only strip if the prefix is a simple identifier (no quotes, parens, or interpolation)
        var prefix = bicepExpression[..firstDot];
        if (prefix.All(c => char.IsLetterOrDigit(c) || c == '_'))
            return bicepExpression[(firstDot + 1)..];

        return bicepExpression;
    }

    /// <summary>
    /// Returns the API version to use for <c>existing</c> resource declarations by ARM resource type.
    /// </summary>
    private static string GetExistingResourceApiVersion(string armResourceType) =>
        armResourceType switch
        {
            "Microsoft.KeyVault/vaults" => "2023-07-01",
            "Microsoft.Cache/Redis" => "2024-03-01",
            "Microsoft.Storage/storageAccounts" => "2023-05-01",
            "Microsoft.Web/serverfarms" => "2023-12-01",
            "Microsoft.Web/sites" => "2023-12-01",
            "Microsoft.Web/sites/functionapp" => "2023-12-01",
            "Microsoft.ManagedIdentity/userAssignedIdentities" => "2023-01-31",
            "Microsoft.AppConfiguration/configurationStores" => "2023-03-01",
            "Microsoft.App/managedEnvironments" => "2024-03-01",
            "Microsoft.App/containerApps" => "2024-03-01",
            "Microsoft.OperationalInsights/workspaces" => "2023-09-01",
            "Microsoft.Insights/components" => "2020-02-02",
            "Microsoft.DocumentDB/databaseAccounts" => "2024-05-15",
            "Microsoft.Sql/servers" => "2023-08-01-preview",
            "Microsoft.Sql/servers/databases" => "2023-08-01-preview",
            "Microsoft.ServiceBus/namespaces" => "2022-10-01-preview",
            _ => "2023-01-01"
        };

    /// <summary>
    /// Generates the reusable Key Vault secret module template used when sensitive outputs
    /// are exported to a Key Vault.
    /// </summary>
    private static string GenerateKvSecretModule()
    {
        var sb = new StringBuilder();
        sb.AppendLine("// ──────────────────────────────────────────────────────────────────────");
        sb.AppendLine("// Key Vault Secret Module — stores a sensitive value in a Key Vault");
        sb.AppendLine("// ──────────────────────────────────────────────────────────────────────");
        sb.AppendLine();
        sb.AppendLine("@description('Name of the Key Vault')");
        sb.AppendLine("param keyVaultName string");
        sb.AppendLine();
        sb.AppendLine("@description('Name of the secret')");
        sb.AppendLine("param secretName string");
        sb.AppendLine();
        sb.AppendLine("@secure()");
        sb.AppendLine("@description('Value of the secret')");
        sb.AppendLine("param secretValue string");
        sb.AppendLine();
        sb.AppendLine("resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {");
        sb.AppendLine("  name: keyVaultName");
        sb.AppendLine("}");
        sb.AppendLine();
        sb.AppendLine("resource secret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {");
        sb.AppendLine("  parent: keyVault");
        sb.AppendLine("  name: secretName");
        sb.AppendLine("  properties: {");
        sb.AppendLine("    value: secretValue");
        sb.AppendLine("  }");
        sb.AppendLine("}");
        sb.AppendLine();
        sb.AppendLine("@description('The secret URI including version')");
        sb.AppendLine("output secretUri string = secret.properties.secretUri");
        return sb.ToString();
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

        // Match a full output declaration block:
        //   optional @description(...)  annotation line(s)
        //   output <name> <type> = <expression>
        // The regex captures the output name so we can decide whether to keep it.
        // We use a line-by-line approach to avoid multi-line regex complexity.
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
                    // Drop this output and its pending annotations.
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
