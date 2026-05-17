using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.GitRouting;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.PushPipelineToGit;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using NSubstitute;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.InfrastructureConfig.Commands.PushPipelineToGit;

public sealed class PushPipelineToGitCommandHandlerTests
{
    private readonly IInfraConfigAccessService _accessService;
    private readonly IInfrastructureConfigRepository _infraConfigRepo;
    private readonly IProjectRepository _projectRepo;
    private readonly IKeyVaultSecretClient _keyVaultClient;
    private readonly IGitProviderFactory _gitProviderFactory;
    private readonly IGeneratedArtifactService _artifactService;
    private readonly IRepositoryTargetResolver _targetResolver;
    private readonly IGitProviderService _gitProvider;
    private readonly PushPipelineToGitCommandHandler _sut;

    private readonly Guid _configGuid = Guid.NewGuid();
    private readonly DomainInfrastructureConfig _config;
    private readonly Project _project;
    private readonly ResolvedRepositoryTarget _target;

    public PushPipelineToGitCommandHandlerTests()
    {
        _accessService = Substitute.For<IInfraConfigAccessService>();
        _infraConfigRepo = Substitute.For<IInfrastructureConfigRepository>();
        _projectRepo = Substitute.For<IProjectRepository>();
        _keyVaultClient = Substitute.For<IKeyVaultSecretClient>();
        _gitProviderFactory = Substitute.For<IGitProviderFactory>();
        _artifactService = Substitute.For<IGeneratedArtifactService>();
        _targetResolver = Substitute.For<IRepositoryTargetResolver>();
        _gitProvider = Substitute.For<IGitProviderService>();

        _project = Project.Create(new Name("test-project"), "Test project", UserId.CreateUnique());
        _config = DomainInfrastructureConfig.Create(new Name("test-config"), _project.Id);
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

        _sut = new PushPipelineToGitCommandHandler(
            _accessService, _infraConfigRepo, _projectRepo,
            _keyVaultClient, _gitProviderFactory, _artifactService, _targetResolver);
    }

    [Fact]
    public async Task Given_AllStepsSucceed_When_Handle_Then_ReturnsPushResultAsync()
    {
        // Arrange
        var command = new PushPipelineToGitCommand(_configGuid, "feature/pipeline", "push pipeline");
        var infraConfigId = new InfrastructureConfigId(_configGuid);

        _accessService.VerifyWriteAccessAsync(infraConfigId, Arg.Any<CancellationToken>())
            .Returns(_config);
        _infraConfigRepo.GetByIdAsync(infraConfigId, Arg.Any<CancellationToken>())
            .Returns(_config);
        _projectRepo.GetByIdWithAllAsync(_config.ProjectId, Arg.Any<CancellationToken>())
            .Returns(_project);
        _targetResolver.Resolve(_project, _config, ArtifactKind.Pipeline)
            .Returns(_target);
        _keyVaultClient.GetSecretAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("ghp_test_pat");
        _artifactService.GetLatestFilesAsync("pipeline", _configGuid, Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, string> { ["main.yml"] = "trigger: none" });

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
        var command = new PushPipelineToGitCommand(_configGuid, "feature/pipeline", "push pipeline");
        _accessService.VerifyWriteAccessAsync(Arg.Any<InfrastructureConfigId>(), Arg.Any<CancellationToken>())
            .Returns(Errors.InfrastructureConfig.NotFoundError(new InfrastructureConfigId(_configGuid)));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        await _infraConfigRepo.DidNotReceive().GetByIdAsync(Arg.Any<InfrastructureConfigId>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_NoPipelineFilesExist_When_Handle_Then_ReturnsNotFoundErrorAsync()
    {
        // Arrange
        var command = new PushPipelineToGitCommand(_configGuid, "feature/pipeline", "push pipeline");
        var infraConfigId = new InfrastructureConfigId(_configGuid);

        _accessService.VerifyWriteAccessAsync(infraConfigId, Arg.Any<CancellationToken>())
            .Returns(_config);
        _infraConfigRepo.GetByIdAsync(infraConfigId, Arg.Any<CancellationToken>())
            .Returns(_config);
        _projectRepo.GetByIdWithAllAsync(_config.ProjectId, Arg.Any<CancellationToken>())
            .Returns(_project);
        _targetResolver.Resolve(_project, _config, ArtifactKind.Pipeline)
            .Returns(_target);
        _keyVaultClient.GetSecretAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("ghp_test_pat");
        _artifactService.GetLatestFilesAsync("pipeline", _configGuid, Arg.Any<CancellationToken>())
            .Returns((IReadOnlyDictionary<string, string>?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
    }
}
