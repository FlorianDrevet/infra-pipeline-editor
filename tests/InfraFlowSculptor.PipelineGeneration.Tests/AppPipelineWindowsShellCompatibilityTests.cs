using InfraFlowSculptor.PipelineGeneration.Generators.App;

namespace InfraFlowSculptor.PipelineGeneration.Tests;

public sealed class AppPipelineWindowsShellCompatibilityTests
{
    [Fact]
    public void Given_SharedTemplates_When_GenerateAll_Then_InlineScriptStepsAvoidBashAndPwsh()
    {
        // Act
        var files = AppPipelineGenerationEngine.GenerateSharedTemplates();

        // Assert
        files[".azuredevops/steps/app-compute-release-tag.step.yml"].Should().Contain("- powershell: |").And.NotContain("- bash:").And.NotContain("- pwsh:");
        files[".azuredevops/steps/app-acr-login.step.yml"].Should().Contain("- powershell: |").And.NotContain("- bash:").And.NotContain("- pwsh:");
        files[".azuredevops/steps/app-docker-buildx-push.step.yml"].Should().Contain("- powershell: |").And.NotContain("- bash:").And.NotContain("- pwsh:");
        files[".azuredevops/steps/app-docker-buildx-validate.step.yml"].Should().Contain("- powershell: |").And.NotContain("- bash:").And.NotContain("- pwsh:");
        files[".azuredevops/steps/app-trivy-scan.step.yml"].Should().Contain("- powershell: |").And.NotContain("- bash:").And.NotContain("- pwsh:");
        files[".azuredevops/steps/app-syft-sbom.step.yml"].Should().Contain("- powershell: |").And.NotContain("- bash:").And.NotContain("- pwsh:");
        files[".azuredevops/steps/app-load-metadata.step.yml"].Should().Contain("- powershell: |").And.NotContain("- bash:").And.NotContain("- pwsh:");
        files[".azuredevops/steps/app-build-code.step.yml"].Should().Contain("- powershell: |").And.NotContain("- bash:").And.NotContain("- pwsh:");
    }

    [Fact]
    public void Given_SharedTemplates_When_GenerateAll_Then_AzureCliStepsUseWindowsPowerShellScriptType()
    {
        // Act
        var files = AppPipelineGenerationEngine.GenerateSharedTemplates();

        // Assert
        files[".azuredevops/steps/app-acr-promote.step.yml"].Should().Contain("scriptType: ps").And.NotContain("scriptType: bash");
        files[".azuredevops/steps/app-deploy-container.step.yml"].Should().Contain("scriptType: ps").And.NotContain("scriptType: bash");
    }

    [Fact]
    public void Given_SharedTemplates_When_GenerateAll_Then_PowerShellLiteralBlocksIndentTheirBodies()
    {
        // Act
        var files = AppPipelineGenerationEngine.GenerateSharedTemplates();

        // Assert
        files[".azuredevops/steps/app-docker-buildx-push.step.yml"].Should().Contain(
            """
              - powershell: |
                  $registryLoginServer = '$(containerRegistryLoginServer)'
            """);

        files[".azuredevops/steps/app-docker-buildx-validate.step.yml"].Should().Contain(
            """
              - powershell: |
                  $imageTag = '${{ parameters.imageRepository }}:pr-$(Build.BuildId)'
            """);

        files[".azuredevops/steps/app-build-code.step.yml"].Should().Contain(
            """
              - ${{ if and(eq(parameters.buildCommand, ''), or(eq(parameters.runtimeStack, 'NODE'), eq(parameters.runtimeStack, 'NODEJS'))) }}:
                - powershell: |
                    npm ci
            """);

        files[".azuredevops/steps/app-build-code.step.yml"].Should().Contain(
            """
              - ${{ if and(eq(parameters.buildCommand, ''), eq(parameters.runtimeStack, 'PYTHON')) }}:
                - powershell: |
                    python -m pip install -r requirements.txt
            """);

        files[".azuredevops/steps/app-syft-sbom.step.yml"].Should().Contain(
            """
              - powershell: |
                  $toolDirectory = '$(Agent.TempDirectory)/supply-chain-tools'
            """);

        files[".azuredevops/steps/app-syft-sbom.step.yml"].Should().Contain(
            """
                  if ($LASTEXITCODE -ne 0) {
                      throw 'Syft SBOM generation failed.'
                  }
                displayName: 'Generate SBOM'
            """);
    }
}