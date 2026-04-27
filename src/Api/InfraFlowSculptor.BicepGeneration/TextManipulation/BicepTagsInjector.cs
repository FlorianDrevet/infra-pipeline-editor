using System.Text.RegularExpressions;
using static InfraFlowSculptor.BicepGeneration.TextManipulation.BicepTextManipulationHelpers;

namespace InfraFlowSculptor.BicepGeneration.TextManipulation;

/// <summary>
/// Injects a <c>param tags object = {}</c> declaration and a <c>tags: tags</c> property
/// into a generated Bicep module template. No-op when tags are already declared.
/// </summary>
public static class BicepTagsInjector
{
    /// <summary>
    /// Adds the <c>tags</c> param after the last existing <c>param</c> line and the
    /// <c>tags: tags</c> property right after the <c>location: location</c> property.
    /// </summary>
    public static string Inject(string moduleBicep)
    {
        if (ParamTagsPattern.IsMatch(moduleBicep))
            return moduleBicep;

        const string paramDecl = "\n@description('Resource tags')\nparam tags object = {}\n";

        var paramMatches = Regex.Matches(moduleBicep, @"\nparam\s+");
        if (paramMatches.Count > 0)
        {
            var lastMatch = paramMatches[^1];
            var endOfLine = moduleBicep.IndexOf('\n', lastMatch.Index + 1);
            if (endOfLine >= 0)
            {
                moduleBicep = InsertAt(moduleBicep, endOfLine, paramDecl);
            }
        }

        var locationMatch = LocationPropertyPattern.Match(moduleBicep);
        if (locationMatch.Success)
        {
            var endOfLocationLine = moduleBicep.IndexOf('\n', locationMatch.Index + locationMatch.Length);
            if (endOfLocationLine >= 0)
            {
                moduleBicep = InsertAt(moduleBicep, endOfLocationLine, "\n  tags: tags");
            }
        }

        return moduleBicep;
    }
}
