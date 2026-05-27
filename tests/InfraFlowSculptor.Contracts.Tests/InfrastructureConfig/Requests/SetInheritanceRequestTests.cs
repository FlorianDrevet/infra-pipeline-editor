using FluentAssertions;
using InfraFlowSculptor.Contracts.InfrastructureConfig.Requests;
using InfraFlowSculptor.Contracts.Tests.TestSupport;

namespace InfraFlowSculptor.Contracts.Tests.InfrastructureConfig.Requests;

public sealed class SetInheritanceRequestTests
{
    [Fact]
    public void Given_ValidTrue_When_Validate_Then_NoError()
    {
        // Arrange
        var sut = new SetInheritanceRequest
        {
            UseProjectNamingConventions = true,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void Given_ValidFalse_When_Validate_Then_NoError()
    {
        // Arrange
        var sut = new SetInheritanceRequest
        {
            UseProjectNamingConventions = false,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.Should().BeEmpty();
    }
}
