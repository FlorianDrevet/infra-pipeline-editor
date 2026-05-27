using FluentAssertions;
using InfraFlowSculptor.Application.ContainerAppEnvironments.Commands.UpdateContainerAppEnvironment;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.ContainerAppEnvironments.Commands.UpdateContainerAppEnvironment;

public sealed class UpdateContainerAppEnvironmentCommandValidatorTests
{
    private readonly UpdateContainerAppEnvironmentCommandValidator _sut = new();

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        var command = new UpdateContainerAppEnvironmentCommand(
            AzureResourceId.CreateUnique(),
            new Name("cae-test"),
            new Location(Location.LocationEnum.WestEurope));

        var result = _sut.Validate(command);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_NullId_When_Validate_Then_Fails()
    {
        var command = new UpdateContainerAppEnvironmentCommand(
            null!,
            new Name("cae-test"),
            new Location(Location.LocationEnum.WestEurope));

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateContainerAppEnvironmentCommand.Id));
    }

    [Fact]
    public void Given_NullName_When_Validate_Then_Fails()
    {
        var command = new UpdateContainerAppEnvironmentCommand(
            AzureResourceId.CreateUnique(),
            null!,
            new Location(Location.LocationEnum.WestEurope));

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateContainerAppEnvironmentCommand.Name));
    }
}
