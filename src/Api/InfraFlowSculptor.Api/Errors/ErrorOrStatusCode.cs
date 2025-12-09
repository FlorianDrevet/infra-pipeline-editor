using ErrorOr;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace InfraFlowSculptor.Api.Errors;

public static class ErrorOrExtended
{
    public static int GetStatusCode(this Error errorOr)
    {
        return errorOr.Type switch
        {
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };
    }

    public static IResult Result(this Error errorOr)
    {
        return Results.Problem(
            statusCode: errorOr.GetStatusCode(),
            detail: errorOr.Description
        );
    }

    public static IResult Result(this List<Error> errors)
    {
        if (errors.All(error => error.Type == ErrorType.Validation))
        {
            var modelState = new ModelStateDictionary();

            foreach (var error in errors)
            {
                modelState.AddModelError(error.Code, error.Description);
            }

            return Results.ValidationProblem(
                modelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                )
            );
        }
        else
        {
            return errors.First().Result();
        }
    }
}