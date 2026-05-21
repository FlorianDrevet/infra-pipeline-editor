using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces.Services;

namespace InfraFlowSculptor.Infrastructure.Services.PipelineDetection;

/// <summary>
/// Analyzes Git repository content to auto-detect test frameworks, linting tools,
/// and other CI/CD pipeline options based on the runtime stack.
/// </summary>
public sealed class PipelineOptionDetectionService(IGitProviderService gitProvider)
    : IPipelineOptionDetectionService
{
    /// <inheritdoc />
    public async Task<ErrorOr<DetectedPipelineOptionsResult>> DetectAsync(
        string token,
        string owner,
        string repositoryName,
        string branch,
        string runtimeStack,
        string? sourceCodePath,
        CancellationToken cancellationToken = default)
    {
        // Search for relevant files in the source code path
        var filesResult = await gitProvider.SearchFilesAsync(
            token, owner, repositoryName, branch, null, cancellationToken);
        if (filesResult.IsError)
            return filesResult.Errors;

        var allFiles = filesResult.Value;
        var prefix = NormalizePrefix(sourceCodePath);

        // Filter files within the source code path scope
        var scopedFiles = allFiles
            .Where(f => string.IsNullOrEmpty(prefix) || f.Path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return runtimeStack switch
        {
            "DotNet" => await DetectDotNetAsync(token, owner, repositoryName, branch, prefix, scopedFiles, cancellationToken),
            "Node" or "NodeJs" or "Angular" => await DetectNodeAsync(token, owner, repositoryName, branch, prefix, scopedFiles, cancellationToken),
            "Python" => await DetectPythonAsync(token, owner, repositoryName, branch, prefix, scopedFiles, cancellationToken),
            "Java" => await DetectJavaAsync(token, owner, repositoryName, branch, prefix, scopedFiles, cancellationToken),
            _ => new DetectedPipelineOptionsResult(),
        };
    }

    private async Task<DetectedPipelineOptionsResult> DetectDotNetAsync(
        string token, string owner, string repositoryName, string branch,
        string prefix, IReadOnlyList<Application.Projects.Common.GitFileResult> files,
        CancellationToken cancellationToken)
    {
        string? testFramework = null;
        var lintingAvailable = false;
        string? suggestedLintCommand = null;
        var sonarDetected = false;
        string? sonarProjectKey = null;

        // Look for test projects (.Tests.csproj)
        var testProjects = files
            .Where(f => f.Name.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase)
                        && f.Name.Contains("Test", StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (testProjects.Count > 0)
        {
            // Read first test project to detect framework
            var firstTestProject = testProjects[0];
            var contentResult = await gitProvider.GetFileContentAsync(
                token, owner, repositoryName, branch, firstTestProject.Path, cancellationToken);

            if (contentResult is { IsError: false, Value: not null })
            {
                var content = contentResult.Value;
                testFramework = DetectDotNetTestFramework(content);
            }
        }

        // Check for .editorconfig or dotnet-format availability
        var hasEditorConfig = files.Any(f =>
            f.Name.Equals(".editorconfig", StringComparison.OrdinalIgnoreCase));
        if (hasEditorConfig)
        {
            lintingAvailable = true;
            suggestedLintCommand = "dotnet format --verify-no-changes --verbosity diagnostic";
        }

        // Check for sonar-project.properties
        var sonarFile = files.FirstOrDefault(f =>
            f.Name.Equals("sonar-project.properties", StringComparison.OrdinalIgnoreCase));
        if (sonarFile is not null)
        {
            sonarDetected = true;
            var sonarContent = await gitProvider.GetFileContentAsync(
                token, owner, repositoryName, branch, sonarFile.Path, cancellationToken);
            if (sonarContent is { IsError: false, Value: not null })
            {
                sonarProjectKey = ExtractSonarProjectKey(sonarContent.Value);
            }
        }

        return new DetectedPipelineOptionsResult
        {
            TestFramework = testFramework,
            SuggestedTestCommand = testFramework is not null
                ? "dotnet test --logger \"trx;LogFileName=results.trx\" --collect:\"XPlat Code Coverage\" --results-directory $(Agent.TempDirectory)/TestResults"
                : null,
            SuggestedTestResultsFormat = testFramework is not null ? "VSTest" : null,
            SuggestedCoverageTool = testFramework is not null ? "Cobertura" : null,
            SuggestedCoverageReportPath = testFramework is not null
                ? "$(Agent.TempDirectory)/TestResults/**/coverage.cobertura.xml"
                : null,
            LintingAvailable = lintingAvailable,
            SuggestedLintCommand = suggestedLintCommand,
            SonarConfigDetected = sonarDetected,
            SuggestedSonarProjectKey = sonarProjectKey,
            DependencyScanAvailable = true,
            SuggestedDependencyScanTool = "OWASPDependencyCheck",
        };
    }

    private async Task<DetectedPipelineOptionsResult> DetectNodeAsync(
        string token, string owner, string repositoryName, string branch,
        string prefix, IReadOnlyList<Application.Projects.Common.GitFileResult> files,
        CancellationToken cancellationToken)
    {
        string? testFramework = null;
        string? suggestedTestCommand = null;
        var lintingAvailable = false;
        string? suggestedLintCommand = null;
        var sonarDetected = false;
        string? sonarProjectKey = null;

        // Find package.json
        var packageJsonFile = files.FirstOrDefault(f =>
            f.Name.Equals("package.json", StringComparison.OrdinalIgnoreCase)
            && (string.IsNullOrEmpty(prefix) || f.Path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)));

        if (packageJsonFile is not null)
        {
            var contentResult = await gitProvider.GetFileContentAsync(
                token, owner, repositoryName, branch, packageJsonFile.Path, cancellationToken);

            if (contentResult is { IsError: false, Value: not null })
            {
                var content = contentResult.Value;

                // Detect test framework
                if (content.Contains("\"vitest\"", StringComparison.OrdinalIgnoreCase))
                {
                    testFramework = "vitest";
                    suggestedTestCommand = "npx vitest run --reporter=junit --outputFile=results.xml --coverage";
                }
                else if (content.Contains("\"jest\"", StringComparison.OrdinalIgnoreCase))
                {
                    testFramework = "jest";
                    suggestedTestCommand = "npx jest --ci --reporters=default --reporters=jest-junit --coverage --coverageReporters=cobertura";
                }
                else if (content.Contains("\"mocha\"", StringComparison.OrdinalIgnoreCase))
                {
                    testFramework = "mocha";
                    suggestedTestCommand = "npx mocha --reporter mocha-junit-reporter";
                }

                // Detect linting
                if (content.Contains("\"eslint\"", StringComparison.OrdinalIgnoreCase))
                {
                    lintingAvailable = true;
                    suggestedLintCommand = "npx eslint . --max-warnings 0";
                }
                else if (content.Contains("\"prettier\"", StringComparison.OrdinalIgnoreCase))
                {
                    lintingAvailable = true;
                    suggestedLintCommand = "npx prettier --check .";
                }
            }
        }

        // Check for sonar-project.properties
        var sonarFile = files.FirstOrDefault(f =>
            f.Name.Equals("sonar-project.properties", StringComparison.OrdinalIgnoreCase));
        if (sonarFile is not null)
        {
            sonarDetected = true;
            var sonarContent = await gitProvider.GetFileContentAsync(
                token, owner, repositoryName, branch, sonarFile.Path, cancellationToken);
            if (sonarContent is { IsError: false, Value: not null })
            {
                sonarProjectKey = ExtractSonarProjectKey(sonarContent.Value);
            }
        }

        return new DetectedPipelineOptionsResult
        {
            TestFramework = testFramework,
            SuggestedTestCommand = suggestedTestCommand,
            SuggestedTestResultsFormat = testFramework is not null ? "JUnit" : null,
            SuggestedCoverageTool = testFramework is not null ? "Cobertura" : null,
            SuggestedCoverageReportPath = testFramework is not null ? "coverage/cobertura-coverage.xml" : null,
            LintingAvailable = lintingAvailable,
            SuggestedLintCommand = suggestedLintCommand,
            SonarConfigDetected = sonarDetected,
            SuggestedSonarProjectKey = sonarProjectKey,
            DependencyScanAvailable = true,
            SuggestedDependencyScanTool = "NpmAudit",
        };
    }

    private async Task<DetectedPipelineOptionsResult> DetectPythonAsync(
        string token, string owner, string repositoryName, string branch,
        string prefix, IReadOnlyList<Application.Projects.Common.GitFileResult> files,
        CancellationToken cancellationToken)
    {
        string? testFramework = null;
        var lintingAvailable = false;
        string? suggestedLintCommand = null;

        // Check requirements.txt or pyproject.toml for pytest
        var requirementsFile = files.FirstOrDefault(f =>
            f.Name.Equals("requirements.txt", StringComparison.OrdinalIgnoreCase)
            || f.Name.Equals("pyproject.toml", StringComparison.OrdinalIgnoreCase));

        if (requirementsFile is not null)
        {
            var contentResult = await gitProvider.GetFileContentAsync(
                token, owner, repositoryName, branch, requirementsFile.Path, cancellationToken);

            if (contentResult is { IsError: false, Value: not null })
            {
                var content = contentResult.Value;

                if (content.Contains("pytest", StringComparison.OrdinalIgnoreCase))
                {
                    testFramework = "pytest";
                }

                // Detect linting tools
                if (content.Contains("ruff", StringComparison.OrdinalIgnoreCase))
                {
                    lintingAvailable = true;
                    suggestedLintCommand = "ruff check .";
                }
                else if (content.Contains("flake8", StringComparison.OrdinalIgnoreCase))
                {
                    lintingAvailable = true;
                    suggestedLintCommand = "flake8 .";
                }
                else if (content.Contains("pylint", StringComparison.OrdinalIgnoreCase))
                {
                    lintingAvailable = true;
                    suggestedLintCommand = "pylint src/";
                }
            }
        }

        return new DetectedPipelineOptionsResult
        {
            TestFramework = testFramework,
            SuggestedTestCommand = testFramework is not null
                ? "python -m pytest --junitxml=results.xml --cov --cov-report=xml"
                : null,
            SuggestedTestResultsFormat = testFramework is not null ? "JUnit" : null,
            SuggestedCoverageTool = testFramework is not null ? "Cobertura" : null,
            SuggestedCoverageReportPath = testFramework is not null ? "coverage.xml" : null,
            LintingAvailable = lintingAvailable,
            SuggestedLintCommand = suggestedLintCommand,
            SonarConfigDetected = false,
            DependencyScanAvailable = true,
            SuggestedDependencyScanTool = "PipAudit",
        };
    }

    private async Task<DetectedPipelineOptionsResult> DetectJavaAsync(
        string token, string owner, string repositoryName, string branch,
        string prefix, IReadOnlyList<Application.Projects.Common.GitFileResult> files,
        CancellationToken cancellationToken)
    {
        string? testFramework = null;
        string? suggestedTestCommand = null;
        var lintingAvailable = false;
        string? suggestedLintCommand = null;
        var usesGradle = false;

        // Check pom.xml or build.gradle
        var buildFile = files.FirstOrDefault(f =>
            f.Name.Equals("pom.xml", StringComparison.OrdinalIgnoreCase)
            || f.Name.Equals("build.gradle", StringComparison.OrdinalIgnoreCase)
            || f.Name.Equals("build.gradle.kts", StringComparison.OrdinalIgnoreCase));

        if (buildFile is not null)
        {
            usesGradle = buildFile.Name.StartsWith("build.gradle", StringComparison.OrdinalIgnoreCase);

            var contentResult = await gitProvider.GetFileContentAsync(
                token, owner, repositoryName, branch, buildFile.Path, cancellationToken);

            if (contentResult is { IsError: false, Value: not null })
            {
                var content = contentResult.Value;

                if (content.Contains("junit-jupiter", StringComparison.OrdinalIgnoreCase)
                    || content.Contains("junit5", StringComparison.OrdinalIgnoreCase))
                {
                    testFramework = "junit5";
                }
                else if (content.Contains("junit", StringComparison.OrdinalIgnoreCase))
                {
                    testFramework = "junit4";
                }
                else if (content.Contains("testng", StringComparison.OrdinalIgnoreCase))
                {
                    testFramework = "testng";
                }

                // Detect linting
                if (content.Contains("checkstyle", StringComparison.OrdinalIgnoreCase))
                {
                    lintingAvailable = true;
                    suggestedLintCommand = usesGradle ? "gradle checkstyleMain" : "mvn checkstyle:check";
                }
                else if (content.Contains("spotbugs", StringComparison.OrdinalIgnoreCase))
                {
                    lintingAvailable = true;
                    suggestedLintCommand = usesGradle ? "gradle spotbugsMain" : "mvn spotbugs:check";
                }
            }

            suggestedTestCommand = usesGradle ? "gradle test" : "mvn test";
        }

        return new DetectedPipelineOptionsResult
        {
            TestFramework = testFramework,
            SuggestedTestCommand = testFramework is not null ? suggestedTestCommand : null,
            SuggestedTestResultsFormat = testFramework is not null ? "JUnit" : null,
            SuggestedCoverageTool = testFramework is not null ? "JaCoCo" : null,
            SuggestedCoverageReportPath = testFramework is not null
                ? (usesGradle ? "build/reports/jacoco/test/jacocoTestReport.xml" : "target/site/jacoco/jacoco.xml")
                : null,
            LintingAvailable = lintingAvailable,
            SuggestedLintCommand = suggestedLintCommand,
            SonarConfigDetected = false,
            DependencyScanAvailable = true,
            SuggestedDependencyScanTool = "OWASPDependencyCheck",
        };
    }

    private static string? DetectDotNetTestFramework(string csprojContent)
    {
        if (csprojContent.Contains("xunit", StringComparison.OrdinalIgnoreCase))
            return "xunit";
        if (csprojContent.Contains("NUnit", StringComparison.OrdinalIgnoreCase))
            return "nunit";
        if (csprojContent.Contains("MSTest", StringComparison.OrdinalIgnoreCase))
            return "mstest";
        return null;
    }

    private static string? ExtractSonarProjectKey(string sonarProperties)
    {
        foreach (var line in sonarProperties.Split('\n'))
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith("sonar.projectKey=", StringComparison.OrdinalIgnoreCase))
            {
                return trimmed["sonar.projectKey=".Length..].Trim();
            }
        }

        return null;
    }

    private static string NormalizePrefix(string? sourceCodePath)
    {
        if (string.IsNullOrWhiteSpace(sourceCodePath))
            return string.Empty;

        var normalized = sourceCodePath.Replace('\\', '/').TrimStart('/');
        return normalized.EndsWith('/') ? normalized : normalized + "/";
    }
}
