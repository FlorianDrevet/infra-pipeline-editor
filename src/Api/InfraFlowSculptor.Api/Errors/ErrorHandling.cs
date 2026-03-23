using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace InfraFlowSculptor.Api.Errors;

public static class ErrorHandling
{
    public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder builder)
    {
        return builder.UseExceptionHandler(exceptionHandlerApp
            => exceptionHandlerApp.Run(async context
                    =>
                {
                    await Results.Problem(
                            statusCode: StatusCodes.Status500InternalServerError,
                            detail: "An error occurred."
                        )
                        .ExecuteAsync(context);
                }
            )
        );
    }
}
