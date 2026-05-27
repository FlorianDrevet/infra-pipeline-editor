namespace InfraFlowSculptor.BicepGeneration.Generators.Constants;

/// <summary>
/// Provides shared literals for concrete Bicep generators when the literal has the same stable meaning across resource types.
/// </summary>
internal static class BicepGeneratorSharedConstants
{
    internal const string TypesImportPath = "./types.bicep";
    internal const string NameParameterName = "name";
    internal const string LocationParameterName = "location";
    internal const string NamePropertyName = "name";
    internal const string LocationPropertyName = "location";
    internal const string PropertiesPropertyName = "properties";
    internal const string KindPropertyName = "kind";
    internal const string IdOutputName = "id";
    internal const string BooleanTrueString = "true";
    internal const string BooleanFalseString = "false";
}
