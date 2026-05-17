using FluentAssertions;
using InfraFlowSculptor.Application.Common.GitRouting;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.InfrastructureConfig.ReadModels;
using InfraFlowSculptor.Application.Projects.Common.Generation;
using InfraFlowSculptor.Application.Projects.Common.Storage;
using InfraFlowSculptor.Application.Projects.Commands.GenerateProjectPipeline;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using NSubstitute;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.Projects.Commands.GenerateProjectPipeline;

public sealed class GenerateProjectPipelineCommandHandlerTests
{
    private readonly IProjectAccessService _accessService;
    private readonly IProjectRepository _projectRepository;
    private readonly IInfrastructureConfigReadRepository _configReadRepository;
    private readonly IRepositoryTargetResolver _targetResolver;
    private readonly IProjectPipelineAggregator _projectPipelineAggregator;
    private readonly IMonoRepoBlobUploadOrchestrator _blobUploadOrchestrator;
    private readonly Project _project;
    private readonly GenerateProjectPipelineCommandHandler _sut;

    public GenerateProjectPipelineCommandHandlerTests()
    {
        _accessService = Substitute.For<IProjectAccessService>();
        _projectRepository = Substitute.For<IProjectRepository>();
        _configReadRepository = Substitute.For<IInfrastructureConfigReadRepository>();
        _targetResolver = Substitute.For<IRepositoryTargetResolver>();
        _projectPipelineAggregator = Substitute.For<IProjectPipelineAggregator>();
        _blobUploadOrchestrator = Substitute.For<IMonoRepoBlobUploadOrchestrator>();
        _project = Project.Create(new Name("Retail Platform"), "Provision retail assets.", UserId.CreateUnique());
        _project.SetLayoutPreset(new LayoutPreset(LayoutPresetEnum.MultiRepo));

        _sut = new GenerateProjectPipelineCommandHandler(
            _accessService,
            _projectRepository,
            _configReadRepository,
            _targetResolver,
            _projectPipelineAggregator,
            _blobUploadOrchestrator);
    }

    [Fact]
    public async Task Given_MultiRepoLayout_When_Handle_Then_ReturnsAmbiguousProjectLevelGenerationAsync()
    {
        // Arrange
        var command = new GenerateProjectPipelineCommand(_project.Id);
        _accessService.VerifyWriteAccessAsync(_project.Id, Arg.Any<CancellationToken>())
            .Returns(_project);
        _configReadRepository.GetAllByProjectIdWithResourcesAsync(_project.Id.Value, Arg.Any<CancellationToken>())
            .Returns([BuildConfigReadModel(_project.Id.Value)]);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(Errors.GitRouting.AmbiguousProjectLevelGeneration.Code);
        await _projectRepository.DidNotReceive()
            .GetByIdWithPipelineVariableGroupsAsync(Arg.Any<ProjectId>(), Arg.Any<CancellationToken>());
        await _projectRepository.DidNotReceive()
            .GetByIdWithAllAsync(Arg.Any<ProjectId>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_ProjectReloadFails_When_Handle_Then_ReturnsNotFoundWithoutLegacyProjectLookupsAsync()
    {
        // Arrange
        var project = Project.Create(new Name("Retail Platform"), "Provision retail assets.", UserId.CreateUnique());
        project.SetLayoutPreset(new LayoutPreset(LayoutPresetEnum.AllInOne));

        var command = new GenerateProjectPipelineCommand(project.Id);
        _accessService.VerifyWriteAccessAsync(project.Id, Arg.Any<CancellationToken>())
            .Returns(project);
        _configReadRepository.GetAllByProjectIdWithResourcesAsync(project.Id.Value, Arg.Any<CancellationToken>())
            .Returns([BuildConfigReadModel(project.Id.Value)]);
        _projectRepository.GetByIdWithAllAndPipelineVariableGroupsAsync(project.Id, Arg.Any<CancellationToken>())
            .Returns((Project?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(Errors.Project.NotFoundError(project.Id).Code);
        await _projectRepository.Received(1)
            .GetByIdWithAllAndPipelineVariableGroupsAsync(project.Id, Arg.Any<CancellationToken>());
        await _projectRepository.DidNotReceive()
            .GetByIdWithPipelineVariableGroupsAsync(Arg.Any<ProjectId>(), Arg.Any<CancellationToken>());
        await _projectRepository.DidNotReceive()
            .GetByIdWithAllAsync(Arg.Any<ProjectId>(), Arg.Any<CancellationToken>());
    }

    private static InfrastructureConfigReadModel BuildConfigReadModel(Guid projectId)
    {
        return new InfrastructureConfigReadModel(
            Guid.NewGuid(),
            "retail-shared",
            projectId,
            [],
            [],
            new NamingContextReadModel(null, new Dictionary<string, string>(), new Dictionary<string, string>()),
            [],
            [],
            [],
            new Dictionary<string, string>(),
            new Dictionary<string, string>());
    }
}
