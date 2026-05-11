using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Projects.Commands.SetProjectLayoutPreset;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using NSubstitute;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.Projects.Commands.SetProjectLayoutPreset;

public sealed class SetProjectLayoutPresetCommandHandlerTests
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectAccessService _accessService;
    private readonly Project _project;
    private readonly SetProjectLayoutPresetCommandHandler _sut;

    public SetProjectLayoutPresetCommandHandlerTests()
    {
        _projectRepository = Substitute.For<IProjectRepository>();
        _accessService = Substitute.For<IProjectAccessService>();
        _project = Project.Create(new Name("Retail Platform"), "Provision retail assets.", UserId.CreateUnique());

        _accessService.VerifyOwnerAccessAsync(_project.Id, Arg.Any<CancellationToken>())
            .Returns(_project);
        _projectRepository.GetByIdWithAllAsync(_project.Id, Arg.Any<CancellationToken>())
            .Returns(_project);

        _sut = new SetProjectLayoutPresetCommandHandler(_projectRepository, _accessService);
    }

    [Fact]
    public async Task Given_InvalidLayoutPreset_When_Handle_Then_ReturnsValidationErrorAsync()
    {
        // Arrange
        var command = new SetProjectLayoutPresetCommand(_project.Id, "Unsupported");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(Errors.Project.InvalidLayoutPreset("Unsupported").Code);
        await _projectRepository.DidNotReceive().UpdateAsync(Arg.Any<Project>());
    }
}