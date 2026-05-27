using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.GenerateBicep;
using InfraFlowSculptor.Application.InfrastructureConfig.Diagnostics;
using InfraFlowSculptor.Application.InfrastructureConfig.ReadModels;
using InfraFlowSculptor.BicepGeneration;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.BicepGeneration.Pipeline;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using NSubstitute;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;

namespace InfraFlowSculptor.Application.Tests.InfrastructureConfig.Commands.GenerateBicep;

public sealed class GenerateBicepCommandHandlerTests
{
    [Fact]
    public async Task Given_CancellationToken_When_Handle_Then_ForwardsItToBicepGenerationContextAsync()
    {
        // Arrange
        var configRepository = Substitute.For<IInfrastructureConfigReadRepository>();
        var blobService = Substitute.For<IBlobService>();
        var diagnosticService = Substitute.For<IConfigDiagnosticService>();
        var accessService = Substitute.For<IInfraConfigAccessService>();

        var configId = Guid.NewGuid();
        var projectId = ProjectId.CreateUnique();
        var domainConfig = DomainInfrastructureConfig.Create(new Name("shared-dev"), projectId);
        var config = BuildConfigReadModel(configId, projectId.Value);

        accessService.VerifyWriteAccessAsync(
                Arg.Is<InfrastructureConfigId>(id => id.Value == configId),
                Arg.Any<CancellationToken>())
            .Returns(domainConfig);
        configRepository.GetByIdWithResourcesAsync(configId, Arg.Any<CancellationToken>())
            .Returns(config);
        blobService.UploadContentAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(new Uri("https://example.test/artifact"));
        diagnosticService.EvaluateAsync(config, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<ResourceDiagnosticItem>());

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
                    MainBicep = "main",
                    TypesBicep = "types",
                    FunctionsBicep = "functions",
                };
            });

        var engine = new BicepGenerationEngine(new BicepGenerationPipeline([stage]));
        var sut = new GenerateBicepCommandHandler(
            configRepository,
            engine,
            blobService,
            diagnosticService,
            accessService);
        using var cancellationTokenSource = new CancellationTokenSource();
        var command = new GenerateBicepCommand(configId);

        // Act
        var result = await sut.Handle(command, cancellationTokenSource.Token);

        // Assert
        result.IsError.Should().BeFalse();
        capturedCancellationToken.Should().Be(cancellationTokenSource.Token);
    }

    private static InfrastructureConfigReadModel BuildConfigReadModel(Guid configId, Guid projectId)
    {
        return new InfrastructureConfigReadModel(
            configId,
            "shared-dev",
            projectId,
            [],
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
