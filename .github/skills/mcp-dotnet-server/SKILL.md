---
name: mcp-dotnet-server
description: Use when: model context protocol, MCP, MCP server, C# MCP, .NET MCP, VS Code mcp.json, local stdio MCP, remote HTTP MCP, deployed MCP endpoint, streamable HTTP, tools/resources/prompts, AI tool integration, create project from prompt, monorepo, split infra/code, clarification workflow, elicitation, IaC import, diagram-to-project, migration planning, or long-term MCP architecture for InfraFlowSculptor.
---

# Skill : mcp-dotnet-server — Concevoir un serveur MCP .NET pour InfraFlowSculptor

> Charger ce skill pour toute demande de conception, planification, implémentation, exposition VS Code, ou evolution long terme d'un serveur MCP dans ce depot.

Ce skill est aligne sur les sources officielles MCP et SDK C# consultees le 2026-04-28 :
- `modelcontextprotocol.io`
- `csharp.sdk.modelcontextprotocol.io`
- `modelcontextprotocol/csharp-sdk`
- documentation VS Code MCP (`.vscode/mcp.json`, gestion des serveurs MCP)

**Hypothese de base pour ce projet :** MCP doit etre une **couche d'adaptation** au-dessus d'InfraFlowSculptor, pas une nouvelle logique metier parallele. Les tools MCP doivent orchestrer les cas d'usage existants via CQRS/Application/Generation, et les nouveaux importeurs IaC doivent etre mutualisables avec l'API HTTP et le frontend.

---

## 1. Quand charger ce skill

Charge ce skill quand la demande parle de :

- "MCP", "Model Context Protocol", "serveur MCP", "outil AI"
- exposition de tools/resources/prompts dans VS Code, Copilot, Claude Desktop, ChatGPT ou autre client MCP
- creation de projet InfraFlowSculptor a partir d'un simple prompt utilisateur
- demandes du type "cree-moi un projet X en mono repo avec un Key Vault le moins cher possible"
- clarification obligatoire quand l'utilisateur n'a pas precise la topologie du repo ou d'autres choix structurants
- serveur MCP en C# / .NET / ASP.NET Core
- choix entre `stdio` et `Streamable HTTP`
- fichier `.vscode/mcp.json`
- import ARM, Terraform, autre IaC, diagramme d'architecture vers InfraFlowSculptor
- migration d'un existant IaC vers le modele projet + generation Bicep
- feuille de route long terme pour coupler MCP avec les futures resources/features

---

## 2. Baseline technique officielle a respecter

### Packages

| Package | Quand l'utiliser | Recommandation |
|---|---|---|
| `ModelContextProtocol.Core` | API bas niveau / client ou server custom minimal | Rare dans ce depot. Reserve aux besoins tres specifiques. |
| `ModelContextProtocol` | serveur `stdio`, DI, hosting, discovery attributaire | **Point de depart par defaut** pour un serveur local lance par VS Code. |
| `ModelContextProtocol.AspNetCore` | serveur MCP over HTTP avec ASP.NET Core | A utiliser quand il faut un endpoint partage, distant, ou securise. |

### Transports

| Transport | Quand le choisir | Recommandation |
|---|---|---|
| `stdio` | usage local depuis VS Code / client desktop, dev rapide, isolation par process | **Premier increment recommande** pour InfraFlowSculptor. Plus simple a exposer dans `.vscode/mcp.json`. |
| Streamable HTTP | serveur mutualise, usage distant, auth centralisee, observabilite web | Ajouter ensuite si le besoin depasse le poste local. |

### Stateful vs Stateless en HTTP

- `Stateless = true` est recommande pour les serveurs qui n'ont pas besoin de requetes serveur-vers-client.
- Passe en `Stateless = false` uniquement si tu as besoin de features comme sampling, elicitation, log streaming, ou autres interactions poussees vers le client.
- Pour des imports IaC, previews, creations de projet et generations Bicep classiques, **stateless est le bon defaut**.

### Bonnes pratiques SDK C#

