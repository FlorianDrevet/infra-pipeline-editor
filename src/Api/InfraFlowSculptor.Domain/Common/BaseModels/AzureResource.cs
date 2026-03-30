using InfraFlowSculptor.Domain.Common.BaseModels.Entites;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.Entities;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects.ResourceParameterUsage;
using InfraFlowSculptor.Domain.ResourceGroupAggregate;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.Common.BaseModels;

/// <summary>
/// Base aggregate root for all Azure resources managed by a resource group.
/// Provides shared behavior: dependencies, role assignments, app settings, and parameter usages.
/// Derived types use EF Core TPT (Table-Per-Type) inheritance.
/// </summary>
public class AzureResource : AggregateRoot<AzureResourceId>
{
    /// <summary>Gets the parent resource group identifier.</summary>
    public required ResourceGroupId ResourceGroupId { get; set; }

    /// <summary>Navigation property to the parent resource group.</summary>
    public ResourceGroup ResourceGroup { get; set; } = null!;

    /// <summary>Gets the display name of the resource.</summary>
    public required Name Name { get; set; }

    /// <summary>Gets the Azure region where the resource is deployed.</summary>
    public required Location Location { get; set; }

    /// <summary>
    /// When set, overrides any naming template and uses this value as the resolved resource name.
    /// Set this at resource creation time when the user explicitly provides a full name.
    /// </summary>
    public string? CustomNameOverride { get; set; }
    
    private readonly List<AzureResource> _dependsOn = [];

    /// <summary>Gets the resources this resource depends on.</summary>
    public IReadOnlyList<AzureResource> DependsOn => _dependsOn.AsReadOnly();

    /// <summary>Gets the parameter usages allowed by this resource type. Override in derived classes to restrict.</summary>
    protected virtual IReadOnlyCollection<ParameterUsage> AllowedParameterUsages { get; }

    private readonly List<ResourceParameterUsage> _parameterUsages = [];

    /// <summary>Gets the parameter usages assigned to this resource.</summary>
    public IReadOnlyCollection<ResourceParameterUsage> ParameterUsages => _parameterUsages;

    private readonly List<InputOutputLink> _inputs = [];

    /// <summary>Gets the input links from other resources to this resource.</summary>
    public IReadOnlyCollection<InputOutputLink> Inputs => _inputs;

    private readonly List<InputOutputLink> _outputs = [];

    /// <summary>Gets the output links from this resource to other resources.</summary>
    public IReadOnlyCollection<InputOutputLink> Outputs => _outputs;

    private readonly List<RoleAssignment> _roleAssignments = [];
    public IReadOnlyCollection<RoleAssignment> RoleAssignments => _roleAssignments.AsReadOnly();

    private readonly List<AppSetting> _appSettings = [];
    /// <summary>Gets the application settings (environment variables) configured on this resource.</summary>
    public IReadOnlyCollection<AppSetting> AppSettings => _appSettings.AsReadOnly();

    /// <summary>Registers a dependency on another Azure resource in the same resource group.</summary>
    /// <exception cref="InvalidOperationException">Thrown when the resource attempts to depend on itself.</exception>
    public void AddDependency(AzureResource resource)
    {
        if (resource.Id == Id)
            throw new InvalidOperationException("A resource cannot depend on itself.");

        if (_dependsOn.Any(r => r.Id == resource.Id))
            return;

        _dependsOn.Add(resource);
    }

    /// <summary>Adds a role assignment from this resource to the specified target resource.</summary>
    /// <param name="targetResourceId">Identifier of the target resource.</param>
    /// <param name="managedIdentityType">Type of managed identity used.</param>
    /// <param name="roleDefinitionId">Azure role definition ID to grant.</param>
    /// <param name="userAssignedIdentityId">Optional User-Assigned Identity resource ID (required when <paramref name="managedIdentityType"/> is UserAssigned).</param>
    public void AddRoleAssignment(
        AzureResourceId targetResourceId,
        ManagedIdentityType managedIdentityType,
        string roleDefinitionId,
        AzureResourceId? userAssignedIdentityId = null)
    {
        if (targetResourceId == Id)
            throw new InvalidOperationException("A resource cannot assign a role to itself.");

        if (_roleAssignments.Any(r =>
                r.TargetResourceId == targetResourceId &&
                r.RoleDefinitionId == roleDefinitionId &&
                r.ManagedIdentityType.Value == managedIdentityType.Value))
            return;

        _roleAssignments.Add(RoleAssignment.Create(Id, targetResourceId, managedIdentityType, roleDefinitionId, userAssignedIdentityId));
    }

    /// <summary>Removes a role assignment by its identifier. No-op if not found.</summary>
    public void RemoveRoleAssignment(RoleAssignmentId roleAssignmentId)
    {
        var assignment = _roleAssignments.FirstOrDefault(r => r.Id == roleAssignmentId);
        if (assignment is not null)
            _roleAssignments.Remove(assignment);
    }

