using InfraFlowSculptor.Application.InfrastructureConfig.Commands.GenerateBicep;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.Application.InfrastructureConfig.Diagnostics;
using InfraFlowSculptor.Application.InfrastructureConfig.Queries.CheckResourceNameAvailability;
using InfraFlowSculptor.Application.InfrastructureConfig.Queries.GetConfigDiagnostics;
using InfraFlowSculptor.Application.InfrastructureConfig.Queries.ListCrossConfigReferences;
using InfraFlowSculptor.Application.InfrastructureConfig.Queries.ListIncomingCrossConfigReferences;
using InfraFlowSculptor.Application.Projects.Queries.ListProjectResources;
using InfraFlowSculptor.Contracts.InfrastructureConfig.Responses;
using InfraFlowSculptor.Contracts.Projects.Responses;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.Entities;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using Mapster;

namespace InfraFlowSculptor.Api.Common.Mapping;

public sealed class InfraConfigMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<InfrastructureConfigId, Guid>()
            .MapWith(src => src.Value);

        config.NewConfig<InfrastructureConfigId, string>()
            .MapWith(src => src.Value.ToString());

        config.NewConfig<Guid, InfrastructureConfigId>()
            .MapWith(src => InfrastructureConfigId.Create(src));

        config.NewConfig<UserId, Guid>()
            .MapWith(src => src.Value);

        config.NewConfig<UserId, string>()
            .MapWith(src => src.Value.ToString());

        config.NewConfig<ResourceNamingTemplateId, Guid>()
            .MapWith(src => src.Value);

        config.NewConfig<ResourceNamingTemplateId, string>()
            .MapWith(src => src.Value.ToString());

        // Tag -> TagResult
        config.NewConfig<Tag, TagResult>()
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Value, src => src.Value);

        // TagResult -> TagResponse
        config.NewConfig<TagResult, TagResponse>()
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Value, src => src.Value);

        // ResourceNamingTemplate entity -> ResourceNamingTemplateResult
        config.NewConfig<ResourceNamingTemplate, ResourceNamingTemplateResult>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.ResourceType, src => src.ResourceType)
            .Map(dest => dest.Template, src => src.Template.Value);

        // ResourceNamingTemplateResult -> ResourceNamingTemplateResponse
        config.NewConfig<ResourceNamingTemplateResult, ResourceNamingTemplateResponse>()
            .Map(dest => dest.Id, src => src.Id.Value.ToString())
            .Map(dest => dest.ResourceType, src => src.ResourceType)
            .Map(dest => dest.Template, src => src.Template);

        // ResourceAbbreviationOverride entity -> ResourceAbbreviationOverrideResult
        config.NewConfig<ResourceAbbreviationOverride, ResourceAbbreviationOverrideResult>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.ResourceType, src => src.ResourceType)
            .Map(dest => dest.Abbreviation, src => src.Abbreviation);

        // ResourceAbbreviationOverrideResult -> ResourceAbbreviationOverrideResponse
        config.NewConfig<ResourceAbbreviationOverrideResult, ResourceAbbreviationOverrideResponse>()
            .Map(dest => dest.Id, src => src.Id.Value.ToString())
            .Map(dest => dest.ResourceType, src => src.ResourceType)
            .Map(dest => dest.Abbreviation, src => src.Abbreviation);

        // GetInfrastructureConfigResult -> InfrastructureConfigResponse
        config.NewConfig<GetInfrastructureConfigResult, InfrastructureConfigResponse>()
            .Map(dest => dest.Id, src => src.Id.Value.ToString())
            .Map(dest => dest.Name, src => src.Name.Value)
            .Map(dest => dest.ProjectId, src => src.ProjectId.Value.ToString())
            .Map(dest => dest.DefaultNamingTemplate, src => src.DefaultNamingTemplate)
            .Map(dest => dest.UseProjectNamingConventions, src => src.UseProjectNamingConventions)
            .Map(dest => dest.ResourceNamingTemplates, src => src.ResourceNamingTemplates)
            .Map(dest => dest.ResourceAbbreviationOverrides, src => src.ResourceAbbreviationOverrides)
            .Map(dest => dest.Tags, src => src.Tags)
            .Map(dest => dest.ResourceGroupCount, src => src.ResourceGroupCount)
            .Map(dest => dest.ResourceCount, src => src.ResourceCount)
            .Map(dest => dest.CrossConfigReferenceCount, src => src.CrossConfigReferenceCount)
            .Map(dest => dest.RepositoryBinding,
                src => src.RepositoryBinding != null
                    ? new RepositoryBindingResponse(
                        src.RepositoryBinding.Alias,
                        src.RepositoryBinding.Branch,
                        src.RepositoryBinding.InfraPath,
                        src.RepositoryBinding.PipelinePath)
                    : null);

        // InfrastructureConfig domain -> GetInfrastructureConfigResult
        config.NewConfig<Domain.InfrastructureConfigAggregate.InfrastructureConfig, GetInfrastructureConfigResult>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.ProjectId, src => src.ProjectId)
            .Map(dest => dest.DefaultNamingTemplate, src => src.DefaultNamingTemplate != null ? src.DefaultNamingTemplate.Value : null)
            .Map(dest => dest.UseProjectNamingConventions, src => src.UseProjectNamingConventions)
            .Map(dest => dest.ResourceNamingTemplates, src => src.ResourceNamingTemplates)
            .Map(dest => dest.ResourceAbbreviationOverrides, src => src.ResourceAbbreviationOverrides)
            .Map(dest => dest.Tags, src => src.Tags)
            .Map(dest => dest.CrossConfigReferenceCount, src => src.CrossConfigReferences.Count)
            .Map(dest => dest.RepositoryBinding,
                src => src.RepositoryBinding != null
                    ? new RepositoryBindingResult(
                        src.RepositoryBinding.Alias.Value,
                        src.RepositoryBinding.Branch,
                        src.RepositoryBinding.InfraPath,
                        src.RepositoryBinding.PipelinePath)
                    : null);

        // User entity -> UserResult
        config.NewConfig<User, UserResult>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.FirstName, src => src.Name.FirstName)
            .Map(dest => dest.LastName, src => src.Name.LastName);

        // UserResult -> UserResponse
        config.NewConfig<UserResult, UserResponse>()
            .Map(dest => dest.Id, src => src.Id.Value.ToString())
            .Map(dest => dest.FirstName, src => src.FirstName)
            .Map(dest => dest.LastName, src => src.LastName);

        // CrossConfigReferenceDetailResult -> CrossConfigReferenceResponse
        config.NewConfig<CrossConfigReferenceDetailResult, CrossConfigReferenceResponse>()
            .Map(dest => dest.ReferenceId, src => src.ReferenceId.ToString())
            .Map(dest => dest.TargetConfigId, src => src.TargetConfigId.ToString())
            .Map(dest => dest.TargetResourceId, src => src.TargetResourceId.ToString());

        // IncomingCrossConfigReferenceResult -> IncomingCrossConfigReferenceResponse
        config.NewConfig<IncomingCrossConfigReferenceResult, IncomingCrossConfigReferenceResponse>()
            .Map(dest => dest.ReferenceId, src => src.ReferenceId.ToString())
            .Map(dest => dest.SourceConfigId, src => src.SourceConfigId.ToString())
            .Map(dest => dest.SourceResourceId, src => src.SourceResourceId.ToString())
            .Map(dest => dest.TargetResourceId, src => src.TargetResourceId.ToString());

        // ProjectResourceResult -> ProjectResourceResponse
        config.NewConfig<ProjectResourceResult, ProjectResourceResponse>()
            .Map(dest => dest.ResourceId, src => src.ResourceId.ToString())
            .Map(dest => dest.ConfigId, src => src.ConfigId.ToString());

        // ResourceDiagnosticItem -> ResourceDiagnosticResponse
        config.NewConfig<ResourceDiagnosticItem, ResourceDiagnosticResponse>()
            .Map(dest => dest.ResourceId, src => src.ResourceId.ToString())
            .Map(dest => dest.Severity, src => src.Severity.ToString());

        // GetConfigDiagnosticsResult -> ConfigDiagnosticsResponse
        config.NewConfig<GetConfigDiagnosticsResult, ConfigDiagnosticsResponse>();

        // GenerateBicepResult -> GenerateBicepResponse
        config.NewConfig<GenerateBicepResult, GenerateBicepResponse>();

        // CheckResourceNameAvailability mappings
        config.NewConfig<EnvironmentNameAvailabilityResult, EnvironmentNameAvailabilityResponseItem>();
        config.NewConfig<CheckResourceNameAvailabilityResult, CheckResourceNameAvailabilityResponse>();
    }
}
