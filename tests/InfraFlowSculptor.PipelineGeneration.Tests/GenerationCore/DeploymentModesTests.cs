using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.PipelineGeneration.Tests.GenerationCore;

public sealed class DeploymentModesTests
{
    [Fact]
    public void Given_DeploymentModes_When_All_Then_ContainsCodeAndContainer()
    {
        // Arrange / Act
        var all = DeploymentModes.All;

        // Assert
        all.Should().HaveCount(2);
        all.Should().Contain(DeploymentModes.Code);
        all.Should().Contain(DeploymentModes.Container);
    }

    [Theory]
    [InlineData("code")]
    [InlineData("CODE")]
    [InlineData("Container")]
    [InlineData("container")]
    public void Given_KnownLowercaseValue_When_AllContains_Then_ReturnsTrue(string value)
    {
        // Act
        var contains = DeploymentModes.All.Contains(value);

        // Assert
        contains.Should().BeTrue();
    }

    [Theory]
    [InlineData("Unknown")]
    [InlineData("Hybrid")]
    [InlineData("")]
    public void Given_UnknownValue_When_AllContains_Then_ReturnsFalse(string value)
    {
        // Act
        var contains = DeploymentModes.All.Contains(value);

        // Assert
        contains.Should().BeFalse();
    }
}
