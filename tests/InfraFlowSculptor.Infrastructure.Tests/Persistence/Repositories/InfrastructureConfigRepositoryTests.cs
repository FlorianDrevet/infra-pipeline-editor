using FluentAssertions;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Infrastructure.Persistence;
using InfraFlowSculptor.Infrastructure.Persistence.Repositories;
using Xunit;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Infrastructure.Tests.Persistence.Repositories;

public sealed class InfrastructureConfigRepositoryTests : IDisposable
{
    private const string ConfigName = "shared-prod";
    private const string OtherConfigName = "shared-dev";

    private readonly ProjectDbContext _context;
    private readonly InfrastructureConfigRepository _sut;

    public InfrastructureConfigRepositoryTests()
    {
        _context = InMemoryDbContextFactory.Create();
        _sut = new InfrastructureConfigRepository(_context);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task Given_StoredConfig_When_GetByIdAsync_Then_ReturnsConfig_Async()
    {
        // Arrange
        var config = InfrastructureConfig.Create(new Name(ConfigName), ProjectId.CreateUnique());
        await _context.InfrastructureConfigs.AddAsync(config);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetByIdAsync(config.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(config.Id);
    }

    [Fact]
    public async Task Given_UnknownId_When_GetByIdAsync_Then_ReturnsNull_Async()
    {
        // Act
        var result = await _sut.GetByIdAsync(InfrastructureConfigId.CreateUnique());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Given_AddedConfig_When_AddAsync_Then_PersistsConfig_Async()
    {
        // Arrange
        var config = InfrastructureConfig.Create(new Name(ConfigName), ProjectId.CreateUnique());

        // Act
        await _sut.AddAsync(config);
        await _context.SaveChangesAsync();

        // Assert
        var stored = await _sut.GetByIdAsync(config.Id);
        stored.Should().NotBeNull();
    }

    [Fact]
    public async Task Given_StoredConfig_When_DeleteAsync_Then_RemovesConfig_Async()
    {
        // Arrange
        var config = InfrastructureConfig.Create(new Name(ConfigName), ProjectId.CreateUnique());
        await _context.InfrastructureConfigs.AddAsync(config);
        await _context.SaveChangesAsync();

        // Act
        var deleted = await _sut.DeleteAsync(config.Id);
        await _context.SaveChangesAsync();

        // Assert
        deleted.Should().BeTrue();
        (await _sut.GetByIdAsync(config.Id)).Should().BeNull();
    }

    [Fact]
    public async Task Given_UnknownId_When_DeleteAsync_Then_ReturnsFalse_Async()
    {
        // Act
        var deleted = await _sut.DeleteAsync(InfrastructureConfigId.CreateUnique());

        // Assert
        deleted.Should().BeFalse();
    }

    [Fact]
    public async Task Given_StoredConfig_When_GetByIdWithMembersAsync_Then_ReturnsConfig_Async()
    {
        // Arrange
        var config = InfrastructureConfig.Create(new Name(ConfigName), ProjectId.CreateUnique());
        await _context.InfrastructureConfigs.AddAsync(config);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetByIdWithMembersAsync(config.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(config.Id);
    }

    [Fact]
    public async Task Given_StoredConfigsForProject_When_GetByProjectIdAsync_Then_ReturnsOnlyMatching_Async()
    {
        // Arrange
        var projectId = ProjectId.CreateUnique();
        var otherProjectId = ProjectId.CreateUnique();
        var owned = InfrastructureConfig.Create(new Name(ConfigName), projectId);
        var unrelated = InfrastructureConfig.Create(new Name(OtherConfigName), otherProjectId);

        await _context.InfrastructureConfigs.AddRangeAsync(owned, unrelated);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetByProjectIdAsync(projectId);

        // Assert
        result.Should().HaveCount(1);
        result.Single().Id.Should().Be(owned.Id);
    }

    [Fact]
    public async Task Given_StoredConfig_When_GetByIdWithNamingTemplatesAsync_Then_ReturnsConfig_Async()
    {
        // Arrange
        var config = InfrastructureConfig.Create(new Name(ConfigName), ProjectId.CreateUnique());
        await _context.InfrastructureConfigs.AddAsync(config);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetByIdWithNamingTemplatesAsync(config.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(config.Id);
    }
}
