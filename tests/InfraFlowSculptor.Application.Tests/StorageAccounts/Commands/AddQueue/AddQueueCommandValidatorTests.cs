using FluentAssertions;
using InfraFlowSculptor.Application.StorageAccounts.Commands.AddQueue;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.StorageAccounts.Commands.AddQueue;

public sealed class AddQueueCommandValidatorTests
{
    private readonly AddQueueCommandValidator _sut = new();

    private static AddQueueCommand ValidCommand() => new(
        AzureResourceId.CreateUnique(),
        "my-queue");

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        var result = _sut.Validate(ValidCommand());

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_EmptyStorageAccountId_When_Validate_Then_FailsOnStorageAccountId()
    {
        var command = ValidCommand() with { StorageAccountId = null! };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AddQueueCommand.StorageAccountId));
    }

    [Fact]
    public void Given_EmptyName_When_Validate_Then_FailsOnName()
    {
        var command = ValidCommand() with { Name = "" };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AddQueueCommand.Name));
    }
}
