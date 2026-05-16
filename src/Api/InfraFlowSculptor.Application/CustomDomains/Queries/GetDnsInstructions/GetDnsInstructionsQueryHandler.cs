using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.GenerationCore;

namespace InfraFlowSculptor.Application.CustomDomains.Queries.GetDnsInstructions;

/// <summary>Handles the <see cref="GetDnsInstructionsQuery"/> request.</summary>
public sealed class GetDnsInstructionsQueryHandler(
    IAzureResourceRepository azureResourceRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService)
    : IQueryHandler<GetDnsInstructionsQuery, DnsInstructionsResult>
{
    private const string CnameRecordType = "CNAME";
    private const string TxtRecordType = "TXT";
    private const string ValidateDnsTitle = "Validate DNS";
    private const string AzureWebsitesSuffix = ".azurewebsites.net";
    private const string AsuidPrefix = "asuid.";

    /// <inheritdoc />
    public async Task<ErrorOr<DnsInstructionsResult>> Handle(
        GetDnsInstructionsQuery request,
        CancellationToken cancellationToken)
    {
        var resource = await azureResourceRepository.GetByIdWithCustomDomainsAsync(
            request.ResourceId, cancellationToken);

        if (resource is null)
            return Errors.CustomDomain.ResourceNotFound(request.ResourceId);

        var resourceGroup = await resourceGroupRepository.GetByIdReadOnlyAsync(
            resource.ResourceGroupId, cancellationToken);

        if (resourceGroup is null)
            return Errors.ResourceGroup.NotFound(resource.ResourceGroupId);

        var authResult = await accessService.VerifyReadAccessAsync(
            resourceGroup.InfraConfigId, cancellationToken);

        if (authResult.IsError)
            return authResult.Errors;

        var domain = resource.CustomDomains.FirstOrDefault(cd => cd.Id == request.CustomDomainId);

        if (domain is null)
            return Errors.CustomDomain.NotFound(request.CustomDomainId);

        var steps = BuildDnsInstructions(resource.ResourceType, resource.Name, domain.DomainName);

        return new DnsInstructionsResult(
            domain.DomainName,
            domain.DnsValidationStatus.Value.ToString(),
            steps);
    }

    private static IReadOnlyList<DnsInstructionStep> BuildDnsInstructions(
        string resourceType,
        string resourceName,
        string domainName)
    {
        return resourceType switch
        {
            AzureResourceTypes.ContainerApp => BuildContainerAppInstructions(resourceName, domainName),
            AzureResourceTypes.WebApp => BuildWebAppInstructions(resourceName, domainName),
            AzureResourceTypes.FunctionApp => BuildFunctionAppInstructions(resourceName, domainName),
            _ => BuildGenericInstructions(domainName),
        };
    }

    private static List<DnsInstructionStep> BuildContainerAppInstructions(string resourceName, string domainName)
    {
        return
        [
            new DnsInstructionStep(
                Order: 1,
                Title: "Create a CNAME record",
                Description: $"Deploy the infrastructure first (without the custom domain binding — it will be added after DNS validation). "
                             + $"Once deployed, open Container App '{resourceName}' in the Azure portal, go to Networking > Custom domains. "
                             + "At the top of the page, note the 'IP address' field (you can also use it for an A record). "
                             + $"For a CNAME, go to Overview and copy the Application URL (without https://) as the CNAME target for '{domainName}'.",
                RecordType: CnameRecordType,
                RecordName: domainName,
                RecordValue: "<application-url-from-container-app-overview>"),
            new DnsInstructionStep(
                Order: 2,
                Title: "Create a TXT verification record",
                Description: $"On the same Custom domains page (Container App '{resourceName}' > Networking > Custom domains), "
                             + "copy the 'Custom Domain Verification ID' displayed at the top of the page. "
                             + $"Create a TXT record at '{AsuidPrefix}{domainName}' with that value.",
                RecordType: TxtRecordType,
                RecordName: $"{AsuidPrefix}{domainName}",
                RecordValue: "<custom-domain-verification-id-from-container-app>"),
            new DnsInstructionStep(
                Order: 3,
                Title: ValidateDnsTitle,
                Description: $"After DNS propagation, return here and click 'Validate DNS'. "
                             + $"Once validated, the next infrastructure deployment will create the custom domain binding with a managed certificate on Container App '{resourceName}'.",
                RecordType: null,
                RecordName: null,
                RecordValue: null),
        ];
    }

    private static List<DnsInstructionStep> BuildWebAppInstructions(string resourceName, string domainName)
    {
        var defaultHostname = $"{resourceName}{AzureWebsitesSuffix}";

        return
        [
            new DnsInstructionStep(
                Order: 1,
                Title: "Create a CNAME record",
                Description: $"Point '{domainName}' to '{defaultHostname}'.",
                RecordType: CnameRecordType,
                RecordName: domainName,
                RecordValue: defaultHostname),
            new DnsInstructionStep(
                Order: 2,
                Title: "Create a TXT verification record",
                Description: $"Create a TXT record at '{AsuidPrefix}{domainName}' with the Custom Domain Verification ID from the Azure portal.",
                RecordType: TxtRecordType,
                RecordName: $"{AsuidPrefix}{domainName}",
                RecordValue: "<custom-domain-verification-id>"),
            new DnsInstructionStep(
                Order: 3,
                Title: ValidateDnsTitle,
                Description: "Once DNS records have propagated, click 'Validate DNS' to confirm the configuration.",
                RecordType: null,
                RecordName: null,
                RecordValue: null),
        ];
    }

    private static List<DnsInstructionStep> BuildFunctionAppInstructions(string resourceName, string domainName)
    {
        var defaultHostname = $"{resourceName}{AzureWebsitesSuffix}";

        return
        [
            new DnsInstructionStep(
                Order: 1,
                Title: "Create a CNAME record",
                Description: $"Point '{domainName}' to '{defaultHostname}'.",
                RecordType: CnameRecordType,
                RecordName: domainName,
                RecordValue: defaultHostname),
            new DnsInstructionStep(
                Order: 2,
                Title: "Create a TXT verification record",
                Description: $"Create a TXT record at '{AsuidPrefix}{domainName}' with the Custom Domain Verification ID from the Azure portal.",
                RecordType: TxtRecordType,
                RecordName: $"{AsuidPrefix}{domainName}",
                RecordValue: "<custom-domain-verification-id>"),
            new DnsInstructionStep(
                Order: 3,
                Title: ValidateDnsTitle,
                Description: "Once DNS records have propagated, click 'Validate DNS' to confirm the configuration.",
                RecordType: null,
                RecordName: null,
                RecordValue: null),
        ];
    }

    private static List<DnsInstructionStep> BuildGenericInstructions(string domainName)
    {
        return
        [
            new DnsInstructionStep(
                Order: 1,
                Title: "Configure DNS records",
                Description: $"Configure the appropriate DNS records for '{domainName}' according to your Azure resource documentation.",
                RecordType: null,
                RecordName: null,
                RecordValue: null),
            new DnsInstructionStep(
                Order: 2,
                Title: ValidateDnsTitle,
                Description: "Once DNS records have propagated, click 'Validate DNS' to confirm the configuration.",
                RecordType: null,
                RecordName: null,
                RecordValue: null),
        ];
    }
}
