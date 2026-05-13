using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.SecureParameterMappings.Queries.GetSecureParameterMappings;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using InfraFlowSculptor.Domain.WebAppAggregate;
using InfraFlowSculptor.Domain.WebAppAggregate.ValueObjects;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using DomainResourceGroup = InfraFlowSculptor.Domain.ResourceGroupAggregate.ResourceGroup;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.SecureParameterMappings.Queries.GetSecureParameterMappings;

public sealed class GetSecureParameterMappingsQueryHandlerTests
{
    private readonly IAzureResourceRepository _azureResourceRepository;
    private readonly IResourceGroupRepository _resourceGroupRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly IProjectRepository _projectRepository;
    private readonly DomainInfrastructureConfig _config;
    private readonly DomainResourceGroup _resourceGroup;
    private readonly Project _project;
    private readonly WebApp _sourceResource;
    private readonly GetSecureParameterMappingsQuery _query;
    private readonly GetSecureParameterMappingsQueryHandler _sut;

    public GetSecureParameterMappingsQueryHandlerTests()
    {
        _azureResourceRepository = Substitute.For<IAzureResourceRepository>();
        _resourceGroupRepository = Substitute.For<IResourceGroupRepository>();
        _accessService = Substitute.For<IInfraConfigAccessService>();
        _projectRepository = Substitute.For<IProjectRepository>();

        _project = Project.Create(new Name("Retail Platform"), "Provision retail assets.", UserId.CreateUnique());
        _config = DomainInfrastructureConfig.Create(new Name("primary"), _project.Id);
        _resourceGroup = DomainResourceGroup.Create(
            new Name("rg-parameters"),
            _config.Id,
            new Location(Location.LocationEnum.FranceCentral));
        _sourceResource = WebApp.Create(
            _resourceGroup.Id,
            new Name("web-params"),
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

        var variableGroup = _project.AddPipelineVariableGroup("vg-shared").Value;
        _ = _sourceResource.SetSecureParameterMapping("DbPassword", variableGroup.Id, "db-password");

        _query = new GetSecureParameterMappingsQuery(_sourceResource.Id);
        _sut = new GetSecureParameterMappingsQueryHandler(
            _azureResourceRepository,
            _resourceGroupRepository,
            _accessService,
            _projectRepository);
    }

    [Fact]
    public async Task Given_VariableGroupMappings_When_Handle_Then_UsesDetachedResourceGroupLookupAsync()
    {
        // Arrange
        _azureResourceRepository.GetByIdWithSecureParameterMappingsAsync(_sourceResource.Id, Arg.Any<CancellationToken>())
            .Returns(_sourceResource);
        _resourceGroupRepository.GetByIdReadOnlyAsync(_resourceGroup.Id, Arg.Any<CancellationToken>())
            .Returns(_resourceGroup);
        _accessService.VerifyReadAccessAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);
        _projectRepository.GetByIdWithPipelineVariableGroupsAsync(_project.Id, Arg.Any<CancellationToken>())
            .Returns(_project);

        // Act
        var result = await _sut.Handle(_query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().ContainSingle().Which.VariableGroupName.Should().Be("vg-shared");
        await _resourceGroupRepository.Received(1)
            .GetByIdReadOnlyAsync(_resourceGroup.Id, Arg.Any<CancellationToken>());
        await _resourceGroupRepository.DidNotReceive()
            .GetByIdAsync(Arg.Any<InfraFlowSculptor.Domain.Common.Models.ValueObject>(), Arg.Any<CancellationToken>());
    }
}