using FluentAssertions;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.CosmosDbAggregate;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using InfraFlowSculptor.Infrastructure.Persistence;
using InfraFlowSculptor.Infrastructure.Persistence.Repositories;
using Xunit;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Infrastructure.Tests.Persistence.Repositories;

public sealed class CosmosDbRepositoryTests : IDisposable
{
    private const string DefaultName = "cosmos-shared";
    private const string OtherName = "cosmos-other";

    private readonly ProjectDbContext _context;
    private readonly CosmosDbRepository _sut;

    public CosmosDbRepositoryTests()
    {
        _context = InMemoryDbContextFactory.Create();
        _sut = new CosmosDbRepository(_context);
    }

    public void Dispose() => _context.Dispose();

    private static CosmosDb NewEntity(ResourceGroupId resourceGroupId, string name = DefaultName)
        => CosmosDb.Create(
            resourceGroupId,
            new Name(name),
            new Location(Location.LocationEnum.WestEurope));

    [Fact]
    public async Task Given_StoredEntity_When_GetByIdAsync_Then_ReturnsEntity_Async()
    {
        // Arrange
        var entity = NewEntity(ResourceGroupId.CreateUnique());
        _context.Set<CosmosDb>().Add(entity);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetByIdAsync(entity.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(entity.Id);
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
    public async Task Given_NewEntity_When_Add_Then_PersistsEntity_Async()
    {
        // Arrange
        var entity = NewEntity(ResourceGroupId.CreateUnique());

        // Act
        _sut.Add(entity);
        await _context.SaveChangesAsync();

        // Assert
        var stored = await _sut.GetByIdAsync(entity.Id, CancellationToken.None);
        stored.Should().NotBeNull();
    }

    [Fact]
    public async Task Given_StoredEntity_When_DeleteAsync_Then_RemovesEntity_Async()
    {
        // Arrange
        var entity = NewEntity(ResourceGroupId.CreateUnique());
        _context.Set<CosmosDb>().Add(entity);
        await _context.SaveChangesAsync();

        // Act
        var deleted = await _sut.DeleteAsync(entity.Id);
        await _context.SaveChangesAsync();

        // Assert
        deleted.Should().BeTrue();
        (await _sut.GetByIdAsync(entity.Id, CancellationToken.None)).Should().BeNull();
    }

    [Fact]
    public async Task Given_StoredEntity_When_GetByIdReadOnlyAsync_Then_ReturnsEntity_Async()
    {
        // Arrange
        var resourceGroup = ResourceGroup.Create(
            new Name("rg-readonly"),
            InfrastructureConfigId.CreateUnique(),
            new Location(Location.LocationEnum.WestEurope));
        _context.ResourceGroups.Add(resourceGroup);
        var entity = NewEntity(resourceGroup.Id);
        _context.Set<CosmosDb>().Add(entity);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetByIdReadOnlyAsync(entity.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(entity.Id);
    }

    [Fact]
    public async Task Given_StoredEntities_When_GetByResourceGroupIdAsync_Then_ReturnsOnlyMatching_Async()
    {
        // Arrange
        var rgId = ResourceGroupId.CreateUnique();
        var owned = NewEntity(rgId);
        var unrelated = NewEntity(ResourceGroupId.CreateUnique(), OtherName);
        _context.Set<CosmosDb>().AddRange(owned, unrelated);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetByResourceGroupIdAsync(rgId);

        // Assert
        result.Should().HaveCount(1);
        result.Single().Id.Should().Be(owned.Id);
    }
}
