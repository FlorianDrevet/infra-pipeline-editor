# Push Bicep Files to Git Repository

> **Feature Proposal** — Push des fichiers Bicep générés directement sur un dépôt Git  
> Date : 2026-03-24  
> Status : Proposition technique et fonctionnelle

---

## Table des matières

1. [Résumé fonctionnel](#1-résumé-fonctionnel)
2. [Flux utilisateur (UX)](#2-flux-utilisateur-ux)
3. [Architecture technique](#3-architecture-technique)
4. [Modèle de domaine](#4-modèle-de-domaine)
5. [Sécurité — Gestion des secrets via Azure Key Vault](#5-sécurité--gestion-des-secrets-via-azure-key-vault)
6. [Couche Application (CQRS)](#6-couche-application-cqrs)
7. [Couche Infrastructure — Providers Git](#7-couche-infrastructure--providers-git)
8. [Couche Contracts (DTOs)](#8-couche-contracts-dtos)
9. [Couche API (Endpoints)](#9-couche-api-endpoints)
10. [Packages NuGet requis](#10-packages-nuget-requis)
11. [Migration EF Core](#11-migration-ef-core)
12. [Frontend (Angular)](#12-frontend-angular)
13. [Diagrammes de séquence](#13-diagrammes-de-séquence)
14. [Plan d'implémentation (étapes)](#14-plan-dimplémentation-étapes)
15. [Risques et considérations](#15-risques-et-considérations)

---

## 1. Résumé fonctionnel

### Besoin

Actuellement, les fichiers Bicep générés à partir d'une configuration d'infrastructure peuvent être :
- **Visualisés** dans le panneau terminal (fichier par fichier)
- **Téléchargés** en ZIP via `GET /generate-bicep/{configId}/download`

L'utilisateur souhaite une **3e option** : **pusher directement** les fichiers Bicep générés sur un dépôt Git (GitHub, Azure DevOps, GitLab), en créant automatiquement une branche et un commit.

### Comportement cible

1. L'utilisateur configure **une seule fois** la connexion Git au niveau du **Projet** (partagée par toutes les configurations du projet)
2. Lors de la génération Bicep, un bouton **"Push to Git"** apparaît à côté du bouton **"Download ZIP"**
3. Au clic, l'utilisateur choisit un nom de branche (pré-rempli avec un pattern comme `bicep/{configName}/{timestamp}`)
4. Le backend :
   - Récupère le PAT/token depuis Azure Key Vault
   - Crée une branche à partir de la branche par défaut du dépôt
   - Commit et push tous les fichiers Bicep générés
5. L'utilisateur reçoit un lien direct vers la branche créée (pour créer une PR)

### Périmètre

| In scope | Out of scope (V1) |
|----------|-------------------|
| Configuration Git au niveau Projet | Création automatique de Pull Request |
| Push sur GitHub (via API REST / Octokit) | Push sur GitLab |
| Push sur Azure DevOps (via API REST) | Webhook / CI trigger automatique |
| Stockage du PAT dans Azure Key Vault | OAuth2 app-to-app (GitHub App) |
| Création de branche + commit | Merge / rebase automatique |
| Arborescence `infra/{configName}/` dans le dépôt | Gestion de conflits |

---

## 2. Flux utilisateur (UX)

### 2.1 Configuration Git (une seule fois par projet)

```
Page Projet → Onglet "Settings" ou "Git Configuration"
┌─────────────────────────────────────────────────────┐
│ Git Repository Configuration                         │
│                                                      │
│  Provider:    [GitHub ▼]                             │
│  Repo URL:    [https://github.com/org/infra-repo]   │
│  Branch:      [main]                                 │
│  Base Path:   [bicep/] (optionnel)                   │
│                                                      │
│  ── Authentication ──                                │
│  Key Vault URL:  [https://myvault.vault.azure.net]   │
│  Secret Name:    [git-pat-infra-repo]                │
│                                                      │
│  [Test Connection]  [Save Configuration]             │
└─────────────────────────────────────────────────────┘
```

- **Provider** : GitHub ou AzureDevOps (extensible à GitLab en V2)
- **Repo URL** : URL HTTPS du dépôt cible
- **Default Branch** : branche de base pour créer les nouvelles branches (ex: `main`)
- **Base Path** : dossier racine dans le dépôt où seront pushés les fichiers (ex: `bicep/`, vide = racine)
- **Key Vault URL** : URL du coffre Azure Key Vault contenant le secret
- **Secret Name** : nom du secret dans le Key Vault (contient le PAT ou token)
- **Test Connection** : vérifie que le PAT est valide et que le dépôt est accessible

### 2.2 Push Bicep (à chaque génération)

```
Page Config Detail → Après génération Bicep réussie
┌──────────────────────────────────────────────────────┐
│ ✅ Bicep generated successfully                       │
│                                                       │
│  📂 types.bicep                                      │
│  📂 functions.bicep                                  │
│  📂 main.bicep                                       │
│  📂 parameters/                                      │
│  📂 modules/                                         │
│                                                       │
│  [📥 Download ZIP]   [🚀 Push to Git]               │
└──────────────────────────────────────────────────────┘

Clic sur "Push to Git" → Dialog:
┌──────────────────────────────────────────────┐
│ Push Bicep to Git Repository                  │
│                                               │
│  Repository: org/infra-repo                   │
│  Base branch: main                            │
│                                               │
│  Branch name:                                 │
│  [bicep/my-config/20260324-143022]            │
│                                               │
│  Commit message:                              │
│  [chore(bicep): generate infra for my-config] │
│                                               │
│            [Cancel]  [Push]                   │
└──────────────────────────────────────────────┘

Résultat:
┌──────────────────────────────────────────────┐
│ ✅ Files pushed successfully!                 │
│                                               │
│  Branch: bicep/my-config/20260324-143022      │
│  Commits: 1 (6 files)                         │
│                                               │
│  [🔗 View branch on GitHub]                  │
│  [🔗 Create Pull Request]                    │
└──────────────────────────────────────────────┘
```

---

## 3. Architecture technique

### 3.1 Vue d'ensemble

```
┌─────────────────────────────────────────────────────────────────────┐
│                         Frontend (Angular)                           │
│  Config Detail → Push Dialog → POST /generate-bicep/{id}/push-git  │
└──────────────────────────────┬──────────────────────────────────────┘
                               │
                               ▼
┌─────────────────────────────────────────────────────────────────────┐
│                        API Layer (Minimal API)                       │
│  BicepGenerationController.MapPost("/{id}/push-to-git")            │
└──────────────────────────────┬──────────────────────────────────────┘
                               │
                               ▼
┌─────────────────────────────────────────────────────────────────────┐
│                    Application Layer (MediatR)                       │
│  PushBicepToGitCommand → PushBicepToGitCommandHandler               │
│    1. Vérifie accès (IInfraConfigAccessService)                     │
│    2. Charge la config Git du projet                                │
│    3. Récupère les fichiers Bicep (DownloadBicep existant)          │
│    4. Appelle IGitProviderService.PushFilesAsync()                  │
└────────────────┬──────────────────────────┬─────────────────────────┘
                 │                          │
                 ▼                          ▼
┌────────────────────────┐   ┌────────────────────────────────────────┐
│   Azure Key Vault       │   │     IGitProviderService                │
│   (IKeyVaultClient)     │   │  ┌──────────────────────────────────┐ │
│                         │   │  │ GitHubGitProviderService         │ │
│  Récupère le PAT/token  │   │  │  (Octokit REST API)             │ │
│  par SecretName         │   │  ├──────────────────────────────────┤ │
│                         │   │  │ AzureDevOpsGitProviderService    │ │
│                         │   │  │  (Azure DevOps REST API)        │ │
│                         │   │  └──────────────────────────────────┘ │
└────────────────────────┘   └────────────────────────────────────────┘
```

### 3.2 Choix technique : API REST vs LibGit2Sharp

| Critère | LibGit2Sharp (clone local) | API REST (Octokit / HTTP) |
|---------|---------------------------|--------------------------|
| Clone nécessaire | ✅ Oui (disque local) | ❌ Non |
| Performance | Lent (clone complet ou shallow) | Rapide (appels API ciblés) |
| Dépendances natives | libgit2 (C natif, portabilité ⚠️) | HTTP uniquement |
| Docker / Linux | Souvent problématique (librairie native) | Aucun souci |
| Fichiers volumineux | Gère bien | Limité (GitHub API : 100 MB max/commit) |
| Complexité | Élevée (credentials, SSH, proxy) | Modérée (PAT dans header) |
| Adapté au use case | ❌ Overkill | ✅ Parfait (quelques fichiers Bicep) |

**Décision : API REST** (Octokit pour GitHub + HttpClient pour Azure DevOps).

Justification :
- Les fichiers Bicep sont de petite taille (quelques KB chacun, max ~50 fichiers)
- Pas besoin de clone : on crée une branche et commit via API
- Pas de dépendances natives : fonctionne en Docker Linux sans problème
- Plus simple à tester et à mocker

### 3.3 Choix technique : Octokit vs HttpClient direct pour GitHub

| Critère | Octokit.net | HttpClient direct |
|---------|------------|-------------------|
| Typage | ✅ Modèles typés | ❌ JSON manuel |
| Maintenance | Maintenu par GitHub | Self-maintained |
| Rate limiting | Géré automatiquement | À implémenter |
| API coverage | Complète | À la demande |
| Taille | ~2.5 MB | 0 (built-in) |

**Décision : Octokit.net** pour GitHub (SDK officiel, typé, rate-limiting intégré).
Pour Azure DevOps : **HttpClient** avec les DTOs nécessaires (le SDK Azure DevOps .NET est lourd et peu maintenu).

---

## 4. Modèle de domaine

### 4.1 Nouvelle entité : `GitRepositoryConfiguration`

```
Project (AggregateRoot)
├── GitRepositoryConfiguration?  ← NOUVEAU (optionnel, 0 ou 1)
│   ├── GitRepositoryConfigurationId (VO)
│   ├── ProviderType (enum: GitHub, AzureDevOps)
│   ├── RepositoryUrl (string)
│   ├── DefaultBranch (string, default "main")
│   ├── BasePath (string?, optionnel)
│   ├── KeyVaultUrl (string)
│   ├── SecretName (string)
│   └── Owner (string, ex: "org" ou "org/repo")
├── Members [...]
├── EnvironmentDefinitions [...]
└── ResourceNamingTemplates [...]
```

**Localisation du fichier** :  
`src/Api/InfraFlowSculptor.Domain/ProjectAggregate/Entities/GitRepositoryConfiguration.cs`

**Value objects** :
- `GitRepositoryConfigurationId` : `Id<GitRepositoryConfigurationId>` (GUID unique)
- `GitProviderType` : `EnumValueObject<GitProviderTypeEnum>` (GitHub, AzureDevOps)

```csharp
// src/Api/InfraFlowSculptor.Domain/ProjectAggregate/Entities/GitRepositoryConfiguration.cs
public sealed class GitRepositoryConfiguration : Entity<GitRepositoryConfigurationId>
{
    public GitProviderType ProviderType { get; private set; }
    public string RepositoryUrl { get; private set; }
    public string DefaultBranch { get; private set; }
    public string? BasePath { get; private set; }
    public string KeyVaultUrl { get; private set; }
    public string SecretName { get; private set; }

    // Extracted from RepositoryUrl for API calls
    public string Owner { get; private set; }
    public string RepositoryName { get; private set; }

    private GitRepositoryConfiguration() { }

    public static GitRepositoryConfiguration Create(
        GitProviderType providerType,
        string repositoryUrl,
        string defaultBranch,
        string? basePath,
        string keyVaultUrl,
        string secretName)
    { ... }

    public void Update(
        GitProviderType providerType,
        string repositoryUrl,
        string defaultBranch,
        string? basePath,
        string keyVaultUrl,
        string secretName)
    { ... }
}
```

### 4.2 Méthodes sur le Project aggregate

```csharp
// Project.cs — nouvelles méthodes
public ErrorOr<GitRepositoryConfiguration> SetGitRepositoryConfiguration(
    GitProviderType providerType,
    string repositoryUrl,
    string defaultBranch,
    string? basePath,
    string keyVaultUrl,
    string secretName)
{
    // Validation domaine : URL format, branch non vide, etc.
    // Crée ou met à jour _gitRepositoryConfiguration
}

public ErrorOr<Deleted> RemoveGitRepositoryConfiguration()
{
    // Supprime la configuration Git
}
```

### 4.3 Value Objects

```csharp
// src/Api/InfraFlowSculptor.Domain/ProjectAggregate/ValueObjects/GitRepositoryConfigurationId.cs
public sealed class GitRepositoryConfigurationId : Id<GitRepositoryConfigurationId>
{
    public GitRepositoryConfigurationId(Guid value) : base(value) { }
    public static GitRepositoryConfigurationId Create(Guid value) => new(value);
}

// src/Api/InfraFlowSculptor.Domain/ProjectAggregate/ValueObjects/GitProviderType.cs
public enum GitProviderTypeEnum
{
    GitHub,
    AzureDevOps
}

public sealed class GitProviderType : EnumValueObject<GitProviderTypeEnum>
{
    public GitProviderType(GitProviderTypeEnum value) : base(value) { }
}
```

### 4.4 Erreurs domaine

```csharp
// src/Api/InfraFlowSculptor.Domain/Common/Errors/Errors.GitRepository.cs
public static partial class Errors
{
    public static class GitRepository
    {
        public static Error NotConfigured() => Error.NotFound(
            "GitRepository.NotConfigured",
            "No Git repository configuration found for this project.");

        public static Error InvalidRepositoryUrl() => Error.Validation(
            "GitRepository.InvalidRepositoryUrl",
            "The repository URL format is invalid.");

        public static Error PushFailed(string reason) => Error.Failure(
            "GitRepository.PushFailed",
            $"Failed to push files to Git repository: {reason}");

        public static Error BranchAlreadyExists(string branchName) => Error.Conflict(
            "GitRepository.BranchAlreadyExists",
            $"Branch '{branchName}' already exists in the repository.");

        public static Error SecretRetrievalFailed() => Error.Failure(
            "GitRepository.SecretRetrievalFailed",
            "Failed to retrieve the authentication token from Key Vault.");

        public static Error ConnectionTestFailed(string reason) => Error.Failure(
            "GitRepository.ConnectionTestFailed",
            $"Git repository connection test failed: {reason}");
    }
}
```

---

## 5. Sécurité — Gestion des secrets via Azure Key Vault

### 5.1 Pourquoi Key Vault ?

Le PAT (Personal Access Token) est un secret sensible qui :
- Donne accès en écriture au dépôt Git
- Ne doit **jamais** être stocké en base de données (même chiffré)
- Ne doit **jamais** transiter dans les logs ou les réponses API
- Doit être **révocable** et **rotable** indépendamment de l'application

Azure Key Vault est le service Azure natif pour ce besoin. L'application stocke uniquement la **référence** au secret (vault URL + secret name), jamais le secret lui-même.

### 5.2 Prérequis Azure

1. **Azure Key Vault** créé et accessible par l'application
2. **Managed Identity** (System-assigned ou User-assigned) sur l'App Service / Container App hébergeant l'API
3. **RBAC Key Vault** : attribuer le rôle `Key Vault Secrets User` à la Managed Identity de l'application
4. **Secret créé** dans le Key Vault avec le PAT comme valeur

### 5.3 Flux de récupération du secret

```
Application                    Azure Key Vault
    │                               │
    │  1. DefaultAzureCredential    │
    │  (Managed Identity en prod,   │
    │   Azure CLI en dev)           │
    │                               │
    │  2. GET secret/{secretName}   │
    ├──────────────────────────────►│
    │                               │
    │  3. Secret value (PAT)        │
    │◄──────────────────────────────┤
    │                               │
    │  4. Use PAT for Git API call  │
    ├──────────────────────────────►│ GitHub/AzureDevOps
```

### 5.4 Interface Key Vault

```csharp
// src/Api/InfraFlowSculptor.Application/Common/Interfaces/Services/IKeyVaultSecretClient.cs
public interface IKeyVaultSecretClient
{
    /// <summary>
    /// Retrieves a secret value from Azure Key Vault.
    /// </summary>
    /// <param name="vaultUrl">The Key Vault URL (e.g., https://myvault.vault.azure.net).</param>
    /// <param name="secretName">The name of the secret to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The secret value, or an error if retrieval fails.</returns>
    Task<ErrorOr<string>> GetSecretAsync(
        string vaultUrl,
        string secretName,
        CancellationToken cancellationToken = default);
}
```

### 5.5 Implémentation Key Vault

```csharp
// src/Api/InfraFlowSculptor.Infrastructure/Services/KeyVault/KeyVaultSecretClient.cs
public sealed class KeyVaultSecretClient(TokenCredential credential, ILogger<KeyVaultSecretClient> logger)
    : IKeyVaultSecretClient
{
    public async Task<ErrorOr<string>> GetSecretAsync(
        string vaultUrl, string secretName, CancellationToken cancellationToken)
    {
        try
        {
            var client = new SecretClient(new Uri(vaultUrl), credential);
            var response = await client.GetSecretAsync(secretName, cancellationToken: cancellationToken);
            return response.Value.Value;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve secret {SecretName} from {VaultUrl}", secretName, vaultUrl);
            return Errors.GitRepository.SecretRetrievalFailed();
        }
    }
}
```

**Note** : On crée un `SecretClient` par appel car chaque projet peut utiliser un Key Vault différent. Le `TokenCredential` est injecté via `DefaultAzureCredential` (résout automatiquement Managed Identity en prod, Azure CLI en dev).

### 5.6 Packages NuGet nécessaires

```xml
<PackageVersion Include="Azure.Security.KeyVault.Secrets" Version="4.7.0" />
<PackageVersion Include="Azure.Identity" Version="1.14.2" />
```

### 5.7 Configuration locale (développement)

En local, `DefaultAzureCredential` utilise :
1. **Azure CLI** : `az login` suffit
2. **Visual Studio / Rider** : authentification intégrée à l'IDE
3. **Variables d'environnement** : `AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, `AZURE_CLIENT_SECRET`

L'utilisateur doit avoir le rôle `Key Vault Secrets User` sur le Key Vault utilisé.

---

## 6. Couche Application (CQRS)

### 6.1 Commande : `SetProjectGitConfigCommand`

```
src/Api/InfraFlowSculptor.Application/Projects/Commands/SetProjectGitConfig/
├── SetProjectGitConfigCommand.cs
├── SetProjectGitConfigCommandHandler.cs
└── SetProjectGitConfigCommandValidator.cs
```

```csharp
public record SetProjectGitConfigCommand(
    Guid ProjectId,
    string ProviderType,       // "GitHub" ou "AzureDevOps"
    string RepositoryUrl,
    string DefaultBranch,
    string? BasePath,
    string KeyVaultUrl,
    string SecretName
) : IRequest<ErrorOr<Unit>>;
```

**Handler** :
1. Vérifie l'accès Owner via `IProjectAccessService.VerifyOwnerAccessAsync()`
2. Charge le Project avec `IProjectRepository.GetByIdWithAllAsync()`
3. Appelle `project.SetGitRepositoryConfiguration(...)`
4. Persiste via `IProjectRepository.UpdateAsync()`

### 6.2 Commande : `RemoveProjectGitConfigCommand`

```
src/Api/InfraFlowSculptor.Application/Projects/Commands/RemoveProjectGitConfig/
├── RemoveProjectGitConfigCommand.cs
├── RemoveProjectGitConfigCommandHandler.cs
└── RemoveProjectGitConfigCommandValidator.cs
```

### 6.3 Commande : `TestGitConnectionCommand`

```
src/Api/InfraFlowSculptor.Application/Projects/Commands/TestGitConnection/
├── TestGitConnectionCommand.cs
├── TestGitConnectionCommandHandler.cs
└── TestGitConnectionCommandValidator.cs
```

```csharp
public record TestGitConnectionCommand(
    Guid ProjectId
) : IRequest<ErrorOr<TestGitConnectionResult>>;

public record TestGitConnectionResult(
    bool Success,
    string? RepositoryFullName,
    string? DefaultBranch,
    string? ErrorMessage);
```

**Handler** :
1. Charge la config Git du projet
2. Récupère le PAT via `IKeyVaultSecretClient`
3. Appelle `IGitProviderService.TestConnectionAsync()`
4. Retourne le résultat

### 6.4 Commande : `PushBicepToGitCommand`

```
src/Api/InfraFlowSculptor.Application/InfrastructureConfig/Commands/PushBicepToGit/
├── PushBicepToGitCommand.cs
├── PushBicepToGitCommandHandler.cs
└── PushBicepToGitCommandValidator.cs
```

```csharp
public record PushBicepToGitCommand(
    Guid InfrastructureConfigId,
    string BranchName,
    string CommitMessage
) : IRequest<ErrorOr<PushBicepToGitResult>>;

public record PushBicepToGitResult(
    string BranchName,
    string BranchUrl,
    string CommitSha,
    int FileCount,
    string? PullRequestUrl);  // null en V1, future: lien de création PR
```

**Handler** — Flux principal :

```csharp
public sealed class PushBicepToGitCommandHandler(
    IInfraConfigAccessService accessService,
    IInfrastructureConfigRepository infraConfigRepo,
    IProjectRepository projectRepo,
    IBlobService blobService,
    IKeyVaultSecretClient keyVaultClient,
    IGitProviderFactory gitProviderFactory,
    ILogger<PushBicepToGitCommandHandler> logger)
    : IRequestHandler<PushBicepToGitCommand, ErrorOr<PushBicepToGitResult>>
{
    public async Task<ErrorOr<PushBicepToGitResult>> Handle(
        PushBicepToGitCommand request, CancellationToken ct)
    {
        // 1. Vérifier l'accès écriture
        var authResult = await accessService.VerifyWriteAccessAsync(
            new InfrastructureConfigId(request.InfrastructureConfigId), ct);
        if (authResult.IsError) return authResult.Errors;

        // 2. Charger la config + le project
        var config = await infraConfigRepo.GetByIdAsync(
            new InfrastructureConfigId(request.InfrastructureConfigId), ct);
        if (config is null) return Errors.InfrastructureConfig.NotFoundError();

        var project = await projectRepo.GetByIdWithAllAsync(config.ProjectId, ct);
        if (project is null) return Errors.Project.NotFound();

        // 3. Vérifier que la config Git existe
        if (project.GitRepositoryConfiguration is null)
            return Errors.GitRepository.NotConfigured();

        var gitConfig = project.GitRepositoryConfiguration;

        // 4. Récupérer le PAT depuis Key Vault
        var secretResult = await keyVaultClient.GetSecretAsync(
            gitConfig.KeyVaultUrl, gitConfig.SecretName, ct);
        if (secretResult.IsError) return secretResult.Errors;

        // 5. Récupérer les fichiers Bicep depuis Blob Storage (dernière génération)
        var files = await GetLatestBicepFilesAsync(config.Id.Value, ct);
        if (files.IsError) return files.Errors;

        // 6. Résoudre le provider Git + push
        var gitProvider = gitProviderFactory.Create(gitConfig.ProviderType);
        var pushResult = await gitProvider.PushFilesAsync(new GitPushRequest
        {
            Token = secretResult.Value,
            Owner = gitConfig.Owner,
            RepositoryName = gitConfig.RepositoryName,
            BaseBranch = gitConfig.DefaultBranch,
            NewBranchName = request.BranchName,
            CommitMessage = request.CommitMessage,
            BasePath = gitConfig.BasePath,
            Files = files.Value
        }, ct);

        return pushResult;
    }
}
```

### 6.5 Service interfaces

```csharp
// src/Api/InfraFlowSculptor.Application/Common/Interfaces/Services/IGitProviderService.cs
public interface IGitProviderService
{
    /// <summary>
    /// Tests the connection to the Git repository.
    /// </summary>
    Task<ErrorOr<TestGitConnectionResult>> TestConnectionAsync(
        string token, string owner, string repositoryName, CancellationToken ct);

    /// <summary>
    /// Creates a branch and pushes files to the Git repository.
    /// </summary>
    Task<ErrorOr<PushBicepToGitResult>> PushFilesAsync(
        GitPushRequest request, CancellationToken ct);
}

// src/Api/InfraFlowSculptor.Application/Common/Interfaces/Services/IGitProviderFactory.cs
public interface IGitProviderFactory
{
    IGitProviderService Create(GitProviderType providerType);
}

// src/Api/InfraFlowSculptor.Application/Common/Models/GitPushRequest.cs
public sealed record GitPushRequest
{
    public required string Token { get; init; }
    public required string Owner { get; init; }
    public required string RepositoryName { get; init; }
    public required string BaseBranch { get; init; }
    public required string NewBranchName { get; init; }
    public required string CommitMessage { get; init; }
    public string? BasePath { get; init; }
    public required IReadOnlyDictionary<string, string> Files { get; init; }
}
```

---

## 7. Couche Infrastructure — Providers Git

### 7.1 GitHub Provider (Octokit) — Create or Update Branch

```csharp
// src/Api/InfraFlowSculptor.Infrastructure/Services/GitProviders/GitHubGitProviderService.cs
```

**Flux GitHub API — Create or Update** :

L'implémentation supporte à la fois la **création d'une nouvelle branche** et la **mise à jour d'une branche existante** (push successifs sur la même branche). Le flux est le suivant :

```
1. GET /repos/{owner}/{repo}/git/ref/heads/{branchName}
   → Tente de récupérer la branche cible
   → Si 404 : branche nouvelle → utiliser le SHA de la branche de base
   → Si 200 : branche existante → utiliser le SHA du HEAD actuel

2. POST /repos/{owner}/{repo}/git/trees
   → Crée un arbre Git avec tous les fichiers Bicep
   Body: { base_tree: parentSha, tree: [
     { path: "bicep/main.bicep", mode: "100644", type: "blob", content: "..." },
     ...
   ]}

3. POST /repos/{owner}/{repo}/git/commits
   → Crée un commit pointant vers le nouvel arbre
   Body: { message: "...", tree: treeSha, parents: [parentSha] }

4a. (Nouvelle branche) POST /repos/{owner}/{repo}/git/refs
    → Crée la branche pointant vers le commit
    Body: { ref: "refs/heads/{branchName}", sha: commitSha }

4b. (Branche existante) PATCH /repos/{owner}/{repo}/git/refs/heads/{branchName}
    → Met à jour la branche pour pointer vers le nouveau commit (fast-forward)
    Body: { sha: commitSha }
```

Cette approche utilise l'API **Git Data (low-level)** qui permet de créer un commit avec N fichiers en **3-4 appels API** seulement.

**Avantage** : l'utilisateur peut réutiliser la même branche (ex: `bicep/my-config`) indéfiniment, accumulant des commits au fil des regenerations. L'historique Git reste propre et traçable.

### 7.2 Azure DevOps Provider (REST API) — Create or Update Branch

```csharp
// src/Api/InfraFlowSculptor.Infrastructure/Services/GitProviders/AzureDevOpsGitProviderService.cs
```

**Flux Azure DevOps REST API — Create or Update** :

```
1. GET /{org}/{project}/_apis/git/repositories/{repo}/refs?filter=heads/{branchName}
   → Tente de récupérer la branche cible
   → Si vide : branche nouvelle → oldObjectId = "0000000000000000000000000000000000000000"
   → Si trouvée : branche existante → oldObjectId = SHA actuel, changeType = "edit"

2. POST /{org}/{project}/_apis/git/repositories/{repo}/pushes
   → Crée ou met à jour la branche + commit en un seul appel API
   Body: {
     refUpdates: [{ 
       name: "refs/heads/{branchName}", 
       oldObjectId: "<zéros ou SHA existant>"
     }],
     commits: [{
       comment: "...",
       changes: [
         { changeType: "add|edit", item: { path: "/bicep/main.bicep" }, 
           newContent: { content: "...", contentType: "rawtext" } },
         ...
       ]
     }]
   }
```

L'API Azure DevOps gère les deux cas (nouvelle branche / branche existante) en **1 seul appel**. Pour une branche existante, les fichiers utilisent `changeType: "edit"` au lieu de `"add"`.

### 7.3 Factory

```csharp
// src/Api/InfraFlowSculptor.Infrastructure/Services/GitProviders/GitProviderFactory.cs
public sealed class GitProviderFactory(
    GitHubGitProviderService gitHub,
    AzureDevOpsGitProviderService azureDevOps)
    : IGitProviderFactory
{
    public IGitProviderService Create(GitProviderType providerType) =>
        providerType.Value switch
        {
            GitProviderTypeEnum.GitHub => gitHub,
            GitProviderTypeEnum.AzureDevOps => azureDevOps,
            _ => throw new ArgumentOutOfRangeException(nameof(providerType))
        };
}
```

### 7.4 Registration DI

```csharp
// Dans Infrastructure/DependencyInjection.cs
services.AddSingleton<TokenCredential>(_ => new DefaultAzureCredential());
services.AddSingleton<IKeyVaultSecretClient, KeyVaultSecretClient>();
services.AddSingleton<GitHubGitProviderService>();
services.AddSingleton<AzureDevOpsGitProviderService>();
services.AddSingleton<IGitProviderFactory, GitProviderFactory>();
```

---

## 8. Couche Contracts (DTOs)

### 8.1 Requests

```csharp
// src/Api/InfraFlowSculptor.Contracts/Projects/Requests/SetGitConfigRequest.cs
public sealed class SetGitConfigRequest
{
    [Required]
    [EnumValidation(typeof(GitProviderTypeEnum))]
    public required string ProviderType { get; init; }

    [Required]
    [Url]
    public required string RepositoryUrl { get; init; }

    [Required]
    public required string DefaultBranch { get; init; }

    public string? BasePath { get; init; }

    [Required]
    [Url]
    public required string KeyVaultUrl { get; init; }

    [Required]
    public required string SecretName { get; init; }
}

// src/Api/InfraFlowSculptor.Contracts/InfrastructureConfig/Requests/PushBicepToGitRequest.cs
public sealed class PushBicepToGitRequest
{
    [Required]
    public required string BranchName { get; init; }

    [Required]
    public required string CommitMessage { get; init; }
}
```

### 8.2 Responses

```csharp
// src/Api/InfraFlowSculptor.Contracts/Projects/Responses/GitConfigResponse.cs
public record GitConfigResponse(
    string ProviderType,
    string RepositoryUrl,
    string DefaultBranch,
    string? BasePath,
    string KeyVaultUrl,
    string SecretName);
// NOTE : jamais le PAT/token dans la réponse !

// src/Api/InfraFlowSculptor.Contracts/InfrastructureConfig/Responses/PushBicepToGitResponse.cs
public record PushBicepToGitResponse(
    string BranchName,
    string BranchUrl,
    string CommitSha,
    int FileCount,
    string? PullRequestUrl);

// src/Api/InfraFlowSculptor.Contracts/Projects/Responses/TestGitConnectionResponse.cs
public record TestGitConnectionResponse(
    bool Success,
    string? RepositoryFullName,
    string? DefaultBranch,
    string? ErrorMessage);
```

### 8.3 Mise à jour du ProjectResponse

```csharp
// Ajouter dans ProjectResponse existant
public record ProjectResponse(
    string Id,
    string Name,
    string? Description,
    // ... membres existants ...
    GitConfigResponse? GitRepositoryConfiguration  // ← NOUVEAU
);
```

---

## 9. Couche API (Endpoints)

### 9.1 Nouveaux endpoints sur ProjectController

```csharp
// PUT /projects/{projectId}/git-config
group.MapPut("/{projectId:guid}/git-config", async (
    Guid projectId,
    [FromBody] SetGitConfigRequest request,
    ISender sender) =>
{
    var command = request.Adapt<SetProjectGitConfigCommand>() with { ProjectId = projectId };
    var result = await sender.Send(command);
    return result.Match(_ => Results.NoContent(), errors => errors.ToErrorResult());
}).WithName("SetProjectGitConfig");

// DELETE /projects/{projectId}/git-config
group.MapDelete("/{projectId:guid}/git-config", async (
    Guid projectId,
    ISender sender) =>
{
    var result = await sender.Send(new RemoveProjectGitConfigCommand(projectId));
    return result.Match(_ => Results.NoContent(), errors => errors.ToErrorResult());
}).WithName("RemoveProjectGitConfig");

// POST /projects/{projectId}/git-config/test
group.MapPost("/{projectId:guid}/git-config/test", async (
    Guid projectId,
    ISender sender) =>
{
    var result = await sender.Send(new TestGitConnectionCommand(projectId));
    return result.Match(
        value => Results.Ok(value.Adapt<TestGitConnectionResponse>()),
        errors => errors.ToErrorResult());
}).WithName("TestGitConnection");
```

### 9.2 Nouvel endpoint sur BicepGenerationController

```csharp
// POST /generate-bicep/{configId}/push-to-git
group.MapPost("/{configId:guid}/push-to-git", async (
    Guid configId,
    [FromBody] PushBicepToGitRequest request,
    ISender sender) =>
{
    var command = new PushBicepToGitCommand(configId, request.BranchName, request.CommitMessage);
    var result = await sender.Send(command);
    return result.Match(
        value => Results.Ok(value.Adapt<PushBicepToGitResponse>()),
        errors => errors.ToErrorResult());
}).WithName("PushBicepToGit");
```

### 9.3 Récapitulatif des endpoints

| Méthode | Route | Description | Auth |
|---------|-------|-------------|------|
| PUT | `/projects/{id}/git-config` | Configurer le dépôt Git | Owner |
| DELETE | `/projects/{id}/git-config` | Supprimer la config Git | Owner |
| POST | `/projects/{id}/git-config/test` | Tester la connexion | Owner/Contributor |
| POST | `/generate-bicep/{id}/push-to-git` | Push les fichiers Bicep | Contributor+ |

---

## 10. Packages NuGet requis

| Package | Version | Usage | Projet cible |
|---------|---------|-------|-------------|
| `Octokit` | 14.x | GitHub REST API (typé, rate-limiting) | Infrastructure |
| `Azure.Security.KeyVault.Secrets` | 4.7.x | Lecture de secrets Key Vault | Infrastructure |
| `Azure.Identity` | 1.14.x | `DefaultAzureCredential` pour auth Key Vault | Infrastructure |

**Impact sur `Directory.Packages.props`** :

```xml
<PackageVersion Include="Octokit" Version="14.0.0" />
<PackageVersion Include="Azure.Security.KeyVault.Secrets" Version="4.7.0" />
<PackageVersion Include="Azure.Identity" Version="1.14.2" />
```

**Note** : `Azure.Identity` pourrait déjà être une dépendance transitive de certains packages Aspire. Vérifier avant d'ajouter.

---

## 11. Migration EF Core

### 11.1 Changements de schéma

```sql
-- Nouvelle table
CREATE TABLE "GitRepositoryConfigurations" (
    "Id"              uuid        NOT NULL PRIMARY KEY,
    "ProjectId"       uuid        NOT NULL UNIQUE,  -- 1:1 avec Projects
    "ProviderType"    text        NOT NULL,          -- "GitHub" | "AzureDevOps"
    "RepositoryUrl"   text        NOT NULL,
    "DefaultBranch"   text        NOT NULL DEFAULT 'main',
    "BasePath"        text        NULL,
    "KeyVaultUrl"     text        NOT NULL,
    "SecretName"      text        NOT NULL,
    "Owner"           text        NOT NULL,
    "RepositoryName"  text        NOT NULL,
    CONSTRAINT "FK_GitRepositoryConfigurations_Projects_ProjectId"
        FOREIGN KEY ("ProjectId") REFERENCES "Projects" ("Id") ON DELETE CASCADE
);

CREATE UNIQUE INDEX "IX_GitRepositoryConfigurations_ProjectId"
    ON "GitRepositoryConfigurations" ("ProjectId");
```

### 11.2 EF Core Configuration

```csharp
// src/Api/InfraFlowSculptor.Infrastructure/Persistence/Configurations/GitRepositoryConfigurationConfiguration.cs
public sealed class GitRepositoryConfigurationConfiguration 
    : IEntityTypeConfiguration<GitRepositoryConfiguration>
{
    public void Configure(EntityTypeBuilder<GitRepositoryConfiguration> builder)
    {
        builder.ToTable("GitRepositoryConfigurations");
        builder.HasKey(x => x.Id);
        builder.ConfigureEntityId<GitRepositoryConfiguration, GitRepositoryConfigurationId>();
        
        builder.Property(x => x.ProviderType)
            .HasConversion(new EnumValueConverter<GitProviderType, GitProviderTypeEnum>())
            .IsRequired();

        builder.Property(x => x.RepositoryUrl).IsRequired();
        builder.Property(x => x.DefaultBranch).IsRequired().HasDefaultValue("main");
        builder.Property(x => x.BasePath);
        builder.Property(x => x.KeyVaultUrl).IsRequired();
        builder.Property(x => x.SecretName).IsRequired();
        builder.Property(x => x.Owner).IsRequired();
        builder.Property(x => x.RepositoryName).IsRequired();
    }
}
```

### 11.3 Relation dans ProjectConfiguration

```csharp
// Ajouter dans ProjectConfiguration.Configure()
builder.HasOne(p => p.GitRepositoryConfiguration)
    .WithOne()
    .HasForeignKey<GitRepositoryConfiguration>("ProjectId")
    .OnDelete(DeleteBehavior.Cascade);
```

### 11.4 Commande migration

```bash
dotnet ef migrations add AddGitRepositoryConfiguration \
    --project src/Api/InfraFlowSculptor.Infrastructure \
    --startup-project src/Api/InfraFlowSculptor.Api
```

---

## 12. Frontend (Angular)

### 12.1 Interfaces TypeScript

```typescript
// src/Front/src/app/shared/interfaces/project.interface.ts (mise à jour)
export interface GitConfigResponse {
  providerType: string;         // "GitHub" | "AzureDevOps"
  repositoryUrl: string;
  defaultBranch: string;
  basePath: string | null;
  keyVaultUrl: string;
  secretName: string;
}

export interface SetGitConfigRequest {
  providerType: string;
  repositoryUrl: string;
  defaultBranch: string;
  basePath?: string;
  keyVaultUrl: string;
  secretName: string;
}

export interface TestGitConnectionResponse {
  success: boolean;
  repositoryFullName: string | null;
  defaultBranch: string | null;
  errorMessage: string | null;
}

// src/Front/src/app/shared/interfaces/bicep-generator.interface.ts (mise à jour)
export interface PushBicepToGitRequest {
  branchName: string;
  commitMessage: string;
}

export interface PushBicepToGitResponse {
  branchName: string;
  branchUrl: string;
  commitSha: string;
  fileCount: number;
  pullRequestUrl: string | null;
}
```

### 12.2 Enum

```typescript
// src/Front/src/app/features/project-detail/enums/git-provider-type.enum.ts
export enum GitProviderTypeEnum {
  GitHub = 'GitHub',
  AzureDevOps = 'AzureDevOps'
}

export const GIT_PROVIDER_OPTIONS = [
  { label: 'GitHub', value: GitProviderTypeEnum.GitHub, icon: 'code' },
  { label: 'Azure DevOps', value: GitProviderTypeEnum.AzureDevOps, icon: 'cloud' }
];
```

### 12.3 Services

```typescript
// Ajouts dans ProjectService
setGitConfig(projectId: string, request: SetGitConfigRequest): Promise<void>
removeGitConfig(projectId: string): Promise<void>
testGitConnection(projectId: string): Promise<TestGitConnectionResponse>

// Ajouts dans BicepGeneratorService
pushToGit(configId: string, request: PushBicepToGitRequest): Promise<PushBicepToGitResponse>
```

### 12.4 Composants

| Composant | Emplacement | Usage |
|-----------|-------------|-------|
| `git-config-dialog` | `features/project-detail/git-config-dialog/` | Dialog configuration Git |
| `push-to-git-dialog` | `features/config-detail/push-to-git-dialog/` | Dialog push avec choix branche/message |

### 12.5 i18n

Nouvelles clés à ajouter dans `fr.json` et `en.json` :

```
PROJECT_DETAIL.GIT_CONFIG.*      (titre, labels, actions, erreurs)
CONFIG_DETAIL.BICEP.PUSH_TO_GIT  (bouton)
CONFIG_DETAIL.BICEP.PUSH_DIALOG.*(dialog push)
CONFIG_DETAIL.BICEP.PUSH_SUCCESS.*(résultat push)
```

---

## 13. Diagrammes de séquence

### 13.1 Configuration Git (première fois)

```
User          Frontend         API              Handler          KeyVault         Git API
 │               │              │                 │                │               │
 │  Form Git     │              │                 │                │               │
 │──────────────>│              │                 │                │               │
 │               │  PUT /git    │                 │                │               │
 │               │─────────────>│                 │                │               │
 │               │              │  SetGitConfig   │                │               │
 │               │              │────────────────>│                │               │
 │               │              │                 │  Update Project│               │
 │               │              │                 │───────┐        │               │
 │               │              │                 │<──────┘        │               │
 │               │              │  204 No Content │                │               │
 │               │<─────────────│<────────────────│                │               │
 │               │              │                 │                │               │
 │  Test Conn    │              │                 │                │               │
 │──────────────>│              │                 │                │               │
 │               │ POST /test   │                 │                │               │
 │               │─────────────>│                 │                │               │
 │               │              │  TestConnection │                │               │
 │               │              │────────────────>│                │               │
 │               │              │                 │  Get Secret    │               │
 │               │              │                 │───────────────>│               │
 │               │              │                 │  PAT           │               │
 │               │              │                 │<───────────────│               │
 │               │              │                 │  GET /repos    │               │
 │               │              │                 │────────────────────────────────>│
 │               │              │                 │  200 OK (repo info)            │
 │               │              │                 │<────────────────────────────────│
 │               │  200 { ok }  │                 │                │               │
 │<──────────────│<─────────────│<────────────────│                │               │
```

### 13.2 Push Bicep to Git

```
User          Frontend         API              Handler          KeyVault    BlobStorage    Git API
 │               │              │                 │                │            │              │
 │  Push to Git  │              │                 │                │            │              │
 │──────────────>│              │                 │                │            │              │
 │               │  POST /push  │                 │                │            │              │
 │               │─────────────>│                 │                │            │              │
 │               │              │  PushBicepCmd   │                │            │              │
 │               │              │────────────────>│                │            │              │
 │               │              │                 │  Verify access │            │              │
 │               │              │                 │───────┐        │            │              │
 │               │              │                 │<──────┘        │            │              │
 │               │              │                 │  Get PAT       │            │              │
 │               │              │                 │───────────────>│            │              │
 │               │              │                 │  PAT value     │            │              │
 │               │              │                 │<───────────────│            │              │
 │               │              │                 │  List blobs    │            │              │
 │               │              │                 │───────────────────────────>│              │
 │               │              │                 │  Download files│            │              │
 │               │              │                 │───────────────────────────>│              │
 │               │              │                 │  files content │            │              │
 │               │              │                 │<───────────────────────────│              │
 │               │              │                 │  Create branch + commit     │              │
 │               │              │                 │──────────────────────────────────────────>│
 │               │              │                 │  branch URL + SHA                         │
 │               │              │                 │<──────────────────────────────────────────│
 │               │  200 result  │                 │                │            │              │
 │<──────────────│<─────────────│<────────────────│                │            │              │
```

---

## 14. Plan d'implémentation (étapes)

### Phase 1 — Backend Core (domaine + infrastructure)

| # | Tâche | Fichiers |
|---|-------|----------|
| 1 | Value Objects : `GitRepositoryConfigurationId`, `GitProviderType` | Domain/ProjectAggregate/ValueObjects/ |
| 2 | Entité : `GitRepositoryConfiguration` | Domain/ProjectAggregate/Entities/ |
| 3 | Erreurs : `Errors.GitRepository` | Domain/Common/Errors/ |
| 4 | Méthodes sur `Project` : Set/Remove GitConfig | Domain/ProjectAggregate/Project.cs |
| 5 | Config EF Core : `GitRepositoryConfigurationConfiguration` | Infrastructure/Persistence/Configurations/ |
| 6 | Migration : `AddGitRepositoryConfiguration` | Infrastructure/Migrations/ |
| 7 | Mise à jour `ProjectConfiguration` (relation 1:1) | Infrastructure/Persistence/Configurations/ |
| 8 | Mise à jour `ProjectRepository` (Include GitConfig) | Infrastructure/Persistence/Repositories/ |
| 9 | Interface `IKeyVaultSecretClient` | Application/Common/Interfaces/Services/ |
| 10 | Interface `IGitProviderService` + `IGitProviderFactory` | Application/Common/Interfaces/Services/ |
| 11 | Modèle `GitPushRequest` | Application/Common/Models/ |
| 12 | Packages NuGet : Octokit, Azure.Identity, Azure.Security.KeyVault.Secrets | Directory.Packages.props, Infrastructure.csproj |

### Phase 2 — Implémentations infrastructure

| # | Tâche | Fichiers |
|---|-------|----------|
| 13 | `KeyVaultSecretClient` | Infrastructure/Services/KeyVault/ |
| 14 | `GitHubGitProviderService` | Infrastructure/Services/GitProviders/ |
| 15 | `AzureDevOpsGitProviderService` | Infrastructure/Services/GitProviders/ |
| 16 | `GitProviderFactory` | Infrastructure/Services/GitProviders/ |
| 17 | Registration DI | Infrastructure/DependencyInjection.cs |

### Phase 3 — CQRS Commands + Contracts + API

| # | Tâche | Fichiers |
|---|-------|----------|
| 18 | `SetProjectGitConfigCommand` + handler + validator | Application/Projects/Commands/ |
| 19 | `RemoveProjectGitConfigCommand` + handler | Application/Projects/Commands/ |
| 20 | `TestGitConnectionCommand` + handler | Application/Projects/Commands/ |
| 21 | `PushBicepToGitCommand` + handler + validator | Application/InfrastructureConfig/Commands/ |
| 22 | Contracts (requests + responses) | Contracts/Projects/, Contracts/InfrastructureConfig/ |
| 23 | Mapster mapping configs | Api/Common/Mapping/ |
| 24 | Endpoints API (ProjectController, BicepGenerationController) | Api/Controllers/ |
| 25 | Mise à jour `ProjectResponse` | Contracts/Projects/Responses/ |

### Phase 4 — Frontend

| # | Tâche | Fichiers |
|---|-------|----------|
| 26 | Interfaces TypeScript | Front/src/app/shared/interfaces/ |
| 27 | Enum `GitProviderTypeEnum` | Front/src/app/features/project-detail/enums/ |
| 28 | Service methods (ProjectService, BicepGeneratorService) | Front/src/app/shared/services/ |
| 29 | Git config dialog (project-detail) | Front/src/app/features/project-detail/git-config-dialog/ |
| 30 | Push to Git dialog (config-detail) | Front/src/app/features/config-detail/push-to-git-dialog/ |
| 31 | Bouton "Push to Git" dans Bicep terminal panel | Front/src/app/features/config-detail/ |
| 32 | i18n (FR + EN) | Front/public/i18n/ |

### Phase 5 — Validation

| # | Tâche |
|---|-------|
| 33 | `dotnet build .\InfraFlowSculptor.slnx` (0 erreurs) |
| 34 | `npm run typecheck` + `npm run build` dans `src/Front` |
| 35 | Test E2E : configurer Git → générer Bicep → push → vérifier branche sur GitHub |

---

## 15. Risques et considérations

### 15.1 Sécurité

| Risque | Mitigation |
|--------|-----------|
| PAT exposed en log | Structured logging sans le token ; `ILogger` never logs `request.Token` |
| PAT exposé en réponse API | `GitConfigResponse` renvoie le secret name, jamais la valeur |
| Injection dans branch name | Validation regex : `^[a-zA-Z0-9/_.-]+$` (pas de `..`, pas de caractères spéciaux) |
| SSRF via repository URL | Validation URL stricte (HTTPS uniquement, domaines autorisés : `github.com`, `dev.azure.com`) |
| PAT avec droits excessifs | Documentation : recommandation de créer un PAT avec scope minimal (`repo` pour GitHub, `Code (Read & Write)` pour ADO) |

### 15.2 Limites GitHub API

| Limite | Valeur | Impact |
|--------|--------|--------|
| Rate limit (PAT) | 5 000 req/h | Largement suffisant (3-4 appels par push) |
| Taille de fichier | 100 MB par blob via API | Aucun risque (Bicep = quelques KB) |
| Nombre de fichiers par commit | Pas de limite documentée | OK |
| Taille de l'arbre | 100 000 entrées | Aucun risque |

### 15.3 Limites Azure DevOps API

| Limite | Valeur | Impact |
|--------|--------|--------|
| Rate limit | 200 req/5 min par utilisateur | Suffisant (1 appel par push) |
| Taille push | 5 GB max | Aucun risque |

### 15.4 Évolutions futures (V2)

| Feature | Effort estimé |
|---------|--------------|
| Création automatique de Pull Request | Faible (1 appel API supplémentaire) |
| Support GitLab | Moyen (nouveau provider + API) |
| OAuth2 GitHub App (sans PAT) | Élevé (flux OAuth2, refresh tokens, app registration) |
| Webhook pour notifier CI/CD | Faible (1 appel POST) |
| Historique des pushs | Moyen (nouvelle table, liste API) |
| Diff avant push (comparaison avec le dépôt) | Élevé |

---

## Annexe A — Scopes recommandés pour les PAT

### GitHub Personal Access Token (Fine-grained)

| Permission | Scope | Raison |
|-----------|-------|--------|
| Contents | Read and write | Créer/modifier des fichiers |
| Metadata | Read-only | Lire les infos du dépôt |

### Azure DevOps Personal Access Token

| Scope | Raison |
|-------|--------|
| Code (Read & Write) | Créer des branches et pusher du code |

---

## Annexe B — Extraction Owner/Repo depuis l'URL

### GitHub

```
Input:  https://github.com/FlorianDrevet/infra-pipeline-editor
Output: Owner = "FlorianDrevet", Repo = "infra-pipeline-editor"

Regex: ^https?://github\.com/(?<owner>[^/]+)/(?<repo>[^/]+?)(?:\.git)?$
```

### Azure DevOps

```
Input:  https://dev.azure.com/org/project/_git/repo-name
Output: Organization = "org", Project = "project", Repo = "repo-name"

Regex: ^https?://dev\.azure\.com/(?<org>[^/]+)/(?<project>[^/]+)/_git/(?<repo>[^/]+?)(?:\.git)?$
```

---

## Annexe C — Structure des fichiers pushés

```
{basePath}/                          ← configurable (ex: "bicep/", ou vide = racine)
├── {configName}/                    ← nom de la configuration
│   ├── types.bicep                  ← types exportés (EnvironmentName, EnvironmentVariables)
│   ├── functions.bicep              ← fonctions de nommage
│   ├── main.bicep                   ← orchestrateur principal
│   ├── parameters/
│   │   ├── main.dev.bicepparam      ← paramètres per-environment
│   │   ├── main.staging.bicepparam
│   │   └── main.prod.bicepparam
│   └── modules/
│       ├── KeyVault/
│       │   ├── keyVault.module.bicep
│       │   └── types.bicep
│       ├── StorageAccount/
│       │   ├── storageAccount.module.bicep
│       │   └── types.bicep
│       └── ...
```
