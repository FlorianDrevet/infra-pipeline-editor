using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

/// <summary>Strongly-typed identifier for <see cref="Entites.AppSetting"/>.</summary>
public sealed class AppSettingId(Guid value) : Id<AppSettingId>(value);
