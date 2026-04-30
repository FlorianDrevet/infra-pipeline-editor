using FluentAssertions;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.StorageAccountAggregate.Entities;
using InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.Tests.StorageAccountAggregate.Entities;

public sealed class BlobContainerTests
{
    private const string ContainerName = "uploads";

    [Fact]
    public void Given_FactoryArguments_When_Create_Then_InitializesProperties()
    {
        // Arrange
        var storageAccountId = AzureResourceId.CreateUnique();
        var publicAccess = new BlobContainerPublicAccess(BlobContainerPublicAccess.AccessLevel.Blob);

        // Act
        var sut = BlobContainer.Create(storageAccountId, ContainerName, publicAccess);

        // Assert
        sut.Id.Should().NotBeNull();
        sut.StorageAccountId.Should().Be(storageAccountId);
        sut.Name.Should().Be(ContainerName);
        sut.PublicAccess.Should().Be(publicAccess);
    }

    [Fact]
    public void Given_NewAccessLevel_When_UpdatePublicAccess_Then_UpdatesAccess()
    {
        // Arrange
        var sut = BlobContainer.Create(
            AzureResourceId.CreateUnique(),
            ContainerName,
            new BlobContainerPublicAccess(BlobContainerPublicAccess.AccessLevel.None));
        var newAccess = new BlobContainerPublicAccess(BlobContainerPublicAccess.AccessLevel.Container);

        // Act
        sut.UpdatePublicAccess(newAccess);

        // Assert
        sut.PublicAccess.Should().Be(newAccess);
    }
}
