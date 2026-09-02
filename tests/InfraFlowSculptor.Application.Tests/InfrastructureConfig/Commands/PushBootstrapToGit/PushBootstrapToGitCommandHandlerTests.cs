using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.GitRouting;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.PushBootstrapToGit;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.InfrastructureConfig.Commands.PushBootstrapToGit;

public sealed class PushBootstrapToGitCommandHandlerTests
{
    private readonly IInfraConfigAccessService _accessService;
    private readonly IInfrastructureConfigRepository _infraConfigRepo;
    private readonly IProjectRepository _projectRepo;
    private readonly IKeyVaultSecretClient _keyVaultClient;
    private readonly IGitProviderFactory _gitProviderFactory;
    private readonly IGeneratedArtifactService _artifactService;
    private readonly IRepositoryTargetResolver _targetResolver;
    private readonly IGitProviderService _gitProvider;
    private readonly PushBootstrapToGitCommandHandler _sut;

    private readonly Guid _configGuid = Guid.NewGuid();
    private readonly DomainInfrastructureConfig _config;
    private readonly Project _project;
    private readonly ResolvedRepositoryTarget _target;

    public PushBootstrapToGitCommandHandlerTests()
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
        _project.SetLayoutPreset(new LayoutPreset(LayoutPresetEnum.MultiRepo));
        _config = DomainInfrastructureConfig.Create(new Name("test-config"), _project.Id);
        _target = new ResolvedRepositoryTarget(
            RepositoryId: ProjectRepositoryId.CreateUnique().Value.ToString(),
            ProviderType: new GitProviderType(GitProviderTypeEnum.GitHub),
            RepositoryUrl: "https://github.com/owner/repo",
            Owner: "owner",
            RepositoryName: "repo",
            Branch: "main",
            BasePath: "infra",
            PipelineBasePath: ".azuredevops",
            PatSecretName: "git-pat-repository");

