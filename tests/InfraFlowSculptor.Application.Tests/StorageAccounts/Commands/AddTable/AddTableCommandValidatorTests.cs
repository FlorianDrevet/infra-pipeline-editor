using FluentAssertions;
using InfraFlowSculptor.Application.StorageAccounts.Commands.AddTable;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.StorageAccounts.Commands.AddTable;

public sealed class AddTableCommandValidatorTests
{
    private readonly AddTableCommandValidator _sut = new();

    private static AddTableCommand ValidCommand() => new(
        AzureResourceId.CreateUnique(),
        "my-table");

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
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AddTableCommand.StorageAccountId));
    }

    [Fact]
    public void Given_EmptyName_When_Validate_Then_FailsOnName()
    {
        var command = ValidCommand() with { Name = "" };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AddTableCommand.Name));
    }
}
