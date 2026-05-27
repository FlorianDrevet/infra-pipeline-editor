using FluentAssertions;
using InfraFlowSculptor.Application.UserAssignedIdentities.Commands.UpdateUserAssignedIdentity;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.UserAssignedIdentities.Commands.UpdateUserAssignedIdentity;

public sealed class UpdateUserAssignedIdentityCommandValidatorTests
{
    private readonly UpdateUserAssignedIdentityCommandValidator _sut = new();

    private static UpdateUserAssignedIdentityCommand ValidCommand() => new(
        AzureResourceId.CreateUnique(),
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
    public void Given_EmptyId_When_Validate_Then_FailsOnId()
    {
        var command = ValidCommand() with { Id = null! };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateUserAssignedIdentityCommand.Id));
    }

    [Fact]
    public void Given_EmptyName_When_Validate_Then_FailsOnName()
    {
        var command = ValidCommand() with { Name = null! };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateUserAssignedIdentityCommand.Name));
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
    public void Given_EmptyLocation_When_Validate_Then_FailsOnLocation()
    {
        var command = ValidCommand() with { Location = null! };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateUserAssignedIdentityCommand.Location));
    }
}
