using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.InfrastructureConfig.ReadModels;
using InfraFlowSculptor.Application.Projects.Common.Storage;
using InfraFlowSculptor.Application.Projects.Commands.GenerateProjectBicep;
using InfraFlowSculptor.BicepGeneration;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.BicepGeneration.Pipeline;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.GenerationCore.Models;
using NSubstitute;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.Projects.Commands.GenerateProjectBicep;

public sealed class GenerateProjectBicepCommandHandlerTests
{
    private readonly IProjectAccessService _accessService;
    private readonly IInfrastructureConfigReadRepository _configReadRepository;
    private readonly IMonoRepoBlobUploadOrchestrator _blobUploadOrchestrator;
    private readonly Project _project;
    private readonly GenerateProjectBicepCommandHandler _sut;

    public GenerateProjectBicepCommandHandlerTests()
    {
        _accessService = Substitute.For<IProjectAccessService>();
        _configReadRepository = Substitute.For<IInfrastructureConfigReadRepository>();
        _blobUploadOrchestrator = Substitute.For<IMonoRepoBlobUploadOrchestrator>();
        _project = Project.Create(new Name("Retail Platform"), "Provision retail assets.", UserId.CreateUnique());
        _project.SetLayoutPreset(new LayoutPreset(LayoutPresetEnum.MultiRepo));

        _sut = new GenerateProjectBicepCommandHandler(
            _accessService,
            _configReadRepository,
            bicepGenerationEngine: null!,
            _blobUploadOrchestrator);
    }

    [Fact]
    public async Task Given_MultiRepoLayout_When_Handle_Then_ReturnsAmbiguousProjectLevelGenerationAsync()
    {
        // Arrange
        var command = new GenerateProjectBicepCommand(_project.Id);
        _accessService.VerifyWriteAccessAsync(_project.Id, Arg.Any<CancellationToken>())
            .Returns(_project);
        _configReadRepository.GetAllByProjectIdWithResourcesAsync(_project.Id.Value, Arg.Any<CancellationToken>())
            .Returns([BuildConfigReadModel(_project.Id.Value)]);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(Errors.GitRouting.AmbiguousProjectLevelGeneration.Code);
    }

