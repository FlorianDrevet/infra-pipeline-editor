using FluentValidation;

namespace InfraFlowSculptor.Application.Projects.Queries.SearchCodeRepoDirectories;

/// <summary>Validates the <see cref="SearchCodeRepoDirectoriesQuery"/>.</summary>
public sealed class SearchCodeRepoDirectoriesQueryValidator : AbstractValidator<SearchCodeRepoDirectoriesQuery>
{
    /// <summary>Initializes a new instance of the <see cref="SearchCodeRepoDirectoriesQueryValidator"/> class.</summary>
    public SearchCodeRepoDirectoriesQueryValidator()
    {
        RuleFor(x => x.ProjectId).NotNull().WithMessage("ProjectId is required.");
        RuleFor(x => x.Branch).NotEmpty().WithMessage("Branch is required.");
    }
}
