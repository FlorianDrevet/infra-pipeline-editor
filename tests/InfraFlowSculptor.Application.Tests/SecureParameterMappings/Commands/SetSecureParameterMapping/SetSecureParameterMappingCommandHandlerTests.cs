using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.SecureParameterMappings.Commands.SetSecureParameterMapping;
using InfraFlowSculptor.Domain.Common.BaseModels;
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

namespace InfraFlowSculptor.Application.Tests.SecureParameterMappings.Commands.SetSecureParameterMapping;

public sealed class SetSecureParameterMappingCommandHandlerTests
{
    private readonly IAzureResourceRepository _azureResourceRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly DomainInfrastructureConfig _config;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly Project _project;
    private readonly WebApp _resource;
    private readonly SetSecureParameterMappingCommandHandler _sut;

    public SetSecureParameterMappingCommandHandlerTests()
    {
        _azureResourceRepository = Substitute.For<IAzureResourceRepository>();
        _resourceGroupRepository = Substitute.For<IResourceGroupRepository>();
        _projectRepository = Substitute.For<IProjectRepository>();
        _accessService = Substitute.For<IInfraConfigAccessService>();

        _project = Project.Create(new Name("Retail Platform"), "Provision retail assets.", UserId.CreateUnique());
        _config = DomainInfrastructureConfig.Create(new Name("primary"), _project.Id);
        _resourceGroup = DomainResourceGroup.Create(
            new Name("rg-test"),
            _config.Id,
            new Location(Location.LocationEnum.FranceCentral));
        _resource = WebApp.Create(
            _resourceGroup.Id,
            new Name("web-test"),
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

        _sut = new SetSecureParameterMappingCommandHandler(
            _azureResourceRepository,
            _resourceGroupRepository,
            _projectRepository,
            _accessService);
    }

    [Fact]
    public async Task Given_ResourceNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _azureResourceRepository.GetByIdWithSecureParameterMappingsAsync(_resource.Id, Arg.Any<CancellationToken>())
            .Returns((AzureResource?)null);
        var command = new SetSecureParameterMappingCommand(_resource.Id, "adminPassword", null, null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Given_ResourceGroupNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _azureResourceRepository.GetByIdWithSecureParameterMappingsAsync(_resource.Id, Arg.Any<CancellationToken>())
            .Returns(_resource);
        _resourceGroupRepository.GetByIdAsync(_resourceGroup.Id, Arg.Any<CancellationToken>())
            .Returns((DomainResourceGroup?)null);
        var command = new SetSecureParameterMappingCommand(_resource.Id, "adminPassword", null, null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Given_AccessDenied_When_Handle_Then_ReturnsErrorAsync()
    {
        // Arrange
        _azureResourceRepository.GetByIdWithSecureParameterMappingsAsync(_resource.Id, Arg.Any<CancellationToken>())
            .Returns(_resource);
        _resourceGroupRepository.GetByIdAsync(_resourceGroup.Id, Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(Error.Unauthorized());
        var command = new SetSecureParameterMappingCommand(_resource.Id, "adminPassword", null, null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Unauthorized);
        _azureResourceRepository.DidNotReceive().Update(Arg.Any<AzureResource>());
    }

    [Fact]
    public async Task Given_ValidMapping_When_Handle_Then_ReturnsUpdatedAsync()
    {
        // Arrange
        var variableGroup = _project.AddPipelineVariableGroup("vg-shared").Value;
        _azureResourceRepository.GetByIdWithSecureParameterMappingsAsync(_resource.Id, Arg.Any<CancellationToken>())
            .Returns(_resource);
        _resourceGroupRepository.GetByIdAsync(_resourceGroup.Id, Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);
        _projectRepository.GetByIdWithPipelineVariableGroupsAsync(_project.Id, Arg.Any<CancellationToken>())
            .Returns(_project);
        var command = new SetSecureParameterMappingCommand(
            _resource.Id, "adminPassword", variableGroup.Id, "admin-pwd");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Updated);
        _azureResourceRepository.Received(1).Update(Arg.Any<AzureResource>());
    }
}
