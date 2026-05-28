using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.DocumentIntelligences.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using MapsterMapper;

namespace InfraFlowSculptor.Application.DocumentIntelligences.Queries;

public class GetDocumentIntelligenceQueryHandler(
    IDocumentIntelligenceRepository documentIntelligenceRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : IQueryHandler<GetDocumentIntelligenceQuery, DocumentIntelligenceResult>
{
    public async Task<ErrorOr<DocumentIntelligenceResult>> Handle(GetDocumentIntelligenceQuery query, CancellationToken cancellationToken)
    {
        var documentIntelligence = await documentIntelligenceRepository.GetByIdReadOnlyAsync(query.Id, cancellationToken);
        if (documentIntelligence is null)
            return Errors.DocumentIntelligence.NotFoundError(query.Id);

        var resourceGroup = await resourceGroupRepository.GetByIdReadOnlyAsync(documentIntelligence.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.DocumentIntelligence.NotFoundError(query.Id);

        var authResult = await accessService.VerifyReadAccessAsync(resourceGroup.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return Errors.DocumentIntelligence.NotFoundError(query.Id);

        return mapper.Map<DocumentIntelligenceResult>(documentIntelligence);
    }
}
