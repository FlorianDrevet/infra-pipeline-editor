using InfraFlowSculptor.Application.Git.Commands.VerifyGitConnection;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Contracts.Git.Requests;
using InfraFlowSculptor.Contracts.Git.Responses;
using Mapster;

namespace InfraFlowSculptor.Api.Common.Mapping;

/// <summary>Mapster mapping configuration for stateless Git operations.</summary>
public sealed class GitMappingConfig : IRegister
{
    /// <inheritdoc />
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<VerifyGitConnectionRequest, VerifyGitConnectionCommand>();

        config.NewConfig<ProjectRepositoryConnectionVerificationResult, VerifyGitConnectionResponse>();

        config.NewConfig<GitBranchResult, VerifiedGitBranchDto>();
    }
}
