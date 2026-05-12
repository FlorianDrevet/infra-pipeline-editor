using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.InfrastructureConfig.ReadModels;
using InfraFlowSculptor.Application.Projects.Commands.GenerateProjectBicep;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using NSubstitute;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.Projects.Commands.GenerateProjectBicep;

public sealed class GenerateProjectBicepCommandHandlerTests
{
    private readonly IProjectAccessService _accessService;
    private readonly IInfrastructureConfigReadRepository _configReadRepository;
    private readonly Project _project;
    private readonly GenerateProjectBicepCommandHandler _sut;

    public GenerateProjectBicepCommandHandlerTests()
    {
        _accessService = Substitute.For<IProjectAccessService>();
        _configReadRepository = Substitute.For<IInfrastructureConfigReadRepository>();
        _project = Project.Create(new Name("Retail Platform"), "Provision retail assets.", UserId.CreateUnique());
        _project.SetLayoutPreset(new LayoutPreset(LayoutPresetEnum.MultiRepo));

        _sut = new GenerateProjectBicepCommandHandler(
            _accessService,
            _configReadRepository,
            bicepGenerationEngine: null!,
            blobService: null!);
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