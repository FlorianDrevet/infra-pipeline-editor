using InfraFlowSculptor.Application.Common.Interfaces.DomainEvents;
using InfraFlowSculptor.Domain.Common.Models;
using Microsoft.Extensions.DependencyInjection;

namespace InfraFlowSculptor.Infrastructure.DomainEvents;

/// <summary>
/// Dispatches domain events to all registered in-process handlers for their concrete type.
/// </summary>
public sealed class DomainEventDispatcher(IServiceProvider serviceProvider) : IDomainEventDispatcher
{
    /// <inheritdoc />
    public async Task DispatchAsync(IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(domainEvents);

        foreach (var domainEvent in domainEvents)
        {
            await DispatchAsync(domainEvent, cancellationToken);
        }
    }

    private async Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(domainEvent.GetType());
        var handleMethod = handlerType.GetMethod(nameof(IDomainEventHandler<IDomainEvent>.HandleAsync))
            ?? throw new InvalidOperationException($"Handler method {nameof(IDomainEventHandler<IDomainEvent>.HandleAsync)} was not found on {handlerType.Name}.");

        foreach (var handler in serviceProvider.GetServices(handlerType))
        {
            if (handler is null)
            {
                continue;
            }

            var task = handleMethod.Invoke(handler, [domainEvent, cancellationToken]) as Task;
            if (task is null)
            {
                throw new InvalidOperationException($"Handler {handler.GetType().Name} did not return a {nameof(Task)}.");
            }

            await task;
        }
    }
}