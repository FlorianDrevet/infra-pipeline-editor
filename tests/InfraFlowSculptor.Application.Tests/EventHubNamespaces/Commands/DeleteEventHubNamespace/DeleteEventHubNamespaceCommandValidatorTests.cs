using FluentAssertions;
using InfraFlowSculptor.Application.EventHubNamespaces.Commands.DeleteEventHubNamespace;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.EventHubNamespaces.Commands.DeleteEventHubNamespace;

public sealed class DeleteEventHubNamespaceCommandValidatorTests
{
    private const string IdProperty = nameof(DeleteEventHubNamespaceCommand.Id);

    private readonly DeleteEventHubNamespaceCommandValidator _sut = new();

    [Fact]
    public void Given_NonNullId_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new DeleteEventHubNamespaceCommand(AzureResourceId.CreateUnique());

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Given_NullId_When_Validate_Then_FailsOnId()
    {
        // Arrange
        var command = new DeleteEventHubNamespaceCommand(null!);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == IdProperty);
    }
}