- Utiliser `AddMcpServer()` avec DI/hosting standard .NET.
- Preferer `WithTools<T>()`, `WithPrompts<T>()`, `WithResources<T>()` aux scans reflexifs globaux si tu veux mieux controler la registration et rester plus robuste en AOT/trimming.
- `WithToolsFromAssembly()` est acceptable pour un spike, mais evite d'en faire la strategie long terme si tu vises un packaging plus durci.
- Pour `stdio`, **ne jamais ecrire de logs sur stdout**. Les logs doivent aller sur stderr via `AddConsole(... LogToStandardErrorThreshold ...)`.

---

## 3. Positionnement recommande dans InfraFlowSculptor

### Regle d'architecture

Le serveur MCP ne doit **pas** :

- parler directement a `DbContext`
- reimplementer la logique de creation de projet ou de generation Bicep
- exposer les agrégats Domain comme des pseudo-outils generiques
- parser les formats IaC avec du regex ad hoc ou de la logique LLM opaque
- propager des structures faibles (`object`, `Dictionary<string, object>`, `JsonDocument`, blobs JSON) au-dela d'un adapter source si un modele explicite est possible
- regrouper des dizaines de DTOs/contracts/tools dans un seul fichier poubelle

Le serveur MCP doit :

- agir comme **adaptateur d'entree**
- appeler la couche `Application` / services de generation existants
- reutiliser les validateurs, policies d'acces, DTOs, et services deja presents
- s'appuyer sur un **modele canonique d'import** commun a tous les formats sources
- convertir les payloads externes vers des contrats typés des que le schema est connu
- garder un type public top-level par fichier pour les tools, resources, prompts, contracts et modeles d'import

### Decoupage cible recommande

```text
src/
├── Mcp/
│   └── InfraFlowSculptor.Mcp/
│       ├── Program.cs
│       ├── Tools/
│       ├── Resources/
│       ├── Prompts/
│       ├── Contracts/
│       └── DependencyInjection/
└── Api/
    ├── InfraFlowSculptor.Application/
    │   └── Imports/
    ├── InfraFlowSculptor.Contracts/
    │   └── Imports/
    └── InfraFlowSculptor.Infrastructure/
        └── Imports/
```

### Pourquoi ce decoupage

- `src/Mcp/InfraFlowSculptor.Mcp` reste un **host d'interface** propre.
- La logique import/migration reutilisable vit dans `Application` + `Infrastructure`, donc elle pourra aussi servir a l'API HTTP et au frontend.
- Chaque nouveau type de resource ajoute plus tard ne demandera qu'un **mapping supplementaire** vers le modele canonique d'import, pas une duplication entiere cote MCP.

---

## 4. Modele cible pour les imports IaC et diagrammes

### Regle cle

**Ne transforme jamais directement un ARM/Terraform/diagramme en commandes d'ecriture finales sans etape intermediaire.**

**Ne laisse jamais un format source faiblement type devenir le contrat interne durable.** Les formes faibles ne vivent qu'a la frontiere d'import, puis sont mappees vers des modeles explicites.

Il faut une chaine en 4 etapes :

1. **Parse source**
2. **Normalise dans un modele canonique**
3. **Preview/valide/diff**
4. **Apply** vers le modele InfraFlowSculptor puis generation

### Pipeline recommande

```text
Source IaC / diagramme
-> SourceAdapter
-> ImportedProjectDefinition (modele canonique)
-> Validation + Preview + Gap analysis
-> Application commands / orchestration
-> Projet InfraFlowSculptor
-> Generation Bicep / Pipelines / autres artifacts
```

### Formes de source a privilegier

| Source | Strategie recommandee |
|---|---|
| ARM template | Parser JSON de maniere deterministe, valider le schema et normaliser les resources/dependencies. |
| ARM "YAML" ou variantes derivees | Normaliser d'abord vers une representation JSON-compatible, puis traiter comme ARM. |
| Terraform | **Preferer `terraform plan/show -json` ou state JSON** plutot que parser directement le HCL brut. Ne pas utiliser de regex. |
| Diagramme d'architecture | Preferer un format structure exportable (`.drawio`, Mermaid, PlantUML, JSON de design) avant toute extraction vision/OCR. |
| Autre IaC | Ajouter un `ISourceImportAdapter` dedie branche sur le meme modele canonique. |

### Guardrails de modelisation

