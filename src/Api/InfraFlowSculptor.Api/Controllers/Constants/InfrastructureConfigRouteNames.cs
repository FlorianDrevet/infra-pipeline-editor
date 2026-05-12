namespace InfraFlowSculptor.Api.Controllers.Constants;

internal static class InfrastructureConfigRouteNames
{
    internal const string ListMyInfrastructureConfigs = nameof(ListMyInfrastructureConfigs);
    internal const string GetInfrastructureConfiguration = nameof(GetInfrastructureConfiguration);
    internal const string ListResourceGroupsByConfig = nameof(ListResourceGroupsByConfig);
    internal const string CreateInfrastructureConfig = nameof(CreateInfrastructureConfig);
    internal const string SetInheritance = nameof(SetInheritance);
    internal const string SetInfraConfigTags = nameof(SetInfraConfigTags);
    internal const string DeleteInfrastructureConfig = nameof(DeleteInfrastructureConfig);
    internal const string ListCrossConfigReferences = nameof(ListCrossConfigReferences);
    internal const string AddCrossConfigReference = nameof(AddCrossConfigReference);
    internal const string RemoveCrossConfigReference = nameof(RemoveCrossConfigReference);
    internal const string ListIncomingCrossConfigReferences = nameof(ListIncomingCrossConfigReferences);
    internal const string GetConfigDiagnostics = nameof(GetConfigDiagnostics);
}
