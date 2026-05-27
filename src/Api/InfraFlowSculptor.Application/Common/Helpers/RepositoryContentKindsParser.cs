using ErrorOr;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Common.Helpers;

internal static class RepositoryContentKindsParser
{
    public static ErrorOr<RepositoryContentKinds> Parse(IReadOnlyList<string> kinds)
    {
        var flags = RepositoryContentKindsEnum.None;
        foreach (var raw in kinds)
        {
            if (!Enum.TryParse<RepositoryContentKindsEnum>(raw, ignoreCase: true, out var parsed)
                || parsed == RepositoryContentKindsEnum.None)
            {
                return Errors.ProjectRepository.NoContentKind();
            }

            flags |= parsed;
        }

        return RepositoryContentKinds.Create(flags);
    }
}