- Si les donnees importees ont un schema connu, creer des `record`/`class`/`enum` dedies plutot qu'un `Dictionary<string, object>` ou un `JsonDocument` transversal.
- Les enums, constantes, et types de contrat partages doivent etre extraits dans des fichiers dedies ; pas de magic strings disperses dans les tools.
- Les dictionnaires ne sont legitimes que pour de vrais cas de cles dynamiques ou de lookup, pas comme substitut a un contrat d'import.
- Si plusieurs patterns sont plausibles (`simple service`, `Strategy`, `Factory`, `Builder`), retenir le plus simple qui garde l'orchestration MCP lisible et maintenable. Pas de pattern decoratif.

### Anti-pattern critique

- Mauvais : "un tool prend un fichier Terraform, cree le projet, genere le Bicep et push Git".
- Bon : `preview_iac_import` -> `apply_import_preview` -> `generate_project_bicep` -> eventuellement `push_generated_artifacts`.

---

## 5. Mapping MCP : tools, resources, prompts

### Regle de design

- **Tool** = action, calcul, mutation, orchestration.
- **Resource** = lecture ou contexte attachable, potentiellement volumineux ou reutilisable.
- **Prompt** = workflow guide, questions de cadrage, aide a la migration; jamais la logique metier elle-meme.

### Mapping recommande pour ce projet

| Besoin | Capacite MCP | Exemple |
|---|---|---|
| Lister les formats/supports d'import | Resource ou tool simple | `ifs://import/formats` ou `list_supported_import_sources` |
| Analyser un fichier/source IaC | Tool read-only | `preview_iac_import` |
| Expliquer le delta entre source et modele IFS | Tool ou resource derivee | `explain_import_gaps` |
| Creer / completer un projet | Tool mutateur | `apply_import_preview` |
| Generer le Bicep d'un projet | Tool mutateur ou semi-mutateur | `generate_project_bicep` |
| Lire l'etat consolide d'un projet | Resource | `ifs://projects/{projectId}/summary` |
| Lire un preview stocke | Resource | `ifs://imports/{previewId}` |
| Guider un import ambigu | Prompt | `ifs.migration_questions` |
| Guider la creation depuis un diagramme | Prompt | `ifs.diagram_intake` |
| Transformer une demande libre en draft de projet | Tool read-only | `draft_project_from_prompt` |
| Demander les choix manquants avant creation | Prompt ou elicitation | `ifs.project_creation_questions` |
| Creer le projet a partir d'un draft valide | Tool mutateur | `create_project_from_draft` |

### Regles pour les outputs

- Les tools ne doivent pas renvoyer des blobs massifs inutiles.
- Si le resultat est long (preview detaille, gros Bicep, rapport de migration), stocke ou recompose le resultat et renvoie un identifiant / URI / resume.
- Les `resources` sont plus adaptees que les `tools` pour exposer un contenu volumineux a relire plusieurs fois.
- Les outputs MCP doivent rester fortement typés et stables ; ne pas rebasculer sur des sacs de proprietes faibles si le schema de sortie est connu.

### Premiere surface de tools recommandee

Ne commence pas avec 30 tools. Pour InfraFlowSculptor, une premiere surface credible et exploitable peut rester resserree autour de 10 a 12 tools.

#### Tools de discovery et cadrage

- `list_repository_topologies`
  Retourne les topologies supportees par le produit, par exemple `MonoRepo` et `SplitInfraCode`, avec une description claire des implications.
- `list_supported_resource_types`
  Retourne le catalogue des resource types supportes et les champs minimaux attendus.
- `list_project_creation_defaults`
  Retourne les conventions disponibles: topologies, strategies d'environnements, modes de generation, options de naming.
- `draft_project_from_prompt`
  Transforme une demande libre en draft structure sans effet de bord.
- `validate_project_draft`
  Indique si le draft est complet, quels champs manquent, et quelles clarifications restent a obtenir.

#### Tools de creation et evolution projet

- `create_project_from_draft`
  Cree le projet une fois le draft complet et valide.
- `add_resource_to_project`
  Ajoute une resource a un projet existant en reappliquant les validations metier.
- `update_resource_configuration`
  Met a jour une configuration ciblee d'une resource existante.
- `upsert_project_secrets`
  Ajoute ou met a jour les secrets associes au projet ou au Key Vault cible.
- `recommend_low_cost_defaults`
  Retourne des defaults de sizing/pricing a faible cout pour un type de resource donne, sans muter le projet.

#### Tools d'import et migration

