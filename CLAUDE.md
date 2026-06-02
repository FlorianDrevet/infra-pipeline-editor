# CLAUDE.md — InfraFlowSculptor

> Fichier d'instructions **auto-chargé par Claude Code** à chaque session (équivalent de `.github/copilot-instructions.md`, qui reste réservé à GitHub Copilot).
> Il est volontairement **concis** pour économiser des tokens. Le détail vit dans la mémoire (`.claude/memory/`), les skills (`.claude/skills/`) et les sous-agents (`.claude/agents/`).

---

## 0. Premier réflexe — Mémoire d'abord

**Avant toute tâche non triviale**, lire l'index mémoire `.claude/memory/MEMORY.md`, puis charger uniquement les fichiers thématiques pertinents (`01-*` … `14-*`). Ne jamais charger toute la mémoire d'un coup.

**Orchestration :** par défaut, orchestre directement (lis la mémoire, délègue aux sous-agents, charge les skills) — tu es le thread principal. La commande **`/dev`** est **optionnelle** : c'est le protocole « artillerie lourde » pour les tâches complexes / cross-cutting / planifiées (passe de contradiction Bicep, phase Research GitNexus, scratchpad multi-agents, tracker multi-PC). Pour une tâche simple et ciblée, pas besoin de `/dev`.

En fin de toute tâche non triviale : mettre à jour le fichier thématique `.claude/memory/` concerné + ajouter une ligne dans `.claude/memory/changelog.md`. La **consolidation** plus profonde de la mémoire est manuelle : commande **`/dream`** quand tu le décides.

---

## 1. Environnement de développement

> L'utilisateur travaille sur **Windows**. Toutes les commandes terminal doivent utiliser la syntaxe **PowerShell**. Utiliser `.\` pour les chemins relatifs, `;` comme séparateur, `$env:` pour les variables d'environnement. Ne jamais suggérer de commandes bash/sh pour les workflows projet.

## 2. Build, run, test

