using System.Text;
using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.PipelineGeneration.Generators.App;

/// <summary>
/// Builds the application release pipeline YAML.
/// </summary>
internal static class AppReleasePipelineBuilder
{
    internal static string BuildContainerPipeline(AppPipelineGenerationRequest request)
    {
        var sb = new StringBuilder();
        var imageRepository = AppPipelineBuilderCommon.ResolveImageRepository(request);

        AppPipelineBuilderCommon.AppendReleaseHeader(sb, request);
        sb.AppendLine("stages:");

        for (var index = 0; index < request.Environments.Count; index++)
        {
            var environment = request.Environments[index];
            var envKey = environment.ShortName.ToLowerInvariant();
            var aliasTag = $"env-{envKey}";

            sb.AppendLine($"  - stage: Deploy_{envKey}");
            sb.AppendLine($"    displayName: 'Promote and deploy to {environment.Name}'");
            sb.AppendLine("    lockBehavior: sequential");
            if (index > 0)
            {
                var previousEnvKey = request.Environments[index - 1].ShortName.ToLowerInvariant();
                sb.AppendLine($"    dependsOn: Deploy_{previousEnvKey}");
            }
            sb.AppendLine("    variables:");
            AppPipelineBuilderCommon.AppendEnvironmentVariableReferences(sb, envKey, request, "      ");
            sb.AppendLine("    jobs:");
            sb.AppendLine($"      - deployment: Deploy_{envKey}");
            sb.AppendLine($"        displayName: 'Deploy {request.ResourceName} to {environment.Name}'");
            sb.AppendLine($"        environment: {environment.Name}");
            sb.AppendLine("        strategy:");
            sb.AppendLine("          runOnce:");
            sb.AppendLine("            deploy:");
            sb.AppendLine("              steps:");
            sb.AppendLine("                - checkout: none");
            sb.AppendLine("                - download: ci");
            sb.AppendLine("                  artifact: app-metadata");
            sb.AppendLine("                  displayName: 'Download CI metadata'");
            sb.AppendLine();

            AppendLoadMetadataStep(sb);
            AppendAcrPromotionStep(sb, request, imageRepository, aliasTag);
            AppendContainerDeployStep(sb, request, imageRepository);
        }

        return sb.ToString();
    }

    internal static string BuildCodePipeline(AppPipelineGenerationRequest request)
    {
        var sb = new StringBuilder();

        AppPipelineBuilderCommon.AppendReleaseHeader(sb, request);
        sb.AppendLine("stages:");

        for (var index = 0; index < request.Environments.Count; index++)
        {
            var environment = request.Environments[index];
            var envKey = environment.ShortName.ToLowerInvariant();

            sb.AppendLine($"  - stage: Deploy_{envKey}");
            sb.AppendLine($"    displayName: 'Deploy to {environment.Name}'");
            sb.AppendLine("    lockBehavior: sequential");
            if (index > 0)
            {
                var previousEnvKey = request.Environments[index - 1].ShortName.ToLowerInvariant();
                sb.AppendLine($"    dependsOn: Deploy_{previousEnvKey}");
            }
            sb.AppendLine("    variables:");
            AppPipelineBuilderCommon.AppendEnvironmentVariableReferences(sb, envKey, request, "      ");
            sb.AppendLine("    jobs:");
            sb.AppendLine($"      - deployment: Deploy_{envKey}");
            sb.AppendLine($"        displayName: 'Deploy {request.ResourceName} to {environment.Name}'");
            sb.AppendLine($"        environment: {environment.Name}");
            sb.AppendLine("        strategy:");
            sb.AppendLine("          runOnce:");
            sb.AppendLine("            deploy:");
            sb.AppendLine("              steps:");
            sb.AppendLine("                - checkout: none");
            sb.AppendLine("                - download: ci");
            sb.AppendLine("                  artifact: app-metadata");
            sb.AppendLine("                  displayName: 'Download CI metadata'");
            sb.AppendLine("                - download: ci");
            sb.AppendLine("                  artifact: application-package");
            sb.AppendLine("                  displayName: 'Download application package'");
            sb.AppendLine();

            AppendLoadMetadataStep(sb);
            AppendCodeDeployStep(sb, request);
        }

        return sb.ToString();
    }

    private static void AppendLoadMetadataStep(StringBuilder sb)
    {
        sb.AppendLine("                - pwsh: |");
        sb.AppendLine("                    $metadataPath = '$(Pipeline.Workspace)/app-metadata/metadata.json'");
        sb.AppendLine("                    if (-not (Test-Path -LiteralPath $metadataPath)) {");
        sb.AppendLine("                        throw \"Metadata artifact not found: $metadataPath\"");
        sb.AppendLine("                    }");
        sb.AppendLine();
        sb.AppendLine("                    $metadata = Get-Content -LiteralPath $metadataPath -Raw | ConvertFrom-Json");
        sb.AppendLine("                    Write-Host \"##vso[task.setvariable variable=ReleaseTag]$($metadata.releaseTag)\"");
        sb.AppendLine("                    Write-Host \"##vso[task.setvariable variable=ImageRepository]$($metadata.imageRepository)\"");
        sb.AppendLine("                    Write-Host \"##vso[task.setvariable variable=SourceRegistryName]$($metadata.sourceRegistryName)\"");
        sb.AppendLine("                    Write-Host \"##vso[task.setvariable variable=SourceRegistryLoginServer]$($metadata.sourceRegistryLoginServer)\"");
        sb.AppendLine("                  displayName: 'Load release metadata'");
        sb.AppendLine();
    }