- `preview_iac_import`
  Analyse une source IaC et retourne un preview exploitable.
- `apply_import_preview`
  Applique un preview valide sur le modele projet.
- `preview_diagram_import`
  Analyse un diagramme d'architecture structure et propose un draft de projet.
- `explain_import_gaps`
  Explique les ecarts entre la source et ce qu'InfraFlowSculptor sait modeliser.

#### Tools de generation et restitution

- `generate_project_bicep`
  Lance la generation Bicep du projet.
- `generate_project_pipelines`
  Lance la generation des pipelines si la feature est active.
- `get_project_summary`
  Retourne un resume compact du projet, des resources et de l'etat de generation.

#### Ordre recommande pour une V1

Si tu veux reduire encore la surface initiale, commence par ces 8 tools :

1. `list_repository_topologies`
2. `list_supported_resource_types`
3. `draft_project_from_prompt`
4. `validate_project_draft`
5. `create_project_from_draft`
6. `preview_iac_import`
7. `apply_import_preview`
8. `generate_project_bicep`

Le reste peut venir dans une phase 2.

### Creation de projet en langage naturel

Le MCP doit pouvoir couvrir des demandes comme :

- "Cree-moi un projet XXX en mono repo avec un Key Vault le moins cher possible et ajoute ces secrets"
- "Cree un projet avec un Key Vault"

Mais il ne faut **jamais** laisser un tool mutateur deviner des choix structurants absents du prompt.

#### Pattern recommande

1. `draft_project_from_prompt` transforme la demande libre en `ProjectCreationDraft`
2. Le draft marque les champs manquants et les hypotheses possibles
3. Tant que des champs obligatoires manquent, **aucune creation reelle** n'est executee
4. Le client/agent pose les questions manquantes a l'utilisateur
5. `create_project_from_draft` n'est appele que lorsque le draft est complet et valide

#### Champs obligatoires a expliciter avant creation

- `projectName`
- `repositoryTopology`
- `environmentStrategy` si elle n'est pas inferable par une convention explicite
- toute information indispensable a la creation correcte des ressources demandees

#### Cas specifique demande par l'utilisateur

Si l'utilisateur dit seulement :

> "Cree un projet avec un Key Vault"

le workflow doit **repondre avec une demande de clarification** au lieu de creer le projet. En particulier, il doit demander au minimum de choisir entre :

- `MonoRepo`
- `SplitInfraCode` / infra et code separes
- autre topologie supportee par le produit si elle est exposee

Le meme principe s'applique a toute ambiguite structurante : on peut inferer un **intent** faible, mais pas lancer une mutation irreversible.

#### Ce qui peut etre infere vs ce qui doit etre confirme

| Type d'information | Regle |
|---|---|
| Nom de ressource citee explicitement (`KeyVault`) | Peut etre infere |
| Intent de cout (`le moins cher possible`) | Peut etre transforme en hint de sizing/pricing a valider par les regles metier |
| Secrets a ajouter | Peut etre collecte comme liste de valeurs a injecter dans le draft |
| Topologie repo (`MonoRepo`, `SplitInfraCode`, etc.) | **Doit etre confirmee si absente** |
| Strategie d'environnements | Doit etre confirmee si elle impacte la structure du projet |

#### Prompt/contract conseille

Le tool `draft_project_from_prompt` devrait retourner un contrat explicite du style :

```json
{
  "draftId": "draft_123",
  "status": "requires_clarification",
  "missingFields": ["repositoryTopology"],
  "clarificationQuestions": [
    {
      "field": "repositoryTopology",
      "message": "Choisissez la topologie du depot pour le projet.",
      "options": ["MonoRepo", "SplitInfraCode"]
    }
  ],
  "inferredIntent": {
    "projectName": "XXX",
    "resources": ["KeyVault"],
    "pricingIntent": "lowest-cost"
  }
}
```

Le status peut ensuite passer a `ready_to_create` une fois les reponses recueillies.

#### Prompts MCP et elicitation

- Pour une V1 simple en `stdio`, le plus robuste est que **l'agent/chat pose lui-meme les questions** avant d'appeler le tool mutateur.
- Si tu veux que le **serveur MCP lui-meme** conduise le questionnement, regarde la feature **elicitation** du SDK C#. Elle demande une architecture plus avancee et repose en pratique sur un mode **HTTP stateful** pour les echanges serveur-vers-client.
- Ne rends pas l'elicitation obligatoire pour le premier increment si le besoin principal est local dans VS Code.

