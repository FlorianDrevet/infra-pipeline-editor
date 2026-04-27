using FluentValidation;

namespace InfraFlowSculptor.Application.Projects.Queries.SearchCodeRepoFiles;

/// <summary>Validates the <see cref="SearchCodeRepoFilesQuery"/>.</summary>
public sealed class SearchCodeRepoFilesQueryValidator : AbstractValidator<SearchCodeRepoFilesQuery>
{
    /// <summary>Initializes a new instance of the <see cref="SearchCodeRepoFilesQueryValidator"/> class.</summary>
    public SearchCodeRepoFilesQueryValidator()
    {
        RuleFor(x => x.Branch)
            .NotEmpty()
            .WithMessage("Branch name is required.");

        RuleFor(x => x.FilenamePattern)
            .MaximumLength(100)
            .When(x => x.FilenamePattern is not null)
            .WithMessage("Filename pattern must not exceed 100 characters.");
    }
}
