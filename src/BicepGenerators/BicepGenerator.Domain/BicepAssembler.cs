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

        // Outer param declarations — one param per module (using module name to ensure
        // uniqueness across resource-group/type combinations and avoid BCP028).
        foreach (var module in modules)
        {
            foreach (var param in module.Parameters)
            {
                if (param.Key == "location")
                    continue;

                var bicepType = InferBicepType(param.Value);
                sb.AppendLine($"param {module.ModuleName} {bicepType}");
            }
        }

        sb.AppendLine();

        // Resource group declarations
        foreach (var rg in resourceGroups)
        {
            var rgSymbol = BicepIdentifierHelper.ToBicepIdentifier(rg.Name);
            sb.AppendLine($"resource {rgSymbol} 'Microsoft.Resources/resourceGroups@2022-09-01' = {{");
            sb.AppendLine($"  name: '{rg.Name}'");
            sb.AppendLine("  location: location");
            sb.AppendLine("}");
            sb.AppendLine();
        }

        // Module declarations — symbol uses Module suffix to avoid BCP028 with param names
        foreach (var module in modules)
        {
            var rgSymbol = BicepIdentifierHelper.ToBicepIdentifier(module.ResourceGroupName);
            var moduleSymbol = $"{module.ModuleName}Module";
            sb.AppendLine($"module {moduleSymbol} './modules/{module.ModuleFileName}' = {{");
            sb.AppendLine($"  name: '{module.ModuleName}'");
            sb.AppendLine($"  scope: {rgSymbol}");
            sb.AppendLine("  params: {");
            sb.AppendLine("    location: location");

            foreach (var paramKey in module.Parameters.Keys)
            {
                if (paramKey != "location")
                    // Map the module's internal param name to the outer param (= module name)
                    sb.AppendLine($"    {paramKey}: {module.ModuleName}");
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
            foreach (var param in module.Parameters)
            {
                if (param.Key == "location")
                    continue;

                // Outer param name = module name (matches the declaration in main.bicep)
                sb.AppendLine($"param {module.ModuleName} = {SerializeToBicep(param.Value)}");
                sb.AppendLine();
            }
        }

        return sb.ToString();
    }

    private static string InferBicepType(object value)
    {
        return value switch
        {
            string => "string",
            int or long or double => "int",
            bool => "bool",
            IEnumerable<object> => "array",
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
            IEnumerable<object> items => SerializeCollection(items),
            _ => SerializeObject(value)
        };
    }

    private static string SerializeCollection(IEnumerable<object> values)
    {
        var sb = new StringBuilder();
        sb.AppendLine("[");

        foreach (var v in values)
        {
            sb.AppendLine($"  {SerializeToBicep(v)}");
        }

        sb.Append("]");
        return sb.ToString();
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
