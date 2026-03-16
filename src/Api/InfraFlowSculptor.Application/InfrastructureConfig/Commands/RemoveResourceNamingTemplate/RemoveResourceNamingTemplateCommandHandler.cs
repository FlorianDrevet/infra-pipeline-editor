using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using MediatR;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.RemoveResourceNamingTemplate;

public class RemoveResourceNamingTemplateCommandHandler(
    IInfrastructureConfigRepository repository,
    ICurrentUser currentUser)
    : IRequestHandler<RemoveResourceNamingTemplateCommand, ErrorOr<Deleted>>
{
    public async Task<ErrorOr<Deleted>> Handle(
        RemoveResourceNamingTemplateCommand command, CancellationToken cancellationToken)
    {
        var authResult = await InfraConfigAccessHelper.VerifyWriteAccessAsync(
            repository, currentUser, command.InfraConfigId, cancellationToken);

        if (authResult.IsError)
            return authResult.Errors;

        var infraConfig = await repository.GetByIdWithNamingTemplatesAsync(command.InfraConfigId, cancellationToken);
        if (infraConfig is null)
            return Errors.InfrastructureConfig.NotFoundError(command.InfraConfigId);

        if (!infraConfig.RemoveResourceNamingTemplate(command.ResourceType))
            return Error.NotFound(
                code: "ResourceNamingTemplate.NotFound",
                description: $"No naming template override exists for resource type '{command.ResourceType}'.");

        await repository.UpdateAsync(infraConfig);

        return Result.Deleted;
    }
}
