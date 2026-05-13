Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repo = "FlorianDrevet/infra-pipeline-editor"
$auditSource = "audits/audit-13-05-2026.md"
$auditDate = "2026-05"

# Ensure labels exist
$requiredLabels = @(
    @{ name = "audit"; color = "0052CC"; description = "Issue generated from automated code audits" }
    @{ name = "audit: 2026-05"; color = "0052CC"; description = "Findings from May 2026 audit" }
    @{ name = "status: new"; color = "1D76DB"; description = "Finding introduced by the latest audit run" }
    @{ name = "severity: critical"; color = "B60205"; description = "Critical severity finding" }
    @{ name = "severity: high"; color = "D93F0B"; description = "High severity finding" }
    @{ name = "severity: medium"; color = "FBCA04"; description = "Medium severity finding" }
    @{ name = "severity: low"; color = "0E8A16"; description = "Low severity finding" }
    @{ name = "area: security"; color = "D93F0B"; description = "Security domain" }
    @{ name = "area: database"; color = "5319E7"; description = "Database / EF Core domain" }
    @{ name = "area: domain"; color = "006B75"; description = "Domain / DDD domain" }
    @{ name = "area: application"; color = "1D76DB"; description = "Application / CQRS domain" }
    @{ name = "area: api"; color = "0075CA"; description = "API / Contracts domain" }
    @{ name = "area: generation"; color = "D4C5F9"; description = "Bicep / Pipeline generation domain" }
    @{ name = "area: architecture"; color = "BFD4F2"; description = "Architecture transversale" }
    @{ name = "area: tests"; color = "C2E0C6"; description = "Tests / coverage domain" }
    @{ name = "area: frontend"; color = "F9D0C4"; description = "Angular frontend domain" }
    @{ name = "area: mcp"; color = "EDEDED"; description = "MCP server domain" }
    @{ name = "phase: 0-foundation"; color = "B60205"; description = "Phase 0 - Foundation" }
    @{ name = "phase: 1-security"; color = "D93F0B"; description = "Phase 1 - Security and domain integrity" }
    @{ name = "phase: 2-database"; color = "FBCA04"; description = "Phase 2 - Quality and maintainability" }
    @{ name = "phase: 3-domain"; color = "0E8A16"; description = "Phase 3 - Backlog and hygiene" }
    @{ name = "type: bug"; color = "D73A4A"; description = "Something is broken" }
    @{ name = "type: performance"; color = "F9D0C4"; description = "Performance improvement" }
    @{ name = "type: refactor"; color = "A2EEEF"; description = "Code refactoring" }
    @{ name = "type: tech-debt"; color = "D4C5F9"; description = "Technical debt" }
)

Write-Host "Ensuring labels exist..."
foreach ($label in $requiredLabels) {
    $existing = gh label list --repo $repo --search $label.name --json name 2>$null | ConvertFrom-Json
    $found = $false
    if ($existing) {
        foreach ($l in $existing) {
            if ($l.name -eq $label.name) { $found = $true; break }
        }
    }
    if (-not $found) {
        Write-Host "  Creating label: $($label.name)"
        gh label create $label.name --repo $repo --color $label.color --description $label.description --force
    } else {
        Write-Host "  Label exists: $($label.name)"
    }
}

