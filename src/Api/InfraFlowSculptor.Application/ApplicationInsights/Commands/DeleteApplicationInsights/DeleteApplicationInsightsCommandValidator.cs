using FluentValidation;

namespace InfraFlowSculptor.Application.ApplicationInsights.Commands.DeleteApplicationInsights;

/// <summary>Validates the <see cref="DeleteApplicationInsightsCommand"/>.</summary>
public sealed class DeleteApplicationInsightsCommandValidator : AbstractValidator<DeleteApplicationInsightsCommand>
{
    public DeleteApplicationInsightsCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id is required.");
    }
}
