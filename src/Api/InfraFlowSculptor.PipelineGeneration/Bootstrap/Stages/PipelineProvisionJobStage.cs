using InfraFlowSculptor.PipelineGeneration.Models;
using static InfraFlowSculptor.PipelineGeneration.Bootstrap.BootstrapYamlHelpers;

namespace InfraFlowSculptor.PipelineGeneration.Bootstrap.Stages;

/// <summary>
/// Emits the <c>ProvisionPipelineDefinitions</c> job that creates Azure DevOps pipeline definitions.
/// Runs only when <see cref="BootstrapGenerationRequest.Pipelines"/> is non-empty.
/// </summary>
public sealed class PipelineProvisionJobStage : IBootstrapPipelineStage
{
    /// <inheritdoc />
    public int Order => 300;

    /// <inheritdoc />
    public void Execute(BootstrapPipelineContext context)
    {
        var request = context.Request;

        if (request.Pipelines.Count == 0)
            return;

        var sb = context.Builder;

        string? dependsOn = request.Mode == BootstrapMode.ApplicationOnly
            && (request.Environments.Count > 0 || request.VariableGroups.Count > 0)
            ? "ValidateSharedResources"
            : null;

        AppendJobHeader(sb, "ProvisionPipelineDefinitions", "Provision Pipeline Definitions", request.AgentPoolName, dependsOn);
        GenerateConfigureStep(sb);
        GeneratePipelineCreationSteps(sb, request);

        context.HasProvisioningJob = true;
    }

    private static void GeneratePipelineCreationSteps(System.Text.StringBuilder sb, BootstrapGenerationRequest request)
    {
        if (request.Pipelines.Count == 0)
            return;

        sb.AppendLine("  # ── Create pipeline definitions ─────────────────────────────────────────");

        foreach (var pipeline in request.Pipelines)
        {
            var sanitizedName = EscapeSingleQuotes(pipeline.Name);
            var sanitizedPath = EscapeSingleQuotes(pipeline.YamlPath);
            var sanitizedFolder = EscapeSingleQuotes(pipeline.Folder);

            sb.AppendLine($"{StepIndent}- powershell: |");
            sb.AppendLine($"{StepBodyIndent}$ErrorActionPreference = 'Stop'");
            sb.AppendLine($"{StepBodyIndent}$existing = az pipelines list --folder-path '{sanitizedFolder}' --query \"[?name=='{sanitizedName}'].id | [0]\" -o tsv --detect false");
            sb.AppendLine($"{StepBodyIndent}if ([string]::IsNullOrWhiteSpace($existing)) {{");
            sb.AppendLine($"{StepBodyIndent}  $previousErrorActionPreference = $ErrorActionPreference");
            sb.AppendLine($"{StepBodyIndent}  try {{");
            sb.AppendLine($"{StepBodyIndent}    $ErrorActionPreference = 'Continue'");
            sb.AppendLine($"{StepBodyIndent}    $createOutput = @(az pipelines create --name '{sanitizedName}' --repository \"$(repositoryName)\" --repository-type tfsgit --branch \"$(defaultBranch)\" --yml-path '{sanitizedPath}' --folder-path '{sanitizedFolder}' --skip-first-run true --query 'id' -o tsv --only-show-errors --detect false 2>&1)");
            sb.AppendLine($"{StepBodyIndent}    $createExitCode = $LASTEXITCODE");
            sb.AppendLine($"{StepBodyIndent}  }}");
            sb.AppendLine($"{StepBodyIndent}  finally {{");
            sb.AppendLine($"{StepBodyIndent}    $ErrorActionPreference = $previousErrorActionPreference");
            sb.AppendLine($"{StepBodyIndent}  }}");
            sb.AppendLine($"{StepBodyIndent}  $createdId = (($createOutput | ForEach-Object {{ $_.ToString().Trim() }} | Where-Object {{ $_ -match '^\\d+$' }} | Select-Object -Last 1) | Out-String).Trim()");
            sb.AppendLine($"{StepBodyIndent}  if ($createExitCode -ne 0 -or [string]::IsNullOrWhiteSpace($createdId)) {{");
            sb.AppendLine($"{StepBodyIndent}    $details = ($createOutput | Out-String).Trim()");
            sb.AppendLine($"{StepBodyIndent}    if ([string]::IsNullOrWhiteSpace($details)) {{");
            sb.AppendLine($"{StepBodyIndent}      $details = 'No output returned by Azure DevOps CLI.'");
            sb.AppendLine($"{StepBodyIndent}    }}");
            sb.AppendLine($"{StepBodyIndent}    throw 'Failed to create pipeline {sanitizedName}. Ensure the Azure DevOps identity used by System.AccessToken has the Create build pipeline permission on path {sanitizedFolder} and Use permission on the agent pool referenced by {sanitizedPath}. Azure DevOps CLI output: ' + $details");
            sb.AppendLine($"{StepBodyIndent}  }}");
            sb.AppendLine($"{StepBodyIndent}  Write-Host ('Created pipeline: {sanitizedName} (ID: ' + $createdId + ')')");
            sb.AppendLine($"{StepBodyIndent}}}");
            sb.AppendLine($"{StepBodyIndent}else {{");
            sb.AppendLine($"{StepBodyIndent}  Write-Host ('Pipeline already exists: {sanitizedName} (ID: ' + $existing + ')')");
            sb.AppendLine($"{StepBodyIndent}}}");
            sb.AppendLine($"{StepPropertyIndent}displayName: 'Create Pipeline: {sanitizedName}'");
            sb.AppendLine($"{StepPropertyIndent}env:");
            sb.AppendLine($"{StepBodyIndent}AZURE_DEVOPS_EXT_PAT: $(System.AccessToken)");
            sb.AppendLine($"{StepBodyIndent}SYSTEM_ACCESSTOKEN: $(System.AccessToken)");
            sb.AppendLine();
        }
    }
}
