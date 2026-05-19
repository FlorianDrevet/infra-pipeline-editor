using FluentAssertions;
using InfraFlowSculptor.Application.Projects.Commands.PushProjectArtifactsToMultiRepo;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.Projects.Commands.PushProjectArtifactsToMultiRepo;

public sealed class PushProjectArtifactsToMultiRepoCommandValidatorTests
{
    private const string ValidBranchName = "feature/deploy";
    private const string ValidCommitMessage = "Push artifacts";
    private const string ProjectIdProperty = nameof(PushProjectArtifactsToMultiRepoCommand.ProjectId);

    private static readonly RepoPushTarget ValidInfra = new(ProjectRepositoryId.CreateUnique(), ValidBranchName, ValidCommitMessage);
    private static readonly RepoPushTarget ValidCode = new(ProjectRepositoryId.CreateUnique(), "main", "Push app code");

    private readonly PushProjectArtifactsToMultiRepoCommandValidator _sut = new();

    [Fact]
    public void Given_ValidCommandWithInfraOnly_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new PushProjectArtifactsToMultiRepoCommand(ProjectId.CreateUnique(), ValidInfra, null);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_ValidCommandWithCodeOnly_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new PushProjectArtifactsToMultiRepoCommand(ProjectId.CreateUnique(), null, ValidCode);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_ValidCommandWithBoth_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new PushProjectArtifactsToMultiRepoCommand(ProjectId.CreateUnique(), ValidInfra, ValidCode);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_NullProjectId_When_Validate_Then_FailsOnProjectId()
    {
        // Arrange
        var command = new PushProjectArtifactsToMultiRepoCommand(null!, ValidInfra, null);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == ProjectIdProperty);
    }

    [Fact]
    public void Given_BothInfraAndCodeNull_When_Validate_Then_Fails()
    {
        // Arrange
        var command = new PushProjectArtifactsToMultiRepoCommand(ProjectId.CreateUnique(), null, null);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Given_InfraEmptyRepositoryId_When_Validate_Then_Fails()
    {
        // Arrange
        var infra = new RepoPushTarget(new ProjectRepositoryId(Guid.Empty), ValidBranchName, ValidCommitMessage);
        var command = new PushProjectArtifactsToMultiRepoCommand(ProjectId.CreateUnique(), infra, null);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Infra.RepositoryId.Value");
    }

    [Fact]
    public void Given_InfraEmptyBranchName_When_Validate_Then_Fails()
    {
        // Arrange
        var infra = new RepoPushTarget(ProjectRepositoryId.CreateUnique(), "", ValidCommitMessage);
        var command = new PushProjectArtifactsToMultiRepoCommand(ProjectId.CreateUnique(), infra, null);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Infra.BranchName");
    }

    [Fact]
    public void Given_InfraEmptyCommitMessage_When_Validate_Then_Fails()
    {
        // Arrange
        var infra = new RepoPushTarget(ProjectRepositoryId.CreateUnique(), ValidBranchName, "");
        var command = new PushProjectArtifactsToMultiRepoCommand(ProjectId.CreateUnique(), infra, null);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Infra.CommitMessage");
    }

    [Fact]
    public void Given_CodeEmptyRepositoryId_When_Validate_Then_Fails()
    {
        // Arrange
        var code = new RepoPushTarget(new ProjectRepositoryId(Guid.Empty), "main", "Push");
        var command = new PushProjectArtifactsToMultiRepoCommand(ProjectId.CreateUnique(), null, code);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Code.RepositoryId.Value");
    }
}
