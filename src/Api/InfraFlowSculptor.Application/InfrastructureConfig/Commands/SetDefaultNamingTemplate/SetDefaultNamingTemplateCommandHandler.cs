using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.SetDefaultNamingTemplate;

public class SetDefaultNamingTemplateCommandHandler(
    IInfrastructureConfigRepository repository,
    ICurrentUser currentUser)
    : IRequestHandler<SetDefaultNamingTemplateCommand, ErrorOr<Updated>>
{
    public async Task<ErrorOr<Updated>> Handle(
        SetDefaultNamingTemplateCommand command, CancellationToken cancellationToken)
    {
        var authResult = await InfraConfigAccessHelper.VerifyWriteAccessAsync(
            repository, currentUser, command.InfraConfigId, cancellationToken);

        if (authResult.IsError)
            return authResult.Errors;

        var infraConfig = await repository.GetByIdAsync(command.InfraConfigId, cancellationToken);
        if (infraConfig is null)
            return Errors.InfrastructureConfig.NotFoundError(command.InfraConfigId);

        var template = command.Template is not null
            ? new NamingTemplate(command.Template)
            : null;

        infraConfig.SetDefaultNamingTemplate(template);

        await repository.UpdateAsync(infraConfig);

        return Result.Updated;
    }
}
