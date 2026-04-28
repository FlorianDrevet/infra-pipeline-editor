# MCP dans Infra Flow Sculptor

## Objectif de ce document

Ce document est un **cours d'onboarding** sur le Model Context Protocol (MCP) applique a **Infra Flow Sculptor**.

Son but est de te permettre de :

1. comprendre **ce qu'est MCP** et ce qu'il n'est pas
2. comprendre **comment un serveur MCP s'integre dans un projet .NET**
3. comprendre **comment l'exposer** localement et une fois deploye
4. comprendre **l'architecture cible** envisagee pour ce depot
5. etre capable de **challenger le code genere** plus tard

Ce document ne remplace pas la documentation officielle MCP. Il sert de pont entre la theorie et **la facon correcte de l'integrer dans ce projet**.

---

## 1. MCP, explique simplement

### Definition courte

MCP, pour **Model Context Protocol**, est un protocole standard qui permet a un client IA de se connecter a un serveur qui expose :

- des **tools** : actions que l'IA peut appeler
- des **resources** : donnees lisibles et reutilisables comme contexte
- des **prompts** : templates ou workflows predefinis

### Image mentale utile

Pense MCP comme une **prise normalisee** entre un client IA et ton systeme.

Sans MCP :

- l'agent connait du texte, mais ne sait pas vraiment agir sur ton systeme

Avec MCP :

- l'agent voit une liste d'actions autorisees
- il peut appeler ces actions de maniere structuree
- il recupere des resultats standardises

### Ce que MCP n'est pas

MCP n'est pas :

- un agent magique qui comprend tout sans travail de conception
- un remplacement de ton API metier
- un backend parallele qui doit dupliquer toute la logique existante
- un parseur IaC en soi

Dans un bon design, **MCP est une couche d'adaptation** au-dessus de la logique metier existante.

---

## 2. Le vocabulaire minimal a connaitre

### Client MCP / Host MCP

Le **client** ou **host** MCP est l'application qui consomme le serveur MCP.

Exemples :

- VS Code / GitHub Copilot Chat
- Claude Desktop
- ChatGPT s'il supporte MCP
- un autre client custom

Le host :

- demarre ou contacte le serveur MCP
- decouvre les tools/resources/prompts
- laisse l'agent choisir quoi appeler

### Serveur MCP

Le **serveur MCP** est ton application qui expose des capacites.

Dans ton cas, il exposera par exemple :

- creation guidee de projet
- import d'un ARM ou d'un plan Terraform
- ajout de ressources dans un projet
- generation du Bicep d'un projet

### Tools

Un **tool** est une action executable.

Exemples :

- `draft_project_from_prompt`
- `create_project_from_draft`
- `preview_iac_import`
- `generate_project_bicep`

Un tool doit avoir :

- un nom clair
- des parametres explicites
- un contrat de sortie stable

### Resources

Une **resource** sert a exposer des donnees lisibles comme contexte.

Exemples :

- resume d'un projet
- resultat d'un preview d'import
- catalogue des topologies supportees

### Prompts

Un **prompt MCP** est un template ou un mini-workflow guide.

Exemples :

- questions de clarification pour la creation de projet
- guide de migration depuis un diagramme

---

## 3. Pourquoi MCP est pertinent pour Infra Flow Sculptor

Infra Flow Sculptor manipule deja un domaine structurant :

- projet
- topologie de depot
- configurations d'infrastructure
- ressources Azure
- generation Bicep
- generation de pipelines

MCP est pertinent ici car il permet d'ouvrir cette logique a des interactions naturelles du type :

- "cree-moi un projet mono repo avec un Key Vault"
- "importe ce Terraform et cree un projet IFS equivalent"
- "a partir de ce diagramme, propose une configuration projet"
- "genere le Bicep du projet X"

La vraie valeur n'est pas "MCP" en soi. La valeur est de **rendre ton moteur de modelisation et de generation utilisable conversationnellement**.

---

## 4. Comment MCP s'integre dans un projet

### Le mauvais reflexe

Le mauvais reflexe consiste a faire :

```text
Prompt utilisateur
-> tool MCP
-> logique metier recodee dans le projet MCP
-> base de donnees / generation
```

Pourquoi c'est mauvais :

- duplication de logique
- validations inconsistantes
- autorisation contournee
- dette de maintenance

