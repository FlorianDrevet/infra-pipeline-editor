using ErrorOr;
using InfraFlowSculptor.Application.AppSettings.Common;
using InfraFlowSculptor.Domain.Common.BaseModels;
using DomainInfrastructureConfig = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.InfrastructureConfig;

namespace InfraFlowSculptor.Application.AppSettings.Commands.AddAppSetting;

/// <summary>
/// Adds an app setting to a supported compute resource after the handler has completed access and duplicate checks.
/// </summary>
public interface IAddAppSettingAdditionService
{
    /// <summary>
    /// Adds the requested app setting to the provided resource.
    /// </summary>
    /// <param name="request">The add-app-setting command being executed.</param>
    /// <param name="resource">The loaded compute resource that will receive the app setting.</param>
    /// <param name="infraConfig">The infrastructure configuration authorized for the operation.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The created <see cref="AppSettingResult"/>, or validation errors when the requested reference is invalid.</returns>
    Task<ErrorOr<AppSettingResult>> AddAsync(
        AddAppSettingCommand request,
        AzureResource resource,
        DomainInfrastructureConfig infraConfig,
        CancellationToken cancellationToken);
}
