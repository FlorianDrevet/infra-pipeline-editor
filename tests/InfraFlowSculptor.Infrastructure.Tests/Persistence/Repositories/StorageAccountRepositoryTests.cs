using FluentAssertions;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using InfraFlowSculptor.Domain.StorageAccountAggregate;
using InfraFlowSculptor.Domain.StorageAccountAggregate.Entities;
using InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;
using InfraFlowSculptor.Infrastructure.Persistence;
using InfraFlowSculptor.Infrastructure.Persistence.Repositories;
using Xunit;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Infrastructure.Tests.Persistence.Repositories;

public sealed class StorageAccountRepositoryTests : IDisposable
{
    private const string AccountName = "stshared001";
    private const string OtherAccountName = "stshared002";
    private const string ContainerName = "logs";

    private readonly ProjectDbContext _context;
    private readonly StorageAccountRepository _sut;

    public StorageAccountRepositoryTests()
    {
        _context = InMemoryDbContextFactory.Create();
        _sut = new StorageAccountRepository(_context);
    }

    public void Dispose() => _context.Dispose();

    private static StorageAccount NewAccount(ResourceGroupId resourceGroupId, string name = AccountName)
        => StorageAccount.Create(
            resourceGroupId,
            new Name(name),
            new Location(Location.LocationEnum.WestEurope),
            new StorageAccountKind(StorageAccountKind.Kind.StorageV2),
            new StorageAccessTier(StorageAccessTier.Tier.Hot),
            allowBlobPublicAccess: false,
            enableHttpsTrafficOnly: true,
            new StorageAccountTlsVersion(StorageAccountTlsVersion.Version.Tls12));

    // Persists a real ResourceGroup and returns its id. The StorageAccount -> ResourceGroup
    // FK is required, so the WithSubResources include is an INNER JOIN: an orphan
    // ResourceGroupId filters the account row out. StorageAccountAccessHelper also reads
    // ResourceGroup.InfraConfigId on both the tracked and read-only paths.
    private ResourceGroupId NewResourceGroup()
    {
        var resourceGroup = ResourceGroup.Create(
            new Name("rg-storage"),
            InfrastructureConfigId.CreateUnique(),
            new Location(Location.LocationEnum.WestEurope));
        _context.ResourceGroups.Add(resourceGroup);
        return resourceGroup.Id;
    }

    [Fact]
    public async Task Given_StoredAccount_When_GetByIdAsync_Then_ReturnsAccount_Async()
    {
        // Arrange
        var account = NewAccount(NewResourceGroup());
        _context.StorageAccounts.Add(account);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetByIdAsync(account.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(account.Id);
    }

    [Fact]
    public async Task Given_UnknownId_When_GetByIdAsync_Then_ReturnsNull_Async()
    {
        // Act
        var result = await _sut.GetByIdAsync(AzureResourceId.CreateUnique(), CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Given_AddedAccount_When_AddAsync_Then_PersistsAccount_Async()
    {
        // Arrange
        var account = NewAccount(NewResourceGroup());

        // Act
        _sut.Add(account);
        await _context.SaveChangesAsync();

        // Assert
        var stored = await _sut.GetByIdAsync(account.Id, CancellationToken.None);
        stored.Should().NotBeNull();
    }

    [Fact]
    public async Task Given_StoredAccount_When_DeleteAsync_Then_RemovesAccount_Async()
    {
        // Arrange
        var account = NewAccount(ResourceGroupId.CreateUnique());
        _context.StorageAccounts.Add(account);
        await _context.SaveChangesAsync();

        // Act
        var deleted = await _sut.DeleteAsync(account.Id);
        await _context.SaveChangesAsync();

        // Assert
        deleted.Should().BeTrue();
        (await _sut.GetByIdAsync(account.Id, CancellationToken.None)).Should().BeNull();
    }

    [Fact]
    public async Task Given_StoredAccount_When_GetByIdWithSubResourcesAsync_Then_ReturnsAccount_Async()
    {
        // Arrange
        var account = NewAccount(NewResourceGroup());
        _context.StorageAccounts.Add(account);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetByIdWithSubResourcesAsync(account.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(account.Id);
    }

    [Fact]
    public async Task Given_StoredAccounts_When_GetByResourceGroupIdAsync_Then_ReturnsOnlyMatching_Async()
    {
        // Arrange
        var rgId = NewResourceGroup();
        var owned = NewAccount(rgId);
        var unrelated = NewAccount(NewResourceGroup(), OtherAccountName);
        await _context.StorageAccounts.AddRangeAsync(owned, unrelated);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetByResourceGroupIdAsync(rgId);

        // Assert
        result.Should().HaveCount(1);
        result.Single().Id.Should().Be(owned.Id);
    }

    [Fact]
    public async Task Given_BlobContainer_When_AddBlobContainer_Then_TracksContainer_Async()
    {
        // Arrange
        var account = NewAccount(ResourceGroupId.CreateUnique());
        _context.StorageAccounts.Add(account);
        await _context.SaveChangesAsync();
        var container = BlobContainer.Create(account.Id, ContainerName, new BlobContainerPublicAccess(BlobContainerPublicAccess.AccessLevel.None));

        // Act
        _sut.AddBlobContainer(container);
        await _context.SaveChangesAsync();

        // Assert
        var found = await _context.BlobContainers.FindAsync(container.Id);
        found.Should().NotBeNull();
        found!.StorageAccountId.Should().Be(account.Id);
    }

    [Fact]
    public async Task Given_StoredBlobContainer_When_RemoveBlobContainerAsync_Then_ReturnsTrue_Async()
    {
        // Arrange
        var account = NewAccount(ResourceGroupId.CreateUnique());
        _context.StorageAccounts.Add(account);
        await _context.SaveChangesAsync();
        var container = BlobContainer.Create(account.Id, ContainerName, new BlobContainerPublicAccess(BlobContainerPublicAccess.AccessLevel.None));
        _context.BlobContainers.Add(container);
        await _context.SaveChangesAsync();

        // Act
        var removed = await _sut.RemoveBlobContainerAsync(account.Id, container.Id);
        await _context.SaveChangesAsync();

        // Assert
        removed.Should().BeTrue();
        (await _context.BlobContainers.FindAsync(container.Id)).Should().BeNull();
    }

    [Fact]
    public async Task Given_UnknownContainer_When_RemoveBlobContainerAsync_Then_ReturnsFalse_Async()
    {
        // Arrange
        var account = NewAccount(ResourceGroupId.CreateUnique());

        // Act
        var removed = await _sut.RemoveBlobContainerAsync(account.Id, BlobContainerId.CreateUnique());

        // Assert
        removed.Should().BeFalse();
    }
}
