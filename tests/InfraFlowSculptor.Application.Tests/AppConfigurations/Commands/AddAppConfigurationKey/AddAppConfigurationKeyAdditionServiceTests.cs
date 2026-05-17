using FluentAssertions;
using InfraFlowSculptor.Application.AppConfigurations.Commands.AddAppConfigurationKey;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.AppConfigurationAggregate;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.AppConfigurations.Commands.AddAppConfigurationKey;

public sealed class AddAppConfigurationKeyAdditionServiceTests
{
    private const string VariableGroupName = "vg-shared";
    private const string PipelineVariableName = "client-secret";
    private const string ConfigurationKeyName = "Core:Authorization:ClientSecret";

    private readonly IAppConfigurationRepository _appConfigurationRepository;
    private readonly IAzureResourceRepository _azureResourceRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly DomainInfrastructureConfig _infraConfig;
    private readonly Project _project;
    private readonly AppConfiguration _appConfiguration;
    private readonly AddAppConfigurationKeyAdditionService _sut;

    public AddAppConfigurationKeyAdditionServiceTests()
    {
        _appConfigurationRepository = Substitute.For<IAppConfigurationRepository>();
        _azureResourceRepository = Substitute.For<IAzureResourceRepository>();
        _projectRepository = Substitute.For<IProjectRepository>();

        _project = Project.Create(new Name("Retail Platform"), "Provision retail assets.", UserId.CreateUnique());
        _infraConfig = DomainInfrastructureConfig.Create(new Name("primary"), _project.Id);
        var resourceGroup = DomainResourceGroup.Create(
            new Name("rg-config"),
            _infraConfig.Id,
            new Location(Location.LocationEnum.FranceCentral));
        _appConfiguration = AppConfiguration.Create(
            resourceGroup.Id,
            new Name("appcs-shared"),
            new Location(Location.LocationEnum.FranceCentral));

        _sut = new AddAppConfigurationKeyAdditionService(
            _appConfigurationRepository,
            _azureResourceRepository,
            _projectRepository);
    }

    [Fact]
    public async Task Given_VariableGroupBackedRequest_When_AddAsync_Then_PersistsKeyAndReturnsVariableGroupNameAsync()
    {
        // Arrange
        var variableGroup = _project.AddPipelineVariableGroup(VariableGroupName).Value;
        var request = new AddAppConfigurationKeyCommand(
            _appConfiguration.Id,
            ConfigurationKeyName,
            Label: "v1",
            EnvironmentValues: null,
            VariableGroupId: variableGroup.Id.Value,
            PipelineVariableName: PipelineVariableName);

        _projectRepository.GetByIdWithPipelineVariableGroupsAsync(_project.Id, Arg.Any<CancellationToken>())
            .Returns(_project);
        _appConfigurationRepository.Update(_appConfiguration)
            .Returns(_appConfiguration);

        // Act
        var result = await _sut.AddAsync(request, _appConfiguration, _infraConfig, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.IsViaVariableGroup.Should().BeTrue();
        result.Value.VariableGroupId.Should().Be(variableGroup.Id.Value);
        result.Value.PipelineVariableName.Should().Be(PipelineVariableName);
        result.Value.VariableGroupName.Should().Be(VariableGroupName);
        _appConfiguration.ConfigurationKeys.Should().ContainSingle(key =>
            key.Key == ConfigurationKeyName
            && key.PipelineVariableName == PipelineVariableName
            && key.VariableGroupId == variableGroup.Id);

        await _projectRepository.Received(1)
            .GetByIdWithPipelineVariableGroupsAsync(_project.Id, Arg.Any<CancellationToken>());
        _appConfigurationRepository.Received(1)
            .Update(_appConfiguration);
        await _azureResourceRepository.DidNotReceive()
            .GetByIdAsync(Arg.Any<AzureResourceId>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_StaticRequestWithoutEnvironmentValues_When_AddAsync_Then_PersistsKeyAndSkipsProjectLookupAsync()
    {
        // Arrange
        var request = new AddAppConfigurationKeyCommand(
            _appConfiguration.Id,
            ConfigurationKeyName,
            Label: null,
            EnvironmentValues: null);

        _appConfigurationRepository.Update(_appConfiguration)
            .Returns(_appConfiguration);

        // Act
        var result = await _sut.AddAsync(request, _appConfiguration, _infraConfig, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.EnvironmentValues.Should().BeNull();
        result.Value.IsOutputReference.Should().BeFalse();
        result.Value.IsKeyVaultReference.Should().BeFalse();
        result.Value.IsViaVariableGroup.Should().BeFalse();
        _appConfiguration.ConfigurationKeys.Should().ContainSingle(key =>
            key.Key == ConfigurationKeyName
            && key.EnvironmentValues.Count == 0);

        await _projectRepository.DidNotReceive()
            .GetByIdWithPipelineVariableGroupsAsync(Arg.Any<ProjectId>(), Arg.Any<CancellationToken>());
        _appConfigurationRepository.Received(1)
            .Update(_appConfiguration);
    }
}
