using FluentAssertions;
using InfraFlowSculptor.Application.AppSettings.Queries.GetAvailableOutputs;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.WebAppAggregate;
using InfraFlowSculptor.Domain.WebAppAggregate.ValueObjects;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.AppSettings.Queries.GetAvailableOutputs;

public sealed class GetAvailableOutputsQueryHandlerTests
{
    private readonly IAzureResourceRepository _azureResourceRepository;
    private readonly WebApp _sourceResource;
    private readonly GetAvailableOutputsQuery _query;
    private readonly GetAvailableOutputsQueryHandler _sut;

    public GetAvailableOutputsQueryHandlerTests()
    {
        _azureResourceRepository = Substitute.For<IAzureResourceRepository>();

        var config = DomainInfrastructureConfig.Create(new Name("primary"), ProjectId.CreateUnique());
        var resourceGroup = DomainResourceGroup.Create(
            new Name("rg-appsettings"),
            config.Id,
            new Location(Location.LocationEnum.FranceCentral));
        _sourceResource = WebApp.Create(
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
        _query = new GetAvailableOutputsQuery(_sourceResource.Id);
        _sut = new GetAvailableOutputsQueryHandler(_azureResourceRepository);
    }

    [Fact]
    public async Task Given_ExistingResource_When_Handle_Then_UsesDetachedResourceLookupAsync()
    {
        // Arrange
        _azureResourceRepository.GetByIdReadOnlyAsync(_sourceResource.Id, Arg.Any<CancellationToken>())
            .Returns(_sourceResource);

        // Act
        var result = await _sut.Handle(_query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.ResourceTypeName.Should().Be(nameof(WebApp));
        await _azureResourceRepository.Received(1)
            .GetByIdReadOnlyAsync(_sourceResource.Id, Arg.Any<CancellationToken>());
        await _azureResourceRepository.DidNotReceive()
            .GetByIdAsync(Arg.Any<AzureResourceId>(), Arg.Any<CancellationToken>());
    }
}