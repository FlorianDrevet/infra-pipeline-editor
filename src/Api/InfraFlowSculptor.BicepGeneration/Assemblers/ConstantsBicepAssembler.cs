using System.Text;
using InfraFlowSculptor.BicepGeneration.Helpers;
using InfraFlowSculptor.GenerationCore;

namespace InfraFlowSculptor.BicepGeneration.Assemblers;

/// <summary>
/// Generates <c>constants.bicep</c> with exported RBAC role definitions
/// grouped by Azure service.
/// </summary>
internal static class ConstantsBicepAssembler
{
    /// <summary>
    /// Generates the <c>constants.bicep</c> content for the given role assignments.
    /// </summary>
    internal static string Generate(IReadOnlyList<RoleAssignmentDefinition> roleAssignments)
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
                sb.AppendLine($"      description: '{BicepFormattingHelper.EscapeBicepString(role.RoleDefinitionDescription)}'");
                sb.AppendLine("    }");
            }

            sb.AppendLine("  }");
        }

        sb.AppendLine("}");

        return sb.ToString();
    }
}
