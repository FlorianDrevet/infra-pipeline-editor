using FluentAssertions;
using InfraFlowSculptor.Contracts.InfrastructureConfig.Requests;
using InfraFlowSculptor.Contracts.Tests.TestSupport;

namespace InfraFlowSculptor.Contracts.Tests.InfrastructureConfig.Requests;

public sealed class CheckResourceNameAvailabilityRequestTests
{
    [Fact]
    public void Given_ValidRequest_When_Validate_Then_NoError()
    {
        // Arrange
        var sut = new CheckResourceNameAvailabilityRequest
        {
            ProjectId = Guid.NewGuid().ToString(),
            Name = "my-resource",
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void Given_NullProjectId_When_Validate_Then_ReturnsError()
    {
        // Arrange
        var sut = new CheckResourceNameAvailabilityRequest
        {
            ProjectId = null!,
            Name = "my-resource",
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(CheckResourceNameAvailabilityRequest.ProjectId)).Should().BeTrue();
    }

    [Fact]
    public void Given_NullName_When_Validate_Then_ReturnsError()
    {
        // Arrange
        var sut = new CheckResourceNameAvailabilityRequest
        {
            ProjectId = Guid.NewGuid().ToString(),
            Name = null!,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(CheckResourceNameAvailabilityRequest.Name)).Should().BeTrue();
    }
}
