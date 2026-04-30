using FluentAssertions;
using InfraFlowSculptor.Application.ServiceBusNamespaces.Commands.DeleteServiceBusNamespace;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.ServiceBusNamespaces.Commands.DeleteServiceBusNamespace;

public sealed class DeleteServiceBusNamespaceCommandValidatorTests
{
    private const string IdProperty = nameof(DeleteServiceBusNamespaceCommand.Id);

    private readonly DeleteServiceBusNamespaceCommandValidator _sut = new();

    [Fact]
    public void Given_NonNullId_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new DeleteServiceBusNamespaceCommand(AzureResourceId.CreateUnique());

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Given_NullId_When_Validate_Then_FailsOnId()
    {
        // Arrange
        var command = new DeleteServiceBusNamespaceCommand(null!);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == IdProperty);
    }
}
