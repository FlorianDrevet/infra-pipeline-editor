using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.GitRouting;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.GenerateBootstrap;
using InfraFlowSculptor.Application.InfrastructureConfig.ReadModels;
using InfraFlowSculptor.Application.Projects.Commands.GenerateProjectBootstrapPipeline;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using InfraFlowSculptor.PipelineGeneration;
using InfraFlowSculptor.PipelineGeneration.Models;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.InfrastructureConfig.Commands.GenerateBootstrap;

public sealed class GenerateBootstrapCommandHandlerTests
{
    private readonly IInfrastructureConfigReadRepository _configRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectBootstrapDefinitionBuilder _definitionBuilder;
    private readonly IGeneratedArtifactService _artifactService;
    private readonly IRepositoryTargetResolver _targetResolver;
    private readonly IInfraConfigAccessService _accessService;
    private readonly GenerateBootstrapCommandHandler _sut;

    public GenerateBootstrapCommandHandlerTests()
    {
        _configRepository = Substitute.For<IInfrastructureConfigReadRepository>();
        _projectRepository = Substitute.For<IProjectRepository>();
        _definitionBuilder = Substitute.For<IProjectBootstrapDefinitionBuilder>();
        _artifactService = Substitute.For<IGeneratedArtifactService>();
        _targetResolver = Substitute.For<IRepositoryTargetResolver>();
        _accessService = Substitute.For<IInfraConfigAccessService>();

        _sut = new GenerateBootstrapCommandHandler(
            _configRepository,
            _projectRepository,
            _definitionBuilder,
            bootstrapEngine: new BootstrapPipelineGenerationEngine(),
            _artifactService,
            _targetResolver,
            _accessService);
    }

    private static InfrastructureConfigReadModel BuildConfigReadModel(Guid configId, Guid projectId) =>
        new(
            configId,
            "dev",
            projectId,
            [],
            [],
            new NamingContextReadModel(null, new Dictionary<string, string>(), new Dictionary<string, string>()),
            [],
            [],
            [],
            new Dictionary<string, string>(),
            new Dictionary<string, string>());

