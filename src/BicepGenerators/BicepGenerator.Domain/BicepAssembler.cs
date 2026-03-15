using System.Text;

namespace BicepGenerator.Domain;

public static class BicepAssembler
{
    public static GenerationResult Assemble(
        IReadOnlyCollection<GeneratedTypeModule> modules,
        IReadOnlyList<ResourceGroupDefinition> resourceGroups)
    {
        var main = GenerateMainBicep(modules, resourceGroups);
        var parameters = GenerateMainParameters(modules);
        var moduleFiles = modules
            .DistinctBy(m => m.ModuleFileName)
            .ToDictionary(
                m => $"modules/{m.ModuleFileName}",
                m => m.ModuleBicepContent);

        return new GenerationResult
        {
            MainBicep = main,
            MainBicepParameters = parameters,
            ModuleFiles = moduleFiles
        };
    }

    private static string GenerateMainBicep(
        IReadOnlyCollection<GeneratedTypeModule> modules,
        IReadOnlyList<ResourceGroupDefinition> resourceGroups)
    {
        var sb = new StringBuilder();

        sb.AppendLine("targetScope = 'subscription'");
        sb.AppendLine();
        sb.AppendLine("param location string = 'westeurope'");
        sb.AppendLine();

        // Outer param declarations — one param per property per resource instance,
        // prefixed with the module name to ensure global uniqueness and avoid BCP028.
        foreach (var module in modules)
        {
            foreach (var (key, value) in module.Parameters.Where(p => p.Key != "location"))
            {
                var bicepType = InferBicepType(value);
                sb.AppendLine($"param {module.ModuleName}{Capitalize(key)} {bicepType}");
            }
        }

        sb.AppendLine();

        // Resource group declarations
        foreach (var (rg, rgSymbol) in resourceGroups.Select(rg => (rg, BicepIdentifierHelper.ToBicepIdentifier(rg.Name))))
        {
            sb.AppendLine($"resource {rgSymbol} 'Microsoft.Resources/resourceGroups@2022-09-01' = {{");
            sb.AppendLine($"  name: '{rg.Name}'");
            sb.AppendLine("  location: location");
            sb.AppendLine("}");
            sb.AppendLine();
        }

        // One module declaration per resource instance
        foreach (var module in modules)
        {
            var rgSymbol = BicepIdentifierHelper.ToBicepIdentifier(module.ResourceGroupName);
            var moduleSymbol = $"{module.ModuleName}Module";
            sb.AppendLine($"module {moduleSymbol} './modules/{module.ModuleFileName}' = {{");
            sb.AppendLine($"  name: '{module.ModuleName}'");
            sb.AppendLine($"  scope: {rgSymbol}");
            sb.AppendLine("  params: {");
            sb.AppendLine("    location: location");

            foreach (var paramKey in module.Parameters.Keys.Where(paramKey => paramKey != "location"))
            {
                // Bind the module's internal param to its uniquely-named outer param
                sb.AppendLine($"    {paramKey}: {module.ModuleName}{Capitalize(paramKey)}");
            }

            sb.AppendLine("  }");
            sb.AppendLine("}");
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private static string GenerateMainParameters(
        IReadOnlyCollection<GeneratedTypeModule> modules)
    {
        var sb = new StringBuilder();

        sb.AppendLine("using 'main.bicep'");
        sb.AppendLine();
        sb.AppendLine("param location = 'westeurope'");
        sb.AppendLine();

        foreach (var module in modules)
        {
            foreach (var (key, value) in module.Parameters.Where(p => p.Key != "location"))
            {
                // Outer param name = moduleName + capitalised key (matches main.bicep declaration)
                sb.AppendLine($"param {module.ModuleName}{Capitalize(key)} = {SerializeToBicep(value)}");
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }

    private static string Capitalize(string s) =>
        s.Length == 0 ? s : char.ToUpperInvariant(s[0]) + s[1..];

    private static string InferBicepType(object value)
    {
        return value switch
        {
            string => "string",
            int or long or double => "int",
            bool => "bool",
            _ => "object"
        };
    }

    private static string SerializeToBicep(object value)
    {
        return value switch
        {
            string s => $"'{s}'",
            bool b => b ? "true" : "false",
            int or long or double => value.ToString()!,
            _ => SerializeObject(value)
        };
    }

    private static string SerializeObject(object obj)
    {
        var props = obj.GetType().GetProperties();

        var sb = new StringBuilder();
        sb.AppendLine("{");

        foreach (var p in props)
        {
            var propValue = p.GetValue(obj);
            if (propValue is not null)
                sb.AppendLine($"  {p.Name}: {SerializeToBicep(propValue)}");
        }

        sb.Append("}");
        return sb.ToString();
    }
}
