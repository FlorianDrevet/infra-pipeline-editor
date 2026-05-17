using FluentAssertions;
using InfraFlowSculptor.Domain.Common.OwnedEntities;

namespace InfraFlowSculptor.Domain.Tests.Common.OwnedEntities;

public sealed class AppPipelineStepOptionsTests
{
    [Fact]
    public void Given_NewValues_When_Update_Then_AssignsAllProperties()
    {
        // Arrange
        var sut = new AppPipelineStepOptions();
        var data = new AppPipelineStepOptionsData
        {
            RunUnitTests = true,
            TestCommand = "dotnet test",
            TestFramework = "xunit",
            TestResultsFormat = "VSTest",
            TestResultsPath = "**/TestResults/*.trx",
            PublishTestResults = true,
            PublishCodeCoverage = true,
            CoverageTool = "Cobertura",
            CoverageReportPath = "coverage/coverage.xml",
            RunSonarAnalysis = true,
            SonarProjectKey = "sonar-project-key",
            SonarOrganization = "sonar-organization",
            SonarServiceConnection = "sonar-service-connection",
            RunLinting = true,
            LintCommand = "npm run lint",
            RunDependencyScan = true,
            DependencyScanTool = "Snyk",
            RunBuildValidation = true,
            EnableDependencyCache = true,
            RunSmokeTests = true,
            SmokeTestCommand = "curl https://example.test/health",
        };

        // Act
        sut.Update(data);

        // Assert
        sut.RunUnitTests.Should().BeTrue();
        sut.TestCommand.Should().Be("dotnet test");
        sut.TestFramework.Should().Be("xunit");
        sut.TestResultsFormat.Should().Be("VSTest");
        sut.TestResultsPath.Should().Be("**/TestResults/*.trx");
        sut.PublishTestResults.Should().BeTrue();
        sut.PublishCodeCoverage.Should().BeTrue();
        sut.CoverageTool.Should().Be("Cobertura");
        sut.CoverageReportPath.Should().Be("coverage/coverage.xml");
        sut.RunSonarAnalysis.Should().BeTrue();
        sut.SonarProjectKey.Should().Be("sonar-project-key");
        sut.SonarOrganization.Should().Be("sonar-organization");
        sut.SonarServiceConnection.Should().Be("sonar-service-connection");
        sut.RunLinting.Should().BeTrue();
        sut.LintCommand.Should().Be("npm run lint");
        sut.RunDependencyScan.Should().BeTrue();
        sut.DependencyScanTool.Should().Be("Snyk");
        sut.RunBuildValidation.Should().BeTrue();
        sut.EnableDependencyCache.Should().BeTrue();
        sut.RunSmokeTests.Should().BeTrue();
        sut.SmokeTestCommand.Should().Be("curl https://example.test/health");
    }
}
