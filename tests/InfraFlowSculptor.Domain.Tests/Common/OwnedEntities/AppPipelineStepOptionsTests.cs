using FluentAssertions;
using InfraFlowSculptor.Domain.Common.OwnedEntities;
using InfraFlowSculptor.Domain.Common.OwnedEntities.Stacks;

namespace InfraFlowSculptor.Domain.Tests.Common.OwnedEntities;

public sealed class AppPipelineStepOptionsTests
{
    [Fact]
    public void Given_NewOptions_When_Created_Then_UsesUnknownStackWithoutProfile()
    {
        // Arrange & Act
        var sut = new AppPipelineStepOptions();

        // Assert
        sut.Stack.Value.Should().Be(ApplicationStack.ApplicationStackEnum.Unknown);
        sut.Profile.Should().BeNull();
    }

    [Fact]
    public void Given_NewValues_When_Update_Then_AssignsAllProperties()
    {
        // Arrange
        var sut = new AppPipelineStepOptions();
        var profile = DotNetPipelineProfile.Create(
            DotNetTestFramework.XUnit,
            collectCoverage: true,
            customTestProjectGlob: "tests/**/*.csproj");

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
            Stack = ApplicationStack.DotNet,
            Profile = profile,
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
        sut.Stack.Should().Be(ApplicationStack.DotNet);
        var dotNetProfile = sut.Profile.Should().BeOfType<DotNetPipelineProfile>().Which;
        dotNetProfile.TestFramework.Should().Be(DotNetTestFramework.XUnit);
        dotNetProfile.CollectCoverage.Should().BeTrue();
        dotNetProfile.CustomTestProjectGlob.Should().Be("tests/**/*.csproj");
    }

    [Fact]
    public void Given_ProfileMatchingStack_When_Update_Then_AssignsProfile()
    {
        // Arrange
        var sut = new AppPipelineStepOptions();
        var profile = AngularPipelineProfile.CreateDefault();
        var data = new AppPipelineStepOptionsData
        {
            Stack = ApplicationStack.Angular,
            Profile = profile,
        };

        // Act
        sut.Update(data);

        // Assert
        sut.Stack.Should().Be(ApplicationStack.Angular);
        sut.Profile.Should().BeSameAs(profile);
    }

    [Fact]
    public void Given_ProfileWithDifferentStack_When_Update_Then_ThrowsArgumentException()
    {
        // Arrange
        var sut = new AppPipelineStepOptions();
        var data = new AppPipelineStepOptionsData
        {
            Stack = ApplicationStack.DotNet,
            Profile = AngularPipelineProfile.CreateDefault(),
        };

        // Act
        var act = () => sut.Update(data);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Pipeline stack profile must match the selected application stack.*");
    }

    [Fact]
    public void Given_ExistingValues_When_UpdateWithDifferentProfileStackThrows_Then_PreservesPreviousValues()
    {
        // Arrange
        var sut = new AppPipelineStepOptions();
        var originalProfile = DotNetPipelineProfile.CreateDefault();
        sut.Update(new AppPipelineStepOptionsData
        {
            RunUnitTests = true,
            TestCommand = "dotnet test",
            Stack = ApplicationStack.DotNet,
            Profile = originalProfile,
        });

        var invalidData = new AppPipelineStepOptionsData
        {
            RunUnitTests = false,
            TestCommand = "npm test",
            Stack = ApplicationStack.DotNet,
            Profile = AngularPipelineProfile.CreateDefault(),
        };

        // Act
        var act = () => sut.Update(invalidData);

        // Assert
        act.Should().Throw<ArgumentException>();
        sut.RunUnitTests.Should().BeTrue();
        sut.TestCommand.Should().Be("dotnet test");
        sut.Stack.Should().Be(ApplicationStack.DotNet);
        sut.Profile.Should().BeSameAs(originalProfile);
    }

    [Fact]
    public void Given_CustomProfile_When_Update_Then_NormalizesWhitespaceCommands()
    {
        // Arrange
        var sut = new AppPipelineStepOptions();
        var profile = CustomPipelineProfile.Create(
            customTestCommand: "   ",
            customLintCommand: " npm run lint ",
            customBuildCommand: "\t");

        var data = new AppPipelineStepOptionsData
        {
            Stack = ApplicationStack.Custom,
            Profile = profile,
        };

        // Act
        sut.Update(data);

        // Assert
        var customProfile = sut.Profile.Should().BeOfType<CustomPipelineProfile>().Which;
        customProfile.CustomTestCommand.Should().BeNull();
        customProfile.CustomLintCommand.Should().Be("npm run lint");
        customProfile.CustomBuildCommand.Should().BeNull();
    }
}
