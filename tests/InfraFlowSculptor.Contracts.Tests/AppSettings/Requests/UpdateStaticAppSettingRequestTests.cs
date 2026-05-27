using FluentAssertions;
using InfraFlowSculptor.Contracts.AppSettings.Requests;
using InfraFlowSculptor.Contracts.Tests.TestSupport;

namespace InfraFlowSculptor.Contracts.Tests.AppSettings.Requests;

public sealed class UpdateStaticAppSettingRequestTests
{
    [Fact]
    public void Given_ValidRequest_When_Validate_Then_NoError()
    {
        // Arrange
        var sut = new UpdateStaticAppSettingRequest
        {
            Name = "KEYVAULT_URI",
            EnvironmentValues = new Dictionary<string, string> { ["dev"] = "value" },
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
        var sut = new UpdateStaticAppSettingRequest
        {
            Name = null!,
            EnvironmentValues = new Dictionary<string, string> { ["dev"] = "value" },
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(UpdateStaticAppSettingRequest.Name)).Should().BeTrue();
    }

    [Fact]
    public void Given_NameOver256Chars_When_Validate_Then_ReturnsError()
    {
        // Arrange
        var sut = new UpdateStaticAppSettingRequest
        {
            Name = new string('x', 257),
            EnvironmentValues = new Dictionary<string, string> { ["dev"] = "value" },
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(UpdateStaticAppSettingRequest.Name)).Should().BeTrue();
    }

    [Fact]
    public void Given_NullEnvironmentValues_When_Validate_Then_ReturnsError()
    {
        // Arrange
        var sut = new UpdateStaticAppSettingRequest
        {
            Name = "KEYVAULT_URI",
            EnvironmentValues = null!,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(UpdateStaticAppSettingRequest.EnvironmentValues)).Should().BeTrue();
    }
}
