using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Ir.Builder;

namespace InfraFlowSculptor.BicepGeneration.Generators.Helpers;

/// <summary>
/// Builds exported type definitions for the Container App Bicep module.
/// </summary>
internal static class ContainerAppTypesBicepHelper
{
    internal const string ContainerRuntimeConfigTypeName = "ContainerRuntimeConfig";
    internal const string ScalingConfigTypeName = "ScalingConfig";
    internal const string IngressConfigTypeName = "IngressConfig";
    internal const string HealthProbeConfigTypeName = "HealthProbeConfig";
    internal const string TransportMethodTypeName = "TransportMethod";
    internal const string ProbeConfigTypeName = "ProbeConfig";

    private const string TransportMethodUnion = "'auto' | 'http' | 'http2' | 'tcp'";

    /// <summary>
    /// Registers all Container App exported types on the builder.
    /// </summary>
    internal static BicepModuleBuilder AddContainerAppExportedTypes(this BicepModuleBuilder builder)
    {
        return builder
            .ExportedType(TransportMethodTypeName,
                new BicepRawExpression(TransportMethodUnion),
                description: "Ingress transport method for the Container App")
            .ExportedType(ContainerRuntimeConfigTypeName, new BicepRawExpression(
                    "{\n  @description('CPU cores allocated to the container')\n  cpuCores: string\n  @description('Memory allocated to the container (e.g. 0.5Gi)')\n  memoryGi: string\n}"),
                description: "Container runtime configuration (CPU, memory)")
            .ExportedType(ScalingConfigTypeName, new BicepRawExpression(
                    "{\n  @description('Minimum number of replicas')\n  minReplicas: int\n  @description('Maximum number of replicas')\n  maxReplicas: int\n}"),
                description: "Scaling configuration for the Container App")
            .ExportedType(IngressConfigTypeName, new BicepRawExpression(
                    $"{{\n  @description('Whether ingress is enabled')\n  enabled: bool\n  @description('Target port for ingress traffic')\n  targetPort: int\n  @description('Whether ingress is externally accessible')\n  external: bool\n  @description('Transport method for ingress')\n  transportMethod: {TransportMethodTypeName}\n}}"),
                description: "Ingress configuration for the Container App")
            .ExportedType(ProbeConfigTypeName, new BicepRawExpression(
                    "{\n  @description('HTTP path for the probe (empty to disable)')\n  path: string\n  @description('Port for the probe (0 to disable)')\n  port: int\n}"),
                description: "Configuration for a single HTTP health probe")
            .ExportedType(HealthProbeConfigTypeName, new BicepRawExpression(
                    $"{{\n  @description('Readiness probe configuration')\n  readiness: {ProbeConfigTypeName}\n  @description('Liveness probe configuration')\n  liveness: {ProbeConfigTypeName}\n  @description('Startup probe configuration')\n  startup: {ProbeConfigTypeName}\n}}"),
                description: "Health probe configuration for the Container App");
    }
}
