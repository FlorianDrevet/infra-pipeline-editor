using System.Text;
using InfraFlowSculptor.BicepGeneration.Helpers;
using InfraFlowSculptor.GenerationCore;

namespace InfraFlowSculptor.BicepGeneration.Assemblers;

/// <summary>
/// Generates <c>functions.bicep</c> with exported naming functions
/// derived from the project's naming templates.
/// </summary>
internal static class FunctionsBicepAssembler
{
    /// <summary>
    /// Generates the <c>functions.bicep</c> content from the given naming context.
    /// </summary>
    internal static string Generate(NamingContext namingContext)
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
}
