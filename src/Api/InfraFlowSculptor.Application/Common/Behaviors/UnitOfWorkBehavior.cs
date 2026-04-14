using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using MediatR;

namespace InfraFlowSculptor.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior that wraps command handlers with a Unit of Work.
/// <para>
/// After a handler returns a successful <see cref="ErrorOr{T}"/> result, this behavior
/// calls <see cref="IUnitOfWork.SaveChangesAsync"/> once to persist all tracked changes
/// in a single atomic batch. If the handler returns errors or throws an exception,
/// no changes are persisted (implicit rollback).
/// </para>
/// <para>
/// This behavior only applies to <see cref="ICommand{TResult}"/> requests.
/// Queries (<see cref="IQuery{TResult}"/>) are not affected.
/// </para>
/// </summary>
public sealed class UnitOfWorkBehavior<TRequest, TResponse>(IUnitOfWork unitOfWork)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommandBase, IRequest<TResponse>
    where TResponse : IErrorOr
{
    /// <inheritdoc />
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var response = await next(cancellationToken);

        if (response.IsError)
            return response;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return response;
    }
}
