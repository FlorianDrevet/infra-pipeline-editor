using InfraFlowSculptor.PipelineGeneration.Models;
using static InfraFlowSculptor.PipelineGeneration.Bootstrap.BootstrapYamlHelpers;

namespace InfraFlowSculptor.PipelineGeneration.Bootstrap.Stages;

/// <summary>
/// Emits the <c>ValidateServiceConnections</c> job that verifies all required Azure DevOps
/// service connections exist before pipelines are created.
/// Runs when <see cref="BootstrapGenerationRequest.ServiceConnections"/> is non-empty.
/// </summary>
public sealed class ServiceConnectionValidationJobStage : IBootstrapPipelineStage
{
    /// <inheritdoc />
    public int Order => 250;

    /// <inheritdoc />
    public void Execute(BootstrapPipelineContext context)
    {
        var request = context.Request;

        if (request.ServiceConnections.Count == 0)
            return;

        var sb = context.Builder;

        AppendJobHeader(sb, "ValidateServiceConnections", "Validate Service Connections", request.AgentPoolName, dependsOn: null);
        GenerateValidationSteps(sb, request);

        context.HasProvisioningJob = true;
    }

    private static void GenerateValidationSteps(System.Text.StringBuilder sb, BootstrapGenerationRequest request)
    {
        sb.AppendLine("  # ── Validate that required service connections exist ─────────────────────");
        sb.AppendLine($"{StepIndent}- powershell: |");
        sb.AppendLine($"{StepBodyIndent}$ErrorActionPreference = 'Stop'");
        sb.AppendLine($"{StepBodyIndent}$missing = @()");
        sb.AppendLine($"{StepBodyIndent}$encodedProjectName = [Uri]::EscapeDataString(\"$(projectName)\")");
        sb.AppendLine($"{StepBodyIndent}$headers = @{{ Authorization = \"Bearer $(System.AccessToken)\" }}");

        var distinctConnections = request.ServiceConnections
            .DistinctBy(sc => sc.Name, StringComparer.OrdinalIgnoreCase)
            .OrderBy(sc => sc.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        foreach (var sc in distinctConnections)
        {
            var sanitizedName = EscapeSingleQuotes(sc.Name);
            var encodedName = $"[Uri]::EscapeDataString('{sanitizedName}')";

            sb.AppendLine($"{StepBodyIndent}$scName = '{sanitizedName}'");
            sb.AppendLine($"{StepBodyIndent}$scUri = \"$(organizationUrl)/$encodedProjectName/_apis/serviceendpoint/endpoints?endpointNames=$({encodedName})&api-version=7.1\"");
            sb.AppendLine($"{StepBodyIndent}$scResponse = Invoke-RestMethod -Method Get -Uri $scUri -Headers $headers -ContentType 'application/json'");
            sb.AppendLine($"{StepBodyIndent}if ($scResponse.count -eq 0) {{ $missing += '{sanitizedName} ({EscapeSingleQuotes(sc.Type)})' }}");
            sb.AppendLine($"{StepBodyIndent}else {{ Write-Host \"Service connection exists: $scName\" }}");
        }

        sb.AppendLine($"{StepBodyIndent}if ($missing.Count -gt 0) {{");
        sb.AppendLine($"{StepBodyIndent}  Write-Host '##[error]Missing service connections:'");
        sb.AppendLine($"{StepBodyIndent}  $missing | ForEach-Object {{ Write-Host \"##[error]  - $_\" }}");
        sb.AppendLine($"{StepBodyIndent}  Write-Host '##[error]Create the missing service connections in Azure DevOps > Project Settings > Service connections before re-running this bootstrap.'");
        sb.AppendLine($"{StepBodyIndent}  throw \"$($missing.Count) required service connection(s) not found in Azure DevOps project.\"");
        sb.AppendLine($"{StepBodyIndent}}}");
        sb.AppendLine($"{StepBodyIndent}Write-Host \"All $({distinctConnections.Count}) required service connections validated.\"");
        sb.AppendLine($"{StepPropertyIndent}displayName: 'Validate Service Connections'");
        sb.AppendLine($"{StepPropertyIndent}env:");
        sb.AppendLine($"{StepBodyIndent}SYSTEM_ACCESSTOKEN: $(System.AccessToken)");
        sb.AppendLine();
    }
}
