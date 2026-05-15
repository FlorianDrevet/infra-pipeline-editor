using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.VirtualNetworkAggregate.ValueObjects;

/// <summary>Subnet delegation type for Azure service integration.</summary>
public sealed class SubnetDelegation(SubnetDelegation.Delegation value) : EnumValueObject<SubnetDelegation.Delegation>(value)
{
    /// <summary>Available subnet delegation types.</summary>
    public enum Delegation
    {
        /// <summary>No delegation.</summary>
        None,
        /// <summary>App Service / Function App (Microsoft.Web/serverFarms).</summary>
        WebServerFarms,
        /// <summary>Container App Environment (Microsoft.App/environments).</summary>
        AppEnvironments,
        /// <summary>Azure Container Instances (Microsoft.ContainerInstance/containerGroups).</summary>
        ContainerGroups,
        /// <summary>Azure SQL Managed Instance (Microsoft.Sql/managedInstances).</summary>
        SqlManagedInstances,
        /// <summary>Azure PostgreSQL Flexible Server (Microsoft.DBforPostgreSQL/flexibleServers).</summary>
        PostgresFlexible
    }

    /// <summary>Maps enum values to ARM delegation service names.</summary>
    public string ToArmServiceName() => Value switch
    {
        Delegation.WebServerFarms => "Microsoft.Web/serverFarms",
        Delegation.AppEnvironments => "Microsoft.App/environments",
        Delegation.ContainerGroups => "Microsoft.ContainerInstance/containerGroups",
        Delegation.SqlManagedInstances => "Microsoft.Sql/managedInstances",
        Delegation.PostgresFlexible => "Microsoft.DBforPostgreSQL/flexibleServers",
        _ => string.Empty
    };
}
