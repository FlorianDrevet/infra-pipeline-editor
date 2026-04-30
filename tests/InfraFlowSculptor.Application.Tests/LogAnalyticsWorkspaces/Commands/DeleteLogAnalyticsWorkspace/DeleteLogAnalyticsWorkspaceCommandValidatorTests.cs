using FluentAssertions;
using InfraFlowSculptor.Application.LogAnalyticsWorkspaces.Commands.DeleteLogAnalyticsWorkspace;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.LogAnalyticsWorkspaces.Commands.DeleteLogAnalyticsWorkspace;

public sealed class DeleteLogAnalyticsWorkspaceCommandValidatorTests
{
    private const string IdProperty = nameof(DeleteLogAnalyticsWorkspaceCommand.Id);

    private readonly DeleteLogAnalyticsWorkspaceCommandValidator _sut = new();

    [Fact]
    public void Given_NonNullId_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new DeleteLogAnalyticsWorkspaceCommand(AzureResourceId.CreateUnique());

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Given_NullId_When_Validate_Then_FailsOnId()
    {
        // Arrange
        var command = new DeleteLogAnalyticsWorkspaceCommand(null!);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == IdProperty);
    }
}
