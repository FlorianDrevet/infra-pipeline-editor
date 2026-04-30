using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.ResourceGroup.Commands.CreateResourceGroup;
using InfraFlowSculptor.Application.ResourceGroups.Commands.CreateResourceGroup;
using InfraFlowSculptor.Application.ResourceGroups.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using MapsterMapper;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.ResourceGroups.Commands.CreateResourceGroup;

public sealed class CreateResourceGroupCommandHandlerTests
{
    private const string ResourceGroupName = "rg-shared";

    private readonly IResourceGroupRepository _repository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly IMapper _mapper;
    private readonly InfrastructureConfigId _infraConfigId;
    private readonly Name _name;
    private readonly Location _location;
    private readonly DomainInfrastructureConfig _config;
    private readonly CreateResourceGroupCommandHandler _sut;

    public CreateResourceGroupCommandHandlerTests()
    {
        _repository = Substitute.For<IResourceGroupRepository>();
        _accessService = Substitute.For<IInfraConfigAccessService>();
        _mapper = Substitute.For<IMapper>();
        _config = DomainInfrastructureConfig.Create(new Name("primary"), ProjectId.CreateUnique());
        _infraConfigId = _config.Id;
        _name = new Name(ResourceGroupName);
        _location = new Location(Location.LocationEnum.FranceCentral);
        _repository.AddAsync(Arg.Any<DomainResourceGroup>())
            .Returns(callInfo => Task.FromResult((DomainResourceGroup)callInfo.Args()[0]));
        _sut = new CreateResourceGroupCommandHandler(_repository, _accessService, _mapper);
    }

    [Fact]
    public async Task Given_WriteAccessGranted_When_Handle_Then_PersistsResourceGroupAsync()
    {
        // Arrange
        _accessService.VerifyWriteAccessAsync(_infraConfigId, Arg.Any<CancellationToken>())
            .Returns(_config);
        var expectedDto = BuildExpectedResult();
        _mapper.Map<ResourceGroupResult>(Arg.Any<DomainResourceGroup>()).Returns(expectedDto);
        var command = new CreateResourceGroupCommand(_infraConfigId, _name, _location);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeSameAs(expectedDto);
        await _repository.Received(1).AddAsync(Arg.Is<DomainResourceGroup>(rg =>
            rg.Name == _name && rg.InfraConfigId == _infraConfigId && rg.Location == _location));
    }

    [Fact]
    public async Task Given_WriteAccessDenied_When_Handle_Then_ReturnsErrorAndDoesNotPersistAsync()
    {
        // Arrange
        _accessService.VerifyWriteAccessAsync(_infraConfigId, Arg.Any<CancellationToken>())
            .Returns(Errors.InfrastructureConfig.ForbiddenError());
        var command = new CreateResourceGroupCommand(_infraConfigId, _name, _location);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Forbidden);
        await _repository.DidNotReceive().AddAsync(Arg.Any<DomainResourceGroup>());
    }

    private ResourceGroupResult BuildExpectedResult() => new(
        Domain.ResourceGroupAggregate.ValueObjects.ResourceGroupId.CreateUnique(),
        _infraConfigId,
        _location,
        _name,
        []);
}
