using InfraFlowSculptor.GenerationCore;
using static InfraFlowSculptor.BicepGeneration.TextManipulation.BicepTextManipulationHelpers;

namespace InfraFlowSculptor.BicepGeneration.TextManipulation;

/// <summary>
/// Injects an <c>appSettings</c> array (Web/Function Apps) or <c>envVars</c> array
/// (Container Apps) parameter into a compute resource module template.
/// </summary>
public static class BicepAppSettingsInjector
{
    /// <summary>
    /// Routes injection to the correct compute-specific implementation based on
    /// <paramref name="armResourceType"/>.
    /// </summary>
    public static string Inject(string moduleBicep, string armResourceType)
    {
        if (armResourceType == AzureResourceTypes.ArmTypes.ContainerApp)
            return InjectContainerAppEnvVars(moduleBicep);

        return InjectWebFunctionAppSettings(moduleBicep);
    }

    /// <summary>
    /// Injects <c>param appSettings array = []</c> and wires it into <c>siteConfig.appSettings</c>
    /// for WebApp and FunctionApp modules. When an existing <c>appSettings: [...]</c> array is
    /// present, the call wraps it via <c>concat([...], appSettings)</c>.
    /// </summary>
    public static string InjectWebFunctionAppSettings(string moduleBicep)
    {
        if (ParamAppSettingsPattern.IsMatch(moduleBicep))
            return moduleBicep;

        const string paramDecl = "\n@description('Application settings (environment variables)')\nparam appSettings array = []\n";

        var insertIdx = FindFirstLineIndex(moduleBicep, "var ");
        if (insertIdx < 0)
            insertIdx = FindFirstLineIndex(moduleBicep, "resource ");
        if (insertIdx < 0)
            return moduleBicep;

        moduleBicep = InsertAt(moduleBicep, insertIdx, paramDecl + "\n");

        var appSettingsMatch = AppSettingsArrayPattern.Match(moduleBicep);
        if (appSettingsMatch.Success)
        {
            // FunctionApp case: existing appSettings array → wrap with concat
            var appSettingsStart = appSettingsMatch.Index;
            var bracketStart = moduleBicep.IndexOf('[', appSettingsStart);
            var depth = 0;
            var bracketEnd = -1;
            for (var i = bracketStart; i < moduleBicep.Length; i++)
            {
                if (moduleBicep[i] == '[') depth++;
                else if (moduleBicep[i] == ']')
                {
                    depth--;
                    if (depth == 0) { bracketEnd = i; break; }
                }
            }

            if (bracketEnd >= 0)
            {
                var existingArray = moduleBicep.Substring(bracketStart, bracketEnd - bracketStart + 1);
                var original = moduleBicep.Substring(appSettingsStart, bracketEnd - appSettingsStart + 1);
                var replacement = $"appSettings: concat({existingArray}, appSettings)";
                moduleBicep = moduleBicep.Replace(original, replacement);
            }
        }
        else
        {
            // WebApp case: add appSettings into siteConfig
            var siteConfigMatch = SiteConfigPattern.Match(moduleBicep);
            if (siteConfigMatch.Success)
            {
                var braceIdx = moduleBicep.IndexOf('{', siteConfigMatch.Index);
                if (braceIdx >= 0)
                {
                    moduleBicep = InsertAt(moduleBicep, braceIdx + 1, "\n      appSettings: appSettings");
                }
            }
        }

        return moduleBicep;
    }

    /// <summary>
    /// Injects <c>param envVars array = []</c> and wires it into the Container App container spec
    /// as the <c>env</c> property.
    /// </summary>
    public static string InjectContainerAppEnvVars(string moduleBicep)
    {
        if (ParamEnvVarsPattern.IsMatch(moduleBicep))
            return moduleBicep;

        const string paramDecl = "\n@description('Environment variables for the container')\nparam envVars array = []\n";

        var insertIdx = FindFirstLineIndex(moduleBicep, "resource ");
        if (insertIdx < 0)
            return moduleBicep;

        moduleBicep = InsertAt(moduleBicep, insertIdx, paramDecl + "\n");

        if (!EnvPropertyPattern.IsMatch(moduleBicep))
        {
            var resourcesMatch = ContainerResourcesPattern.Match(moduleBicep);
            if (resourcesMatch.Success)
            {
                var indent = resourcesMatch.Value[..resourcesMatch.Value.IndexOf('r')];
                moduleBicep = InsertAt(moduleBicep, resourcesMatch.Index, $"{indent}env: envVars\n");
            }
        }

        return moduleBicep;
    }
}
