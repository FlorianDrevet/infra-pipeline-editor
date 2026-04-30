using FluentAssertions;
using InfraFlowSculptor.Application.ContainerRegistries.Commands.DeleteContainerRegistry;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.ContainerRegistries.Commands.DeleteContainerRegistry;

public sealed class DeleteContainerRegistryCommandValidatorTests
{
    private const string IdProperty = nameof(DeleteContainerRegistryCommand.Id);

    private readonly DeleteContainerRegistryCommandValidator _sut = new();

    [Fact]
    public void Given_NonNullId_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new DeleteContainerRegistryCommand(AzureResourceId.CreateUnique());

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Given_NullId_When_Validate_Then_FailsOnId()
    {
        // Arrange
        var command = new DeleteContainerRegistryCommand(null!);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == IdProperty);
    }
}
