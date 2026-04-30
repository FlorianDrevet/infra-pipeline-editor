using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.CreateInfraConfig;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using MapsterMapper;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.InfrastructureConfig.Commands.CreateInfraConfig;

public sealed class CreateInfrastructureConfigCommandHandlerTests
{
    private const string ConfigName = "primary";

    private readonly IInfrastructureConfigRepository _repository;
    private readonly IProjectAccessService _accessService;
    private readonly IMapper _mapper;
    private readonly Project _project;
    private readonly Guid _projectGuid;
    private readonly CreateInfrastructureConfigCommandHandler _sut;

    public CreateInfrastructureConfigCommandHandlerTests()
    {
        _repository = Substitute.For<IInfrastructureConfigRepository>();
        _accessService = Substitute.For<IProjectAccessService>();
        _mapper = Substitute.For<IMapper>();
        _project = Project.Create(new Name("RetailApi"), null, UserId.CreateUnique());
        _projectGuid = _project.Id.Value;
        _repository.AddAsync(Arg.Any<DomainInfrastructureConfig>())
            .Returns(callInfo => Task.FromResult((DomainInfrastructureConfig)callInfo.Args()[0]));
        _sut = new CreateInfrastructureConfigCommandHandler(_repository, _accessService, _mapper);
    }

    [Fact]
    public async Task Given_WriteAccessGranted_When_Handle_Then_PersistsConfigAndReturnsMappedResultAsync()
    {
        // Arrange
        _accessService.VerifyWriteAccessAsync(Arg.Any<ProjectId>(), Arg.Any<CancellationToken>())
            .Returns(_project);
        var expectedDto = BuildExpectedResult();
        _mapper.Map<GetInfrastructureConfigResult>(Arg.Any<DomainInfrastructureConfig>())
            .Returns(expectedDto);
        var command = new CreateInfrastructureConfigCommand(ConfigName, _projectGuid);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeSameAs(expectedDto);
        await _repository.Received(1).AddAsync(Arg.Is<DomainInfrastructureConfig>(c =>
            c.Name.Value == ConfigName && c.ProjectId.Value == _projectGuid));
    }

    [Fact]
    public async Task Given_WriteAccessDenied_When_Handle_Then_ReturnsErrorAndDoesNotPersistAsync()
    {
        // Arrange
        _accessService.VerifyWriteAccessAsync(Arg.Any<ProjectId>(), Arg.Any<CancellationToken>())
            .Returns(Errors.Project.ForbiddenError());
        var command = new CreateInfrastructureConfigCommand(ConfigName, _projectGuid);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Forbidden);
        await _repository.DidNotReceive().AddAsync(Arg.Any<DomainInfrastructureConfig>());
    }

    private static GetInfrastructureConfigResult BuildExpectedResult() => new(
        InfrastructureConfigId.CreateUnique(),
        new Name(ConfigName),
        ProjectId.CreateUnique(),
        DefaultNamingTemplate: null,
        UseProjectNamingConventions: true,
        ResourceNamingTemplates: [],
        ResourceAbbreviationOverrides: [],
        Tags: []);
}
