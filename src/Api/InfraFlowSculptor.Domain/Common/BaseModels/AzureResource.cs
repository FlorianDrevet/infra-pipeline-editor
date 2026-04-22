using ErrorOr;
using InfraFlowSculptor.Domain.Common.BaseModels.Entites;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.Entities;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects.ResourceParameterUsage;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
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
    /// <summary>
    /// Gets the concrete resource type name (e.g. "KeyVault", "WebApp").
    /// Persisted as a discriminator column to enable lightweight queries without TPT resolution.
    /// Automatically set by <see cref="Infrastructure"/> on entity creation.
    /// </summary>
    public string ResourceType { get; private set; } = string.Empty;

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

    /// <summary>
    /// Gets whether this resource already exists in Azure and is not managed (deployed) by this project.
    /// When <c>true</c>, the resource generates a Bicep <c>existing</c> declaration instead of a deployment module.
    /// Immutable after creation.
    /// </summary>
    public bool IsExisting { get; protected set; }

    private readonly List<AzureResource> _dependsOn = [];

    /// <summary>Gets the resources this resource depends on.</summary>
    public IReadOnlyList<AzureResource> DependsOn => _dependsOn.AsReadOnly();

    /// <summary>Gets the parameter usages allowed by this resource type. Override in derived classes to restrict.</summary>
    protected virtual IReadOnlyCollection<ParameterUsage> AllowedParameterUsages { get; } = [];

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

    private readonly List<SecureParameterMapping> _secureParameterMappings = [];

    /// <summary>Gets the secure parameter mappings for this resource.</summary>
    public IReadOnlyCollection<SecureParameterMapping> SecureParameterMappings => _secureParameterMappings.AsReadOnly();

    private readonly List<CustomDomain> _customDomains = [];

    /// <summary>Gets the custom domain bindings configured on this resource.</summary>
    public IReadOnlyCollection<CustomDomain> CustomDomains => _customDomains.AsReadOnly();

    /// <summary>Gets the optional User-Assigned Identity explicitly attached to this resource.</summary>
    public AzureResourceId? AssignedUserAssignedIdentityId { get; private set; }

    /// <summary>
    /// Assigns a User-Assigned Identity to this resource.
    /// This models the ARM <c>identity: { type: 'UserAssigned' }</c> concept.
    /// </summary>
    /// <param name="identityId">The identifier of the User-Assigned Identity resource.</param>
    public void AssignUserAssignedIdentity(AzureResourceId identityId)
    {
        AssignedUserAssignedIdentityId = identityId;
    }

    /// <summary>Removes the assigned User-Assigned Identity from this resource.</summary>
    public void UnassignUserAssignedIdentity()
    {
        AssignedUserAssignedIdentityId = null;
    }

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

    /// <summary>Adds an app setting whose value comes from a pipeline variable group at deploy time.</summary>
    /// <param name="name">The environment variable name.</param>
    /// <param name="variableGroupId">The pipeline variable group identifier.</param>
    /// <param name="pipelineVariableName">The pipeline variable name within the group.</param>
    public AppSetting AddViaVariableGroupAppSetting(
        string name,
        ProjectPipelineVariableGroupId variableGroupId,
        string pipelineVariableName)
    {
        var setting = AppSetting.CreateViaVariableGroup(Id, name, variableGroupId, pipelineVariableName);
        _appSettings.Add(setting);
        return setting;
    }

    /// <summary>
    /// Adds an app setting whose value comes from a pipeline variable group
    /// and is stored as a Key Vault secret referenced by the app setting.
    /// </summary>
    /// <param name="name">The environment variable name.</param>
    /// <param name="variableGroupId">The pipeline variable group identifier.</param>
    /// <param name="pipelineVariableName">The pipeline variable name within the group.</param>
    /// <param name="keyVaultResourceId">The Key Vault resource identifier.</param>
    /// <param name="secretName">The secret name in the Key Vault.</param>
    /// <param name="assignment">Determines how the secret value is assigned.</param>
    public AppSetting AddViaVariableGroupKeyVaultReferenceAppSetting(
        string name,
        ProjectPipelineVariableGroupId variableGroupId,
        string pipelineVariableName,
        AzureResourceId keyVaultResourceId,
        string secretName,
        SecretValueAssignment assignment)
    {
        var setting = AppSetting.CreateViaVariableGroupKeyVaultReference(
            Id, name, variableGroupId, pipelineVariableName, keyVaultResourceId, secretName, assignment);
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

    /// <summary>
    /// Adds a custom domain binding to this resource for a specific environment.
    /// Duplicates (same environment + domain name) are rejected.
    /// </summary>
    /// <param name="environmentName">The deployment environment name.</param>
    /// <param name="domainName">The fully qualified domain name.</param>
    /// <param name="bindingType">The SSL binding type (default: "SniEnabled").</param>
    /// <returns>The created <see cref="CustomDomain"/>, or an error if duplicate.</returns>
    public ErrorOr<CustomDomain> AddCustomDomain(
        string environmentName,
        string domainName,
        string bindingType = "SniEnabled")
    {
        if (IsExisting)
            return Errors.Errors.CustomDomain.NotSupportedForExistingResource();

        var normalizedDomain = domainName.ToLowerInvariant().Trim();

        if (_customDomains.Any(cd =>
                cd.EnvironmentName == environmentName &&
                cd.DomainName == normalizedDomain))
            return Errors.Errors.CustomDomain.DuplicateDomain(environmentName, normalizedDomain);

        var customDomain = CustomDomain.Create(Id, environmentName, normalizedDomain, bindingType);
        _customDomains.Add(customDomain);
        return customDomain;
    }

    /// <summary>Removes a custom domain binding by its identifier. No-op if not found.</summary>
    /// <param name="customDomainId">Identifier of the custom domain to remove.</param>
    public void RemoveCustomDomain(CustomDomainId customDomainId)
    {
        var domain = _customDomains.FirstOrDefault(cd => cd.Id == customDomainId);
        if (domain is not null)
            _customDomains.Remove(domain);
    }

    /// <summary>
    /// Sets or clears a mapping for a secure Bicep parameter to a pipeline variable group.
    /// When <paramref name="variableGroupId"/> is <c>null</c>, the existing mapping is removed.
    /// </summary>
    /// <param name="secureParameterName">Name of the secure Bicep parameter.</param>
    /// <param name="variableGroupId">Pipeline variable group identifier, or <c>null</c> to clear.</param>
    /// <param name="pipelineVariableName">Variable name within the group, or <c>null</c> to clear.</param>
    /// <returns>The created/updated mapping, or an error.</returns>
    public ErrorOr<SecureParameterMapping> SetSecureParameterMapping(
        string secureParameterName,
        ProjectPipelineVariableGroupId? variableGroupId,
        string? pipelineVariableName)
    {
        if ((variableGroupId is not null && string.IsNullOrWhiteSpace(pipelineVariableName)) ||
            (variableGroupId is null && !string.IsNullOrWhiteSpace(pipelineVariableName)))
            return Errors.Errors.SecureParameterMapping.InconsistentMapping();

        var existing = _secureParameterMappings.FirstOrDefault(
            m => m.SecureParameterName == secureParameterName);

        if (existing is not null)
        {
            if (variableGroupId is null)
            {
                _secureParameterMappings.Remove(existing);
                return existing;
            }

            existing.Update(variableGroupId, pipelineVariableName!);
            return existing;
        }

        if (variableGroupId is null)
            return Errors.Errors.SecureParameterMapping.NotFound(secureParameterName);

        var mapping = SecureParameterMapping.Create(Id, secureParameterName, variableGroupId, pipelineVariableName!);
        _secureParameterMappings.Add(mapping);
        return mapping;
    }

    /// <summary>EF Core constructor.</summary>
    protected AzureResource()
    {
    }
}