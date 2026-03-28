using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

/// <summary>Strongly-typed identifier for <see cref="Entites.AppSettingEnvironmentValue"/>.</summary>
public sealed class AppSettingEnvironmentValueId(Guid value) : Id<AppSettingEnvironmentValueId>(value);