### Le bon reflexe

Le bon design consiste a faire :

```text
Prompt utilisateur
-> serveur MCP
-> mapping / adaptation d'entree
-> Application / services existants
-> Domain / Infrastructure / Generation
-> resultat structure
```

Autrement dit :

- le serveur MCP **traduit l'intention**
- la logique metier reste dans les couches du projet

### Application a ce depot

Dans **Infra Flow Sculptor**, le serveur MCP ne doit pas :

- parler directement au `DbContext`
- recrire la logique de creation de projet
- recrire la logique de generation Bicep

Il doit :

- appeler la couche `Application`
- reutiliser les validateurs et services metier
- deleguer la persistance a `Infrastructure`
- deleguer la generation au moteur Bicep/Pipeline existant

---

## 5. Architecture cible pour ce depot

### Structure recommandee

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

### Repartition des responsabilites

#### Projet `InfraFlowSculptor.Mcp`

Contient :

- le host MCP
- la configuration du transport
- les tools/resources/prompts
- les contrats d'entree/sortie MCP

Ne contient pas :

- la logique metier profonde
- les regles de domaine principales
- la persistance metier directe

#### Couche `Application`

Doit contenir :

- les cas d'usage reutilisables par MCP
- les commandes/queries ou services applicatifs dedies a l'import
- les validations fonctionnelles

#### Couche `Infrastructure`

Doit contenir :

- les adaptateurs techniques d'import
- les parseurs / connecteurs / persistance necessaire

#### Couche de generation

Doit rester responsable de :

- la generation Bicep
- la generation de pipelines

---

## 6. Le flux complet d'une requete MCP

Prenons un exemple simple :

> "Cree-moi un projet `RetailApi` avec un Key Vault"

### Flux recommande

```text
1. Le host MCP envoie la demande
2. L'agent choisit le tool `draft_project_from_prompt`
3. Le serveur MCP transforme la demande libre en draft structure
4. Le draft indique qu'il manque `repositoryTopology`
5. L'agent demande une clarification a l'utilisateur
6. L'utilisateur repond `MonoRepo`
7. L'agent appelle `create_project_from_draft`
8. Le serveur MCP appelle la logique applicative de creation
9. Le projet est cree dans Infra Flow Sculptor
10. L'agent peut ensuite appeler `generate_project_bicep`
```

### Point architectural critique

Le serveur MCP **ne doit pas deviner** un choix structurant comme la topologie du depot si le prompt ne la precise pas.

Il doit :

- extraire l'intention
- detecter les champs manquants
- refuser la mutation tant que le draft est incomplet

---

## 7. Les deux grandes manieres d'exposer un serveur MCP

Il existe deux modes d'exposition qui t'interessent directement.

### Mode 1 : `stdio`

Le host demarre le serveur comme un process local sur la machine du developpeur.

#### Quand l'utiliser

- developpement local
- spike rapide
- test en local depuis VS Code

#### Avantages

- simple a mettre en place
- pas besoin d'infrastructure HTTP au debut
- parfait pour la V1 locale

#### Limites

- ne marche pas pour un service deja deploye distant
- tourne sur le poste de l'utilisateur

### Mode 2 : `http`

Le host se connecte a un endpoint MCP accessible sur le reseau.

#### Quand l'utiliser

- environnement partage
- service de recette / preprod / prod
- utilisateurs qui veulent connecter VS Code a ton site deja deploye

#### Avantages

- utilisable a distance
- auth et observabilite plus standard
- plus naturel pour une plateforme partagee

#### Limites

- plus d'infra
- besoin de securiser l'exposition

### La regle simple a retenir

```text
stdio = local
http = deploye / distant
```

---

## 8. Comment on configure VS Code

### Fichier `mcp.json`

VS Code utilise un fichier `mcp.json` pour declarer les serveurs MCP.

Ce fichier peut etre :

- dans le workspace : `.vscode/mcp.json`
- dans le profil utilisateur VS Code : configuration globale utilisateur

### Exemple local `stdio`

