using FluentAssertions;
using InfraFlowSculptor.Application.AppSettings.Commands.AddAppSetting;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using InfraFlowSculptor.Domain.WebAppAggregate;
using InfraFlowSculptor.Domain.WebAppAggregate.ValueObjects;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.AppSettings.Commands.AddAppSetting;

public sealed class AddAppSettingAdditionServiceTests
{
    private const string VariableGroupName = "vg-shared";
    private const string PipelineVariableName = "api-password";
    private const string StaticAppSettingName = "ConnectionStrings__Api";
    private const string StaticEnvironmentName = "prod";
    private const string StaticEnvironmentValue = "https://api.example";

    private readonly IAzureResourceRepository _azureResourceRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly DomainInfrastructureConfig _infraConfig;
    private readonly Project _project;
    private readonly WebApp _resource;
    private readonly AddAppSettingAdditionService _sut;

    public AddAppSettingAdditionServiceTests()
    {
        _azureResourceRepository = Substitute.For<IAzureResourceRepository>();
        _projectRepository = Substitute.For<IProjectRepository>();

        _project = Project.Create(new Name("Retail Platform"), "Provision retail assets.", UserId.CreateUnique());
        _infraConfig = DomainInfrastructureConfig.Create(new Name("primary"), _project.Id);
        var resourceGroup = DomainResourceGroup.Create(
            new Name("rg-appsettings"),
            _infraConfig.Id,
            new Location(Location.LocationEnum.FranceCentral));
        _resource = WebApp.Create(
            resourceGroup.Id,
            new Name("web-shared"),
            new Location(Location.LocationEnum.FranceCentral),
            AzureResourceId.CreateUnique(),
            new WebAppRuntimeStack(WebAppRuntimeStack.WebAppRuntimeStackEnum.DotNet),
            "8.0",
            alwaysOn: true,
            httpsOnly: true,
            new DeploymentMode(DeploymentMode.DeploymentModeType.Code),
            containerRegistryId: null,
            acrAuthMode: null,
            dockerImageName: null);

        _sut = new AddAppSettingAdditionService(_azureResourceRepository, _projectRepository);
    }

    [Fact]
    public async Task Given_VariableGroupBackedRequest_When_AddAsync_Then_PersistsSettingAndReturnsVariableGroupNameAsync()
    {
        // Arrange
        var variableGroup = _project.AddPipelineVariableGroup(VariableGroupName).Value;
        var request = new AddAppSettingCommand(
            _resource.Id,
            StaticAppSettingName,
            EnvironmentValues: null,
            SourceResourceId: null,
            SourceOutputName: null,
            KeyVaultResourceId: null,
            SecretName: null,
            VariableGroupId: variableGroup.Id.Value,
            PipelineVariableName: PipelineVariableName);

        _projectRepository.GetByIdWithPipelineVariableGroupsAsync(_project.Id, Arg.Any<CancellationToken>())
            .Returns(_project);
        _azureResourceRepository.UpdateAsync(_resource, Arg.Any<CancellationToken>())
            .Returns(_resource);

        // Act
        var result = await _sut.AddAsync(request, _resource, _infraConfig, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.IsViaVariableGroup.Should().BeTrue();
        result.Value.VariableGroupId.Should().Be(variableGroup.Id.Value);
        result.Value.PipelineVariableName.Should().Be(PipelineVariableName);
        result.Value.VariableGroupName.Should().Be(VariableGroupName);
        _resource.AppSettings.Should().ContainSingle(setting =>
            setting.Name == StaticAppSettingName
            && setting.PipelineVariableName == PipelineVariableName
            && setting.VariableGroupId == variableGroup.Id);

        await _projectRepository.Received(1)
            .GetByIdWithPipelineVariableGroupsAsync(_project.Id, Arg.Any<CancellationToken>());
        await _azureResourceRepository.Received(1)
            .UpdateAsync(_resource, Arg.Any<CancellationToken>());
        await _azureResourceRepository.DidNotReceive()
            .GetByIdAsync(Arg.Any<AzureResourceId>(), Arg.Any<CancellationToken>());
        await _azureResourceRepository.DidNotReceive()
            .GetByIdWithRoleAssignmentsAsync(Arg.Any<AzureResourceId>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_StaticRequestWithoutEnvironmentValues_When_AddAsync_Then_PersistsStaticSettingAndSkipsProjectLookupAsync()
    {
        // Arrange
        var request = new AddAppSettingCommand(
            _resource.Id,
            StaticAppSettingName,
            EnvironmentValues: null,
            SourceResourceId: null,
            SourceOutputName: null,
            KeyVaultResourceId: null,
            SecretName: null);

        _azureResourceRepository.UpdateAsync(_resource, Arg.Any<CancellationToken>())
            .Returns(_resource);

        // Act
        var result = await _sut.AddAsync(request, _resource, _infraConfig, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.EnvironmentValues.Should().BeNull();
        result.Value.IsOutputReference.Should().BeFalse();
        result.Value.IsKeyVaultReference.Should().BeFalse();
        result.Value.IsViaVariableGroup.Should().BeFalse();
        _resource.AppSettings.Should().ContainSingle(setting =>
            setting.Name == StaticAppSettingName
            && setting.EnvironmentValues.Count == 0);

        await _projectRepository.DidNotReceive()
            .GetByIdWithPipelineVariableGroupsAsync(Arg.Any<ProjectId>(), Arg.Any<CancellationToken>());
        await _azureResourceRepository.Received(1)
            .UpdateAsync(_resource, Arg.Any<CancellationToken>());
    }
}