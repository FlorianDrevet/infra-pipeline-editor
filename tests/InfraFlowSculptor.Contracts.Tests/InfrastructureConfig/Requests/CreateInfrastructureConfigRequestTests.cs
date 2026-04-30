using FluentAssertions;
using InfraFlowSculptor.Contracts.InfrastructureConfig.Requests;
using InfraFlowSculptor.Contracts.Tests.TestSupport;

namespace InfraFlowSculptor.Contracts.Tests.InfrastructureConfig.Requests;

public sealed class CreateInfrastructureConfigRequestTests
{
    private const string ValidProjectId = "11111111-1111-1111-1111-111111111111";

    [Fact]
    public void Given_ValidRequest_When_Validate_Then_NoError()
    {
        // Arrange
        var sut = new CreateInfrastructureConfigRequest
        {
            Name = "infra-prod",
            ProjectId = ValidProjectId,
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
        var sut = new CreateInfrastructureConfigRequest
        {
            Name = null!,
            ProjectId = ValidProjectId,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(CreateInfrastructureConfigRequest.Name)).Should().BeTrue();
    }

    [Fact]
    public void Given_NullProjectId_When_Validate_Then_ReturnsRequiredError()
    {
        // Arrange
        var sut = new CreateInfrastructureConfigRequest
        {
            Name = "infra",
            ProjectId = null!,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(CreateInfrastructureConfigRequest.ProjectId)).Should().BeTrue();
    }

    [Fact]
    public void Given_MalformedProjectId_When_Validate_Then_ReturnsGuidError()
    {
        // Arrange
        var sut = new CreateInfrastructureConfigRequest
        {
            Name = "infra",
            ProjectId = "not-a-guid",
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(CreateInfrastructureConfigRequest.ProjectId)).Should().BeTrue();
    }

    [Fact]
    public void Given_EmptyGuidProjectId_When_Validate_Then_ReturnsGuidError()
    {
        // Arrange
        var sut = new CreateInfrastructureConfigRequest
        {
            Name = "infra",
            ProjectId = Guid.Empty.ToString(),
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(CreateInfrastructureConfigRequest.ProjectId)).Should().BeTrue();
    }
}
