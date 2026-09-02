using ErrorOr;
using InfraFlowSculptor.Application.Common.Helpers;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.DocumentIntelligences.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.DocumentIntelligenceAggregate;
using InfraFlowSculptor.Domain.DocumentIntelligenceAggregate.ValueObjects;
using MapsterMapper;

namespace InfraFlowSculptor.Application.DocumentIntelligences.Commands.CreateDocumentIntelligence;

public class CreateDocumentIntelligenceCommandHandler(
    IDocumentIntelligenceRepository documentIntelligenceRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : ICommandHandler<CreateDocumentIntelligenceCommand, DocumentIntelligenceResult>
{
    public async Task<ErrorOr<DocumentIntelligenceResult>> Handle(CreateDocumentIntelligenceCommand request, CancellationToken cancellationToken)
    {
        var accessResult = await EnsureWriteAccessAsync(request.ResourceGroupId, cancellationToken);
        if (accessResult.IsError)
            return accessResult.Errors;

        var environmentSettingsResult = ParseEnvironmentSettings(request.EnvironmentSettings);
        if (environmentSettingsResult.IsError)
            return environmentSettingsResult.Errors;

        var documentIntelligence = DocumentIntelligence.Create(
            request.ResourceGroupId,
            request.Name,
            request.Location,
            request.CustomSubDomainName,
            environmentSettingsResult.Value,
            isExisting: request.IsExisting);

        var saved = documentIntelligenceRepository.Add(documentIntelligence);

        return mapper.Map<DocumentIntelligenceResult>(saved);
    }

    private async Task<ErrorOr<Success>> EnsureWriteAccessAsync(
        Domain.ResourceGroupAggregate.ValueObjects.ResourceGroupId resourceGroupId,
        CancellationToken cancellationToken)
    {
        var resourceGroup = await resourceGroupRepository.GetByIdAsync(resourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.ResourceGroup.NotFound(resourceGroupId);

        var authResult = await accessService.VerifyWriteAccessAsync(resourceGroup.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        return Result.Success;
    }

    private static ErrorOr<List<(string EnvironmentName, DocumentIntelligenceSku? Sku, PublicNetworkAccessMode? PublicNetworkAccess, bool DisableLocalAuth)>?> ParseEnvironmentSettings(
        IReadOnlyList<DocumentIntelligenceEnvironmentConfigData>? environmentSettings)
    {
        if (environmentSettings is null)
            return (List<(string EnvironmentName, DocumentIntelligenceSku? Sku, PublicNetworkAccessMode? PublicNetworkAccess, bool DisableLocalAuth)>?)null;

        var parsedSettings = new List<(string EnvironmentName, DocumentIntelligenceSku? Sku, PublicNetworkAccessMode? PublicNetworkAccess, bool DisableLocalAuth)>(environmentSettings.Count);
        foreach (var setting in environmentSettings)
        {
            var parsedResult = ParseEnvironmentSetting(setting);
            if (parsedResult.IsError)
                return parsedResult.Errors;

            parsedSettings.Add(parsedResult.Value);
        }

        return parsedSettings;
    }

    private static ErrorOr<(string EnvironmentName, DocumentIntelligenceSku? Sku, PublicNetworkAccessMode? PublicNetworkAccess, bool DisableLocalAuth)> ParseEnvironmentSetting(
        DocumentIntelligenceEnvironmentConfigData setting)
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

        return (setting.EnvironmentName, skuParse.Value, publicNetworkParse.Value, setting.DisableLocalAuth);
    }
}
