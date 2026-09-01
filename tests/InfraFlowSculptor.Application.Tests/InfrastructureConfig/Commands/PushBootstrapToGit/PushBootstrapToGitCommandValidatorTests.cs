using FluentAssertions;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.PushBootstrapToGit;

namespace InfraFlowSculptor.Application.Tests.InfrastructureConfig.Commands.PushBootstrapToGit;

public sealed class PushBootstrapToGitCommandValidatorTests
{
    private readonly PushBootstrapToGitCommandValidator _sut = new();

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        var command = new PushBootstrapToGitCommand(Guid.NewGuid(), "main", "Add bootstrap");

        var result = _sut.Validate(command);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_EmptyInfrastructureConfigId_When_Validate_Then_Fails()
    {
        var command = new PushBootstrapToGitCommand(Guid.Empty, "main", "commit");

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(PushBootstrapToGitCommand.InfrastructureConfigId));
    }

    [Fact]
    public void Given_EmptyBranchName_When_Validate_Then_Fails()
    {
        var command = new PushBootstrapToGitCommand(Guid.NewGuid(), "", "commit");

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(PushBootstrapToGitCommand.BranchName));
    }

    [Fact]
    public void Given_BranchNameTooLong_When_Validate_Then_Fails()
    {
        var command = new PushBootstrapToGitCommand(Guid.NewGuid(), new string('a', 251), "commit");

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(PushBootstrapToGitCommand.BranchName));
    }

    [Fact]
    public void Given_EmptyCommitMessage_When_Validate_Then_Fails()
    {
        var command = new PushBootstrapToGitCommand(Guid.NewGuid(), "main", "");

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(PushBootstrapToGitCommand.CommitMessage));
    }

    [Fact]
    public void Given_CommitMessageTooLong_When_Validate_Then_Fails()
    {
        var command = new PushBootstrapToGitCommand(Guid.NewGuid(), "main", new string('m', 501));

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(PushBootstrapToGitCommand.CommitMessage));
    }
}
