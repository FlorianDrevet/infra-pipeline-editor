using System.Text;
using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.PipelineGeneration.Generators.App;

/// <summary>
/// Builds the application CI pipeline YAML.
/// </summary>
internal static class AppCiPipelineBuilder
{
    internal static string BuildContainerPipeline(AppPipelineGenerationRequest request)
    {
        var sb = new StringBuilder();
        var buildSourceEnvironment = AppPipelineBuilderCommon.GetBuildSourceEnvironment(request);
        var buildSourceEnvKey = buildSourceEnvironment?.ShortName.ToLowerInvariant();
        var imageRepository = AppPipelineBuilderCommon.ResolveImageRepository(request);
        var imageTagPattern = AppPipelineBuilderCommon.ResolveImageTagPattern(request);
        var dockerfilePath = request.DockerfilePath ?? "Dockerfile";
        var buildContext = request.SourceCodePath ?? ".";
        var defaultRegistryName = request.ContainerRegistryName ?? string.Empty;
        var defaultRegistryLoginServer = string.IsNullOrWhiteSpace(defaultRegistryName)
            ? string.Empty
            : $"{defaultRegistryName}.azurecr.io";

        AppPipelineBuilderCommon.AppendCiHeader(sb, request.ResourceName, request.ConfigName, request.AgentPoolName);

        sb.AppendLine("stages:");
        sb.AppendLine("  - stage: Build");
        sb.AppendLine("    displayName: 'Build, scan and publish image'");
        if (!string.IsNullOrWhiteSpace(buildSourceEnvKey))
        {
            sb.AppendLine("    variables:");
            AppPipelineBuilderCommon.AppendEnvironmentVariableReferences(sb, buildSourceEnvKey, request, "      ");
        }
        sb.AppendLine("    jobs:");
        sb.AppendLine("      - job: BuildAndPush");
        sb.AppendLine("        displayName: 'Build and push immutable image'");
        sb.AppendLine("        steps:");
        sb.AppendLine("          - checkout: self");
        sb.AppendLine("            fetchDepth: 0");
        sb.AppendLine();

        AppendMetadataStep(
            sb,
            request,
            imageRepository,
            imageTagPattern,
            defaultRegistryName,
            defaultRegistryLoginServer,
            includeRegistryMetadata: true);

        if (AppPipelineBuilderCommon.UsesAdminCredentials(request))
        {
            sb.AppendLine("          - bash: |");
            sb.AppendLine("              set -euo pipefail");
            sb.AppendLine("              echo \"$(containerRegistryPassword)\" | docker login \"$(containerRegistryLoginServer)\" --username \"$(containerRegistryUsername)\" --password-stdin");
            sb.AppendLine("            displayName: 'Authenticate to build ACR with admin credentials'");
            sb.AppendLine();
        }
        else
        {
            sb.AppendLine("          - task: Docker@2");
            sb.AppendLine("            displayName: 'Authenticate to build ACR'");
            sb.AppendLine("            inputs:");
            sb.AppendLine("              command: login");
            sb.AppendLine("              containerRegistry: $(containerRegistryServiceConnection)");
            sb.AppendLine();
        }

        sb.AppendLine("          - bash: |");
        sb.AppendLine("              set -euo pipefail");
        sb.AppendLine("              registryLoginServer=\"$(containerRegistryLoginServer)\"");
        sb.AppendLine("              if [ -z \"$registryLoginServer\" ] || [ \"$registryLoginServer\" = '$(containerRegistryLoginServer)' ]; then registryLoginServer=\"" + AppPipelineBuilderCommon.EscapeForDoubleQuotedBash(defaultRegistryLoginServer) + "\"; fi");
        sb.AppendLine("              docker buildx inspect ifs-builder >/dev/null 2>&1 || docker buildx create --name ifs-builder --use >/dev/null");
        sb.AppendLine("              docker buildx use ifs-builder >/dev/null");
        sb.AppendLine("              docker buildx build \\");
        sb.AppendLine("                --file \"" + AppPipelineBuilderCommon.EscapeForDoubleQuotedBash(dockerfilePath) + "\" \\");
        sb.AppendLine("                --label \"org.opencontainers.image.revision=$(Build.SourceVersion)\" \\");
        sb.AppendLine("                --label \"org.opencontainers.image.version=$(ReleaseTag)\" \\");
        sb.AppendLine("                --label \"org.opencontainers.image.source=$(Build.Repository.Uri)\" \\");
        sb.AppendLine("                --label \"org.opencontainers.image.created=$(date -u +%Y-%m-%dT%H:%M:%SZ)\" \\");
        sb.AppendLine("                --tag \"${registryLoginServer}/" + AppPipelineBuilderCommon.EscapeForDoubleQuotedBash(imageRepository) + ":$(ReleaseTag)\" \\");
        sb.AppendLine("                --tag \"${registryLoginServer}/" + AppPipelineBuilderCommon.EscapeForDoubleQuotedBash(imageRepository) + ":sha-$(ShortSha)\" \\");
        sb.AppendLine("                --push \\");
        sb.AppendLine("                \"" + AppPipelineBuilderCommon.EscapeForDoubleQuotedBash(buildContext) + "\"");
        sb.AppendLine("            displayName: 'Build and push immutable image tags'");
        sb.AppendLine();

        if (request.EnableSecurityScans)
        {
            sb.AppendLine("          - bash: |");
            sb.AppendLine("              set -euo pipefail");
            sb.AppendLine("              toolDir=\"$(Agent.TempDirectory)/supply-chain-tools\"");
            sb.AppendLine("              mkdir -p \"$toolDir\"");
            sb.AppendLine("              curl -sfL https://raw.githubusercontent.com/aquasecurity/trivy/main/contrib/install.sh | sh -s -- -b \"$toolDir\"");
            sb.AppendLine("              echo \"##vso[task.prependpath]$toolDir\"");
            sb.AppendLine("            displayName: 'Install Trivy'");
            sb.AppendLine();

            sb.AppendLine("          - bash: |");
            sb.AppendLine("              set -euo pipefail");
            sb.AppendLine("              registryLoginServer=\"$(containerRegistryLoginServer)\"");
            sb.AppendLine("              if [ -z \"$registryLoginServer\" ] || [ \"$registryLoginServer\" = '$(containerRegistryLoginServer)' ]; then registryLoginServer=\"" + AppPipelineBuilderCommon.EscapeForDoubleQuotedBash(defaultRegistryLoginServer) + "\"; fi");
            sb.AppendLine("              mkdir -p \"$(Build.ArtifactStagingDirectory)/supply-chain\"");
            sb.AppendLine("              trivy image --scanners vuln --severity HIGH,CRITICAL --ignore-unfixed --format json --output \"$(Build.ArtifactStagingDirectory)/supply-chain/trivy-report.json\" --exit-code 1 \"${registryLoginServer}/" + AppPipelineBuilderCommon.EscapeForDoubleQuotedBash(imageRepository) + ":$(ReleaseTag)\"");
            sb.AppendLine("            displayName: 'Scan image with Trivy'");
            sb.AppendLine();

            sb.AppendLine("          - bash: |");
            sb.AppendLine("              set -euo pipefail");
            sb.AppendLine("              toolDir=\"$(Agent.TempDirectory)/supply-chain-tools\"");
            sb.AppendLine("              mkdir -p \"$toolDir\"");
            sb.AppendLine("              curl -sfL https://raw.githubusercontent.com/anchore/syft/main/install.sh | sh -s -- -b \"$toolDir\"");
            sb.AppendLine("              echo \"##vso[task.prependpath]$toolDir\"");
            sb.AppendLine("              registryLoginServer=\"$(containerRegistryLoginServer)\"");
            sb.AppendLine("              if [ -z \"$registryLoginServer\" ] || [ \"$registryLoginServer\" = '$(containerRegistryLoginServer)' ]; then registryLoginServer=\"" + AppPipelineBuilderCommon.EscapeForDoubleQuotedBash(defaultRegistryLoginServer) + "\"; fi");
            sb.AppendLine("              mkdir -p \"$(Build.ArtifactStagingDirectory)/supply-chain\"");
            sb.AppendLine("              syft \"${registryLoginServer}/" + AppPipelineBuilderCommon.EscapeForDoubleQuotedBash(imageRepository) + ":$(ReleaseTag)\" -o cyclonedx-json > \"$(Build.ArtifactStagingDirectory)/supply-chain/sbom.cyclonedx.json\"");
            sb.AppendLine("            displayName: 'Generate SBOM'");
            sb.AppendLine();
        }

        sb.AppendLine("          - task: PublishPipelineArtifact@1");
        sb.AppendLine("            displayName: 'Publish app metadata'");
        sb.AppendLine("            inputs:");
        sb.AppendLine("              targetPath: '$(Build.ArtifactStagingDirectory)/app-metadata'");
        sb.AppendLine("              artifact: 'app-metadata'");
        sb.AppendLine();

        if (request.EnableSecurityScans)
        {
            sb.AppendLine("          - task: PublishPipelineArtifact@1");
            sb.AppendLine("            displayName: 'Publish supply chain reports'");
            sb.AppendLine("            inputs:");
            sb.AppendLine("              targetPath: '$(Build.ArtifactStagingDirectory)/supply-chain'");
            sb.AppendLine("              artifact: 'supply-chain'");
            sb.AppendLine();
        }

        return sb.ToString();
    }

