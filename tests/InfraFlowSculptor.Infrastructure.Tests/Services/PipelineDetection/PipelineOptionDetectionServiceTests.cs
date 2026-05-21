using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Infrastructure.Services.PipelineDetection;
using NSubstitute;
using Xunit;

namespace InfraFlowSculptor.Infrastructure.Tests.Services.PipelineDetection;

public sealed class PipelineOptionDetectionServiceTests
{
    private readonly IGitProviderService _gitProvider = Substitute.For<IGitProviderService>();
    private readonly PipelineOptionDetectionService _sut;

    public PipelineOptionDetectionServiceTests()
    {
        _sut = new PipelineOptionDetectionService(_gitProvider);
    }

    [Fact]
    public async Task Given_DotNetRepoWithXunit_When_Detect_Then_ReturnsXunitFramework()
    {
        // Arrange
        SetupSearchFiles(
            new GitFileResult("src/MyApp.Tests/MyApp.Tests.csproj", "MyApp.Tests.csproj"),
            new GitFileResult("src/MyApp/MyApp.csproj", "MyApp.csproj"),
            new GitFileResult(".editorconfig", ".editorconfig"));

        SetupFileContent("src/MyApp.Tests/MyApp.Tests.csproj",
            """
            <Project Sdk="Microsoft.NET.Sdk">
              <ItemGroup>
                <PackageReference Include="xunit" Version="2.9.0" />
                <PackageReference Include="coverlet.collector" Version="6.0.0" />
              </ItemGroup>
            </Project>
            """);

        // Act
        var result = await _sut.DetectAsync("token", "owner", "repo", "main", "DotNet", null, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.TestFramework.Should().Be("xunit");
        result.Value.SuggestedTestCommand.Should().Contain("dotnet test");
        result.Value.SuggestedTestResultsFormat.Should().Be("VSTest");
        result.Value.SuggestedCoverageTool.Should().Be("Cobertura");
        result.Value.LintingAvailable.Should().BeTrue();
        result.Value.SuggestedLintCommand.Should().Contain("dotnet format");
        result.Value.DependencyScanAvailable.Should().BeTrue();
        result.Value.SuggestedDependencyScanTool.Should().Be("OWASPDependencyCheck");
    }

    [Fact]
    public async Task Given_DotNetRepoWithNUnit_When_Detect_Then_ReturnsNunitFramework()
    {
        // Arrange
        SetupSearchFiles(
            new GitFileResult("tests/Tests.csproj", "Tests.csproj"));

        SetupFileContent("tests/Tests.csproj",
            """
            <Project Sdk="Microsoft.NET.Sdk">
              <ItemGroup>
                <PackageReference Include="NUnit" Version="4.0.0" />
              </ItemGroup>
            </Project>
            """);

        // Act
        var result = await _sut.DetectAsync("token", "owner", "repo", "main", "DotNet", null, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.TestFramework.Should().Be("nunit");
    }

    [Fact]
    public async Task Given_DotNetRepoWithSonar_When_Detect_Then_DetectsSonarConfig()
    {
        // Arrange
        SetupSearchFiles(
            new GitFileResult("sonar-project.properties", "sonar-project.properties"));

        SetupFileContent("sonar-project.properties",
            """
            sonar.projectKey=my-project
            sonar.organization=my-org
            """);

        // Act
        var result = await _sut.DetectAsync("token", "owner", "repo", "main", "DotNet", null, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.SonarConfigDetected.Should().BeTrue();
        result.Value.SuggestedSonarProjectKey.Should().Be("my-project");
    }

    [Fact]
    public async Task Given_NodeRepoWithJest_When_Detect_Then_ReturnsJestFramework()
    {
        // Arrange
        SetupSearchFiles(
            new GitFileResult("package.json", "package.json"));

        SetupFileContent("package.json",
            """
            {
              "name": "my-app",
              "devDependencies": {
                "jest": "^29.0.0",
                "eslint": "^8.0.0"
              }
            }
            """);

        // Act
        var result = await _sut.DetectAsync("token", "owner", "repo", "main", "Node", null, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.TestFramework.Should().Be("jest");
        result.Value.SuggestedTestCommand.Should().Contain("jest");
        result.Value.SuggestedTestResultsFormat.Should().Be("JUnit");
        result.Value.LintingAvailable.Should().BeTrue();
        result.Value.SuggestedLintCommand.Should().Contain("eslint");
        result.Value.SuggestedDependencyScanTool.Should().Be("NpmAudit");
    }

    [Fact]
    public async Task Given_NodeRepoWithVitest_When_Detect_Then_ReturnsVitestFramework()
    {
        // Arrange
        SetupSearchFiles(
            new GitFileResult("package.json", "package.json"));

        SetupFileContent("package.json",
            """
            {
              "name": "my-app",
              "devDependencies": {
                "vitest": "^1.0.0",
                "prettier": "^3.0.0"
              }
            }
            """);

        // Act
        var result = await _sut.DetectAsync("token", "owner", "repo", "main", "NodeJs", null, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.TestFramework.Should().Be("vitest");
        result.Value.SuggestedTestCommand.Should().Contain("vitest");
        result.Value.LintingAvailable.Should().BeTrue();
        result.Value.SuggestedLintCommand.Should().Contain("prettier");
    }

    [Fact]
    public async Task Given_PythonRepoWithPytest_When_Detect_Then_ReturnsPytestFramework()
    {
        // Arrange
        SetupSearchFiles(
            new GitFileResult("requirements.txt", "requirements.txt"));

        SetupFileContent("requirements.txt",
            """
            flask==2.0.0
            pytest==7.4.0
            ruff==0.1.0
            """);

        // Act
        var result = await _sut.DetectAsync("token", "owner", "repo", "main", "Python", null, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.TestFramework.Should().Be("pytest");
        result.Value.SuggestedTestCommand.Should().Contain("pytest");
        result.Value.SuggestedCoverageTool.Should().Be("Cobertura");
        result.Value.LintingAvailable.Should().BeTrue();
        result.Value.SuggestedLintCommand.Should().Contain("ruff");
        result.Value.SuggestedDependencyScanTool.Should().Be("PipAudit");
    }

    [Fact]
    public async Task Given_JavaRepoWithGradle_When_Detect_Then_ReturnsGradleCommands()
    {
        // Arrange
        SetupSearchFiles(
            new GitFileResult("build.gradle", "build.gradle"));

        SetupFileContent("build.gradle",
            """
            dependencies {
                testImplementation 'org.junit.jupiter:junit-jupiter:5.9.0'
                checkstyle 'com.puppycrawl.tools:checkstyle:10.0'
            }
            """);

        // Act
        var result = await _sut.DetectAsync("token", "owner", "repo", "main", "Java", null, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.TestFramework.Should().Be("junit5");
        result.Value.SuggestedTestCommand.Should().Be("gradle test");
        result.Value.SuggestedCoverageTool.Should().Be("JaCoCo");
        result.Value.LintingAvailable.Should().BeTrue();
        result.Value.SuggestedLintCommand.Should().Contain("gradle");
        result.Value.SuggestedDependencyScanTool.Should().Be("OWASPDependencyCheck");
    }

    [Fact]
    public async Task Given_JavaRepoWithMaven_When_Detect_Then_ReturnsMavenCommands()
    {
        // Arrange
        SetupSearchFiles(
            new GitFileResult("pom.xml", "pom.xml"));

        SetupFileContent("pom.xml",
            """
            <project>
              <dependencies>
                <dependency>
                  <groupId>junit</groupId>
                  <artifactId>junit</artifactId>
                  <version>4.13</version>
                </dependency>
              </dependencies>
            </project>
            """);

        // Act
        var result = await _sut.DetectAsync("token", "owner", "repo", "main", "Java", null, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.TestFramework.Should().Be("junit4");
        result.Value.SuggestedTestCommand.Should().Be("mvn test");
        result.Value.SuggestedCoverageReportPath.Should().Contain("target/site/jacoco");
    }

    [Fact]
    public async Task Given_UnknownStack_When_Detect_Then_ReturnsEmptyResult()
    {
        // Arrange
        SetupSearchFiles();

        // Act
        var result = await _sut.DetectAsync("token", "owner", "repo", "main", "Go", null, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.TestFramework.Should().BeNull();
        result.Value.LintingAvailable.Should().BeFalse();
        result.Value.SonarConfigDetected.Should().BeFalse();
    }

    [Fact]
    public async Task Given_SearchFilesFails_When_Detect_Then_ReturnsError()
    {
        // Arrange
        _gitProvider.SearchFilesAsync(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
                Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Error.Failure("Git.Unauthorized", "Invalid token"));

        // Act
        var result = await _sut.DetectAsync("token", "owner", "repo", "main", "DotNet", null, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Git.Unauthorized");
    }

    [Fact]
    public async Task Given_SourceCodePath_When_Detect_Then_FiltersFilesByPrefix()
    {
        // Arrange
        SetupSearchFiles(
            new GitFileResult("backend/src/MyApp.Tests/MyApp.Tests.csproj", "MyApp.Tests.csproj"),
            new GitFileResult("frontend/package.json", "package.json"));

        SetupFileContent("backend/src/MyApp.Tests/MyApp.Tests.csproj",
            """
            <Project Sdk="Microsoft.NET.Sdk">
              <ItemGroup>
                <PackageReference Include="MSTest" Version="3.0.0" />
              </ItemGroup>
            </Project>
            """);

        // Act
        var result = await _sut.DetectAsync("token", "owner", "repo", "main", "DotNet", "backend", CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.TestFramework.Should().Be("mstest");
    }

    // ─── Helpers ───

    private void SetupSearchFiles(params GitFileResult[] files)
    {
        _gitProvider.SearchFilesAsync(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
                Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From<IReadOnlyList<GitFileResult>>(files.ToList()));
    }

    private void SetupFileContent(string path, string content)
    {
        _gitProvider.GetFileContentAsync(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
                Arg.Any<string>(), path, Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From<string?>(content));
    }
}
