using FluentAssertions;
using InfraFlowSculptor.Contracts.Projects.Requests;
using InfraFlowSculptor.Contracts.Tests.TestSupport;

namespace InfraFlowSculptor.Contracts.Tests.Projects.Requests;

public sealed class SetProjectLayoutPresetRequestTests
{
    [Fact]
    public void Given_ValidRequest_When_Validate_Then_NoError()
    {
        // Arrange
        var sut = new SetProjectLayoutPresetRequest
        {
            Preset = "AllInOne",
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void Given_NullPreset_When_Validate_Then_ReturnsError()
    {
        // Arrange
        var sut = new SetProjectLayoutPresetRequest
        {
            Preset = null!,
        };

        // Act
        var results = RequestValidator.Validate(sut);

        // Assert
        results.HasErrorForMember(nameof(SetProjectLayoutPresetRequest.Preset)).Should().BeTrue();
    }
}
