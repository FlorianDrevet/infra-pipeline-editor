using InfraFlowSculptor.PipelineGeneration.Models;
using static InfraFlowSculptor.PipelineGeneration.Bootstrap.BootstrapYamlHelpers;

namespace InfraFlowSculptor.PipelineGeneration.Bootstrap.Stages;

/// <summary>
/// Emits the <c>ProvisionEnvironments</c> job that creates Azure DevOps environments.
/// Runs only when <see cref="BootstrapMode.FullOwner"/> and environments are requested.
/// </summary>
public sealed class EnvironmentProvisionJobStage : IBootstrapPipelineStage
{
    /// <inheritdoc />
    public int Order => 400;

    /// <inheritdoc />
    public void Execute(BootstrapPipelineContext context)
    {
        var request = context.Request;

        if (request.Mode != BootstrapMode.FullOwner)
            return;

        if (request.Environments.Count == 0)
            return;

        var sb = context.Builder;

        AppendJobHeader(sb, "ProvisionEnvironments", "Provision Environments", request.AgentPoolName, dependsOn: null);
        GenerateEnvironmentCreationSteps(sb, request);

        context.HasProvisioningJob = true;
    }

    private static void GenerateEnvironmentCreationSteps(System.Text.StringBuilder sb, BootstrapGenerationRequest request)
    {
        if (request.Environments.Count == 0)
            return;

        sb.AppendLine("  # ── Create Azure DevOps environments ───────────────────────────────────");

        foreach (var environment in request.Environments)
        {
            var sanitizedEnvironmentName = EscapeSingleQuotes(environment.Name);
            var sanitizedDescription = EscapeSingleQuotes(BuildEnvironmentDescription(environment));

            sb.AppendLine($"{StepIndent}- powershell: |");
            sb.AppendLine($"{StepBodyIndent}$ErrorActionPreference = 'Stop'");
            sb.AppendLine($"{StepBodyIndent}$encodedProjectName = [Uri]::EscapeDataString(\"$(projectName)\")");
            sb.AppendLine($"{StepBodyIndent}$headers = @{{ Authorization = \"Bearer $(System.AccessToken)\" }}");
            sb.AppendLine($"{StepBodyIndent}$environmentUri = \"$(organizationUrl)/$encodedProjectName/_apis/distributedtask/environments?api-version=7.1\"");
            sb.AppendLine($"{StepBodyIndent}$existingResponse = Invoke-RestMethod -Method Get -Uri $environmentUri -Headers $headers -ContentType 'application/json'");
            sb.AppendLine($"{StepBodyIndent}$existing = @($existingResponse.value | Where-Object {{ $_.name -eq '{sanitizedEnvironmentName}' }} | Select-Object -First 1)");
            sb.AppendLine($"{StepBodyIndent}if ($existing.Count -eq 0) {{");
            sb.AppendLine($"{StepBodyIndent}  $body = @{{");
            sb.AppendLine($"{StepBodyIndent}    name = '{sanitizedEnvironmentName}'");
            sb.AppendLine($"{StepBodyIndent}    description = '{sanitizedDescription}'");
            sb.AppendLine($"{StepBodyIndent}  }} | ConvertTo-Json");
            sb.AppendLine($"{StepBodyIndent}  $created = Invoke-RestMethod -Method Post -Uri $environmentUri -Headers $headers -Body $body -ContentType 'application/json'");
            sb.AppendLine($"{StepBodyIndent}  Write-Host ('Created environment: {sanitizedEnvironmentName} (ID: ' + $created.id + ')')");
            sb.AppendLine($"{StepBodyIndent}}}");
            sb.AppendLine($"{StepBodyIndent}else {{");
            sb.AppendLine($"{StepBodyIndent}  Write-Host ('Environment already exists: {sanitizedEnvironmentName} (ID: ' + $existing[0].id + ')')");
            sb.AppendLine($"{StepBodyIndent}}}");

            if (environment.RequiresApproval)
            {
                sb.AppendLine($"{StepBodyIndent}Write-Host 'InfraFlowSculptor marks this environment as approval-required. Add the Azure DevOps Approval check manually after bootstrap because approver identities are not stored in the app yet.'");
            }

            sb.AppendLine($"{StepPropertyIndent}displayName: 'Create Environment: {sanitizedEnvironmentName}'");
            sb.AppendLine($"{StepPropertyIndent}env:");
            sb.AppendLine($"{StepBodyIndent}SYSTEM_ACCESSTOKEN: $(System.AccessToken)");
            sb.AppendLine();
        }
    }
}
