using System.Text;
using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.PipelineGeneration.Generators.App;

/// <summary>
/// Emits SDK setup steps and code build steps for application CI pipelines.
/// </summary>
internal static class AppBuildStepEmitter
{
    /// <summary>
    /// Determines whether the request uses ACR admin credentials for authentication.
    /// </summary>
    /// <param name="request">The application pipeline generation request.</param>
    /// <returns><c>true</c> when <see cref="AppPipelineGenerationRequest.AcrAuthMode"/> equals
    /// <see cref="AcrAuthModes.AdminCredentials"/>; otherwise <c>false</c>.</returns>
    internal static bool UsesAdminCredentials(AppPipelineGenerationRequest request)
    {
        return string.Equals(
            request.AcrAuthMode,
            AcrAuthModes.AdminCredentials,
            StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Appends the SDK/runtime setup step matching the requested runtime stack and version.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> to append to.</param>
    /// <param name="runtimeStack">The runtime stack identifier (e.g. <c>DOTNETCORE</c>, <c>NODE</c>).</param>
    /// <param name="runtimeVersion">The runtime version (e.g. <c>8.0</c>).</param>
    internal static void AppendSdkSetupStep(StringBuilder sb, string? runtimeStack, string? runtimeVersion)
    {
        var stack = runtimeStack?.ToUpperInvariant() ?? "DOTNETCORE";
        var version = runtimeVersion ?? "8.0";

        switch (stack)
        {
            case "DOTNETCORE" or "DOTNET":
                sb.AppendLine("          - task: UseDotNet@2");
                sb.AppendLine("            displayName: 'Setup .NET SDK'");
                sb.AppendLine("            inputs:");
                sb.AppendLine("              packageType: sdk");
                sb.AppendLine($"              version: {version}.x");
                sb.AppendLine();
                break;

            case "NODE" or "NODEJS":
                sb.AppendLine("          - task: UseNode@1");
                sb.AppendLine("            displayName: 'Setup Node.js'");
                sb.AppendLine("            inputs:");
                sb.AppendLine($"              version: {version}.x");
                sb.AppendLine();
                break;

            case "PYTHON":
                sb.AppendLine("          - task: UsePythonVersion@0");
                sb.AppendLine("            displayName: 'Setup Python'");
                sb.AppendLine("            inputs:");
                sb.AppendLine($"              versionSpec: {version}");
                sb.AppendLine();
                break;

            case "JAVA":
                sb.AppendLine("          - task: JavaToolInstaller@0");
                sb.AppendLine("            displayName: 'Setup Java'");
                sb.AppendLine("            inputs:");
                sb.AppendLine($"              versionSpec: {version}");
                sb.AppendLine("              jdkArchitectureOption: x64");
                sb.AppendLine("              jdkSourceOption: PreInstalled");
                sb.AppendLine();
                break;
        }
    }

    /// <summary>
    /// Appends code build steps (test, build, publish) based on the runtime stack and request settings.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> to append to.</param>
    /// <param name="request">The application pipeline generation request.</param>
    internal static void AppendCodeBuildSteps(StringBuilder sb, AppPipelineGenerationRequest request)
    {
        var runtimeStack = request.RuntimeStack?.ToUpperInvariant() ?? "DOTNETCORE";
        var sourcePath = request.SourceCodePath ?? ".";
        var packagePath = $"$(Build.ArtifactStagingDirectory)/{AppHeaderEmitter.PackageArtifactName}";

        if (!string.IsNullOrWhiteSpace(request.TestCommand))
        {
            sb.AppendLine("          - pwsh: |");
            sb.AppendLine($"              {request.TestCommand}");
            sb.AppendLine("            displayName: 'Run application tests'");
            sb.AppendLine($"            workingDirectory: {sourcePath}");
            sb.AppendLine();
        }

        if (!string.IsNullOrWhiteSpace(request.BuildCommand))
        {
            sb.AppendLine("          - pwsh: |");
            sb.AppendLine($"              {request.BuildCommand}");
            sb.AppendLine("            displayName: 'Build and package application'");
            sb.AppendLine($"            workingDirectory: {sourcePath}");
            sb.AppendLine();
            return;
        }

        if (runtimeStack is "DOTNETCORE" or "DOTNET")
        {
            AppendDotNetCodeBuildSteps(sb, sourcePath, packagePath);
            return;
        }

        AppendGenericCodeBuildSteps(sb, runtimeStack, sourcePath, packagePath, request.TestCommand);
    }

    private static void AppendDotNetCodeBuildSteps(StringBuilder sb, string sourcePath, string packagePath)
    {
        sb.AppendLine("          - pwsh: |");
        sb.AppendLine("              dotnet restore");
        sb.AppendLine("              dotnet build --configuration Release --no-restore");
        sb.AppendLine("            displayName: 'Restore and build .NET application'");
        sb.AppendLine($"            workingDirectory: {sourcePath}");
        sb.AppendLine();

        sb.AppendLine("          - pwsh: |");
        sb.AppendLine("              dotnet test --configuration Release --no-build --logger \"trx;LogFileName=test-results.trx\" --results-directory \"$(Common.TestResultsDirectory)\" --collect \"XPlat Code Coverage\"");
        sb.AppendLine("            displayName: 'Run automated tests'");
        sb.AppendLine($"            workingDirectory: {sourcePath}");
        sb.AppendLine();

        sb.AppendLine("          - task: PublishTestResults@2");
        sb.AppendLine("            displayName: 'Publish test results'");
        sb.AppendLine("            inputs:");
        sb.AppendLine("              testResultsFormat: VSTest");
        sb.AppendLine("              testResultsFiles: '$(Common.TestResultsDirectory)/**/*.trx'");
        sb.AppendLine("              failTaskOnFailedTests: true");
        sb.AppendLine();

        sb.AppendLine("          - task: PublishCodeCoverageResults@2");
        sb.AppendLine("            displayName: 'Publish code coverage'");
        sb.AppendLine("            inputs:");
        sb.AppendLine("              codeCoverageTool: Cobertura");
        sb.AppendLine("              summaryFileLocation: '$(Common.TestResultsDirectory)/**/coverage.cobertura.xml'");
        sb.AppendLine("              failIfCoverageEmpty: false");
        sb.AppendLine();

        sb.AppendLine("          - pwsh: |");
        sb.AppendLine($"              dotnet publish --configuration Release --no-build --output \"{packagePath}\"");
        sb.AppendLine("            displayName: 'Publish application package'");
        sb.AppendLine($"            workingDirectory: {sourcePath}");
        sb.AppendLine();
    }

    private static void AppendGenericCodeBuildSteps(
        StringBuilder sb,
        string runtimeStack,
        string sourcePath,
        string packagePath,
        string? testCommand)
    {
        sb.AppendLine("          - bash: |");
        sb.AppendLine("              set -euo pipefail");

        switch (runtimeStack)
        {
            case "NODE" or "NODEJS":
                sb.AppendLine("              npm ci");
                if (!string.IsNullOrWhiteSpace(testCommand))
                {
                    sb.AppendLine($"              {testCommand}");
                }
                else
                {
                    sb.AppendLine("              npm run test --if-present");
                }
                sb.AppendLine("              npm run build");
                sb.AppendLine($"              mkdir -p \"{packagePath}\"");
                sb.AppendLine("              if [ -d dist ]; then cp -R dist/. \"$(Build.ArtifactStagingDirectory)/application-package\"; elif [ -d build ]; then cp -R build/. \"$(Build.ArtifactStagingDirectory)/application-package\"; else cp -R . \"$(Build.ArtifactStagingDirectory)/application-package\"; fi");
                break;

            case "PYTHON":
                sb.AppendLine("              python -m pip install -r requirements.txt");
                if (!string.IsNullOrWhiteSpace(testCommand))
                {
                    sb.AppendLine($"              {testCommand}");
                }
                sb.AppendLine($"              mkdir -p \"{packagePath}\"");
                sb.AppendLine("              cp -R . \"$(Build.ArtifactStagingDirectory)/application-package\"");
                break;

            default:
                sb.AppendLine($"              mkdir -p \"{packagePath}\"");
                sb.AppendLine("              cp -R . \"$(Build.ArtifactStagingDirectory)/application-package\"");
                break;
        }

        sb.AppendLine("            displayName: 'Build and package application'");
        sb.AppendLine($"            workingDirectory: {sourcePath}");
        sb.AppendLine();
    }
}