# Define all findings
$findings = @(
    # CRITICAL
    @{ id="SEC-007"; title="Global exception handler ne loggue pas l'exception"; severity="critical"; area="security"; phase="0-foundation"; type="bug"; firstSeen="audit-23-04-2026.md"
       body="Le handler UseExceptionHandler retourne un ProblemDetails 500 generique mais ne loggue pas l'exception. En production, impossible de diagnostiquer des erreurs non capturees.`n`nFichiers: src/Api/InfraFlowSculptor.Api/Errors/ErrorHandling.cs`n`nRecommandation: Injecter ILogger, LogError(exception), inclure Activity.Current?.Id dans Extensions[traceId]." }
    @{ id="DOM-002"; title="AzureResource proprietes publiques mutables"; severity="critical"; area="domain"; phase="1-security"; type="bug"; firstSeen="audit-14-04-2026"
       body="AzureResource.cs expose ResourceGroupId, Name, Location, CustomNameOverride, ResourceGroup en get/set public. Aucun invariant de domaine garanti.`n`nFichiers: src/Api/InfraFlowSculptor.Domain/Common/BaseModels/AzureResource.cs`n`nRecommandation: private set + methodes de domaine (Rename, MoveToResourceGroup, OverrideName)." }

    # HIGH
    @{ id="SEC-008"; title="UserProvisioningMiddleware court-circuite UnitOfWork"; severity="high"; area="security"; phase="1-security"; type="refactor"; firstSeen="audit-23-04-2026.md"
       body="Le middleware injecte ProjectDbContext directement et appelle SaveChangesAsync contournant l'UnitOfWork. Race condition possible sur premiere auth concurrente.`n`nFichiers: src/Api/InfraFlowSculptor.Api/Common/UserProvisioningMiddleware.cs`n`nRecommandation: Migrer vers IRequest MediatR ou IUserProvisioningService cote Application avec gestion de conflit UNIQUE." }
    @{ id="DB-001"; title="HasMaxLength manquant sur colonnes string restantes"; severity="high"; area="database"; phase="1-security"; type="tech-debt"; firstSeen="audit-14-04-2026"
       body="Couverture HasMaxLength largement amelioree mais subsistent des colonnes sans contrainte sur RoleAssignment.RoleDefinitionId et certains champs enum convertis en string.`n`nRecommandation: Audit final colonne par colonne, scanner automatique en test." }
    @{ id="DB-003"; title="AsNoTracking absent dans GetAllAsync"; severity="high"; area="database"; phase="1-security"; type="performance"; firstSeen="audit-14-04-2026"
       body="GetByIdReadOnlyAsync utilise AsNoTracking mais GetAllAsync ne le fait toujours pas - toutes les listes payent le change tracking.`n`nFichiers: src/Api/InfraFlowSculptor.Infrastructure/Persistence/Repositories/BaseRepository.cs`n`nRecommandation: Ajouter AsNoTracking() dans GetAllAsync ou creer GetAllReadOnlyAsync." }
    @{ id="DB-007"; title="Duplication d'Include dans les repositories"; severity="high"; area="database"; phase="2-database"; type="refactor"; firstSeen="audit-23-04-2026.md"
       body="Les memes chaines Include sont repetees dans 3-4 methodes par repository (StorageAccount, CosmosDb, ServiceBusNamespace).`n`nRecommandation: Extraire private static IQueryable<T> WithSubResources(IQueryable<T> q)." }
    @{ id="DOM-003"; title="Collections mutables exposees"; severity="high"; area="domain"; phase="1-security"; type="refactor"; firstSeen="audit-14-04-2026"
       body="CorsRule et divers sous-agregats AzureResource exposent des collections mutables.`n`nRecommandation: private readonly List<T> _x = []; public IReadOnlyCollection<T> X => _x;" }
    @{ id="DOM-005"; title="AddDependency n'empeche ni cycles ni self-deps"; severity="high"; area="domain"; phase="1-security"; type="bug"; firstSeen="audit-23-04-2026.md"
       body="La methode AddDependency ne verifie pas les cycles ni les self-dependencies.`n`nRecommandation: Garde if (dependency.Id == Id) throw ... + IResourceDependencyValidator en Application." }
    @{ id="DOM-006"; title="Factories Create non uniformisees"; severity="high"; area="domain"; phase="1-security"; type="refactor"; firstSeen="audit-23-04-2026.md"
       body="Les agregats n'ont pas tous une factory Create statique uniforme. Certains constructeurs sont publics sans validation.`n`nRecommandation: Forcer static Create(...) sur tous les agregats, ctor parameterless protected." }
    @{ id="APP-003"; title="N+1 residuel ListCrossConfigReferencesQueryHandler"; severity="high"; area="application"; phase="1-security"; type="performance"; firstSeen="audit-23-04-2026.md"
       body="ListCrossConfigReferencesQueryHandler charge les configs une par une.`n`nFichiers: src/Api/InfraFlowSculptor.Application/InfrastructureConfig/Queries/ListCrossConfigReferences/ListCrossConfigReferencesQueryHandler.cs`n`nRecommandation: IInfrastructureConfigRepository.GetByIdsAsync(IEnumerable<InfrastructureConfigId>)." }
    @{ id="APP-005"; title="God-handlers depassent 300 lignes"; severity="high"; area="application"; phase="2-database"; type="refactor"; firstSeen="audit-23-04-2026.md"
       body="GenerateProjectBootstrapPipelineCommandHandler (380 lignes), PushProjectArtifactsToMultiRepoCommandHandler (319), AddAppConfigurationKeyCommandHandler (305), AddAppSettingCommandHandler (305).`n`nRecommandation: Extraire orchestrators. Plafond souple 150 lignes." }
    @{ id="APP-008"; title="Duplication de logique de mapping Generate*"; severity="high"; area="application"; phase="2-database"; type="refactor"; firstSeen="audit-23-04-2026.md"
       body="La logique de mapping dans les handlers Generate* est dupliquee.`n`nRecommandation: IGenerationContextBuilder centralise." }
    @{ id="API-003"; title="Route {id} sans contrainte :guid sur certains endpoints"; severity="high"; area="api"; phase="3-domain"; type="bug"; firstSeen="audit-23-04-2026.md"
       body="Certains endpoints acceptent {id} sans contrainte de type :guid.`n`nRecommandation: Audit des routes, contraindre tous les Guid." }
    @{ id="GEN-001"; title="MainBicepAssembler 928 lignes"; severity="high"; area="generation"; phase="2-database"; type="refactor"; firstSeen="audit-23-04-2026.md"
       body="MainBicepAssembler.cs fait 928 lignes. BicepGenerationEngine a ete reduit (1088 a 143) mais l'assembler reste enorme.`n`nFichiers: src/Api/InfraFlowSculptor.BicepGeneration/Assemblers/MainBicepAssembler.cs`n`nRecommandation: Decomposer en OutputAssembler, ParameterAssembler, ModuleDeclarationAssembler." }
    @{ id="GEN-002"; title="Generateurs depassent 500 lignes (god classes)"; severity="high"; area="generation"; phase="2-database"; type="refactor"; firstSeen="audit-23-04-2026.md"
       body="AppPipelineTemplatesGenerator (948), ContainerAppTypeBicepGenerator (828), FunctionAppTypeBicepGenerator (652), WebAppTypeBicepGenerator (633), PipelineGenerationEngine (594), StorageAccountTypeBicepGenerator (538).`n`nRecommandation: Extraire en sous-composants par section." }
    @{ id="GEN-003"; title="Pas de validation bicep build en CI"; severity="high"; area="generation"; phase="2-database"; type="tech-debt"; firstSeen="audit-23-04-2026.md"
       body="Aucune step CI ne valide les fichiers Bicep generes avec bicep build.`n`nRecommandation: CI step bicep build sur tous les fichiers generes a partir des fixtures." }
    @{ id="ARCH-001"; title="Api -> DbContext direct (UserProvisioningMiddleware)"; severity="high"; area="architecture"; phase="1-security"; type="refactor"; firstSeen="audit-23-04-2026.md"
       body="Dependance directe de la couche Api vers DbContext via UserProvisioningMiddleware. Meme probleme que SEC-008/APP-010.`n`nRecommandation: Migrer vers la couche Application." }
    @{ id="TEST-001"; title="Pas de collecte de coverage (Coverlet)"; severity="high"; area="tests"; phase="0-foundation"; type="tech-debt"; firstSeen="audits/audit-13-05-2026.md"
       body="10 projets de test existent avec ~46000 lignes. Mais aucune collecte de code coverage n'est configuree. Impossible de mesurer le taux reel.`n`nRecommandation: Ajouter Coverlet + seuils dans CI (target: 60% Domain, 40% Application)." }
    @{ id="FRONT-001"; title="resource-edit.component.ts : 3838 lignes"; severity="high"; area="frontend"; phase="2-database"; type="refactor"; firstSeen="audits/audit-13-05-2026.md"
       body="God component massif gerant 18 types de ressources Azure dans un seul composant avec des branches switch/if.`n`nFichiers: src/Front/src/app/features/resource-edit/resource-edit.component.ts`n`nRecommandation: Extraire un composant par type de ressource orchestre par un composant routeur." }
    @{ id="FRONT-002"; title="config-detail.component.ts : 2201 lignes"; severity="high"; area="frontend"; phase="2-database"; type="refactor"; firstSeen="audits/audit-13-05-2026.md"
       body="God component gerant la vue detail d'une configuration infrastructure avec toutes les sections inline.`n`nFichiers: src/Front/src/app/features/config-detail/config-detail.component.ts`n`nRecommandation: Extraire en sous-composants par section." }
    @{ id="MCP-001"; title="MCP sans rate limiting ni security headers"; severity="high"; area="mcp"; phase="0-foundation"; type="bug"; firstSeen="audits/audit-13-05-2026.md"
       body="Le serveur MCP expose un endpoint HTTP authentifie par PAT mais sans rate limiting, sans security headers, sans CORS policy.`n`nFichiers: src/Mcp/InfraFlowSculptor.Mcp/Program.cs`n`nRecommandation: Reutiliser SecurityHeadersMiddleware et RateLimitingServiceCollectionExtensions de l'API principale." }
    @{ id="MCP-002"; title="MCP tools sans CancellationToken"; severity="high"; area="mcp"; phase="0-foundation"; type="bug"; firstSeen="audits/audit-13-05-2026.md"
       body="Les methodes [McpServerTool] async envoient des commandes MediatR sans passer de CancellationToken. Si le client MCP deconnecte, les commandes continuent.`n`nFichiers: src/Mcp/InfraFlowSculptor.Mcp/Tools/`n`nRecommandation: Ajouter CancellationToken aux tool methods et propager au mediator.Send." }

    # MEDIUM
    @{ id="SEC-009"; title="Health checks sans rate limit"; severity="medium"; area="security"; phase="3-domain"; type="tech-debt"; firstSeen="audit-23-04-2026.md"
       body="/health et /alive sont publics sans rate limit. Utilisable comme oracle (database up/down) et vecteur de micro-DoS.`n`nRecommandation: Ajouter RequireRateLimiting(HealthChecks) avec policy dediee (30 req/min par IP)." }
    @{ id="SEC-010"; title="Politique IsAdmin magic string"; severity="medium"; area="security"; phase="3-domain"; type="refactor"; firstSeen="audit-23-04-2026.md"
       body="AddPolicy(IsAdmin, ...) avec literal non centralise.`n`nFichiers: src/Api/InfraFlowSculptor.Api/Program.cs`n`nRecommandation: Extraire dans AuthorizationPolicyNames.IsAdmin / AppRoles.Admin." }
    @{ id="SEC-011"; title="PAT SaveChangesAsync sur chaque requete"; severity="medium"; area="security"; phase="3-domain"; type="performance"; firstSeen="audits/audit-13-05-2026.md"
       body="Le PAT handler effectue SaveChangesAsync pour RecordUsage() a chaque requete authentifiee.`n`nFichiers: src/Api/InfraFlowSculptor.Infrastructure/Auth/PersonalAccessTokenAuthenticationHandler.cs`n`nRecommandation: Deporter RecordUsage() dans un background channel ou batcher." }
    @{ id="DB-008"; title="73 migrations sans squash"; severity="medium"; area="database"; phase="3-domain"; type="tech-debt"; firstSeen="audit-23-04-2026.md"
       body="73 migrations empilees sans squash. Impact sur le temps de migration et la lisibilite.`n`nRecommandation: Squash avant premiere mise en prod. Convention: max 1 migration par sprint." }
    @{ id="DB-009"; title="BaseRepository.AddAsync simule l'async via Task.FromResult"; severity="medium"; area="database"; phase="3-domain"; type="refactor"; firstSeen="audit-23-04-2026.md"
       body="AddAsync et UpdateAsync utilisent Task.FromResult sans aucun appel async reel.`n`nFichiers: src/Api/InfraFlowSculptor.Infrastructure/Persistence/Repositories/BaseRepository.cs`n`nRecommandation: Rendre synchrone (Add, Update) ou aligner sur vrai async avec CancellationToken." }
    @{ id="DB-011"; title="Pas de convention EF partagee (BaseEntityConfiguration)"; severity="medium"; area="database"; phase="3-domain"; type="tech-debt"; firstSeen="audit-23-04-2026.md"
       body="69 configurations libres. Pas de scanner d'enum/value objects.`n`nRecommandation: ApplyConfigurationsFromAssembly + conventions communes." }
    @{ id="DB-012"; title="Pas de ExecuteDeleteAsync sur operations bulk"; severity="medium"; area="database"; phase="3-domain"; type="performance"; firstSeen="audit-23-04-2026.md"
       body="Les operations Remove en boucle ne profitent pas des bulk operations EF Core.`n`nRecommandation: Migrer vers ExecuteDeleteAsync." }
    @{ id="DB-014"; title="Pas de QueryFilter global pour soft-delete / multi-tenant"; severity="medium"; area="database"; phase="3-domain"; type="tech-debt"; firstSeen="audit-23-04-2026.md"
       body="Aucun HasQueryFilter global prepare pour soft-delete ou multi-tenant.`n`nRecommandation: Preparer via HasQueryFilter no-op + interface ISoftDeletable." }
    @{ id="DOM-008"; title="Discriminator ResourceType gere cote Infrastructure"; severity="medium"; area="domain"; phase="3-domain"; type="refactor"; firstSeen="audit-23-04-2026.md"
       body="Le discriminator ResourceType (string) est gere cote Infrastructure, pas domaine.`n`nRecommandation: VO ResourceTypeName ou shadow property EF." }
    @{ id="DOM-010"; title="IsExisting mutable via protected set sans methode de domaine"; severity="medium"; area="domain"; phase="3-domain"; type="refactor"; firstSeen="audit-23-04-2026.md"
       body="IsExisting est modifiable via protected set sans methode de domaine.`n`nRecommandation: init ou private set + factory." }
    @{ id="DOM-011"; title="Pas d'IDomainEvent ni de IRaiseDomainEvents"; severity="medium"; area="domain"; phase="3-domain"; type="tech-debt"; firstSeen="audit-23-04-2026.md"
       body="Aucun mecanisme de domain events implemente.`n`nRecommandation: _domainEvents + dispatch via MediatR INotification post-SaveChanges." }
    @{ id="APP-009"; title="Behaviors non hierarchises explicitement"; severity="medium"; area="application"; phase="3-domain"; type="tech-debt"; firstSeen="audit-23-04-2026.md"
       body="L'ordre d'execution des pipeline behaviors MediatR n'est pas garanti par test.`n`nRecommandation: Test d'integration verifiant l'ordre." }
    @{ id="APP-010"; title="UserProvisioningMiddleware dans Application layer"; severity="medium"; area="application"; phase="1-security"; type="refactor"; firstSeen="audit-23-04-2026.md"
       body="Le middleware de provisioning utilisateur contourne la couche Application. Meme finding que SEC-008 et ARCH-001.`n`nRecommandation: Migrer vers la couche Application." }
    @{ id="APP-012"; title="Validators en duplication structurelle"; severity="medium"; area="application"; phase="3-domain"; type="refactor"; firstSeen="audit-23-04-2026.md"
       body="143 validators existent mais le boilerplate est repetitif.`n`nRecommandation: Extraire EntityCommandValidator<T>." }
    @{ id="API-005"; title="Response DTOs Guid vs string verification"; severity="medium"; area="api"; phase="3-domain"; type="refactor"; firstSeen="audit-23-04-2026.md"
       body="Convention projet: les Response DTOs doivent retourner des IDs string. Verifier la conformite complete.`n`nRecommandation: Confirmer que tous les Response DTOs retournent des IDs string." }
    @{ id="API-006"; title="Magic strings de routes"; severity="medium"; area="api"; phase="3-domain"; type="refactor"; firstSeen="audit-23-04-2026.md"
       body="Les routes inline dans les controllers ne sont pas centralisees.`n`nRecommandation: static class Routes { ... }." }
    @{ id="GEN-004"; title="Manipulation textuelle fragile (regex) pour patcher Bicep"; severity="medium"; area="generation"; phase="3-domain"; type="refactor"; firstSeen="audit-23-04-2026.md"
       body="Le pattern fondamental reste de la manipulation textuelle par regex pour generer le Bicep.`n`nRecommandation: AST/IR Bicep type a terme." }
    @{ id="GEN-005"; title="Duplication entre chemins de generation mono/multi repo"; severity="medium"; area="generation"; phase="3-domain"; type="refactor"; firstSeen="audit-23-04-2026.md"
       body="Logique dupliquee entre les chemins de generation mono-repo et multi-repo.`n`nRecommandation: GenerateInternal + delegue de pruning." }
    @{ id="GEN-006"; title="Magic strings ARM dispersees"; severity="medium"; area="generation"; phase="3-domain"; type="refactor"; firstSeen="audit-23-04-2026.md"
       body="Des identifiants ARM sont encore hardcodes en dehors de AzureResourceTypes.`n`nRecommandation: Centraliser via ArmResourceTypes constants." }
    @{ id="ARCH-003"; title="GenerationCore ownership floue"; severity="medium"; area="architecture"; phase="2-database"; type="refactor"; firstSeen="audit-23-04-2026.md"
       body="La couche GenerationCore n'a pas d'ownership claire entre contracts et engine.`n`nRecommandation: Isoler GenerationCore.Contracts vs GenerationCore.Engine." }
    @{ id="ARCH-004"; title="TreatWarningsAsErrors absent"; severity="medium"; area="architecture"; phase="3-domain"; type="tech-debt"; firstSeen="audit-23-04-2026.md"
       body="Aucun TreatWarningsAsErrors global dans le solution.`n`nRecommandation: <TreatWarningsAsErrors>true</TreatWarningsAsErrors> dans Directory.Build.props." }
    @{ id="ARCH-005"; title="Aucun analyseur Roslyn enabled"; severity="medium"; area="architecture"; phase="3-domain"; type="tech-debt"; firstSeen="audit-23-04-2026.md"
       body="Pas d'analyseur Roslyn configure au niveau solution.`n`nRecommandation: AnalysisLevel = latest-all." }
    @{ id="TEST-002"; title="Pas de tests integration EF Testcontainers"; severity="medium"; area="tests"; phase="3-domain"; type="tech-debt"; firstSeen="audit-23-04-2026.md"
       body="Infrastructure.Tests ne contient pas de tests avec Testcontainers PostgreSQL. Les tests repository sont probablement bases sur InMemory.`n`nRecommandation: Ajouter Testcontainers PostgreSQL." }
    @{ id="TEST-003"; title="Tests frontend absents"; severity="medium"; area="tests"; phase="3-domain"; type="tech-debt"; firstSeen="audits/audit-13-05-2026.md"
       body="83 composants Angular, 0 fichier *.spec.ts observe, pas de commande ng test dans les scripts CI.`n`nRecommandation: Ajouter au minimum des tests pour les composants critiques." }
    @{ id="FRONT-003"; title="add-resource-dialog.component.ts : 1662 lignes"; severity="medium"; area="frontend"; phase="3-domain"; type="refactor"; firstSeen="audits/audit-13-05-2026.md"
       body="Composant de dialogue d'ajout de ressource trop volumineux.`n`nFichiers: src/Front/src/app/features/resource-edit/add-resource-dialog.component.ts`n`nRecommandation: Extraire les formulaires par type de ressource en sous-composants." }
    @{ id="FRONT-004"; title="project-detail.component.ts : 1444 lignes"; severity="medium"; area="frontend"; phase="3-domain"; type="refactor"; firstSeen="audits/audit-13-05-2026.md"
       body="Composant project-detail trop volumineux.`n`nFichiers: src/Front/src/app/features/project-detail/project-detail.component.ts`n`nRecommandation: Extraire les sections (download, generation, members, settings)." }
    @{ id="MCP-003"; title="Drafts in-memory sans limite de taille"; severity="medium"; area="mcp"; phase="3-domain"; type="tech-debt"; firstSeen="audits/audit-13-05-2026.md"
       body="IProjectDraftService et IImportPreviewService sont Singleton avec des stores in-memory. Pas de limite de taille du dictionnaire.`n`nFichiers: src/Mcp/InfraFlowSculptor.Mcp/Drafts/, src/Mcp/InfraFlowSculptor.Mcp/Imports/`n`nRecommandation: Ajouter un MaxDraftCount configurable avec rejet 429 au-dela." }
    @{ id="MCP-004"; title="MCP reference directement Api project"; severity="medium"; area="mcp"; phase="2-database"; type="refactor"; firstSeen="audits/audit-13-05-2026.md"
       body="Le projet MCP a une ProjectReference vers InfraFlowSculptor.Api - toute la couche API est embarquee dans le processus MCP.`n`nFichiers: src/Mcp/InfraFlowSculptor.Mcp/InfraFlowSculptor.Mcp.csproj`n`nRecommandation: Extraire les parties partagees dans un projet commun." }

    # LOW
    @{ id="SEC-012"; title="PAT scope/permission model absent"; severity="low"; area="security"; phase="3-domain"; type="tech-debt"; firstSeen="audits/audit-13-05-2026.md"
       body="Les PAT n'ont aucun scope (read, write, admin). Tout token valide donne un acces complet via MCP.`n`nRecommandation: Ajouter un modele de scopes (read, write, generate) et les mapper en claims/policies." }
    @{ id="DB-015"; title="Conversions Guid? repetees dans 8+ configurations"; severity="low"; area="database"; phase="3-domain"; type="refactor"; firstSeen="audit-23-04-2026.md"
       body="Les conversions Guid nullable sont repetees dans 8+ fichiers de configuration.`n`nRecommandation: NullableIdValueConverter<TId> reutilisable." }
    @{ id="DB-016"; title="Pas de ConcurrencyCheck sur agregats"; severity="low"; area="database"; phase="3-domain"; type="tech-debt"; firstSeen="audit-23-04-2026.md"
       body="Aucun mecanisme de concurrence optimiste (xmin ou RowVersion).`n`nRecommandation: xmin PostgreSQL ou RowVersion sur Project, InfrastructureConfig, AzureResource." }
    @{ id="DB-017"; title="Pas de transaction explicite multi-agregats"; severity="low"; area="database"; phase="3-domain"; type="tech-debt"; firstSeen="audit-23-04-2026.md"
       body="Pas de documentation de la strategie transactionnelle pour les operations multi-agregats.`n`nRecommandation: Documenter la strategie transactionnelle." }
    @{ id="DOM-012"; title="ValueObject GetEqualityComponents non verifiable"; severity="low"; area="domain"; phase="3-domain"; type="tech-debt"; firstSeen="audit-23-04-2026.md"
       body="Pas d'exigence statique sur GetEqualityComponents.`n`nRecommandation: Test via reflexion en CI." }
    @{ id="DOM-013"; title="ResourceType string primitive obsession"; severity="low"; area="domain"; phase="3-domain"; type="refactor"; firstSeen="audit-23-04-2026.md"
       body="ResourceType est un string non encapsule.`n`nRecommandation: VO ResourceTypeName ou enum." }
    @{ id="APP-013"; title="Mapping Mapster workaround eparpille"; severity="low"; area="application"; phase="3-domain"; type="tech-debt"; firstSeen="audit-23-04-2026.md"
       body="Les workarounds Mapster sont disperses sans centralisation.`n`nRecommandation: Centraliser + TypeAdapterConfig.Compile() au demarrage." }
    @{ id="APP-014"; title="CancellationToken non propage sur certains handlers"; severity="low"; area="application"; phase="3-domain"; type="tech-debt"; firstSeen="audit-23-04-2026.md"
       body="Certains handlers ne propagent pas le CancellationToken.`n`nRecommandation: Activer CA2016 comme erreur." }
    @{ id="API-008"; title="Aucun endpoint ne retourne Cache-Control / ETag"; severity="low"; area="api"; phase="3-domain"; type="tech-debt"; firstSeen="audit-23-04-2026.md"
       body="Aucun mecanisme de cache HTTP implemente.`n`nRecommandation: ResponseCaching + ETag sur les Get frequents." }
    @{ id="API-009"; title="Mapping dans la couche Api"; severity="low"; area="api"; phase="3-domain"; type="refactor"; firstSeen="audit-23-04-2026.md"
       body="Du mapping est effectue directement dans la couche Api au lieu de la couche Application.`n`nRecommandation: Mapping cote Application uniquement." }
    @{ id="GEN-007"; title="CancellationToken absent dans pipeline de generation"; severity="low"; area="generation"; phase="3-domain"; type="tech-debt"; firstSeen="audit-23-04-2026.md"
       body="Le CancellationToken n'est pas propage jusqu'aux generateurs.`n`nRecommandation: Propager CancellationToken." }
    @{ id="ARCH-007"; title="Pas d'integration Azure Key Vault en configuration prod"; severity="low"; area="architecture"; phase="3-domain"; type="tech-debt"; firstSeen="audit-23-04-2026.md"
       body="Les secrets de production ne sont pas charges depuis Azure Key Vault.`n`nRecommandation: AddAzureKeyVault en prod." }
    @{ id="TEST-004"; title="Pas de snapshot tests DTOs"; severity="low"; area="tests"; phase="3-domain"; type="tech-debt"; firstSeen="audits/audit-13-05-2026.md"
       body="Contracts.Tests existe (26 fichiers) mais pas de snapshot tests pour detecter les breaking changes API.`n`nRecommandation: Ajouter des snapshot tests (Verify)." }
    @{ id="FRONT-005"; title="Aucun test frontend"; severity="low"; area="frontend"; phase="2-database"; type="tech-debt"; firstSeen="audits/audit-13-05-2026.md"
       body="83 composants Angular sans un seul test. Lie a TEST-003.`n`nRecommandation: Commencer par les composants critiques (project-detail, resource-edit)." }
    @{ id="MCP-005"; title="MCP listen HTTP sans TLS warning"; severity="low"; area="mcp"; phase="3-domain"; type="tech-debt"; firstSeen="audits/audit-13-05-2026.md"
       body="ListenUrl = http://127.0.0.1:5258 - localhost OK pour dev, mais en production le PAT transiterait en clair sans TLS.`n`nFichiers: src/Mcp/InfraFlowSculptor.Mcp/Common/McpOptions.cs`n`nRecommandation: Documenter que la production DOIT utiliser HTTPS. Ajouter un warning au demarrage." }
)

Write-Host "`nCreating $($findings.Count) issues..."
$created = 0
$failed = 0

foreach ($f in $findings) {
    $title = "[$($f.id)] $($f.title)"
    $labels = @("audit", "audit: $auditDate", "severity: $($f.severity)", "area: $($f.area)", "phase: $($f.phase)", "type: $($f.type)", "status: new")
    $labelsJoined = $labels -join ","

    $bodyContent = "<!-- audit-finding-id: $($f.id) -->`n<!-- audit-source: $auditSource -->`n<!-- audit-first-seen: $($f.firstSeen) -->`n<!-- audit-last-seen: $auditSource -->`n`n$($f.body)"

    try {
        $result = gh issue create --repo $repo --title $title --body $bodyContent --label $labelsJoined 2>&1
        if ($LASTEXITCODE -eq 0) {
            $created++
            Write-Host "  [OK] $title -> $result"
        } else {
            $failed++
            Write-Host "  [FAIL] $title -> $result" -ForegroundColor Red
        }
    } catch {
        $failed++
        Write-Host "  [ERROR] $title -> $_" -ForegroundColor Red
    }
}

Write-Host "`nDone: $created created, $failed failed."
