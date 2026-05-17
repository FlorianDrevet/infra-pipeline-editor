using FluentAssertions;
using InfraFlowSculptor.Application.AppConfigurations.Queries.ListAppConfigurationKeys;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.AppConfigurationAggregate;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.AppConfigurations.Queries.ListAppConfigurationKeys;

public sealed class ListAppConfigurationKeysQueryHandlerTests
{
    private readonly IAppConfigurationRepository _appConfigurationRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly IProjectRepository _projectRepository;
    private readonly DomainInfrastructureConfig _config;
    private readonly Project _project;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly AppConfiguration _appConfiguration;
    private readonly ListAppConfigurationKeysQuery _query;
    private readonly ListAppConfigurationKeysQueryHandler _sut;

    public ListAppConfigurationKeysQueryHandlerTests()
    {
        _appConfigurationRepository = Substitute.For<IAppConfigurationRepository>();
        _resourceGroupRepository = Substitute.For<IResourceGroupRepository>();
        _accessService = Substitute.For<IInfraConfigAccessService>();
        _projectRepository = Substitute.For<IProjectRepository>();

        _project = Project.Create(new Name("Retail Platform"), "Provision retail assets.", UserId.CreateUnique());
        _config = DomainInfrastructureConfig.Create(new Name("primary"), _project.Id);
        _resourceGroup = DomainResourceGroup.Create(
            new Name("rg-config"),
            _config.Id,
            new Location(Location.LocationEnum.FranceCentral));
        _appConfiguration = AppConfiguration.Create(
            _resourceGroup.Id,
            new Name("appcs-shared"),
            new Location(Location.LocationEnum.FranceCentral));

        var variableGroup = _project.AddPipelineVariableGroup("vg-shared").Value;
        _appConfiguration.AddViaVariableGroupConfigurationKey(
            "Core:Auth:ClientSecret",
            label: null,
            variableGroup.Id,
            "client-secret");

        _query = new ListAppConfigurationKeysQuery(_appConfiguration.Id);
        _sut = new ListAppConfigurationKeysQueryHandler(
            _appConfigurationRepository,
            _resourceGroupRepository,
            _accessService,
            _projectRepository);
    }

    [Fact]
    public async Task Given_VariableGroupKeys_When_Handle_Then_UsesReadOnlyResourceGroupLookupAsync()
    {
        // Arrange
        _appConfigurationRepository.GetByIdWithConfigurationKeysAndRoleAssignmentsAsync(
                _appConfiguration.Id,
                Arg.Any<CancellationToken>())
            .Returns(_appConfiguration);
        _resourceGroupRepository.GetByIdReadOnlyAsync(
                _resourceGroup.Id,
                Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyReadAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);
        _projectRepository.GetByIdWithPipelineVariableGroupsAsync(
                _project.Id,
                Arg.Any<CancellationToken>())
            .Returns(_project);

        // Act
        var result = await _sut.Handle(_query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().ContainSingle();
        result.Value[0].VariableGroupName.Should().Be("vg-shared");

        await _resourceGroupRepository.Received(1).GetByIdReadOnlyAsync(
            _resourceGroup.Id,
            Arg.Any<CancellationToken>());
        await _resourceGroupRepository.DidNotReceive().GetByIdAsync(
            _resourceGroup.Id,
            Arg.Any<CancellationToken>());
    }
}
