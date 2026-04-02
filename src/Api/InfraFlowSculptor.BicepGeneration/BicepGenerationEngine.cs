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
        var result = GenerateCore(request);
        PruneModuleOutputs(result);
        return result;
    }

    /// <summary>
    /// Generates Bicep files for an entire project in mono-repo mode.
    /// Each configuration is generated independently, then assembled into a shared Common folder
    /// and per-configuration folders. Unused outputs are pruned using the combined references
    /// from all configuration <c>main.bicep</c> files.
    /// </summary>
    public MonoRepoGenerationResult GenerateMonoRepo(MonoRepoGenerationRequest request)
    {
        var perConfigResults = new Dictionary<string, GenerationResult>();
        var hasAnyRoleAssignments = false;

        foreach (var (configName, configRequest) in request.ConfigRequests)
        {
            var result = GenerateCore(configRequest);
            perConfigResults[configName] = result;

            if (configRequest.RoleAssignments.Count > 0)
                hasAnyRoleAssignments = true;
        }

        var monoResult = MonoRepoBicepAssembler.Assemble(
            perConfigResults,
            request.NamingContext,
            request.Environments,
            hasAnyRoleAssignments);

        PruneMonoRepoModuleOutputs(monoResult, perConfigResults);

        return monoResult;
    }

    /// <summary>
    /// Core generation logic shared by single-config and mono-repo paths.
    /// Returns an un-pruned <see cref="GenerationResult"/>.
    /// </summary>
    private GenerationResult GenerateCore(GenerationRequest request)
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

        // Also include resources with explicitly assigned UAI (even if no UA role assignments)
        foreach (var resource in request.Resources)
        {
            if (resource.AssignedUserAssignedIdentityName is not null)
            {
                var key = (resource.Name, resource.Type);
                var uaiBicepId = BicepIdentifierHelper.ToBicepIdentifier(resource.AssignedUserAssignedIdentityName);
                if (sourceResourcesNeedingUserIdentity.TryGetValue(key, out var existingList))
                {
                    if (!existingList.Contains(uaiBicepId, StringComparer.OrdinalIgnoreCase))
                    {
                        existingList.Add(uaiBicepId);
                    }
                }
                else
                {
                    sourceResourcesNeedingUserIdentity[key] = [uaiBicepId];
                }
            }
        }

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

        // ── Build resource ID → logical name lookup for parent references ───
        var resourceIdToName = request.Resources
            .Where(r => r.ResourceId != Guid.Empty)
            .ToDictionary(r => r.ResourceId, r => r.Name);

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

            // Inject tags param and apply to resource declaration
            moduleBicepContent = InjectTagsParam(moduleBicepContent);

            // ── Resolve parent module references (FK cross-resource links) ──
            var parentModuleIdRefs = new Dictionary<string, string>();
            var parentModuleNameRefs = new Dictionary<string, string>();

            // appServicePlanId: WebApp and FunctionApp → AppServicePlan module outputs.id
            if (resource.Properties.TryGetValue("appServicePlanId", out var aspIdStr)
                && Guid.TryParse(aspIdStr, out var aspGuid)
                && resourceIdToName.TryGetValue(aspGuid, out var aspName))
            {
                parentModuleIdRefs["appServicePlanId"] = aspName;
            }

            // containerAppEnvironmentId: ContainerApp → ContainerAppEnvironment module outputs.id
            if (resource.Properties.TryGetValue("containerAppEnvironmentId", out var caeIdStr)
                && Guid.TryParse(caeIdStr, out var caeGuid)
                && resourceIdToName.TryGetValue(caeGuid, out var caeName))
            {
                parentModuleIdRefs["containerAppEnvironmentId"] = caeName;
            }

            // logAnalyticsWorkspaceId: ApplicationInsights → LogAnalyticsWorkspace module outputs.id
            if (resource.Properties.TryGetValue("logAnalyticsWorkspaceId", out var lawIdStr)
                && Guid.TryParse(lawIdStr, out var lawGuid)
                && resourceIdToName.TryGetValue(lawGuid, out var lawName))
            {
                parentModuleIdRefs["logAnalyticsWorkspaceId"] = lawName;
            }

            // sqlServerId: SqlDatabase → SqlServer module computed name expression
            if (resource.Properties.TryGetValue("sqlServerId", out var sqlIdStr)
                && Guid.TryParse(sqlIdStr, out var sqlGuid)
                && resourceIdToName.TryGetValue(sqlGuid, out var sqlName))
            {
                parentModuleNameRefs["sqlServerName"] = sqlName;
            }

            modules.Add(module with
            {
                ModuleName = moduleName,
                ModuleBicepContent = moduleBicepContent,
                ResourceGroupName = resource.ResourceGroupName,
                LogicalResourceName = resource.Name,
                ResourceAbbreviation = resource.ResourceAbbreviation,
                IdentityKind = identityKind,
                UsesParameterizedIdentity = isMixed,
                ModuleTypesBicepContent = isMixed
                    ? (module.ModuleTypesBicepContent ?? string.Empty) + ManagedIdentityTypeBicepType
                    : module.ModuleTypesBicepContent,
                ParentModuleIdReferences = parentModuleIdRefs,
                ParentModuleNameReferences = parentModuleNameRefs,
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
            request.ExistingResourceReferences,
            request.ProjectTags,
            request.ConfigTags);
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
            uaiEntries.AppendLine($"      '${{{paramName}}}': {{}}");
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
    /// The module receives <c>param identityType ManagedIdentityType</c> (typed union from types.bicep)
    /// and optional UAI resource ID params.
    /// The identity section is built dynamically at deployment time.
    /// Also appends <c>ManagedIdentityType</c> to the module's <c>types.bicep</c> content
    /// and adds the import to the module template.
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

        // Add ManagedIdentityType to the import line in the module template
        moduleBicep = AddTypeImport(moduleBicep, "ManagedIdentityType");

        // Build param declarations
        var paramSb = new System.Text.StringBuilder();
        paramSb.AppendLine("@description('Managed identity type for this resource')");
        paramSb.AppendLine("param identityType ManagedIdentityType = 'SystemAssigned'");
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

        // Build the identity block — always present (no 'None' case)
        string identityBlock;
        if (hasUais)
        {
            identityBlock = """
              identity: {
                type: identityType
                userAssignedIdentities: contains(identityType, 'UserAssigned') ? userAssignedIdentityMap : null
              }

            """;
        }
        else
        {
            identityBlock = """
              identity: {
                type: identityType
              }

            """;
        }

        // Insert identity block before "properties:" line
        var propertiesIdx = moduleBicep.IndexOf("  properties:", StringComparison.Ordinal);
        if (propertiesIdx >= 0)
        {
            moduleBicep = moduleBicep.Insert(propertiesIdx, identityBlock);
        }

        // Append principalId output (empty string when no SystemAssigned component)
        if (!moduleBicep.Contains("output principalId"))
        {
            moduleBicep = moduleBicep.TrimEnd() +
                $"\n\noutput principalId string = contains(identityType, 'SystemAssigned') ? {symbol}.identity.principalId : ''\n";
        }

        return moduleBicep;
    }

    /// <summary>
    /// The <c>ManagedIdentityType</c> Bicep type definition to append to a module's <c>types.bicep</c>.
    /// </summary>
    internal const string ManagedIdentityTypeBicepType = """

        @export()
        @description('Managed identity type for Azure resources')
        type ManagedIdentityType = 'SystemAssigned' | 'UserAssigned' | 'SystemAssigned, UserAssigned'
        """;

    /// <summary>
    /// Adds <paramref name="typeName"/> to the existing <c>import { ... } from './types.bicep'</c> line.
    /// If no import line exists, prepends a new one.
    /// </summary>
    private static string AddTypeImport(string moduleBicep, string typeName)
    {
        // Match existing import line: import { Foo, Bar } from './types.bicep'
        var importRegex = new Regex(@"import\s*\{([^}]+)\}\s*from\s*'\.\/types\.bicep'");
        var match = importRegex.Match(moduleBicep);
        if (match.Success)
        {
            var existingImports = match.Groups[1].Value;
            if (existingImports.Contains(typeName))
                return moduleBicep;

            var newImports = existingImports.TrimEnd() + ", " + typeName + " ";
            return moduleBicep.Replace(match.Value, $"import {{{newImports}}} from './types.bicep'");
        }

        // No import line — prepend one
        return $"import {{ {typeName} }} from './types.bicep'\n\n" + moduleBicep;
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
    /// Injects <c>param tags object = {}</c> and <c>tags: tags</c> into a resource module template.
    /// </summary>
    private static string InjectTagsParam(string moduleBicep)
    {
        if (moduleBicep.Contains("param tags "))
            return moduleBicep;

        // Add the tags param declaration after existing params
        var paramDecl = "\n@description('Resource tags')\nparam tags object = {}\n";

        // Find place to insert: after the last param line
        var lastParamIdx = moduleBicep.LastIndexOf("\nparam ", StringComparison.Ordinal);
        if (lastParamIdx >= 0)
        {
            var endOfLine = moduleBicep.IndexOf('\n', lastParamIdx + 1);
            if (endOfLine >= 0)
            {
                moduleBicep = moduleBicep.Insert(endOfLine, paramDecl);
            }
        }

        // Add tags: tags to the resource declaration, after "location: location"
        var locationIdx = moduleBicep.IndexOf("location: location", StringComparison.Ordinal);
        if (locationIdx >= 0)
        {
            var endOfLocationLine = moduleBicep.IndexOf('\n', locationIdx);
            if (endOfLocationLine >= 0)
            {
                moduleBicep = moduleBicep.Insert(endOfLocationLine, "\n  tags: tags");
            }
        }

        return moduleBicep;
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

    // ────────────────────────────────────────────────────────────────────────
    // Module output pruning
    // ────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Prunes unused output declarations from the module files of a single-config
    /// <see cref="GenerationResult"/> based on output references in <c>main.bicep</c>.
    /// </summary>
    private static void PruneModuleOutputs(GenerationResult result)
    {
        if (result.ModuleFiles is not Dictionary<string, string> mutableModuleFiles)
            return;

        var usedOutputsByPath = CollectUsedOutputsByPath(result.MainBicep, mutableModuleFiles);

        foreach (var path in mutableModuleFiles.Keys.ToList())
        {
            var usedOutputs = usedOutputsByPath.GetValueOrDefault(path)
                ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            mutableModuleFiles[path] = BicepAssembler.PruneUnusedOutputs(mutableModuleFiles[path], usedOutputs);
        }
    }

    /// <summary>
    /// Prunes unused output declarations from the shared <c>CommonFiles</c> of a
    /// <see cref="MonoRepoGenerationResult"/> using the combined output references from
    /// all per-config <c>main.bicep</c> files.
    /// </summary>
    private static void PruneMonoRepoModuleOutputs(
        MonoRepoGenerationResult monoResult,
        IReadOnlyDictionary<string, GenerationResult> perConfigResults)
    {
        // Collect used outputs from ALL config main.bicep files (union)
        var combinedUsedOutputs = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var (_, configResult) in perConfigResults)
        {
            // Use the per-config ModuleFiles to map module names to file paths
            var configUsed = CollectUsedOutputsByPath(configResult.MainBicep, configResult.ModuleFiles);

            foreach (var (path, outputs) in configUsed)
            {
                // In mono-repo, common files use prefix "modules/" directly
                if (!combinedUsedOutputs.TryGetValue(path, out var existing))
                {
                    existing = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    combinedUsedOutputs[path] = existing;
                }

                existing.UnionWith(outputs);
            }
        }

        // Prune the shared common module files
        var mutableCommon = monoResult.CommonFiles as Dictionary<string, string>
            ?? new Dictionary<string, string>(monoResult.CommonFiles);

        foreach (var path in mutableCommon.Keys.ToList())
        {
            if (!path.StartsWith("modules/", StringComparison.OrdinalIgnoreCase))
                continue;

            var usedOutputs = combinedUsedOutputs.GetValueOrDefault(path)
                ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            mutableCommon[path] = BicepAssembler.PruneUnusedOutputs(mutableCommon[path], usedOutputs);
        }
    }

    /// <summary>
    /// Scans <c>main.bicep</c> for <c>{moduleName}Module.outputs.{outputName}</c> references
    /// and maps them to module file paths using the module file dictionary keys.
    /// </summary>
    private static Dictionary<string, HashSet<string>> CollectUsedOutputsByPath(
        string mainBicep,
        IReadOnlyDictionary<string, string> moduleFiles)
    {
        // Build a lookup from module symbol prefix → file path.
        // Module symbols in main.bicep follow the pattern: {moduleName}Module
        // Module file paths follow: modules/{Folder}/{fileName}
        // We extract the module base name from the file path and match with main.bicep symbols.
        var outputRefRegex = new Regex(@"(\w+)Module\.outputs\.(\w+)");
        var matches = outputRefRegex.Matches(mainBicep);

        // Build reverse lookup: module symbol name (before "Module") → module file paths
        // We'll match module names from main.bicep when they appear in the match set.
        var symbolToPath = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var match in matches.Cast<Match>())
        {
            var symbolName = match.Groups[1].Value;
            if (symbolToPath.ContainsKey(symbolName))
                continue;

            // Find the module file path by matching the module declaration in main.bicep
            // Pattern: module {symbolName}Module './modules/{Folder}/{fileName}'
            var declRegex = new Regex(
                @"module\s+" + Regex.Escape(symbolName) + @"Module\s+'\.\/([^']+)'");
            var declMatch = declRegex.Match(mainBicep);
            if (declMatch.Success)
            {
                symbolToPath[symbolName] = declMatch.Groups[1].Value;
            }
        }

        var result = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (Match match in matches)
        {
            var symbolName = match.Groups[1].Value;
            var outputName = match.Groups[2].Value;

            if (!symbolToPath.TryGetValue(symbolName, out var filePath))
                continue;

            if (!result.TryGetValue(filePath, out var outputs))
            {
                outputs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                result[filePath] = outputs;
            }

            outputs.Add(outputName);
        }

        return result;
    }
}
