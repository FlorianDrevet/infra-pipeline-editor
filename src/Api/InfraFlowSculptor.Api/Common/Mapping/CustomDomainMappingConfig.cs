using InfraFlowSculptor.Application.CustomDomains.Common;
using InfraFlowSculptor.Application.CustomDomains.Queries.GetDnsInstructions;
using InfraFlowSculptor.Contracts.CustomDomains.Responses;
using Mapster;

namespace InfraFlowSculptor.Api.Common.Mapping;

/// <summary>Mapster mapping configuration for the custom domain feature.</summary>
public sealed class CustomDomainMappingConfig : IRegister
{
    /// <inheritdoc />
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CustomDomainResult, CustomDomainResponse>()
            .MapWith(src => new CustomDomainResponse(
                src.Id.Value.ToString(),
                src.ResourceId.Value.ToString(),
                src.EnvironmentName,
                src.DomainName,
                src.CertificateMode,
                src.KeyVaultUrl,
                src.ManagedIdentityResourceId,
                src.CertificateName,
                src.DnsValidationStatus));

        config.NewConfig<DnsInstructionsResult, DnsInstructionsResponse>()
            .MapWith(src => new DnsInstructionsResponse(
                src.DomainName,
                src.DnsValidationStatus,
                src.Steps.Select(s => new DnsInstructionStepResponse(
                    s.Order,
                    s.Title,
                    s.Description,
                    s.RecordType,
                    s.RecordName,
                    s.RecordValue)).ToList()));
    }
}
