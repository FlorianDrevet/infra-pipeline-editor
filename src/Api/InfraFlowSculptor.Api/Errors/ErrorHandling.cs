using Microsoft.AspNetCore.Diagnostics;

namespace InfraFlowSculptor.Api.Errors;

public static class ErrorHandling
{
    public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder builder)
    {
        return builder.UseExceptionHandler(exceptionHandlerApp
            => exceptionHandlerApp.Run(async context
                    =>
                {
                    var (statusCode, message) = context.Features.Get<IExceptionHandlerFeature>()?.Error switch
                    {
                        _ => (StatusCodes.Status500InternalServerError, "An error occurred.")
                    };
                    await Results.Problem(
                            statusCode: statusCode,
                            detail: message
                        )
                        .ExecuteAsync(context);
                }
            )
        );
    }
}