using InfraFlowSculptor.PipelineGeneration.Models;
using static InfraFlowSculptor.PipelineGeneration.Bootstrap.BootstrapYamlHelpers;

namespace InfraFlowSculptor.PipelineGeneration.Bootstrap.Stages;

/// <summary>
/// Emits the <c>ProvisionVariableGroups</c> job that creates Azure DevOps variable groups.
/// Runs only when <see cref="BootstrapMode.FullOwner"/> and variable groups are requested.
/// </summary>
public sealed class VariableGroupProvisionJobStage : IBootstrapPipelineStage
{
    /// <inheritdoc />
    public int Order => 500;

    /// <inheritdoc />
    public void Execute(BootstrapPipelineContext context)
    {
        var request = context.Request;

        if (request.Mode != BootstrapMode.FullOwner)
            return;

        if (request.VariableGroups.Count == 0)
            return;

        var sb = context.Builder;

        AppendJobHeader(sb, "ProvisionVariableGroups", "Provision Variable Groups", request.AgentPoolName, dependsOn: null);
        GenerateConfigureStep(sb);
        GenerateVariableGroupCreationSteps(sb, request);

        context.HasProvisioningJob = true;
    }

    private static void GenerateVariableGroupCreationSteps(System.Text.StringBuilder sb, BootstrapGenerationRequest request)
    {
        if (request.VariableGroups.Count == 0)
            return;

        sb.AppendLine("  # ── Create variable groups ──────────────────────────────────────────────");

        foreach (var group in request.VariableGroups)
        {
            var sanitizedGroupName = EscapeSingleQuotes(group.GroupName);

            var plainVars = group.Variables.Where(v => !v.IsSecret).ToList();
            var variableTokens = plainVars.Count > 0
                ? plainVars.Select(v => $"{v.Name}={v.Value}").ToList()
                : ["PLACEHOLDER=bootstrap"];
            var secretVars = group.Variables.Where(v => v.IsSecret).ToList();

            sb.AppendLine($"{StepIndent}- powershell: |");
            sb.AppendLine($"{StepBodyIndent}$ErrorActionPreference = 'Stop'");
            sb.AppendLine($"{StepBodyIndent}$existingId = az pipelines variable-group list --query \"[?name=='{sanitizedGroupName}'].id | [0]\" -o tsv --detect false 2>$null");
            sb.AppendLine($"{StepBodyIndent}$groupCreated = $false");
            sb.AppendLine($"{StepBodyIndent}if ([string]::IsNullOrWhiteSpace($existingId)) {{");
            sb.Append($"{StepBodyIndent}  $null = az pipelines variable-group create --name '{sanitizedGroupName}' --variables");
            foreach (var token in variableTokens)
            {
                sb.Append($" '{EscapeSingleQuotes(token)}'");
            }
            sb.AppendLine(" --detect false");
            sb.AppendLine($"{StepBodyIndent}  $vgId = az pipelines variable-group list --query \"[?name=='{sanitizedGroupName}'].id | [0]\" -o tsv --detect false");
            sb.AppendLine($"{StepBodyIndent}  if ([string]::IsNullOrWhiteSpace($vgId)) {{");
            sb.AppendLine($"{StepBodyIndent}    throw 'Unable to resolve variable group id after creation: {sanitizedGroupName}'");
            sb.AppendLine($"{StepBodyIndent}  }}");
            sb.AppendLine($"{StepBodyIndent}  $groupCreated = $true");
            sb.AppendLine($"{StepBodyIndent}}}");
            sb.AppendLine($"{StepBodyIndent}else {{");
            sb.AppendLine($"{StepBodyIndent}  $vgId = $existingId");
            sb.AppendLine($"{StepBodyIndent}}}");
            sb.AppendLine($"{StepBodyIndent}$existingVariablesJson = az pipelines variable-group variable list --group-id $vgId -o json --detect false");
            sb.AppendLine($"{StepBodyIndent}$existingVariableNames = @()");
            sb.AppendLine($"{StepBodyIndent}if (-not [string]::IsNullOrWhiteSpace($existingVariablesJson) -and $existingVariablesJson -ne '{{}}') {{");
            sb.AppendLine($"{StepBodyIndent}  $existingVariables = $existingVariablesJson | ConvertFrom-Json");
            sb.AppendLine($"{StepBodyIndent}  $existingVariableNames = @($existingVariables.PSObject.Properties.Name)");
            sb.AppendLine($"{StepBodyIndent}}}");

            foreach (var plainVar in plainVars)
            {
                var sanitizedVarName = EscapeSingleQuotes(plainVar.Name);
                var sanitizedVarValue = EscapeSingleQuotes(plainVar.Value);
                sb.AppendLine($"{StepBodyIndent}if (-not ($existingVariableNames -contains '{sanitizedVarName}')) {{");
                sb.AppendLine($"{StepBodyIndent}  $null = az pipelines variable-group variable create --group-id $vgId --name '{sanitizedVarName}' --value '{sanitizedVarValue}' --detect false");
                sb.AppendLine($"{StepBodyIndent}  $existingVariableNames += '{sanitizedVarName}'");
                sb.AppendLine($"{StepBodyIndent}}}");
            }

            foreach (var secretVar in secretVars)
            {
                var sanitizedVarName = EscapeSingleQuotes(secretVar.Name);
                sb.AppendLine($"{StepBodyIndent}if (-not ($existingVariableNames -contains '{sanitizedVarName}')) {{");
                sb.AppendLine($"{StepBodyIndent}  $null = az pipelines variable-group variable create --group-id $vgId --name '{sanitizedVarName}' --value ' ' --secret true --detect false");
                sb.AppendLine($"{StepBodyIndent}  $existingVariableNames += '{sanitizedVarName}'");
                sb.AppendLine($"{StepBodyIndent}}}");
            }

            sb.AppendLine($"{StepBodyIndent}if (($existingVariableNames -contains 'PLACEHOLDER') -and $existingVariableNames.Count -gt 1) {{");
            sb.AppendLine($"{StepBodyIndent}  $null = az pipelines variable-group variable delete --group-id $vgId --name PLACEHOLDER --yes --detect false");
            sb.AppendLine($"{StepBodyIndent}  $existingVariableNames = @($existingVariableNames | Where-Object {{ $_ -ne 'PLACEHOLDER' }})");
            sb.AppendLine($"{StepBodyIndent}}}");

            sb.AppendLine($"{StepBodyIndent}if ($groupCreated) {{");
            sb.AppendLine($"{StepBodyIndent}  Write-Host ('Created variable group: {sanitizedGroupName} (ID: ' + $vgId + ')')");
            sb.AppendLine($"{StepBodyIndent}}}");
            sb.AppendLine($"{StepBodyIndent}else {{");
            sb.AppendLine($"{StepBodyIndent}  Write-Host ('Variable group already exists: {sanitizedGroupName} (ID: ' + $vgId + ')')");
            sb.AppendLine($"{StepBodyIndent}}}");
            sb.AppendLine($"{StepBodyIndent}Write-Host ('Ensured variables for group {sanitizedGroupName}: ' + $existingVariableNames.Count)");
            sb.AppendLine($"{StepPropertyIndent}displayName: 'Create Variable Group: {sanitizedGroupName}'");
            sb.AppendLine($"{StepPropertyIndent}env:");
            sb.AppendLine($"{StepBodyIndent}AZURE_DEVOPS_EXT_PAT: $(System.AccessToken)");
            sb.AppendLine($"{StepBodyIndent}SYSTEM_ACCESSTOKEN: $(System.AccessToken)");
            sb.AppendLine();
        }
    }
}
