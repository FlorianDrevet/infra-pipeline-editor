using BicepGenerator.Contracts.GenerateBicep.Requests;
using BicepGenerator.Contracts.GenerateBicep.Responses;
using Refit;

namespace InfraFlowSculptor.Application.Common.Clients;

[Headers("accept: application/json", "Authorization: Bearer")]
public interface IGenerateBicepClient
{
    [Post("/generate-bicep")]
    public Task<GenerateBicepResponse> GenerateBicepAsync([Body] GenerateBicepRequest request, CancellationToken cancellationToken = default);
}