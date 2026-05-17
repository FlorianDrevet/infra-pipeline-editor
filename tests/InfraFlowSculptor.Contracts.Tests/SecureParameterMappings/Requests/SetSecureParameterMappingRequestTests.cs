using FluentAssertions;
using InfraFlowSculptor.Contracts.SecureParameterMappings.Requests;
using InfraFlowSculptor.Contracts.Tests.TestSupport;

namespace InfraFlowSculptor.Contracts.Tests.SecureParameterMappings.Requests;

public sealed class SetSecureParameterMappingRequestTests
{
    [Fact]
    public void Given_ValidRequest_When_Validate_Then_NoError()
    {
        // Arrange
        var sut = new SetSecureParameterMappingRequest
        {
            SecureParameterName = "administratorLoginPassword",
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void Given_NullSecureParameterName_When_Validate_Then_ReturnsError()
    {
        // Arrange
        var sut = new SetSecureParameterMappingRequest
        {
            SecureParameterName = null!,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(SetSecureParameterMappingRequest.SecureParameterName)).Should().BeTrue();
    }

    [Fact]
    public void Given_PipelineVariableNameOver200Chars_When_Validate_Then_ReturnsError()
    {
        // Arrange
        var sut = new SetSecureParameterMappingRequest
        {
            SecureParameterName = "administratorLoginPassword",
            PipelineVariableName = new string('x', 201),
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(SetSecureParameterMappingRequest.PipelineVariableName)).Should().BeTrue();
    }
}
