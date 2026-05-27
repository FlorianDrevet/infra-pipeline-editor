using ErrorOr;

namespace InfraFlowSculptor.Application.Common.Interfaces;

/// <summary>
/// Marker interface for CQRS commands that trigger artifact generation and therefore require
/// the dedicated PAT <c>Generate</c> scope instead of the broader write scope.
/// </summary>
/// <typeparam name="TResult">The success result type wrapped in <see cref="ErrorOr{TResult}"/>.</typeparam>
public interface IGenerateCommand<TResult> : ICommand<TResult>;