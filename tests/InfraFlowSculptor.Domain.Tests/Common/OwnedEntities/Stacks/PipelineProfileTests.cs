using FluentAssertions;
using InfraFlowSculptor.Domain.Common.OwnedEntities.Stacks;

namespace InfraFlowSculptor.Domain.Tests.Common.OwnedEntities.Stacks;

public sealed class PipelineProfileTests
{
    [Fact]
    public void Given_DefaultDotNetProfile_When_Created_Then_UsesXUnitWithoutCoverage()
    {
        // Act
        var profile = DotNetPipelineProfile.CreateDefault();

        // Assert
        profile.Stack.Should().Be(ApplicationStack.DotNet);
        profile.TestFramework.Should().Be(DotNetTestFramework.XUnit);
        profile.CollectCoverage.Should().BeFalse();
        profile.CustomTestProjectGlob.Should().BeNull();
    }

    [Fact]
    public void Given_DefaultNodeJsProfile_When_Created_Then_UsesNpmJestAndDefaultScripts()
    {
        // Act
        var profile = NodeJsPipelineProfile.CreateDefault();

        // Assert
        profile.Stack.Should().Be(ApplicationStack.NodeJs);
        profile.PackageManager.Should().Be(NodePackageManager.Npm);
        profile.TestFramework.Should().Be(NodeTestFramework.Jest);
        profile.RunLintScript.Should().BeFalse();
        profile.TestScriptName.Should().Be("test");
        profile.LintScriptName.Should().Be("lint");
    }

    [Fact]
    public void Given_DefaultJavaProfile_When_Created_Then_UsesMavenAndJUnit5()
    {
        // Act
        var profile = JavaPipelineProfile.CreateDefault();

        // Assert
        profile.Stack.Should().Be(ApplicationStack.Java);
        profile.BuildTool.Should().Be(JavaBuildTool.Maven);
        profile.TestFramework.Should().Be(JavaTestFramework.JUnit5);
        profile.CollectCoverage.Should().BeFalse();
    }

    [Fact]
    public void Given_DefaultPythonProfile_When_Created_Then_UsesPipAndPytest()
    {
        // Act
        var profile = PythonPipelineProfile.CreateDefault();

        // Assert
        profile.Stack.Should().Be(ApplicationStack.Python);
        profile.PackageManager.Should().Be(PythonPackageManager.Pip);
        profile.TestFramework.Should().Be(PythonTestFramework.Pytest);
        profile.CollectCoverage.Should().BeFalse();
    }

    [Theory]
    [InlineData("", "dist")]
    [InlineData("   ", "dist")]
    [InlineData("npm run build", "")]
    [InlineData("npm run build", "   ")]
    public void Given_BlankStaticSiteProfileValues_When_Create_Then_ThrowsArgumentException(
        string buildCommand,
        string outputDirectory)
    {
        // Act
        var act = () => StaticSitePipelineProfile.Create(buildCommand, outputDirectory);

        // Assert
        act.Should().Throw<ArgumentException>();
    }
}