using InfraFlowSculptor.BicepGeneration.Generators.Constants;
using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Ir.Builder;

namespace InfraFlowSculptor.BicepGeneration.Generators.Helpers;

internal static class MessagingNamespaceBicepGeneratorHelper
{
    internal const string SkuNameTypeName = "SkuName";
    internal const string TlsVersionTypeName = "TlsVersion";
    internal const string DefaultSkuName = "Standard";
    internal const string DefaultMinimumTlsVersion = "1.2";
    internal const string SkuNameUnion = "'Basic' | 'Standard' | 'Premium'";
    internal const string TlsVersionUnion = "'1.0' | '1.1' | '1.2'";

    internal static BicepModuleBuilder AddMessagingNamespaceTypeImport(this BicepModuleBuilder builder)
    {
        return builder.Import(
            BicepGeneratorSharedConstants.TypesImportPath,
            SkuNameTypeName,
            TlsVersionTypeName);
    }

    internal static BicepModuleBuilder AddMessagingNamespaceExportedTypes(
        this BicepModuleBuilder builder,
        string resourceDisplayName)
    {
        return builder
            .ExportedType(
                SkuNameTypeName,
                new BicepRawExpression(SkuNameUnion),
                description: $"SKU name for the {resourceDisplayName}")
            .ExportedType(
                TlsVersionTypeName,
                new BicepRawExpression(TlsVersionUnion),
                description: $"Minimum TLS version for the {resourceDisplayName}");
    }

    internal static string BuildTypesTemplate(string resourceDisplayName)
    {
        return $$"""
            @export()
            @description('SKU name for the {{resourceDisplayName}}')
            type SkuName = 'Basic' | 'Standard' | 'Premium'

            @export()
            @description('Minimum TLS version for the {{resourceDisplayName}}')
            type TlsVersion = '1.0' | '1.1' | '1.2'
            """;
    }
}
