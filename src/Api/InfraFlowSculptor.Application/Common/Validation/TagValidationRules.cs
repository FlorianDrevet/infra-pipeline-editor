using System.Linq.Expressions;
using FluentValidation;

namespace InfraFlowSculptor.Application.Common.Validation;

/// <summary>
/// Shared tag validation rules applied by both <c>SetInfraConfigTagsCommandValidator</c>
/// and <c>SetProjectTagsCommandValidator</c>.
/// </summary>
internal static class TagValidationRules
{
    /// <summary>
    /// Applies the standard tag list rules to a validator:
    /// max 15 tags, each tag name required (max 512), each tag value required (max 256).
    /// </summary>
    /// <typeparam name="T">The command type being validated.</typeparam>
    /// <param name="validator">The validator to add rules to.</param>
    /// <param name="tagsSelector">Expression selecting the tags collection from the command.</param>
    internal static void ApplyTagRules<T>(
        AbstractValidator<T> validator,
        Expression<Func<T, IReadOnlyList<(string Name, string Value)>>> tagsSelector)
    {
        validator.RuleFor(tagsSelector)
            .Must(t => t.Count <= 15).WithMessage("A maximum of 15 tags is allowed.");

        // Compose an IEnumerable-typed expression so FluentValidation's RuleForEach can infer TElement.
        var enumerableSelector = Expression.Lambda<Func<T, IEnumerable<(string Name, string Value)>>>(
            tagsSelector.Body, tagsSelector.Parameters);

        validator.RuleForEach(enumerableSelector).ChildRules(tag =>
        {
            tag.RuleFor(t => t.Name)
                .NotEmpty().WithMessage("Tag name is required.")
                .MaximumLength(512).WithMessage("Tag name must not exceed 512 characters.");
            tag.RuleFor(t => t.Value)
                .NotEmpty().WithMessage("Tag value is required.")
                .MaximumLength(256).WithMessage("Tag value must not exceed 256 characters.");
        });
    }
}
