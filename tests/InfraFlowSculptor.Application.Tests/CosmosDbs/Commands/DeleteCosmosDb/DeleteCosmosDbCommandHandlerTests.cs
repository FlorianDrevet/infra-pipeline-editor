using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.CosmosDbs.Commands.DeleteCosmosDb;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.CosmosDbAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.CosmosDbs.Commands.DeleteCosmosDb;

public sealed class DeleteCosmosDbCommandHandlerTests
{
    private readonly ICosmosDbRepository _cosmosDbRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly CosmosDb _cosmosDb;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly DomainInfrastructureConfig _config;
    private readonly DeleteCosmosDbCommand _command;
    private readonly DeleteCosmosDbCommandHandler _sut;

    public DeleteCosmosDbCommandHandlerTests()
    {
        _cosmosDbRepository = Substitute.For<ICosmosDbRepository>();
        _resourceGroupRepository = Substitute.For<IResourceGroupRepository>();
        _accessService = Substitute.For<IInfraConfigAccessService>();
        _config = DomainInfrastructureConfig.Create(new Name("primary"), ProjectId.CreateUnique());
        _resourceGroup = DomainResourceGroup.Create(
            new Name("rg-shared"),
            _config.Id,
            new Location(Location.LocationEnum.FranceCentral));
        _cosmosDb = CosmosDb.Create(
            _resourceGroup.Id,
            new Name("cosmos-shared"),
            new Location(Location.LocationEnum.FranceCentral));
        _command = new DeleteCosmosDbCommand(_cosmosDb.Id);
        _sut = new DeleteCosmosDbCommandHandler(
            _cosmosDbRepository, _resourceGroupRepository, _accessService);
    }

    [Fact]
    public async Task Given_CosmosDbNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _cosmosDbRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns((CosmosDb?)null);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        await _cosmosDbRepository.DidNotReceive().DeleteAsync(Arg.Any<AzureResourceId>());
    }

    [Fact]
    public async Task Given_WriteAccessGranted_When_Handle_Then_DeletesCosmosDbAsync()
    {
        // Arrange
        _cosmosDbRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_cosmosDb);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Deleted);
        await _cosmosDbRepository.Received(1).DeleteAsync(_cosmosDb.Id);
    }
}
