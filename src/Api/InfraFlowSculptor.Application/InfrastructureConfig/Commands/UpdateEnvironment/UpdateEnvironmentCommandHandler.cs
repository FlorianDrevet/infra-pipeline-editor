using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using MapsterMapper;
using MediatR;
using Location = InfraFlowSculptor.Domain.Common.ValueObjects.Location;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.UpdateEnvironment;

public class UpdateEnvironmentCommandHandler(
    IInfrastructureConfigRepository repository,
    ICurrentUser currentUser,
    IMapper mapper)
    : IRequestHandler<UpdateEnvironmentCommand, ErrorOr<EnvironmentDefinitionResult>>
{
    public async Task<ErrorOr<EnvironmentDefinitionResult>> Handle(
        UpdateEnvironmentCommand command, CancellationToken cancellationToken)
    {
        var authResult = await InfraConfigAccessHelper.VerifyWriteAccessAsync(
            repository, currentUser, command.InfraConfigId, cancellationToken);

        if (authResult.IsError)
            return authResult.Errors;

        var infraConfig = await repository.GetByIdWithEnvironmentsAsync(command.InfraConfigId, cancellationToken);
        if (infraConfig is null)
            return Errors.InfrastructureConfig.NotFoundError(command.InfraConfigId);

        var tags = command.Tags.Select(t => new Tag(t.Name, t.Value));

        var env = infraConfig.UpdateEnvironment(
            command.EnvironmentId,
            new Name(command.Name),
            new Prefix(command.Prefix),
            new Suffix(command.Suffix),
            new Location(Enum.Parse<Location.LocationEnum>(command.Location, ignoreCase: true)),
            new TenantId(command.TenantId),
            new SubscriptionId(command.SubscriptionId),
            new Order(command.Order),
            new RequiresApproval(command.RequiresApproval),
            tags);

        if (env is null)
            return Errors.EnvironmentDefinition.NotFoundError(command.EnvironmentId);

        await repository.UpdateAsync(infraConfig);

        return mapper.Map<EnvironmentDefinitionResult>(env);
    }
}