    internal static string BuildCodePipeline(AppPipelineGenerationRequest request)
    {
        var sb = new StringBuilder();
        var imageRepository = AppPipelineBuilderCommon.ResolveImageRepository(request);
        var imageTagPattern = AppPipelineBuilderCommon.ResolveImageTagPattern(request);

        AppPipelineBuilderCommon.AppendCiHeader(sb, request.ResourceName, request.ConfigName, request.AgentPoolName);

        sb.AppendLine("stages:");
        sb.AppendLine("  - stage: Build");
        sb.AppendLine("    displayName: 'Build, test and publish package'");
        sb.AppendLine("    jobs:");
        sb.AppendLine("      - job: BuildAndPublish");
        sb.AppendLine("        displayName: 'Build application package'");
        sb.AppendLine("        steps:");
        sb.AppendLine("          - checkout: self");
        sb.AppendLine("            fetchDepth: 0");
        sb.AppendLine();

        AppendMetadataStep(
            sb,
            request,
            imageRepository,
            imageTagPattern,
            defaultRegistryName: string.Empty,
            defaultRegistryLoginServer: string.Empty,
            includeRegistryMetadata: false);

        AppPipelineBuilderCommon.AppendSdkSetupStep(sb, request.RuntimeStack, request.RuntimeVersion);
        AppPipelineBuilderCommon.AppendCodeBuildSteps(sb, request);

        sb.AppendLine("          - task: PublishPipelineArtifact@1");
        sb.AppendLine("            displayName: 'Publish app metadata'");
        sb.AppendLine("            inputs:");
        sb.AppendLine("              targetPath: '$(Build.ArtifactStagingDirectory)/app-metadata'");
        sb.AppendLine("              artifact: 'app-metadata'");
        sb.AppendLine();

        sb.AppendLine("          - task: PublishPipelineArtifact@1");
        sb.AppendLine("            displayName: 'Publish application package'");
        sb.AppendLine("            inputs:");
        sb.AppendLine("              targetPath: '$(Build.ArtifactStagingDirectory)/application-package'");
        sb.AppendLine("              artifact: 'application-package'");
        sb.AppendLine();

        return sb.ToString();
    }

