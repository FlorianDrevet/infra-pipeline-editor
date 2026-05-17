using ErrorOr;
using FluentAssertions;
using FluentValidation;
using InfraFlowSculptor.Application.Common.Behaviors;
using InfraFlowSculptor.Application.Common.Interfaces;

namespace InfraFlowSculptor.Application.Tests.Common.Behaviors;

public sealed class ValidationBehaviorTests
{
    [Fact]
    public async Task Given_InvalidCommand_When_Handle_Then_ReturnsValidationErrorsWithoutCallingNextAsync()
    {
        // Arrange
        var validator = new TestCommandValidator();
        var sut = new ValidationBehavior<TestCommand, ErrorOr<string>>(validator);
        var request = new TestCommand(string.Empty);
        var nextCalled = false;

        // Act
        var result = await sut.Handle(
            request,
            _ =>
            {
                nextCalled = true;
                return Task.FromResult<ErrorOr<string>>("unexpected");
            },
            CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Validation);
        nextCalled.Should().BeFalse();
    }

    [Fact]
    public async Task Given_InvalidQuery_When_Handle_Then_SkipsValidationAndCallsNextAsync()
    {
        // Arrange
        var validator = new TestQueryValidator();
        var sut = new ValidationBehavior<TestQuery, ErrorOr<string>>(validator);
        var request = new TestQuery(string.Empty);
        var nextCalled = false;

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

    private sealed record TestCommand(string Name) : ICommand<string>;

    private sealed record TestQuery(string Name) : IQuery<string>;

    private sealed class TestCommandValidator : AbstractValidator<TestCommand>
    {
        public TestCommandValidator()
        {
            RuleFor(command => command.Name).NotEmpty();
        }
    }

    private sealed class TestQueryValidator : AbstractValidator<TestQuery>
    {
        public TestQueryValidator()
        {
            RuleFor(query => query.Name).NotEmpty();
        }
    }
}
