using FluentAssertions;
using InfraFlowSculptor.Application.Projects.Commands.PushProjectGeneratedArtifactsToGit;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.Projects.Commands.PushProjectGeneratedArtifactsToGit;

public sealed class PushProjectGeneratedArtifactsToGitCommandValidatorTests
{
    private const string ValidBranchName = "feature/generated-artifacts";
    private const string ValidCommitMessage = "Push generated artifacts";
    private const string ProjectIdProperty = "ProjectId.Value";
    private const string BranchNameProperty = nameof(PushProjectGeneratedArtifactsToGitCommand.BranchName);
    private const string CommitMessageProperty = nameof(PushProjectGeneratedArtifactsToGitCommand.CommitMessage);

    private readonly PushProjectGeneratedArtifactsToGitCommandValidator _sut = new();

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
        result.Errors.Should().Contain(error => error.PropertyName == ProjectIdProperty);
    }

    [Theory]
    [InlineData("")]
    [InlineData("feature invalid")]
    public void Given_InvalidBranchName_When_Validate_Then_FailsOnBranchName(string branchName)
    {
        // Arrange
        var command = CreateCommand(branchName: branchName);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == BranchNameProperty);
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
        result.Errors.Should().Contain(error => error.PropertyName == CommitMessageProperty);
    }

    private static PushProjectGeneratedArtifactsToGitCommand CreateCommand(
        ProjectId? projectId = null,
        string branchName = ValidBranchName,
        string commitMessage = ValidCommitMessage)
    {
        return new PushProjectGeneratedArtifactsToGitCommand(
            projectId ?? new ProjectId(Guid.NewGuid()),
            branchName,
            commitMessage);
    }
}
