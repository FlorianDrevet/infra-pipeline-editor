using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.GitRouting;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.Projects.Commands.PushProjectPipelineToGit;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using NSubstitute;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.Projects.Commands.PushProjectPipelineToGit;

public sealed class PushProjectPipelineToGitCommandHandlerTests
{
    private readonly IProjectAccessService _accessService;
    private readonly IProjectRepository _projectRepo;
    private readonly IKeyVaultSecretClient _keyVaultClient;
    private readonly IGitProviderFactory _gitProviderFactory;
    private readonly IBlobService _blobService;
    private readonly IRepositoryTargetResolver _targetResolver;
    private readonly IGitProviderService _gitProvider;
    private readonly PushProjectPipelineToGitCommandHandler _sut;

    private readonly ProjectId _projectId = ProjectId.CreateUnique();
    private readonly Project _project;
    private readonly ResolvedRepositoryTarget _target;

    public PushProjectPipelineToGitCommandHandlerTests()
    {
        _accessService = Substitute.For<IProjectAccessService>();
        _projectRepo = Substitute.For<IProjectRepository>();
        _keyVaultClient = Substitute.For<IKeyVaultSecretClient>();
        _gitProviderFactory = Substitute.For<IGitProviderFactory>();
        _blobService = Substitute.For<IBlobService>();
        _targetResolver = Substitute.For<IRepositoryTargetResolver>();
        _gitProvider = Substitute.For<IGitProviderService>();

        _project = Project.Create(new Name("test-project"), "Test project", UserId.CreateUnique());
        _target = new ResolvedRepositoryTarget(
            Alias: "default",
            ProviderType: new GitProviderType(GitProviderTypeEnum.GitHub),
            RepositoryUrl: "https://github.com/owner/repo",
            Owner: "owner",
            RepositoryName: "repo",
            Branch: "main",
            BasePath: "infra",
            PipelineBasePath: ".azuredevops",
            PatSecretName: null);

        _sut = new PushProjectPipelineToGitCommandHandler(
            _accessService, _projectRepo, _keyVaultClient,
            _gitProviderFactory, _blobService, _targetResolver);
    }

    [Fact]
    public async Task Given_AllStepsSucceed_When_Handle_Then_ReturnsPushResultAsync()
    {
        // Arrange
        var command = new PushProjectPipelineToGitCommand(_projectId, "feature/pipeline", "push project pipeline");

        _accessService.VerifyWriteAccessAsync(_projectId, Arg.Any<CancellationToken>())
            .Returns(_project);
        _projectRepo.GetByIdWithAllAsync(_projectId, Arg.Any<CancellationToken>())
            .Returns(_project);
        _targetResolver.Resolve(_project, null, ArtifactKind.Pipeline)
            .Returns(_target);
        _keyVaultClient.GetSecretAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("ghp_test_pat");

        var blobName = $"pipeline/project/{_projectId.Value}/20260517120000/main.yml";
        _blobService.ListBlobsAsync($"pipeline/project/{_projectId.Value}/")
            .Returns(new List<string> { blobName });
        _blobService.DownloadContentAsync(blobName)
            .Returns("trigger: none");

        var pushResult = new PushBicepToGitResult("feature/pipeline", "https://github.com/owner/repo/tree/feature/pipeline", "abc123", 1);
        _gitProviderFactory.Create(_target.ProviderType).Returns(_gitProvider);
        _gitProvider.PushFilesAsync(Arg.Any<GitPushRequest>(), Arg.Any<CancellationToken>())
            .Returns(pushResult);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.BranchName.Should().Be("feature/pipeline");
    }

    [Fact]
    public async Task Given_WriteAccessDenied_When_Handle_Then_ReturnsErrorAsync()
    {
        // Arrange
        var command = new PushProjectPipelineToGitCommand(_projectId, "feature/pipeline", "push project pipeline");
        _accessService.VerifyWriteAccessAsync(_projectId, Arg.Any<CancellationToken>())
            .Returns(Errors.Project.NotFoundError(_projectId));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        await _projectRepo.DidNotReceive().GetByIdWithAllAsync(Arg.Any<ProjectId>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_ProjectNotFound_When_Handle_Then_ReturnsNotFoundErrorAsync()
    {
        // Arrange
        var command = new PushProjectPipelineToGitCommand(_projectId, "feature/pipeline", "push project pipeline");

        _accessService.VerifyWriteAccessAsync(_projectId, Arg.Any<CancellationToken>())
            .Returns(_project);
        _projectRepo.GetByIdWithAllAsync(_projectId, Arg.Any<CancellationToken>())
            .Returns((Project?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }
}
