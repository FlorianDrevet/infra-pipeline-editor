using FluentValidation;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;

namespace InfraFlowSculptor.Application.Common.Validation;

/// <summary>
/// Shared naming template validation rules applied by both
/// <c>SetResourceNamingTemplateCommandValidator</c> and <c>SetProjectResourceNamingTemplateCommandValidator</c>.
/// </summary>
internal static class NamingTemplateValidationRules
{
    /// <summary>
    /// Applies the standard naming template rules: not empty, max 500, no unknown placeholders.
    /// </summary>
    /// <typeparam name="T">The command type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the template string property.</param>
    /// <returns>The rule builder options for further chaining.</returns>
    internal static IRuleBuilderOptions<T, string> ApplyTemplateRules<T>(
        IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .MaximumLength(500)
            .Must(t => NamingTemplateValidator.GetUnknownPlaceholders(t).Count == 0)
            .WithMessage(t =>
            {
                // Extract the template value via the validation context
                var template = t as string ?? string.Empty;
                var unknown = NamingTemplateValidator.GetUnknownPlaceholders(template);
                return $"Unknown placeholder(s): {string.Join(", ", unknown.Select(p => $"{{{p}}}"))}. " +
                       $"Allowed placeholders are: {string.Join(", ", NamingTemplateValidator.AllowedPlaceholders.Select(p => $"{{{p}}}"))}";
            });
    }

    /// <summary>
    /// Applies the static character validation rule for naming templates,
    /// verifying that static characters in the template are valid for the given resource type.
    /// </summary>
    /// <typeparam name="T">The command type being validated.</typeparam>
    /// <param name="validator">The validator to add the rule to.</param>
    /// <param name="templateSelector">Function to extract the template string from the command.</param>
    /// <param name="resourceTypeSelector">Function to extract the resource type from the command.</param>
    internal static void ApplyStaticCharsRule<T>(
        AbstractValidator<T> validator,
        Func<T, string> templateSelector,
        Func<T, string> resourceTypeSelector)
    {
        validator.RuleFor(x => x)
            .Must(cmd => NamingTemplateValidator.HasValidStaticCharsForResourceType(
                templateSelector(cmd), resourceTypeSelector(cmd)))
            .WithMessage(cmd =>
            {
                var description = NamingTemplateValidator.GetAllowedCharsDescription(resourceTypeSelector(cmd));
                return description is not null
                    ? $"The template contains characters not allowed for {resourceTypeSelector(cmd)}. Allowed: {description}."
                    : "The template contains invalid characters. Only alphanumeric characters, hyphens, and underscores are allowed outside placeholders.";
            })
            .When(cmd => !string.IsNullOrEmpty(templateSelector(cmd)));
    }
}
