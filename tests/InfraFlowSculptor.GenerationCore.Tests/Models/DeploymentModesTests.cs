using FluentAssertions;
using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.GenerationCore.Tests.Models;

public sealed class DeploymentModesTests
{
    [Fact]
    public void Code_ShouldBeCode()
    {
        DeploymentModes.Code.Should().Be("Code");
    }

    [Fact]
    public void Container_ShouldBeContainer()
    {
        DeploymentModes.Container.Should().Be("Container");
    }

    [Fact]
    public void All_ShouldContainCode()
    {
        DeploymentModes.All.Should().Contain("Code");
    }

    [Fact]
    public void All_ShouldContainContainer()
    {
        DeploymentModes.All.Should().Contain("Container");
    }

    [Fact]
    public void All_ShouldHaveCount2()
    {
        DeploymentModes.All.Should().HaveCount(2);
    }

    [Theory]
    [InlineData("code")]
    [InlineData("CODE")]
    [InlineData("Code")]
    [InlineData("container")]
    [InlineData("CONTAINER")]
    [InlineData("Container")]
    public void All_ShouldBeCaseInsensitive(string value)
    {
        DeploymentModes.All.Contains(value).Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("Unknown")]
    [InlineData("Hybrid")]
    public void All_GivenInvalidMode_ShouldNotContain(string value)
    {
        DeploymentModes.All.Contains(value).Should().BeFalse();
    }
}
