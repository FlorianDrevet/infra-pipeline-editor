using InfraFlowSculptor.GenerationCore.Models;
using InfraFlowSculptor.PipelineGeneration.Generators;

namespace InfraFlowSculptor.PipelineGeneration.Tests.TestDoubles;

/// <summary>
/// Minimal stub <see cref="IAppPipelineGenerator"/> used to verify dispatch
/// behaviour in <see cref="AppPipelineGenerationEngine"/> tests.
/// </summary>
internal sealed class StubAppPipelineGenerator : IAppPipelineGenerator
{
    private readonly AppPipelineGenerationResult _result;

    public StubAppPipelineGenerator(string resourceType, string deploymentMode, AppPipelineGenerationResult? result = null)
    {
        ResourceType = resourceType;
        DeploymentMode = deploymentMode;
        _result = result ?? new AppPipelineGenerationResult
        {
            Files = new Dictionary<string, string> { ["stub.yml"] = "stub" },
        };
    }

    public string ResourceType { get; }

    public string DeploymentMode { get; }

    public int InvocationCount { get; private set; }

    public AppPipelineGenerationRequest? LastRequest { get; private set; }

    public AppPipelineGenerationResult Generate(AppPipelineGenerationRequest request)
    {
        InvocationCount++;
        LastRequest = request;
        return _result;
    }
}
