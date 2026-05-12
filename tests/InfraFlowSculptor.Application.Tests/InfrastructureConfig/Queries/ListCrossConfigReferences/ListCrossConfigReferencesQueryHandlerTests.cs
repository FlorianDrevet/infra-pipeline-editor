using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.Application.InfrastructureConfig.Queries.ListCrossConfigReferences;
using InfraFlowSculptor.Application.ResourceGroups.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.InfrastructureConfig.Queries.ListCrossConfigReferences;

public sealed class ListCrossConfigReferencesQueryHandlerTests
{
    private const string SourceConfigName = "source-config";
    private const string TargetConfigAName = "target-config-a";
    private const string TargetConfigBName = "target-config-b";

    private readonly IInfraConfigAccessService _accessService;
    private readonly IInfrastructureConfigRepository _infrastructureConfigRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly InfrastructureConfigId _sourceConfigId;
    private readonly InfrastructureConfigId _targetConfigAId;
    private readonly InfrastructureConfigId _targetConfigBId;
    private readonly AzureResourceId _targetResourceAId;
    private readonly AzureResourceId _targetResourceBId;
    private readonly AzureResourceId _targetResourceCId;
    private readonly DomainInfrastructureConfig _sourceConfig;
    private readonly ListCrossConfigReferencesQueryHandler _sut;

    public ListCrossConfigReferencesQueryHandlerTests()
    {
        _accessService = Substitute.For<IInfraConfigAccessService>();
        _infrastructureConfigRepository = Substitute.For<IInfrastructureConfigRepository>();
        _resourceGroupRepository = Substitute.For<IResourceGroupRepository>();

        var projectId = ProjectId.CreateUnique();
        _sourceConfig = DomainInfrastructureConfig.Create(new Name(SourceConfigName), projectId);
        _sourceConfigId = _sourceConfig.Id;
        _targetConfigAId = InfrastructureConfigId.CreateUnique();
        _targetConfigBId = InfrastructureConfigId.CreateUnique();
        _targetResourceAId = AzureResourceId.CreateUnique();
        _targetResourceBId = AzureResourceId.CreateUnique();
        _targetResourceCId = AzureResourceId.CreateUnique();

        _sourceConfig.AddCrossConfigReference(_targetConfigAId, _targetResourceAId);
        _sourceConfig.AddCrossConfigReference(_targetConfigAId, _targetResourceBId);
        _sourceConfig.AddCrossConfigReference(_targetConfigBId, _targetResourceCId);

        _sut = new ListCrossConfigReferencesQueryHandler(
            _accessService,
            _infrastructureConfigRepository,
            _resourceGroupRepository);
    }

    [Fact]
    public async Task Given_MultipleReferencesSharingTargetConfigs_When_Handle_Then_LoadsTargetConfigsInOneBatchAsync()
    {
        // Arrange
        _accessService.VerifyReadAccessAsync(_sourceConfigId, Arg.Any<CancellationToken>())
            .Returns(_sourceConfig);
        _infrastructureConfigRepository.GetByIdWithMembersAsync(_sourceConfigId, Arg.Any<CancellationToken>())
            .Returns(_sourceConfig);
        _infrastructureConfigRepository.GetConfigSummariesByIdsAsync(
                Arg.Any<IReadOnlyList<InfrastructureConfigId>>(),
                Arg.Any<CancellationToken>())
            .Returns(new List<InfraConfigSummary>
            {
                new(_targetConfigAId.Value, TargetConfigAName),
                new(_targetConfigBId.Value, TargetConfigBName),
            });
        _resourceGroupRepository.GetResourceMetadataBatchAsync(
                Arg.Any<IReadOnlyList<AzureResourceId>>(),
                Arg.Any<CancellationToken>())
            .Returns(new List<ResourceMetadata>
            {
                new(_targetResourceAId.Value, "kv-shared", "KeyVault", "rg-shared-a"),
                new(_targetResourceBId.Value, "appconfig-shared", "AppConfiguration", "rg-shared-a"),
                new(_targetResourceCId.Value, "sb-shared", "ServiceBusNamespace", "rg-shared-b"),
            });
        var query = new ListCrossConfigReferencesQuery(_sourceConfigId.Value);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().HaveCount(3);

        await _infrastructureConfigRepository.Received(1).GetConfigSummariesByIdsAsync(
            Arg.Is<IReadOnlyList<InfrastructureConfigId>>(ids =>
                ids.Count == 2
                && ids.Contains(_targetConfigAId)
                && ids.Contains(_targetConfigBId)),
            Arg.Any<CancellationToken>());
    }
}