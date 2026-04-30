using FluentAssertions;
using InfraFlowSculptor.Domain.UserAggregate;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using InfraFlowSculptor.Infrastructure.Persistence;
using InfraFlowSculptor.Infrastructure.Persistence.Repositories;
using Xunit;
using Name = InfraFlowSculptor.Domain.UserAggregate.ValueObjects.Name;

namespace InfraFlowSculptor.Infrastructure.Tests.Persistence.Repositories;

/// <summary>
/// EF Core InMemory provider does not support querying entities with
/// <c>ComplexProperty</c>-mapped value objects (e.g. <see cref="Name"/>).
/// Tests that materialize <see cref="User"/> from the database via a query
/// (FirstOrDefault, ToList, Find on a non-tracked entity) are skipped.
/// </summary>
public sealed class UserRepositoryTests : IDisposable
{
    private const string AliceFirstName = "Alice";
    private const string AliceLastName = "Liddell";
    private const string SkipReason =
        "EF Core InMemory provider cannot translate queries against entities with ComplexProperty value objects (User.Name).";

    private readonly ProjectDbContext _context;
    private readonly UserRepository _sut;

    public UserRepositoryTests()
    {
        _context = InMemoryDbContextFactory.Create();
        _sut = new UserRepository(_context);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task Given_StoredUser_When_GetByIdAsync_Then_ReturnsUserFromChangeTracker_Async()
    {
        // Arrange — entity is added then loaded from change-tracker cache, avoiding query translation.
        var user = User.Create(new EntraId(Guid.NewGuid()), new Name(AliceFirstName, AliceLastName));
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetByIdAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
        result.Name.FirstName.Should().Be(AliceFirstName);
    }

    [Fact]
    public async Task Given_AddedUser_When_AddAsync_Then_PersistsUserId_Async()
    {
        // Arrange
        var user = User.Create(new EntraId(Guid.NewGuid()), new Name(AliceFirstName, AliceLastName));

        // Act
        var added = await _sut.AddAsync(user);
        await _context.SaveChangesAsync();

        // Assert — verify via change-tracker entry rather than a query (InMemory limitation).
        added.Id.Should().Be(user.Id);
        _context.ChangeTracker.Entries<User>().Should().ContainSingle(e => e.Entity.Id == user.Id);
    }

    [Fact(Skip = SkipReason)]
    public Task Given_UnknownId_When_GetByIdAsync_Then_ReturnsNull_Async() => Task.CompletedTask;

    [Fact(Skip = SkipReason)]
    public Task Given_StoredUser_When_DeleteAsync_Then_RemovesUser_Async() => Task.CompletedTask;

    [Fact(Skip = SkipReason)]
    public Task Given_StoredUser_When_GetByEntraIdAsync_Then_ReturnsMatchingUser_Async() => Task.CompletedTask;

    [Fact(Skip = SkipReason)]
    public Task Given_NoMatch_When_GetByEntraIdAsync_Then_ReturnsNull_Async() => Task.CompletedTask;

    [Fact(Skip = SkipReason)]
    public Task Given_StoredUsers_When_GetByIdsAsync_Then_ReturnsRequestedSubset_Async() => Task.CompletedTask;

    [Fact(Skip = SkipReason)]
    public Task Given_StoredUsers_When_GetAllAsync_Then_ReturnsAll_Async() => Task.CompletedTask;
}