        _sut = new PushBootstrapToGitCommandHandler(
            _accessService, _infraConfigRepo, _projectRepo,
            _keyVaultClient, _gitProviderFactory, _artifactService, _targetResolver);
    }

    [Fact]
    public async Task Given_AllStepsSucceed_When_Handle_Then_ReturnsPushResultWithoutPathNormalizationAsync()
    {
        // Arrange
        var command = new PushBootstrapToGitCommand(_configGuid, "feature/bootstrap", "push bootstrap");
        var infraConfigId = new InfrastructureConfigId(_configGuid);

        _accessService.VerifyWriteAccessAsync(infraConfigId, Arg.Any<CancellationToken>())
            .Returns(_config);
        _infraConfigRepo.GetByIdAsync(infraConfigId, Arg.Any<CancellationToken>())
            .Returns(_config);
        _projectRepo.GetByIdWithAllAsync(_config.ProjectId, Arg.Any<CancellationToken>())
            .Returns(_project);
        _targetResolver.Resolve(_project, _config, ArtifactKind.Bootstrap)
            .Returns(_target);
        _keyVaultClient.GetSecretAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("ghp_test_pat");
        _artifactService.GetLatestFilesAsync("bootstrap", _configGuid, Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, string> { ["bootstrap.pipeline.yml"] = "trigger: none" });

        var pushResult = new PushBicepToGitResult("feature/bootstrap", "https://github.com/owner/repo/tree/feature/bootstrap", "abc123", 1);
        _gitProviderFactory.Create(_target.ProviderType).Returns(_gitProvider);
        _gitProvider.PushFilesAsync(Arg.Any<GitPushRequest>(), Arg.Any<CancellationToken>())
            .Returns(pushResult);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.BranchName.Should().Be("feature/bootstrap");
        await _gitProvider.Received(1).PushFilesAsync(
            Arg.Is<GitPushRequest>(req =>
                req.BasePath == _target.PipelineBasePath
                && req.Files.ContainsKey("bootstrap.pipeline.yml")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_WriteAccessDenied_When_Handle_Then_ReturnsErrorAsync()
    {
        // Arrange
        var command = new PushBootstrapToGitCommand(_configGuid, "feature/bootstrap", "push bootstrap");
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
    public async Task Given_NoBootstrapFilesExist_When_Handle_Then_ReturnsNotFoundErrorAsync()
    {
        // Arrange
        var command = new PushBootstrapToGitCommand(_configGuid, "feature/bootstrap", "push bootstrap");
        var infraConfigId = new InfrastructureConfigId(_configGuid);

        _accessService.VerifyWriteAccessAsync(infraConfigId, Arg.Any<CancellationToken>())
            .Returns(_config);
        _infraConfigRepo.GetByIdAsync(infraConfigId, Arg.Any<CancellationToken>())
            .Returns(_config);
        _projectRepo.GetByIdWithAllAsync(_config.ProjectId, Arg.Any<CancellationToken>())
            .Returns(_project);
        _targetResolver.Resolve(_project, _config, ArtifactKind.Bootstrap)
            .Returns(_target);
        _keyVaultClient.GetSecretAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("ghp_test_pat");
        _artifactService.GetLatestFilesAsync("bootstrap", _configGuid, Arg.Any<CancellationToken>())
            .Returns((IReadOnlyDictionary<string, string>?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(Errors.InfrastructureConfig.BootstrapFilesNotFoundError(_configGuid).Code);
    }

    [Fact]
    public async Task Given_NoRepositoryConfigured_When_Handle_Then_ReturnsTargetResolutionErrorAsync()
    {
        // Arrange
        var command = new PushBootstrapToGitCommand(_configGuid, "feature/bootstrap", "push bootstrap");
        var infraConfigId = new InfrastructureConfigId(_configGuid);

        _accessService.VerifyWriteAccessAsync(infraConfigId, Arg.Any<CancellationToken>())
            .Returns(_config);
        _infraConfigRepo.GetByIdAsync(infraConfigId, Arg.Any<CancellationToken>())
            .Returns(_config);
        _projectRepo.GetByIdWithAllAsync(_config.ProjectId, Arg.Any<CancellationToken>())
            .Returns(_project);
        _targetResolver.Resolve(_project, _config, ArtifactKind.Bootstrap)
            .Returns(Errors.GitRouting.NoRepositoryConfigured(_project.Id));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(Errors.GitRouting.NoRepositoryConfigured(_project.Id).Code);
    }

    [Fact]
    public async Task Given_SplitInfraCodeBootstrap_When_Handle_Then_PushesOnlyInfrastructureBootstrapFilesAsync()
    {
        // Arrange
        var command = new PushBootstrapToGitCommand(_configGuid, "feature/bootstrap", "push bootstrap");
        _config.SetLayoutMode(new ConfigLayoutMode(ConfigLayoutModeEnum.SplitInfraCode));
        _accessService.VerifyWriteAccessAsync(Arg.Any<InfrastructureConfigId>(), Arg.Any<CancellationToken>())
            .Returns(_config);
        _infraConfigRepo.GetByIdAsync(Arg.Any<InfrastructureConfigId>(), Arg.Any<CancellationToken>())
            .Returns(_config);
        _projectRepo.GetByIdWithAllAsync(_config.ProjectId, Arg.Any<CancellationToken>())
            .Returns(_project);
        _targetResolver.Resolve(_project, _config, ArtifactKind.Bootstrap).Returns(_target);
        _keyVaultClient.GetSecretAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("config-pat");
        _artifactService.GetLatestFilesAsync("bootstrap", _configGuid, Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, string>
            {
                ["infra/bootstrap.pipeline.yml"] = "infra-bootstrap",
                ["app/bootstrap.pipeline.yml"] = "app-bootstrap"
            });
        _gitProviderFactory.Create(_target.ProviderType).Returns(_gitProvider);
        _gitProvider.PushFilesAsync(Arg.Any<GitPushRequest>(), Arg.Any<CancellationToken>())
            .Returns(new PushBicepToGitResult("feature/bootstrap", "https://example/branch", "abc123", 1));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        await _gitProvider.Received(1).PushFilesAsync(
            Arg.Is<GitPushRequest>(request =>
                request.Files.Count == 1
                && request.Files.ContainsKey("bootstrap.pipeline.yml")
                && request.Files["bootstrap.pipeline.yml"] == "infra-bootstrap"
                && !request.Files.ContainsKey("app/bootstrap.pipeline.yml")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_ConfigRepositoryTargetHasNoPatSecret_When_Handle_Then_DoesNotUseProjectSecretFallbackAsync()
    {
        // Arrange
        var command = new PushBootstrapToGitCommand(_configGuid, "feature/bootstrap", "push bootstrap");
        var targetWithoutSecret = _target with { PatSecretName = null };
        _accessService.VerifyWriteAccessAsync(Arg.Any<InfrastructureConfigId>(), Arg.Any<CancellationToken>())
            .Returns(_config);
        _infraConfigRepo.GetByIdAsync(Arg.Any<InfrastructureConfigId>(), Arg.Any<CancellationToken>())
            .Returns(_config);
        _projectRepo.GetByIdWithAllAsync(_config.ProjectId, Arg.Any<CancellationToken>())
            .Returns(_project);
        _targetResolver.Resolve(_project, _config, ArtifactKind.Bootstrap).Returns(targetWithoutSecret);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(Errors.GitRepository.SecretRetrievalFailed().Code);
        await _keyVaultClient.DidNotReceive()
            .GetSecretAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
