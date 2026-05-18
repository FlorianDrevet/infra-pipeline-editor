using System.ComponentModel;
using System.Text.Json;
using InfraFlowSculptor.Application.CustomDomains.Commands.AddCustomDomain;
using InfraFlowSculptor.Application.StorageAccounts.Commands.AddBlobContainer;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;
using InfraFlowSculptor.Mcp.Common;
using MediatR;
using ModelContextProtocol.Server;

namespace InfraFlowSculptor.Mcp.Tools;

/// <summary>
/// Provides MCP tools for managing sub-resources: blob containers and custom domains.
/// </summary>
[McpServerToolType]
public sealed class SubResourceTools
{
    private const string InvalidIdError = "invalid_id";

    private SubResourceTools() { }

    /// <summary>
    /// Adds a blob container to an existing storage account.
    /// </summary>
    [McpServerTool(Name = "add_blob_container")]
    [Description(
        "Adds a blob container to a storage account. " +
        "publicAccess can be: 'None', 'Blob', or 'Container'.")]
    public static async Task<string> AddBlobContainer(
        ISender mediator,
        [Description("The storage account resource ID (GUID).")] string storageAccountId,
        [Description("The blob container name.")] string name,
        [Description("Public access level: 'None', 'Blob', or 'Container'.")] string publicAccess = "None",
        CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(storageAccountId, out var saId))
        {
            return McpJsonDefaults.Error(InvalidIdError, "The storageAccountId must be a valid GUID.");
        }

        if (!Enum.TryParse<BlobContainerPublicAccess.AccessLevel>(publicAccess, ignoreCase: true, out var accessLevel))
        {
            return McpJsonDefaults.Error("invalid_public_access", $"'{publicAccess}' is not valid. Use 'None', 'Blob', or 'Container'.");
        }

        var command = new AddBlobContainerCommand(
            StorageAccountId: AzureResourceId.Create(saId),
            Name: name,
            PublicAccess: new BlobContainerPublicAccess(accessLevel));

        var result = await mediator.Send(command, cancellationToken);

        return result.Match(
            sa => JsonSerializer.Serialize(new
            {
                status = "success",
                storageAccountId,
                containerName = name,
                publicAccess,
            }, McpJsonDefaults.SerializerOptions),
            errors => McpJsonDefaults.Error("command_failed", string.Join("; ", errors.Select(e => e.Description))));
    }

    /// <summary>
    /// Adds a custom domain binding to a compute resource (ContainerApp, WebApp, FunctionApp).
    /// </summary>
    [McpServerTool(Name = "add_custom_domain")]
    [Description(
        "Adds a custom domain binding to a compute resource for a specific environment. " +
        "certificateMode: 'ManagedCertificate' (default), 'KeyVaultCertificate', 'ManualCertificate', or 'Disabled'.")]
    public static async Task<string> AddCustomDomain(
        ISender mediator,
        [Description("The compute resource ID (GUID).")] string resourceId,
        [Description("The environment name (e.g. 'Development').")] string environmentName,
        [Description("The fully qualified domain name (e.g. 'api.example.com').")] string domainName,
        [Description("Certificate mode: 'ManagedCertificate', 'KeyVaultCertificate', 'ManualCertificate', or 'Disabled'.")] string certificateMode = "ManagedCertificate",
        CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(resourceId, out var id))
        {
            return McpJsonDefaults.Error(InvalidIdError, "The resourceId must be a valid GUID.");
        }

        var command = new AddCustomDomainCommand(
            ResourceId: AzureResourceId.Create(id),
            EnvironmentName: environmentName,
            DomainName: domainName,
            CertificateMode: certificateMode);

        var result = await mediator.Send(command, cancellationToken);

        return result.Match(
            domain => JsonSerializer.Serialize(new
            {
                status = "success",
                customDomainId = domain.Id.ToString(),
                domainName,
                environmentName,
                certificateMode,
            }, McpJsonDefaults.SerializerOptions),
            errors => McpJsonDefaults.Error("command_failed", string.Join("; ", errors.Select(e => e.Description))));
    }
}
