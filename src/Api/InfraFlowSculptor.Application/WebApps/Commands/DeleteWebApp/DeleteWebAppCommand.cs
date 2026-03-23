using ErrorOr;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.WebApps.Commands.DeleteWebApp;

/// <summary>Command to delete a Web App.</summary>
public record DeleteWebAppCommand(
    AzureResourceId Id
) : IRequest<ErrorOr<Deleted>>;
