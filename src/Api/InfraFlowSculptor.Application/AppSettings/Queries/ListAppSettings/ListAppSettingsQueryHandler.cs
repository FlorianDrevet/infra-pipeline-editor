using ErrorOr;
using InfraFlowSculptor.Application.AppSettings.Common;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;
using MediatR;

namespace InfraFlowSculptor.Application.AppSettings.Queries.ListAppSettings;

/// <summary>Handles the <see cref="ListAppSettingsQuery"/> request.</summary>
public sealed class ListAppSettingsQueryHandler(
    IAzureResourceRepository azureResourceRepository)
    : IRequestHandler<ListAppSettingsQuery, ErrorOr<IReadOnlyList<AppSettingResult>>>
{
    /// <inheritdoc />
    public async Task<ErrorOr<IReadOnlyList<AppSettingResult>>> Handle(
        ListAppSettingsQuery request,
        CancellationToken cancellationToken)
    {
        var resource = await azureResourceRepository.GetByIdWithAppSettingsAsync(
            request.ResourceId, cancellationToken);

        if (resource is null)
            return Errors.AppSetting.SourceResourceNotFound(request.ResourceId);

        return resource.AppSettings
            .Select(s => new AppSettingResult(
                s.Id, s.ResourceId, s.Name,
                s.EnvironmentValues.Count > 0
                    ? s.EnvironmentValues.ToDictionary(ev => ev.EnvironmentName, ev => ev.Value)
                    : null,
                s.SourceResourceId,
                s.SourceOutputName, s.IsOutputReference,
                s.KeyVaultResourceId, s.SecretName,
                s.IsKeyVaultReference, null,
                s.SecretValueAssignment))
            .ToList();
    }
}
