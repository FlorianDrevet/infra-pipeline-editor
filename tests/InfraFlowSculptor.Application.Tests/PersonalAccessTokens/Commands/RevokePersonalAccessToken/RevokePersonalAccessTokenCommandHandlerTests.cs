using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.PersonalAccessTokens.Commands.RevokePersonalAccessToken;
using InfraFlowSculptor.Domain.PersonalAccessTokenAggregate;
using InfraFlowSculptor.Domain.PersonalAccessTokenAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using NSubstitute;

namespace InfraFlowSculptor.Application.Tests.PersonalAccessTokens.Commands.RevokePersonalAccessToken;

public sealed class RevokePersonalAccessTokenCommandHandlerTests
{
    private readonly IPersonalAccessTokenRepository _repository;
    private readonly ICurrentUser _currentUser;
    private readonly UserId _userId;
    private readonly RevokePersonalAccessTokenCommandHandler _sut;

    public RevokePersonalAccessTokenCommandHandlerTests()
    {
        _repository = Substitute.For<IPersonalAccessTokenRepository>();
        _currentUser = Substitute.For<ICurrentUser>();
        _userId = UserId.CreateUnique();
        _currentUser.GetUserIdAsync(Arg.Any<CancellationToken>()).Returns(_userId);
        _sut = new RevokePersonalAccessTokenCommandHandler(_repository, _currentUser);
    }

    [Fact]
    public async Task Given_TokenNotFound_When_Handle_Then_ReturnsNotFoundErrorAsync()
    {
        // Arrange
        var tokenId = PersonalAccessTokenId.CreateUnique();
        var command = new RevokePersonalAccessTokenCommand(tokenId);
        _repository.GetByIdAsync(tokenId, Arg.Any<CancellationToken>())
            .Returns((PersonalAccessToken?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Given_TokenBelongsToDifferentUser_When_Handle_Then_ReturnsNotFoundErrorAsync()
    {
        // Arrange
        var otherUserId = UserId.CreateUnique();
        var (token, _) = PersonalAccessToken.Create(otherUserId, "OtherUserToken", null);
        var command = new RevokePersonalAccessTokenCommand(token.Id);
        _repository.GetByIdAsync(token.Id, Arg.Any<CancellationToken>())
            .Returns(token);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Given_ValidToken_When_Handle_Then_RevokesAndReturnsTrueAsync()
    {
        // Arrange
        var (token, _) = PersonalAccessToken.Create(_userId, "MyToken", null);
        var command = new RevokePersonalAccessTokenCommand(token.Id);
        _repository.GetByIdAsync(token.Id, Arg.Any<CancellationToken>())
            .Returns(token);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeTrue();
        token.IsRevoked.Should().BeTrue();
    }

    [Fact]
    public async Task Given_AlreadyRevokedToken_When_Handle_Then_ReturnsAlreadyRevokedErrorAsync()
    {
        // Arrange
        var (token, _) = PersonalAccessToken.Create(_userId, "MyToken", null);
        token.Revoke();
        var command = new RevokePersonalAccessTokenCommand(token.Id);
        _repository.GetByIdAsync(token.Id, Arg.Any<CancellationToken>())
            .Returns(token);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
    }
}
