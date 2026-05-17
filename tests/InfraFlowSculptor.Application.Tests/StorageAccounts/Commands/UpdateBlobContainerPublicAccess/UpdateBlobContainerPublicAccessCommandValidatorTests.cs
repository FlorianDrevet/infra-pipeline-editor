using FluentAssertions;
using InfraFlowSculptor.Application.StorageAccounts.Commands.UpdateBlobContainerPublicAccess;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.StorageAccounts.Commands.UpdateBlobContainerPublicAccess;

public sealed class UpdateBlobContainerPublicAccessCommandValidatorTests
{
    private readonly UpdateBlobContainerPublicAccessCommandValidator _sut = new();

    private static UpdateBlobContainerPublicAccessCommand ValidCommand() => new(
        AzureResourceId.CreateUnique(),
        BlobContainerId.CreateUnique(),
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
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateBlobContainerPublicAccessCommand.StorageAccountId));
    }

    [Fact]
    public void Given_EmptyContainerId_When_Validate_Then_FailsOnContainerId()
    {
        var command = ValidCommand() with { ContainerId = null! };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateBlobContainerPublicAccessCommand.ContainerId));
    }

    [Fact]
    public void Given_NullPublicAccess_When_Validate_Then_FailsOnPublicAccess()
    {
        var command = ValidCommand() with { PublicAccess = null! };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateBlobContainerPublicAccessCommand.PublicAccess));
    }
}
