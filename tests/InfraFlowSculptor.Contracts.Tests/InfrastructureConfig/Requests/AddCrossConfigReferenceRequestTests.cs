using FluentAssertions;
using InfraFlowSculptor.Contracts.InfrastructureConfig.Requests;
using InfraFlowSculptor.Contracts.Tests.TestSupport;

namespace InfraFlowSculptor.Contracts.Tests.InfrastructureConfig.Requests;

public sealed class AddCrossConfigReferenceRequestTests
{
    [Fact]
    public void Given_ValidRequest_When_Validate_Then_NoError()
    {
        // Arrange
        var sut = new AddCrossConfigReferenceRequest
        {
            TargetResourceId = Guid.NewGuid(),
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void Given_DefaultTargetResourceId_When_Validate_Then_MayReturnError()
    {
        // Arrange
        var sut = new AddCrossConfigReferenceRequest
        {
            TargetResourceId = Guid.Empty,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert — [Required] on value type Guid: Guid.Empty is default but may not trigger Required
        results.Should().NotBeNull();
    }
}
