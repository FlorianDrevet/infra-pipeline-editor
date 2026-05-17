using FluentAssertions;
using InfraFlowSculptor.Application.StorageAccounts.Commands.RemoveQueue;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.StorageAccounts.Commands.RemoveQueue;

public sealed class RemoveQueueCommandValidatorTests
{
    private readonly RemoveQueueCommandValidator _sut = new();

    private static RemoveQueueCommand ValidCommand() => new(
        AzureResourceId.CreateUnique(),
        StorageQueueId.CreateUnique());

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
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RemoveQueueCommand.StorageAccountId));
    }

    [Fact]
    public void Given_EmptyQueueId_When_Validate_Then_FailsOnQueueId()
    {
        var command = ValidCommand() with { QueueId = null! };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RemoveQueueCommand.QueueId));
    }
}
