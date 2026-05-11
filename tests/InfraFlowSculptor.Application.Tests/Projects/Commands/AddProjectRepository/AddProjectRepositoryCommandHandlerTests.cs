using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Projects.Commands.AddProjectRepository;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using NSubstitute;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.Projects.Commands.AddProjectRepository;

public sealed class AddProjectRepositoryCommandHandlerTests
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectAccessService _accessService;
    private readonly Project _project;
    private readonly AddProjectRepositoryCommandHandler _sut;

    public AddProjectRepositoryCommandHandlerTests()
    {
        _projectRepository = Substitute.For<IProjectRepository>();
        _accessService = Substitute.For<IProjectAccessService>();
        _project = Project.Create(new Name("Retail Platform"), "Provision retail assets.", UserId.CreateUnique());

        _accessService.VerifyOwnerAccessAsync(_project.Id, Arg.Any<CancellationToken>())
            .Returns(_project);
        _projectRepository.GetByIdWithAllAsync(_project.Id, Arg.Any<CancellationToken>())
            .Returns(_project);

        _sut = new AddProjectRepositoryCommandHandler(_projectRepository, _accessService);
    }

    [Fact]
    public async Task Given_InvalidProviderType_When_Handle_Then_ReturnsValidationErrorAsync()
    {
        // Arrange
        var command = new AddProjectRepositoryCommand(
            _project.Id,
            Alias: "infra",
            ProviderType: "Unsupported",
            RepositoryUrl: "https://github.com/floriandrevet/infra",
            DefaultBranch: "main",
            ContentKinds: [nameof(RepositoryContentKindsEnum.Infrastructure), nameof(RepositoryContentKindsEnum.ApplicationCode)]);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(Errors.GitRepository.InvalidProviderType("Unsupported").Code);
        await _projectRepository.DidNotReceive().UpdateAsync(Arg.Any<Project>());
    }

    [Fact]
    public async Task Given_InvalidContentKinds_When_Handle_Then_ReturnsValidationErrorAsync()
    {
        // Arrange
        var command = new AddProjectRepositoryCommand(
            _project.Id,
            Alias: "infra",
            ProviderType: null,
            RepositoryUrl: null,
            DefaultBranch: null,
            ContentKinds: ["Unsupported"]);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(Errors.ProjectRepository.NoContentKind().Code);
        await _projectRepository.DidNotReceive().UpdateAsync(Arg.Any<Project>());
    }
}