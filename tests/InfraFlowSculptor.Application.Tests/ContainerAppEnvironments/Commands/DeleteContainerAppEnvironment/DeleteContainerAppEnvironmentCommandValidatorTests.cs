using FluentAssertions;
using InfraFlowSculptor.Application.ContainerAppEnvironments.Commands.DeleteContainerAppEnvironment;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.ContainerAppEnvironments.Commands.DeleteContainerAppEnvironment;

public sealed class DeleteContainerAppEnvironmentCommandValidatorTests
{
    private const string IdProperty = nameof(DeleteContainerAppEnvironmentCommand.Id);

    private readonly DeleteContainerAppEnvironmentCommandValidator _sut = new();

    [Fact]
    public void Given_NonNullId_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new DeleteContainerAppEnvironmentCommand(AzureResourceId.CreateUnique());

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Given_NullId_When_Validate_Then_FailsOnId()
    {
        // Arrange
        var command = new DeleteContainerAppEnvironmentCommand(null!);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == IdProperty);
    }
}
