using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.GitRouting;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.Projects.Commands.PushProjectBicepToGit;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using NSubstitute;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.Projects.Commands.PushProjectBicepToGit;

public sealed class PushProjectBicepToGitCommandHandlerTests
{
    private readonly IProjectAccessService _accessService;
    private readonly IProjectRepository _projectRepo;
    private readonly IKeyVaultSecretClient _keyVaultClient;
    private readonly IGitProviderFactory _gitProviderFactory;
    private readonly IBlobService _blobService;
    private readonly IRepositoryTargetResolver _targetResolver;
    private readonly IGitProviderService _gitProvider;
    private readonly PushProjectBicepToGitCommandHandler _sut;

    private readonly ProjectId _projectId = ProjectId.CreateUnique();
    private readonly Project _project;
    private readonly ResolvedRepositoryTarget _target;

    public PushProjectBicepToGitCommandHandlerTests()
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
            RepositoryId: ProjectRepositoryId.CreateUnique().Value.ToString(),
            ProviderType: new GitProviderType(GitProviderTypeEnum.GitHub),
            RepositoryUrl: "https://github.com/owner/repo",
            Owner: "owner",
            RepositoryName: "repo",
            Branch: "main",
            BasePath: "infra",
            PipelineBasePath: ".azuredevops",
            PatSecretName: null);

        _sut = new PushProjectBicepToGitCommandHandler(
            _accessService, _projectRepo, _keyVaultClient,
            _gitProviderFactory, _blobService, _targetResolver);
    }

    [Fact]
    public async Task Given_AllStepsSucceed_When_Handle_Then_ReturnsPushResultAsync()
    {
        // Arrange
        var command = new PushProjectBicepToGitCommand(_projectId, "feature/bicep", "push project bicep");

        _accessService.VerifyWriteAccessAsync(_projectId, Arg.Any<CancellationToken>())
            .Returns(_project);
        _projectRepo.GetByIdWithAllAsync(_projectId, Arg.Any<CancellationToken>())
            .Returns(_project);
        _targetResolver.Resolve(_project, null, ArtifactKind.Infrastructure)
            .Returns(_target);
        _keyVaultClient.GetSecretAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("ghp_test_pat");

        var blobName = $"bicep/project/{_projectId.Value}/20260517120000/main.bicep";
        _blobService.ListBlobsAsync($"bicep/project/{_projectId.Value}/")
            .Returns(new List<string> { blobName });
        _blobService.DownloadContentAsync(blobName)
            .Returns("param location string");

        var pushResult = new PushBicepToGitResult("feature/bicep", "https://github.com/owner/repo/tree/feature/bicep", "abc123", 1);
        _gitProviderFactory.Create(_target.ProviderType).Returns(_gitProvider);
        _gitProvider.PushFilesAsync(Arg.Any<GitPushRequest>(), Arg.Any<CancellationToken>())
            .Returns(pushResult);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.BranchName.Should().Be("feature/bicep");
        result.Value.FileCount.Should().Be(1);
    }

    [Fact]
    public async Task Given_WriteAccessDenied_When_Handle_Then_ReturnsErrorAsync()
    {
        // Arrange
        var command = new PushProjectBicepToGitCommand(_projectId, "feature/bicep", "push project bicep");
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
        var command = new PushProjectBicepToGitCommand(_projectId, "feature/bicep", "push project bicep");

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
