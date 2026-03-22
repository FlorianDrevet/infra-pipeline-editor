using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.CreateInfraConfig;

public sealed class CreateInfrastructureConfigCommandHandler(
    IInfrastructureConfigRepository repository,
    IProjectAccessService projectAccessService,
    IMapper mapper)
    : IRequestHandler<CreateInfrastructureConfigCommand,
        ErrorOr<GetInfrastructureConfigResult>>
{
    public async Task<ErrorOr<GetInfrastructureConfigResult>> Handle(
        CreateInfrastructureConfigCommand command, CancellationToken cancellationToken)
    {
        var projectId = ProjectId.Create(command.ProjectId);
        var accessResult = await projectAccessService.VerifyWriteAccessAsync(projectId, cancellationToken);
        if (accessResult.IsError)
            return accessResult.Errors;

        var nameVo = new Name(command.Name);
        var infra = Domain.InfrastructureConfigAggregate.InfrastructureConfig.Create(nameVo, projectId);

        var saved = await repository.AddAsync(infra);

        return mapper.Map<GetInfrastructureConfigResult>(saved);
    }
}