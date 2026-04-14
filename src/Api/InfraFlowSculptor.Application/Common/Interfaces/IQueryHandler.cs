using ErrorOr;
using MediatR;

namespace InfraFlowSculptor.Application.Common.Interfaces;

/// <summary>
/// Marker interface for CQRS query handlers that process an <see cref="IQuery{TResult}"/>.
/// </summary>
/// <typeparam name="TQuery">The query type.</typeparam>
/// <typeparam name="TResult">The success result type.</typeparam>
public interface IQueryHandler<in TQuery, TResult>
    : IRequestHandler<TQuery, ErrorOr<TResult>>
    where TQuery : IQuery<TResult>;
