using FluentAssertions;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.PushBicepToGit;

namespace InfraFlowSculptor.Application.Tests.InfrastructureConfig.Commands.PushBicepToGit;

public sealed class PushBicepToGitCommandValidatorTests
{
    private readonly PushBicepToGitCommandValidator _sut = new();

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        var command = new PushBicepToGitCommand(Guid.NewGuid(), "feature/my-branch", "Initial commit");

        var result = _sut.Validate(command);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_EmptyInfrastructureConfigId_When_Validate_Then_Fails()
    {
        var command = new PushBicepToGitCommand(Guid.Empty, "main", "commit");

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(PushBicepToGitCommand.InfrastructureConfigId));
    }

    [Fact]
    public void Given_EmptyBranchName_When_Validate_Then_Fails()
    {
        var command = new PushBicepToGitCommand(Guid.NewGuid(), "", "commit");

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(PushBicepToGitCommand.BranchName));
    }

    [Fact]
    public void Given_BranchNameTooLong_When_Validate_Then_Fails()
    {
        var command = new PushBicepToGitCommand(Guid.NewGuid(), new string('a', 201), "commit");

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(PushBicepToGitCommand.BranchName));
    }

    [Fact]
    public void Given_BranchNameWithInvalidChars_When_Validate_Then_Fails()
    {
        var command = new PushBicepToGitCommand(Guid.NewGuid(), "feature/my branch!", "commit");

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(PushBicepToGitCommand.BranchName));
    }

    [Fact]
    public void Given_EmptyCommitMessage_When_Validate_Then_Fails()
    {
        var command = new PushBicepToGitCommand(Guid.NewGuid(), "main", "");

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(PushBicepToGitCommand.CommitMessage));
    }

    [Fact]
    public void Given_CommitMessageTooLong_When_Validate_Then_Fails()
    {
        var command = new PushBicepToGitCommand(Guid.NewGuid(), "main", new string('m', 501));

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(PushBicepToGitCommand.CommitMessage));
    }
}
