using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.PersonalAccessTokens.Commands.CreatePersonalAccessToken;
using InfraFlowSculptor.Domain.PersonalAccessTokenAggregate;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using NSubstitute;

namespace InfraFlowSculptor.Application.Tests.PersonalAccessTokens.Commands.CreatePersonalAccessToken;

public sealed class CreatePersonalAccessTokenCommandHandlerTests
{
    private readonly IPersonalAccessTokenRepository _repository;
    private readonly ICurrentUser _currentUser;
    private readonly UserId _userId;
    private readonly CreatePersonalAccessTokenCommandHandler _sut;

    public CreatePersonalAccessTokenCommandHandlerTests()
    {
        _repository = Substitute.For<IPersonalAccessTokenRepository>();
        _currentUser = Substitute.For<ICurrentUser>();
        _userId = UserId.CreateUnique();
        _currentUser.GetUserIdAsync(Arg.Any<CancellationToken>()).Returns(_userId);
        _sut = new CreatePersonalAccessTokenCommandHandler(_repository, _currentUser);
    }

    [Fact]
    public async Task Given_ValidCommand_When_Handle_Then_CreatesTokenAndReturnsPlainTextAsync()
    {
        // Arrange
        var command = new CreatePersonalAccessTokenCommand("CI Token", DateTime.UtcNow.AddDays(90));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.PlainTextToken.Should().NotBeNullOrWhiteSpace();
        result.Value.Token.Name.Should().Be("CI Token");
        result.Value.Token.UserId.Should().Be(_userId);
        _repository.Received(1).Add(Arg.Any<PersonalAccessToken>());
    }

    [Fact]
    public async Task Given_NullScopes_When_Handle_Then_DefaultsToReadScopeAsync()
    {
        // Arrange
        var command = new CreatePersonalAccessTokenCommand("ReadOnly", null, null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Token.Scopes.Should().Contain("Read");
    }

    [Fact]
    public async Task Given_ExplicitScopes_When_Handle_Then_AssignsRequestedScopesAsync()
    {
        // Arrange
        var command = new CreatePersonalAccessTokenCommand(
            "FullAccess", DateTime.UtcNow.AddDays(30),
            ["Read", "Write", "Generate"]);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Token.Scopes.Should().Contain("Read");
        result.Value.Token.Scopes.Should().Contain("Write");
        result.Value.Token.Scopes.Should().Contain("Generate");
    }
}
