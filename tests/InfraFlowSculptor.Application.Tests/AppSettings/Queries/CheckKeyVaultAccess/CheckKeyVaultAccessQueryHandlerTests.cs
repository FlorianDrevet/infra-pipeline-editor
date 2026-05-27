using FluentAssertions;
using InfraFlowSculptor.Application.AppSettings.Queries.CheckKeyVaultAccess;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.AzureRoleDefinitions;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.KeyVaultAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.WebAppAggregate;
using InfraFlowSculptor.Domain.WebAppAggregate.ValueObjects;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.AppSettings.Queries.CheckKeyVaultAccess;

public sealed class CheckKeyVaultAccessQueryHandlerTests
{
    private readonly IAzureResourceRepository _azureResourceRepository;
    private readonly WebApp _sourceResource;
    private readonly KeyVault _keyVault;
    private readonly CheckKeyVaultAccessQuery _query;
    private readonly CheckKeyVaultAccessQueryHandler _sut;

    public CheckKeyVaultAccessQueryHandlerTests()
    {
        _azureResourceRepository = Substitute.For<IAzureResourceRepository>();

        var config = DomainInfrastructureConfig.Create(new Name("primary"), ProjectId.CreateUnique());
        var resourceGroup = DomainResourceGroup.Create(
            new Name("rg-rbac"),
            config.Id,
            new Location(Location.LocationEnum.FranceCentral));
        _sourceResource = WebApp.Create(
            resourceGroup.Id,
            new Name("web-rbac"),
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
        _keyVault = KeyVault.Create(
            resourceGroup.Id,
            new Name("kv-rbac"),
            new Location(Location.LocationEnum.FranceCentral));
        _sourceResource.AddRoleAssignment(
            _keyVault.Id,
            new ManagedIdentityType(ManagedIdentityType.IdentityTypeEnum.SystemAssigned),
            AzureRoleDefinitionCatalog.KeyVaultSecretsUser);

        _query = new CheckKeyVaultAccessQuery(_sourceResource.Id, _keyVault.Id);
        _sut = new CheckKeyVaultAccessQueryHandler(_azureResourceRepository);
    }

    [Fact]
    public async Task Given_KeyVaultRoleAssignment_When_Handle_Then_UsesDetachedRoleAssignmentLookupAsync()
    {
        // Arrange
        _azureResourceRepository.GetByIdWithRoleAssignmentsReadOnlyAsync(_sourceResource.Id, Arg.Any<CancellationToken>())
            .Returns(_sourceResource);

        // Act
        var result = await _sut.Handle(_query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.HasAccess.Should().BeTrue();
        await _azureResourceRepository.Received(1)
            .GetByIdWithRoleAssignmentsReadOnlyAsync(_sourceResource.Id, Arg.Any<CancellationToken>());
        await _azureResourceRepository.DidNotReceive()
            .GetByIdWithRoleAssignmentsAsync(Arg.Any<AzureResourceId>(), Arg.Any<CancellationToken>());
    }
}
