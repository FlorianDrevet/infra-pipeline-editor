using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Projects.Common;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.Projects.Queries.GetProject;

/// <summary>Handles the <see cref="GetProjectQuery"/> request.</summary>
public sealed class GetProjectQueryHandler(
    IProjectAccessService accessService,
    IMapper mapper)
    : IRequestHandler<GetProjectQuery, ErrorOr<ProjectResult>>
{
    /// <inheritdoc />
    public async Task<ErrorOr<ProjectResult>> Handle(
        GetProjectQuery query, CancellationToken cancellationToken)
    {
        var accessResult = await accessService.VerifyReadAccessAsync(query.Id, cancellationToken);
        if (accessResult.IsError)
            return accessResult.Errors;

        return mapper.Map<ProjectResult>(accessResult.Value);
    }
}
