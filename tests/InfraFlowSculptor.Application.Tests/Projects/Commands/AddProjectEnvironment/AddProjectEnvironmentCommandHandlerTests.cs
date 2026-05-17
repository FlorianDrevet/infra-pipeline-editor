using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Projects.Commands.AddProjectEnvironment;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using MapsterMapper;
using NSubstitute;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.Projects.Commands.AddProjectEnvironment;

public sealed class AddProjectEnvironmentCommandHandlerTests
{
    private const string EnvName = "Production";

    private readonly IProjectRepository _projectRepository;
    private readonly IProjectAccessService _accessService;
    private readonly IMapper _mapper;
    private readonly Project _project;
    private readonly AddProjectEnvironmentCommand _command;
    private readonly AddProjectEnvironmentCommandHandler _sut;

    public AddProjectEnvironmentCommandHandlerTests()
    {
        _projectRepository = Substitute.For<IProjectRepository>();
        _accessService = Substitute.For<IProjectAccessService>();
        _mapper = Substitute.For<IMapper>();
        _project = Project.Create(new Name("test-project"), "Test project", UserId.CreateUnique());
        _command = new AddProjectEnvironmentCommand(
            _project.Id,
            EnvName,
            ShortName: "prd",
            Prefix: "p",
            Suffix: "01",
            Location: "FranceCentral",
            SubscriptionId: Guid.NewGuid(),
            Order: 1,
            RequiresApproval: true,
            AzureResourceManagerConnection: null,
            Tags: []);
        _sut = new AddProjectEnvironmentCommandHandler(_projectRepository, _accessService, _mapper);
    }

    [Fact]
    public async Task Given_AccessDenied_When_Handle_Then_ReturnsErrorAsync()
    {
        // Arrange
        _accessService.VerifyWriteAccessAsync(_project.Id, Arg.Any<CancellationToken>())
            .Returns(Error.Unauthorized());

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Unauthorized);
    }

    [Fact]
    public async Task Given_ProjectNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _accessService.VerifyWriteAccessAsync(_project.Id, Arg.Any<CancellationToken>())
            .Returns(_project);
        _projectRepository.GetByIdWithAllAsync(_project.Id, Arg.Any<CancellationToken>())
            .Returns((Project?)null);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Given_WriteAccessGranted_When_Handle_Then_AddsEnvironmentAndMapsResultAsync()
    {
        // Arrange
        _accessService.VerifyWriteAccessAsync(_project.Id, Arg.Any<CancellationToken>())
            .Returns(_project);
        _projectRepository.GetByIdWithAllAsync(_project.Id, Arg.Any<CancellationToken>())
            .Returns(_project);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        _projectRepository.Received(1).Update(Arg.Any<Project>());
        _mapper.Received(1).Map<ProjectEnvironmentDefinitionResult>(Arg.Any<object>());
    }
}
