using FluentAssertions;
using InfraFlowSculptor.Contracts.InfrastructureConfig.Requests;
using InfraFlowSculptor.Contracts.Tests.TestSupport;

namespace InfraFlowSculptor.Contracts.Tests.InfrastructureConfig.Requests;

public sealed class SetResourceNamingTemplateRequestTests
{
    [Fact]
    public void Given_ValidRequest_When_Validate_Then_NoError()
    {
        // Arrange
        var sut = new SetResourceNamingTemplateRequest
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
        var sut = new SetResourceNamingTemplateRequest
        {
            Template = null!,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(SetResourceNamingTemplateRequest.Template)).Should().BeTrue();
    }
}
