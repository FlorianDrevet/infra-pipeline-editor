using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Behaviors;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Domain.PersonalAccessTokenAggregate.ValueObjects;
using NSubstitute;

namespace InfraFlowSculptor.Application.Tests.Common.Behaviors;

public sealed class PersonalAccessTokenScopeBehaviorTests
{
    private readonly ICurrentUser _currentUser;

    public PersonalAccessTokenScopeBehaviorTests()
    {
        _currentUser = Substitute.For<ICurrentUser>();
    }

    [Fact]
    public async Task Given_QueryPatMissingReadAndWriteScopes_When_Handle_Then_ReturnsForbiddenWithoutCallingNextAsync()
    {
        // Arrange
        var sut = new PersonalAccessTokenScopeBehavior<TestQuery, ErrorOr<string>>(_currentUser);
        var request = new TestQuery("alpha");
        var nextCalled = false;

        _currentUser.HasPersonalAccessTokenScopeAsync(PatScopeType.Read, Arg.Any<CancellationToken>())
            .Returns(false);
        _currentUser.HasPersonalAccessTokenScopeAsync(PatScopeType.Write, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await sut.Handle(
            request,
            _ =>
            {
                nextCalled = true;
                return Task.FromResult<ErrorOr<string>>("query-result");
            },
            CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Forbidden);
        nextCalled.Should().BeFalse();
    }

    [Fact]
    public async Task Given_QueryPatWithWriteScope_When_Handle_Then_AllowsNextAsync()
    {
        // Arrange
        var sut = new PersonalAccessTokenScopeBehavior<TestQuery, ErrorOr<string>>(_currentUser);
        var request = new TestQuery("alpha");
        var nextCalled = false;

        _currentUser.HasPersonalAccessTokenScopeAsync(PatScopeType.Read, Arg.Any<CancellationToken>())
            .Returns(false);
        _currentUser.HasPersonalAccessTokenScopeAsync(PatScopeType.Write, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await sut.Handle(
            request,
            _ =>
            {
                nextCalled = true;
                return Task.FromResult<ErrorOr<string>>("query-result");
            },
            CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be("query-result");
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Given_CommandPatMissingWriteScope_When_Handle_Then_ReturnsForbiddenWithoutCallingNextAsync()
    {
        // Arrange
        var sut = new PersonalAccessTokenScopeBehavior<TestCommand, ErrorOr<string>>(_currentUser);
        var request = new TestCommand("alpha");
        var nextCalled = false;

        _currentUser.HasPersonalAccessTokenScopeAsync(PatScopeType.Write, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await sut.Handle(
            request,
            _ =>
            {
                nextCalled = true;
                return Task.FromResult<ErrorOr<string>>("command-result");
            },
            CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Forbidden);
        nextCalled.Should().BeFalse();
    }

    [Fact]
    public async Task Given_GenerateCommandPatMissingGenerateScope_When_Handle_Then_ReturnsForbiddenWithoutCallingNextAsync()
    {
        // Arrange
        var sut = new PersonalAccessTokenScopeBehavior<GenerateTestCommand, ErrorOr<string>>(_currentUser);
        var request = new GenerateTestCommand("alpha");
        var nextCalled = false;

        _currentUser.HasPersonalAccessTokenScopeAsync(PatScopeType.Generate, Arg.Any<CancellationToken>())
            .Returns(false);
        _currentUser.HasPersonalAccessTokenScopeAsync(PatScopeType.Write, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await sut.Handle(
            request,
            _ =>
            {
                nextCalled = true;
                return Task.FromResult<ErrorOr<string>>("generate-result");
            },
            CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Forbidden);
        nextCalled.Should().BeFalse();
    }

    private sealed record TestQuery(string Name) : IQuery<string>;

    private sealed record TestCommand(string Name) : ICommand<string>;

    private sealed record GenerateTestCommand(string Name) : IGenerateCommand<string>;
}