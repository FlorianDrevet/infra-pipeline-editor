using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.Application.Common.Interfaces.Services;

/// <summary>
/// Creates typed <see cref="AppPipelineGenerationRequest"/> instances for supported compute resources.
/// </summary>
public interface IAppPipelineRequestFactory
{
    /// <summary>
    /// Creates an application pipeline generation request for the specified resource when its type is supported.
    /// </summary>
    /// <param name="resourceId">The identifier of the compute resource to inspect.</param>
    /// <param name="resourceType">The Azure ARM resource type string.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>
    /// The matching <see cref="AppPipelineGenerationRequest"/>, or <c>null</c> when the resource type is unsupported
    /// or the resource cannot be loaded.
    /// </returns>
    Task<AppPipelineGenerationRequest?> CreateAsync(
        AzureResourceId resourceId,
        string resourceType,
        CancellationToken cancellationToken = default);
}
