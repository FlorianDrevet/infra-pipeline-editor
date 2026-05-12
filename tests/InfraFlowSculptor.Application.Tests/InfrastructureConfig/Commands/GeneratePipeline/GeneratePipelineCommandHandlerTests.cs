using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.GeneratePipeline;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using NSubstitute;

namespace InfraFlowSculptor.Application.Tests.InfrastructureConfig.Commands.GeneratePipeline;

public sealed class GeneratePipelineCommandHandlerTests
{
    private readonly IInfrastructureConfigReadRepository _configReadRepository;
    private readonly IInfraConfigAccessService _accessService;
    private readonly GeneratePipelineCommandHandler _sut;

    public GeneratePipelineCommandHandlerTests()
    {
        _configReadRepository = Substitute.For<IInfrastructureConfigReadRepository>();
        _accessService = Substitute.For<IInfraConfigAccessService>();
        _sut = new GeneratePipelineCommandHandler(
            _configReadRepository,
            projectRepository: null!,
            pipelineGenerationEngine: null!,
            appPipelineGenerationEngine: null!,
            appPipelineRequestFactory: null!,
            bicepGenerators: [],
            artifactService: null!,
            targetResolver: null!,
            infraConfigRepository: null!,
            accessService: _accessService);
    }

    [Fact]
    public async Task Given_WriteAccessDenied_When_Handle_Then_ReturnsErrorAndDoesNotLoadConfigAsync()
    {
        // Arrange
        var command = new GeneratePipelineCommand(Guid.NewGuid());
        _accessService.VerifyWriteAccessAsync(Arg.Any<InfrastructureConfigId>(), Arg.Any<CancellationToken>())
            .Returns(Errors.InfrastructureConfig.ForbiddenError());

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Forbidden);
        await _configReadRepository.DidNotReceive()
            .GetByIdWithResourcesAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
}