using InfraFlowSculptor.Domain.Common.OwnedEntities;
using InfraFlowSculptor.Domain.Common.OwnedEntities.Stacks;
using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.Application.Common.Helpers;

/// <summary>
/// Resolves stack-profile-specific default commands for pipeline generation when
/// the shared templates do not already handle the stack natively.
/// </summary>
public static class StackProfileCommandResolver
{
    /// <summary>
    /// Fills in <see cref="AppPipelineGenerationRequest"/> build/test/lint commands from
    /// the stack profile when the user has not explicitly set them and the shared templates
    /// do not have built-in defaults for the stack.
    /// </summary>
    public static void ApplyProfileDefaults(AppPipelineGenerationRequest request, AppPipelineStepOptions options)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(options);

        var profile = options.Profile;
        if (profile is null)
            return;

        switch (profile)
        {
            case AngularPipelineProfile angular:
                ApplyAngularDefaults(request, angular);
                break;
            case JavaPipelineProfile java:
                ApplyJavaDefaults(request, java);
                break;
            case PythonPipelineProfile python:
                ApplyPythonDefaults(request, python);
                break;
            case StaticSitePipelineProfile staticSite:
                ApplyStaticSiteDefaults(request, staticSite);
                break;
            case NodeJsPipelineProfile node:
                ApplyNodeJsDefaults(request, node);
                break;
            case CustomPipelineProfile custom:
                ApplyCustomDefaults(request, custom);
                break;
            // DotNet: templates already handle all .NET defaults natively
        }
    }

    private static void ApplyAngularDefaults(AppPipelineGenerationRequest request, AngularPipelineProfile profile)
    {
        var pm = ResolveNodePackageManager(profile.PackageManager);
        var install = ResolveNodeInstallCommand(profile.PackageManager);

        if (string.IsNullOrWhiteSpace(request.BuildCommand))
        {
            var buildPart = profile.RunNgBuildProduction
                ? "npx ng build --configuration production"
                : "npx ng build";

            var projectSuffix = !string.IsNullOrWhiteSpace(profile.ProjectName)
                ? $" --project {profile.ProjectName}"
                : string.Empty;

            request.BuildCommand = $"{install}\n{buildPart}{projectSuffix}";
        }

        if (string.IsNullOrWhiteSpace(request.TestCommand) && request.RunUnitTests && profile.RunNgTest)
        {
            var projectSuffix = !string.IsNullOrWhiteSpace(profile.ProjectName)
                ? $" --project {profile.ProjectName}"
                : string.Empty;

            request.TestCommand = $"npx ng test --no-watch --code-coverage --browsers=ChromeHeadless{projectSuffix}";
        }

        if (string.IsNullOrWhiteSpace(request.LintCommand) && request.RunLinting && profile.RunNgLint)
        {
            var projectSuffix = !string.IsNullOrWhiteSpace(profile.ProjectName)
                ? $" --project {profile.ProjectName}"
                : string.Empty;

            request.LintCommand = $"npx ng lint{projectSuffix}";
        }
    }

    private static void ApplyJavaDefaults(AppPipelineGenerationRequest request, JavaPipelineProfile profile)
    {
        var isGradle = profile.BuildTool.Value == JavaBuildTool.JavaBuildToolType.Gradle;

        if (string.IsNullOrWhiteSpace(request.BuildCommand))
        {
            request.BuildCommand = isGradle
                ? "gradle build -x test"
                : "mvn clean package -DskipTests";
        }

        if (string.IsNullOrWhiteSpace(request.TestCommand) && request.RunUnitTests)
        {
            request.TestCommand = isGradle
                ? "gradle test"
                : "mvn test";

            request.TestResultsFormat ??= "JUnit";
            request.CoverageTool ??= profile.CollectCoverage ? "JaCoCo" : null;
            request.CoverageReportPath ??= profile.CollectCoverage
                ? (isGradle
                    ? "build/reports/jacoco/test/jacocoTestReport.xml"
                    : "target/site/jacoco/jacoco.xml")
                : null;
        }
    }

    private static void ApplyPythonDefaults(AppPipelineGenerationRequest request, PythonPipelineProfile profile)
    {
        var installCmd = profile.PackageManager.Value switch
        {
            PythonPackageManager.PythonPackageManagerType.Poetry => "poetry install",
            PythonPackageManager.PythonPackageManagerType.Uv => "uv pip install -r requirements.txt",
            _ => "pip install -r requirements.txt",
        };

        if (string.IsNullOrWhiteSpace(request.BuildCommand))
        {
            request.BuildCommand = installCmd;
        }

        if (string.IsNullOrWhiteSpace(request.TestCommand) && request.RunUnitTests)
        {
            var isPytest = profile.TestFramework.Value == PythonTestFramework.PythonTestFrameworkType.Pytest;
            request.TestCommand = isPytest
                ? (profile.CollectCoverage
                    ? "python -m pytest --junitxml=results.xml --cov --cov-report=xml"
                    : "python -m pytest --junitxml=results.xml")
                : "python -m unittest discover -s tests";

            request.TestResultsFormat ??= "JUnit";
            request.CoverageTool ??= profile.CollectCoverage ? "Cobertura" : null;
            request.CoverageReportPath ??= profile.CollectCoverage ? "coverage.xml" : null;
        }
    }

    private static void ApplyStaticSiteDefaults(AppPipelineGenerationRequest request, StaticSitePipelineProfile profile)
    {
        if (string.IsNullOrWhiteSpace(request.BuildCommand) && !string.IsNullOrWhiteSpace(profile.BuildCommand))
        {
            request.BuildCommand = profile.BuildCommand;
        }
    }

    private static void ApplyNodeJsDefaults(AppPipelineGenerationRequest request, NodeJsPipelineProfile profile)
    {
        // Only override when using a non-npm package manager (templates default to npm ci)
        if (profile.PackageManager.Value == NodePackageManager.NodePackageManagerType.Npm)
            return;

        var install = ResolveNodeInstallCommand(profile.PackageManager);

        if (string.IsNullOrWhiteSpace(request.BuildCommand))
        {
            request.BuildCommand = $"{install}\nnpm run build";
        }

        if (string.IsNullOrWhiteSpace(request.TestCommand) && request.RunUnitTests)
        {
            var scriptName = string.IsNullOrWhiteSpace(profile.TestScriptName) ? "test" : profile.TestScriptName;
            request.TestCommand = $"{install}\nnpm run {scriptName}";
        }

        if (string.IsNullOrWhiteSpace(request.LintCommand) && request.RunLinting && profile.RunLintScript)
        {
            var scriptName = string.IsNullOrWhiteSpace(profile.LintScriptName) ? "lint" : profile.LintScriptName;
            request.LintCommand = $"npm run {scriptName}";
        }
    }

    private static void ApplyCustomDefaults(AppPipelineGenerationRequest request, CustomPipelineProfile profile)
    {
        if (string.IsNullOrWhiteSpace(request.BuildCommand) && !string.IsNullOrWhiteSpace(profile.CustomBuildCommand))
        {
            request.BuildCommand = profile.CustomBuildCommand;
        }

        if (string.IsNullOrWhiteSpace(request.TestCommand) && request.RunUnitTests
            && !string.IsNullOrWhiteSpace(profile.CustomTestCommand))
        {
            request.TestCommand = profile.CustomTestCommand;
        }

        if (string.IsNullOrWhiteSpace(request.LintCommand) && request.RunLinting
            && !string.IsNullOrWhiteSpace(profile.CustomLintCommand))
        {
            request.LintCommand = profile.CustomLintCommand;
        }
    }

    private static string ResolveNodeInstallCommand(NodePackageManager packageManager)
    {
        return packageManager.Value switch
        {
            NodePackageManager.NodePackageManagerType.Yarn => "yarn install --frozen-lockfile",
            NodePackageManager.NodePackageManagerType.Pnpm => "pnpm install --frozen-lockfile",
            _ => "npm ci",
        };
    }

    private static string ResolveNodePackageManager(NodePackageManager packageManager)
    {
        return packageManager.Value switch
        {
            NodePackageManager.NodePackageManagerType.Yarn => "yarn",
            NodePackageManager.NodePackageManagerType.Pnpm => "pnpm",
            _ => "npm",
        };
    }
}
