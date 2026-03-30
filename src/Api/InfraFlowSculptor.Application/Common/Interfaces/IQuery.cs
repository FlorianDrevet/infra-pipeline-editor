using ErrorOr;
using MediatR;

namespace InfraFlowSculptor.Application.Common.Interfaces;

/// <summary>
/// Marker interface for CQRS queries that return <see cref="ErrorOr{TResult}"/>.
/// Queries represent read-only operations and are not wrapped by the <c>UnitOfWorkBehavior</c>.
/// </summary>
/// <typeparam name="TResult">The success result type wrapped in <see cref="ErrorOr{TResult}"/>.</typeparam>
public interface IQuery<TResult> : IRequest<ErrorOr<TResult>>;
