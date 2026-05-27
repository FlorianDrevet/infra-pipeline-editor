using FluentAssertions;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ContainerAppAggregate;
using InfraFlowSculptor.Domain.ContainerAppAggregate.Models;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate;
using InfraFlowSculptor.Infrastructure.Persistence;
using InfraFlowSculptor.Infrastructure.Persistence.ReadRepositories;
using InfraFlowSculptor.Infrastructure.Tests.Persistence.Repositories;
using Xunit;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Infrastructure.Tests.Persistence.ReadRepositories;

public sealed class ContainerAppReadRepositoryTests : IDisposable
{
    private readonly ProjectDbContext _context;
    private readonly ContainerAppReadRepository _sut;

    public ContainerAppReadRepositoryTests()
    {
        _context = InMemoryDbContextFactory.Create();
        _sut = new ContainerAppReadRepository(_context);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task Given_StoredContainerApp_When_GetByIdAsync_Then_ReturnsProjectedDetailAsync()
    {
        // Arrange
        var projectId = ProjectId.CreateUnique();
        var config = InfrastructureConfig.Create(new Name("dev"), projectId);
        var resourceGroup = ResourceGroup.Create(
            new Name("rg-app"),
            config.Id,
            new Location(Location.LocationEnum.WestEurope));
        var containerRegistryId = AzureResourceId.CreateUnique();
        var acrPullIdentityId = AzureResourceId.CreateUnique();
        var settings = new List<ContainerAppEnvironmentSettingsData>
        {
            new(
                "dev",
                CpuCores: "0.5",
                MemoryGi: "1Gi",
                MinReplicas: 1,
                MaxReplicas: 3,
                IngressEnabled: true,
                IngressTargetPort: 8080,
                IngressExternal: true,
                TransportMethod: "auto",
                ReadinessProbePath: "/ready",
                ReadinessProbePort: 8080,
                LivenessProbePath: "/live",
                LivenessProbePort: 8080,
                StartupProbePath: "/startup",
                StartupProbePort: 8080,
                ContainerRegistryServiceConnection: "acr-service-connection")
        };
        var acrAuthMode = new AcrAuthMode(AcrAuthMode.AcrAuthModeType.ManagedIdentity);
        var containerApp = ContainerApp.Create(
            resourceGroup.Id,
            new Name("ca-api"),
            new Location(Location.LocationEnum.WestEurope),
            AzureResourceId.CreateUnique(),
            containerRegistryId,
            acrAuthMode,
            acrPullIdentityId,
            dockerImageName: "registry.azurecr.io/api",
            dockerImageValidated: true,
            dockerfilePath: "src/Api/Dockerfile",
            applicationName: "api",
            sourceCodePath: "src/Api",
            environmentSettings: settings);
        _context.InfrastructureConfigs.Add(config);
        _context.ResourceGroups.Add(resourceGroup);
        _context.ContainerApps.Add(containerApp);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetByIdAsync(containerApp.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.InfraConfigId.Should().Be(config.Id);
        result.Result.Id.Should().Be(containerApp.Id);
        result.Result.ResourceGroupId.Should().Be(resourceGroup.Id);
        result.Result.Name.Value.Should().Be("ca-api");
        result.Result.ContainerRegistryId.Should().Be(containerRegistryId.Value);
        result.Result.AcrAuthMode.Should().Be(acrAuthMode.Value.ToString());
        result.Result.AcrPullIdentityId.Should().Be(acrPullIdentityId.Value);
        result.Result.DockerImageName.Should().Be("registry.azurecr.io/api");
        result.Result.DockerImageValidated.Should().BeTrue();
        result.Result.DockerfilePath.Should().Be("src/Api/Dockerfile");
        result.Result.ApplicationName.Should().Be("api");
        result.Result.SourceCodePath.Should().Be("src/Api");
        result.Result.EnvironmentSettings.Should().ContainSingle()
            .Which.ContainerRegistryServiceConnection.Should().Be("acr-service-connection");
    }

    [Fact]
    public async Task Given_UnknownContainerApp_When_GetByIdAsync_Then_ReturnsNullAsync()
    {
        // Act
        var result = await _sut.GetByIdAsync(AzureResourceId.CreateUnique(), CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
}