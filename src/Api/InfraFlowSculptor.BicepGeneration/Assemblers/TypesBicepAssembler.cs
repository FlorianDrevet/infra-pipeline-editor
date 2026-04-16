using System.Text;
using InfraFlowSculptor.BicepGeneration.Helpers;
using InfraFlowSculptor.GenerationCore;

namespace InfraFlowSculptor.BicepGeneration.Assemblers;

/// <summary>
/// Generates <c>types.bicep</c> with exported environment types, variable map, and optional RBAC types.
/// </summary>
internal static class TypesBicepAssembler
{
    /// <summary>
    /// Generates the <c>types.bicep</c> content for the given environments and optional RBAC type.
    /// </summary>
    internal static string Generate(IReadOnlyCollection<EnvironmentDefinition> environments, bool includeRbacType)
    {
        var sb = new StringBuilder();

        // ── EnvironmentName union type ──────────────────────────────────────
        if (environments.Count > 0)
        {
            sb.AppendLine("@export()");
            sb.Append("type EnvironmentName = ");
            sb.AppendJoin(" | ", environments.Select(e => $"'{BicepFormattingHelper.SanitizeBicepKey(e.Name)}'"));
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
            var key = BicepFormattingHelper.SanitizeBicepKey(env.Name);
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
                sb.AppendLine($"      {BicepFormattingHelper.FormatBicepObjectKey(tagKey)}: '{BicepFormattingHelper.EscapeBicepString(tagValue)}'");
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
}