    /// <summary>Updates the managed identity on an existing role assignment.</summary>
    /// <param name="roleAssignmentId">Identifier of the role assignment to update.</param>
    /// <param name="managedIdentityType">New managed identity type.</param>
    /// <param name="userAssignedIdentityId">New User-Assigned Identity resource ID, or <c>null</c> for system-assigned.</param>
    /// <returns>The updated <see cref="RoleAssignment"/>, or <c>null</c> if not found.</returns>
    public RoleAssignment? UpdateRoleAssignmentIdentity(
        RoleAssignmentId roleAssignmentId,
        ManagedIdentityType managedIdentityType,
        AzureResourceId? userAssignedIdentityId)
    {
        var assignment = _roleAssignments.FirstOrDefault(r => r.Id == roleAssignmentId);
        assignment?.UpdateIdentity(managedIdentityType, userAssignedIdentityId);
        return assignment;
    }

    /// <summary>Adds a static-value app setting with per-environment values to this resource.</summary>
    /// <param name="name">The environment variable name.</param>
    /// <param name="environmentValues">A dictionary of environment name → value.</param>
    public AppSetting AddStaticAppSetting(string name, IReadOnlyDictionary<string, string> environmentValues)
    {
        var setting = AppSetting.CreateStatic(Id, name, environmentValues);
        _appSettings.Add(setting);
        return setting;
    }

    /// <summary>Adds an app setting that references another resource's output.</summary>
    /// <param name="name">The environment variable name.</param>
    /// <param name="sourceResourceId">The source resource whose output to reference.</param>
    /// <param name="sourceOutputName">The output name on the source resource.</param>
    public AppSetting AddOutputReferenceAppSetting(
        string name,
        AzureResourceId sourceResourceId,
        string sourceOutputName)
    {
        var setting = AppSetting.CreateOutputReference(Id, name, sourceResourceId, sourceOutputName);
        _appSettings.Add(setting);
        return setting;
    }

    /// <summary>Adds an app setting that references a Key Vault secret.</summary>
    /// <param name="name">The environment variable name.</param>
    /// <param name="keyVaultResourceId">The Key Vault resource identifier.</param>
    /// <param name="secretName">The secret name in the Key Vault.</param>
    /// <param name="assignment">Determines how the secret value is assigned.</param>
    public AppSetting AddKeyVaultReferenceAppSetting(
        string name,
        AzureResourceId keyVaultResourceId,
        string secretName,
        SecretValueAssignment assignment)
    {
        var setting = AppSetting.CreateKeyVaultReference(Id, name, keyVaultResourceId, secretName, assignment);
        _appSettings.Add(setting);
        return setting;
    }

    /// <summary>
    /// Adds an app setting that exports a sensitive resource output as a Key Vault secret
    /// and references that secret via a Key Vault reference.
    /// </summary>
    /// <param name="name">The environment variable name.</param>
    /// <param name="sourceResourceId">The source resource whose output is exported.</param>
    /// <param name="sourceOutputName">The output name on the source resource.</param>
    /// <param name="keyVaultResourceId">The Key Vault resource where the secret will be stored.</param>
    /// <param name="secretName">The secret name in the Key Vault.</param>
    public AppSetting AddSensitiveOutputKeyVaultReferenceAppSetting(
        string name,
        AzureResourceId sourceResourceId,
        string sourceOutputName,
        AzureResourceId keyVaultResourceId,
        string secretName)
    {
        var setting = AppSetting.CreateSensitiveOutputKeyVaultReference(
            Id, name, sourceResourceId, sourceOutputName, keyVaultResourceId, secretName);
        _appSettings.Add(setting);
        return setting;
    }

    /// <summary>Removes an app setting by its identifier.</summary>
    /// <summary>Removes an app setting by its identifier. No-op if not found.</summary>
    public void RemoveAppSetting(AppSettingId appSettingId)
    {
        var setting = _appSettings.FirstOrDefault(s => s.Id == appSettingId);
        if (setting is not null)
            _appSettings.Remove(setting);
    }

    /// <summary>Updates an existing app setting to static per-environment values.</summary>
    public void UpdateAppSettingToStatic(AppSettingId appSettingId, string name, IReadOnlyDictionary<string, string> environmentValues)
    {
        var setting = _appSettings.FirstOrDefault(s => s.Id == appSettingId);
        setting?.UpdateToStatic(name, environmentValues);
    }

    /// <summary>Updates an existing app setting to reference a resource output.</summary>
    public void UpdateAppSettingToOutputReference(
        AppSettingId appSettingId,
        string name,
        AzureResourceId sourceResourceId,
        string sourceOutputName)
    {
        var setting = _appSettings.FirstOrDefault(s => s.Id == appSettingId);
        setting?.UpdateToOutputReference(name, sourceResourceId, sourceOutputName);
    }

    protected void AddParameterUsage(
        ParameterDefinition parameter,
        ParameterUsage usage)
    {
        if (!AllowedParameterUsages.Contains(usage))
            throw new InvalidOperationException(
                $"{GetType().Name} does not support parameter usage '{usage}'");

        _parameterUsages.Add(
            new ResourceParameterUsage(Id, parameter.Id, usage));
    }

    /// <summary>EF Core constructor.</summary>
    protected AzureResource()
    {
    }
}