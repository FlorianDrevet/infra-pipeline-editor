using FluentAssertions;
using InfraFlowSculptor.Contracts.AppSettings.Requests;
using InfraFlowSculptor.Contracts.Tests.TestSupport;

namespace InfraFlowSculptor.Contracts.Tests.AppSettings.Requests;

public sealed class AddAppSettingRequestTests
{
    [Fact]
    public void Given_ValidRequest_When_Validate_Then_NoError()
    {
        // Arrange
        var sut = new AddAppSettingRequest
        {
            Name = "KEYVAULT_URI",
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void Given_NullName_When_Validate_Then_ReturnsError()
    {
        // Arrange
        var sut = new AddAppSettingRequest
        {
            Name = null!,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(AddAppSettingRequest.Name)).Should().BeTrue();
    }

    [Fact]
    public void Given_NameOver256Chars_When_Validate_Then_ReturnsError()
    {
        // Arrange
        var sut = new AddAppSettingRequest
        {
            Name = new string('x', 257),
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(AddAppSettingRequest.Name)).Should().BeTrue();
    }

    [Fact]
    public void Given_SourceOutputNameOver128Chars_When_Validate_Then_ReturnsError()
    {
        // Arrange
        var sut = new AddAppSettingRequest
        {
            Name = "VALID",
            SourceOutputName = new string('x', 129),
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(AddAppSettingRequest.SourceOutputName)).Should().BeTrue();
    }

    [Fact]
    public void Given_SecretNameOver256Chars_When_Validate_Then_ReturnsError()
    {
        // Arrange
        var sut = new AddAppSettingRequest
        {
            Name = "VALID",
            SecretName = new string('x', 257),
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(AddAppSettingRequest.SecretName)).Should().BeTrue();
    }
}
