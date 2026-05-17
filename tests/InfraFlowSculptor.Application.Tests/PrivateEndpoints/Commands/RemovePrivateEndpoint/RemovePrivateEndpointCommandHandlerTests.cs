using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.PrivateEndpoints.Commands.RemovePrivateEndpoint;
using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.WebAppAggregate;
using InfraFlowSculptor.Domain.WebAppAggregate.ValueObjects;
using InfraFlowSculptor.GenerationCore;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.PrivateEndpoints.Commands.RemovePrivateEndpoint;

public sealed class RemovePrivateEndpointCommandHandlerTests
{
    private readonly IAzureResourceRepository _resourceRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly DomainInfrastructureConfig _config;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly WebApp _webApp;
    private readonly RemovePrivateEndpointCommand _command;
    private readonly RemovePrivateEndpointCommandHandler _sut;

    public RemovePrivateEndpointCommandHandlerTests()
    {
        _resourceRepository = Substitute.For<IAzureResourceRepository>();
        _accessService = Substitute.For<IInfraConfigAccessService>();
        _config = DomainInfrastructureConfig.Create(new Name("primary"), ProjectId.CreateUnique());
        _resourceGroup = DomainResourceGroup.Create(
            new Name("rg-shared"),
            _config.Id,
            new Location(Location.LocationEnum.FranceCentral));
        _webApp = WebApp.Create(
            _resourceGroup.Id,
            new Name("web-api"),
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

        typeof(AzureResource).GetProperty(nameof(AzureResource.ResourceType))!
            .SetValue(_webApp, new ResourceTypeName(AzureResourceTypes.WebApp));

        // Set ResourceGroup navigation property via reflection (handler uses resource.ResourceGroup!.InfraConfigId)
        typeof(AzureResource).GetProperty(nameof(AzureResource.ResourceGroup))!
            .SetValue(_webApp, _resourceGroup);

        _command = new RemovePrivateEndpointCommand(
            _webApp.Id,
            PrivateEndpointConfigId.CreateUnique());
        _sut = new RemovePrivateEndpointCommandHandler(_resourceRepository, _accessService);
    }

    [Fact]
    public async Task Given_ResourceNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _resourceRepository.GetByIdWithPrivateEndpointsAsync(Arg.Any<AzureResourceId>(), Arg.Any<CancellationToken>())
            .Returns((AzureResource?)null);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Given_AccessDenied_When_Handle_Then_ReturnsUnauthorizedAsync()
    {
        // Arrange
        _resourceRepository.GetByIdWithPrivateEndpointsAsync(Arg.Any<AzureResourceId>(), Arg.Any<CancellationToken>())
            .Returns(_webApp);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(Error.Unauthorized());

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Unauthorized);
    }

    [Fact]
    public async Task Given_ValidRequest_When_Handle_Then_ReturnsDeletedAsync()
    {
        // Arrange — add a private endpoint so the remove succeeds
        var subnetId = AzureResourceId.CreateUnique();
        var peConfig = _webApp.AddPrivateEndpoint(subnetId, "sites", autoApproval: false, privateDnsZoneId: null, customNetworkInterfaceName: null);
        var command = new RemovePrivateEndpointCommand(_webApp.Id, peConfig.Id);

        _resourceRepository.GetByIdWithPrivateEndpointsAsync(Arg.Any<AzureResourceId>(), Arg.Any<CancellationToken>())
            .Returns(_webApp);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Deleted);
    }
}