---

## 6. Feuille de route recommandee

### Phase 0 — ADR et cadrage

- Choisir le premier use case concret : `preview_iac_import` ou `generate_project_bicep`.
- Fixer le transport initial : `stdio`.
- Definir le modele canonique d'import.
- Decider ce qui est **in scope** vs **out of scope** pour V1.

### Phase 1 — Serveur MCP local minimal

- Nouveau projet `src/Mcp/InfraFlowSculptor.Mcp`.
- 2 a 3 tools read-only maximum, dont idealement `draft_project_from_prompt`.
- Exposition dans `.vscode/mcp.json` au niveau workspace.
- Aucun side effect irreversible.

### Phase 2 — Preview d'import canonique

- Ajouter le pipeline `SourceAdapter -> ImportedProjectDefinition -> Validation -> Preview`.
- Sauvegarder un `previewId` temporaire / stocke.
- Tester les imports sur snapshots representatifs.

### Phase 3 — Apply controllable

- Tool `apply_import_preview` idempotent autant que possible.
- Journaliser ce qui est cree vs mis a jour vs ignore.
- Reutiliser l'autorisation et les policies d'acces existantes.

### Phase 4 — Generation et post-traitement

- Tool `generate_project_bicep`.
- Optionnel : ressources MCP pour recuperer les artefacts et resumes de generation.
- Garder le push Git hors V1 si possible.

### Phase 5 — Durcissement long terme

- Ajout HTTP si besoin multi-utilisateur ou remote.
- Auth + `AddAuthorizationFilters()` pour supporter `[Authorize]`.
- Tasks MCP pour operations longues.
- Observabilite OTel, metrics, audit, quotas.

---

## 7. Exposition dans VS Code

### Workspace config recommandee

Utilise `.vscode/mcp.json` pour partager la configuration dans le depot une fois le chemin stabilise.

Exemple pour un serveur local `stdio` :

```json
{
  "servers": {
    "infraflowsculptor-mcp": {
      "type": "stdio",
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "${workspaceFolder}/src/Mcp/InfraFlowSculptor.Mcp/InfraFlowSculptor.Mcp.csproj"
      ]
    }
  }
}
```

### Configuration distante recommandee

Si tu veux que des utilisateurs se connectent a ton service **deja deploye**, `stdio` ne suffit pas. Il faut exposer un **endpoint MCP HTTP** public ou prive, par exemple :

- `https://api.ton-domaine.com/mcp`
- ou `https://ton-domaine.com/mcp` si tu assumes le couplage sur le meme domaine

Exemple `mcp.json` pour une connexion distante :

```json
{
  "inputs": [
    {
      "type": "promptString",
      "id": "ifs-prod-token",
      "description": "InfraFlowSculptor MCP production token",
      "password": true
    }
  ],
  "servers": {
    "infraFlowSculptorProd": {
      "type": "http",
      "url": "https://api.ton-domaine.com/mcp",
      "headers": {
        "Authorization": "Bearer ${input:ifs-prod-token}"
      }
    }
  }
}
```

### Regle cle pour le distant

- `stdio` = uniquement local, lance un process sur la machine de l'utilisateur.
- `http` = connexion a un serveur deja deploye.

Si ton objectif est de supporter **les deux**, conçois la meme surface MCP avec deux modes d'acces :

- **Local dev** : host `stdio` pour tester vite en local.
- **Deployed** : host HTTP pour permettre a n'importe quel user autorise de brancher VS Code sur ton endpoint MCP.

### Recommandation d'architecture de deploiement

- N'expose pas le MCP sur le frontend Angular lui-meme.
- Expose-le sur un host .NET cote API ou sur un service dedie, par exemple sous `/mcp`.
- Idealement, separe l'URL fonctionnelle du site web et l'URL MCP, par exemple `app.ton-domaine.com` pour l'UI et `api.ton-domaine.com/mcp` pour le protocole MCP.
- Applique auth, rate limiting, logs, et versioning comme pour n'importe quelle API sensible.

### Workspace vs User profile

