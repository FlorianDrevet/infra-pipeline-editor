using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.CosmosDbs.Commands.CreateCosmosDb;
using InfraFlowSculptor.Application.CosmosDbs.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.CosmosDbAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using MapsterMapper;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.CosmosDbs.Commands.CreateCosmosDb;

public sealed class CreateCosmosDbCommandHandlerTests
{
    private const string CosmosDbName = "cosmos-shared";

    private readonly ICosmosDbRepository _cosmosDbRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly IMapper _mapper;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly DomainInfrastructureConfig _config;
    private readonly CreateCosmosDbCommand _command;
    private readonly CreateCosmosDbCommandHandler _sut;

    public CreateCosmosDbCommandHandlerTests()
    {
        _cosmosDbRepository = Substitute.For<ICosmosDbRepository>();
        _resourceGroupRepository = Substitute.For<IResourceGroupRepository>();
        _accessService = Substitute.For<IInfraConfigAccessService>();
        _mapper = Substitute.For<IMapper>();
        _config = DomainInfrastructureConfig.Create(new Name("primary"), ProjectId.CreateUnique());
        _resourceGroup = DomainResourceGroup.Create(
            new Name("rg-shared"),
            _config.Id,
            new Location(Location.LocationEnum.FranceCentral));
        _command = new CreateCosmosDbCommand(
            _resourceGroup.Id,
            new Name(CosmosDbName),
            new Location(Location.LocationEnum.FranceCentral));
        _cosmosDbRepository.AddAsync(Arg.Any<CosmosDb>())
            .Returns(callInfo => Task.FromResult((CosmosDb)callInfo.Args()[0]));
        _sut = new CreateCosmosDbCommandHandler(
            _cosmosDbRepository, _resourceGroupRepository, _accessService, _mapper);
    }

    [Fact]
    public async Task Given_ResourceGroupNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns((DomainResourceGroup?)null);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        await _cosmosDbRepository.DidNotReceive().AddAsync(Arg.Any<CosmosDb>());
    }

    [Fact]
    public async Task Given_WriteAccessGranted_When_Handle_Then_PersistsCosmosDbAndMapsResultAsync()
    {
        // Arrange
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        await _cosmosDbRepository.Received(1).AddAsync(Arg.Is<CosmosDb>(c =>
            c.ResourceGroupId == _resourceGroup.Id && c.Name.Value == CosmosDbName));
        _mapper.Received(1).Map<CosmosDbResult>(Arg.Any<CosmosDb>());
    }
}
