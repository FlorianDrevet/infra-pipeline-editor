using FluentAssertions;
using InfraFlowSculptor.Contracts.Projects.Requests;
using InfraFlowSculptor.Contracts.Tests.TestSupport;

namespace InfraFlowSculptor.Contracts.Tests.Projects.Requests;

public sealed class SetProjectResourceNamingTemplateRequestTests
{
    [Fact]
    public void Given_ValidRequest_When_Validate_Then_NoError()
    {
        // Arrange
        var sut = new SetProjectResourceNamingTemplateRequest
        {
            Template = "{prefix}-{name}-{env}",
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void Given_NullTemplate_When_Validate_Then_ReturnsError()
    {
        // Arrange
        var sut = new SetProjectResourceNamingTemplateRequest
        {
            Template = null!,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(SetProjectResourceNamingTemplateRequest.Template)).Should().BeTrue();
    }
}
