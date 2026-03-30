using ErrorOr;
using MediatR;

namespace InfraFlowSculptor.Application.Common.Interfaces;

/// <summary>
/// Marker interface for CQRS command handlers that process an <see cref="ICommand{TResult}"/>.
/// </summary>
/// <typeparam name="TCommand">The command type.</typeparam>
/// <typeparam name="TResult">The success result type.</typeparam>
public interface ICommandHandler<in TCommand, TResult>
    : IRequestHandler<TCommand, ErrorOr<TResult>>
    where TCommand : ICommand<TResult>;
