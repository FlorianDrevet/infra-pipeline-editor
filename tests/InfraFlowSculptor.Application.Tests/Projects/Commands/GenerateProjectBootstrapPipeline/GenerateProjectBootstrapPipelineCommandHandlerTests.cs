using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Projects.Commands.GenerateProjectBootstrapPipeline;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using NSubstitute;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.Projects.Commands.GenerateProjectBootstrapPipeline;

public sealed class GenerateProjectBootstrapPipelineCommandHandlerTests
{
    private readonly IProjectAccessService _accessService;
    private readonly IProjectRepository _projectRepository;
    private readonly IInfrastructureConfigReadRepository _configReadRepository;
    private readonly GenerateProjectBootstrapPipelineCommandHandler _sut;

    public GenerateProjectBootstrapPipelineCommandHandlerTests()
    {
        _accessService = Substitute.For<IProjectAccessService>();
        _projectRepository = Substitute.For<IProjectRepository>();
        _configReadRepository = Substitute.For<IInfrastructureConfigReadRepository>();

        _sut = new GenerateProjectBootstrapPipelineCommandHandler(
            _accessService,
            _projectRepository,
            _configReadRepository,
            definitionBuilder: null!,
            bootstrapEngine: null!,
            blobService: null!,
            targetResolver: null!);
    }

    [Fact]
    public async Task Given_MultiRepoLayout_When_Handle_Then_ReturnsAmbiguousProjectLevelGenerationAsync()
    {
        // Arrange
        var project = Project.Create(new Name("Retail Platform"), "Provision retail assets.", UserId.CreateUnique());
        project.SetLayoutPreset(new LayoutPreset(LayoutPresetEnum.MultiRepo));

        var command = new GenerateProjectBootstrapPipelineCommand(project.Id);
        _accessService.VerifyWriteAccessAsync(project.Id, Arg.Any<CancellationToken>())
            .Returns(project);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(Errors.GitRouting.AmbiguousProjectLevelGeneration.Code);
        await _projectRepository.DidNotReceive()
            .GetByIdWithAllAndPipelineVariableGroupsAsync(Arg.Any<ProjectId>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_ProjectReloadFails_When_Handle_Then_ReturnsNotFoundWithoutLegacyProjectLookupsAsync()
    {
        // Arrange
        var project = Project.Create(new Name("Retail Platform"), "Provision retail assets.", UserId.CreateUnique());
        project.SetLayoutPreset(new LayoutPreset(LayoutPresetEnum.AllInOne));

        var command = new GenerateProjectBootstrapPipelineCommand(project.Id);
        _accessService.VerifyWriteAccessAsync(project.Id, Arg.Any<CancellationToken>())
            .Returns(project);
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
            .GetByIdWithAllAsync(Arg.Any<ProjectId>(), Arg.Any<CancellationToken>());
        await _projectRepository.DidNotReceive()
            .GetByIdWithPipelineVariableGroupsAsync(Arg.Any<ProjectId>(), Arg.Any<CancellationToken>());
    }
}
