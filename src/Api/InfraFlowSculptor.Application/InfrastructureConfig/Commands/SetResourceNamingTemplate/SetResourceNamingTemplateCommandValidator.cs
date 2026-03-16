using FluentValidation;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.SetResourceNamingTemplate;

public class SetResourceNamingTemplateCommandValidator : AbstractValidator<SetResourceNamingTemplateCommand>
{
    public SetResourceNamingTemplateCommandValidator()
    {
        RuleFor(x => x.ResourceType).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Template).NotEmpty().MaximumLength(500);
    }
}
