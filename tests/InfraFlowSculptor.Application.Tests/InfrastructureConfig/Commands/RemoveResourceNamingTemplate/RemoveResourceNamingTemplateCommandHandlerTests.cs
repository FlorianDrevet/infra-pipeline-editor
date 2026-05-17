using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.RemoveResourceNamingTemplate;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using NSubstitute;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Tests.InfrastructureConfig.Commands.RemoveResourceNamingTemplate;

public sealed class RemoveResourceNamingTemplateCommandHandlerTests
{
    private readonly IInfraConfigAccessService _accessService;
    private readonly IInfrastructureConfigRepository _repository;
    private readonly DomainInfrastructureConfig _config;
    private readonly RemoveResourceNamingTemplateCommandHandler _sut;

    public RemoveResourceNamingTemplateCommandHandlerTests()
    {
        _accessService = Substitute.For<IInfraConfigAccessService>();
        _repository = Substitute.For<IInfrastructureConfigRepository>();
        _config = DomainInfrastructureConfig.Create(new Name("test-config"), ProjectId.CreateUnique());
        _sut = new RemoveResourceNamingTemplateCommandHandler(_repository, _accessService);
    }

    [Fact]
    public async Task Given_AccessDenied_When_Handle_Then_ReturnsUnauthorizedAsync()
    {
        // Arrange
        var command = new RemoveResourceNamingTemplateCommand(_config.Id, "Microsoft.KeyVault/vaults");
        _accessService.VerifyWriteAccessAsync(Arg.Any<InfrastructureConfigId>(), Arg.Any<CancellationToken>())
            .Returns(Error.Unauthorized());

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Unauthorized);
    }

    [Fact]
    public async Task Given_ConfigNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange
        var command = new RemoveResourceNamingTemplateCommand(_config.Id, "Microsoft.KeyVault/vaults");
        _accessService.VerifyWriteAccessAsync(Arg.Any<InfrastructureConfigId>(), Arg.Any<CancellationToken>())
            .Returns(_config);
        _repository.GetByIdWithNamingTemplatesAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns((DomainInfrastructureConfig?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Given_TemplateNotFound_When_Handle_Then_ReturnsNotFoundAsync()
    {
        // Arrange — config has no naming template for this resource type
        var command = new RemoveResourceNamingTemplateCommand(_config.Id, "Microsoft.KeyVault/vaults");
        _accessService.VerifyWriteAccessAsync(Arg.Any<InfrastructureConfigId>(), Arg.Any<CancellationToken>())
            .Returns(_config);
        _repository.GetByIdWithNamingTemplatesAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Given_ValidRequest_When_Handle_Then_ReturnsDeletedAsync()
    {
        // Arrange — add a naming template, then remove it
        const string resourceType = "Microsoft.KeyVault/vaults";
        _config.SetResourceNamingTemplate(resourceType, new NamingTemplate("{name}-{env}-kv"));
        var command = new RemoveResourceNamingTemplateCommand(_config.Id, resourceType);

        _accessService.VerifyWriteAccessAsync(Arg.Any<InfrastructureConfigId>(), Arg.Any<CancellationToken>())
            .Returns(_config);
        _repository.GetByIdWithNamingTemplatesAsync(_config.Id, Arg.Any<CancellationToken>())
            .Returns(_config);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Deleted);
        _repository.Received(1).Update(Arg.Any<DomainInfrastructureConfig>());
    }
}
