using InfraFlowSculptor.PipelineGeneration.Models;
using static InfraFlowSculptor.PipelineGeneration.Bootstrap.BootstrapYamlHelpers;

namespace InfraFlowSculptor.PipelineGeneration.Bootstrap.Stages;

/// <summary>
/// Emits the <c>ValidateSharedResources</c> job that checks environments and variable groups
/// already exist before the application-only bootstrap creates pipeline definitions.
/// Runs only when <see cref="BootstrapMode.ApplicationOnly"/> and shared resources are requested.
/// </summary>
public sealed class ValidateSharedResourcesJobStage : IBootstrapPipelineStage
{
    /// <inheritdoc />
    public int Order => 200;

    /// <inheritdoc />
    public void Execute(BootstrapPipelineContext context)
    {
        var request = context.Request;

        if (request.Mode != BootstrapMode.ApplicationOnly)
            return;

        if (request.Environments.Count == 0 && request.VariableGroups.Count == 0)
            return;

        var sb = context.Builder;

        AppendJobHeader(sb, "ValidateSharedResources", "Validate Shared Project Resources", request.AgentPoolName, dependsOn: null);
        GenerateConfigureStep(sb);
        GenerateValidateSharedResourcesSteps(sb, request);

        context.HasProvisioningJob = true;
    }

    private static void GenerateValidateSharedResourcesSteps(System.Text.StringBuilder sb, BootstrapGenerationRequest request)
    {
        sb.AppendLine("  # ── Validate shared project resources owned by the infra bootstrap ──────");
        sb.AppendLine($"{StepIndent}- powershell: |");
        sb.AppendLine($"{StepBodyIndent}$ErrorActionPreference = 'Stop'");
        sb.AppendLine($"{StepBodyIndent}$missing = @()");

        if (request.Environments.Count > 0)
        {
            sb.AppendLine($"{StepBodyIndent}$encodedProjectName = [Uri]::EscapeDataString(\"$(projectName)\")");
            sb.AppendLine($"{StepBodyIndent}$headers = @{{ Authorization = \"Bearer $(System.AccessToken)\" }}");
            sb.AppendLine($"{StepBodyIndent}$environmentUri = \"$(organizationUrl)/$encodedProjectName/_apis/distributedtask/environments?api-version=7.1\"");
            sb.AppendLine($"{StepBodyIndent}$environmentsResponse = Invoke-RestMethod -Method Get -Uri $environmentUri -Headers $headers -ContentType 'application/json'");
            sb.AppendLine($"{StepBodyIndent}$existingEnvironmentNames = @()");
            sb.AppendLine($"{StepBodyIndent}if ($environmentsResponse.value) {{ $existingEnvironmentNames = @($environmentsResponse.value | ForEach-Object {{ $_.name }}) }}");

            foreach (var environment in request.Environments)
            {
                var sanitizedEnvironmentName = EscapeSingleQuotes(environment.Name);
                sb.AppendLine($"{StepBodyIndent}if (-not ($existingEnvironmentNames -contains '{sanitizedEnvironmentName}')) {{ $missing += 'environment:{sanitizedEnvironmentName}' }}");
            }
        }

        if (request.VariableGroups.Count > 0)
        {
            sb.AppendLine($"{StepBodyIndent}$existingGroupsJson = az pipelines variable-group list -o json --detect false");
            sb.AppendLine($"{StepBodyIndent}$existingGroupNames = @()");
            sb.AppendLine($"{StepBodyIndent}if (-not [string]::IsNullOrWhiteSpace($existingGroupsJson)) {{");
            sb.AppendLine($"{StepBodyIndent}  $existingGroups = $existingGroupsJson | ConvertFrom-Json");
            sb.AppendLine($"{StepBodyIndent}  if ($existingGroups) {{ $existingGroupNames = @($existingGroups | ForEach-Object {{ $_.name }}) }}");
            sb.AppendLine($"{StepBodyIndent}}}");

            foreach (var group in request.VariableGroups)
            {
                var sanitizedGroupName = EscapeSingleQuotes(group.GroupName);
                sb.AppendLine($"{StepBodyIndent}if (-not ($existingGroupNames -contains '{sanitizedGroupName}')) {{ $missing += 'variableGroup:{sanitizedGroupName}' }}");
            }
        }

        sb.AppendLine($"{StepBodyIndent}if ($missing.Count -gt 0) {{");
        sb.AppendLine($"{StepBodyIndent}  Write-Host '##[error]Missing shared project resources: ' ($missing -join ', ')");
        sb.AppendLine($"{StepBodyIndent}  throw 'Shared project resources are missing. Run the infrastructure bootstrap pipeline first to provision the listed environments and variable groups.'");
        sb.AppendLine($"{StepBodyIndent}}}");
        sb.AppendLine($"{StepBodyIndent}Write-Host 'Shared project resources validated.'");
        sb.AppendLine($"{StepPropertyIndent}displayName: 'Validate shared project resources'");
        sb.AppendLine($"{StepPropertyIndent}env:");
        sb.AppendLine($"{StepBodyIndent}AZURE_DEVOPS_EXT_PAT: $(System.AccessToken)");
        sb.AppendLine($"{StepBodyIndent}SYSTEM_ACCESSTOKEN: $(System.AccessToken)");
        sb.AppendLine();
    }
}
