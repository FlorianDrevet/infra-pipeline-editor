using InfraFlowSculptor.Api.Errors;
using InfraFlowSculptor.Application.Imports.Common;
using InfraFlowSculptor.Application.Imports.Queries.PreviewIacImport;
using InfraFlowSculptor.Contracts.Imports.Requests;
using InfraFlowSculptor.Contracts.Imports.Responses;
using MediatR;

namespace InfraFlowSculptor.Api.Controllers;

/// <summary>
/// Exposes import preview endpoints.
/// </summary>
public static class ImportController
{
    /// <summary>
    /// Registers the import preview endpoints.
    /// </summary>
    /// <param name="builder">The application builder.</param>
    /// <returns>The application builder.</returns>
    public static IApplicationBuilder UseImportController(this IApplicationBuilder builder)
    {
        return builder.UseEndpoints(endpoints =>
        {
            var group = endpoints.MapGroup("/imports")
                .WithTags("Imports");

            group.MapPost("/preview",
                    async (PreviewIacImportRequest request, IMediator mediator) =>
                    {
                        var query = new PreviewIacImportQuery(request.SourceFormat, request.SourceContent);
                        var result = await mediator.Send(query);

                        return result.Match(
                            value => Results.Ok(ToResponse(value)),
                            errors => errors.Result());
                    })
                .WithName("PreviewIacImport")
                .Produces<PreviewIacImportResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status500InternalServerError);
        });
    }

    private static PreviewIacImportResponse ToResponse(ImportPreviewAnalysisResult analysis)
    {
        var mappedResources = analysis.Resources
            .Where(resource => resource.MappedResourceType is not null)
            .Select(resource => new PreviewIacImportMappedResourceResponseItem(
                resource.SourceType,
                resource.SourceName,
                resource.MappedResourceType!,
                resource.MappedName ?? resource.SourceName,
                resource.Confidence.ToString().ToLowerInvariant(),
                resource.ExtractedProperties,
                resource.UnmappedProperties))
            .ToList();

        var gaps = analysis.Gaps
            .Select(gap => new PreviewIacImportGapResponseItem(
                gap.Severity.ToString().ToLowerInvariant(),
                gap.Category,
                gap.Message,
                gap.SourceResourceName))
            .ToList();

        var dependencies = analysis.Dependencies
            .Select(dependency => new PreviewIacImportDependencyResponseItem(
                dependency.FromResourceName,
                dependency.ToResourceName,
                dependency.DependencyType))
            .ToList();

        return new PreviewIacImportResponse(
            analysis.SourceFormat,
            analysis.Resources.Count,
            mappedResources,
            gaps,
            analysis.UnsupportedResources,
            dependencies,
            analysis.Metadata,
            analysis.Summary);
    }
}