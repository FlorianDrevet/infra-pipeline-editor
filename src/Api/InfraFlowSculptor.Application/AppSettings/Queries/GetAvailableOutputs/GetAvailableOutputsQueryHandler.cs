using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.ResourceOutputs;
using MediatR;

namespace InfraFlowSculptor.Application.AppSettings.Queries.GetAvailableOutputs;

/// <summary>Handles the <see cref="GetAvailableOutputsQuery"/> request.</summary>
public sealed class GetAvailableOutputsQueryHandler(
    IAzureResourceRepository azureResourceRepository)
    : IRequestHandler<GetAvailableOutputsQuery, ErrorOr<AvailableOutputsResult>>
{
    /// <inheritdoc />
    public async Task<ErrorOr<AvailableOutputsResult>> Handle(
        GetAvailableOutputsQuery request,
        CancellationToken cancellationToken)
    {
        var resource = await azureResourceRepository.GetByIdAsync(
            request.ResourceId, cancellationToken);

        if (resource is null)
            return Errors.AppSetting.SourceResourceNotFound(request.ResourceId);

        var resourceTypeName = resource.GetType().Name;
        var outputs = ResourceOutputCatalog.GetForResourceType(resourceTypeName);

        return new AvailableOutputsResult(
            resourceTypeName,
            outputs.Select(o => new OutputDefinitionResult(o.Name, o.Description, o.IsSensitive)).ToList());
    }
}
