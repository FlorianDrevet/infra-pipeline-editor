using System.Text.RegularExpressions;
using InfraFlowSculptor.BicepGeneration.Generators;
using InfraFlowSculptor.BicepGeneration.Helpers;
using InfraFlowSculptor.BicepGeneration.Models;

namespace InfraFlowSculptor.BicepGeneration;

public sealed class BicepGenerationEngine
{
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
            request.RoleAssignments);
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
}
