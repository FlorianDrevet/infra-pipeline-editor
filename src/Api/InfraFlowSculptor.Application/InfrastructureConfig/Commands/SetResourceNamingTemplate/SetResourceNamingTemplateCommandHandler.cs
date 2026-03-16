using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.SetResourceNamingTemplate;

public class SetResourceNamingTemplateCommandHandler(
    IInfrastructureConfigRepository repository,
    ICurrentUser currentUser,
    IMapper mapper)
    : IRequestHandler<SetResourceNamingTemplateCommand, ErrorOr<ResourceNamingTemplateResult>>
{
    public async Task<ErrorOr<ResourceNamingTemplateResult>> Handle(
        SetResourceNamingTemplateCommand command, CancellationToken cancellationToken)
    {
        var authResult = await InfraConfigAccessHelper.VerifyWriteAccessAsync(
            repository, currentUser, command.InfraConfigId, cancellationToken);

        if (authResult.IsError)
            return authResult.Errors;

        var infraConfig = await repository.GetByIdWithNamingTemplatesAsync(command.InfraConfigId, cancellationToken);
        if (infraConfig is null)
            return Errors.InfrastructureConfig.NotFoundError(command.InfraConfigId);

        var entry = infraConfig.SetResourceNamingTemplate(
            command.ResourceType,
            new NamingTemplate(command.Template));

        await repository.UpdateAsync(infraConfig);

        return mapper.Map<ResourceNamingTemplateResult>(entry);
    }
}
