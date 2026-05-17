using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.CustomDomains.Queries.GetDnsInstructions;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ContainerAppAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.GenerationCore;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.CustomDomains.Queries.GetDnsInstructions;

public sealed class GetDnsInstructionsQueryHandlerTests
{
    private readonly IAzureResourceRepository _azureResourceRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly DomainInfrastructureConfig _config;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly ContainerApp _containerApp;
    private readonly GetDnsInstructionsQuery _query;
    private readonly GetDnsInstructionsQueryHandler _sut;

    public GetDnsInstructionsQueryHandlerTests()
    {
        _azureResourceRepository = Substitute.For<IAzureResourceRepository>();
        _resourceGroupRepository = Substitute.For<IResourceGroupRepository>();
        _accessService = Substitute.For<IInfraConfigAccessService>();

        _config = DomainInfrastructureConfig.Create(new Name("primary"), ProjectId.CreateUnique());
        _resourceGroup = DomainResourceGroup.Create(
            new Name("rg-ifs-dev"),
            _config.Id,
            new Location(Location.LocationEnum.FranceCentral));

        _containerApp = ContainerApp.Create(
            _resourceGroup.Id,
            new Name("ifs-frontend"),
            new Location(Location.LocationEnum.FranceCentral),
            AzureResourceId.CreateUnique(),
            null,
            null);
        typeof(InfraFlowSculptor.Domain.Common.BaseModels.AzureResource)
            .GetProperty(nameof(InfraFlowSculptor.Domain.Common.BaseModels.AzureResource.ResourceType))!
            .SetValue(_containerApp, new ResourceTypeName(AzureResourceTypes.ContainerApp));

        var customDomain = _containerApp.AddCustomDomain("dev", "infraflowsculptor.fr").Value;
        _query = new GetDnsInstructionsQuery(_containerApp.Id, customDomain.Id);

        _sut = new GetDnsInstructionsQueryHandler(
            _azureResourceRepository,
            _resourceGroupRepository,
            _accessService);
    }

    [Fact]
    public async Task Given_ContainerAppCustomDomain_When_Handle_Then_ReturnsPortalGuidedInstructionsAsync()
    {
        // Arrange
        _azureResourceRepository.GetByIdWithCustomDomainsAsync(_containerApp.Id, Arg.Any<CancellationToken>())
            .Returns(_containerApp);
        _resourceGroupRepository.GetByIdReadOnlyAsync(_resourceGroup.Id, Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyReadAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);

        // Act
        var result = await _sut.Handle(_query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Steps.Should().HaveCount(3);

        var cnameStep = result.Value.Steps[0];
        cnameStep.Description.Should().Contain("Container App");
        cnameStep.Description.Should().Contain("Networking > Custom domains");
        cnameStep.Description.Should().Contain("IP address");
        cnameStep.Description.Should().Contain("Application URL");
        cnameStep.RecordValue.Should().Be("<application-url-from-container-app-overview>");

        var txtStep = result.Value.Steps[1];
        txtStep.Description.Should().Contain("Custom Domain Verification ID");
        txtStep.RecordName.Should().Be("asuid.infraflowsculptor.fr");
        txtStep.RecordValue.Should().Be("<custom-domain-verification-id-from-container-app>");

        var validateStep = result.Value.Steps[2];
        validateStep.Description.Should().Contain("Validate DNS");
        validateStep.Description.Should().Contain("managed certificate");
    }
}
