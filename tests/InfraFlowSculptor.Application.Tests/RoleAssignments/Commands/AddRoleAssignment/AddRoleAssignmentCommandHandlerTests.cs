using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.RoleAssignments.Commands.AddRoleAssignment;
using InfraFlowSculptor.Application.RoleAssignments.Common;
using InfraFlowSculptor.Domain.Common.AzureRoleDefinitions;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.KeyVaultAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.WebAppAggregate;
using InfraFlowSculptor.Domain.WebAppAggregate.ValueObjects;
using InfraFlowSculptor.GenerationCore;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;

namespace InfraFlowSculptor.Application.Tests.RoleAssignments.Commands.AddRoleAssignment;

public sealed class AddRoleAssignmentCommandHandlerTests
{
    private readonly IAzureResourceRepository _azureResourceRepository;
    private readonly IRoleAssignmentDomainService _roleAssignmentDomainService;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly WebApp _sourceResource;
    private readonly KeyVault _targetResource;
    private readonly WebApp _updatedResourceWithoutAssignments;
    private readonly ManagedIdentityType _managedIdentityType;
    private readonly string _roleDefinitionId;
    private readonly AddRoleAssignmentCommand _command;
    private readonly AddRoleAssignmentCommandHandler _sut;

    public AddRoleAssignmentCommandHandlerTests()
    {
        _azureResourceRepository = Substitute.For<IAzureResourceRepository>();
        _roleAssignmentDomainService = Substitute.For<IRoleAssignmentDomainService>();

        var infrastructureConfig = DomainInfrastructureConfig.Create(new Name("primary"), ProjectId.CreateUnique());
        _resourceGroup = DomainResourceGroup.Create(
            new Name("rg-shared"),
            infrastructureConfig.Id,
            new Location(Location.LocationEnum.FranceCentral));

        _sourceResource = WebApp.Create(
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

        _targetResource = KeyVault.Create(
            _resourceGroup.Id,
            new Name("kv-shared"),
            new Location(Location.LocationEnum.FranceCentral));

        _updatedResourceWithoutAssignments = WebApp.Create(
            _resourceGroup.Id,
            new Name("web-updated"),
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

        _roleDefinitionId = AzureRoleDefinitionCatalog
            .GetForResourceType(AzureResourceTypes.KeyVault)[0]
            .Id;
        _managedIdentityType = new ManagedIdentityType(ManagedIdentityType.IdentityTypeEnum.SystemAssigned);

        _command = new AddRoleAssignmentCommand(
            _sourceResource.Id,
            _targetResource.Id,
            ManagedIdentityType.IdentityTypeEnum.SystemAssigned.ToString(),
            _roleDefinitionId,
            null);

        _sut = new AddRoleAssignmentCommandHandler(
            _azureResourceRepository,
            _roleAssignmentDomainService);
    }

    [Fact]
    public async Task Given_UpdatedResourceWithoutLoadedRoleAssignments_When_Handle_Then_ReturnsCreatedAssignmentFromSourceResourceAsync()
    {
        // Arrange
        _roleAssignmentDomainService.LoadResourceAndAuthorizeAsync(
                _command.SourceResourceId,
                includeRoleAssignments: true,
                Arg.Any<CancellationToken>())
            .Returns(_sourceResource);
        _azureResourceRepository.GetByIdAsync(_command.TargetResourceId, Arg.Any<CancellationToken>())
            .Returns(_targetResource);
        _roleAssignmentDomainService.ValidateIdentityTypeAsync(
                _command.ManagedIdentityType,
                _command.UserAssignedIdentityId,
                Arg.Any<CancellationToken>())
            .Returns(_managedIdentityType);
        _azureResourceRepository.UpdateAsync(_sourceResource, Arg.Any<CancellationToken>())
            .Returns(_updatedResourceWithoutAssignments);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        _sourceResource.RoleAssignments.Should().ContainSingle();

        var createdAssignment = _sourceResource.RoleAssignments.Single();
        result.Value.Id.Should().Be(createdAssignment.Id);
        result.Value.SourceResourceId.Should().Be(_sourceResource.Id);
        result.Value.TargetResourceId.Should().Be(_targetResource.Id);
        result.Value.ManagedIdentityType.Should().Be(_managedIdentityType);
        result.Value.RoleDefinitionId.Should().Be(_roleDefinitionId);
        result.Value.UserAssignedIdentityId.Should().BeNull();
    }
}