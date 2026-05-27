using FluentAssertions;
using InfraFlowSculptor.Application.ContainerAppEnvironments.Commands.CreateContainerAppEnvironment;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.ContainerAppEnvironments.Commands.CreateContainerAppEnvironment;

public sealed class CreateContainerAppEnvironmentCommandValidatorTests
{
    private readonly CreateContainerAppEnvironmentCommandValidator _sut = new();

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        var command = new CreateContainerAppEnvironmentCommand(
            ResourceGroupId.CreateUnique(),
            new Name("cae-test"),
            new Location(Location.LocationEnum.WestEurope));

        var result = _sut.Validate(command);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_NullName_When_Validate_Then_Fails()
    {
        var command = new CreateContainerAppEnvironmentCommand(
            ResourceGroupId.CreateUnique(),
            null!,
            new Location(Location.LocationEnum.WestEurope));

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateContainerAppEnvironmentCommand.Name));
    }

    [Fact]
    public void Given_NullResourceGroupId_When_Validate_Then_Fails()
    {
        var command = new CreateContainerAppEnvironmentCommand(
            null!,
            new Name("cae-test"),
            new Location(Location.LocationEnum.WestEurope));

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateContainerAppEnvironmentCommand.ResourceGroupId));
    }
}
