using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.CustomDomains.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.CustomDomains.Commands.AddCustomDomain;

/// <summary>Command to add a custom domain binding to a compute resource for a specific environment.</summary>
/// <param name="ResourceId">Identifier of the compute resource (WebApp, FunctionApp, or ContainerApp).</param>
/// <param name="EnvironmentName">The deployment environment name.</param>
/// <param name="DomainName">The fully qualified domain name (e.g. "api.example.com").</param>
/// <param name="BindingType">SSL binding type: "SniEnabled" (default) or "Disabled".</param>
public record AddCustomDomainCommand(
    AzureResourceId ResourceId,
    string EnvironmentName,
    string DomainName,
    string BindingType = "SniEnabled") : ICommand<CustomDomainResult>;
