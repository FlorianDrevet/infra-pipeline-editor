using FluentAssertions;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using InfraFlowSculptor.Domain.WebAppAggregate;
using InfraFlowSculptor.Domain.WebAppAggregate.ValueObjects;
using InfraFlowSculptor.Infrastructure.Persistence;
using InfraFlowSculptor.Infrastructure.Persistence.Repositories;
using Xunit;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Infrastructure.Tests.Persistence.Repositories;

public sealed class BaseRepositoryTests : IDisposable
{
    private const string DefaultName = "wa-base-test";
    private const string OtherName = "wa-base-other";
    private const string RuntimeVersion = "8.0";

    private readonly ProjectDbContext _context;
    private readonly AzureResourceRepository<WebApp> _sut;

    public BaseRepositoryTests()
    {
        _context = InMemoryDbContextFactory.Create();
        _sut = new WebAppRepository(_context);
    }

    public void Dispose() => _context.Dispose();

    private static WebApp NewEntity(ResourceGroupId resourceGroupId, string name = DefaultName)
        => WebApp.Create(
            resourceGroupId,
            new Name(name),
            new Location(Location.LocationEnum.WestEurope),
            AzureResourceId.CreateUnique(),
            new WebAppRuntimeStack(WebAppRuntimeStack.WebAppRuntimeStackEnum.DotNet),
            RuntimeVersion,
            alwaysOn: true,
            httpsOnly: true,
            new DeploymentMode(DeploymentMode.DeploymentModeType.Code),
            containerRegistryId: null,
            acrAuthMode: null);

    [Fact]
    public async Task Given_NewEntity_When_Add_Then_PersistsEntity_Async()
    {
        // Arrange
        var entity = NewEntity(ResourceGroupId.CreateUnique());

        // Act
        var added = _sut.Add(entity);
        await _context.SaveChangesAsync();

        // Assert
        added.Should().NotBeNull();
        added.Id.Should().Be(entity.Id);

        var stored = await _context.Set<WebApp>().FindAsync(entity.Id);
        stored.Should().NotBeNull();
    }

    [Fact]
    public async Task Given_StoredEntity_When_DeleteAsync_Then_RemovesEntity_Async()
    {
        // Arrange
        var entity = NewEntity(ResourceGroupId.CreateUnique());
        _context.Set<WebApp>().Add(entity);
        await _context.SaveChangesAsync();

        // Act
        var deleted = await _sut.DeleteAsync(entity.Id);
        await _context.SaveChangesAsync();

        // Assert
        deleted.Should().BeTrue();
        (await _context.Set<WebApp>().FindAsync(entity.Id)).Should().BeNull();
    }

    [Fact]
    public async Task Given_UnknownId_When_DeleteAsync_Then_ReturnsFalse_Async()
    {
        // Act
        var deleted = await _sut.DeleteAsync(AzureResourceId.CreateUnique());

        // Assert
        deleted.Should().BeFalse();
    }

    [Fact]
    public async Task Given_StoredEntity_When_GetByIdAsync_Then_ReturnsEntity_Async()
    {
        // Arrange
        var entity = NewEntity(ResourceGroupId.CreateUnique());
        _context.Set<WebApp>().Add(entity);
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
    public async Task Given_StoredEntity_When_GetByIdReadOnlyAsync_Then_ReturnsEntity_Async()
    {
        // Arrange
        var resourceGroup = ResourceGroup.Create(
            new Name("rg-readonly"),
            InfrastructureConfigId.CreateUnique(),
            new Location(Location.LocationEnum.WestEurope));
        _context.ResourceGroups.Add(resourceGroup);
        var entity = NewEntity(resourceGroup.Id);
        _context.Set<WebApp>().Add(entity);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetByIdReadOnlyAsync(entity.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(entity.Id);
    }

    [Fact]
    public async Task Given_StoredEntities_When_GetAllAsync_Then_ReturnsAll_Async()
    {
        // Arrange
        var entity1 = NewEntity(ResourceGroupId.CreateUnique());
        var entity2 = NewEntity(ResourceGroupId.CreateUnique(), OtherName);
        _context.Set<WebApp>().AddRange(entity1, entity2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Given_EmptyStore_When_GetAllAsync_Then_ReturnsEmpty_Async()
    {
        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }
}
