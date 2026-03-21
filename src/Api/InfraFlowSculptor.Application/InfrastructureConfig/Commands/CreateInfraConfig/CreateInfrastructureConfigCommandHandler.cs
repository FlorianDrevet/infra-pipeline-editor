using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.CreateInfraConfig;

public class CreateInfrastructureConfigCommandHandler(
    IInfrastructureConfigRepository repository, ICurrentUser currentUser, IMapper mapper)
    : IRequestHandler<CreateInfrastructureConfigCommand,
        ErrorOr<GetInfrastructureConfigResult>>
{
    /// <summary>Default naming template applied to every new configuration.</summary>
    private const string DefaultTemplate = "{name}-{resourceAbbr}{suffix}";

    /// <summary>Per-resource-type naming template overrides applied on creation.</summary>
    private static readonly Dictionary<string, string> DefaultResourceTemplates = new()
    {
        ["ResourceGroup"] = "{resourceAbbr}-{name}{suffix}",
        ["StorageAccount"] = "{name}{resourceAbbr}{suffix}",
    };

    public async Task<ErrorOr<GetInfrastructureConfigResult>> Handle(
        CreateInfrastructureConfigCommand command, CancellationToken cancellationToken)
    {
        var nameVo = new Name(command.Name);
        var userId = await currentUser.GetUserIdAsync(cancellationToken);
        var infra = Domain.InfrastructureConfigAggregate.InfrastructureConfig.Create(nameVo, userId);

        infra.SetDefaultNamingTemplate(new NamingTemplate(DefaultTemplate));

        foreach (var (resourceType, template) in DefaultResourceTemplates)
        {
            infra.SetResourceNamingTemplate(resourceType, new NamingTemplate(template));
        }

        var saved = await repository.AddAsync(infra);

        return mapper.Map<GetInfrastructureConfigResult>(saved);
    }
}