using System.Text;
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
        NamingContext namingContext)
    {
        var typesBicep = GenerateTypesBicep(environments);
        var functionsBicep = GenerateFunctionsBicep(namingContext);
        var main = GenerateMainBicep(modules, resourceGroups, namingContext);

        var environmentParameterFiles = GenerateEnvironmentParameterFiles(
            modules, environmentNames, resources);

        var moduleFiles = new Dictionary<string, string>();

        foreach (var module in modules.DistinctBy(m => m.ModuleFileName))
        {
            var folder = module.ModuleFolderName;
            moduleFiles[$"modules/{folder}/{module.ModuleFileName}"] = module.ModuleBicepContent;

            if (!string.IsNullOrEmpty(module.ModuleTypesBicepContent))
            {
                moduleFiles[$"modules/{folder}/types.bicep"] = module.ModuleTypesBicepContent;
            }
        }

        return new GenerationResult
        {
            MainBicep = main,
            TypesBicep = typesBicep,
            FunctionsBicep = functionsBicep,
            EnvironmentParameterFiles = environmentParameterFiles,
            ModuleFiles = moduleFiles
        };
    }

    // ────────────────────────────────────────────────────────────────────────
    // types.bicep
    // ────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Generates <c>types.bicep</c> with exported environment types and variable map.
    /// </summary>
    internal static string GenerateTypesBicep(IReadOnlyList<EnvironmentDefinition> environments)
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
            var shortSuffix = env.Suffix;
            var envSuffix = string.IsNullOrEmpty(shortSuffix) ? "" : $"-{shortSuffix}";
            var shortPrefix = env.Prefix;
            var envPrefix = string.IsNullOrEmpty(shortPrefix) ? "" : $"{shortPrefix}-";

            sb.AppendLine($"  {key}: {{");
            sb.AppendLine($"    envName: '{env.Name}'");
            sb.AppendLine($"    envShort: '{env.ShortName}'");
            sb.AppendLine($"    envSuffix: '{envSuffix}'");
            sb.AppendLine($"    envPrefix: '{envPrefix}'");
            sb.AppendLine($"    location: '{env.Location}'");
            sb.AppendLine("  }");
        }

        sb.AppendLine("}");

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
    // main.bicep
    // ────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Generates <c>main.bicep</c> with environment-aware naming and resource deployment.
    /// </summary>
    private static string GenerateMainBicep(
        IReadOnlyCollection<GeneratedTypeModule> modules,
        IReadOnlyList<ResourceGroupDefinition> resourceGroups,
        NamingContext namingContext)
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

        // ── Module declarations ─────────────────────────────────────────────
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

            sb.AppendLine("  }");
            sb.AppendLine("}");
            sb.AppendLine();
        }

        return sb.ToString();
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

        sb.AppendLine("using 'main.bicep'");
        sb.AppendLine();
        sb.AppendLine($"param environmentName = '{environmentName}'");
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
}
