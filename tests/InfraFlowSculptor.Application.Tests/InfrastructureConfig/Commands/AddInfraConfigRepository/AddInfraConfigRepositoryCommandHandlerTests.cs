using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.AddInfraConfigRepository;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.InfrastructureConfig.Commands.AddInfraConfigRepository;

public sealed class AddInfraConfigRepositoryCommandHandlerTests
{
    private readonly IInfrastructureConfigRepository _infrastructureConfigRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectAccessService _accessService;
    private readonly Project _project;
    private readonly DomainInfrastructureConfig _config;
    private readonly AddInfraConfigRepositoryCommandHandler _sut;

    public AddInfraConfigRepositoryCommandHandlerTests()
    {
        _infrastructureConfigRepository = Substitute.For<IInfrastructureConfigRepository>();
        _projectRepository = Substitute.For<IProjectRepository>();
        _accessService = Substitute.For<IProjectAccessService>();
        _project = Project.Create(new Name("Retail Platform"), "Provision retail assets.", UserId.CreateUnique());
        _config = DomainInfrastructureConfig.Create(new Name("primary"), _project.Id);

        _accessService.VerifyOwnerAccessAsync(_project.Id, Arg.Any<CancellationToken>())
            .Returns(_project);
        _projectRepository.GetByIdAsync(_project.Id)
            .Returns(_project);
        _infrastructureConfigRepository.GetByIdAsync(_config.Id)
            .Returns(_config);

        _sut = new AddInfraConfigRepositoryCommandHandler(
            _infrastructureConfigRepository,
            _projectRepository,
            _accessService);
    }

    [Fact]
    public async Task Given_InvalidProviderType_When_Handle_Then_ReturnsValidationErrorAsync()
    {
        // Arrange
        var command = new AddInfraConfigRepositoryCommand(
            _project.Id,
            _config.Id,
            Alias: "infra",
            ProviderType: "Unsupported",
            RepositoryUrl: "https://github.com/floriandrevet/infra",
            DefaultBranch: "main",
            ContentKinds: [nameof(RepositoryContentKindsEnum.Infrastructure)]);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(Errors.GitRepository.InvalidProviderType("Unsupported").Code);
        await _infrastructureConfigRepository.DidNotReceive().UpdateAsync(Arg.Any<DomainInfrastructureConfig>());
    }

    [Fact]
    public async Task Given_InvalidContentKinds_When_Handle_Then_ReturnsValidationErrorAsync()
    {
        // Arrange
        var command = new AddInfraConfigRepositoryCommand(
            _project.Id,
            _config.Id,
            Alias: "infra",
            ProviderType: nameof(GitProviderTypeEnum.GitHub),
            RepositoryUrl: "https://github.com/floriandrevet/infra",
            DefaultBranch: "main",
            ContentKinds: ["Unsupported"]);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(Errors.ProjectRepository.NoContentKind().Code);
        await _infrastructureConfigRepository.DidNotReceive().UpdateAsync(Arg.Any<DomainInfrastructureConfig>());
    }
}