- **Workspace `.vscode/mcp.json`** : bien pour partager une config d'equipe, sans secrets en dur.
- **User profile `mcp.json`** : bien pour laisser chaque utilisateur connecter son VS Code a ton environnement de dev, de recette, ou de production.

### Endpoint public, prive, ou hybride

Tu peux proposer plusieurs entrees :

- `infraFlowSculptorLocal` en `stdio`
- `infraFlowSculptorDev` en `http`
- `infraFlowSculptorProd` en `http`

Exemple :

```json
{
  "inputs": [
    {
      "type": "promptString",
      "id": "ifs-dev-token",
      "description": "InfraFlowSculptor dev MCP token",
      "password": true
    },
    {
      "type": "promptString",
      "id": "ifs-prod-token",
      "description": "InfraFlowSculptor prod MCP token",
      "password": true
    }
  ],
  "servers": {
    "infraFlowSculptorLocal": {
      "type": "stdio",
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "${workspaceFolder}/src/Mcp/InfraFlowSculptor.Mcp/InfraFlowSculptor.Mcp.csproj"
      ]
    },
    "infraFlowSculptorDev": {
      "type": "http",
      "url": "https://dev-api.ton-domaine.com/mcp",
      "headers": {
        "Authorization": "Bearer ${input:ifs-dev-token}"
      }
    },
    "infraFlowSculptorProd": {
      "type": "http",
      "url": "https://api.ton-domaine.com/mcp",
      "headers": {
        "Authorization": "Bearer ${input:ifs-prod-token}"
      }
    }
  }
}
```

### Regles VS Code

- Ne mets jamais de secret en dur dans `mcp.json`.
- Utilise des variables d'entree / env files quand le serveur a besoin de credentials.
- Pour une connexion a un site deploye, utilise `type: "http"` avec `url` et eventuellement `headers` pour l'authentification.
- Sur Windows, les chemins absolus exigent une attention particuliere; `${workspaceFolder}` avec des `/` est en general plus robuste.
- Le trust MCP dans VS Code fait partie du modele de securite: ne contourne pas ce garde-fou.

---

## 8. Squelettes techniques de reference

### Serveur `stdio` minimal

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddConsole(options =>
{
    options.LogToStandardErrorThreshold = LogLevel.Information;
});

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<ImportTools>()
    .WithResources<ProjectResources>()
    .WithPrompts<MigrationPrompts>();

await builder.Build().RunAsync();
```

### Serveur HTTP minimal

```csharp
using ModelContextProtocol.Server;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddMcpServer()
    .WithHttpTransport(options =>
    {
        options.Stateless = true;
    })
    .AddAuthorizationFilters()
    .WithTools<ImportTools>()
    .WithResources<ProjectResources>()
    .WithPrompts<MigrationPrompts>();

var app = builder.Build();

app.MapMcp();