    private static void AppendMetadataStep(
        StringBuilder sb,
        AppPipelineGenerationRequest request,
        string imageRepository,
        string imageTagPattern,
        string defaultRegistryName,
        string defaultRegistryLoginServer,
        bool includeRegistryMetadata)
    {
        sb.AppendLine("          - bash: |");
        sb.AppendLine("              set -euo pipefail");
        sb.AppendLine("              sourceVersion=\"$(Build.SourceVersion)\"");
        sb.AppendLine("              shortSha=$(echo \"$sourceVersion\" | cut -c1-7)");
        sb.AppendLine("              branchName=\"$(Build.SourceBranchName)\"");
        sb.AppendLine("              sanitizedBranch=$(echo \"$branchName\" | tr '/_' '-' | tr '[:upper:]' '[:lower:]')");
        sb.AppendLine("              releaseTagPattern=\"" + AppPipelineBuilderCommon.EscapeForDoubleQuotedBash(imageTagPattern) + "\"");
        sb.AppendLine("              releaseTag=${releaseTagPattern//\\{buildNumber\\}/$(Build.BuildNumber)}");
        sb.AppendLine("              releaseTag=${releaseTag//\\{shortSha\\}/$shortSha}");
        sb.AppendLine("              releaseTag=${releaseTag//\\{branch\\}/$sanitizedBranch}");
        sb.AppendLine("              releaseTag=$(echo \"$releaseTag\" | tr '[:upper:]' '[:lower:]')");

        if (includeRegistryMetadata)
        {
            sb.AppendLine("              sourceRegistryName=\"$(containerRegistryName)\"");
            sb.AppendLine("              if [ -z \"$sourceRegistryName\" ] || [ \"$sourceRegistryName\" = '$(containerRegistryName)' ]; then sourceRegistryName=\"" + AppPipelineBuilderCommon.EscapeForDoubleQuotedBash(defaultRegistryName) + "\"; fi");
            sb.AppendLine("              sourceRegistryLoginServer=\"$(containerRegistryLoginServer)\"");
            sb.AppendLine("              if [ -z \"$sourceRegistryLoginServer\" ] || [ \"$sourceRegistryLoginServer\" = '$(containerRegistryLoginServer)' ]; then sourceRegistryLoginServer=\"" + AppPipelineBuilderCommon.EscapeForDoubleQuotedBash(defaultRegistryLoginServer) + "\"; fi");
        }
        else
        {
            sb.AppendLine("              sourceRegistryName=\"\"");
            sb.AppendLine("              sourceRegistryLoginServer=\"\"");
        }

        sb.AppendLine("              mkdir -p \"$(Build.ArtifactStagingDirectory)/app-metadata\"");
        sb.AppendLine("              cat > \"$(Build.ArtifactStagingDirectory)/app-metadata/metadata.json\" <<EOF");
        sb.AppendLine("              {");
        sb.AppendLine("                \"releaseTag\": \"$releaseTag\",");
        sb.AppendLine("                \"shortSha\": \"$shortSha\",");
        sb.AppendLine("                \"imageRepository\": \"" + AppPipelineBuilderCommon.EscapeForDoubleQuotedBash(imageRepository) + "\",");
        sb.AppendLine("                \"sourceRegistryName\": \"$sourceRegistryName\",");
        sb.AppendLine("                \"sourceRegistryLoginServer\": \"$sourceRegistryLoginServer\",");
        sb.AppendLine("                \"configName\": \"" + AppPipelineBuilderCommon.EscapeForDoubleQuotedBash(request.ConfigName) + "\",");
        sb.AppendLine("                \"resourceName\": \"" + AppPipelineBuilderCommon.EscapeForDoubleQuotedBash(request.ResourceName) + "\",");
        sb.AppendLine("                \"resourceType\": \"" + AppPipelineBuilderCommon.EscapeForDoubleQuotedBash(request.ResourceType) + "\",");
        sb.AppendLine("                \"promotionStrategy\": \"" + request.PromotionStrategy + "\"");
        sb.AppendLine("              }");
        sb.AppendLine("              EOF");
        sb.AppendLine("              echo \"##vso[task.setvariable variable=ReleaseTag]$releaseTag\"");
        sb.AppendLine("              echo \"##vso[task.setvariable variable=ShortSha]$shortSha\"");
        sb.AppendLine("            displayName: 'Compute immutable release tag'");
        sb.AppendLine();
    }
}