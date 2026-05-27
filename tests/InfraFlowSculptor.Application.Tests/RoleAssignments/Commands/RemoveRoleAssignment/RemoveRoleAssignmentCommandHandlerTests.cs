using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.RoleAssignments.Commands.RemoveRoleAssignment;
using InfraFlowSculptor.Application.RoleAssignments.Common;
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

namespace InfraFlowSculptor.Application.Tests.RoleAssignments.Commands.RemoveRoleAssignment;

public sealed class RemoveRoleAssignmentCommandHandlerTests
{
    private readonly IAzureResourceRepository _azureResourceRepository;
    private readonly IRoleAssignmentDomainService _roleAssignmentDomainService;
    private readonly DomainInfrastructureConfig _config;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly WebApp _webApp;
    private readonly RemoveRoleAssignmentCommand _command;
    private readonly RemoveRoleAssignmentCommandHandler _sut;

    public RemoveRoleAssignmentCommandHandlerTests()
    {
        _azureResourceRepository = Substitute.For<IAzureResourceRepository>();
        _roleAssignmentDomainService = Substitute.For<IRoleAssignmentDomainService>();
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

        _command = new RemoveRoleAssignmentCommand(
            _webApp.Id,
            RoleAssignmentId.CreateUnique());
        _sut = new RemoveRoleAssignmentCommandHandler(
            _azureResourceRepository, _roleAssignmentDomainService);
    }

    [Fact]
    public async Task Given_LoadResourceFails_When_Handle_Then_ReturnsErrorAsync()
    {
        // Arrange
        _roleAssignmentDomainService.LoadResourceAndAuthorizeAsync(
                Arg.Any<AzureResourceId>(), true, Arg.Any<CancellationToken>())
            .Returns(Error.NotFound());

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Given_RoleAssignmentNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange — resource has no role assignments
        _roleAssignmentDomainService.LoadResourceAndAuthorizeAsync(
                Arg.Any<AzureResourceId>(), true, Arg.Any<CancellationToken>())
            .Returns(_webApp);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Given_ValidRequest_When_Handle_Then_ReturnsDeletedAsync()
    {
        // Arrange — add a role assignment first
        var targetResourceId = AzureResourceId.CreateUnique();
        _webApp.AddRoleAssignment(
            targetResourceId,
            new Domain.Common.BaseModels.ValueObjects.ManagedIdentityType(
                Domain.Common.BaseModels.ValueObjects.ManagedIdentityType.IdentityTypeEnum.SystemAssigned),
            "b24988ac-6180-42a0-ab88-20f7382dd24c",
            userAssignedIdentityId: null);
        var assignmentId = _webApp.RoleAssignments.First().Id;
        var command = new RemoveRoleAssignmentCommand(_webApp.Id, assignmentId);

        _roleAssignmentDomainService.LoadResourceAndAuthorizeAsync(
                Arg.Any<AzureResourceId>(), true, Arg.Any<CancellationToken>())
            .Returns(_webApp);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Deleted);
        _azureResourceRepository.Received(1).Update(Arg.Any<AzureResource>());
    }
}
