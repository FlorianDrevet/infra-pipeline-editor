using System.Text;

namespace BicepGenerator.Domain;

public static class BicepAssembler
{
    public static GenerationResult Assemble(
        IReadOnlyCollection<GeneratedTypeModule> modules)
    {
        var main = GenerateMainBicep(modules);
        var parameters = GenerateMainParameters(modules);
        var moduleFiles = modules.ToDictionary(
            m => $"modules/{m.ModuleFileName}",
            m => m.ModuleBicepContent);

        return new GenerationResult
        {
            MainBicep = main,
            MainBicepParameters = parameters,
            ModuleFiles = moduleFiles
        };
    }
    
    private static string GenerateMainParameters(
        IReadOnlyCollection<GeneratedTypeModule> modules)
    {
        var sb = new StringBuilder();

        sb.AppendLine("""
                      using 'main.bicep'
                      """);

        sb.AppendLine("param location = 'westeurope'");
        sb.AppendLine();

        foreach (var module in modules)
        {
            foreach (var param in module.Parameters)
            {
                if (param.Key == "location") 
                    continue;

                sb.AppendLine($"param {param.Key} = {SerializeToBicep(param.Value)}");
                sb.AppendLine();
            }
        }

        return sb.ToString();
    }


    private static string GenerateMainBicep(
        IReadOnlyCollection<GeneratedTypeModule> modules)
    {
        var sb = new StringBuilder();

        sb.AppendLine("""
                      targetScope = 'resourceGroup'
                      """);

        sb.AppendLine("param location string");
        sb.AppendLine();

        // paramètres agrégés par module
        foreach (var module in modules)
        {
            foreach (var param in module.Parameters.Keys)
            {
                if (param != "location")
                    sb.AppendLine($"param {param} array");
            }
        }

        sb.AppendLine();

        foreach (var module in modules)
        {
            sb.AppendLine($$"""
                            module {{module.ModuleName}} './modules/{{module.ModuleFileName}}' = {
                              name: '{{module.ModuleName}}'
                              params: {
                                location: location
                            """);

            foreach (var param in module.Parameters.Keys)
            {
                if (param != "location")
                    sb.AppendLine($"    {param}: {param}");
            }

            sb.AppendLine("""
                            }
                          }
                          """);
        }

        return sb.ToString();
    }
    
    private static string SerializeToBicep(object value)
    {
        return value switch
        {
            string s => $"'{s}'",
            int or long or double or bool => value.ToString()!,
            IEnumerable<object> arr => SerializeArray(arr),
            _ => SerializeObject(value)
        };
    }

    private static string SerializeArray(IEnumerable<object> values)
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
            var value = p.GetValue(obj);
            sb.AppendLine($"  {p.Name}: {SerializeToBicep(value!)}");
        }

        sb.Append("}");
        return sb.ToString();
    }
}