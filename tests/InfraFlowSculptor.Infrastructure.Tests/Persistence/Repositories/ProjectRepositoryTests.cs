using FluentAssertions;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using InfraFlowSculptor.Infrastructure.Persistence;
using InfraFlowSculptor.Infrastructure.Persistence.Repositories;
using Xunit;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Infrastructure.Tests.Persistence.Repositories;

public sealed class ProjectRepositoryTests : IDisposable
{
    private const string ProjectName = "alpha";
    private const string ProjectDescription = "primary workload";
    private const string UserJoinSkipReason =
        "EF Core InMemory provider cannot translate queries that materialize User (ComplexProperty Name).";

    private readonly ProjectDbContext _context;
    private readonly ProjectRepository _sut;

    public ProjectRepositoryTests()
    {
        _context = InMemoryDbContextFactory.Create();
        _sut = new ProjectRepository(_context);
    }

    public void Dispose() => _context.Dispose();

    private static Project NewProject(UserId? ownerId = null)
        => Project.Create(new Name(ProjectName), ProjectDescription, ownerId ?? UserId.CreateUnique());

    [Fact]
    public async Task Given_StoredProject_When_GetByIdAsync_Then_ReturnsProject_Async()
    {
        // Arrange
        var project = NewProject();
        await _context.Projects.AddAsync(project);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetByIdAsync(project.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(project.Id);
    }

    [Fact]
    public async Task Given_UnknownId_When_GetByIdAsync_Then_ReturnsNull_Async()
    {
        // Act
        var result = await _sut.GetByIdAsync(ProjectId.CreateUnique());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Given_AddedProject_When_AddAsync_Then_PersistsProject_Async()
    {
        // Arrange
        var project = NewProject();

        // Act
        await _sut.AddAsync(project);
        await _context.SaveChangesAsync();

        // Assert
        var stored = await _sut.GetByIdAsync(project.Id);
        stored.Should().NotBeNull();
    }

    [Fact]
    public async Task Given_StoredProject_When_DeleteAsync_Then_RemovesProject_Async()
    {
        // Arrange
        var project = NewProject();
        await _context.Projects.AddAsync(project);
        await _context.SaveChangesAsync();

        // Act
        var deleted = await _sut.DeleteAsync(project.Id);
        await _context.SaveChangesAsync();

        // Assert
        deleted.Should().BeTrue();
        (await _sut.GetByIdAsync(project.Id)).Should().BeNull();
    }

    [Fact(Skip = UserJoinSkipReason)]
    public Task Given_StoredProject_When_GetByIdWithMembersAsync_Then_LoadsMembers_Async() => Task.CompletedTask;

    [Fact(Skip = UserJoinSkipReason)]
    public Task Given_StoredProject_When_GetByIdWithAllAsync_Then_ReturnsProject_Async() => Task.CompletedTask;

    [Fact]
    public async Task Given_StoredProject_When_GetByIdWithPipelineVariableGroupsAsync_Then_ReturnsProject_Async()
    {
        // Arrange
        var project = NewProject();
        await _context.Projects.AddAsync(project);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetByIdWithPipelineVariableGroupsAsync(project.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(project.Id);
    }

    [Fact(Skip = UserJoinSkipReason)]
    public Task Given_ProjectsForUser_When_GetAllForUserAsync_Then_ReturnsOnlyOwned_Async() => Task.CompletedTask;

    [Fact]
    public async Task Given_ProjectsForUser_When_GetProjectIdsForUserAsync_Then_ReturnsOnlyOwnedIds_Async()
    {
        // Arrange
        var ownerId = UserId.CreateUnique();
        var owned = NewProject(ownerId);
        var unrelated = NewProject();
        await _context.Projects.AddRangeAsync(owned, unrelated);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetProjectIdsForUserAsync(ownerId);

        // Assert
        result.Should().ContainSingle(id => id == owned.Id);
    }

    [Fact]
    public async Task Given_NoVariableGroups_When_GetPipelineVariableUsagesAsync_Then_ReturnsEmpty_Async()
    {
        // Act
        var result = await _sut.GetPipelineVariableUsagesAsync([]);

        // Assert
        result.Should().BeEmpty();
    }
}
