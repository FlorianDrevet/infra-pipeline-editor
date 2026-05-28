using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;

namespace InfraFlowSculptor.Application.DocumentIntelligences.Commands.DeleteDocumentIntelligence;

public class DeleteDocumentIntelligenceCommandHandler(
    IDocumentIntelligenceRepository documentIntelligenceRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService)
    : ICommandHandler<DeleteDocumentIntelligenceCommand, Deleted>
{
    public async Task<ErrorOr<Deleted>> Handle(DeleteDocumentIntelligenceCommand request, CancellationToken cancellationToken)
    {
        var documentIntelligence = await documentIntelligenceRepository.GetByIdAsync(request.Id, cancellationToken);
        if (documentIntelligence is null)
            return Errors.DocumentIntelligence.NotFoundError(request.Id);

        var resourceGroup = await resourceGroupRepository.GetByIdAsync(documentIntelligence.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.DocumentIntelligence.NotFoundError(request.Id);

        var authResult = await accessService.VerifyWriteAccessAsync(resourceGroup.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        await documentIntelligenceRepository.DeleteAsync(request.Id);

        return Result.Deleted;
    }
}