    private static void AppendAcrPromotionStep(
        StringBuilder sb,
        AppPipelineGenerationRequest request,
        string imageRepository,
        string aliasTag)
    {
        sb.AppendLine("                - task: AzureCLI@2");
        sb.AppendLine("                  displayName: 'Promote image in ACR'");
        sb.AppendLine("                  inputs:");
        sb.AppendLine("                    azureSubscription: $(azureResourceManagerConnection)");
        sb.AppendLine("                    scriptType: bash");
        sb.AppendLine("                    scriptLocation: inlineScript");
        sb.AppendLine("                    inlineScript: |");
        sb.AppendLine("                      set -euo pipefail");
        sb.AppendLine("                      promotionStrategy=\"" + request.PromotionStrategy + "\"");
        sb.AppendLine("                      imageRepository=\"$(ImageRepository)\"");
        sb.AppendLine("                      if [ -z \"$imageRepository\" ] || [ \"$imageRepository\" = '$(ImageRepository)' ]; then imageRepository=\"" + AppPipelineBuilderCommon.EscapeForDoubleQuotedBash(imageRepository) + "\"; fi");
        sb.AppendLine("                      if [ \"$promotionStrategy\" = \"AcrImport\" ] && [ \"$(containerRegistryLoginServer)\" != \"$(SourceRegistryLoginServer)\" ]; then");
        sb.AppendLine("                        az acr import --name \"$(containerRegistryName)\" --source \"$(SourceRegistryLoginServer)/${imageRepository}:$(ReleaseTag)\" --image \"${imageRepository}:$(ReleaseTag)\" --force");
        sb.AppendLine("                      fi");
        sb.AppendLine("                      az acr import --name \"$(containerRegistryName)\" --source \"$(containerRegistryLoginServer)/${imageRepository}:$(ReleaseTag)\" --image \"${imageRepository}:" + AppPipelineBuilderCommon.EscapeForDoubleQuotedBash(aliasTag) + "\" --force");
        sb.AppendLine();
    }

    private static void AppendContainerDeployStep(
        StringBuilder sb,
        AppPipelineGenerationRequest request,
        string imageRepository)
    {
        if (request.ResourceType == AzureResourceTypes.ContainerApp)
        {
            sb.AppendLine("                - task: AzureCLI@2");
            sb.AppendLine("                  displayName: 'Deploy image to Container App'");
            sb.AppendLine("                  inputs:");
            sb.AppendLine("                    azureSubscription: $(azureResourceManagerConnection)");
            sb.AppendLine("                    scriptType: bash");
            sb.AppendLine("                    scriptLocation: inlineScript");
            sb.AppendLine("                    inlineScript: |");
            sb.AppendLine("                      set -euo pipefail");
            sb.AppendLine("                      imageRepository=\"$(ImageRepository)\"");
            sb.AppendLine("                      if [ -z \"$imageRepository\" ] || [ \"$imageRepository\" = '$(ImageRepository)' ]; then imageRepository=\"" + AppPipelineBuilderCommon.EscapeForDoubleQuotedBash(imageRepository) + "\"; fi");
            sb.AppendLine("                      az containerapp update --name \"$(containerAppName)\" --resource-group \"$(resourceGroupName)\" --image \"$(containerRegistryLoginServer)/${imageRepository}:$(ReleaseTag)\"");
            sb.AppendLine();
            return;
        }

        sb.AppendLine("                - task: " + AppPipelineBuilderCommon.GetAppServiceTaskName(request.ResourceType));
        sb.AppendLine("                  displayName: 'Deploy container image'");
        sb.AppendLine("                  inputs:");
        sb.AppendLine("                    azureSubscription: $(azureResourceManagerConnection)");
        sb.AppendLine("                    appType: " + AppPipelineBuilderCommon.GetAppServiceContainerType(request.ResourceType));
        sb.AppendLine("                    appName: $(" + AppPipelineBuilderCommon.GetDeploymentResourceNameVariable(request.ResourceType) + ")");
        sb.AppendLine("                    containers: $(containerRegistryLoginServer)/$(ImageRepository):$(ReleaseTag)");
        sb.AppendLine();
    }

    private static void AppendCodeDeployStep(StringBuilder sb, AppPipelineGenerationRequest request)
    {
        sb.AppendLine("                - task: " + AppPipelineBuilderCommon.GetAppServiceTaskName(request.ResourceType));
        sb.AppendLine("                  displayName: 'Deploy application package'");
        sb.AppendLine("                  inputs:");
        sb.AppendLine("                    azureSubscription: $(azureResourceManagerConnection)");
        sb.AppendLine("                    appType: " + AppPipelineBuilderCommon.GetAppServiceCodeType(request.ResourceType));
        sb.AppendLine("                    appName: $(" + AppPipelineBuilderCommon.GetDeploymentResourceNameVariable(request.ResourceType) + ")");
        sb.AppendLine("                    package: $(Pipeline.Workspace)/application-package");
        sb.AppendLine();
    }
}