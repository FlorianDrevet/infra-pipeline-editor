using Refit;

namespace InfraFlowSculptor.Application.Common.Clients;

[Headers("accept: application/json", "Authorization: Bearer")]
public interface IGenerateBicepClient
{
    [Post("/api/generate-bicep")]
    public Task<Uri> GenerateBicepAsync(CancellationToken cancellationToken = default);
}