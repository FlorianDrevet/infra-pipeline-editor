using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.GeneratePipeline;
using InfraFlowSculptor.Application.InfrastructureConfig.ReadModels;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.Entities;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;

namespace InfraFlowSculptor.Application.Tests.InfrastructureConfig.Commands.GeneratePipeline;

public sealed class GeneratePipelineCommandHandlerTests
{
    private readonly IInfrastructureConfigReadRepository _configReadRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly IProjectRepository _projectRepository;
    private readonly IConfigPipelineGenerationService _configPipelineGenerationService;
    private readonly GeneratePipelineCommandHandler _sut;

    public GeneratePipelineCommandHandlerTests()
    {
        _configReadRepository = Substitute.For<IInfrastructureConfigReadRepository>();
        _accessService = Substitute.For<IInfraConfigAccessService>();
        _projectRepository = Substitute.For<IProjectRepository>();
        _configPipelineGenerationService = Substitute.For<IConfigPipelineGenerationService>();
        _sut = new GeneratePipelineCommandHandler(
            _configReadRepository,
            _projectRepository,
            pipelineGenerationEngine: null!,
            _configPipelineGenerationService,
            artifactService: null!,
            targetResolver: null!,
            accessService: _accessService);
    }

    [Fact]
    public async Task Given_WriteAccessDenied_When_Handle_Then_ReturnsErrorAndDoesNotLoadConfigAsync()
    {
        // Arrange
        var command = new GeneratePipelineCommand(Guid.NewGuid());
        _accessService.VerifyWriteAccessAsync(Arg.Any<InfrastructureConfigId>(), Arg.Any<CancellationToken>())
            .Returns(Errors.InfrastructureConfig.ForbiddenError());

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Forbidden);
        await _configReadRepository.DidNotReceive()
            .GetByIdWithResourcesAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_ConfigExists_When_Handle_Then_LoadsProjectGenerationContextOnceAsync()
    {
        // Arrange
        var configId = Guid.NewGuid();
        var projectGuid = Guid.NewGuid();
        var command = new GeneratePipelineCommand(configId);
        var domainConfig = DomainInfrastructureConfig.Create(new Name("dev"), new ProjectId(projectGuid));
        var config = new InfrastructureConfigReadModel(
            configId,
            "dev",
            projectGuid,
            [],
            [],
            new NamingContextReadModel(null, new Dictionary<string, string>(), new Dictionary<string, string>()),
            [],
            [],
            [],
            new Dictionary<string, string>(),
            new Dictionary<string, string>());

        _accessService.VerifyWriteAccessAsync(Arg.Any<InfrastructureConfigId>(), Arg.Any<CancellationToken>())
            .Returns(domainConfig);
        _configReadRepository.GetByIdWithResourcesAsync(configId, Arg.Any<CancellationToken>())
            .Returns(config);
        _projectRepository.GetByIdWithAllAndPipelineVariableGroupsAsync(Arg.Any<ProjectId>(), Arg.Any<CancellationToken>())
            .Returns((Project?)null);
        _configPipelineGenerationService
            .When(service => service.BuildGenerationRequestForPipeline(
                Arg.Any<InfrastructureConfigReadModel>(),
                Arg.Any<IReadOnlyCollection<ProjectPipelineVariableGroup>>(),
                Arg.Any<string?>(),
                Arg.Any<string?>()))
            .Do(_ => throw new InvalidOperationException("stop"));

        // Act
        var act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("stop");
        await _projectRepository.Received(1)
            .GetByIdWithAllAndPipelineVariableGroupsAsync(
                Arg.Is<ProjectId>(id => id.Value == projectGuid),
                Arg.Any<CancellationToken>());
        await _projectRepository.DidNotReceive()
            .GetByIdWithPipelineVariableGroupsAsync(Arg.Any<ProjectId>(), Arg.Any<CancellationToken>());
        await _projectRepository.DidNotReceive()
            .GetByIdWithAllAsync(Arg.Any<ProjectId>(), Arg.Any<CancellationToken>());
    }
}