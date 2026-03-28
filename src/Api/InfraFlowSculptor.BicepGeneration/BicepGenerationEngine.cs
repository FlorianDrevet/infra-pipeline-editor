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

        // ── Compute per-resource identity needs ─────────────────────────────
        // Key = (Name, ARM ResourceType).
        var sourceResourcesNeedingSystemIdentity = request.RoleAssignments
            .Where(ra => ra.ManagedIdentityType == "SystemAssigned")
            .Select(ra => (ra.SourceResourceName, ra.SourceResourceType))
            .ToHashSet();

        var sourceResourcesNeedingUserIdentity = request.RoleAssignments
            .Where(ra => ra.ManagedIdentityType == "UserAssigned" && ra.UserAssignedIdentityName is not null)
            .GroupBy(ra => (ra.SourceResourceName, ra.SourceResourceType))
            .ToDictionary(
                g => g.Key,
                g => g.Select(ra => BicepIdentifierHelper.ToBicepIdentifier(ra.UserAssignedIdentityName!))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList());

        // ── Determine per ARM type whether identity is uniform or mixed ─────
        // For each ARM type (excluding UserAssignedIdentity itself), collect the set of
        // distinct identity kinds needed. If only one kind → hardcode in the module template.
        // If multiple kinds → parameterize the identity in the module template.
        var identityKindsByArmType = ComputeIdentityKindsByArmType(
            request.Resources, sourceResourcesNeedingSystemIdentity, sourceResourcesNeedingUserIdentity);

        var mixedIdentityArmTypes = identityKindsByArmType
            .Where(kv => kv.Value.Count > 1)
            .Select(kv => kv.Key)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Determine which source resources need output declarations for app settings
        // Include both regular output references AND sensitive outputs exported to KV
        var outputsBySourceResource = request.AppSettings
            .Where(s => (s.IsOutputReference || s.IsSensitiveOutputExportedToKeyVault)
                && s.SourceResourceName is not null
                && s.SourceOutputName is not null && s.SourceOutputBicepExpression is not null)
            .GroupBy(s => s.SourceResourceName!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                g => g.Key,
                g => g.Select(s => (
                        OutputName: s.SourceOutputName!,
                        BicepExpression: s.SourceOutputBicepExpression!,
                        IsSecure: s.IsSensitiveOutputExportedToKeyVault))
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

            // ── Identity injection ──────────────────────────────────────────
            var resourceKey = (resource.Name, resource.Type);
            var needsSystem = sourceResourcesNeedingSystemIdentity.Contains(resourceKey);
            sourceResourcesNeedingUserIdentity.TryGetValue(resourceKey, out var uaiIdentifiers);
            var needsUser = resource.Type != "Microsoft.ManagedIdentity/userAssignedIdentities"
                && uaiIdentifiers is { Count: > 0 };
            var isMixed = mixedIdentityArmTypes.Contains(resource.Type);

            string? identityKind = null;
            if (needsSystem && needsUser)
                identityKind = "SystemAssigned, UserAssigned";
            else if (needsSystem)
                identityKind = "SystemAssigned";
            else if (needsUser)
                identityKind = "UserAssigned";

            if (isMixed)
            {
                // Mixed ARM type: inject parameterized identity into the module template.
                // This makes the module accept identityType + optional UAI params.
                // All UAI param declarations referenced by ANY resource of this type are injected;
                // each instance passes only the UAI ids it needs from main.bicep.
                var allUaiIdsForType = sourceResourcesNeedingUserIdentity
                    .Where(kv => kv.Key.SourceResourceType == resource.Type)
                    .SelectMany(kv => kv.Value)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                moduleBicepContent = InjectParameterizedIdentity(moduleBicepContent, allUaiIdsForType);
            }
            else
            {
                // Uniform ARM type: hardcode identity block directly in the module template.
                if (needsSystem)
                {
                    moduleBicepContent = InjectSystemAssignedIdentity(moduleBicepContent);
                }

                if (needsUser)
                {
                    moduleBicepContent = InjectUserAssignedIdentity(
                        moduleBicepContent, uaiIdentifiers!, needsSystem);
                }
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
                ResourceAbbreviation = resource.ResourceAbbreviation,
                IdentityKind = identityKind,
                UsesParameterizedIdentity = isMixed
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

        // Check if identity block already exists (match indented block, not param substrings)
        if (moduleBicep.Contains("\n  identity:"))
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

    /// <summary>
    /// Injects <c>identity: { type: 'UserAssigned', userAssignedIdentities: { ... } }</c>
    /// into the resource declaration. When the resource also has SystemAssigned identity,
    /// upgrades to <c>'SystemAssigned, UserAssigned'</c>.
    /// Each UAI is referenced via a resource-id parameter.
    /// </summary>
    private static string InjectUserAssignedIdentity(
        string moduleBicep,
        List<string> uaiIdentifiers,
        bool alsoHasSystemAssigned)
    {
        // Build parameter declarations for each UAI resource ID
        var paramDeclarations = new System.Text.StringBuilder();
        foreach (var uaiId in uaiIdentifiers)
        {
            var paramName = $"userAssignedIdentity{Capitalize(uaiId)}Id";
            if (moduleBicep.Contains($"param {paramName} "))
                continue;
            paramDeclarations.AppendLine($"@description('Resource ID of user-assigned identity: {uaiId}')");
            paramDeclarations.AppendLine($"param {paramName} string");
        }

        // Insert params before the first 'var' or 'resource' line
        if (paramDeclarations.Length > 0)
        {
            var insertIdx = FindFirstLineIndex(moduleBicep, "var ");
            if (insertIdx < 0)
                insertIdx = FindFirstLineIndex(moduleBicep, "resource ");
            if (insertIdx >= 0)
            {
                moduleBicep = moduleBicep.Insert(insertIdx, paramDeclarations.ToString() + "\n");
            }
        }

        // Build the userAssignedIdentities object entries
        var uaiEntries = new System.Text.StringBuilder();
        foreach (var uaiId in uaiIdentifiers)
        {
            var paramName = $"userAssignedIdentity{Capitalize(uaiId)}Id";
            uaiEntries.AppendLine($"      '${{${paramName}}}': {{}}");
        }

        // Determine desired identity type
        var identityType = alsoHasSystemAssigned
            ? "SystemAssigned, UserAssigned"
            : "UserAssigned";

        if (moduleBicep.Contains("\n  identity:"))
        {
            // Identity block already exists (SystemAssigned was injected first).
            // Replace type and add userAssignedIdentities.
            moduleBicep = moduleBicep.Replace(
                "type: 'SystemAssigned'",
                $"type: '{identityType}'\n    userAssignedIdentities: {{\n{uaiEntries}    }}");
        }
        else
        {
            // No identity block yet — inject before 'properties:' or before closing brace
            var identityBlock =
                $"  identity: {{\n    type: '{identityType}'\n    userAssignedIdentities: {{\n{uaiEntries}    }}\n  }}\n";

            var propertiesIdx = moduleBicep.IndexOf("  properties:", StringComparison.Ordinal);
            if (propertiesIdx >= 0)
            {
                moduleBicep = moduleBicep.Insert(propertiesIdx, identityBlock);
            }
            else
            {
                // For resources without properties block, insert before closing } of the resource
                var symbolMatch = Regex.Match(moduleBicep, @"resource\s+(\w+)\s+'");
                if (symbolMatch.Success)
                {
                    var symbol = symbolMatch.Groups[1].Value;
                    var resourcePattern = new Regex($@"resource\s+{Regex.Escape(symbol)}\s+'[^']+'\s*=\s*\{{");
                    var resourceMatch = resourcePattern.Match(moduleBicep);
                    if (resourceMatch.Success)
                    {
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
            }
        }

        return moduleBicep;
    }

    /// <summary>
    /// Analyses all resources in the request to compute, for each ARM resource type,
    /// the set of distinct identity kinds (<c>SystemAssigned</c>, <c>UserAssigned</c>,
    /// <c>SystemAssigned, UserAssigned</c>) required by the instances of that type.
    /// </summary>
    private static Dictionary<string, HashSet<string>> ComputeIdentityKindsByArmType(
        IEnumerable<ResourceDefinition> resources,
        HashSet<(string Name, string Type)> systemIdentityResources,
        Dictionary<(string Name, string Type), List<string>> userIdentityResources)
    {
        var result = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var resource in resources)
        {
            if (resource.Type == "Microsoft.ManagedIdentity/userAssignedIdentities")
                continue;

            var key = (resource.Name, resource.Type);
            var hasSystem = systemIdentityResources.Contains(key);
            var hasUser = userIdentityResources.ContainsKey(key);

            string? kind = null;
            if (hasSystem && hasUser)
                kind = "SystemAssigned, UserAssigned";
            else if (hasSystem)
                kind = "SystemAssigned";
            else if (hasUser)
                kind = "UserAssigned";

            if (kind is null) continue;

            if (!result.TryGetValue(resource.Type, out var kindSet))
            {
                kindSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                result[resource.Type] = kindSet;
            }

            kindSet.Add(kind);
        }

        return result;
    }

    /// <summary>
    /// Injects a parameterized identity block into the module template.
    /// The module receives <c>param identityType string</c> and optional UAI resource ID params.
    /// The identity section is built dynamically at deployment time.
    /// </summary>
    private static string InjectParameterizedIdentity(string moduleBicep, List<string> allUaiIds)
    {
        var symbolMatch = Regex.Match(moduleBicep, @"resource\s+(\w+)\s+'");
        if (!symbolMatch.Success) return moduleBicep;

        var symbol = symbolMatch.Groups[1].Value;

        // Check if we already injected parameterized identity
        if (moduleBicep.Contains("param identityType "))
            return moduleBicep;

        // Check if a hardcoded identity block exists (shouldn't in mixed case, but guard)
        if (moduleBicep.Contains("\n  identity:"))
            return moduleBicep;

        // Build param declarations
        var paramSb = new System.Text.StringBuilder();
        paramSb.AppendLine("@description('Managed identity type for this resource (None, SystemAssigned, UserAssigned, or SystemAssigned, UserAssigned)')");
        paramSb.AppendLine("param identityType string = 'None'");
        paramSb.AppendLine();

        foreach (var uaiId in allUaiIds)
        {
            var paramName = $"userAssignedIdentity{Capitalize(uaiId)}Id";
            if (!moduleBicep.Contains($"param {paramName} "))
            {
                paramSb.AppendLine($"@description('Resource ID of user-assigned identity: {uaiId} (empty when not applicable)')");
                paramSb.AppendLine($"param {paramName} string = ''");
                paramSb.AppendLine();
            }
        }

        // Insert params before the first 'var' or 'resource' line
        var insertIdx = FindFirstLineIndex(moduleBicep, "var ");
        if (insertIdx < 0)
            insertIdx = FindFirstLineIndex(moduleBicep, "resource ");
        if (insertIdx >= 0)
        {
            moduleBicep = moduleBicep.Insert(insertIdx, paramSb.ToString());
        }

        // Build: var userAssignedIdentities = { '${uai1Id}': {}, '${uai2Id}': {} }
        // Only included when identityType contains 'UserAssigned'
        var hasUais = allUaiIds.Count > 0;
        if (hasUais)
        {
            var varSb = new System.Text.StringBuilder();
            varSb.AppendLine("var userAssignedIdentityMap = {");
            foreach (var uaiId in allUaiIds)
            {
                var paramName = $"userAssignedIdentity{Capitalize(uaiId)}Id";
                varSb.AppendLine($"  '${{{paramName}}}': {{}}");
            }
            varSb.AppendLine("}");
            varSb.AppendLine();

            var resourceIdx = FindFirstLineIndex(moduleBicep, "resource ");
            if (resourceIdx >= 0)
            {
                moduleBicep = moduleBicep.Insert(resourceIdx, varSb.ToString());
            }
        }

        // Build the conditional identity block
        string identityBlock;
        if (hasUais)
        {
            identityBlock = """
              identity: identityType != 'None' ? {
                type: identityType
                userAssignedIdentities: contains(identityType, 'UserAssigned') ? userAssignedIdentityMap : null
              } : null

            """;
        }
        else
        {
            identityBlock = """
              identity: identityType != 'None' ? {
                type: identityType
              } : null

            """;
        }

        // Insert identity block before "properties:" line
        var propertiesIdx = moduleBicep.IndexOf("  properties:", StringComparison.Ordinal);
        if (propertiesIdx >= 0)
        {
            moduleBicep = moduleBicep.Insert(propertiesIdx, identityBlock);
        }

        // Append principalId output (only meaningful for SystemAssigned, but safe to include always)
        if (!moduleBicep.Contains("output principalId"))
        {
            moduleBicep = moduleBicep.TrimEnd() +
                $"\n\noutput principalId string = contains(identityType, 'SystemAssigned') ? {symbol}.identity.principalId : ''\n";
        }

        return moduleBicep;
    }

    private static string Capitalize(string s) =>
        s.Length == 0 ? s : char.ToUpperInvariant(s[0]) + s[1..];

    /// <summary>
    /// Injects <c>output</c> declarations into a module template for outputs
    /// that are referenced by app settings on other resources.
    /// Sensitive outputs are decorated with <c>@secure()</c> to prevent leaking to deployment logs.
    /// </summary>
    private static string InjectOutputDeclarations(
        string moduleBicep,
        List<(string OutputName, string BicepExpression, bool IsSecure)> outputs)
    {
        var sb = new System.Text.StringBuilder(moduleBicep.TrimEnd());

        foreach (var (outputName, bicepExpression, isSecure) in outputs)
        {
            // Skip if output already declared
            if (moduleBicep.Contains($"output {outputName} "))
                continue;

            sb.AppendLine();
            if (isSecure)
            {
                sb.AppendLine();
                sb.AppendLine("@secure()");
                sb.Append($"output {outputName} string = {bicepExpression}");
            }
            else
            {
                sb.AppendLine();
                sb.Append($"output {outputName} string = {bicepExpression}");
            }
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
