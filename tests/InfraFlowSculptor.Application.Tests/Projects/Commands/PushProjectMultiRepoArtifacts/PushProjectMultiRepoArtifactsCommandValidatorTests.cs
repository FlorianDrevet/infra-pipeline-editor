using FluentAssertions;
using InfraFlowSculptor.Application.Projects.Commands.PushProjectMultiRepoArtifacts;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.Projects.Commands.PushProjectMultiRepoArtifacts;

public sealed class PushProjectMultiRepoArtifactsCommandValidatorTests
{
    private const string ValidBranchName = "feature/deploy";
    private const string ValidCommitMessage = "Push artifacts";

    private readonly PushProjectMultiRepoArtifactsCommandValidator _sut = new();

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = CreateValidCommand();

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_EmptyProjectId_When_Validate_Then_Fails()
    {
        // Arrange
        var command = new PushProjectMultiRepoArtifactsCommand(
            new ProjectId(Guid.Empty),
            [CreateValidConfiguration()]);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Given_EmptyConfigurationList_When_Validate_Then_Fails()
    {
        // Arrange
        var command = new PushProjectMultiRepoArtifactsCommand(ProjectId.CreateUnique(), []);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Given_EmptyInfrastructureConfigId_When_Validate_Then_Fails()
    {
        // Arrange
        var configuration = CreateValidConfiguration(new InfrastructureConfigId(Guid.Empty));
        var command = new PushProjectMultiRepoArtifactsCommand(ProjectId.CreateUnique(), [configuration]);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Given_EmptyRepositoryList_When_Validate_Then_Fails()
    {
        // Arrange
        var configuration = new InfrastructureConfigPushTarget(
            InfrastructureConfigId.CreateUnique(),
            []);
        var command = new PushProjectMultiRepoArtifactsCommand(ProjectId.CreateUnique(), [configuration]);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Given_EmptyRepositoryId_When_Validate_Then_Fails()
    {
        // Arrange
        var repository = new ConfigRepositoryPushTarget(
            new InfraConfigRepositoryId(Guid.Empty),
            ValidBranchName,
            ValidCommitMessage);
        var configuration = new InfrastructureConfigPushTarget(
            InfrastructureConfigId.CreateUnique(),
            [repository]);
        var command = new PushProjectMultiRepoArtifactsCommand(ProjectId.CreateUnique(), [configuration]);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Given_EmptyBranchName_When_Validate_Then_Fails()
    {
        // Arrange
        var command = CreateValidCommand(branchName: string.Empty);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Given_BranchNameExceedingExistingLimit_When_Validate_Then_Fails()
    {
        // Arrange
        var command = CreateValidCommand(branchName: new string('a', 201));

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Given_EmptyCommitMessage_When_Validate_Then_Fails()
    {
        // Arrange
        var command = CreateValidCommand(commitMessage: string.Empty);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Given_CommitMessageExceedingExistingLimit_When_Validate_Then_Fails()
    {
        // Arrange
        var command = CreateValidCommand(commitMessage: new string('a', 501));

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Given_InvalidBranchName_When_Validate_Then_Fails()
    {
        // Arrange
        var command = CreateValidCommand(branchName: "feature with spaces");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Given_DuplicateInfrastructureConfigIds_When_Validate_Then_Fails()
    {
        // Arrange
        var infrastructureConfigId = InfrastructureConfigId.CreateUnique();
        var configurations = new[]
        {
            CreateValidConfiguration(infrastructureConfigId),
            CreateValidConfiguration(infrastructureConfigId)
        };
        var command = new PushProjectMultiRepoArtifactsCommand(ProjectId.CreateUnique(), configurations);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Given_DuplicateRepositoryIdsInOneConfiguration_When_Validate_Then_Fails()
    {
        // Arrange
        var repositoryId = InfraConfigRepositoryId.CreateUnique();
        var configuration = new InfrastructureConfigPushTarget(
            InfrastructureConfigId.CreateUnique(),
            [
                new ConfigRepositoryPushTarget(repositoryId, ValidBranchName, ValidCommitMessage),
                new ConfigRepositoryPushTarget(repositoryId, "main", "Push again")
            ]);
        var command = new PushProjectMultiRepoArtifactsCommand(ProjectId.CreateUnique(), [configuration]);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    private static PushProjectMultiRepoArtifactsCommand CreateValidCommand(
        string branchName = ValidBranchName,
        string commitMessage = ValidCommitMessage)
    {
        return new PushProjectMultiRepoArtifactsCommand(
            ProjectId.CreateUnique(),
            [CreateValidConfiguration(branchName: branchName, commitMessage: commitMessage)]);
    }

    private static InfrastructureConfigPushTarget CreateValidConfiguration(
        InfrastructureConfigId? infrastructureConfigId = null,
        string branchName = ValidBranchName,
        string commitMessage = ValidCommitMessage)
    {
        return new InfrastructureConfigPushTarget(
            infrastructureConfigId ?? InfrastructureConfigId.CreateUnique(),
            [new ConfigRepositoryPushTarget(
                InfraConfigRepositoryId.CreateUnique(),
                branchName,
                commitMessage)]);
    }
}