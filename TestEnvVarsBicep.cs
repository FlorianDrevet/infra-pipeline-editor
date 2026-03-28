using InfraFlowSculptor.BicepGeneration;
using InfraFlowSculptor.BicepGeneration.Models;

var modules = new List<GeneratedTypeModule>
{
    new()
    {
        ModuleName = "containerAppTest",
        ModuleFileName = "containerApp",
        ModuleFolderName = "ContainerApp",
        ResourceTypeName = "ContainerApp",
        LogicalResourceName = "test",
        ResourceGroupName = "test",
        ResourceAbbreviation = "ca",
        ModuleBicepContent = "param location string\nparam name string\nresource ca 'Microsoft.App/containerApps@2024-03-01' = { name: name }",
        Parameters = new Dictionary<string, object>()
    }
};

var resourceGroups = new List<ResourceGroupDefinition>
{
    new() { Name = "test", Location = "westeurope", ResourceAbbreviation = "rg" }
};

var environments = new List<EnvironmentDefinition>
{
    new() { Name = "Development", ShortName = "dev", Location = "westeurope", Prefix = "", Suffix = "dev" },
    new() { Name = "Production", ShortName = "prd", Location = "westeurope", Prefix = "", Suffix = "prd" }
};

var envNames = new List<string> { "Development", "Production" };

var resources = new List<ResourceDefinition>
{
    new() { Name = "test", Type = "Microsoft.App/containerApps", ResourceGroupName = "test", ResourceAbbreviation = "ca" }
};

var appSettings = new List<AppSettingDefinition>
{
    new()
    {
        Name = "MY_API_URL",
        TargetResourceName = "test",
        EnvironmentValues = new Dictionary<string, string>
        {
            ["Development"] = "https://dev.api.com",
            ["Production"] = "https://prod.api.com"
        }
    }
};

var namingCtx = new NamingContext { DefaultTemplate = "{name}-{resourceAbbr}-{suffix}" };

var result = BicepAssembler.Assemble(modules, resourceGroups, environments, envNames, resources, namingCtx, [], appSettings);

Console.WriteLine("=== main.bicep ===");
Console.WriteLine(result.MainBicep);

Console.WriteLine("\n=== Parameter files ===");
foreach (var (k, v) in result.EnvironmentParameterFiles)
{
    Console.WriteLine($"--- {k} ---");
    Console.WriteLine(v);
}
