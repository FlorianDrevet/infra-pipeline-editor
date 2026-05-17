using FluentAssertions;
using InfraFlowSculptor.Application.UserAssignedIdentities.Commands.CreateUserAssignedIdentity;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.UserAssignedIdentities.Commands.CreateUserAssignedIdentity;

public sealed class CreateUserAssignedIdentityCommandValidatorTests
{
    private readonly CreateUserAssignedIdentityCommandValidator _sut = new();

    private static CreateUserAssignedIdentityCommand ValidCommand() => new(
        ResourceGroupId.CreateUnique(),
        new Name("my-identity"),
        new Location(Location.LocationEnum.FranceCentral));

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        var result = _sut.Validate(ValidCommand());

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_EmptyName_When_Validate_Then_FailsOnName()
    {
        var command = ValidCommand() with { Name = null! };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateUserAssignedIdentityCommand.Name));
    }

    [Fact]
    public void Given_NameTooLong_When_Validate_Then_FailsOnNameValue()
    {
        var command = ValidCommand() with { Name = new Name(new string('a', 81)) };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name.Value");
    }

    [Fact]
    public void Given_EmptyResourceGroupId_When_Validate_Then_FailsOnResourceGroupId()
    {
        var command = ValidCommand() with { ResourceGroupId = null! };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateUserAssignedIdentityCommand.ResourceGroupId));
    }
}
