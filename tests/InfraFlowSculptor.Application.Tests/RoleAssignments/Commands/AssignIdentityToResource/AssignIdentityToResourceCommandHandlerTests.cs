using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.RoleAssignments.Commands.AssignIdentityToResource;
using InfraFlowSculptor.Application.RoleAssignments.Common;
using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.WebAppAggregate;
using InfraFlowSculptor.Domain.WebAppAggregate.ValueObjects;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.RoleAssignments.Commands.AssignIdentityToResource;

public sealed class AssignIdentityToResourceCommandHandlerTests
{
    private readonly IAzureResourceRepository _azureResourceRepository;
    private readonly IRoleAssignmentDomainService _roleAssignmentDomainService;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly WebApp _resource;
    private readonly AzureResourceId _identityId;
    private readonly AssignIdentityToResourceCommand _command;
    private readonly AssignIdentityToResourceCommandHandler _sut;

    public AssignIdentityToResourceCommandHandlerTests()
    {
        _azureResourceRepository = Substitute.For<IAzureResourceRepository>();
        _roleAssignmentDomainService = Substitute.For<IRoleAssignmentDomainService>();
        var config = DomainInfrastructureConfig.Create(new Name("primary"), ProjectId.CreateUnique());
        _resourceGroup = DomainResourceGroup.Create(
            new Name("rg-shared"),
            config.Id,
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
        _identityId = AzureResourceId.CreateUnique();
        _command = new AssignIdentityToResourceCommand(_resource.Id, _identityId);
        _sut = new AssignIdentityToResourceCommandHandler(
            _azureResourceRepository, _roleAssignmentDomainService);
    }

    [Fact]
    public async Task Given_ResourceLoadFails_When_Handle_Then_ReturnsErrorAsync()
    {
        // Arrange
        _roleAssignmentDomainService.LoadResourceAndAuthorizeAsync(
                _command.ResourceId,
                includeRoleAssignments: true,
                Arg.Any<CancellationToken>())
            .Returns(Error.NotFound());

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Given_IdentityResourceNotFound_When_Handle_Then_ReturnsErrorAsync()
    {
        // Arrange
        _roleAssignmentDomainService.LoadResourceAndAuthorizeAsync(
                _command.ResourceId,
                includeRoleAssignments: true,
                Arg.Any<CancellationToken>())
            .Returns(_resource);
        _roleAssignmentDomainService.ValidateIdentityResourceExistsAsync(
                _command.UserAssignedIdentityId,
                Arg.Any<CancellationToken>())
            .Returns(Error.NotFound());

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Given_ValidRequest_When_Handle_Then_AssignsIdentityAndUpdatesResourceAsync()
    {
        // Arrange
        _roleAssignmentDomainService.LoadResourceAndAuthorizeAsync(
                _command.ResourceId,
                includeRoleAssignments: true,
                Arg.Any<CancellationToken>())
            .Returns(_resource);
        _roleAssignmentDomainService.ValidateIdentityResourceExistsAsync(
                _command.UserAssignedIdentityId,
                Arg.Any<CancellationToken>())
            .Returns(new Success());

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        _azureResourceRepository.Received(1).Update(Arg.Any<AzureResource>());
    }
}
