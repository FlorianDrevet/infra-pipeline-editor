using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Application.AppSettings.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.AppSettings.Queries.ListAppSettings;

/// <summary>Query to list all app settings on a resource.</summary>
/// <param name="ResourceId">Identifier of the Azure resource.</param>
public record ListAppSettingsQuery(AzureResourceId ResourceId)
    : IQuery<IReadOnlyList<AppSettingResult>>;
