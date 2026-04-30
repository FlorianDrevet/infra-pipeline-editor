using FluentAssertions;
using InfraFlowSculptor.Domain.PersonalAccessTokenAggregate;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using InfraFlowSculptor.Infrastructure.Persistence;
using InfraFlowSculptor.Infrastructure.Persistence.Repositories;
using Xunit;

namespace InfraFlowSculptor.Infrastructure.Tests.Persistence.Repositories;

public sealed class PersonalAccessTokenRepositoryTests : IDisposable
{
    private const string PrimaryTokenName = "primary";
    private const string SecondaryTokenName = "secondary";

    private readonly ProjectDbContext _context;
    private readonly PersonalAccessTokenRepository _sut;

    public PersonalAccessTokenRepositoryTests()
    {
        _context = InMemoryDbContextFactory.Create();
        _sut = new PersonalAccessTokenRepository(_context);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task Given_StoredToken_When_GetByIdAsync_Then_ReturnsToken_Async()
    {
        // Arrange
        var (token, _) = PersonalAccessToken.Create(UserId.CreateUnique(), PrimaryTokenName, expiresAt: null);
        await _context.PersonalAccessTokens.AddAsync(token);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetByIdAsync(token.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(token.Id);
        result.Name.Should().Be(PrimaryTokenName);
    }

    [Fact]
    public async Task Given_AddedToken_When_SaveChanges_Then_PersistsToken_Async()
    {
        // Arrange
        var (token, _) = PersonalAccessToken.Create(UserId.CreateUnique(), PrimaryTokenName, expiresAt: null);

        // Act
        await _sut.AddAsync(token);
        await _context.SaveChangesAsync();

        // Assert
        var stored = await _sut.GetByIdAsync(token.Id);
        stored.Should().NotBeNull();
    }

    [Fact]
    public async Task Given_StoredToken_When_DeleteAsync_Then_RemovesToken_Async()
    {
        // Arrange
        var (token, _) = PersonalAccessToken.Create(UserId.CreateUnique(), PrimaryTokenName, expiresAt: null);
        await _context.PersonalAccessTokens.AddAsync(token);
        await _context.SaveChangesAsync();

        // Act
        var deleted = await _sut.DeleteAsync(token.Id);
        await _context.SaveChangesAsync();

        // Assert
        deleted.Should().BeTrue();
        (await _sut.GetByIdAsync(token.Id)).Should().BeNull();
    }

    [Fact]
    public async Task Given_StoredToken_When_GetByTokenHashAsync_Then_ReturnsMatchingToken_Async()
    {
        // Arrange
        var (token, _) = PersonalAccessToken.Create(UserId.CreateUnique(), PrimaryTokenName, expiresAt: null);
        await _context.PersonalAccessTokens.AddAsync(token);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetByTokenHashAsync(token.TokenHash);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(token.Id);
    }

    [Fact]
    public async Task Given_NoMatch_When_GetByTokenHashAsync_Then_ReturnsNull_Async()
    {
        // Arrange
        var unknownHash = InfraFlowSculptor.Domain.PersonalAccessTokenAggregate.ValueObjects.TokenHash.Compute("ifs_unknown");

        // Act
        var result = await _sut.GetByTokenHashAsync(unknownHash);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Given_TokensForUser_When_GetByUserIdAsync_Then_ReturnsOnlyOwnedTokens_Async()
    {
        // Arrange
        var ownerId = UserId.CreateUnique();
        var otherId = UserId.CreateUnique();
        var (mine, _) = PersonalAccessToken.Create(ownerId, PrimaryTokenName, expiresAt: null);
        var (other, _) = PersonalAccessToken.Create(otherId, SecondaryTokenName, expiresAt: null);

        await _context.PersonalAccessTokens.AddRangeAsync(mine, other);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetByUserIdAsync(ownerId);

        // Assert
        result.Should().HaveCount(1);
        result.Single().Id.Should().Be(mine.Id);
    }

    [Fact]
    public async Task Given_NoTokensForUser_When_GetByUserIdAsync_Then_ReturnsEmpty_Async()
    {
        // Act
        var result = await _sut.GetByUserIdAsync(UserId.CreateUnique());

        // Assert
        result.Should().BeEmpty();
    }
}