- SDK : `.NET 10.0.100` (depuis `global.json`).
- Build solution : `dotnet build .\InfraFlowSculptor.slnx`
- Tests solution : `dotnet test .\InfraFlowSculptor.slnx`
- Test d'un seul projet : `dotnet test .\tests\<TargetAssembly>.Tests\<TargetAssembly>.Tests.csproj`
- Stack locale complète (Aspire) : `dotnet run --project .\src\Aspire\InfraFlowSculptor.AppHost\InfraFlowSculptor.AppHost.csproj`
- Frontend (depuis `src\Front`) : `npm install` · `npm run start` · `npm run build` · `npm run typecheck`
- Les projets de tests vivent sous `tests\<TargetAssembly>.Tests\` (un projet par assembly cible).

## 3. Architecture (haut niveau)

- `src\Aspire` — AppHost distribué (PostgreSQL, DbGate, API principale, API Bicep generator) + service defaults.
- `src\Api` — stack CQRS en couches : `Api` (Minimal API, OpenAPI, auth, Mapster) · `Application` (MediatR, handlers, validators, behaviors) · `Domain` (agrégats, value objects) · `Infrastructure` (EF Core, repositories, Azure, Refit) · `Contracts` (DTOs par feature).
- `src\BicepGenerators` — même découpage en couches pour l'API qui génère le Bicep et stocke les artefacts.
- `src\Front` — Angular (standalone components, Material, Tailwind, Axios, auth facade/guards).
- `src\Shared` — base DDD, abstractions applicatives, middleware/options API, converters/persistence partagés.
- Flux principal : endpoint Minimal API → mapping Mapster → command/query MediatR → handler/repository/service → domaine / EF Core → DTO typé en retour.

## 4. Request challenge rules (Bicep / pipelines / DevOps)

- Ne jamais traiter la dernière instruction utilisateur comme spec suffisante quand elle touche la **génération Bicep**, les **pipelines Azure DevOps**, le **bootstrap**, la **topologie de repos**, ou les **service connections**.
- Pour ces sujets : confronter d'abord la demande au modèle de domaine, à l'architecture de génération, aux contraintes réelles Azure DevOps/Bicep et à la mémoire projet. Si la demande est incohérente, incomplète ou techniquement fausse, le dire explicitement et proposer la direction corrigée.
- Ne jamais introduire un fallback "temporaire", un raccourci partagé, ou une commodité UI qui affaiblit l'isolation des environnements ou masque une hypothèse Bicep/pipeline invalide.

## 5. Snapshot / Seed (paire à synchroniser)

`docs/project-snapshots/fb8699ea-ifs-project.md` et `scripts/seed-project-snapshot.sql` sont un artefact **apparié** pour le seed développeur local (projet `fb8699ea-f568-4afb-864b-e82d2efd0905`). Toute mise à jour de l'un impose la mise à jour de l'autre dans la même tâche, puis revalidation du SQL contre le schéma `infraDb` local.

---

## 6. Sous-agents spécialisés (`.claude/agents/`)

> Dans Claude Code, un **sous-agent** est un exécutant isolé que **le thread principal** lance via le tool *Agent*. Un sous-agent **ne peut pas en lancer un autre** : l'orchestration reste dans le thread principal (rôle tenu par le protocole `/dev`).

| Tâche | Sous-agent |
|-------|-----------|
| Explorer le codebase avant d'agir (read-only, rapide) | `Explore` *(built-in)* |
| Analyse d'architecture, faisabilité, plan d'implémentation, challenge de feature | `architect` |
| Code backend C#/.NET (Domain, Application, Infra, Contracts, API, Shared) | `dotnet-dev` |
| Code frontend Angular (`src\Front`) | `angular-front` |
| Debug runtime / AppHost Aspire (ressources, logs, traces, recovery) | `aspire-debug` |
| Revue de code pré-merge (gate qualité avant merge sur `main`) | `review-expert` |
| Seconde passe anti-vibe-coding (smells, abstractions bidon, tests théâtre) | `vibe-coding-refractaire` |
| Appliquer un backlog de correction issu d'une review | `review-remediator` |
| Audit technique complet du dépôt + sync issues GitHub | `audit-expert` |
| Documentation technique / onboarding / pédagogie projet | `documentation-professor` |
| Création / soumission de Pull Request (conventions) | `pr-manager` |
| Fusionner `main` sur la branche courante | `merge-main` |
| Montée de version .NET / Angular / NuGet / npm | `upgrade-orchestrator` |
| Consolidation mémoire (Dream) | `dream` |

## 7. Skills (`.claude/skills/`)

> Un **skill** est de la connaissance projet pure (un `SKILL.md`), **chargé à la demande** quand la tâche correspond à sa `description`. Sans outils, réutilisable par n'importe quel agent. **Quand un skill s'applique, le charger AVANT de coder.**

| Skill | Quand le charger |
|-------|------------------|
| `cqrs-feature` | Nouvel agrégat / feature CQRS complète |
| `new-azure-resource` | Nouvelle ressource Azure end-to-end (Domain→…→Bicep→Front→i18n) |
| `dotnet-patterns` | Tout code C#/.NET (nommage, XML docs, typage fort, patterns) |
| `xunit-unit-testing` | Tests unitaires .NET/xUnit |
| `tdd-workflow` | **Toute** modification de code (cycle RED→GREEN→REFACTOR→VERIFY) |
| `angular-patterns` | Tout code Angular (signals, standalone, forms, i18n) |
| `ui-ux-front-saas` | Toute UI/UX frontend (écran, composant, layout, styles) |
| `bicep-v2-migration` | Migration d'un générateur Bicep legacy → Builder + IR |
| `dotnet-upgrade` / `angular-upgrade` | Montées de version .NET / Angular |
| `gitnexus-workflow` | Exploration structurelle, analyse d'impact, validation post-change |
| `graphify-corpus` | Vue transversale code+docs, onboarding, god nodes, audit |
| `audit-workflow` | Audit technique + sync issues/labels GitHub |
| `draw-io-diagram-generator` | Diagrammes `.drawio` (archi, flow, séquence, ER, UML) |
| `mcp-dotnet-server` | Conception/implémentation d'un serveur MCP .NET |

## 8. Commandes (`.claude/commands/`)

Slash-commands disponibles : `/dev` (orchestrateur — optionnel, protocole lourd), `/dream` (consolidation mémoire manuelle), `/new-cqrs-feature`, `/add-azure-resource`, `/review-main`, `/merge-main`, `/run-audit`.

---

## 9. Pièges critiques (référence rapide)

1. **EF Core LINQ :** jamais `x.Id.Value == id.Value` — toujours `x.Id == id`.
2. **Mapster nulls :** `x != null`, jamais `(object?)x`, jamais `is not null` (CS8122 en expression tree).
3. **Repositories :** ne JAMAIS appeler `SaveChangesAsync()` — l'Unit of Work s'en charge.
4. **i18n dialog keys :** chemin imbriqué complet `RESOURCE_EDIT.DIALOG_NAME.*`.
5. **Magic strings :** jamais de type de ressource Azure hardcodé — utiliser `AzureResourceTypes.*`.
6. **Domain quality :** XML docs, `private set`, erreurs en anglais, initializers `= []`, `sealed` sur agrégats.
7. **FK cascade on delete :** FKs cross-ressource en SetNull ou Cascade, jamais Restrict.
8. **Response DTO IDs :** toujours `string` (pas `Guid`) — Mapster mappe `Id.Value.ToString()`.
9. **OpenAPI 401 :** tout endpoint protégé inclut `.ProducesProblem(401)`.
10. **GitNexus :** avant de modifier un symbole partagé, lancer `gitnexus_impact()` pour le blast radius.
11. **Graphes :** GitNexus pour structure/impact code, Graphify pour corpus (docs+diagrammes+audits). Jamais l'inverse.
12. **TDD obligatoire :** jamais de code de prod sans tests d'abord — skill `tdd-workflow`, dette tracée dans `.claude/test-debt.md`.
13. **DS obligatoire (Frontend) :** toute UI de prod part des `app-ds-*` existants. Interdit de fabriquer à la main un composant visuel si le design system couvre le besoin ; sinon créer/étendre d'abord un primitive DS dans `src/Front/src/app/shared/components/ds/`.
14. **Une classe par fichier :** pas de fichier poubelle `Dtos.cs`, `Models.cs`, `Responses.cs`, `Helpers.cs`.
15. **Typage fort d'abord :** éviter `object`, `Dictionary<,>`, `JsonDocument`, blobs JSON quand le schéma est connu.
16. **Patterns avec levier uniquement :** comparer les options et garder la plus simple ; pas d'abstraction décorative.

## 10. Guardrails structurels (toute génération de code)

- Pas de magic strings : enums, constantes dédiées, options typées, ou `nameof()`.
- Un seul type public top-level par fichier ; sous-dossiers thématiques dès qu'un dossier dépasse ~6 fichiers ou mélange des responsabilités (namespace = chemin physique).
- Typage fort avant structures faibles ; une frontière externe faible est isolée dans l'adapter, validée, puis mappée immédiatement.
- Choix de pattern discipliné : comparer ≥2 options plausibles, garder la plus lisible/maintenable/scalable.

## 11. Pull Requests

Déléguer au sous-agent `pr-manager`. Titre : `type(scope): description`. Description via `.github/PULL_REQUEST_TEMPLATE.md`. **Gate obligatoire avant PR** : `/review-main` (review-expert + vibe-coding-refractaire).

## 12. MCP disponibles

`gitnexus` (intelligence de code : query/context/impact/detect_changes) et `sonarqube` (qualité). Activés dans `.claude/settings.local.json`. Si un outil GitNexus signale un index obsolète : `npx gitnexus analyze`.