    [Fact]
    public async Task Given_WriteAccessDenied_When_Handle_Then_ReturnsErrorAndDoesNotLoadConfigAsync()
    {
        // Arrange
        var command = new GenerateBootstrapCommand(Guid.NewGuid());
        _accessService.VerifyWriteAccessAsync(Arg.Any<InfrastructureConfigId>(), Arg.Any<CancellationToken>())
            .Returns(Errors.InfrastructureConfig.ForbiddenError());

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Forbidden);
        await _configRepository.DidNotReceive()
            .GetByIdWithResourcesAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_ConfigNotFound_When_Handle_Then_ReturnsNotFoundErrorAsync()
    {
        // Arrange
        var configId = Guid.NewGuid();
        var command = new GenerateBootstrapCommand(configId);
        var domainConfig = DomainInfrastructureConfig.Create(new Name("dev"), new ProjectId(Guid.NewGuid()));

        _accessService.VerifyWriteAccessAsync(Arg.Any<InfrastructureConfigId>(), Arg.Any<CancellationToken>())
            .Returns(domainConfig);
        _configRepository.GetByIdWithResourcesAsync(configId, Arg.Any<CancellationToken>())
            .Returns((InfrastructureConfigReadModel?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(Errors.InfrastructureConfig.NotFoundError(new InfrastructureConfigId(configId)).Code);
    }

    [Fact]
    public async Task Given_ProjectNotFound_When_Handle_Then_ReturnsProjectNotFoundErrorAsync()
    {
        // Arrange
        var configId = Guid.NewGuid();
        var projectGuid = Guid.NewGuid();
        var command = new GenerateBootstrapCommand(configId);
        var domainConfig = DomainInfrastructureConfig.Create(new Name("dev"), new ProjectId(projectGuid));
        var config = BuildConfigReadModel(configId, projectGuid);

        _accessService.VerifyWriteAccessAsync(Arg.Any<InfrastructureConfigId>(), Arg.Any<CancellationToken>())
            .Returns(domainConfig);
        _configRepository.GetByIdWithResourcesAsync(configId, Arg.Any<CancellationToken>())
            .Returns(config);
        _projectRepository.GetByIdWithAllAndPipelineVariableGroupsAsync(Arg.Any<ProjectId>(), Arg.Any<CancellationToken>())
            .Returns((Project?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(Errors.Project.NotFoundError(new ProjectId(projectGuid)).Code);
    }

    [Fact]
    public async Task Given_NoRepositoryConfigured_When_Handle_Then_ReturnsTargetResolutionErrorAsync()
    {
        // Arrange
        var configId = Guid.NewGuid();
        var projectGuid = Guid.NewGuid();
        var command = new GenerateBootstrapCommand(configId);
        var domainConfig = DomainInfrastructureConfig.Create(new Name("dev"), new ProjectId(projectGuid));
        var config = BuildConfigReadModel(configId, projectGuid);
        var project = Project.Create(new Name("Retail Platform"), "Provision retail assets.", UserId.CreateUnique());
        project.SetLayoutPreset(new LayoutPreset(LayoutPresetEnum.MultiRepo));

        _accessService.VerifyWriteAccessAsync(Arg.Any<InfrastructureConfigId>(), Arg.Any<CancellationToken>())
            .Returns(domainConfig);
        _configRepository.GetByIdWithResourcesAsync(configId, Arg.Any<CancellationToken>())
            .Returns(config);
        _projectRepository.GetByIdWithAllAndPipelineVariableGroupsAsync(Arg.Any<ProjectId>(), Arg.Any<CancellationToken>())
            .Returns(project);
        _targetResolver.Resolve(project, domainConfig, ArtifactKind.Bootstrap)
            .Returns(Errors.GitRouting.NoRepositoryConfigured(project.Id));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(Errors.GitRouting.NoRepositoryConfigured(project.Id).Code);
        await _definitionBuilder.DidNotReceive().BuildAsync(
            Arg.Any<Project>(),
            Arg.Any<IReadOnlyList<InfrastructureConfigReadModel>>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_ValidRequest_When_Handle_Then_GeneratesAndUploadsBootstrapFilesScopedToConfigAsync()
    {
        // Arrange
        var configId = Guid.NewGuid();
        var projectGuid = Guid.NewGuid();
        var command = new GenerateBootstrapCommand(configId);
        var domainConfig = DomainInfrastructureConfig.Create(new Name("dev"), new ProjectId(projectGuid));
        var config = BuildConfigReadModel(configId, projectGuid);
        var project = Project.Create(new Name("Retail Platform"), "Provision retail assets.", UserId.CreateUnique());
        project.SetLayoutPreset(new LayoutPreset(LayoutPresetEnum.MultiRepo));

        var target = new ResolvedRepositoryTarget(
            RepositoryId: Guid.NewGuid().ToString(),
            ProviderType: new GitProviderType(GitProviderTypeEnum.GitHub),
            RepositoryUrl: "https://github.com/owner/repo",
            Owner: "owner",
            RepositoryName: "repo",
            Branch: "main",
            BasePath: "infra",
            PipelineBasePath: ".azuredevops",
            PatSecretName: null);

        _accessService.VerifyWriteAccessAsync(Arg.Any<InfrastructureConfigId>(), Arg.Any<CancellationToken>())
            .Returns(domainConfig);
        _configRepository.GetByIdWithResourcesAsync(configId, Arg.Any<CancellationToken>())
            .Returns(config);
        _projectRepository.GetByIdWithAllAndPipelineVariableGroupsAsync(Arg.Any<ProjectId>(), Arg.Any<CancellationToken>())
            .Returns(project);
        _targetResolver.Resolve(project, domainConfig, ArtifactKind.Bootstrap)
            .Returns(target);

        var definitions = new ProjectBootstrapDefinitions(
            InfraPipelines: [new BootstrapPipelineDefinition("[Infra] dev - CI", "/.azuredevops/dev/ci.pipeline.yml", "\\dev")],
            AppPipelines: [],
            VariableGroups: [],
            Environments: [new BootstrapEnvironmentDefinition("dev", "Development", false)],
            ServiceConnections: []);

        _definitionBuilder.BuildAsync(
                project,
                Arg.Is<IReadOnlyList<InfrastructureConfigReadModel>>(list => list.Count == 1 && list[0] == config),
                target.PipelineBasePath,
                target.PipelineBasePath,
                Arg.Any<CancellationToken>())
            .Returns(definitions);

        _artifactService.UploadArtifactAsync(
                "bootstrap", configId, Arg.Any<string>(), "bootstrap.pipeline.yml", Arg.Any<string>())
            .Returns(new Uri("https://blob.example.com/bootstrap/dev/bootstrap.pipeline.yml"));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.FileUris.Should().ContainKey("bootstrap.pipeline.yml");
        await _artifactService.Received(1).UploadArtifactAsync(
            "bootstrap", configId, Arg.Any<string>(), "bootstrap.pipeline.yml", Arg.Any<string>());
    }
}
