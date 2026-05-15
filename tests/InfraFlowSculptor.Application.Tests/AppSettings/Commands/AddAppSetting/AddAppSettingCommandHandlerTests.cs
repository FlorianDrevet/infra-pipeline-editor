using FluentAssertions;
using InfraFlowSculptor.Application.AppSettings.Commands.AddAppSetting;
using InfraFlowSculptor.Application.AppSettings.Common;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using InfraFlowSculptor.Domain.WebAppAggregate;
using InfraFlowSculptor.Domain.WebAppAggregate.ValueObjects;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.AppSettings.Commands.AddAppSetting;

public sealed class AddAppSettingCommandHandlerTests
{
    private readonly IAzureResourceRepository _azureResourceRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly IAddAppSettingAdditionService _additionService;
    private readonly DomainInfrastructureConfig _infraConfig;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly WebApp _resource;
    private readonly AddAppSettingCommand _command;
    private readonly AddAppSettingCommandHandler _sut;

    public AddAppSettingCommandHandlerTests()
    {
        _azureResourceRepository = Substitute.For<IAzureResourceRepository>();
        _resourceGroupRepository = Substitute.For<IResourceGroupRepository>();
        _accessService = Substitute.For<IInfraConfigAccessService>();
        _additionService = Substitute.For<IAddAppSettingAdditionService>();

        var project = Project.Create(new Name("Retail Platform"), "Provision retail assets.", UserId.CreateUnique());
        _infraConfig = DomainInfrastructureConfig.Create(new Name("primary"), project.Id);
        _resourceGroup = DomainResourceGroup.Create(
            new Name("rg-appsettings"),
            _infraConfig.Id,
            new Location(Location.LocationEnum.FranceCentral));
        _resource = WebApp.Create(
            _resourceGroup.Id,
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
        _command = new AddAppSettingCommand(
            _resource.Id,
            "ConnectionStrings__Api",
            EnvironmentValues: new Dictionary<string, string>
            {
                ["prod"] = "https://api.example",
            },
            SourceResourceId: null,
            SourceOutputName: null,
            KeyVaultResourceId: null,
            SecretName: null);

        _sut = new AddAppSettingCommandHandler(
            _azureResourceRepository,
            _resourceGroupRepository,
            _accessService,
            _additionService);
    }

    [Fact]
    public async Task Given_ValidUniqueRequest_When_Handle_Then_DelegatesToAdditionServiceAsync()
    {
        // Arrange
        var expected = new AppSettingResult(
            AppSettingId.CreateUnique(),
            _resource.Id,
            _command.Name,
            _command.EnvironmentValues,
            SourceResourceId: null,
            SourceOutputName: null,
            IsOutputReference: false,
            KeyVaultResourceId: null,
            SecretName: null,
            IsKeyVaultReference: false,
            HasKeyVaultAccess: null,
            SecretValueAssignment: null,
            VariableGroupId: null,
            PipelineVariableName: null,
            VariableGroupName: null,
            IsViaVariableGroup: false);

        _azureResourceRepository.GetByIdWithAppSettingsAsync(_resource.Id, Arg.Any<CancellationToken>())
            .Returns(_resource);
        _resourceGroupRepository.GetByIdAsync(_resourceGroup.Id, Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_infraConfig.Id, Arg.Any<CancellationToken>())
            .Returns(_infraConfig);
        _additionService.AddAsync(_command, _resource, _infraConfig, Arg.Any<CancellationToken>())
            .Returns(expected);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(expected);

        await _additionService.Received(1)
            .AddAsync(_command, _resource, _infraConfig, Arg.Any<CancellationToken>());
    }
}