using System.Security.Claims;
using System.Text.Encodings.Web;
using FluentAssertions;
using InfraFlowSculptor.Domain.PersonalAccessTokenAggregate;
using InfraFlowSculptor.Domain.PersonalAccessTokenAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using InfraFlowSculptor.Infrastructure.Auth;
using InfraFlowSculptor.Infrastructure.Persistence;
using InfraFlowSculptor.Infrastructure.Persistence.Repositories;
using InfraFlowSculptor.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace InfraFlowSculptor.Infrastructure.Tests.Auth;

public sealed class PersonalAccessTokenAuthenticationHandlerTests : IAsyncDisposable
{
    private const string AuthorizationHeaderName = "Authorization";
    private const string BearerSchemePrefix = "Bearer ";
    private const string PersonalAccessTokenName = "automation";

    private readonly CountingProjectDbContext _dbContext;
    private readonly PersonalAccessTokenRepository _repository;

    public PersonalAccessTokenAuthenticationHandlerTests()
    {
        _dbContext = CountingProjectDbContext.Create();
        _repository = new PersonalAccessTokenRepository(_dbContext);
    }

    public ValueTask DisposeAsync() => _dbContext.DisposeAsync();

    [Fact]
    public async Task Given_NeverUsedPat_When_Authenticate_Then_PersistsUsageAndResolvesCurrentUser_Async()
    {
        // Arrange
        var (token, plainTextToken) = PersonalAccessToken.Create(UserId.CreateUnique(), PersonalAccessTokenName, expiresAt: null);
        await SeedTokenAsync(token);
        var httpContext = CreateHttpContext(plainTextToken);

        // Act
        var result = await AuthenticateAsync(httpContext);
        httpContext.User = result.Principal!;

        // Assert
        result.Succeeded.Should().BeTrue();
        _dbContext.SaveChangesCallCount.Should().Be(1);
        token.LastUsedAt.Should().NotBeNull();
        await AssertCurrentUserResolutionAsync(httpContext, token.UserId);
        result.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value.Should().Be(token.UserId.Value.ToString());
    }

    [Fact]
    public async Task Given_RecentlyUsedPat_When_Authenticate_Then_DoesNotPersistUsageAgain_Async()
    {
        // Arrange
        var recentLastUsedAt = DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(1));
        var (token, plainTextToken) = PersonalAccessToken.Create(UserId.CreateUnique(), PersonalAccessTokenName, expiresAt: null);
        await SeedTokenAsync(token, recentLastUsedAt);
        var httpContext = CreateHttpContext(plainTextToken);

        // Act
        var result = await AuthenticateAsync(httpContext);
        httpContext.User = result.Principal!;

