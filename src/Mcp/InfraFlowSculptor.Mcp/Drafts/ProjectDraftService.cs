using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.GenerationCore;

namespace InfraFlowSculptor.Mcp.Drafts;

/// <summary>In-memory implementation of <see cref="IProjectDraftService"/> for the MCP session.</summary>
public sealed class ProjectDraftService : IProjectDraftService
{
    private static readonly TimeSpan RegexTimeout = TimeSpan.FromMilliseconds(250);

    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "avec", "en", "de", "du", "le", "la", "les", "un", "une", "des", "pour", "dans", "qui", "que",
        "the", "a", "an", "with", "in", "for", "and", "or", "from", "to", "that", "this",
    };

    private static readonly Dictionary<string, string> ResourceTypeAliases = BuildResourceTypeAliases();

    private readonly ConcurrentDictionary<string, ProjectCreationDraft> _drafts = new();

    /// <inheritdoc />
    public ProjectCreationDraft CreateDraftFromPrompt(string userPrompt)
    {
        var draftId = "draft_" + Guid.NewGuid().ToString("N")[..8];
        var intent = ParsePromptIntent(userPrompt);
        var missingFields = new List<string>();
        var clarificationQuestions = new List<DraftClarificationQuestion>();
        var warnings = new List<string>();

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
            warnings.Add("No environments specified â€” defaulting to a single 'Development' environment.");
        }

        if (intent.LayoutPreset is not null)
        {
            intent.Repositories = BuildDefaultRepositories(intent.LayoutPreset.Value);
        }

        if (intent.Environments.Any(e => e.SubscriptionId == Guid.Empty))
        {
            warnings.Add("One or more environments have no subscription ID configured.");
        }

        if (intent.Environments.Any(e => string.Equals(e.Location, "westeurope", StringComparison.OrdinalIgnoreCase)))
        {
            warnings.Add("Default Azure region is 'westeurope'. You can change this later.");
        }

        var status = missingFields.Count == 0 ? DraftStatus.ReadyToCreate : DraftStatus.RequiresClarification;

        var draft = new ProjectCreationDraft
        {
            DraftId = draftId,
            Status = status,
            MissingFields = missingFields,
            ClarificationQuestions = clarificationQuestions,
            Intent = intent,
            Warnings = warnings,
        };

        _drafts[draftId] = draft;
        return draft;
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

        if (overrides.AgentPoolName is not null)
        {
            intent.AgentPoolName = overrides.AgentPoolName;
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

        draft.MissingFields = missingFields;
        draft.Errors = errors;
        draft.ClarificationQuestions = clarificationQuestions;
        draft.Status = missingFields.Count == 0 && errors.Count == 0
            ? DraftStatus.ReadyToCreate
            : DraftStatus.RequiresClarification;
    }

    private static DraftProjectIntent ParsePromptIntent(string userPrompt)
    {
        return new DraftProjectIntent
        {
            ProjectName = ExtractProjectName(userPrompt),
            LayoutPreset = ExtractLayoutPreset(userPrompt),
            Resources = ExtractResourceTypes(userPrompt),
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

        var nameMatch = Regex.Match(prompt, @"\b(?:projet|project)\s+(\w+)", RegexOptions.IgnoreCase, RegexTimeout);
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

        return results;
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
                new DraftRepositoryIntent { Alias = "main", ContentKinds = ["Infrastructure", "Application"] },
            ],
            LayoutPresetEnum.SplitInfraCode =>
            [
                new DraftRepositoryIntent { Alias = "infra", ContentKinds = ["Infrastructure"] },
                new DraftRepositoryIntent { Alias = "app", ContentKinds = ["Application"] },
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

        return aliases;
    }

    /// <summary>Well-known field names used in drafts.</summary>
    internal static class DraftFieldNames
    {
        internal const string ProjectName = "projectName";
        internal const string LayoutPreset = "layoutPreset";
        internal const string Environments = "environments";
    }
}
