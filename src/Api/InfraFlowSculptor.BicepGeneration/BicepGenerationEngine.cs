using System.Text;
using System.Text.RegularExpressions;
using InfraFlowSculptor.BicepGeneration.Generators;
using InfraFlowSculptor.BicepGeneration.Helpers;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.GenerationCore;

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

    // ── Compiled regex patterns for whitespace-tolerant Bicep template matching ──
    private static readonly Regex ResourceSymbolPattern = new(@"resource\s+(\w+)\s+'", RegexOptions.Compiled);
    private static readonly Regex IdentityBlockPattern = new(@"^\s+identity\s*:", RegexOptions.Multiline | RegexOptions.Compiled);
    private static readonly Regex PropertiesBlockPattern = new(@"^\s+properties\s*:", RegexOptions.Multiline | RegexOptions.Compiled);
    private static readonly Regex OutputPrincipalIdPattern = new(@"^output\s+principalId\b", RegexOptions.Multiline | RegexOptions.Compiled);
    private static readonly Regex ParamIdentityTypePattern = new(@"^param\s+identityType\b", RegexOptions.Multiline | RegexOptions.Compiled);
    private static readonly Regex ParamTagsPattern = new(@"^param\s+tags\b", RegexOptions.Multiline | RegexOptions.Compiled);
    private static readonly Regex ParamAppSettingsPattern = new(@"^param\s+appSettings\b", RegexOptions.Multiline | RegexOptions.Compiled);
    private static readonly Regex ParamEnvVarsPattern = new(@"^param\s+envVars\b", RegexOptions.Multiline | RegexOptions.Compiled);
    private static readonly Regex LocationPropertyPattern = new(@"^\s+location\s*:\s*location\b", RegexOptions.Multiline | RegexOptions.Compiled);
    private static readonly Regex AppSettingsArrayPattern = new(@"appSettings\s*:\s*\[", RegexOptions.Compiled);
    private static readonly Regex SiteConfigPattern = new(@"\bsiteConfig\s*:", RegexOptions.Compiled);
    private static readonly Regex EnvPropertyPattern = new(@"\benv\s*:", RegexOptions.Compiled);
    private static readonly Regex ContainerResourcesPattern = new(@"^\s+resources\s*:\s*\{", RegexOptions.Multiline | RegexOptions.Compiled);

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

        // ── Build resource ID → (name, resource type name) lookup for parent references ──
        var generatorByArmType = _generators.ToDictionary(g => g.ResourceType, g => g, StringComparer.OrdinalIgnoreCase);
        var resourceIdToInfo = request.Resources
            .Where(r => r.ResourceId != Guid.Empty)
            .ToDictionary(r => r.ResourceId, r =>
            {
                var typeName = generatorByArmType.TryGetValue(r.Type, out var gen) ? gen.ResourceTypeName : string.Empty;
                return (Name: r.Name, ResourceTypeName: typeName);
            });

        foreach (var resource in request.Resources)
        {
            var generator = _generators
                .SingleOrDefault(g => g.ResourceType == resource.Type)
                ?? throw new NotSupportedException(
                    $"No Bicep generator registered for resource '{resource.Name}' with resource type '{resource.Type}'.");

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
            var parentModuleIdRefs = new Dictionary<string, (string Name, string ResourceTypeName)>();
            var parentModuleNameRefs = new Dictionary<string, (string Name, string ResourceTypeName)>();
            var existingResourceIdRefs = new Dictionary<string, string>();

            // appServicePlanId: WebApp and FunctionApp → AppServicePlan module outputs.id
            if (resource.Properties.TryGetValue("appServicePlanId", out var aspIdStr)
                && Guid.TryParse(aspIdStr, out var aspGuid)
                && resourceIdToInfo.TryGetValue(aspGuid, out var aspInfo))
            {
                parentModuleIdRefs["appServicePlanId"] = aspInfo;
            }

            // containerAppEnvironmentId: ContainerApp → ContainerAppEnvironment module outputs.id
            if (resource.Properties.TryGetValue("containerAppEnvironmentId", out var caeIdStr)
                && Guid.TryParse(caeIdStr, out var caeGuid)
                && resourceIdToInfo.TryGetValue(caeGuid, out var caeInfo))
            {
                parentModuleIdRefs["containerAppEnvironmentId"] = caeInfo;
            }

            // logAnalyticsWorkspaceId: ApplicationInsights / ContainerAppEnvironment → LogAnalyticsWorkspace module outputs.id
            if (resource.Properties.TryGetValue("logAnalyticsWorkspaceId", out var lawIdStr)
                && Guid.TryParse(lawIdStr, out var lawGuid)
                && resourceIdToInfo.TryGetValue(lawGuid, out var lawInfo))
            {
                parentModuleIdRefs["logAnalyticsWorkspaceId"] = lawInfo;
            }
            else if (resource.Type is "Microsoft.Insights/components" or "Microsoft.App/managedEnvironments"
                && !parentModuleIdRefs.ContainsKey("logAnalyticsWorkspaceId"))
            {
                // Fallback: auto-detect LAW in the same config when the FK property isn't set.
                var fallbackLaw = request.Resources.FirstOrDefault(r =>
                    r.Type.Equals("Microsoft.OperationalInsights/workspaces", StringComparison.OrdinalIgnoreCase));
                if (fallbackLaw is not null)
                {
                    parentModuleIdRefs["logAnalyticsWorkspaceId"] = (fallbackLaw.Name, AzureResourceTypes.LogAnalyticsWorkspace);
                }
                else
                {
                    // Second fallback: cross-configuration existing LAW reference
                    var existingLaw = request.ExistingResourceReferences.FirstOrDefault(r =>
                        r.ResourceType.Equals("Microsoft.OperationalInsights/workspaces", StringComparison.OrdinalIgnoreCase));
                    if (existingLaw is not null)
                    {
                        existingResourceIdRefs["logAnalyticsWorkspaceId"] = existingLaw.ResourceName;
                    }
                }
            }

            // sqlServerId: SqlDatabase → SqlServer module computed name expression
            if (resource.Properties.TryGetValue("sqlServerId", out var sqlIdStr)
                && Guid.TryParse(sqlIdStr, out var sqlGuid)
                && resourceIdToInfo.TryGetValue(sqlGuid, out var sqlInfo))
            {
                parentModuleNameRefs["sqlServerName"] = sqlInfo;
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
                ExistingResourceIdReferences = existingResourceIdRefs,
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
        var symbolMatch = ResourceSymbolPattern.Match(moduleBicep);
        if (!symbolMatch.Success) return moduleBicep;

        var symbol = symbolMatch.Groups[1].Value;

        // Check if identity block already exists (whitespace-tolerant regex)
        if (IdentityBlockPattern.IsMatch(moduleBicep))
            return moduleBicep;

        // Check if principalId output already exists
        if (OutputPrincipalIdPattern.IsMatch(moduleBicep))
            return moduleBicep;

        var identityBlock = "  identity: {\n    type: 'SystemAssigned'\n  }\n";

        // Insert identity block before "properties:" line (whitespace-tolerant)
        var propertiesMatch = PropertiesBlockPattern.Match(moduleBicep);
        if (propertiesMatch.Success)
        {
            moduleBicep = InsertAt(moduleBicep, propertiesMatch.Index, identityBlock);
        }
        else
        {
            // For resources without properties block (e.g. UserAssignedIdentity),
            // insert before the closing } of the resource block
            var resourcePattern = new Regex($@"resource\s+{Regex.Escape(symbol)}\s+'[^']+'\s*=\s*\{{");
            var resourceMatch = resourcePattern.Match(moduleBicep);
            if (resourceMatch.Success)
            {
                var insertPos = FindClosingBrace(moduleBicep, resourceMatch.Index + resourceMatch.Length);
                if (insertPos >= 0)
                {
                    moduleBicep = InsertAt(moduleBicep, insertPos, identityBlock);
                }
            }
        }

        // Append principalId output
        var sb = new StringBuilder(moduleBicep.TrimEnd());
        sb.AppendLine();
        sb.AppendLine();
        sb.Append($"output principalId string = {symbol}.identity.principalId");
        sb.AppendLine();
        return sb.ToString();
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
        var paramDeclarations = new StringBuilder();
        foreach (var uaiId in uaiIdentifiers)
        {
            var paramName = $"userAssignedIdentity{Capitalize(uaiId)}Id";
            if (HasParam(moduleBicep, paramName))
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
                moduleBicep = InsertAt(moduleBicep, insertIdx, paramDeclarations.ToString() + "\n");
            }
        }

        // Build the userAssignedIdentities object entries
        var uaiEntries = new StringBuilder();
        foreach (var uaiId in uaiIdentifiers)
        {
            var paramName = $"userAssignedIdentity{Capitalize(uaiId)}Id";
            uaiEntries.AppendLine($"      '${{{paramName}}}': {{}}");
        }

        // Determine desired identity type
        var identityType = alsoHasSystemAssigned
            ? "SystemAssigned, UserAssigned"
            : "UserAssigned";

        if (IdentityBlockPattern.IsMatch(moduleBicep))
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

            var propertiesMatch = PropertiesBlockPattern.Match(moduleBicep);
            if (propertiesMatch.Success)
            {
                moduleBicep = InsertAt(moduleBicep, propertiesMatch.Index, identityBlock);
            }
            else
            {
                // For resources without properties block, insert before closing } of the resource
                var symbolMatch = ResourceSymbolPattern.Match(moduleBicep);
                if (symbolMatch.Success)
                {
                    var symbol = symbolMatch.Groups[1].Value;
                    var resourcePattern = new Regex($@"resource\s+{Regex.Escape(symbol)}\s+'[^']+'\s*=\s*\{{");
                    var resourceMatch = resourcePattern.Match(moduleBicep);
                    if (resourceMatch.Success)
                    {
                        var insertPos = FindClosingBrace(moduleBicep, resourceMatch.Index + resourceMatch.Length);
                        if (insertPos >= 0)
                        {
                            moduleBicep = InsertAt(moduleBicep, insertPos, identityBlock);
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
        var symbolMatch = ResourceSymbolPattern.Match(moduleBicep);
        if (!symbolMatch.Success) return moduleBicep;

        var symbol = symbolMatch.Groups[1].Value;

        // Check if we already injected parameterized identity (whitespace-tolerant)
        if (ParamIdentityTypePattern.IsMatch(moduleBicep))
            return moduleBicep;

        // Check if a hardcoded identity block exists (shouldn't in mixed case, but guard)
        if (IdentityBlockPattern.IsMatch(moduleBicep))
            return moduleBicep;

        // Add ManagedIdentityType to the import line in the module template
        moduleBicep = AddTypeImport(moduleBicep, "ManagedIdentityType");

        // Build param declarations
        var paramSb = new StringBuilder();
        paramSb.AppendLine("@description('Managed identity type for this resource')");
        paramSb.AppendLine("param identityType ManagedIdentityType = 'SystemAssigned'");
        paramSb.AppendLine();

        foreach (var uaiId in allUaiIds)
        {
            var paramName = $"userAssignedIdentity{Capitalize(uaiId)}Id";
            if (!HasParam(moduleBicep, paramName))
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
            moduleBicep = InsertAt(moduleBicep, insertIdx, paramSb.ToString());
        }

        // Build: var userAssignedIdentities = { '${uai1Id}': {}, '${uai2Id}': {} }
        // Only included when identityType contains 'UserAssigned'
        var hasUais = allUaiIds.Count > 0;
        if (hasUais)
        {
            var varSb = new StringBuilder();
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
                moduleBicep = InsertAt(moduleBicep, resourceIdx, varSb.ToString());
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

        // Insert identity block before "properties:" line (whitespace-tolerant)
        var propertiesMatch = PropertiesBlockPattern.Match(moduleBicep);
        if (propertiesMatch.Success)
        {
            moduleBicep = InsertAt(moduleBicep, propertiesMatch.Index, identityBlock);
        }

        // Append principalId output (empty string when no SystemAssigned component)
        if (!OutputPrincipalIdPattern.IsMatch(moduleBicep))
        {
            var sb = new StringBuilder(moduleBicep.TrimEnd());
            sb.AppendLine();
            sb.AppendLine();
            sb.Append($"output principalId string = contains(identityType, 'SystemAssigned') ? {symbol}.identity.principalId : ''");
            sb.AppendLine();
            moduleBicep = sb.ToString();
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
        // Collect all resource symbol names declared in the module (e.g. "sqlDatabase", "kv").
        var declaredSymbols = new HashSet<string>(StringComparer.Ordinal);
        foreach (Match match in ResourceSymbolPattern.Matches(moduleBicep))
        {
            declaredSymbols.Add(match.Groups[1].Value);
        }

        var sb = new System.Text.StringBuilder(moduleBicep.TrimEnd());

        foreach (var (outputName, bicepExpression, isSecure) in outputs)
        {
            // Skip if output already declared (whitespace-tolerant)
            if (HasOutput(moduleBicep, outputName))
                continue;

            // Validate: the root symbol in the expression must exist in this module.
            // Expression examples: "kv.properties.vaultUri", "'Server=tcp:${sqlServer.properties...}'"
            var rootSymbol = ExtractRootSymbol(bicepExpression);
            if (rootSymbol is not null && !declaredSymbols.Contains(rootSymbol))
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
    /// Extracts the root resource symbol from a Bicep expression.
    /// For <c>"kv.properties.vaultUri"</c> returns <c>"kv"</c>.
    /// For interpolated strings like <c>"'${sqlServer.properties.fqdn}'"</c> returns <c>"sqlServer"</c>.
    /// Returns <c>null</c> when the expression is a literal or cannot be parsed.
    /// </summary>
    private static string? ExtractRootSymbol(string bicepExpression)
    {
        var expr = bicepExpression.Trim();

        // Try direct identifier: "symbol.property..."
        if (char.IsLetter(expr[0]) || expr[0] == '_')
        {
            var dotIndex = expr.IndexOf('.');
            return dotIndex > 0 ? expr[..dotIndex] : null;
        }

        // Try interpolated string: "'...${symbol.property}...'"
        var interpIdx = expr.IndexOf("${", StringComparison.Ordinal);
        if (interpIdx >= 0)
        {
            var start = interpIdx + 2;
            var rest = expr.AsSpan(start);
            var dotPos = rest.IndexOf('.');
            if (dotPos > 0)
            {
                return rest[..dotPos].ToString();
            }
        }

        return null;
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
        if (ParamTagsPattern.IsMatch(moduleBicep))
            return moduleBicep;

        // Add the tags param declaration after existing params
        var paramDecl = "\n@description('Resource tags')\nparam tags object = {}\n";

        // Find place to insert: after the last param line (whitespace-tolerant)
        var paramMatches = Regex.Matches(moduleBicep, @"\nparam\s+");
        if (paramMatches.Count > 0)
        {
            var lastMatch = paramMatches[^1];
            var endOfLine = moduleBicep.IndexOf('\n', lastMatch.Index + 1);
            if (endOfLine >= 0)
            {
                moduleBicep = InsertAt(moduleBicep, endOfLine, paramDecl);
            }
        }

        // Add tags: tags to the resource declaration, after "location: location" (whitespace-tolerant)
        var locationMatch = LocationPropertyPattern.Match(moduleBicep);
        if (locationMatch.Success)
        {
            var endOfLocationLine = moduleBicep.IndexOf('\n', locationMatch.Index + locationMatch.Length);
            if (endOfLocationLine >= 0)
            {
                moduleBicep = InsertAt(moduleBicep, endOfLocationLine, "\n  tags: tags");
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
        if (ParamAppSettingsPattern.IsMatch(moduleBicep))
            return moduleBicep;

        // Find the last param line to insert after it
        var paramDecl = "\n@description('Application settings (environment variables)')\nparam appSettings array = []\n";

        // Insert param before the first 'var' or 'resource' line
        var insertIdx = FindFirstLineIndex(moduleBicep, "var ");
        if (insertIdx < 0)
            insertIdx = FindFirstLineIndex(moduleBicep, "resource ");
        if (insertIdx < 0)
            return moduleBicep;

        moduleBicep = InsertAt(moduleBicep, insertIdx, paramDecl + "\n");

        // Wire into siteConfig: if appSettings already exists in siteConfig, concat; otherwise add
        var appSettingsMatch = AppSettingsArrayPattern.Match(moduleBicep);
        if (appSettingsMatch.Success)
        {
            // FunctionApp case: has existing appSettings array — wrap with concat
            var appSettingsStart = appSettingsMatch.Index;

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
        else
        {
            // WebApp case: no existing appSettings in siteConfig — add it (whitespace-tolerant)
            var siteConfigMatch = SiteConfigPattern.Match(moduleBicep);
            if (siteConfigMatch.Success)
            {
                var braceIdx = moduleBicep.IndexOf('{', siteConfigMatch.Index);
                if (braceIdx >= 0)
                {
                    moduleBicep = InsertAt(moduleBicep, braceIdx + 1, "\n      appSettings: appSettings");
                }
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
        if (ParamEnvVarsPattern.IsMatch(moduleBicep))
            return moduleBicep;

        var paramDecl = "\n@description('Environment variables for the container')\nparam envVars array = []\n";

        var insertIdx = FindFirstLineIndex(moduleBicep, "resource ");
        if (insertIdx < 0)
            return moduleBicep;

        moduleBicep = InsertAt(moduleBicep, insertIdx, paramDecl + "\n");

        // Add env property to the container spec if not already present (whitespace-tolerant)
        if (!EnvPropertyPattern.IsMatch(moduleBicep))
        {
            // Find "resources: {" in the container spec — insert env BEFORE it (at container level, not inside resources)
            var resourcesMatch = ContainerResourcesPattern.Match(moduleBicep);
            if (resourcesMatch.Success)
            {
                // Determine indentation from the matched line and insert env on the preceding line
                var indent = resourcesMatch.Value[..resourcesMatch.Value.IndexOf('r')];
                moduleBicep = InsertAt(moduleBicep, resourcesMatch.Index, $"{indent}env: envVars\n");
            }
        }

        return moduleBicep;
    }

    /// <summary>
    /// Finds the byte index of the first line in the template that starts with the given prefix
    /// (after trimming whitespace). Uses regex for whitespace-tolerant matching.
    /// Returns -1 if not found.
    /// </summary>
    private static int FindFirstLineIndex(string content, string linePrefix)
    {
        var match = Regex.Match(content, $@"^[ \t]*{Regex.Escape(linePrefix)}", RegexOptions.Multiline);
        return match.Success ? match.Index : -1;
    }

    /// <summary>
    /// Inserts <paramref name="content"/> into <paramref name="source"/> at the specified position
    /// using <see cref="StringBuilder"/> to avoid O(n) string allocation per insert.
    /// </summary>
    private static string InsertAt(string source, int position, string content)
    {
        var sb = new StringBuilder(source.Length + content.Length);
        sb.Append(source, 0, position);
        sb.Append(content);
        sb.Append(source, position, source.Length - position);
        return sb.ToString();
    }

    /// <summary>
    /// Finds the position of the closing brace that matches the opening brace context.
    /// Starts scanning from <paramref name="startIndex"/> with an initial depth of 1.
    /// Returns -1 if no matching brace is found.
    /// </summary>
    private static int FindClosingBrace(string source, int startIndex)
    {
        var depth = 1;
        for (var i = startIndex; i < source.Length; i++)
        {
            if (source[i] == '{') depth++;
            else if (source[i] == '}')
            {
                depth--;
                if (depth == 0) return i;
            }
        }
        return -1;
    }

    /// <summary>
    /// Returns true if a param with the given name exists in the Bicep template.
    /// Uses whitespace-tolerant regex matching at line boundaries.
    /// </summary>
    private static bool HasParam(string bicep, string paramName) =>
        Regex.IsMatch(bicep, $@"^param\s+{Regex.Escape(paramName)}\b", RegexOptions.Multiline);

    /// <summary>
    /// Returns true if an output with the given name exists in the Bicep template.
    /// Uses whitespace-tolerant regex matching at line boundaries.
    /// </summary>
    private static bool HasOutput(string bicep, string outputName) =>
        Regex.IsMatch(bicep, $@"^output\s+{Regex.Escape(outputName)}\b", RegexOptions.Multiline);

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
