using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.InfrastructureConfig.ReadModels;
using InfraFlowSculptor.Application.Projects.Common.Generation;
using InfraFlowSculptor.Domain.ProjectAggregate.Entities;
using InfraFlowSculptor.GenerationCore.Models;
using InfraFlowSculptor.PipelineGeneration;
using InfraFlowSculptor.PipelineGeneration.Infra;
using InfraFlowSculptor.PipelineGeneration.Models;
using NSubstitute;

namespace InfraFlowSculptor.Application.Tests.Projects.Common.Generation;

public sealed class ProjectPipelineAggregatorTests
{
    [Fact]
    public async Task Given_InfraPipelineGenerationThrowsUnexpectedInvalidOperation_When_GenerateAsync_Then_RethrowsWithoutGeneratingAppPipelinesAsync()
    {
        // Arrange
        var configPipelineGenerationService = Substitute.For<IConfigPipelineGenerationService>();
        var config = CreateConfig("core", "Development", "dev");
        var generationRequest = CreateGenerationRequest("Development", "dev");
        configPipelineGenerationService.BuildGenerationRequestForPipeline(
                config,
                Arg.Any<IReadOnlyCollection<ProjectPipelineVariableGroup>>(),
                Arg.Any<string?>(),
                Arg.Any<string?>())
            .Returns(generationRequest);

        var stage = Substitute.For<IInfraPipelineStage>();
        stage.Order.Returns(0);
        stage.When(current => current.Execute(Arg.Any<InfraPipelineContext>()))
            .Do(_ => throw new InvalidOperationException("broken pipeline"));

        var sut = new ProjectPipelineAggregator(
            new PipelineGenerationEngine(new InfraPipeline([stage])),
            configPipelineGenerationService);

        // Act
        var act = async () => await sut.GenerateAsync(
            [config],
            Array.Empty<ProjectPipelineVariableGroup>(),
            agentPoolName: null,
            bicepBasePath: null,
            pipelineBasePath: null,
            CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("broken pipeline");
        await configPipelineGenerationService.DidNotReceive()
            .GenerateAppPipelinesAsync(
                Arg.Any<InfrastructureConfigReadModel>(),
                Arg.Any<GenerationRequest>(),
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_AppPipelineGenerationFails_When_GenerateAsync_Then_ReturnsAppPipelineErrorAsync()
    {
        // Arrange
        var configPipelineGenerationService = Substitute.For<IConfigPipelineGenerationService>();
        var config = CreateConfig("core", "Development", "dev");
        var generationRequest = CreateGenerationRequest("Development", "dev");
        var expectedError = Error.Failure(
            code: "AppPipeline.GenerationFailed",
            description: "Application pipeline generation failed.");

        configPipelineGenerationService.BuildGenerationRequestForPipeline(
                config,
                Arg.Any<IReadOnlyCollection<ProjectPipelineVariableGroup>>(),
                Arg.Any<string?>(),
                Arg.Any<string?>())
            .Returns(generationRequest);
        configPipelineGenerationService.GenerateAppPipelinesAsync(
                config,
                generationRequest,
                true,
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ErrorOr<AppPipelineGenerationResult>>(expectedError));

        var stage = Substitute.For<IInfraPipelineStage>();
        stage.Order.Returns(0);
        stage.When(current => current.Execute(Arg.Any<InfraPipelineContext>()))
            .Do(callInfo =>
            {
                var context = callInfo.Arg<InfraPipelineContext>();
                context.Files["release.pipeline.yml"] = "infra-core";
            });

        var sut = new ProjectPipelineAggregator(
            new PipelineGenerationEngine(new InfraPipeline([stage])),
            configPipelineGenerationService);

        // Act
        var result = await sut.GenerateAsync(
            [config],
            Array.Empty<ProjectPipelineVariableGroup>(),
            agentPoolName: "private-agents",
            bicepBasePath: "infra",
            pipelineBasePath: "pipelines",
            CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(expectedError.Code);
    }

    [Fact]
    public async Task Given_DuplicateEnvironmentShortNames_When_GenerateAsync_Then_DeduplicatesEnvironmentsAndMergesAppFilesAsync()
    {
        // Arrange
        var configPipelineGenerationService = Substitute.For<IConfigPipelineGenerationService>();
        var coreConfig = CreateConfig("core", "Development", "dev");
        var appConfig = CreateConfig("app", "Development Secondary", "DEV");

        configPipelineGenerationService.BuildGenerationRequestForPipeline(
                coreConfig,
                Arg.Any<IReadOnlyCollection<ProjectPipelineVariableGroup>>(),
                Arg.Any<string?>(),
                Arg.Any<string?>())
            .Returns(CreateGenerationRequest("Development", "dev"));
        configPipelineGenerationService.BuildGenerationRequestForPipeline(
                appConfig,
                Arg.Any<IReadOnlyCollection<ProjectPipelineVariableGroup>>(),
                Arg.Any<string?>(),
                Arg.Any<string?>())
            .Returns(CreateGenerationRequest("Development Secondary", "DEV"));
        configPipelineGenerationService.GenerateAppPipelinesAsync(
                coreConfig,
                Arg.Any<GenerationRequest>(),
                true,
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ErrorOr<AppPipelineGenerationResult>>(
                new AppPipelineGenerationResult
                {
                    Files = new Dictionary<string, string>
                    {
                        ["apps/api/ci.pipeline.yml"] = "app-core",
                    },
                }));
        configPipelineGenerationService.GenerateAppPipelinesAsync(
                appConfig,
                Arg.Any<GenerationRequest>(),
                true,
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ErrorOr<AppPipelineGenerationResult>>(
                new AppPipelineGenerationResult()));

        var stage = Substitute.For<IInfraPipelineStage>();
        stage.Order.Returns(0);
        stage.When(current => current.Execute(Arg.Any<InfraPipelineContext>()))
            .Do(callInfo =>
            {
                var context = callInfo.Arg<InfraPipelineContext>();
                context.Files["release.pipeline.yml"] = $"infra-{context.ConfigName}";
            });

        var sut = new ProjectPipelineAggregator(
            new PipelineGenerationEngine(new InfraPipeline([stage])),
            configPipelineGenerationService);

        // Act
        var result = await sut.GenerateAsync(
            [coreConfig, appConfig],
            Array.Empty<ProjectPipelineVariableGroup>(),
            agentPoolName: "private-agents",
            bicepBasePath: "infra",
            pipelineBasePath: "pipelines",
            CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.CommonFiles.Keys.Count(path => path == "variables/dev.variables.yml").Should().Be(1);
        result.Value.ConfigFiles["core"].Should().ContainKey("release.pipeline.yml");
        result.Value.ConfigFiles["core"].Should().ContainKey("apps/api/ci.pipeline.yml");
        result.Value.ConfigFiles["core"]["apps/api/ci.pipeline.yml"].Should().Be("app-core");
        result.Value.ConfigFiles["app"].Should().ContainKey("release.pipeline.yml");
    }

    private static InfrastructureConfigReadModel CreateConfig(
        string configName,
        string environmentName,
        string environmentShortName)
    {
        return new InfrastructureConfigReadModel(
            Guid.NewGuid(),
            configName,
            Guid.NewGuid(),
            [],
            [
                new EnvironmentDefinitionReadModel(
                    Guid.NewGuid(),
                    environmentName,
                    environmentShortName,
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

    private static GenerationRequest CreateGenerationRequest(
        string environmentName,
        string environmentShortName)
    {
        return new GenerationRequest
        {
            Environments =
            [
                new EnvironmentDefinition
                {
                    Name = environmentName,
                    ShortName = environmentShortName,
                    Location = "francecentral",
                    Prefix = string.Empty,
                    Suffix = string.Empty,
                    Tags = new Dictionary<string, string>(),
                }
            ],
            EnvironmentNames = [environmentShortName],
        };
    }
}