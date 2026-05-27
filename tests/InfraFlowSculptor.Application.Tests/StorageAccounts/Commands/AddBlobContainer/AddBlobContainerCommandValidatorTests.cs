using FluentAssertions;
using InfraFlowSculptor.Application.StorageAccounts.Commands.AddBlobContainer;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.StorageAccounts.Commands.AddBlobContainer;

public sealed class AddBlobContainerCommandValidatorTests
{
    private readonly AddBlobContainerCommandValidator _sut = new();

    private static AddBlobContainerCommand ValidCommand() => new(
        AzureResourceId.CreateUnique(),
        "my-container",
        new BlobContainerPublicAccess(BlobContainerPublicAccess.AccessLevel.None));

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
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AddBlobContainerCommand.StorageAccountId));
    }

    [Fact]
    public void Given_EmptyName_When_Validate_Then_FailsOnName()
    {
        var command = ValidCommand() with { Name = "" };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AddBlobContainerCommand.Name));
    }

    [Fact]
    public void Given_NullPublicAccess_When_Validate_Then_FailsOnPublicAccess()
    {
        var command = ValidCommand() with { PublicAccess = null! };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AddBlobContainerCommand.PublicAccess));
    }
}
