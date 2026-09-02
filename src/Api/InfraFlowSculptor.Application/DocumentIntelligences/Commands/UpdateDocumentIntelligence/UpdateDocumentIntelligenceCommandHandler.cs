using ErrorOr;
using InfraFlowSculptor.Application.Common.Helpers;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.DocumentIntelligences.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.DocumentIntelligenceAggregate.ValueObjects;
using MapsterMapper;

namespace InfraFlowSculptor.Application.DocumentIntelligences.Commands.UpdateDocumentIntelligence;

public class UpdateDocumentIntelligenceCommandHandler(
    IDocumentIntelligenceRepository documentIntelligenceRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : ICommandHandler<UpdateDocumentIntelligenceCommand, DocumentIntelligenceResult>
{
    public async Task<ErrorOr<DocumentIntelligenceResult>> Handle(UpdateDocumentIntelligenceCommand request, CancellationToken cancellationToken)
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

        documentIntelligence.Update(
            request.Name,
            request.Location,
            request.CustomSubDomainName);

        if (request.EnvironmentSettings is not null)
        {
            var parsed = ParseEnvironmentSettings(request.EnvironmentSettings);
            if (parsed.IsError) return parsed.Errors;
            documentIntelligence.SetAllEnvironmentSettings(parsed.Value);
        }

        var updated = documentIntelligenceRepository.Update(documentIntelligence);
        return mapper.Map<DocumentIntelligenceResult>(updated);
    }

    private static ErrorOr<List<(string EnvironmentName, DocumentIntelligenceSku? Sku, PublicNetworkAccessMode? PublicNetworkAccess, bool DisableLocalAuth)>> ParseEnvironmentSettings(
        IEnumerable<DocumentIntelligenceEnvironmentConfigData> environmentSettings)
    {
        var parsedSettings = new List<(string EnvironmentName, DocumentIntelligenceSku? Sku, PublicNetworkAccessMode? PublicNetworkAccess, bool DisableLocalAuth)>();
        foreach (var setting in environmentSettings)
        {
            var skuParse = EnumValueObjectParser.ParseOrNull<DocumentIntelligenceSku.Sku, DocumentIntelligenceSku>(
                setting.Sku,
                static parsed => new DocumentIntelligenceSku(parsed),
                Errors.DocumentIntelligence.InvalidSku);
            if (skuParse.IsError) return skuParse.Errors;

            var publicNetworkParse = EnumValueObjectParser.ParseOrNull<PublicNetworkAccessMode.Mode, PublicNetworkAccessMode>(
                setting.PublicNetworkAccess,
                static parsed => new PublicNetworkAccessMode(parsed),
                Errors.DocumentIntelligence.InvalidPublicNetworkAccess);
            if (publicNetworkParse.IsError) return publicNetworkParse.Errors;

            parsedSettings.Add((setting.EnvironmentName, skuParse.Value, publicNetworkParse.Value, setting.DisableLocalAuth));
        }

        return parsedSettings;
    }
}
