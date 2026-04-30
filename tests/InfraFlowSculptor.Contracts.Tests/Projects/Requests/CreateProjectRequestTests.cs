using FluentAssertions;
using InfraFlowSculptor.Contracts.Projects.Requests;
using InfraFlowSculptor.Contracts.Tests.TestSupport;

namespace InfraFlowSculptor.Contracts.Tests.Projects.Requests;

public sealed class CreateProjectRequestTests
{
    [Fact]
    public void Given_ValidName_When_Validate_Then_NoError()
    {
        // Arrange
        var sut = new CreateProjectRequest
        {
            Name = "my-project",
            Description = "A demo project",
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void Given_NullName_When_Validate_Then_ReturnsRequiredError()
    {
        // Arrange
        var sut = new CreateProjectRequest
        {
            Name = null!,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.Should().ContainSingle()
            .Which.MemberNames.Should().Contain(nameof(CreateProjectRequest.Name));
    }

    [Fact]
    public void Given_NullDescription_When_Validate_Then_NoError()
    {
        // Arrange
        var sut = new CreateProjectRequest
        {
            Name = "ok-name",
            Description = null,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.Should().BeEmpty();
    }
}