        // Assert
        result.Succeeded.Should().BeTrue();
        _dbContext.SaveChangesCallCount.Should().Be(0);
        token.LastUsedAt.Should().Be(recentLastUsedAt);
        await AssertCurrentUserResolutionAsync(httpContext, token.UserId);
    }

    [Fact]
    public async Task Given_StalePatUsage_When_Authenticate_Then_PersistsUsageAgain_Async()
    {
        // Arrange
        var staleLastUsedAt = DateTime.UtcNow
            .Subtract(PersonalAccessTokenAuthenticationDefaults.UsagePersistenceInterval)
            .Subtract(TimeSpan.FromMinutes(1));
        var (token, plainTextToken) = PersonalAccessToken.Create(UserId.CreateUnique(), PersonalAccessTokenName, expiresAt: null);
        await SeedTokenAsync(token, staleLastUsedAt);
        var httpContext = CreateHttpContext(plainTextToken);

        // Act
        var result = await AuthenticateAsync(httpContext);

        // Assert
        result.Succeeded.Should().BeTrue();
        _dbContext.SaveChangesCallCount.Should().Be(1);
        token.LastUsedAt.Should().NotBe(staleLastUsedAt);
        await AssertCurrentUserResolutionAsync(httpContext, token.UserId);
    }

    [Fact]
    public async Task Given_PatWithGrantedScopes_When_Authenticate_Then_EmitsScopeClaimsAndResolvesThemFromCurrentUserAsync()
    {
        // Arrange
        var scopes = new[]
        {
            new PatScope(PatScopeType.Read),
            new PatScope(PatScopeType.Generate),
        };
        var (token, plainTextToken) = PersonalAccessToken.Create(
            UserId.CreateUnique(),
            PersonalAccessTokenName,
            expiresAt: null,
            scopes);
        await SeedTokenAsync(token);
        var httpContext = CreateHttpContext(plainTextToken);

        // Act
        var result = await AuthenticateAsync(httpContext);
        httpContext.User = result.Principal!;

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Principal?.FindAll(PersonalAccessTokenClaimNames.Scope)
            .Select(claim => claim.Value)
            .Should()
            .BeEquivalentTo(new[] { nameof(PatScopeType.Read), nameof(PatScopeType.Generate) });

        var currentUser = new CurrentUser(new HttpContextAccessor { HttpContext = httpContext });

        (await currentUser.HasPersonalAccessTokenScopeAsync(PatScopeType.Read)).Should().BeTrue();
        (await currentUser.HasPersonalAccessTokenScopeAsync(PatScopeType.Write)).Should().BeFalse();
        (await currentUser.HasPersonalAccessTokenScopeAsync(PatScopeType.Generate)).Should().BeTrue();
    }

    private static DefaultHttpContext CreateHttpContext(string plainTextToken)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers[AuthorizationHeaderName] = $"{BearerSchemePrefix}{plainTextToken}";
        return httpContext;
    }

    private async Task<AuthenticateResult> AuthenticateAsync(HttpContext httpContext)
    {
        var options = Substitute.For<IOptionsMonitor<AuthenticationSchemeOptions>>();
        options.Get(Arg.Any<string>()).Returns(new AuthenticationSchemeOptions());

        var sut = new PersonalAccessTokenAuthenticationHandler(
            options,
            NullLoggerFactory.Instance,
            UrlEncoder.Default,
            _repository,
            _dbContext);

        var scheme = new AuthenticationScheme(
            PersonalAccessTokenAuthenticationDefaults.AuthenticationScheme,
            displayName: null,
            typeof(PersonalAccessTokenAuthenticationHandler));

        await sut.InitializeAsync(scheme, httpContext);
        return await sut.AuthenticateAsync();
    }

    private async Task SeedTokenAsync(PersonalAccessToken token, DateTime? lastUsedAt = null)
    {
        _dbContext.PersonalAccessTokens.Add(token);
        await _dbContext.SaveChangesAsync();

        if (lastUsedAt.HasValue)
        {
            _dbContext.Entry(token).Property(pat => pat.LastUsedAt).CurrentValue = lastUsedAt.Value;
            await _dbContext.SaveChangesAsync();
        }

        _dbContext.ResetSaveChangesCallCount();
    }

    private static async Task AssertCurrentUserResolutionAsync(HttpContext httpContext, UserId expectedUserId)
    {
        var currentUser = new CurrentUser(new HttpContextAccessor { HttpContext = httpContext });

        var resolvedUserId = await currentUser.GetUserIdAsync();

        resolvedUserId.Should().Be(expectedUserId);
    }

    private sealed class CountingProjectDbContext(DbContextOptions<ProjectDbContext> options)
        : ProjectDbContext(options)
    {
        public int SaveChangesCallCount { get; private set; }

        public static CountingProjectDbContext Create()
        {
            var options = new DbContextOptionsBuilder<ProjectDbContext>()
                .UseInMemoryDatabase($"pat-auth-handler-tests-{Guid.NewGuid()}")
                .ConfigureWarnings(builder =>
                    builder.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            return new CountingProjectDbContext(options);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveChangesCallCount++;
            return await base.SaveChangesAsync(cancellationToken);
        }

        public void ResetSaveChangesCallCount()
        {
            SaveChangesCallCount = 0;
        }
    }
}
