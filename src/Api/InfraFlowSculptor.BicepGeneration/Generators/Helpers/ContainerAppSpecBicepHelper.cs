using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Ir.Builder;

namespace InfraFlowSculptor.BicepGeneration.Generators.Helpers;

/// <summary>
/// Builds the ingress, template, and probes IR nodes for the Container App <c>GenerateSpec</c> path.
/// </summary>
internal static class ContainerAppSpecBicepHelper
{
    private const string IngressParameterName = "ingress";
    private const string HealthProbesParameterName = "healthProbes";
    private const string ContainerRuntimeParameterName = "containerRuntime";
    private const string ScalingParameterName = "scaling";

    private const string ExternalPropertyName = "external";
    private const string TargetPortPropertyName = "targetPort";
    private const string TransportPropertyName = "transport";
    private const string TransportMethodPropertyName = "transportMethod";
    private const string EnabledPropertyName = "enabled";
    private const string ContainersPropertyName = "containers";
    private const string ImagePropertyName = "image";
    private const string ResourcesPropertyName = "resources";
    private const string CpuPropertyName = "cpu";
    private const string MemoryPropertyName = "memory";
    private const string ProbesPropertyName = "probes";
    private const string ScalePropertyName = "scale";
    private const string MinReplicasPropertyName = "minReplicas";
    private const string MaxReplicasPropertyName = "maxReplicas";
    private const string CpuCoresPropertyName = "cpuCores";
    private const string MemoryGiPropertyName = "memoryGi";
    private const string CustomDomainsParameterName = "customDomains";
    private const string CustomDomainBindingsVariableName = "customDomainBindings";
    private const string NullExpression = "null";
    private const string CustomDomainsConditionExpression = "!empty(customDomains)";

    private const string IngressEnabledSelector = IngressParameterName + "." + EnabledPropertyName;
    private const string IngressExternalSelector = IngressParameterName + "." + ExternalPropertyName;
    private const string IngressTargetPortSelector = IngressParameterName + "." + TargetPortPropertyName;
    private const string IngressTransportMethodSelector = IngressParameterName + "." + TransportMethodPropertyName;
    private const string ContainerRuntimeCpuJsonExpression = "json(" + ContainerRuntimeParameterName + "." + CpuCoresPropertyName + ")";
    private const string ContainerRuntimeMemorySelector = ContainerRuntimeParameterName + "." + MemoryGiPropertyName;
    private const string ScalingMinReplicasSelector = ScalingParameterName + "." + MinReplicasPropertyName;
    private const string ScalingMaxReplicasSelector = ScalingParameterName + "." + MaxReplicasPropertyName;

    private const string ProbesUnionExpression = """
        union(
          !empty(healthProbes.readiness.path) && healthProbes.readiness.port > 0 ? [{
            type: 'Readiness'
            httpGet: {
              path: healthProbes.readiness.path
              port: healthProbes.readiness.port
            }
          }] : [],
          !empty(healthProbes.liveness.path) && healthProbes.liveness.port > 0 ? [{
            type: 'Liveness'
            httpGet: {
              path: healthProbes.liveness.path
              port: healthProbes.liveness.port
            }
          }] : [],
          !empty(healthProbes.startup.path) && healthProbes.startup.port > 0 ? [{
            type: 'Startup'
            httpGet: {
              path: healthProbes.startup.path
              port: healthProbes.startup.port
            }
          }] : []
        )
        """;

    /// <summary>
    /// Builds the ingress configuration property assignment (conditional on <c>ingress.enabled</c>).
    /// </summary>
    internal static BicepPropertyAssignment BuildIngressConfigProperty(
        bool hasValidatedCustomDomains)
    {
        var ingressProperties = new List<BicepPropertyAssignment>
        {
            new(ExternalPropertyName, new BicepReference(IngressExternalSelector)),
            new(TargetPortPropertyName, new BicepReference(IngressTargetPortSelector)),
            new(TransportPropertyName, new BicepReference(IngressTransportMethodSelector)),
        };

        if (hasValidatedCustomDomains)
        {
            ingressProperties.Add(new BicepPropertyAssignment(
                CustomDomainsParameterName,
                new BicepConditionalExpression(
                    new BicepRawExpression(CustomDomainsConditionExpression),
                    new BicepReference(CustomDomainBindingsVariableName),
                    new BicepRawExpression(NullExpression))));
        }

        return new BicepPropertyAssignment(
            "ingress",
            new BicepConditionalExpression(
                new BicepReference(IngressEnabledSelector),
                new BicepObjectExpression(ingressProperties),
                new BicepRawExpression(NullExpression)));
    }

    /// <summary>
    /// Builds the template sub-object (containers array + scale) for the resource properties.
    /// </summary>
    internal static BicepObjectExpression BuildTemplateObject(
        string nameParameterName,
        string containerImageParameterName)
    {
        return new BicepObjectExpression([
            new BicepPropertyAssignment(ContainersPropertyName, new BicepArrayExpression([
                new BicepObjectExpression([
                    new BicepPropertyAssignment("name", new BicepReference(nameParameterName)),
                    new BicepPropertyAssignment(ImagePropertyName, new BicepReference(containerImageParameterName)),
                    new BicepPropertyAssignment(ResourcesPropertyName, new BicepObjectExpression([
                        new BicepPropertyAssignment(CpuPropertyName, new BicepRawExpression(ContainerRuntimeCpuJsonExpression)),
                        new BicepPropertyAssignment(MemoryPropertyName, new BicepReference(ContainerRuntimeMemorySelector)),
                    ])),
                    new BicepPropertyAssignment(ProbesPropertyName, new BicepRawExpression(ProbesUnionExpression)),
                ]),
            ])),
            new BicepPropertyAssignment(ScalePropertyName, new BicepObjectExpression([
                new BicepPropertyAssignment(MinReplicasPropertyName, new BicepReference(ScalingMinReplicasSelector)),
                new BicepPropertyAssignment(MaxReplicasPropertyName, new BicepReference(ScalingMaxReplicasSelector)),
            ])),
        ]);
    }
}
