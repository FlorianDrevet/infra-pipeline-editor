using FluentAssertions;
using InfraFlowSculptor.Contracts.Projects.Requests;
using InfraFlowSculptor.Contracts.Tests.TestSupport;

namespace InfraFlowSculptor.Contracts.Tests.Projects.Requests;

public sealed class ValidateRecentItemsRequestTests
{
    [Fact]
    public void Given_ValidRequest_When_Validate_Then_NoError()
    {
        // Arrange
        var sut = new ValidateRecentItemsRequest
        {
            Items =
            [
                new RecentItemEntry { Id = Guid.NewGuid().ToString(), Type = "project" },
            ],
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void Given_NullItems_When_Validate_Then_ReturnsError()
    {
        // Arrange
        var sut = new ValidateRecentItemsRequest
        {
            Items = null!,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(ValidateRecentItemsRequest.Items)).Should().BeTrue();
    }
}
