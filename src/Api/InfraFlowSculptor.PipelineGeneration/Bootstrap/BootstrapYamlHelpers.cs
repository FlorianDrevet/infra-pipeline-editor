using System.Text;
using InfraFlowSculptor.PipelineGeneration.Models;

namespace InfraFlowSculptor.PipelineGeneration.Bootstrap;

/// <summary>
/// Shared YAML emission helpers for Bootstrap pipeline stages.
/// All methods are pure lift-and-shift from the original <c>BootstrapPipelineGenerationEngine</c>.
/// </summary>
internal static class BootstrapYamlHelpers
{
    internal const string StepIndent = "        ";
    internal const string StepBodyIndent = "            ";
    internal const string StepPropertyIndent = "          ";

    /// <summary>
    /// Appends the standard job header including pool configuration.
    /// </summary>
    internal static void AppendJobHeader(
        StringBuilder sb,
        string jobName,
        string displayName,
        string? agentPoolName,
        string? dependsOn)
    {
        sb.AppendLine($"    - job: {jobName}");
        sb.AppendLine($"      displayName: '{displayName}'");
        if (!string.IsNullOrEmpty(dependsOn))
        {
            sb.AppendLine($"      dependsOn: {dependsOn}");
        }

        PipelineGenerationEngine.AppendPool(sb, agentPoolName, indent: "      ");
        sb.AppendLine("      steps:");
    }

    /// <summary>
    /// Appends the Azure DevOps CLI configuration step.
    /// </summary>
    internal static void GenerateConfigureStep(StringBuilder sb)
    {
        sb.AppendLine($"{StepIndent}# ── Configure Azure DevOps CLI ─────────────────────────────────────────");
        sb.AppendLine($"{StepIndent}- powershell: |");
        sb.AppendLine($"{StepBodyIndent}$ErrorActionPreference = 'Stop'");
        sb.AppendLine($"{StepBodyIndent}$null = az config set extension.use_dynamic_install=yes_without_prompt");
        sb.AppendLine($"{StepBodyIndent}$null = az config set extension.dynamic_install_allow_preview=false");
        sb.AppendLine($"{StepBodyIndent}$null = az extension show --name azure-devops 2>$null");
        sb.AppendLine($"{StepBodyIndent}if ($LASTEXITCODE -ne 0) {{");
        sb.AppendLine($"{StepBodyIndent}  $null = az extension add --name azure-devops --yes");
        sb.AppendLine($"{StepBodyIndent}}}");
        sb.AppendLine($"{StepBodyIndent}$null = az devops configure --defaults organization=\"$(organizationUrl)\" project=\"$(projectName)\"");
        sb.AppendLine($"{StepPropertyIndent}displayName: 'Configure Azure DevOps CLI'");
        sb.AppendLine($"{StepPropertyIndent}env:");
        sb.AppendLine($"{StepBodyIndent}AZURE_DEVOPS_EXT_PAT: $(System.AccessToken)");
        sb.AppendLine($"{StepBodyIndent}SYSTEM_ACCESSTOKEN: $(System.AccessToken)");
        sb.AppendLine();
    }

    /// <summary>
    /// Escapes single quotes for safe embedding in PowerShell string literals.
    /// </summary>
    internal static string EscapeSingleQuotes(string value) =>
        value.Replace("'", "''");

    /// <summary>
    /// Builds the description text for an Azure DevOps environment.
    /// </summary>
    internal static string BuildEnvironmentDescription(BootstrapEnvironmentDefinition environment)
    {
        var approvalText = environment.RequiresApproval ? "yes" : "no";
        return $"Generated from InfraFlowSculptor environment '{environment.DisplayName}'. Approval required in app: {approvalText}.";
    }
}
