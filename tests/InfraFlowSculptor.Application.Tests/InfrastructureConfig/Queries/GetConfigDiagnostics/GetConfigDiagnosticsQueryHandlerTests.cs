using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.InfrastructureConfig.Diagnostics;
using InfraFlowSculptor.Application.InfrastructureConfig.Queries.GetConfigDiagnostics;
using InfraFlowSculptor.Application.InfrastructureConfig.ReadModels;
using NSubstitute;

namespace InfraFlowSculptor.Application.Tests.InfrastructureConfig.Queries.GetConfigDiagnostics;

public sealed class GetConfigDiagnosticsQueryHandlerTests
{
    private readonly IInfrastructureConfigReadRepository _configRepository;
    private readonly IConfigDiagnosticService _diagnosticService;
    private readonly GetConfigDiagnosticsQueryHandler _sut;

    public GetConfigDiagnosticsQueryHandlerTests()
    {
        _configRepository = Substitute.For<IInfrastructureConfigReadRepository>();
        _diagnosticService = Substitute.For<IConfigDiagnosticService>();
        _sut = new GetConfigDiagnosticsQueryHandler(_configRepository, _diagnosticService);
    }

    [Fact]
    public async Task Given_ConfigExists_When_Handle_Then_ReturnsDiagnosticsAsync()
    {
        // Arrange
        var configId = Guid.NewGuid();
        var query = new GetConfigDiagnosticsQuery(configId);
        var readModel = CreateMinimalReadModel(configId);
        var diagnostics = new List<ResourceDiagnosticItem>
        {
            new(Guid.NewGuid(), "web-app", "Microsoft.Web/sites", DiagnosticSeverity.Warning, "MISSING_ROLE", "kv-shared")
        };

        _configRepository.GetByIdWithResourcesAsync(configId, Arg.Any<CancellationToken>())
            .Returns(readModel);
        _diagnosticService.EvaluateAsync(readModel, Arg.Any<CancellationToken>())
            .Returns(diagnostics);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Diagnostics.Should().HaveCount(1);
        result.Value.Diagnostics[0].RuleCode.Should().Be("MISSING_ROLE");
    }

    [Fact]
    public async Task Given_ConfigNotFound_When_Handle_Then_ReturnsNotFoundErrorAsync()
    {
        // Arrange
        var query = new GetConfigDiagnosticsQuery(Guid.NewGuid());
        _configRepository.GetByIdWithResourcesAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((InfrastructureConfigReadModel?)null);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Given_ConfigExistsNoDiagnostics_When_Handle_Then_ReturnsEmptyListAsync()
    {
        // Arrange
        var configId = Guid.NewGuid();
        var query = new GetConfigDiagnosticsQuery(configId);
        var readModel = CreateMinimalReadModel(configId);

        _configRepository.GetByIdWithResourcesAsync(configId, Arg.Any<CancellationToken>())
            .Returns(readModel);
        _diagnosticService.EvaluateAsync(readModel, Arg.Any<CancellationToken>())
            .Returns(new List<ResourceDiagnosticItem>());

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Diagnostics.Should().BeEmpty();
    }

    private static InfrastructureConfigReadModel CreateMinimalReadModel(Guid id)
    {
        return new InfrastructureConfigReadModel(
            id,
            "primary",
            Guid.NewGuid(),
            [],
            [],
            new NamingContextReadModel(null, new Dictionary<string, string>(), new Dictionary<string, string>()),
            [],
            [],
            [],
            new Dictionary<string, string>(),
            new Dictionary<string, string>());
    }
}