    [Fact]
    public async Task Given_CrossConfigContainerRegistryReferenceOnlyViaResourceProperty_When_Handle_Then_InfersExistingResourceReferenceAsync()
    {
        // Arrange
        var project = Project.Create(new Name("Infra Flow Sculptor"), "Provision IFS assets.", UserId.CreateUnique());
        project.SetLayoutPreset(new LayoutPreset(LayoutPresetEnum.AllInOne));

        var containerRegistryId = Guid.NewGuid();
        var coreConfig = BuildConfigReadModel(
            project.Id.Value,
            "Core",
            [
                new ResourceGroupReadModel(
                    Id: Guid.NewGuid(),
                    Name: "ifs-core",
                    Location: "francecentral",
                    Resources:
                    [
                        new AzureResourceReadModel(
                            Id: containerRegistryId,
                            Name: "infraflowsculptor",
                            Location: "francecentral",
                            ResourceType: AzureResourceTypes.ArmTypes.ContainerRegistryType,
                            Properties: new Dictionary<string, string>(),
                            EnvironmentConfigs: [])
                    ])
            ]);

        var appConfig = BuildConfigReadModel(
            project.Id.Value,
            "Infra-Flow-Sculptor",
            [
                new ResourceGroupReadModel(
                    Id: Guid.NewGuid(),
                    Name: "ifs",
                    Location: "francecentral",
                    Resources:
                    [
                        new AzureResourceReadModel(
                            Id: Guid.NewGuid(),
                            Name: "ifs-api",
                            Location: "francecentral",
                            ResourceType: AzureResourceTypes.ArmTypes.ContainerAppType,
                            Properties: new Dictionary<string, string>
                            {
                                ["containerRegistryId"] = containerRegistryId.ToString(),
                            },
                            EnvironmentConfigs: [])
                    ])
            ]);

        _accessService.VerifyWriteAccessAsync(project.Id, Arg.Any<CancellationToken>())
            .Returns(project);
        _configReadRepository.GetAllByProjectIdWithResourcesAsync(project.Id.Value, Arg.Any<CancellationToken>())
            .Returns([coreConfig, appConfig]);

        var blobUploadOrchestrator = Substitute.For<IMonoRepoBlobUploadOrchestrator>();
        blobUploadOrchestrator.UploadBicepAsync(
                Arg.Any<string>(),
                Arg.Any<MonoRepoGenerationResult>(),
                Arg.Any<CancellationToken>())
            .Returns(new ProjectBicepBlobUploadResult(
                new Dictionary<string, Uri>(),
                new Dictionary<string, IReadOnlyDictionary<string, Uri>>()));

        GenerationRequest? capturedAppRequest = null;
        var stage = Substitute.For<IBicepGenerationStage>();
        stage.Order.Returns(100);
        stage.When(current => current.Execute(Arg.Any<BicepGenerationContext>()))
            .Do(callInfo =>
            {
                var context = callInfo.Arg<BicepGenerationContext>();
                if (context.Request.Resources.Any(resource => resource.Name == "ifs-api"))
                {
                    capturedAppRequest = context.Request;
                }

                context.Result = new GenerationResult
                {
                    MainBicep = "targetScope = 'subscription'\n",
                };
            });

        var engine = new BicepGenerationEngine(new BicepGenerationPipeline([stage]));
        var handler = new GenerateProjectBicepCommandHandler(
            _accessService,
            _configReadRepository,
            engine,
            blobUploadOrchestrator);

        var command = new GenerateProjectBicepCommand(project.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        capturedAppRequest.Should().NotBeNull();
        capturedAppRequest!.ExistingResourceReferences.Should().ContainSingle(reference =>
            reference.ResourceName == "infraflowsculptor"
            && reference.ResourceGroupName == "ifs-core"
            && reference.ResourceType == AzureResourceTypes.ArmTypes.ContainerRegistryType
            && reference.SourceConfigName == "Core"
            && reference.TargetResourceId == containerRegistryId);
    }

    [Fact]
    public async Task Given_CancellationToken_When_Handle_Then_ForwardsItToBicepGenerationContextAsync()
    {
        // Arrange
        var project = Project.Create(new Name("Infra Flow Sculptor"), "Provision IFS assets.", UserId.CreateUnique());
        project.SetLayoutPreset(new LayoutPreset(LayoutPresetEnum.AllInOne));

        _accessService.VerifyWriteAccessAsync(project.Id, Arg.Any<CancellationToken>())
            .Returns(project);
        _configReadRepository.GetAllByProjectIdWithResourcesAsync(project.Id.Value, Arg.Any<CancellationToken>())
            .Returns([BuildConfigReadModel(project.Id.Value)]);
        _blobUploadOrchestrator.UploadBicepAsync(
                Arg.Any<string>(),
                Arg.Any<MonoRepoGenerationResult>(),
                Arg.Any<CancellationToken>())
            .Returns(new ProjectBicepBlobUploadResult(
                new Dictionary<string, Uri>(),
                new Dictionary<string, IReadOnlyDictionary<string, Uri>>()));

        var capturedCancellationToken = CancellationToken.None;
        var stage = Substitute.For<IBicepGenerationStage>();
        stage.Order.Returns(100);
        stage.When(current => current.Execute(Arg.Any<BicepGenerationContext>()))
            .Do(callInfo =>
            {
                var context = callInfo.Arg<BicepGenerationContext>();
                capturedCancellationToken = context.CancellationToken;
                context.Result = new GenerationResult
                {
                    MainBicep = "targetScope = 'subscription'\n",
                };
            });

        var engine = new BicepGenerationEngine(new BicepGenerationPipeline([stage]));
        var sut = new GenerateProjectBicepCommandHandler(
            _accessService,
            _configReadRepository,
            engine,
            _blobUploadOrchestrator);

        using var cancellationTokenSource = new CancellationTokenSource();
        var command = new GenerateProjectBicepCommand(project.Id);

        // Act
        var result = await sut.Handle(command, cancellationTokenSource.Token);

        // Assert
        result.IsError.Should().BeFalse();
        capturedCancellationToken.Should().Be(cancellationTokenSource.Token);
    }

    private static InfrastructureConfigReadModel BuildConfigReadModel(
        Guid projectId,
        string configName = "retail-shared",
        IReadOnlyList<ResourceGroupReadModel>? resourceGroups = null)
    {
        return new InfrastructureConfigReadModel(
            Guid.NewGuid(),
            configName,
            projectId,
            resourceGroups ?? [],
            [
                new EnvironmentDefinitionReadModel(
                    Guid.NewGuid(),
                    "dev",
                    "dev",
                    "francecentral",
                    string.Empty,
                    string.Empty,
                    null,
                    null,
                    new Dictionary<string, string>())
            ],
            new NamingContextReadModel(null, new Dictionary<string, string>(), new Dictionary<string, string>()),
            [],
            [],
            [],
            new Dictionary<string, string>(),
            new Dictionary<string, string>());
    }
}