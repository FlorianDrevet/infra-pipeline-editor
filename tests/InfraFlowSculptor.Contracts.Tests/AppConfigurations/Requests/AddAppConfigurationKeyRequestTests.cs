using FluentAssertions;
using InfraFlowSculptor.Contracts.AppConfigurations.Requests;
using InfraFlowSculptor.Contracts.Tests.TestSupport;

namespace InfraFlowSculptor.Contracts.Tests.AppConfigurations.Requests;

public sealed class AddAppConfigurationKeyRequestTests
{
    [Fact]
    public void Given_ValidRequest_When_Validate_Then_NoError()
    {
        // Arrange
        var sut = new AddAppConfigurationKeyRequest
        {
            Key = "Core:Authorization:ClientId",
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void Given_NullKey_When_Validate_Then_ReturnsError()
    {
        // Arrange
        var sut = new AddAppConfigurationKeyRequest
        {
            Key = null!,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(AddAppConfigurationKeyRequest.Key)).Should().BeTrue();
    }

    [Fact]
    public void Given_KeyOver512Chars_When_Validate_Then_ReturnsError()
    {
        // Arrange
        var sut = new AddAppConfigurationKeyRequest
        {
            Key = new string('x', 513),
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(AddAppConfigurationKeyRequest.Key)).Should().BeTrue();
    }

    [Fact]
    public void Given_LabelOver128Chars_When_Validate_Then_ReturnsError()
    {
        // Arrange
        var sut = new AddAppConfigurationKeyRequest
        {
            Key = "ValidKey",
            Label = new string('x', 129),
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(AddAppConfigurationKeyRequest.Label)).Should().BeTrue();
    }

    [Fact]
    public void Given_SecretNameOver256Chars_When_Validate_Then_ReturnsError()
    {
        // Arrange
        var sut = new AddAppConfigurationKeyRequest
        {
            Key = "ValidKey",
            SecretName = new string('x', 257),
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(AddAppConfigurationKeyRequest.SecretName)).Should().BeTrue();
    }

    [Fact]
    public void Given_PipelineVariableNameOver256Chars_When_Validate_Then_ReturnsError()
    {
        // Arrange
        var sut = new AddAppConfigurationKeyRequest
        {
            Key = "ValidKey",
            PipelineVariableName = new string('x', 257),
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(AddAppConfigurationKeyRequest.PipelineVariableName)).Should().BeTrue();
    }

    [Fact]
    public void Given_SourceOutputNameOver128Chars_When_Validate_Then_ReturnsError()
    {
        // Arrange
        var sut = new AddAppConfigurationKeyRequest
        {
            Key = "ValidKey",
            SourceOutputName = new string('x', 129),
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(AddAppConfigurationKeyRequest.SourceOutputName)).Should().BeTrue();
    }
}
