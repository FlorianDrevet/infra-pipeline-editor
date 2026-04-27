using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.PipelineGeneration.Tests.GenerationCore;

public sealed class AcrAuthModesTests
{
    [Fact]
    public void Given_AcrAuthModes_When_All_Then_ContainsAllExpectedValues()
    {
        // Arrange / Act
        var all = AcrAuthModes.All;

        // Assert
        all.Should().HaveCount(2);
        all.Should().Contain(AcrAuthModes.ManagedIdentity);
        all.Should().Contain(AcrAuthModes.AdminCredentials);
    }

    [Theory]
    [InlineData("managedidentity")]
    [InlineData("MANAGEDIDENTITY")]
    [InlineData("admincredentials")]
    [InlineData("AdminCredentials")]
    public void Given_KnownLowercaseValue_When_AllContains_Then_ReturnsTrue(string value)
    {
        // Act
        var contains = AcrAuthModes.All.Contains(value);

        // Assert
        contains.Should().BeTrue();
    }

    [Theory]
    [InlineData("ServiceConnection")]
    [InlineData("Anonymous")]
    [InlineData("")]
    public void Given_UnknownValue_When_AllContains_Then_ReturnsFalse(string value)
    {
        // Act
        var contains = AcrAuthModes.All.Contains(value);

        // Assert
        contains.Should().BeFalse();
    }
}
