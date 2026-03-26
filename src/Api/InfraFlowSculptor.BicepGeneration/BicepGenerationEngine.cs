using System.Text.RegularExpressions;
using InfraFlowSculptor.BicepGeneration.Generators;
using InfraFlowSculptor.BicepGeneration.Helpers;
using InfraFlowSculptor.BicepGeneration.Models;

namespace InfraFlowSculptor.BicepGeneration;

public sealed class BicepGenerationEngine
{
    /// <summary>ARM resource types that support app settings / environment variables.</summary>
    private static readonly HashSet<string> ComputeResourceTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Microsoft.Web/sites",
        "Microsoft.Web/sites/functionapp",
        "Microsoft.App/containerApps",
    };

    private readonly IEnumerable<IResourceTypeBicepGenerator> _generators;

    public BicepGenerationEngine(
        IEnumerable<IResourceTypeBicepGenerator> generators)
    {
        _generators = generators;
    }

    public GenerationResult Generate(GenerationRequest request)
    {
        var modules = new List<GeneratedTypeModule>();

        // Determine which source resources need system-assigned identity output
        var sourceResourcesNeedingIdentity = request.RoleAssignments
            .Where(ra => ra.ManagedIdentityType == "SystemAssigned")
            .Select(ra => ra.SourceResourceName)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Determine which source resources need output declarations for app settings
        var outputsBySourceResource = request.AppSettings
            .Where(s => s.IsOutputReference && s.SourceResourceName is not null
                && s.SourceOutputName is not null && s.SourceOutputBicepExpression is not null)
            .GroupBy(s => s.SourceResourceName!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                g => g.Key,
                g => g.Select(s => (OutputName: s.SourceOutputName!, BicepExpression: s.SourceOutputBicepExpression!))
                    .DistinctBy(x => x.OutputName, StringComparer.OrdinalIgnoreCase)
                    .ToList(),
                StringComparer.OrdinalIgnoreCase);

        // Determine which target resources have app settings
        var targetResourcesWithAppSettings = request.AppSettings
            .Select(s => s.TargetResourceName)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var resource in request.Resources)
        {
            var generator = _generators
                .Single(g => g.ResourceType == resource.Type);

            var module = generator.Generate(resource);

            var resourceIdentifier = BicepIdentifierHelper.ToBicepIdentifier(resource.Name);
            var moduleName = $"{module.ModuleName}{Capitalize(resourceIdentifier)}";

            var moduleBicepContent = module.ModuleBicepContent;

            // Inject system-assigned identity and principalId output
            // for resources that are sources of role assignments
            if (sourceResourcesNeedingIdentity.Contains(resource.Name))
            {
                moduleBicepContent = InjectSystemAssignedIdentity(moduleBicepContent);
            }

            // Inject output declarations for resources referenced by app settings
            if (outputsBySourceResource.TryGetValue(resource.Name, out var outputs))
            {
                moduleBicepContent = InjectOutputDeclarations(moduleBicepContent, outputs);
            }

            // Inject appSettings/env param for compute resources that have app settings
            if (targetResourcesWithAppSettings.Contains(resource.Name)
                && ComputeResourceTypes.Contains(resource.Type))
            {
                moduleBicepContent = InjectAppSettingsParam(moduleBicepContent, resource.Type);
            }

            modules.Add(module with
            {
                ModuleName = moduleName,
                ModuleBicepContent = moduleBicepContent,
                ResourceGroupName = resource.ResourceGroupName,
                LogicalResourceName = resource.Name,
                ResourceAbbreviation = resource.ResourceAbbreviation
            });
        }

        return BicepAssembler.Assemble(
            modules,
            request.ResourceGroups,
            request.Environments,
            request.EnvironmentNames,
            request.Resources,
            request.NamingContext,
            request.RoleAssignments,
            request.AppSettings,
            request.ExistingResourceReferences);
    }

    /// <summary>
    /// Generates Bicep files for an entire project in mono-repo mode.
    /// Each configuration is generated independently, then assembled into a shared Common folder
    /// and per-configuration folders.
    /// </summary>
    public MonoRepoGenerationResult GenerateMonoRepo(MonoRepoGenerationRequest request)
    {
        var perConfigResults = new Dictionary<string, GenerationResult>();
        var hasAnyRoleAssignments = false;

        foreach (var (configName, configRequest) in request.ConfigRequests)
        {
            var result = Generate(configRequest);
            perConfigResults[configName] = result;

            if (configRequest.RoleAssignments.Count > 0)
                hasAnyRoleAssignments = true;
        }

        return MonoRepoBicepAssembler.Assemble(
            perConfigResults,
            request.NamingContext,
            request.Environments,
            hasAnyRoleAssignments);
    }

    /// <summary>
    /// Injects <c>identity: { type: 'SystemAssigned' }</c> into the resource declaration
    /// and appends a <c>principalId</c> output to the module template.
    /// </summary>
    private static string InjectSystemAssignedIdentity(string moduleBicep)
    {
        // Extract the resource symbol name from the template
        var symbolMatch = Regex.Match(moduleBicep, @"resource\s+(\w+)\s+'");
        if (!symbolMatch.Success) return moduleBicep;

        var symbol = symbolMatch.Groups[1].Value;

        // Check if identity block already exists
        if (moduleBicep.Contains("identity:"))
            return moduleBicep;

        // Check if principalId output already exists
        if (moduleBicep.Contains("output principalId"))
            return moduleBicep;

        var identityBlock = "  identity: {\n    type: 'SystemAssigned'\n  }\n";

        // Insert identity block before "properties:" line
        var propertiesIdx = moduleBicep.IndexOf("  properties:", StringComparison.Ordinal);
        if (propertiesIdx >= 0)
        {
            moduleBicep = moduleBicep.Insert(propertiesIdx, identityBlock);
        }
        else
        {
            // For resources without properties block (e.g. UserAssignedIdentity),
            // insert before the closing } of the resource block
            var resourcePattern = new Regex($@"resource\s+{Regex.Escape(symbol)}\s+'[^']+'\s*=\s*\{{");
            var resourceMatch = resourcePattern.Match(moduleBicep);
            if (resourceMatch.Success)
            {
                // Find the matching closing brace for this resource
                var braceStart = resourceMatch.Index + resourceMatch.Length;
                var depth = 1;
                var insertPos = -1;
                for (var i = braceStart; i < moduleBicep.Length; i++)
                {
                    if (moduleBicep[i] == '{') depth++;
                    else if (moduleBicep[i] == '}')
                    {
                        depth--;
                        if (depth == 0) { insertPos = i; break; }
                    }
                }

                if (insertPos >= 0)
                {
                    moduleBicep = moduleBicep.Insert(insertPos, identityBlock);
                }
            }
        }

        // Append principalId output
        moduleBicep = moduleBicep.TrimEnd() +
            $"\n\noutput principalId string = {symbol}.identity.principalId\n";

        return moduleBicep;
    }

    private static string Capitalize(string s) =>
        s.Length == 0 ? s : char.ToUpperInvariant(s[0]) + s[1..];

    /// <summary>
    /// Injects <c>output</c> declarations into a module template for outputs
    /// that are referenced by app settings on other resources.
    /// </summary>
    private static string InjectOutputDeclarations(
        string moduleBicep,
        List<(string OutputName, string BicepExpression)> outputs)
    {
        var sb = new System.Text.StringBuilder(moduleBicep.TrimEnd());

        foreach (var (outputName, bicepExpression) in outputs)
        {
            // Skip if output already declared
            if (moduleBicep.Contains($"output {outputName} "))
                continue;

            sb.AppendLine();
            sb.AppendLine();
            sb.Append($"output {outputName} string = {bicepExpression}");
        }

        sb.AppendLine();
        return sb.ToString();
    }

    /// <summary>
    /// Injects an <c>appSettings</c> (or <c>envVars</c> for Container Apps) parameter
    /// into a compute resource module template and wires it into the resource declaration.
    /// </summary>
    private static string InjectAppSettingsParam(string moduleBicep, string resourceType)
    {
        if (resourceType == "Microsoft.App/containerApps")
            return InjectContainerAppEnvVars(moduleBicep);

        return InjectWebFunctionAppSettings(moduleBicep);
    }

    /// <summary>
    /// Injects <c>param appSettings array = []</c> and wires it into
    /// <c>siteConfig.appSettings</c> for WebApp and FunctionApp modules.
    /// </summary>
    private static string InjectWebFunctionAppSettings(string moduleBicep)
    {
        if (moduleBicep.Contains("param appSettings "))
            return moduleBicep;

        // Find the last param line to insert after it
        var paramDecl = "\n@description('Application settings (environment variables)')\nparam appSettings array = []\n";

        // Insert param before the first 'var' or 'resource' line
        var insertIdx = FindFirstLineIndex(moduleBicep, "var ");
        if (insertIdx < 0)
            insertIdx = FindFirstLineIndex(moduleBicep, "resource ");
        if (insertIdx < 0)
            return moduleBicep;

        moduleBicep = moduleBicep.Insert(insertIdx, paramDecl + "\n");

        // Wire into siteConfig: if appSettings already exists in siteConfig, concat; otherwise add
        if (moduleBicep.Contains("appSettings: ["))
        {
            // FunctionApp case: has existing appSettings array — wrap with concat
            var appSettingsStart = moduleBicep.IndexOf("appSettings: [", StringComparison.Ordinal);
            if (appSettingsStart >= 0)
            {
                // Find the matching ] for this appSettings array
                var bracketStart = moduleBicep.IndexOf('[', appSettingsStart);
                var depth = 0;
                var bracketEnd = -1;
                for (var i = bracketStart; i < moduleBicep.Length; i++)
                {
                    if (moduleBicep[i] == '[') depth++;
                    else if (moduleBicep[i] == ']')
                    {
                        depth--;
                        if (depth == 0) { bracketEnd = i; break; }
                    }
                }

                if (bracketEnd >= 0)
                {
                    // Replace "appSettings: [...]" with "appSettings: concat([...], appSettings)"
                    var existingArray = moduleBicep.Substring(bracketStart, bracketEnd - bracketStart + 1);
                    var original = moduleBicep.Substring(appSettingsStart, bracketEnd - appSettingsStart + 1);
                    var replacement = $"appSettings: concat({existingArray}, appSettings)";
                    moduleBicep = moduleBicep.Replace(original, replacement);
                }
            }
        }
        else if (moduleBicep.Contains("siteConfig:"))
        {
            // WebApp case: no existing appSettings in siteConfig — add it
            var siteConfigIdx = moduleBicep.IndexOf("siteConfig:", StringComparison.Ordinal);
            var braceIdx = moduleBicep.IndexOf('{', siteConfigIdx);
            if (braceIdx >= 0)
            {
                moduleBicep = moduleBicep.Insert(braceIdx + 1, "\n      appSettings: appSettings");
            }
        }

        return moduleBicep;
    }

    /// <summary>
    /// Injects <c>param envVars array = []</c> and wires it into
    /// <c>template.containers[].env</c> for Container App modules.
    /// </summary>
    private static string InjectContainerAppEnvVars(string moduleBicep)
    {
        if (moduleBicep.Contains("param envVars "))
            return moduleBicep;

        var paramDecl = "\n@description('Environment variables for the container')\nparam envVars array = []\n";

        var insertIdx = FindFirstLineIndex(moduleBicep, "resource ");
        if (insertIdx < 0)
            return moduleBicep;

        moduleBicep = moduleBicep.Insert(insertIdx, paramDecl + "\n");

        // Add env property to the container spec if not already present
        if (!moduleBicep.Contains("env:"))
        {
            // Find "resources:" in the container spec — insert env after the "memory:" line
            var memoryIdx = moduleBicep.IndexOf("memory:", StringComparison.Ordinal);
            if (memoryIdx >= 0)
            {
                var endOfLine = moduleBicep.IndexOf('\n', memoryIdx);
                if (endOfLine >= 0)
                {
                    moduleBicep = moduleBicep.Insert(endOfLine + 1, "                  env: envVars\n");
                }
            }
        }

        return moduleBicep;
    }

    /// <summary>
    /// Finds the byte index of the first line in the template that starts with the given prefix
    /// (after trimming whitespace). Returns -1 if not found.
    /// </summary>
    private static int FindFirstLineIndex(string content, string linePrefix)
    {
        var lines = content.Split('\n');
        var idx = 0;
        foreach (var line in lines)
        {
            if (line.TrimStart().StartsWith(linePrefix, StringComparison.Ordinal))
                return idx;
            idx += line.Length + 1; // +1 for the '\n'
        }
        return -1;
    }
}
