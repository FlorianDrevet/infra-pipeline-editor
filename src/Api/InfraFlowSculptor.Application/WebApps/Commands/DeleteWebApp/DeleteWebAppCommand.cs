using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.WebApps.Commands.DeleteWebApp;

/// <summary>Command to delete a Web App.</summary>
public record DeleteWebAppCommand(
    AzureResourceId Id
) : ICommand<Deleted>;
