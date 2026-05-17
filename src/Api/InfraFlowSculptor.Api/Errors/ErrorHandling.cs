using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;

namespace InfraFlowSculptor.Api.Errors;

/// <summary>
/// Registers the global exception handling middleware for the API.
/// </summary>
public static class ErrorHandling
{
    private const string GenericErrorDetail = "An error occurred.";
    private const string TraceIdExtensionName = "traceId";

    /// <summary>
    /// Uses the global exception handler that returns a generic RFC 7807 response enriched with a trace identifier.
    /// </summary>
    /// <param name="builder">The application builder.</param>
    /// <returns>The updated application builder.</returns>
    public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder builder)
    {
        return builder.UseExceptionHandler(exceptionHandlerApp =>
        {
            var logger = exceptionHandlerApp.ApplicationServices
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger("InfraFlowSculptor.Api.ErrorHandling");

            exceptionHandlerApp.Run(async context =>
            {
                var traceId = Activity.Current?.Id ?? context.TraceIdentifier;
                var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;

                logger.LogError(exception, "Unhandled exception {TraceId}", traceId);

                await Results.Problem(
                        statusCode: StatusCodes.Status500InternalServerError,
                        detail: GenericErrorDetail,
                        extensions: new Dictionary<string, object?>
                        {
                            [TraceIdExtensionName] = traceId,
                        }
                    )
                    .ExecuteAsync(context);
            });
        });
    }
}
