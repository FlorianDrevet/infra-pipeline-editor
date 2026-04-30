using FluentAssertions;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using InfraFlowSculptor.Infrastructure.Persistence;
using InfraFlowSculptor.Infrastructure.Persistence.Repositories;
using Xunit;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Infrastructure.Tests.Persistence.Repositories;

public sealed class ResourceGroupRepositoryTests : IDisposable
{
    private const string GroupName = "rg-shared-001";
    private const string OtherGroupName = "rg-shared-002";

    private readonly ProjectDbContext _context;
    private readonly ResourceGroupRepository _sut;

    public ResourceGroupRepositoryTests()
    {
        _context = InMemoryDbContextFactory.Create();
        _sut = new ResourceGroupRepository(_context);
    }

    public void Dispose() => _context.Dispose();

    private static ResourceGroup NewGroup(InfrastructureConfigId infraConfigId, string name = GroupName)
        => ResourceGroup.Create(new Name(name), infraConfigId, new Location(Location.LocationEnum.WestEurope));

    [Fact]
    public async Task Given_StoredGroup_When_GetByIdAsync_Then_ReturnsGroup_Async()
    {
        // Arrange
        var group = NewGroup(InfrastructureConfigId.CreateUnique());
        await _context.ResourceGroups.AddAsync(group);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetByIdAsync(group.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(group.Id);
    }

    [Fact]
    public async Task Given_UnknownId_When_GetByIdAsync_Then_ReturnsNull_Async()
    {
        // Act
        var result = await _sut.GetByIdAsync(ResourceGroupId.CreateUnique(), CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Given_AddedGroup_When_AddAsync_Then_PersistsGroup_Async()
    {
        // Arrange
        var group = NewGroup(InfrastructureConfigId.CreateUnique());

        // Act
        await _sut.AddAsync(group);
        await _context.SaveChangesAsync();

        // Assert
        var stored = await _sut.GetByIdAsync(group.Id, CancellationToken.None);
        stored.Should().NotBeNull();
    }

    [Fact]
    public async Task Given_StoredGroup_When_DeleteAsync_Then_RemovesGroup_Async()
    {
        // Arrange
        var group = NewGroup(InfrastructureConfigId.CreateUnique());
        await _context.ResourceGroups.AddAsync(group);
        await _context.SaveChangesAsync();

        // Act
        var deleted = await _sut.DeleteAsync(group.Id);
        await _context.SaveChangesAsync();

        // Assert
        deleted.Should().BeTrue();
        (await _sut.GetByIdAsync(group.Id, CancellationToken.None)).Should().BeNull();
    }

    [Fact]
    public async Task Given_StoredGroups_When_GetByInfraConfigIdAsync_Then_ReturnsOnlyMatching_Async()
    {
        // Arrange
        var configId = InfrastructureConfigId.CreateUnique();
        var otherConfigId = InfrastructureConfigId.CreateUnique();
        var owned = NewGroup(configId);
        var other = NewGroup(otherConfigId, OtherGroupName);
        await _context.ResourceGroups.AddRangeAsync(owned, other);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetByInfraConfigIdAsync(configId);

        // Assert
        result.Should().HaveCount(1);
        result.Single().Id.Should().Be(owned.Id);
    }

    [Fact]
    public async Task Given_StoredGroups_When_GetLightweightByInfraConfigIdAsync_Then_ReturnsOnlyMatching_Async()
    {
        // Arrange
        var configId = InfrastructureConfigId.CreateUnique();
        var owned = NewGroup(configId);
        var other = NewGroup(InfrastructureConfigId.CreateUnique(), OtherGroupName);
        await _context.ResourceGroups.AddRangeAsync(owned, other);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetLightweightByInfraConfigIdAsync(configId);

        // Assert
        result.Should().HaveCount(1);
        result.Single().Id.Should().Be(owned.Id);
    }

    [Fact]
    public async Task Given_StoredGroup_When_GetByIdWithResourcesAsync_Then_ReturnsGroup_Async()
    {
        // Arrange
        var group = NewGroup(InfrastructureConfigId.CreateUnique());
        await _context.ResourceGroups.AddAsync(group);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetByIdWithResourcesAsync(group.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(group.Id);
    }

    [Fact]
    public async Task Given_NoConfigs_When_GetResourceCountsByInfraConfigIdsAsync_Then_ReturnsEmpty_Async()
    {
        // Act
        var result = await _sut.GetResourceCountsByInfraConfigIdsAsync([]);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Given_StoredGroups_When_GetResourceCountsByInfraConfigIdsAsync_Then_ReturnsCounts_Async()
    {
        // Arrange
        var configId = InfrastructureConfigId.CreateUnique();
        await _context.ResourceGroups.AddRangeAsync(
            NewGroup(configId),
            NewGroup(configId, OtherGroupName));
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetResourceCountsByInfraConfigIdsAsync([configId]);

        // Assert
        result.Should().ContainKey(configId.Value);
        result[configId.Value].ResourceGroupCount.Should().Be(2);
        result[configId.Value].ResourceCount.Should().Be(0);
    }
}
