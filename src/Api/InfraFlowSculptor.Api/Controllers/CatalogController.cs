using InfraFlowSculptor.Api.Controllers.Constants;
using InfraFlowSculptor.Application.Common.Interfaces.Catalogs;
using InfraFlowSculptor.Contracts.Catalogs;
using InfraFlowSculptor.Domain.Common.Catalogs.TestFrameworks;
using Microsoft.AspNetCore.Mvc;

namespace InfraFlowSculptor.Api.Controllers;

/// <summary>API endpoints for retrieving reference catalogs (test frameworks, etc.).</summary>
public static class CatalogController
{
    /// <summary>Registers the catalog endpoints.</summary>
    public static IApplicationBuilder UseCatalogController(this IApplicationBuilder builder)
    {
        return builder.UseEndpoints(endpoints =>
        {
            var group = endpoints.MapGroup(Routes.Catalogs)
                .WithTags("Catalogs");

            group.MapGet("/test-frameworks",
                    ([FromQuery] string? stack, ITestFrameworkCatalog catalog) =>
                    {
                        IReadOnlyList<TestFrameworkDefinition> definitions;

                        if (!string.IsNullOrWhiteSpace(stack)
                            && Enum.TryParse<Domain.Common.OwnedEntities.Stacks.ApplicationStack.ApplicationStackEnum>(stack, ignoreCase: true, out var parsed))
                        {
                            definitions = catalog.GetFrameworksForStack(parsed);
                        }
                        else
                        {
                            definitions = catalog.GetAll();
                        }

                        var response = definitions.Select(d => new TestFrameworkDefinitionResponse
                        {
                            Stack = d.Key.Stack.ToString(),
                            FrameworkKey = d.Key.Framework,
                            DisplayName = d.DisplayName,
                            DefaultCommand = d.DefaultCommand,
                            DefaultResultsFormat = d.DefaultResultsFormat,
                            DefaultCoverageTool = d.DefaultCoverageTool,
                            DefaultCoverageReportGlob = d.DefaultCoverageReportGlob,
                        }).ToList();

                        return Results.Ok(response);
                    })
                .WithName(CatalogRouteNames.GetTestFrameworks)
                .Produces<List<TestFrameworkDefinitionResponse>>();
        });
    }
}
