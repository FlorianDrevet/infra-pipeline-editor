using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.ResourceGroups.Common;
using InfraFlowSculptor.Application.ResourceGroups.Queries.ListResourceGroupResources;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.ResourceGroups.Queries.ListResourceGroupResources;

public sealed class ListResourceGroupResourcesQueryHandlerTests
{
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly DomainInfrastructureConfig _config;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly ListResourceGroupResourcesQueryHandler _sut;

    public ListResourceGroupResourcesQueryHandlerTests()
    {
        _resourceGroupRepository = Substitute.For<IResourceGroupRepository>();
        _accessService = Substitute.For<IInfraConfigAccessService>();
        _config = DomainInfrastructureConfig.Create(new Name("primary"), ProjectId.CreateUnique());
        _resourceGroup = DomainResourceGroup.Create(
            new Name("rg-shared"),
            _config.Id,
            new Location(Location.LocationEnum.FranceCentral));
        _sut = new ListResourceGroupResourcesQueryHandler(_resourceGroupRepository, _accessService);
    }

    [Fact]
    public async Task Given_ReadAccessGranted_When_Handle_Then_UsesReadOnlyLookupAsync()
    {
        // Arrange
        var query = new ListResourceGroupResourcesQuery(_resourceGroup.Id);
        _resourceGroupRepository.GetByIdReadOnlyAsync(query.Id, Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyReadAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);
        _resourceGroupRepository.GetChildToParentMappingAsync(query.Id, Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, Guid>());
        _resourceGroupRepository.GetConfiguredEnvironmentsByResourceGroupAsync(query.Id, Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, List<string>>());
        _resourceGroupRepository.GetResourceSummariesByGroupIdAsync(query.Id, Arg.Any<CancellationToken>())
            .Returns([]);
        _resourceGroupRepository.GetStorageSubResourcesByStorageAccountIdsAsync(
                Arg.Any<IReadOnlyList<InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects.AzureResourceId>>(),
                Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, StorageAccountSubResourcesResult>());

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEmpty();
        await _resourceGroupRepository.Received(1)
            .GetByIdReadOnlyAsync(query.Id, Arg.Any<CancellationToken>());
        await _resourceGroupRepository.DidNotReceive()
            .GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>());
    }
}
