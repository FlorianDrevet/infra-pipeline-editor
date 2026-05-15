using ErrorOr;
using InfraFlowSculptor.Application.AppConfigurations.Common;
using InfraFlowSculptor.Domain.AppConfigurationAggregate;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;

namespace InfraFlowSculptor.Application.AppConfigurations.Commands.AddAppConfigurationKey;

/// <summary>
/// Adds a configuration key to an App Configuration resource after the handler has completed access and duplicate checks.
/// </summary>
public interface IAddAppConfigurationKeyAdditionService
{
    /// <summary>
    /// Adds the requested configuration key to the provided App Configuration resource.
    /// </summary>
    /// <param name="request">The add-app-configuration-key command being executed.</param>
    /// <param name="appConfiguration">The loaded App Configuration resource that will receive the key.</param>
    /// <param name="infraConfig">The infrastructure configuration authorized for the operation.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The created <see cref="AppConfigurationKeyResult"/>, or validation errors when the requested reference is invalid.</returns>
    Task<ErrorOr<AppConfigurationKeyResult>> AddAsync(
        AddAppConfigurationKeyCommand request,
        AppConfiguration appConfiguration,
        DomainInfrastructureConfig infraConfig,
        CancellationToken cancellationToken);
}