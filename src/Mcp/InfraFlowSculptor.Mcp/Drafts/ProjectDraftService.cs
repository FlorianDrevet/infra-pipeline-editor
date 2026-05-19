using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.Mcp.Drafts.Models;
using Microsoft.Extensions.Options;

namespace InfraFlowSculptor.Mcp.Drafts;

/// <summary>In-memory implementation of <see cref="IProjectDraftService"/> for the MCP session.</summary>
public sealed class ProjectDraftService : IProjectDraftService
{
    private const string DraftIdPrefix = "draft_";
    private static readonly TimeSpan RegexTimeout = TimeSpan.FromMilliseconds(250);

    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "avec", "en", "de", "du", "le", "la", "les", "un", "une", "des", "pour", "dans", "qui", "que",
        "the", "a", "an", "with", "in", "for", "and", "or", "from", "to", "that", "this",
    };

    private static readonly Dictionary<string, string> ResourceTypeAliases = BuildResourceTypeAliases();

    private readonly ConcurrentDictionary<string, ProjectCreationDraft> _drafts = new();
    private readonly object _syncRoot = new();
    private readonly int _maxDraftCount;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectDraftService"/> class.
    /// </summary>
    /// <param name="draftStorageOptions">The configured in-memory storage options for project drafts.</param>
    public ProjectDraftService(IOptions<ProjectDraftStorageOptions> draftStorageOptions)
    {
        ArgumentNullException.ThrowIfNull(draftStorageOptions);

        _maxDraftCount = draftStorageOptions.Value.MaxDraftCount;

        if (_maxDraftCount < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(draftStorageOptions), _maxDraftCount, "MaxDraftCount must be greater than zero.");
        }
    }

    /// <inheritdoc />
    public ProjectCreationDraft CreateDraftFromPrompt(string userPrompt)
    {
        var intent = ParsePromptIntent(userPrompt);
        var missingFields = new List<string>();
        var clarificationQuestions = new List<DraftClarificationQuestion>();
        var defaultEnvironmentAdded = false;

        if (intent.LayoutPreset is null)
        {
            missingFields.Add(DraftFieldNames.LayoutPreset);
            clarificationQuestions.Add(BuildLayoutPresetQuestion());
        }

        if (intent.ProjectName is null)
        {
            missingFields.Add(DraftFieldNames.ProjectName);
            clarificationQuestions.Add(new DraftClarificationQuestion
            {
                Field = DraftFieldNames.ProjectName,
                Message = "What name would you like for your project?",
            });
        }

        if (intent.Environments is null or { Count: 0 })
        {
            intent.Environments = [new DraftEnvironmentIntent()];
            defaultEnvironmentAdded = true;
        }

        if (intent.LayoutPreset is not null)
        {
            intent.Repositories = BuildDefaultRepositories(intent.LayoutPreset.Value);
        }

        var warnings = ProjectDraftWarnings.Build(intent.Environments, defaultEnvironmentAdded);

        var status = missingFields.Count == 0 ? DraftStatus.ReadyToCreate : DraftStatus.RequiresClarification;

        lock (_syncRoot)
        {
            if (_drafts.Count >= _maxDraftCount)
            {
                throw new ProjectDraftLimitExceededException(_maxDraftCount);
            }

            while (true)
            {
                var draftId = CreateDraftId();
                var draft = new ProjectCreationDraft
                {
                    DraftId = draftId,
                    Status = status,
                    MissingFields = missingFields,
                    ClarificationQuestions = clarificationQuestions,
                    Intent = intent,
                    Warnings = warnings,
                };

                if (_drafts.TryAdd(draftId, draft))
                {
                    return draft;
                }
            }
        }
    }

    /// <inheritdoc />
    public ProjectCreationDraft? GetDraft(string draftId)
    {
        return _drafts.TryGetValue(draftId, out var draft) ? draft : null;
    }

    /// <inheritdoc />
    public ProjectCreationDraft? ValidateAndUpdate(string draftId, DraftOverrides overrides)
    {
        if (!_drafts.TryGetValue(draftId, out var draft))
        {
            return null;
        }

        ApplyOverrides(draft.Intent, overrides);

        if (overrides.LayoutPreset is not null)
        {
            draft.Intent.Repositories = BuildDefaultRepositories(draft.Intent.LayoutPreset!.Value);
        }

        Revalidate(draft);
        return draft;
    }

    private static void ApplyOverrides(DraftProjectIntent intent, DraftOverrides overrides)
    {
        if (overrides.ProjectName is not null)
        {
            intent.ProjectName = overrides.ProjectName;
        }

        if (overrides.LayoutPreset is not null)
        {
            intent.LayoutPreset = overrides.LayoutPreset;
        }

        if (overrides.Description is not null)
        {
            intent.Description = overrides.Description;
        }

        if (overrides.Environments is not null)
        {
            intent.Environments = overrides.Environments;
        }

        if (overrides.Repositories is not null)
        {
            intent.Repositories = overrides.Repositories;
        }

        if (overrides.ResourceGroupAssignments is not null)
        {
            intent.ResourceGroupAssignments = overrides.ResourceGroupAssignments;
        }

        if (overrides.Resources is not null)
        {
            intent.Resources = overrides.Resources;
        }

        if (overrides.AgentPoolName is not null)
        {
            intent.AgentPoolName = overrides.AgentPoolName;
        }

        if (overrides.RepositoryUrl is not null && intent.Repositories is { Count: > 0 })
        {
            intent.Repositories[0].RepositoryUrl = overrides.RepositoryUrl;
        }
    }

    private static void Revalidate(ProjectCreationDraft draft)
    {
        var missingFields = new List<string>();
        var errors = new List<DraftValidationError>();
        var clarificationQuestions = new List<DraftClarificationQuestion>();

        if (string.IsNullOrWhiteSpace(draft.Intent.ProjectName))
        {
            missingFields.Add(DraftFieldNames.ProjectName);
            clarificationQuestions.Add(new DraftClarificationQuestion
            {
                Field = DraftFieldNames.ProjectName,
                Message = "What name would you like for your project?",
            });
        }
        else if (draft.Intent.ProjectName.Length < 3 || draft.Intent.ProjectName.Length > 80)
        {
            errors.Add(new DraftValidationError
            {
                Field = DraftFieldNames.ProjectName,
                Message = "Project name must be between 3 and 80 characters.",
            });
        }

        if (draft.Intent.LayoutPreset is null)
        {
            missingFields.Add(DraftFieldNames.LayoutPreset);
            clarificationQuestions.Add(BuildLayoutPresetQuestion());
        }

        if (draft.Intent.Environments is null or { Count: 0 })
        {
            errors.Add(new DraftValidationError
            {
                Field = DraftFieldNames.Environments,
                Message = "At least one environment is required.",
            });
        }

        // Clarification: multi-RG topology detected but resource names are missing
        if (draft.Intent.HasMultiResourceGroupTopology
            && draft.Intent.Resources is { Count: > 0 }
            && draft.Intent.Resources.Any(r => string.IsNullOrWhiteSpace(r.Name)))
        {
            clarificationQuestions.Add(new DraftClarificationQuestion
            {
                Field = DraftFieldNames.ResourceNames,
                Message = "You have multiple resources of the same type. Please provide a semantic name for each (e.g. 'api', 'backoffice', 'website').",
            });
        }

        // Clarification: multiple instances of the same resource type without names
        var duplicateTypes = draft.Intent.Resources?
            .GroupBy(r => r.ResourceType, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1 && g.Any(r => string.IsNullOrWhiteSpace(r.Name)))
            .ToList() ?? [];

        if (duplicateTypes.Count > 0 && !clarificationQuestions.Any(q => q.Field == DraftFieldNames.ResourceNames))
        {
            var typeList = string.Join(", ", duplicateTypes.Select(g => $"{g.Count()}x {g.Key}"));
            clarificationQuestions.Add(new DraftClarificationQuestion
            {
                Field = DraftFieldNames.ResourceNames,
                Message = $"Multiple instances detected ({typeList}). Please provide distinct semantic names for each instance.",
            });
        }

        draft.MissingFields = missingFields;
        draft.Errors = errors;
        draft.ClarificationQuestions = clarificationQuestions;
        draft.Warnings = ProjectDraftWarnings.Build(draft.Intent.Environments, includeDefaultEnvironmentWarning: false);

        if (draft.Intent.LayoutPreset is LayoutPresetEnum.AllInOne or LayoutPresetEnum.SplitInfraCode
            && draft.Intent.Repositories is { Count: > 0 }
            && draft.Intent.Repositories.Any(r => string.IsNullOrWhiteSpace(r.RepositoryUrl)))
        {
            draft.Warnings.Add("One or more repositories have no URL configured. Set 'repositoryUrl' in overrides or configure it later in the project settings.");
        }

        // Warning about multi-RG topology
        if (draft.Intent.HasMultiResourceGroupTopology)
        {
            draft.Warnings.Add($"Multi-resource-group topology detected ({draft.Intent.ResourceGroupAssignments!.Count} groups). " +
                "After project creation, use 'create_infrastructure_config' + 'create_resource_group' for each group, " +
                "then 'add_cross_config_reference' to wire shared resources.");
        }

        draft.Status = missingFields.Count == 0 && errors.Count == 0
            ? DraftStatus.ReadyToCreate
            : DraftStatus.RequiresClarification;
    }

    private static DraftProjectIntent ParsePromptIntent(string userPrompt)
    {
        var resources = ExtractResourceTypes(userPrompt);
        var resourceGroupAssignments = ExtractResourceGroupAssignments(userPrompt, resources);

        return new DraftProjectIntent
        {
            ProjectName = ExtractProjectName(userPrompt),
            LayoutPreset = ExtractLayoutPreset(userPrompt),
            Resources = resources,
            Environments = ExtractEnvironments(userPrompt),
            ResourceGroupAssignments = resourceGroupAssignments,
            PricingIntent = ExtractPricingIntent(userPrompt),
        };
    }

    private static string? ExtractProjectName(string prompt)
    {
        var quotedMatch = Regex.Match(prompt, """["']([^"']+)["']""", RegexOptions.None, RegexTimeout);
        if (quotedMatch.Success)
        {
            return quotedMatch.Groups[1].Value;
        }

        var nameMatch = Regex.Match(prompt, @"\b(?:projet|project)\s+(\w[\w-]*)", RegexOptions.IgnoreCase, RegexTimeout);
        if (nameMatch.Success)
        {
            var candidate = nameMatch.Groups[1].Value;
            if (!StopWords.Contains(candidate))
            {
                return candidate;
            }
        }

        return null;
    }

    private static LayoutPresetEnum? ExtractLayoutPreset(string prompt)
    {
        var lower = prompt.ToLowerInvariant();

        if (lower.Contains("mono") || lower.Contains("all in one") || lower.Contains("all-in-one") || lower.Contains("allinone"))
        {
            return LayoutPresetEnum.AllInOne;
        }

        if (lower.Contains("split"))
        {
            return LayoutPresetEnum.SplitInfraCode;
        }

        if (lower.Contains("multi"))
        {
            return LayoutPresetEnum.MultiRepo;
        }

        return null;
    }

    private static List<DraftResourceIntent> ExtractResourceTypes(string prompt)
    {
        var results = new List<DraftResourceIntent>();
        var lower = prompt.ToLowerInvariant();
        var matched = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var (alias, resourceType) in ResourceTypeAliases)
        {
            if (lower.Contains(alias) && matched.Add(resourceType))
            {
                results.Add(new DraftResourceIntent { ResourceType = resourceType });
            }
        }

        // Detect multiple instances of the same resource type via quantifiers
        DetectMultipleInstances(prompt, results);

        return results;
    }

    private static void DetectMultipleInstances(string prompt, List<DraftResourceIntent> results)
    {
        var lower = prompt.ToLowerInvariant();

        // Patterns like "3 container apps", "deux fronts", "two apis", "mes deux fronts"
        var quantifierPatterns = new (Regex Pattern, string[] ResourceTypes)[]
        {
            (new Regex(@"(\d+)\s+(?:container\s*apps?|aca)", RegexOptions.IgnoreCase, RegexTimeout), [AzureResourceTypes.ContainerApp]),
            (new Regex(@"(\d+)\s+(?:web\s*apps?|sites?)", RegexOptions.IgnoreCase, RegexTimeout), [AzureResourceTypes.WebApp]),
            (new Regex(@"(\d+)\s+(?:function\s*apps?|functions?)", RegexOptions.IgnoreCase, RegexTimeout), [AzureResourceTypes.FunctionApp]),
            (new Regex(@"\b(?:deux|two|2)\s+(?:fronts?|frontends?)", RegexOptions.IgnoreCase, RegexTimeout), [AzureResourceTypes.ContainerApp, AzureResourceTypes.WebApp]),
            (new Regex(@"\b(?:trois|three|3)\s+(?:fronts?|frontends?|apps?|applicatifs?|services?)", RegexOptions.IgnoreCase, RegexTimeout), [AzureResourceTypes.ContainerApp, AzureResourceTypes.WebApp]),
        };

        foreach (var (pattern, targetTypes) in quantifierPatterns)
        {
            var match = pattern.Match(lower);
            if (!match.Success) continue;

            var count = ParseFrenchQuantifier(match.Groups[1].Value);
            if (count <= 1) continue;

            // Find which of the target types is already in the results
            var existingResource = results.FirstOrDefault(r => targetTypes.Contains(r.ResourceType, StringComparer.OrdinalIgnoreCase));
            if (existingResource is null) continue;

            // Add additional instances (the first one already exists)
            for (var i = 1; i < count; i++)
            {
                results.Add(new DraftResourceIntent { ResourceType = existingResource.ResourceType });
            }

            break; // Only apply the first matching quantifier pattern
        }

        // Detect semantic names: "mon api et mes deux fronts" → name the instances
        ExtractSemanticNames(prompt, results);
    }

    private static void ExtractSemanticNames(string prompt, List<DraftResourceIntent> results)
    {
        var lower = prompt.ToLowerInvariant();

        // Pattern: "mon/my api" → assign name "api" to first compute resource
        var apiMatch = Regex.Match(lower, @"\b(?:mon|my|l'?)\s*(api|backend|server)", RegexOptions.None, RegexTimeout);
        if (apiMatch.Success)
        {
            var apiResource = results.FirstOrDefault(r => r.Name is null &&
                (r.ResourceType == AzureResourceTypes.ContainerApp || r.ResourceType == AzureResourceTypes.WebApp || r.ResourceType == AzureResourceTypes.FunctionApp));
            if (apiResource is not null)
            {
                apiResource.Name = apiMatch.Groups[1].Value;
            }
        }

        // Pattern: "mes/my fronts/frontends" → name unnamed compute resources as front-1, front-2...
        var frontMatch = Regex.Match(lower, @"\b(?:mes|my|les|deux|two|2)\s+(?:fronts?|frontends?)", RegexOptions.None, RegexTimeout);
        if (frontMatch.Success)
        {
            var unnamedFronts = results
                .Where(r => r.Name is null && (r.ResourceType == AzureResourceTypes.ContainerApp || r.ResourceType == AzureResourceTypes.WebApp))
                .ToList();

            for (var i = 0; i < unnamedFronts.Count; i++)
            {
                unnamedFronts[i].Name = $"front-{i + 1}";
            }
        }
    }

    private static int ParseFrenchQuantifier(string value)
    {
        if (int.TryParse(value, out var number))
            return number;

        return value.ToLowerInvariant() switch
        {
            "deux" or "two" => 2,
            "trois" or "three" => 3,
            "quatre" or "four" => 4,
            "cinq" or "five" => 5,
            _ => 1,
        };
    }

    private static List<DraftEnvironmentIntent>? ExtractEnvironments(string prompt)
    {
        var lower = prompt.ToLowerInvariant();
        var environments = new List<DraftEnvironmentIntent>();

        // Detect dev/development
        if (ContainsEnvironmentKeyword(lower, "dev", "development", "développement"))
        {
            environments.Add(new DraftEnvironmentIntent
            {
                Name = "Development",
                ShortName = "dev",
                Prefix = "",
                Suffix = "-dev",
                Order = 0,
                RequiresApproval = false,
            });
        }

        // Detect staging/recette/stg
        if (ContainsEnvironmentKeyword(lower, "stg", "staging", "recette", "preprod", "pré-prod"))
        {
            environments.Add(new DraftEnvironmentIntent
            {
                Name = "Staging",
                ShortName = "stg",
                Prefix = "",
                Suffix = "-stg",
                Order = 1,
                RequiresApproval = true,
            });
        }

        // Detect prod/production
        if (ContainsEnvironmentKeyword(lower, "prod", "production"))
        {
            environments.Add(new DraftEnvironmentIntent
            {
                Name = "Production",
                ShortName = "prod",
                Prefix = "",
                Suffix = "-prod",
                Order = environments.Count,
                RequiresApproval = true,
            });
        }

        return environments.Count > 0 ? environments : null;
    }

    private static bool ContainsEnvironmentKeyword(string lower, params string[] keywords)
    {
        foreach (var keyword in keywords)
        {
            // Use word boundary check to avoid false positives (e.g. "production" matching in "reproduction")
            if (Regex.IsMatch(lower, @$"\b{Regex.Escape(keyword)}\b", RegexOptions.None, RegexTimeout))
            {
                return true;
            }
        }

        return false;
    }

    private static List<DraftResourceGroupAssignment>? ExtractResourceGroupAssignments(
        string prompt,
        List<DraftResourceIntent> resources)
    {
        var lower = prompt.ToLowerInvariant();

        // Detect multi-RG patterns
        var hasCommonRg = Regex.IsMatch(lower, @"\b(?:rg|resource\s*group)\s+(?:common|commun|shared|partagé|mutualisé)", RegexOptions.None, RegexTimeout)
            || Regex.IsMatch(lower, @"\b(?:common|commun|shared|partagé|mutualisé)\s+(?:rg|resource\s*group)", RegexOptions.None, RegexTimeout);

        var hasAppRg = Regex.IsMatch(lower, @"\b(?:rg|resource\s*group)\s+(?:pour|for)\s+\w+", RegexOptions.None, RegexTimeout)
            || Regex.IsMatch(lower, @"\ble\s+reste\s+dans\s+(?:un\s+)?(?:rg|resource\s*group)", RegexOptions.None, RegexTimeout);

        if (!hasCommonRg && !hasAppRg)
        {
            return null;
        }

        var assignments = new List<DraftResourceGroupAssignment>();

        // Shared/common resource group: typically ACR, LAW, AppInsights, KeyVault
        var sharedResourceTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            AzureResourceTypes.ContainerRegistry,
            AzureResourceTypes.LogAnalyticsWorkspace,
            AzureResourceTypes.ApplicationInsights,
        };

        // Detect what belongs in common from prompt context
        var commonResources = resources
            .Where(r => sharedResourceTypes.Contains(r.ResourceType))
            .Select(r => r.Name ?? r.ResourceType)
            .ToList();

        if (commonResources.Count > 0 || hasCommonRg)
        {
            var commonGroup = new DraftResourceGroupAssignment
            {
                GroupName = "common",
                Description = "Shared infrastructure resources (monitoring, registry)",
                ResourceIdentifiers = commonResources.Count > 0 ? commonResources : [AzureResourceTypes.LogAnalyticsWorkspace],
                IsShared = true,
            };
            assignments.Add(commonGroup);

            // Mark the resources with their group
            foreach (var resource in resources.Where(r => sharedResourceTypes.Contains(r.ResourceType)))
            {
                resource.ResourceGroupName = "common";
            }
        }

        // Application resource group: everything else
        var appResources = resources
            .Where(r => !sharedResourceTypes.Contains(r.ResourceType))
            .Select(r => r.Name ?? r.ResourceType)
            .ToList();

        if (appResources.Count > 0 || hasAppRg)
        {
            var appGroup = new DraftResourceGroupAssignment
            {
                GroupName = "app",
                Description = "Application resources",
                ResourceIdentifiers = appResources,
                IsShared = false,
            };
            assignments.Add(appGroup);

            foreach (var resource in resources.Where(r => !sharedResourceTypes.Contains(r.ResourceType)))
            {
                resource.ResourceGroupName ??= "app";
            }
        }

        return assignments.Count > 0 ? assignments : null;
    }

    private static string? ExtractPricingIntent(string prompt)
    {
        var lower = prompt.ToLowerInvariant();

        if (lower.Contains("le moins cher") || lower.Contains("cheapest") || lower.Contains("lowest cost"))
        {
            return "cheapest";
        }

        if (lower.Contains("premium"))
        {
            return "premium";
        }

        return null;
    }

    private static List<DraftRepositoryIntent> BuildDefaultRepositories(LayoutPresetEnum layoutPreset)
    {
        return layoutPreset switch
        {
            LayoutPresetEnum.AllInOne =>
            [
                new DraftRepositoryIntent { ContentKinds = [nameof(RepositoryContentKindsEnum.Infrastructure), nameof(RepositoryContentKindsEnum.ApplicationCode)] },
            ],
            LayoutPresetEnum.SplitInfraCode =>
            [
                new DraftRepositoryIntent { ContentKinds = [nameof(RepositoryContentKindsEnum.Infrastructure)] },
                new DraftRepositoryIntent { ContentKinds = [nameof(RepositoryContentKindsEnum.ApplicationCode)] },
            ],
            LayoutPresetEnum.MultiRepo => [],
            _ => [],
        };
    }

    private static DraftClarificationQuestion BuildLayoutPresetQuestion() =>
        new()
        {
            Field = DraftFieldNames.LayoutPreset,
            Message = "Which repository topology would you like to use?",
            Options =
            [
                new DraftOption
                {
                    Value = nameof(LayoutPresetEnum.AllInOne),
                    Label = "All-in-One (Mono Repo)",
                    Description = "One single repository for infrastructure and application code.",
                },
                new DraftOption
                {
                    Value = nameof(LayoutPresetEnum.SplitInfraCode),
                    Label = "Split Infra / Code",
                    Description = "Two repositories: one for infrastructure, one for application code.",
                },
                new DraftOption
                {
                    Value = nameof(LayoutPresetEnum.MultiRepo),
                    Label = "Multi-Repo",
                    Description = "Repositories are declared per infrastructure configuration.",
                },
            ],
        };

    private static Dictionary<string, string> BuildResourceTypeAliases()
    {
        var aliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var resourceType in AzureResourceTypes.All)
        {
            aliases.TryAdd(resourceType.ToLowerInvariant(), resourceType);

            var spaced = Regex.Replace(resourceType, "(?<=[a-z])([A-Z])", " $1", RegexOptions.None, RegexTimeout)
                .ToLowerInvariant();

            if (spaced != resourceType.ToLowerInvariant())
            {
                aliases.TryAdd(spaced, resourceType);
            }
        }

        // Common abbreviations and French/English aliases
        aliases.TryAdd("aca", AzureResourceTypes.ContainerApp);
        aliases.TryAdd("container app", AzureResourceTypes.ContainerApp);
        aliases.TryAdd("container apps", AzureResourceTypes.ContainerApp);
        aliases.TryAdd("acr", AzureResourceTypes.ContainerRegistry);
        aliases.TryAdd("docker registry", AzureResourceTypes.ContainerRegistry);
        aliases.TryAdd("docker", AzureResourceTypes.ContainerRegistry);
        aliases.TryAdd("law", AzureResourceTypes.LogAnalyticsWorkspace);
        aliases.TryAdd("log analytics", AzureResourceTypes.LogAnalyticsWorkspace);
        aliases.TryAdd("logs workspace", AzureResourceTypes.LogAnalyticsWorkspace);
        aliases.TryAdd("kv", AzureResourceTypes.KeyVault);
        aliases.TryAdd("key vault", AzureResourceTypes.KeyVault);
        aliases.TryAdd("keyvault", AzureResourceTypes.KeyVault);
        aliases.TryAdd("coffre", AzureResourceTypes.KeyVault);
        aliases.TryAdd("redis", AzureResourceTypes.RedisCache);
        aliases.TryAdd("cache redis", AzureResourceTypes.RedisCache);
        aliases.TryAdd("app insights", AzureResourceTypes.ApplicationInsights);
        aliases.TryAdd("appinsights", AzureResourceTypes.ApplicationInsights);
        aliases.TryAdd("cosmos", AzureResourceTypes.CosmosDb);
        aliases.TryAdd("cosmosdb", AzureResourceTypes.CosmosDb);
        aliases.TryAdd("sql", AzureResourceTypes.SqlServer);
        aliases.TryAdd("sql server", AzureResourceTypes.SqlServer);
        aliases.TryAdd("base de données", AzureResourceTypes.SqlServer);
        aliases.TryAdd("base sql", AzureResourceTypes.SqlServer);
        aliases.TryAdd("storage", AzureResourceTypes.StorageAccount);
        aliases.TryAdd("blob", AzureResourceTypes.StorageAccount);
        aliases.TryAdd("stockage", AzureResourceTypes.StorageAccount);
        aliases.TryAdd("service bus", AzureResourceTypes.ServiceBusNamespace);
        aliases.TryAdd("servicebus", AzureResourceTypes.ServiceBusNamespace);
        aliases.TryAdd("event hub", AzureResourceTypes.EventHubNamespace);
        aliases.TryAdd("eventhub", AzureResourceTypes.EventHubNamespace);
        aliases.TryAdd("identité managée", AzureResourceTypes.UserAssignedIdentity);
        aliases.TryAdd("managed identity", AzureResourceTypes.UserAssignedIdentity);
        aliases.TryAdd("uai", AzureResourceTypes.UserAssignedIdentity);
        aliases.TryAdd("cae", AzureResourceTypes.ContainerAppEnvironment);
        aliases.TryAdd("container app environment", AzureResourceTypes.ContainerAppEnvironment);
        aliases.TryAdd("app config", AzureResourceTypes.AppConfiguration);
        aliases.TryAdd("app configuration", AzureResourceTypes.AppConfiguration);
        aliases.TryAdd("function app", AzureResourceTypes.FunctionApp);
        aliases.TryAdd("azure function", AzureResourceTypes.FunctionApp);
        aliases.TryAdd("fonction", AzureResourceTypes.FunctionApp);
        aliases.TryAdd("web app", AzureResourceTypes.WebApp);
        aliases.TryAdd("webapp", AzureResourceTypes.WebApp);
        aliases.TryAdd("app service plan", AzureResourceTypes.AppServicePlan);
        aliases.TryAdd("plan", AzureResourceTypes.AppServicePlan);
        aliases.TryAdd("front door", AzureResourceTypes.FrontDoor);
        aliases.TryAdd("cdn", AzureResourceTypes.FrontDoor);

        return aliases;
    }

    /// <summary>Well-known field names used in drafts.</summary>
    internal static class DraftFieldNames
    {
        internal const string ProjectName = "projectName";
        internal const string LayoutPreset = "layoutPreset";
        internal const string Environments = "environments";
        internal const string ResourceNames = "resourceNames";
        internal const string ResourceGroupAssignments = "resourceGroupAssignments";
    }

    /// <inheritdoc />
    public bool RemoveDraft(string draftId)
    {
        lock (_syncRoot)
        {
            return _drafts.TryRemove(draftId, out _);
        }
    }

    /// <inheritdoc />
    public int EvictExpired(TimeSpan maxAge)
    {
        lock (_syncRoot)
        {
            var cutoff = DateTime.UtcNow - maxAge;
            var expired = _drafts
                .Where(kvp => kvp.Value.CreatedAtUtc < cutoff)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in expired)
            {
                _drafts.TryRemove(key, out _);
            }

            return expired.Count;
        }
    }

    private static string CreateDraftId()
    {
        return DraftIdPrefix + Guid.NewGuid().ToString("N")[..8];
    }
}

internal static class ProjectDraftWarnings
{
    internal const string DefaultEnvironmentWarning = "No environments specified - defaulting to a single 'Development' environment.";

    internal const string MissingSubscriptionWarning =
        "One or more environments have no subscription ID configured. This is allowed during project creation and can be configured later.";

    internal static List<string> Build(
        IReadOnlyCollection<DraftEnvironmentIntent>? environments,
        bool includeDefaultEnvironmentWarning)
    {
        var warnings = new List<string>();

        if (includeDefaultEnvironmentWarning)
        {
            warnings.Add(DefaultEnvironmentWarning);
        }

        if (environments is null || environments.Count == 0)
        {
            return warnings;
        }

        if (environments.Any(environment => environment.SubscriptionId == Guid.Empty))
        {
            warnings.Add(MissingSubscriptionWarning);
        }

        if (environments.Any(environment => string.Equals(environment.Location, Location.DefaultAzureRegionKey, StringComparison.OrdinalIgnoreCase)))
        {
            warnings.Add($"Default Azure region is '{Location.DefaultAzureRegionKey}'. You can change this later.");
        }

        return warnings;
    }
}
