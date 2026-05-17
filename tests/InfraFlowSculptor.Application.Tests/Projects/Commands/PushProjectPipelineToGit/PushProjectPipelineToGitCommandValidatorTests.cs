using FluentAssertions;
using InfraFlowSculptor.Application.Projects.Commands.PushProjectPipelineToGit;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.Projects.Commands.PushProjectPipelineToGit;

public sealed class PushProjectPipelineToGitCommandValidatorTests
{
    private const string ValidBranchName = "feature/pipeline";
    private const string ValidCommitMessage = "Push pipeline files";
    private const string ProjectIdProperty = "ProjectId.Value";
    private const string BranchNameProperty = nameof(PushProjectPipelineToGitCommand.BranchName);
    private const string CommitMessageProperty = nameof(PushProjectPipelineToGitCommand.CommitMessage);

    private readonly PushProjectPipelineToGitCommandValidator _sut = new();

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = CreateCommand();

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_EmptyProjectId_When_Validate_Then_FailsOnProjectId()
    {
        // Arrange
        var command = CreateCommand(projectId: new ProjectId(Guid.Empty));

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == ProjectIdProperty);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Given_EmptyBranchName_When_Validate_Then_FailsOnBranchName(string? branchName)
    {
        // Arrange
        var command = CreateCommand(branchName: branchName!);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == BranchNameProperty);
    }

    [Fact]
    public void Given_BranchNameLongerThan200Characters_When_Validate_Then_FailsOnBranchName()
    {
        // Arrange
        var command = CreateCommand(branchName: new string('a', 201));

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == BranchNameProperty);
    }

    [Theory]
    [InlineData("feature invalid")]
    [InlineData("branch@name")]
    public void Given_BranchNameWithInvalidChars_When_Validate_Then_FailsOnBranchName(string branchName)
    {
        // Arrange
        var command = CreateCommand(branchName: branchName);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == BranchNameProperty);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Given_EmptyCommitMessage_When_Validate_Then_FailsOnCommitMessage(string? commitMessage)
    {
        // Arrange
        var command = CreateCommand(commitMessage: commitMessage!);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == CommitMessageProperty);
    }

    [Fact]
    public void Given_CommitMessageLongerThan500Characters_When_Validate_Then_FailsOnCommitMessage()
    {
        // Arrange
        var command = CreateCommand(commitMessage: new string('a', 501));

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == CommitMessageProperty);
    }

    private static PushProjectPipelineToGitCommand CreateCommand(
        ProjectId? projectId = null,
        string branchName = ValidBranchName,
        string commitMessage = ValidCommitMessage)
    {
        return new PushProjectPipelineToGitCommand(
            projectId ?? new ProjectId(Guid.NewGuid()),
            branchName,
            commitMessage);
    }
}
