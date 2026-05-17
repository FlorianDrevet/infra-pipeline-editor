using FluentAssertions;
using InfraFlowSculptor.Application.LogAnalyticsWorkspaces.Commands.UpdateLogAnalyticsWorkspace;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.LogAnalyticsWorkspaces.Commands.UpdateLogAnalyticsWorkspace;

public sealed class UpdateLogAnalyticsWorkspaceCommandValidatorTests
{
    private readonly UpdateLogAnalyticsWorkspaceCommandValidator _sut = new();

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new UpdateLogAnalyticsWorkspaceCommand(
            AzureResourceId.CreateUnique(),
            new Name("my-workspace"),
            new Location(Location.LocationEnum.WestEurope));

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_EmptyId_When_Validate_Then_FailsOnId()
    {
        // Arrange
        var command = new UpdateLogAnalyticsWorkspaceCommand(
            null!,
            new Name("my-workspace"),
            new Location(Location.LocationEnum.WestEurope));

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateLogAnalyticsWorkspaceCommand.Id));
    }

    [Fact]
    public void Given_EmptyName_When_Validate_Then_FailsOnName()
    {
        // Arrange
        var command = new UpdateLogAnalyticsWorkspaceCommand(
            AzureResourceId.CreateUnique(),
            null!,
            new Location(Location.LocationEnum.WestEurope));

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateLogAnalyticsWorkspaceCommand.Name));
    }
}
