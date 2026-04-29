using InfraFlowSculptor.Api.Errors;
using InfraFlowSculptor.Application.Imports.Commands.ApplyImportPreview;
using InfraFlowSculptor.Application.Imports.Common.Analysis;
using InfraFlowSculptor.Application.Imports.Common.Constants;
using InfraFlowSculptor.Application.Imports.Common.Creation;
using InfraFlowSculptor.Application.Imports.Queries.PreviewIacImport;
using InfraFlowSculptor.Application.Projects.Commands.CreateProjectWithSetup;
using InfraFlowSculptor.Contracts.Imports.Requests;
using InfraFlowSculptor.Contracts.Imports.Responses;
using MediatR;

namespace InfraFlowSculptor.Api.Controllers;

/// <summary>
/// Exposes import endpoints.
/// </summary>
public static class ImportController
{
    /// <summary>
    /// Registers the import endpoints.
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

            group.MapPost("/apply",
                    async (ApplyImportPreviewRequest request, IMediator mediator) =>
                    {
                        var command = new ApplyImportPreviewCommand(
                            request.ProjectName,
                            request.LayoutPreset,
                            ToAnalysis(request.Preview),
                            request.Environments?.Select(environment => new EnvironmentSetupItem(
                                environment.Name,
                                environment.ShortName,
                                environment.Prefix ?? string.Empty,
                                environment.Suffix ?? string.Empty,
                                environment.Location,
                                environment.SubscriptionId,
                                environment.Order,
                                environment.RequiresApproval)).ToList(),
                            request.ResourceFilter);
                        var result = await mediator.Send(command);

                        return result.Match(
                            value => Results.Ok(ToResponse(value)),
                            errors => errors.Result());
                    })
                .WithName("ApplyImportPreview")
                .Produces<ApplyImportPreviewResponse>(StatusCodes.Status200OK)
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

    private static ApplyImportPreviewResponse ToResponse(ApplyImportPreviewResult result)
    {
        return new ApplyImportPreviewResponse(
            result.Status,
            result.ProjectId,
            result.ProjectName,
            result.InfrastructureConfigId,
            result.ResourceGroupId,
            result.InfrastructureError,
            result.CreatedResources.Select(resource => new ApplyImportPreviewCreatedResourceResponseItem(
                resource.ResourceType,
                resource.ResourceId,
                resource.Name)).ToList(),
            result.SkippedResources.Select(resource => new ApplyImportPreviewSkippedResourceResponseItem(
                resource.ResourceType,
                resource.Name,
                resource.Reason)).ToList(),
            result.NextSuggestedActions.ToList());
    }

    private static ImportPreviewAnalysisResult ToAnalysis(ImportPreviewPayloadRequest preview)
    {
        return new ImportPreviewAnalysisResult
        {
            SourceFormat = preview.SourceFormat,
            Resources = preview.Resources.Select(resource => new ImportedResourceAnalysisResult
            {
                SourceType = resource.SourceType,
                SourceName = resource.SourceName,
                MappedResourceType = resource.MappedResourceType,
                MappedName = resource.MappedName,
                Confidence = Enum.TryParse<ImportPreviewMappingConfidence>(resource.Confidence, true, out var confidence)
                    ? confidence
                    : ImportPreviewMappingConfidence.Low,
                ExtractedProperties = new Dictionary<string, object?>(resource.ExtractedProperties),
                UnmappedProperties = resource.UnmappedProperties.ToList(),
            }).ToList(),
            Dependencies = preview.Dependencies.Select(dependency => new ImportedDependencyAnalysisResult(
                dependency.FromResourceName,
                dependency.ToResourceName,
                dependency.DependencyType)).ToList(),
            Metadata = new Dictionary<string, string>(preview.Metadata),
            Gaps = preview.Gaps.Select(gap => new ImportPreviewGapResult
            {
                Severity = Enum.TryParse<ImportPreviewGapSeverity>(gap.Severity, true, out var severity)
                    ? severity
                    : ImportPreviewGapSeverity.Warning,
                Category = gap.Category,
                Message = gap.Message,
                SourceResourceName = gap.SourceResourceName,
            }).ToList(),
            UnsupportedResources = preview.UnsupportedResources.ToList(),
            Summary = preview.Summary,
        };
    }
}