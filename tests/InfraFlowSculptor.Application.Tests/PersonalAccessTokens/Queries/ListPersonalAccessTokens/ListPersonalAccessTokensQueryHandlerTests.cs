using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.PersonalAccessTokens.Queries.ListPersonalAccessTokens;
using InfraFlowSculptor.Domain.PersonalAccessTokenAggregate;
using InfraFlowSculptor.Domain.PersonalAccessTokenAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using NSubstitute;

namespace InfraFlowSculptor.Application.Tests.PersonalAccessTokens.Queries.ListPersonalAccessTokens;

public sealed class ListPersonalAccessTokensQueryHandlerTests
{
    private readonly IPersonalAccessTokenRepository _repository;
    private readonly ICurrentUser _currentUser;
    private readonly UserId _userId;
    private readonly ListPersonalAccessTokensQueryHandler _sut;

    public ListPersonalAccessTokensQueryHandlerTests()
    {
        _repository = Substitute.For<IPersonalAccessTokenRepository>();
        _currentUser = Substitute.For<ICurrentUser>();
        _userId = UserId.CreateUnique();
        _currentUser.GetUserIdAsync(Arg.Any<CancellationToken>()).Returns(_userId);
        _sut = new ListPersonalAccessTokensQueryHandler(_repository, _currentUser);
    }

    [Fact]
    public async Task Given_UserHasTokens_When_Handle_Then_ReturnsTokenListAsync()
    {
        // Arrange
        var (token1, _) = PersonalAccessToken.Create(_userId, "Token1", DateTime.UtcNow.AddDays(30),
            [new PatScope(PatScopeType.Read)]);
        var (token2, _) = PersonalAccessToken.Create(_userId, "Token2", null,
            [new PatScope(PatScopeType.Write)]);

        _repository.GetByUserIdAsync(_userId, Arg.Any<CancellationToken>())
            .Returns(new List<PersonalAccessToken> { token1, token2 });
        var query = new ListPersonalAccessTokensQuery();

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().HaveCount(2);
        result.Value[0].Name.Should().Be("Token1");
        result.Value[1].Name.Should().Be("Token2");
    }

    [Fact]
    public async Task Given_UserHasNoTokens_When_Handle_Then_ReturnsEmptyListAsync()
    {
        // Arrange
        _repository.GetByUserIdAsync(_userId, Arg.Any<CancellationToken>())
            .Returns(new List<PersonalAccessToken>());
        var query = new ListPersonalAccessTokensQuery();

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Given_TokenWithScopes_When_Handle_Then_ReturnsScopeNamesAsync()
    {
        // Arrange
        var (token, _) = PersonalAccessToken.Create(_userId, "ScopedToken", null,
            [new PatScope(PatScopeType.Read), new PatScope(PatScopeType.Write)]);

        _repository.GetByUserIdAsync(_userId, Arg.Any<CancellationToken>())
            .Returns(new List<PersonalAccessToken> { token });
        var query = new ListPersonalAccessTokensQuery();

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value[0].Scopes.Should().Contain("Read");
        result.Value[0].Scopes.Should().Contain("Write");
    }
}
