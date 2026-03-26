using System.Text;
using InfraFlowSculptor.BicepGeneration.Generators;
using InfraFlowSculptor.BicepGeneration.Helpers;
using InfraFlowSculptor.BicepGeneration.Models;

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
        IReadOnlyList<ExistingResourceReference>? existingResourceReferences = null)
    {
        var hasRoleAssignments = roleAssignments.Count > 0;
        var typesBicep = GenerateTypesBicep(environments, hasRoleAssignments);
        var functionsBicep = GenerateFunctionsBicep(namingContext);
        var constantsBicep = hasRoleAssignments ? GenerateConstantsBicep(roleAssignments) : string.Empty;
        var main = GenerateMainBicep(modules, resourceGroups, namingContext, roleAssignments, appSettings, existingResourceReferences ?? []);

        var environmentParameterFiles = GenerateEnvironmentParameterFiles(
            modules, environmentNames, resources);

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

            if (module.CompanionModule is not null)
            {
                var companion = module.CompanionModule;
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
            "KeyVault" => "KeyVault",
            "RedisCache" => "RedisCache",
            "StorageAccount" => "StorageAccount",
            "AppServicePlan" => "AppServicePlan",
            "WebApp" => "WebApp",
            "FunctionApp" => "FunctionApp",
            "UserAssignedIdentity" => "UserAssignedIdentity",
            "AppConfiguration" => "AppConfiguration",
            "ContainerAppEnvironment" => "ContainerAppEnvironment",
            "ContainerApp" => "ContainerApp",
            "LogAnalyticsWorkspace" => "LogAnalyticsWorkspace",
            "ApplicationInsights" => "ApplicationInsights",
            "CosmosDb" => "CosmosDb",
            "SqlServer" => "SqlServer",
            "SqlDatabase" => "SqlDatabase",
            _ => resourceTypeName
        };

    /// <summary>
    /// Returns a human-readable display name for the resource type.
    /// </summary>
    private static string GetResourceTypeDisplayName(string resourceTypeName) =>
        resourceTypeName switch
        {
            "KeyVault" => "Key Vault",
            "RedisCache" => "Redis Cache",
            "StorageAccount" => "Storage Account",
            "AppServicePlan" => "App Service Plan",
            "WebApp" => "Web App",
            "FunctionApp" => "Function App",
            "UserAssignedIdentity" => "User Assigned Identity",
            "AppConfiguration" => "App Configuration",
            "ContainerAppEnvironment" => "Container App Environment",
            "ContainerApp" => "Container App",
            "LogAnalyticsWorkspace" => "Log Analytics Workspace",
            "ApplicationInsights" => "Application Insights",
            "CosmosDb" => "Cosmos DB",
            "SqlServer" => "SQL Server",
            "SqlDatabase" => "SQL Database",
            "ServiceBusNamespace" => "Service Bus Namespace",
            _ => resourceTypeName
        };

    /// <summary>
    /// Returns the Microsoft Learn documentation URL for the resource type.
    /// </summary>
    private static string GetResourceTypeDocumentationUrl(string resourceTypeName) =>
        resourceTypeName switch
        {
            "KeyVault" => "https://learn.microsoft.com/en-us/azure/templates/microsoft.keyvault/vaults",
            "RedisCache" => "https://learn.microsoft.com/en-us/azure/templates/microsoft.cache/redis",
            "StorageAccount" => "https://learn.microsoft.com/en-us/azure/templates/microsoft.storage/storageaccounts",
            "AppServicePlan" => "https://learn.microsoft.com/en-us/azure/templates/microsoft.web/serverfarms",
            "WebApp" => "https://learn.microsoft.com/en-us/azure/templates/microsoft.web/sites",
            "FunctionApp" => "https://learn.microsoft.com/en-us/azure/templates/microsoft.web/sites",
            "UserAssignedIdentity" => "https://learn.microsoft.com/en-us/azure/templates/microsoft.managedidentity/userassignedidentities",
            "AppConfiguration" => "https://learn.microsoft.com/en-us/azure/templates/microsoft.appconfiguration/configurationstores",
            "ContainerAppEnvironment" => "https://learn.microsoft.com/en-us/azure/templates/microsoft.app/managedenvironments",
            "ContainerApp" => "https://learn.microsoft.com/en-us/azure/templates/microsoft.app/containerapps",
            "LogAnalyticsWorkspace" => "https://learn.microsoft.com/en-us/azure/templates/microsoft.operationalinsights/workspaces",
            "ApplicationInsights" => "https://learn.microsoft.com/en-us/azure/templates/microsoft.insights/components",
            "CosmosDb" => "https://learn.microsoft.com/en-us/azure/templates/microsoft.documentdb/databaseaccounts",
            "SqlServer" => "https://learn.microsoft.com/en-us/azure/templates/microsoft.sql/servers",
            "SqlDatabase" => "https://learn.microsoft.com/en-us/azure/templates/microsoft.sql/servers/databases",
            "ServiceBusNamespace" => "https://learn.microsoft.com/en-us/azure/templates/microsoft.servicebus/namespaces",
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

    private static IEnumerable<string> RenderCorsRules(IReadOnlyList<BlobCorsRuleData> corsRules)
    {
        if (corsRules.Count == 0)
        {
            yield return "[]";
            yield break;
        }

        yield return "[";
        foreach (var rule in corsRules)
        {
            yield return "      {";
            yield return $"        allowedOrigins: {RenderBicepStringArray(rule.AllowedOrigins)}";
            yield return $"        allowedMethods: {RenderBicepStringArray(rule.AllowedMethods)}";
            yield return $"        allowedHeaders: {RenderBicepStringArray(rule.AllowedHeaders)}";
            yield return $"        exposedHeaders: {RenderBicepStringArray(rule.ExposedHeaders)}";
            yield return $"        maxAgeInSeconds: {rule.MaxAgeInSeconds}";
            yield return "      }";
        }

        yield return "    ]";
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
        IReadOnlyList<ExistingResourceReference> existingResourceReferences)
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
        }

        sb.AppendLine();

        // ── Environment resolution ──────────────────────────────────────────
        sb.AppendLine("var env = environments[environmentName]");
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

            foreach (var paramKey in module.Parameters.Keys)
            {
                sb.AppendLine($"    {paramKey}: {module.ModuleName}{Capitalize(paramKey)}");
            }

            // Inject appSettings / envVars param for compute modules only
            var isComputeModule = module.ResourceTypeName is "WebApp" or "FunctionApp" or "ContainerApp";
            if (isComputeModule
                && appSettingsByTarget.TryGetValue(module.LogicalResourceName, out var resourceAppSettings))
            {
                var isContainerApp = module.ResourceTypeName == "ContainerApp";
                var paramName = isContainerApp ? "envVars" : "appSettings";

                sb.AppendLine($"    {paramName}: [");
                foreach (var setting in resourceAppSettings)
                {
                    sb.AppendLine("      {");
                    sb.AppendLine($"        name: '{setting.Name}'");

                    if (setting.IsKeyVaultReference && setting.KeyVaultResourceName is not null && setting.SecretName is not null)
                    {
                        // Key Vault secret reference
                        var kvModule = modules.FirstOrDefault(m =>
                            m.LogicalResourceName.Equals(setting.KeyVaultResourceName, StringComparison.OrdinalIgnoreCase));

                        if (kvModule is not null)
                        {
                            if (isContainerApp)
                            {
                                // ContainerApp uses keyVaultUrl in the secrets array pattern
                                sb.AppendLine($"        value: '${{{kvModule.ModuleName}Module.outputs.vaultUri}}secrets/{EscapeBicepString(setting.SecretName)}'");
                            }
                            else
                            {
                                // WebApp/FunctionApp use @Microsoft.KeyVault(SecretUri=...) syntax
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
                            sb.AppendLine($"        value: {extSymbol}.{setting.SourceOutputBicepExpression ?? $"properties.{setting.SourceOutputName}"}");
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

            if (module.CompanionModule is not null)
            {
                var companion = module.CompanionModule;
                var companionSymbol = $"{module.ModuleName}BlobsModule";

                sb.AppendLine($"module {companionSymbol} './modules/{companion.FolderName}/{companion.FileName}' = {{");
                sb.AppendLine($"  name: '{module.ModuleName}Blobs'");
                sb.AppendLine($"  scope: {rgSymbol}");
                sb.AppendLine("  params: {");
                sb.AppendLine($"    storageAccountName: {nameExpr}");
                sb.AppendLine($"    blobContainerNames: {RenderBicepStringArray(companion.BlobContainerNames)}");
                sb.AppendLine("    corsRules: ");
                foreach (var line in RenderCorsRules(companion.CorsRules))
                {
                    sb.AppendLine(line);
                }
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
                ra.UserAssignedIdentityName))
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
        IEnumerable<ResourceDefinition> resources)
    {
        var resourceList = resources.ToList();
        var result = new Dictionary<string, string>();

        foreach (var envName in environmentNames)
        {
            var envModules = ApplyEnvironmentOverrides(modules, envName, resourceList);
            var paramContent = GenerateMainParameters(envModules, envName);
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
            "Microsoft.KeyVault/vaults" => "keyVault",
            "Microsoft.Cache/Redis" => "redisCache",
            "Microsoft.Storage/storageAccounts" => "storageAccount",
            "Microsoft.Web/serverfarms" => "appServicePlan",
            "Microsoft.Web/sites" => "webApp",
            "Microsoft.Web/sites/functionapp" => "functionApp",
            "Microsoft.ManagedIdentity/userAssignedIdentities" => "userAssignedIdentity",
            "Microsoft.AppConfiguration/configurationStores" => "appConfiguration",
            "Microsoft.App/managedEnvironments" => "containerAppEnvironment",
            "Microsoft.App/containerApps" => "containerApp",
            "Microsoft.OperationalInsights/workspaces" => "logAnalyticsWorkspace",
            "Microsoft.Insights/components" => "applicationInsights",
            "Microsoft.DocumentDB/databaseAccounts" => "cosmosDb",
            "Microsoft.Sql/servers" => "sqlServer",
            "Microsoft.Sql/servers/databases" => "sqlDatabase",
            _ => "unknown"
        };
    }

    /// <summary>
    /// Generates a single <c>.bicepparam</c> file setting <c>environmentName</c>
    /// and all resource-specific parameters.
    /// </summary>
    private static string GenerateMainParameters(
        IReadOnlyCollection<GeneratedTypeModule> modules,
        string environmentName)
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

            sb.AppendLine();
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

    private static string Capitalize(string s) =>
        s.Length == 0 ? s : char.ToUpperInvariant(s[0]) + s[1..];

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
}
