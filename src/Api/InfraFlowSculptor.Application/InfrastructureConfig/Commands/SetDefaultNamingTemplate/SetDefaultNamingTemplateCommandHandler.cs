using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.SetDefaultNamingTemplate;

public class SetDefaultNamingTemplateCommandHandler(
    IInfrastructureConfigRepository repository,
    IInfraConfigAccessService accessService)
    : IRequestHandler<SetDefaultNamingTemplateCommand, ErrorOr<Updated>>
{
    public async Task<ErrorOr<Updated>> Handle(
        SetDefaultNamingTemplateCommand command, CancellationToken cancellationToken)
    {
        var authResult = await accessService.VerifyWriteAccessAsync(command.InfraConfigId, cancellationToken);

        if (authResult.IsError)
            return authResult.Errors;

        var infraConfig = authResult.Value;

        var template = command.Template is not null
            ? new NamingTemplate(command.Template)
            : null;

        infraConfig.SetDefaultNamingTemplate(template);

        await repository.UpdateAsync(infraConfig);

        return Result.Updated;
    }
}