app.Run();
```

### Commentaire important

- `AddAuthorizationFilters()` est a activer pour les serveurs ASP.NET Core si tu veux que `[Authorize]` / `[AllowAnonymous]` soient respectes.
- Les tools instance peuvent utiliser DI; les tools statiques conviennent bien aux spikes.

---

## 9. Regles d'integration avec la stack existante

### Ce que les tools MCP doivent appeler

Ordre de preference :

1. `Application` commands/queries/handlers existants
2. services applicatifs dedies a l'import/migration
3. services de generation Bicep/Pipeline deja exposes par la solution

### Ce qu'ils ne doivent pas appeler directement

- `ProjectDbContext`
- repositories pour contourner les handlers / policies / validations
- logique de generation recodee dans le projet MCP

### Recommandation forte

Quand un use case MCP n'existe pas encore cote `Application`, cree la logique **dans Application/Infrastructure**, puis appelle-la depuis MCP. Ne fais pas du MCP le seul endroit ou la fonctionnalite existe.

---

## 10. Securite, robustesse, operations longues

### Securite

- Revalider toutes les entrees MCP via FluentValidation ou validateurs equivalent.
- Pour les actions mutatrices, preferer des tools a perimetre etroit et explicite.
- Exiger que tout tool mutateur de creation de projet refuse les drafts incomplets et retourne une demande de clarification exploitable.
- Reutiliser `IInfraConfigAccessService` et les policies metier existantes pour les droits.
- Journaliser les actions sensibles (imports, creations, generation, push).

### Operations longues

- Si les imports / generations deviennent longues sur HTTP, activer MCP Tasks.
- `InMemoryMcpTaskStore` convient au dev et au mono-instance.
- En prod/multi-instance, implementer `IMcpTaskStore` sur un stockage persistant (BD, Redis, etc.).

### Observabilite

- Pour `stdio`, logs -> stderr uniquement.
- Pour HTTP, brancher OTel/logging comme le reste du projet.
- Exposer des diagnostics exploitables pour comprendre pourquoi un import est partiel ou invalide.

---

## 11. Strategie de tests

### Types de tests attendus

- **Unit tests** pour chaque `SourceAdapter` et pour la normalisation vers le modele canonique.
- **Unit tests** pour chaque tool MCP qui mappe les entrees/sorties et la validation.
- **Integration tests** pour le serveur MCP via transport memoire/streams ou via HTTP test host.
- **Golden tests** pour les previews d'import et les rapports de migration si le format doit rester stable.

### Point de vigilance SDK

- N'utilise pas `WithStdioServerTransport()` dans un test in-process standard pour "resoudre" le transport et l'executer comme un service ordinaire: le SDK souligne que `stdio` ouvre `Console.OpenStandardInput()` et peut bloquer le host de test.
- Pour les tests, preferer un transport memoire/stream ou le transport HTTP de test.

### Commandes repo

- `dotnet build .\InfraFlowSculptor.slnx`
- `dotnet test .\InfraFlowSculptor.slnx`

---

## 12. Pieges a eviter

1. **Faire du MCP un deuxieme backend** avec sa propre logique metier dupliquee.
2. **Parser Terraform ou des diagrammes avec du regex** au lieu d'un parseur ou d'un format structure.
3. **Fusionner preview + apply + generate** dans un seul tool opaque.
4. **Retourner trop de texte dans les tools** au lieu d'utiliser resources / IDs / artifacts.
5. **Ecrire sur stdout en `stdio`** et casser le protocole JSON-RPC.
6. **Utiliser le mode HTTP stateful par defaut** sans besoin reel.
7. **Oublier `AddAuthorizationFilters()`** sur ASP.NET Core quand des autorisations sont requises.
8. **Rester sur du scan assembly partout** si le serveur doit etre durci ou packagé en AOT.
9. **Faire du MCP le seul point d'entree** d'une nouvelle fonctionnalite import/migration au lieu de la rendre reutilisable par le reste de la plateforme.
10. **Ignorer la dette d'evolution**: chaque nouvelle resource InfraFlowSculptor devra mettre a jour le mapping d'import, les prompts de migration et les tests MCP.
11. **Deviner la topologie du depot** ou un autre choix structurant quand le prompt utilisateur est incomplet.
12. **Melanger extraction d'intent et mutation** dans un seul tool de creation de projet.

---

## 13. Checklist de planification

Avant toute implementation, repondre explicitement a ces questions :

1. Quel est le **premier use case** MCP a forte valeur ?
2. Quelle est la **source d'entree** exacte (ARM JSON, Terraform plan JSON, draw.io XML, etc.) ?
3. Quel est le **modele canonique d'import** ?
4. Quelle est la frontiere entre **preview** et **apply** ?
5. Quels droits sont necessaires pour chaque tool ?
6. Quels resultats doivent etre des **resources** plutot que des retours de tools ?
7. Le serveur est-il **local-only** (stdio) ou devra-t-il devenir **shared** (HTTP) ?
8. Comment chaque nouvelle resource/future feature sera-t-elle branchee dans le pipeline d'import ?

---

## 14. Workflow recommande avec les autres agents/skills

- **Pour planifier** : charger ce skill puis deleguer a `architect`.
- **Pour implementer en .NET** : charger ce skill + `tdd-workflow` + `xunit-unit-testing`, puis deleguer a `dotnet-dev`.
- **Pour des imports depuis diagrammes** : charger aussi `draw-io-diagram-generator` si le format source est `.drawio`.
- **Pour mesurer l'impact sur des services partages** : charger aussi `gitnexus-workflow` avant modification.

Le pattern cible dans ce depot est donc :

`architect` + `mcp-dotnet-server` -> plan -> `dotnet-dev` + `mcp-dotnet-server` + `tdd-workflow` + `xunit-unit-testing`.