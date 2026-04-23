using ErrorOr;
using FluentValidation;
using MediatR;

namespace InfraFlowSculptor.Application.Common.Behaviors;

/// <summary>
/// Pipeline behavior that runs FluentValidation against the incoming request and short-circuits
/// the pipeline with an <see cref="ErrorOr{TValue}"/> validation failure when the request is invalid.
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse>(IValidator<TRequest>? validator = null) :
    IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IErrorOr
{
    /// <inheritdoc />
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (validator is null)
        {
            return await next();
        }

        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (validationResult.IsValid)
        {
            return await next();
        }

        var errors = validationResult.Errors
            .ConvertAll(validationError => Error.Validation(
                validationError.PropertyName,
                validationError.ErrorMessage));

        return BuildErrorResponse(errors);
    }

    /// <summary>
    /// Constructs the typed <typeparamref name="TResponse"/> (which is always an
    /// <c>ErrorOr&lt;TValue&gt;</c>) from a list of validation errors.
    /// Replaces the legacy <c>(dynamic)errors</c> implicit conversion which lost type
    /// safety and could throw <see cref="Microsoft.CSharp.RuntimeBinder.RuntimeBinderException"/>
    /// when the implicit conversion was unavailable. (Audit APP-004 — 2026-04-23.)
    /// </summary>
    private static TResponse BuildErrorResponse(List<Error> errors)
    {
        var responseType = typeof(TResponse);

        if (!responseType.IsGenericType || responseType.GetGenericTypeDefinition() != typeof(ErrorOr<>))
        {
            throw new InvalidOperationException(
                $"ValidationBehavior expects TResponse to be ErrorOr<T> but got {responseType.FullName}.");
        }

        // ErrorOr<T> exposes an implicit conversion from List<Error>; invoke it via reflection.
        var implicitOp = responseType.GetMethod(
            "op_Implicit",
            [typeof(List<Error>)]);

        if (implicitOp is null)
        {
            throw new InvalidOperationException(
                $"ErrorOr<T> implicit conversion from List<Error> not found on {responseType.FullName}.");
        }

        return (TResponse)implicitOp.Invoke(null, [errors])!;
    }
}