using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.CustomDomains.Queries.ListCustomDomains;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.WebAppAggregate;
using InfraFlowSculptor.Domain.WebAppAggregate.ValueObjects;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.CustomDomains.Queries.ListCustomDomains;

public sealed class ListCustomDomainsQueryHandlerTests
{
    private readonly IAzureResourceRepository _azureResourceRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly DomainInfrastructureConfig _config;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly WebApp _sourceResource;
    private readonly ListCustomDomainsQuery _query;
    private readonly ListCustomDomainsQueryHandler _sut;

    public ListCustomDomainsQueryHandlerTests()
    {
        _azureResourceRepository = Substitute.For<IAzureResourceRepository>();
        _resourceGroupRepository = Substitute.For<IResourceGroupRepository>();
        _accessService = Substitute.For<IInfraConfigAccessService>();

        _config = DomainInfrastructureConfig.Create(new Name("primary"), ProjectId.CreateUnique());
        _resourceGroup = DomainResourceGroup.Create(
            new Name("rg-domains"),
            _config.Id,
            new Location(Location.LocationEnum.FranceCentral));
        _sourceResource = WebApp.Create(
            _resourceGroup.Id,
            new Name("web-domain"),
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
        _ = _sourceResource.AddCustomDomain("dev", "api.example.com");

        _query = new ListCustomDomainsQuery(_sourceResource.Id);
        _sut = new ListCustomDomainsQueryHandler(
            _azureResourceRepository,
            _resourceGroupRepository,
            _accessService);
    }

    [Fact]
    public async Task Given_CustomDomains_When_Handle_Then_UsesDetachedResourceGroupLookupAsync()
    {
        // Arrange
        _azureResourceRepository.GetByIdWithCustomDomainsAsync(_sourceResource.Id, Arg.Any<CancellationToken>())
            .Returns(_sourceResource);
        _resourceGroupRepository.GetByIdReadOnlyAsync(_resourceGroup.Id, Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyReadAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);

        // Act
        var result = await _sut.Handle(_query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().ContainSingle().Which.DomainName.Should().Be("api.example.com");
        await _resourceGroupRepository.Received(1)
            .GetByIdReadOnlyAsync(_resourceGroup.Id, Arg.Any<CancellationToken>());
        await _resourceGroupRepository.DidNotReceive()
            .GetByIdAsync(Arg.Any<InfraFlowSculptor.Domain.Common.Models.ValueObject>(), Arg.Any<CancellationToken>());
    }
}