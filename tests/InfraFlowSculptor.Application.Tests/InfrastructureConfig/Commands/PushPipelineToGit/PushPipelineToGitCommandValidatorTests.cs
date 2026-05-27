using FluentAssertions;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.PushPipelineToGit;

namespace InfraFlowSculptor.Application.Tests.InfrastructureConfig.Commands.PushPipelineToGit;

public sealed class PushPipelineToGitCommandValidatorTests
{
    private readonly PushPipelineToGitCommandValidator _sut = new();

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        var command = new PushPipelineToGitCommand(Guid.NewGuid(), "main", "Add pipeline");

        var result = _sut.Validate(command);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_EmptyInfrastructureConfigId_When_Validate_Then_Fails()
    {
        var command = new PushPipelineToGitCommand(Guid.Empty, "main", "commit");

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(PushPipelineToGitCommand.InfrastructureConfigId));
    }

    [Fact]
    public void Given_EmptyBranchName_When_Validate_Then_Fails()
    {
        var command = new PushPipelineToGitCommand(Guid.NewGuid(), "", "commit");

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(PushPipelineToGitCommand.BranchName));
    }

    [Fact]
    public void Given_BranchNameTooLong_When_Validate_Then_Fails()
    {
        var command = new PushPipelineToGitCommand(Guid.NewGuid(), new string('a', 251), "commit");

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(PushPipelineToGitCommand.BranchName));
    }

    [Fact]
    public void Given_EmptyCommitMessage_When_Validate_Then_Fails()
    {
        var command = new PushPipelineToGitCommand(Guid.NewGuid(), "main", "");

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(PushPipelineToGitCommand.CommitMessage));
    }

    [Fact]
    public void Given_CommitMessageTooLong_When_Validate_Then_Fails()
    {
        var command = new PushPipelineToGitCommand(Guid.NewGuid(), "main", new string('m', 501));

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(PushPipelineToGitCommand.CommitMessage));
    }
}
