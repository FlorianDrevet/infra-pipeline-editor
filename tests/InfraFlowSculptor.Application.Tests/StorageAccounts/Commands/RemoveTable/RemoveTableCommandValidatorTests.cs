using FluentAssertions;
using InfraFlowSculptor.Application.StorageAccounts.Commands.RemoveTable;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.StorageAccounts.Commands.RemoveTable;

public sealed class RemoveTableCommandValidatorTests
{
    private readonly RemoveTableCommandValidator _sut = new();

    private static RemoveTableCommand ValidCommand() => new(
        AzureResourceId.CreateUnique(),
        StorageTableId.CreateUnique());

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
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RemoveTableCommand.StorageAccountId));
    }

    [Fact]
    public void Given_EmptyTableId_When_Validate_Then_FailsOnTableId()
    {
        var command = ValidCommand() with { TableId = null! };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RemoveTableCommand.TableId));
    }
}