```json
{
  "servers": {
    "infraflowsculptorMcp": {
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

### Exemple distant `http`

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

### Ce qu'il faut retenir

- ne jamais mettre de secret en dur dans `mcp.json`
- utiliser `inputs` ou un `envFile`
- un serveur distant MCP doit exposer une URL HTTP dediee

---

## 9. Comment implementer un serveur MCP .NET

### Packages principaux

Pour .NET, les packages de reference sont :

- `ModelContextProtocol`
- `ModelContextProtocol.AspNetCore` si tu exposes en HTTP

### Exemple minimal local

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
    .WithTools<ProjectTools>()
    .WithResources<ProjectResources>()
    .WithPrompts<ProjectPrompts>();

await builder.Build().RunAsync();
```

### Exemple minimal HTTP

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
    .WithTools<ProjectTools>()
    .WithResources<ProjectResources>()
    .WithPrompts<ProjectPrompts>();

var app = builder.Build();

app.MapMcp();

app.Run();
```

### Pourquoi ces lignes sont importantes

- `AddMcpServer()` initialise le serveur MCP
- `WithStdioServerTransport()` ou `WithHttpTransport(...)` choisit le mode d'exposition
- `WithTools<T>()` enregistre les tools
- `WithResources<T>()` enregistre les resources
- `WithPrompts<T>()` enregistre les prompts
- `MapMcp()` expose l'endpoint HTTP MCP

### Bon point de revue

Si plus tard du code genere utilise directement `DbContext` dans un tool MCP, c'est un mauvais signal.

---

## 10. Comment concevoir correctement les tools

### Regle 1 : un tool = une responsabilite claire

Mauvais :

- `create_project_from_everything_and_generate_and_push`

Bon :

- `draft_project_from_prompt`
- `create_project_from_draft`
- `generate_project_bicep`

### Regle 2 : les tools mutateurs doivent etre prudents

Un tool qui cree ou modifie des donnees doit :

- valider ses entrees
- refuser les drafts incomplets
- retourner des erreurs ou demandes de clarification exploitables

### Regle 3 : les resources servent au contexte

Si le resultat est gros ou doit etre relu plusieurs fois, prefere une resource.

Exemples :

- `ifs://projects/{projectId}/summary`
- `ifs://imports/{previewId}`

### Regle 4 : les prompts ne remplacent pas la logique

Un prompt sert a guider, pas a porter la logique metier.

---

## 11. Surface de tools recommandee pour Infra Flow Sculptor

### Tools de base

Pour une V1 raisonnable, le coeur peut etre :

1. `list_repository_topologies`
2. `list_supported_resource_types`
3. `draft_project_from_prompt`
4. `validate_project_draft`
5. `create_project_from_draft`
6. `preview_iac_import`
7. `apply_import_preview`
8. `generate_project_bicep`

### Tools additionnels utiles ensuite

9. `add_resource_to_project`
10. `update_resource_configuration`
11. `upsert_project_secrets`
12. `recommend_low_cost_defaults`
13. `preview_diagram_import`
14. `explain_import_gaps`
15. `generate_project_pipelines`
16. `get_project_summary`

### Pourquoi cet ordre

Parce qu'il couvre deja :

- creation guidee
- clarification
- import IaC
- generation Bicep

Sans ouvrir trop tot une surface de maintenance trop large.

---

## 12. L'exemple le plus important : creation de projet par prompt

### Cas 1 : prompt complet

Prompt utilisateur :

> Cree-moi un projet `RetailApi` en `MonoRepo` avec un Key Vault le moins cher possible et ajoute ces secrets.

Comportement attendu :

1. `draft_project_from_prompt`
2. draft complet ou presque complet
3. validation
4. `create_project_from_draft`
5. eventuellement `upsert_project_secrets`
6. eventuellement `generate_project_bicep`

### Cas 2 : prompt incomplet

Prompt utilisateur :

> Cree un projet avec un Key Vault.

Comportement attendu :

1. `draft_project_from_prompt`
2. le draft retourne `requires_clarification`
3. il manque au moins `repositoryTopology`
4. l'agent demande :
   - `MonoRepo`
   - `SplitInfraCode`

### Pourquoi c'est important

Parce que si le code genere cree un projet sans poser cette question, alors il :

- prend une decision structurante sans mandat explicite
- risque de produire un mauvais projet
- viole la regle d'or de ton design MCP

---

## 13. Comment challenger le code genere plus tard

Quand on generera le code MCP, voici les questions que tu devras te poser.

