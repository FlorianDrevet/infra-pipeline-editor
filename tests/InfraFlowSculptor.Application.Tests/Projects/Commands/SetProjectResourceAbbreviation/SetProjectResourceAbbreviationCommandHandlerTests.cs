using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Projects.Commands.SetProjectResourceAbbreviation;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using MapsterMapper;
using NSubstitute;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.Projects.Commands.SetProjectResourceAbbreviation;

public sealed class SetProjectResourceAbbreviationCommandHandlerTests
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectAccessService _accessService;
    private readonly IMapper _mapper;
    private readonly Project _project;
    private readonly SetProjectResourceAbbreviationCommandHandler _sut;

    public SetProjectResourceAbbreviationCommandHandlerTests()
    {
        _projectRepository = Substitute.For<IProjectRepository>();
        _accessService = Substitute.For<IProjectAccessService>();
        _mapper = Substitute.For<IMapper>();
        _project = Project.Create(new Name("Retail Platform"), "Provision retail assets.", UserId.CreateUnique());

        _sut = new SetProjectResourceAbbreviationCommandHandler(_projectRepository, _accessService, _mapper);
    }

    [Fact]
    public async Task Given_ValidAbbreviation_When_Handle_Then_ReturnsMappedResultAsync()
    {
        // Arrange
        _accessService.VerifyWriteAccessAsync(_project.Id, Arg.Any<CancellationToken>())
            .Returns(_project);
        _projectRepository.GetByIdWithAllAsync(_project.Id, Arg.Any<CancellationToken>())
            .Returns(_project);
        var expectedResult = new ProjectResourceAbbreviationResult(
            ProjectResourceAbbreviationId.CreateUnique(), "KeyVault", "kv");
        _mapper.Map<ProjectResourceAbbreviationResult>(Arg.Any<object>())
            .Returns(expectedResult);
        var command = new SetProjectResourceAbbreviationCommand(_project.Id, "KeyVault", "kv");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeSameAs(expectedResult);
        _projectRepository.Received(1).Update(Arg.Is<Project>(p => p.Id == _project.Id));
    }

    [Fact]
    public async Task Given_AccessDenied_When_Handle_Then_ReturnsErrorAsync()
    {
        // Arrange
        _accessService.VerifyWriteAccessAsync(_project.Id, Arg.Any<CancellationToken>())
            .Returns(Error.Unauthorized());
        var command = new SetProjectResourceAbbreviationCommand(_project.Id, "KeyVault", "kv");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Unauthorized);
        _projectRepository.DidNotReceive().Update(Arg.Any<Project>());
    }

    [Fact]
    public async Task Given_ProjectNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _accessService.VerifyWriteAccessAsync(_project.Id, Arg.Any<CancellationToken>())
            .Returns(_project);
        _projectRepository.GetByIdWithAllAsync(_project.Id, Arg.Any<CancellationToken>())
            .Returns((Project?)null);
        var command = new SetProjectResourceAbbreviationCommand(_project.Id, "KeyVault", "kv");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        _projectRepository.DidNotReceive().Update(Arg.Any<Project>());
    }
}
