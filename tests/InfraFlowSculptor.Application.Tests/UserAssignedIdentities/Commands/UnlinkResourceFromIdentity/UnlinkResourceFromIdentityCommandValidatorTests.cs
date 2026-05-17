using FluentAssertions;
using InfraFlowSculptor.Application.UserAssignedIdentities.Commands.UnlinkResourceFromIdentity;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.UserAssignedIdentities.Commands.UnlinkResourceFromIdentity;

public sealed class UnlinkResourceFromIdentityCommandValidatorTests
{
    private readonly UnlinkResourceFromIdentityCommandValidator _sut = new();

    private static UnlinkResourceFromIdentityCommand ValidCommand() => new(
        AzureResourceId.CreateUnique(),
        AzureResourceId.CreateUnique());

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        var result = _sut.Validate(ValidCommand());

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_EmptyIdentityId_When_Validate_Then_FailsOnIdentityId()
    {
        var command = ValidCommand() with { IdentityId = null! };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UnlinkResourceFromIdentityCommand.IdentityId));
    }

    [Fact]
    public void Given_EmptySourceResourceId_When_Validate_Then_FailsOnSourceResourceId()
    {
        var command = ValidCommand() with { SourceResourceId = null! };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UnlinkResourceFromIdentityCommand.SourceResourceId));
    }
}