### Architecture

1. Est-ce que le projet MCP est bien un **adaptateur** et non un second backend ?
2. Est-ce que les tools appellent la couche `Application` plutot que `DbContext` ?
3. Est-ce que l'import IaC passe bien par un **modele canonique intermediaire** ?

### Design des tools

4. Les tools ont-ils une responsabilite claire ?
5. Les tools mutateurs refusent-ils les entrees incompletes ?
6. Les gros resultats sont-ils exposes comme resources plutot que renvoyes en texte brut ?

### Creation conversationnelle

7. Le code detecte-t-il correctement les champs manquants ?
8. Est-ce qu'il refuse de deviner `MonoRepo` vs `SplitInfraCode` ?
9. Les contrats de sortie des drafts sont-ils exploitables par un agent ?

### Exposition

10. Le mode local est-il bien en `stdio` ?
11. Le mode deploye est-il bien en `http` ?
12. L'endpoint `/mcp` est-il expose cote API ou service dedie, et pas dans le frontend ?

### Securite

13. Les secrets sont-ils absents du code et de `mcp.json` ?
14. L'authentification est-elle active sur le serveur deploye ?
15. Les tools mutateurs sont-ils journalises et proteges ?

### Qualite technique

16. Les logs `stdio` vont-ils bien sur stderr ?
17. Les tests couvrent-ils les tools, les drafts incomplets et les imports ?
18. Les operations longues ont-elles une strategie claire ?

---

## 14. Les pieges classiques a eviter

1. Faire du MCP un backend parallele
2. Parser Terraform ou un diagramme avec des regex fragiles
3. Fusionner draft, creation, generation et push dans un seul tool
4. Exposer trop de tools trop tot
5. Ecrire sur stdout en mode `stdio`
6. Exposer un serveur deploye en `stdio` alors qu'il faut du `http`
7. Deviner des choix structurants absents du prompt
8. Mettre les secrets en dur dans `mcp.json`
9. Oublier l'autorisation des actions mutatrices
10. Coupler trop fortement le MCP a l'UI frontend

---

## 15. Strategie de mise en oeuvre pour ce projet

### Etape 1

Creer un serveur MCP local `stdio` avec une toute petite surface :

- `list_repository_topologies`
- `draft_project_from_prompt`
- `validate_project_draft`

Objectif : prouver le flux conversationnel sans side effect irreversibles.

### Etape 2

Ajouter :

- `create_project_from_draft`
- `generate_project_bicep`

Objectif : couvrir un scenario bout en bout simple.

### Etape 3

Ajouter l'import IaC :

- `preview_iac_import`
- `apply_import_preview`

Objectif : activer les migrations.

### Etape 4

Exposer un endpoint HTTP deploye sous `/mcp`.

Objectif : permettre a VS Code de se connecter au service distant via `mcp.json`.

---

## 16. Ce qu'il faut absolument retenir

Si tu ne retiens que 10 idees, retiens celles-ci :

1. MCP est une **interface standard** entre une IA et ton systeme.
2. Un serveur MCP expose des **tools**, **resources** et **prompts**.
3. MCP ne remplace pas ton API metier; il s'y branche.
4. Dans ce projet, MCP doit etre une **couche d'adaptation** au-dessus de `Application`.
5. `stdio` sert au **local**.
6. `http` sert au **deploiement distant**.
7. Les tools mutateurs ne doivent pas deviner des choix structurants.
8. La creation par prompt doit passer par un **draft** puis une **validation**.
9. L'import IaC doit passer par un **modele canonique intermediaire**.
10. Un bon code MCP se challenge comme n'importe quel code critique : architecture, securite, responsabilites, tests.

---

## 17. Ordre de lecture recommande apres ce cours

Pour continuer :

1. lire [overview.md](overview.md) pour la vue d'ensemble du projet
2. relire [bicep-generation.md](bicep-generation.md) pour comprendre le moteur de generation
3. relire le skill MCP dans `.github/skills/mcp-dotnet-server/SKILL.md`
4. lire le [plan d'implementation MCP V1](mcp-v1-implementation-plan.md) pour les contrats exacts des tools et le decoupage par phase
5. ensuite seulement regarder le code genere

Avec cet ordre, tu peux passer de la comprehension generale a la relecture critique du code.