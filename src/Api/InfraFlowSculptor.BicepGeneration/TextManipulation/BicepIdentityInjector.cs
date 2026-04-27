using System.Text;
using System.Text.RegularExpressions;
using static InfraFlowSculptor.BicepGeneration.TextManipulation.BicepTextManipulationHelpers;

namespace InfraFlowSculptor.BicepGeneration.TextManipulation;

/// <summary>
/// Pure text-level injectors that add managed-identity blocks (system-assigned, user-assigned,
/// or parameterized) into a generated Bicep module template.
/// </summary>
/// <remarks>
/// All operations are idempotent: invoking the same injector twice on the same module
/// yields the same result. Injectors only mutate the resource-root <c>identity:</c> block,
/// not nested <c>identity:</c> properties (e.g. Container App ACR registry entries).
/// </remarks>
public static class BicepIdentityInjector
{
    /// <summary>
    /// Bicep type definition for <c>ManagedIdentityType</c> appended to a module's <c>types.bicep</c>
    /// when the parameterized-identity injection is applied.
    /// </summary>
    public const string ManagedIdentityTypeBicepType = """

        @export()
        @description('Managed identity type for Azure resources')
        type ManagedIdentityType = 'SystemAssigned' | 'UserAssigned' | 'SystemAssigned, UserAssigned'
        """;

    /// <summary>
    /// Injects a hard-coded <c>identity: { type: 'SystemAssigned' }</c> block into the module
    /// and appends a <c>principalId</c> output. No-op when an identity block or principalId
    /// output already exists.
    /// </summary>
    public static string InjectSystemAssigned(string moduleBicep)
    {
        var symbolMatch = ResourceSymbolPattern.Match(moduleBicep);
        if (!symbolMatch.Success) return moduleBicep;

        var symbol = symbolMatch.Groups[1].Value;

        if (IdentityBlockPattern.IsMatch(moduleBicep))
            return moduleBicep;

        if (OutputPrincipalIdPattern.IsMatch(moduleBicep))
            return moduleBicep;

        const string identityBlock = "  identity: {\n    type: 'SystemAssigned'\n  }\n";

        var propertiesMatch = PropertiesBlockPattern.Match(moduleBicep);
        if (propertiesMatch.Success)
        {
            moduleBicep = InsertAt(moduleBicep, propertiesMatch.Index, identityBlock);
        }
        else
        {
            // Resources without a properties block (e.g. UserAssignedIdentity itself).
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

        var sb = new StringBuilder(moduleBicep.TrimEnd());
        sb.AppendLine();
        sb.AppendLine();
        sb.Append($"output principalId string = {symbol}.identity.principalId");
        sb.AppendLine();
        return sb.ToString();
    }

    /// <summary>
    /// Injects a <c>identity: { type: 'UserAssigned' | 'SystemAssigned, UserAssigned',
    /// userAssignedIdentities: { '${userAssignedIdentityId}': {} } }</c> block.
    /// Adds the generic <c>userAssignedIdentityId</c> param when missing. When
    /// <paramref name="alsoHasSystemAssigned"/> is <c>true</c>, upgrades an existing
    /// system-assigned block instead of creating a new one.
    /// </summary>
    public static string InjectUserAssigned(
        string moduleBicep,
        IReadOnlyList<string> uaiIdentifiers,
        bool alsoHasSystemAssigned)
    {
        _ = uaiIdentifiers; // retained for API parity with the legacy signature

        if (!HasParam(moduleBicep, "userAssignedIdentityId"))
        {
            var paramDeclarations = new StringBuilder();
            paramDeclarations.AppendLine("@description('Resource ID of user-assigned managed identity')");
            paramDeclarations.AppendLine("param userAssignedIdentityId string");
            paramDeclarations.AppendLine();

            var insertIdx = FindFirstLineIndex(moduleBicep, "var ");
            if (insertIdx < 0)
                insertIdx = FindFirstLineIndex(moduleBicep, "resource ");
            if (insertIdx >= 0)
            {
                moduleBicep = InsertAt(moduleBicep, insertIdx, paramDeclarations.ToString());
            }
        }

        var identityType = alsoHasSystemAssigned
            ? "SystemAssigned, UserAssigned"
            : "UserAssigned";

        if (IdentityBlockPattern.IsMatch(moduleBicep))
        {
            moduleBicep = moduleBicep.Replace(
                "type: 'SystemAssigned'",
                $"type: '{identityType}'\n    userAssignedIdentities: {{\n      '${{userAssignedIdentityId}}': {{}}\n    }}");
        }
        else
        {
            var identityBlock =
                $"  identity: {{\n    type: '{identityType}'\n    userAssignedIdentities: {{\n      '${{userAssignedIdentityId}}': {{}}\n    }}\n  }}\n";

            var propertiesMatch = PropertiesBlockPattern.Match(moduleBicep);
            if (propertiesMatch.Success)
            {
                moduleBicep = InsertAt(moduleBicep, propertiesMatch.Index, identityBlock);
            }
            else
            {
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
    /// Injects a parameterized identity block (<c>param identityType ManagedIdentityType</c>) and the
    /// matching <c>identity</c> property into the module. Used for ARM types that host instances
    /// requiring different identity kinds within the same template (Azure Verified Modules style).
    /// Also imports <c>ManagedIdentityType</c> from <c>./types.bicep</c>.
    /// </summary>
    public static string InjectParameterized(string moduleBicep, bool hasAnyUai)
    {
        var symbolMatch = ResourceSymbolPattern.Match(moduleBicep);
        if (!symbolMatch.Success) return moduleBicep;

        var symbol = symbolMatch.Groups[1].Value;

        if (ParamIdentityTypePattern.IsMatch(moduleBicep))
            return moduleBicep;

        if (IdentityBlockPattern.IsMatch(moduleBicep))
            return moduleBicep;

        moduleBicep = AddTypeImport(moduleBicep, "ManagedIdentityType");

        var paramSb = new StringBuilder();
        paramSb.AppendLine("@description('Managed identity type for this resource')");
        paramSb.AppendLine("param identityType ManagedIdentityType = 'SystemAssigned'");
        paramSb.AppendLine();

        if (hasAnyUai && !HasParam(moduleBicep, "userAssignedIdentityId"))
        {
            paramSb.AppendLine("@description('Resource ID of user-assigned managed identity (empty when not applicable)')");
            paramSb.AppendLine("param userAssignedIdentityId string = ''");
            paramSb.AppendLine();
        }

        var insertIdx = FindFirstLineIndex(moduleBicep, "var ");
        if (insertIdx < 0)
            insertIdx = FindFirstLineIndex(moduleBicep, "resource ");
        if (insertIdx >= 0)
        {
            moduleBicep = InsertAt(moduleBicep, insertIdx, paramSb.ToString());
        }

        string identityBlock;
        if (hasAnyUai)
        {
            identityBlock = """
              identity: {
                type: identityType
                userAssignedIdentities: contains(identityType, 'UserAssigned') && !empty(userAssignedIdentityId) ? { '${userAssignedIdentityId}': {} } : null
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

        var propertiesMatch = PropertiesBlockPattern.Match(moduleBicep);
        if (propertiesMatch.Success)
        {
            moduleBicep = InsertAt(moduleBicep, propertiesMatch.Index, identityBlock);
        }

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
}
