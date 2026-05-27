using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.PersonalAccessTokenAggregate.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior that enforces personal access token scopes consistently across
/// queries, write commands, and generation commands.
/// </summary>
public sealed class PersonalAccessTokenScopeBehavior<TRequest, TResponse>(ICurrentUser currentUser)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IErrorOr
{
    /// <inheritdoc />
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requiredScope = ResolveRequiredScope(request);

        if (requiredScope is null || await HasRequiredScopeAsync(requiredScope.Value, cancellationToken))
        {
            return await next(cancellationToken);
        }

        return BuildErrorResponse(Errors.PersonalAccessToken.MissingScope(requiredScope.Value));
    }

    private async Task<bool> HasRequiredScopeAsync(PatScopeType requiredScope, CancellationToken cancellationToken)
    {
        return requiredScope switch
        {
            PatScopeType.Read =>
                await currentUser.HasPersonalAccessTokenScopeAsync(PatScopeType.Read, cancellationToken) ||
                await currentUser.HasPersonalAccessTokenScopeAsync(PatScopeType.Write, cancellationToken),
            PatScopeType.Write => await currentUser.HasPersonalAccessTokenScopeAsync(PatScopeType.Write, cancellationToken),
            PatScopeType.Generate => await currentUser.HasPersonalAccessTokenScopeAsync(PatScopeType.Generate, cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(requiredScope), requiredScope, "Unsupported PAT scope."),
        };
    }

    private static PatScopeType? ResolveRequiredScope(TRequest request)
    {
        return request switch
        {
            _ when ImplementsOpenGenericInterface(request, typeof(IGenerateCommand<>)) => PatScopeType.Generate,
            ICommandBase => PatScopeType.Write,
            _ when ImplementsOpenGenericInterface(request, typeof(IQuery<>)) => PatScopeType.Read,
            _ => null,
        };
    }

    private static bool ImplementsOpenGenericInterface(TRequest request, Type openGenericInterfaceType)
    {
        return request.GetType().GetInterfaces().Any(interfaceType =>
            interfaceType.IsGenericType &&
            interfaceType.GetGenericTypeDefinition() == openGenericInterfaceType);
    }

    private static TResponse BuildErrorResponse(Error error)
    {
        var responseType = typeof(TResponse);

        if (!responseType.IsGenericType || responseType.GetGenericTypeDefinition() != typeof(ErrorOr<>))
        {
            throw new InvalidOperationException(
                $"PersonalAccessTokenScopeBehavior expects TResponse to be ErrorOr<T> but got {responseType.FullName}.");
        }

        var errorImplicitOperator = responseType.GetMethod(
            "op_Implicit",
            [typeof(Error)]);

        if (errorImplicitOperator is not null)
        {
            return (TResponse)errorImplicitOperator.Invoke(null, [error])!;
        }

        var listImplicitOperator = responseType.GetMethod(
            "op_Implicit",
            [typeof(List<Error>)]);

        if (listImplicitOperator is null)
        {
            throw new InvalidOperationException(
                $"ErrorOr<T> implicit conversion from Error or List<Error> not found on {responseType.FullName}.");
        }

        return (TResponse)listImplicitOperator.Invoke(null, [new List<Error> { error }])!;
    }
}