using ErrorOr;
using MediatR;

namespace InfraFlowSculptor.Application.Common.Interfaces;

/// <summary>
/// Non-generic marker used by <c>UnitOfWorkBehavior</c> to identify command requests
/// at the generic constraint level (avoids the <c>ErrorOr</c> type mismatch in
/// <c>IPipelineBehavior&lt;TRequest, TResponse&gt;</c>).
/// </summary>
public interface ICommandBase;

/// <summary>
/// Marker interface for CQRS commands that return <see cref="ErrorOr{TResult}"/>.
/// Commands represent write operations and are handled by the <c>UnitOfWorkBehavior</c>.
/// </summary>
/// <typeparam name="TResult">The success result type wrapped in <see cref="ErrorOr{TResult}"/>.</typeparam>
public interface ICommand<TResult> : IRequest<ErrorOr<TResult>>, ICommandBase;
