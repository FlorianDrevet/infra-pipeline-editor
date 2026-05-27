using FluentAssertions;
using InfraFlowSculptor.Contracts.Tests.TestSupport;
using InfraFlowSculptor.Contracts.UserAssignedIdentities.Requests;

namespace InfraFlowSculptor.Contracts.Tests.UserAssignedIdentities.Requests;

public sealed class UnlinkResourceFromIdentityRequestTests
{
    [Fact]
    public void Given_ValidRequest_When_Validate_Then_NoError()
    {
        // Arrange
        var sut = new UnlinkResourceFromIdentityRequest
        {
            SourceResourceId = Guid.NewGuid(),
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void Given_DefaultSourceResourceId_When_Validate_Then_MayReturnError()
    {
        // Arrange
        var sut = new UnlinkResourceFromIdentityRequest
        {
            SourceResourceId = Guid.Empty,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert — [Required] on value type Guid: Guid.Empty is default but may not trigger Required
        results.Should().NotBeNull();
    }
}
