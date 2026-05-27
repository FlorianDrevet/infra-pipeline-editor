namespace InfraFlowSculptor.PipelineGeneration;

/// <summary>
/// Validates and resolves pipeline variable group names before they are emitted into generated YAML.
/// </summary>
internal static class PipelineVariableGroupNameHelper
{
    private const string EnvironmentPlaceholder = "{env}";
    private const string AzureTemplateExpressionStart = "${{";
    private const string AzureTemplateExpressionEnd = "}}";

    /// <summary>
    /// Resolves a variable-group template to a single-quoted YAML scalar.
    /// </summary>
    /// <param name="groupNameTemplate">The raw group-name template.</param>
    /// <param name="environmentToken">The environment token or expression that replaces <c>{env}</c>.</param>
    /// <returns>The resolved variable-group name wrapped as a YAML single-quoted scalar.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the template contains unsupported Azure DevOps expressions.</exception>
    internal static string ResolveYamlScalar(string groupNameTemplate, string environmentToken)
    {
        ValidateTemplate(groupNameTemplate);

        var resolvedName = groupNameTemplate.Replace(
            EnvironmentPlaceholder,
            environmentToken,
            StringComparison.OrdinalIgnoreCase);

        return QuoteSingleQuotedYaml(resolvedName);
    }

    private static void ValidateTemplate(string groupNameTemplate)
    {
        if (string.IsNullOrWhiteSpace(groupNameTemplate))
        {
            throw new InvalidOperationException("Pipeline variable group names must not be empty.");
        }

        if (groupNameTemplate.Contains(AzureTemplateExpressionStart, StringComparison.Ordinal) ||
            groupNameTemplate.Contains(AzureTemplateExpressionEnd, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Pipeline variable group names must not contain Azure DevOps template expressions. Use '{env}' as the only supported placeholder.");
        }

        if (groupNameTemplate.Contains('\r') || groupNameTemplate.Contains('\n'))
        {
            throw new InvalidOperationException("Pipeline variable group names must be single-line values.");
        }
    }

    private static string QuoteSingleQuotedYaml(string value)
    {
        var escapedValue = value.Replace("'", "''", StringComparison.Ordinal);
        return $"'{escapedValue}'";
    }
}
