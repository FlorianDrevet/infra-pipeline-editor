using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.CustomDomains.Commands.RemoveCustomDomain;
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

namespace InfraFlowSculptor.Application.Tests.CustomDomains.Commands.RemoveCustomDomain;

public sealed class RemoveCustomDomainCommandHandlerTests
{
    private readonly IAzureResourceRepository _azureResourceRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly DomainInfrastructureConfig _config;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly WebApp _webApp;
    private readonly RemoveCustomDomainCommand _command;
    private readonly RemoveCustomDomainCommandHandler _sut;

    public RemoveCustomDomainCommandHandlerTests()
    {
        _azureResourceRepository = Substitute.For<IAzureResourceRepository>();
        _resourceGroupRepository = Substitute.For<IResourceGroupRepository>();
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

        _command = new RemoveCustomDomainCommand(
            _webApp.Id,
            CustomDomainId.CreateUnique());
        _sut = new RemoveCustomDomainCommandHandler(
            _azureResourceRepository, _resourceGroupRepository, _accessService);
    }

    [Fact]
    public async Task Given_ResourceNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        _azureResourceRepository.GetByIdWithCustomDomainsAsync(Arg.Any<AzureResourceId>(), Arg.Any<CancellationToken>())
            .Returns((AzureResource?)null);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Given_CustomDomainNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange — no custom domain with this ID on resource
        _azureResourceRepository.GetByIdWithCustomDomainsAsync(Arg.Any<AzureResourceId>(), Arg.Any<CancellationToken>())
            .Returns(_webApp);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Given_ResourceGroupNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange — add a custom domain so that check passes
        _webApp.AddCustomDomain("Production", "api.example.com");
        var domainId = _webApp.CustomDomains.First().Id;
        var command = new RemoveCustomDomainCommand(_webApp.Id, domainId);

        _azureResourceRepository.GetByIdWithCustomDomainsAsync(Arg.Any<AzureResourceId>(), Arg.Any<CancellationToken>())
            .Returns(_webApp);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns((DomainResourceGroup?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Given_AccessDenied_When_Handle_Then_ReturnsUnauthorizedAsync()
    {
        // Arrange
        _webApp.AddCustomDomain("Production", "api.example.com");
        var domainId = _webApp.CustomDomains.First().Id;
        var command = new RemoveCustomDomainCommand(_webApp.Id, domainId);

        _azureResourceRepository.GetByIdWithCustomDomainsAsync(Arg.Any<AzureResourceId>(), Arg.Any<CancellationToken>())
            .Returns(_webApp);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(Error.Unauthorized());

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Unauthorized);
    }

    [Fact]
    public async Task Given_ValidRequest_When_Handle_Then_ReturnsDeletedAsync()
    {
        // Arrange
        _webApp.AddCustomDomain("Production", "api.example.com");
        var domainId = _webApp.CustomDomains.First().Id;
        var command = new RemoveCustomDomainCommand(_webApp.Id, domainId);

        _azureResourceRepository.GetByIdWithCustomDomainsAsync(Arg.Any<AzureResourceId>(), Arg.Any<CancellationToken>())
            .Returns(_webApp);
        _resourceGroupRepository.GetByIdAsync(Arg.Any<ValueObject>(), Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyWriteAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Deleted);
        _azureResourceRepository.Received(1).Update(Arg.Any<AzureResource>());
    }
}
