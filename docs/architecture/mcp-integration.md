# MCP dans Infra Flow Sculptor

## Objectif de ce document

Ce document est le **cours d'onboarding complet** sur la partie MCP du projet, avec un second objectif tres concret : t'expliquer **de bout en bout** comment fonctionne l'authentification par **PAT** dans cette implementation.

Il doit te permettre de :

1. comprendre ce qu'est MCP et a quoi servent ses `tools`, `resources` et `prompts`
2. comprendre **l'implementation actuelle** du serveur MCP dans ce depot
3. comprendre ce qu'est un **PAT** et pourquoi le projet s'en sert pour securiser `/mcp`
4. savoir **creer**, **utiliser**, **revoquer** et **connecter** un PAT dans VS Code
5. connaitre l'inventaire exact des capacites MCP exposees aujourd'hui
6. savoir dans quel ordre lire le code pour verifier chaque explication

Ce document ne remplace pas la documentation officielle MCP. Il sert de pont entre la theorie et **la facon dont MCP est vraiment implemente ici**.

> **Etat actuel a retenir tout de suite**
>
> - le serveur MCP du projet est aujourd'hui un **serveur HTTP stateless**
> - il expose par defaut la route **`/mcp`**
> - il est protege par une authentification **Bearer PAT**
> - la configuration VS Code partagee du depot est [../../.vscode/mcp.json](../../.vscode/mcp.json), dans une entree dediee a `infraflowsculptor-mcp`
> - la surface MCP actuelle expose **8 tools**, **2 resources** et **1 prompt**

---

## 1. MCP, explique simplement

### Definition courte

MCP, pour **Model Context Protocol**, est un protocole standard qui permet a un client IA de se connecter a un serveur qui expose :

- des **tools** : des actions executables
- des **resources** : des donnees lisibles comme contexte
- des **prompts** : des consignes ou workflows predefinis

### Image mentale utile

Pense MCP comme une **prise standard** entre un agent IA et ton systeme.

Sans MCP :

- l'agent lit du texte, mais il ne sait pas agir proprement sur ton application

Avec MCP :

- l'agent voit une liste de capacites nommees
- il sait quels parametres fournir
- il recoit des resultats structurels et reutilisables

### Ce que MCP n'est pas

MCP n'est pas :

- un agent magique qui devine tout sans travail de conception
- un remplacement de ton API metier
- une excuse pour dupliquer la logique existante dans un nouveau backend parallele
- un format de securite a lui seul

Dans un bon design, **MCP est une couche d'adaptation** au-dessus de la logique applicative deja en place.

---

## 2. Le vocabulaire minimal a connaitre

### Host MCP

Le **host MCP** est le client qui consomme le serveur MCP.

Exemples :

- VS Code / Copilot Chat
- Claude Desktop
- un autre client compatible MCP

Le host :

- se connecte au serveur MCP
- decouvre les tools/resources/prompts
- laisse ensuite l'agent choisir quoi appeler

### Serveur MCP

Le **serveur MCP** est l'application qui expose des capacites au host.

Dans ce projet, le serveur MCP peut par exemple :

- transformer un prompt libre en draft de projet
- valider ce draft
- creer un projet a partir du draft valide
- analyser un template ARM
- appliquer un import
- lancer la generation Bicep

### Tool

Un **tool** est une action executable.

Exemples actuels du projet :

- `draft_project_from_prompt`
- `create_project_from_draft`
- `preview_iac_import`
- `generate_project_bicep`

### Resource

Une **resource** expose un contenu lisible comme contexte.

Exemples actuels du projet :

- `ifs://projects/{projectId}/summary`
- `ifs://imports/{previewId}`

### Prompt

Un **prompt MCP** est une consigne preemballee fournie au host pour guider le comportement de l'agent.

Exemple actuel du projet :

- `project_creation_guide`

---

## 3. PAT, explique simplement

### Definition courte

Un **PAT** est un **Personal Access Token**.

Concretement, c'est une **chaine secrete** qui remplace un mot de passe dans un usage programme-a-programme.

Au lieu de dire :

- "voici mon login et mon mot de passe"

on dit :

- "voici un token dedie a cet usage, que je peux expirier ou revoquer sans toucher a mon compte principal"

### Pourquoi c'est utile

Un PAT permet de :

- ne pas reutiliser le mot de passe utilisateur
- limiter un usage a une integration precise
- revoquer un acces sans casser tout le reste
- tracer une derniere date d'utilisation
- donner une duree de vie optionnelle au secret

### Ce PAT n'est pas le PAT GitHub ou Azure DevOps

Dans ce depot, il faut distinguer **deux familles de PAT** :

1. les **PAT externes** pour GitHub / Azure DevOps, par exemple dans [../features/push-bicep-to-git.md](../features/push-bicep-to-git.md)
2. le **PAT interne Infra Flow Sculptor**, utilise pour authentifier un client MCP contre **ton propre serveur `/mcp`**

Le document que tu lis ici parle surtout du **PAT interne** du serveur MCP.

### Le point cle a retenir

Le PAT Infra Flow Sculptor n'est pas juste un token arbitraire colle dans un header. C'est un vrai petit sous-systeme du produit :

- creation cote domaine / application / API
- stockage securise en base sous forme de hash
- validation a chaque appel MCP
- revocation et expiration
- suivi de `LastUsedAt`

---

## 4. Pourquoi ce projet utilise un PAT pour MCP

Le serveur MCP tourne aujourd'hui comme un **endpoint HTTP** que VS Code peut appeler via [../../.vscode/mcp.json](../../.vscode/mcp.json).

Dans ce contexte, il faut une authentification qui soit :

- simple a utiliser depuis un client MCP
- compatible avec un appel HTTP standard
- independante d'un parcours interactif de login navigateur
- revocable facilement
- exploitable cote serveur pour resoudre un `CurrentUser`

Le PAT repond bien a ce besoin.

### Pourquoi ne pas utiliser directement l'auth interactive web ?

Parce qu'un client MCP n'a pas toujours un flux d'authentification navigateur aussi naturel qu'une SPA classique.

Pour un premier niveau d'integration robuste :

- **Bearer PAT** dans un header HTTP est plus simple
- le serveur sait a quel utilisateur rattacher l'appel
- l'agent peut se reconnecter sans redemarrer un flux complet d'auth web

---

## 5. Architecture actuelle MCP + PAT dans le depot

### Vue d'ensemble

Le point d'entree principal est [../../src/Mcp/InfraFlowSculptor.Mcp/Program.cs](../../src/Mcp/InfraFlowSculptor.Mcp/Program.cs).

Le serveur :

- cree un host ASP.NET Core
- ecoute par defaut sur `http://127.0.0.1:5258`
- monte MCP par defaut sur `/mcp`
- enregistre tools, resources et prompts explicitement
- branche `Application` + `Infrastructure`
- remplace l'auth interactive par une auth PAT dediee

Ces deux valeurs sont centralisees dans [../../src/Mcp/InfraFlowSculptor.Mcp/Common/McpOptions.cs](../../src/Mcp/InfraFlowSculptor.Mcp/Common/McpOptions.cs) :

- `ListenUrl` est surchargeable via `Mcp:ListenUrl` ou `MCP__LISTENURL`
- `Route` est surchargeable via `Mcp:Route`

### Le schema mental du runtime

```text
VS Code / Copilot
-> lit .vscode/mcp.json
-> cible l'entree infraflowsculptor-mcp
-> demande un PAT a l'utilisateur
-> envoie Authorization: Bearer ifs_...
-> POST/HTTP sur /mcp
-> PersonalAccessTokenAuthenticationHandler
-> resolution du user courant
-> tool/resource/prompt MCP
-> Application / Infrastructure / Generation
```

### Fichiers clefs de l'implementation

| Fichier | Role |
|---|---|
| [../../src/Mcp/InfraFlowSculptor.Mcp/Program.cs](../../src/Mcp/InfraFlowSculptor.Mcp/Program.cs) | Host MCP HTTP, registration des capacites, route `/mcp`, auth requise |
| [../../src/Mcp/InfraFlowSculptor.Mcp/Common/McpOptions.cs](../../src/Mcp/InfraFlowSculptor.Mcp/Common/McpOptions.cs) | Valeurs par defaut et surcharges de `ListenUrl` / `Route` |
| [../../src/Api/InfraFlowSculptor.Infrastructure/DependencyInjection.cs](../../src/Api/InfraFlowSculptor.Infrastructure/DependencyInjection.cs) | `AddPatAuthentication()` enregistre le scheme PAT par defaut |
| [../../src/Api/InfraFlowSculptor.Infrastructure/Auth/PersonalAccessTokenAuthenticationHandler.cs](../../src/Api/InfraFlowSculptor.Infrastructure/Auth/PersonalAccessTokenAuthenticationHandler.cs) | Verifie le Bearer PAT, resout l'utilisateur, enregistre `LastUsedAt` |
| [../../src/Api/InfraFlowSculptor.Domain/PersonalAccessTokenAggregate/PersonalAccessToken.cs](../../src/Api/InfraFlowSculptor.Domain/PersonalAccessTokenAggregate/PersonalAccessToken.cs) | Aggregate racine PAT : creation, prefixe, expiration, revocation, usage |
| [../../src/Api/InfraFlowSculptor.Api/Controllers/PersonalAccessTokenController.cs](../../src/Api/InfraFlowSculptor.Api/Controllers/PersonalAccessTokenController.cs) | Endpoints de gestion des PAT |
| [../../src/Front/src/app/features/settings/settings.component.ts](../../src/Front/src/app/features/settings/settings.component.ts) | Page frontend qui liste et revoque les PAT |
| [../../src/Front/src/app/features/settings/create-pat-dialog/create-pat-dialog.component.ts](../../src/Front/src/app/features/settings/create-pat-dialog/create-pat-dialog.component.ts) | Dialogue frontend de creation et affichage one-shot du token |
| [../../.vscode/mcp.json](../../.vscode/mcp.json) | Configuration VS Code partagee pour la connexion MCP |

### Un detail important : le serveur est stateless

Dans [../../src/Mcp/InfraFlowSculptor.Mcp/Program.cs](../../src/Mcp/InfraFlowSculptor.Mcp/Program.cs), le transport HTTP MCP est configure avec `options.Stateless = true;`.

Cela veut dire que le serveur ne depend pas d'une session HTTP serveur complexe pour fonctionner.

En revanche, certains objets temporaires existent en memoire cote application pour stocker :

- les drafts de creation de projet
- les previews d'import

Ces objets sont geres par :

- `IProjectDraftService`
- `IImportPreviewService`
- `InMemoryCleanupService`

Le modele a retenir est donc :

- **stateless pour le protocole HTTP**
- **avec stockage temporaire applicatif** pour les drafts et previews

---

## 6. Comment un PAT est implemente dans ce projet

## 6.1. Generation du token

La logique de generation vit dans [../../src/Api/InfraFlowSculptor.Domain/PersonalAccessTokenAggregate/PersonalAccessToken.cs](../../src/Api/InfraFlowSculptor.Domain/PersonalAccessTokenAggregate/PersonalAccessToken.cs).

Lorsqu'on cree un PAT :

1. le domaine genere **32 octets aleatoires** cryptographiquement forts
2. ces octets sont encodes en **Base64 URL-safe**
3. le prefixe fixe `ifs_` est ajoute
4. un hash SHA-256 du token plaintext est calcule
5. **seul le hash** est persiste
6. le plaintext n'est renvoye qu'une seule fois au moment de la creation

### Ce que cela veut dire en pratique

Si tu perds le PAT plaintext :

- tu ne peux pas le relire depuis la base
- tu dois en creer un nouveau

C'est exactement ce qu'on veut pour un secret.

## 6.2. Proprietes suivies par le domaine

Le PAT garde notamment :

- `UserId`
- `Name`
- `TokenHash`
- `TokenPrefix`
- `ExpiresAt`
- `CreatedAt`
- `LastUsedAt`
- `IsRevoked`

Le `TokenPrefix` sert a afficher un identifiant reconnaissable en UI sans redivulguer le secret complet.

## 6.3. Creation via Application et API

La creation passe par :

- [../../src/Api/InfraFlowSculptor.Application/PersonalAccessTokens/Commands/CreatePersonalAccessToken/CreatePersonalAccessTokenCommandHandler.cs](../../src/Api/InfraFlowSculptor.Application/PersonalAccessTokens/Commands/CreatePersonalAccessToken/CreatePersonalAccessTokenCommandHandler.cs)
- [../../src/Api/InfraFlowSculptor.Api/Controllers/PersonalAccessTokenController.cs](../../src/Api/InfraFlowSculptor.Api/Controllers/PersonalAccessTokenController.cs)

Les endpoints exposes sont :

- `GET /personal-access-tokens` : liste les tokens du user courant, sans leur valeur plaintext
- `POST /personal-access-tokens` : cree un token et retourne la valeur plaintext **une seule fois**
- `DELETE /personal-access-tokens/{id}` : revoque un token

Le contrat de creation retourne :

- les metadonnees du token
- `plainTextToken`

### Regle de securite a retenir

La phrase cle du systeme est : **"displayed only once at creation"**.

Le PAT doit etre copie et stocke au bon moment, sinon il faudra en recreer un.

## 6.4. Validation a l'authentification

La validation du token HTTP est faite par [../../src/Api/InfraFlowSculptor.Infrastructure/Auth/PersonalAccessTokenAuthenticationHandler.cs](../../src/Api/InfraFlowSculptor.Infrastructure/Auth/PersonalAccessTokenAuthenticationHandler.cs).

Le handler fait exactement ceci :

1. lit le header `Authorization`
2. verifie qu'il commence par `Bearer `
3. extrait la valeur du token
4. verifie que le token commence par `ifs_`
5. hash le token recu
6. cherche ce hash en base via `IPersonalAccessTokenRepository`
7. refuse si le token n'existe pas
8. refuse si le token est revoque ou expire
9. appelle `RecordUsage()`
10. persiste tout de suite `LastUsedAt`
11. injecte le `UserId` dans `HttpContext.Items`
12. cree un `ClaimsPrincipal` authentifie

### Pourquoi l'etape 10 est importante

Le commentaire du handler l'explique bien : comme il s'agit d'une requete de lecture cote auth, le `UnitOfWork` applicatif ne suffit pas a flusher `LastUsedAt` tout seul.

Sans `SaveChangesAsync()`, la date de derniere utilisation serait perdue.

## 6.5. Comment `CurrentUser` est resolu ensuite

Une fois le PAT valide :

- le serveur sait quel `UserId` est authentifie
- les handlers applicatifs MCP reutilisent ensuite le meme mecanisme `ICurrentUser` que le reste du backend

Autrement dit :

- le MCP n'invente pas un modele utilisateur parallele
- il se branche sur le pipeline existant

---

## 7. Comment creer et utiliser un PAT dans la vraie vie

### Methode recommandee : via le frontend

La partie UI est visible dans :

- [../../src/Front/src/app/features/settings/settings.component.ts](../../src/Front/src/app/features/settings/settings.component.ts)
- [../../src/Front/src/app/features/settings/create-pat-dialog/create-pat-dialog.component.ts](../../src/Front/src/app/features/settings/create-pat-dialog/create-pat-dialog.component.ts)

Le parcours utilisateur est :

1. ouvrir la page **Settings**
2. aller a la section **Personal Access Tokens**
3. cliquer sur **Create token**
4. donner un nom au token
5. definir une expiration optionnelle
6. creer le token
7. copier immediatement la valeur affichee

### Ce qui se passe ensuite

Le frontend :

- appelle `POST /personal-access-tokens`
- recoit `plainTextToken`
- l'affiche une fois dans le dialogue
- permet de le copier via le presse-papier

### Si tu fermes le dialogue sans copier

Le token est deja cree, mais sa valeur complete ne sera plus relue.

La bonne reaction est alors :

1. revoquer ce token
2. en recreer un nouveau

### La bonne hygiene d'usage

- donne un nom explicite au token, par exemple `VS Code MCP local`
- mets une expiration si tu peux
- revoque les tokens inutiles
- ne le committe jamais dans un fichier
- ne l'envoie jamais dans un log ou une issue

---

## 8. Comment VS Code se connecte au serveur MCP

La configuration partagee du depot est [../../.vscode/mcp.json](../../.vscode/mcp.json).

Le fichier complet est maintenant un **fichier MCP multi-serveurs** pour le workspace. Le bloc ci-dessous est donc volontairement **un extrait cible** sur l'entree `infraflowsculptor-mcp`.

La partie importante pour Infra Flow Sculptor est :

```json
{
  "servers": {
    "infraflowsculptor-mcp": {
      "type": "http",
      "url": "http://127.0.0.1:5258/mcp",
      "headers": {
        "Authorization": "Bearer ${input:ifs_pat}"
      }
    }
  },
  "inputs": [
    {
      "id": "ifs_pat",
      "type": "promptString",
      "description": "InfraFlowSculptor personal access token (generate from Settings > Personal Access Tokens).",
      "password": true
    }
  ]
}
```

### Ce qu'il faut comprendre ligne par ligne

- `type: "http"` : le host se connecte a un serveur MCP HTTP deja lance
- `url: "http://127.0.0.1:5258/mcp"` : URL par defaut du serveur MCP dans ce repo, composee du `ListenUrl` et de la `Route` par defaut
- `Authorization: "Bearer ${input:ifs_pat}"` : VS Code injecte le PAT saisi par l'utilisateur
- `password: true` : la valeur du prompt reste masquee dans l'UI

### Pourquoi cette config est propre

Parce que :

- le secret n'est pas committe dans le fichier
- la connexion est explicite
- le serveur MCP d'Infra Flow Sculptor est facilement partageable a toute l'equipe

### Ce qu'un agent doit faire conceptuellement

1. le host se connecte au serveur MCP
2. il s'authentifie avec le PAT
3. il decouvre tools/resources/prompts
4. il choisit quoi appeler selon la demande utilisateur

---

## 9. Surface MCP actuelle exposee par le projet

L'enregistrement des capacites est fait dans [../../src/Mcp/InfraFlowSculptor.Mcp/Program.cs](../../src/Mcp/InfraFlowSculptor.Mcp/Program.cs).

Aujourd'hui, la surface reelle est :

- **8 tools**
- **2 resources**
- **1 prompt**

## 9.1. Tools

| Tool | Classe source | Type | Role |
|---|---|---|---|
| `list_repository_topologies` | [../../src/Mcp/InfraFlowSculptor.Mcp/Tools/DiscoveryTools.cs](../../src/Mcp/InfraFlowSculptor.Mcp/Tools/DiscoveryTools.cs) | lecture | Retourne les topologies de depot supportees |
| `list_supported_resource_types` | [../../src/Mcp/InfraFlowSculptor.Mcp/Tools/DiscoveryTools.cs](../../src/Mcp/InfraFlowSculptor.Mcp/Tools/DiscoveryTools.cs) | lecture | Retourne le catalogue des types de ressources Azure supportes |
| `draft_project_from_prompt` | [../../src/Mcp/InfraFlowSculptor.Mcp/Tools/ProjectDraftTools.cs](../../src/Mcp/InfraFlowSculptor.Mcp/Tools/ProjectDraftTools.cs) | lecture | Transforme une demande libre en draft structure sans effet de bord |
| `validate_project_draft` | [../../src/Mcp/InfraFlowSculptor.Mcp/Tools/ProjectDraftTools.cs](../../src/Mcp/InfraFlowSculptor.Mcp/Tools/ProjectDraftTools.cs) | lecture | Verifie les champs manquants, erreurs et warnings d'un draft |
| `create_project_from_draft` | [../../src/Mcp/InfraFlowSculptor.Mcp/Tools/ProjectCreationTools.cs](../../src/Mcp/InfraFlowSculptor.Mcp/Tools/ProjectCreationTools.cs) | mutation | Cree le projet, l'infrastructure de base et les ressources du draft valide |
| `preview_iac_import` | [../../src/Mcp/InfraFlowSculptor.Mcp/Tools/IacImportTools.cs](../../src/Mcp/InfraFlowSculptor.Mcp/Tools/IacImportTools.cs) | lecture | Analyse une source IaC et produit un preview |
| `apply_import_preview` | [../../src/Mcp/InfraFlowSculptor.Mcp/Tools/IacImportTools.cs](../../src/Mcp/InfraFlowSculptor.Mcp/Tools/IacImportTools.cs) | mutation | Applique un preview valide pour creer un projet importe |
| `generate_project_bicep` | [../../src/Mcp/InfraFlowSculptor.Mcp/Tools/BicepGenerationTools.cs](../../src/Mcp/InfraFlowSculptor.Mcp/Tools/BicepGenerationTools.cs) | mutation semi-lourde | Lance la generation Bicep pour un projet existant |

### Notes importantes sur les tools

- `preview_iac_import` supporte actuellement **`arm-json`** en V1
- `create_project_from_draft` et `apply_import_preview` sont les tools mutateurs critiques
- `draft_project_from_prompt` et `validate_project_draft` existent justement pour **eviter de muter trop tot**

## 9.2. Resources

| Resource | Classe source | Role |
|---|---|---|
| `ifs://projects/{projectId}/summary` | [../../src/Mcp/InfraFlowSculptor.Mcp/Resources/ProjectResources.cs](../../src/Mcp/InfraFlowSculptor.Mcp/Resources/ProjectResources.cs) | Retourne un resume structure d'un projet |
| `ifs://imports/{previewId}` | [../../src/Mcp/InfraFlowSculptor.Mcp/Imports/Resources/ImportPreviewResources.cs](../../src/Mcp/InfraFlowSculptor.Mcp/Imports/Resources/ImportPreviewResources.cs) | Retourne le contenu complet d'un preview d'import stocke |

### Pourquoi ces resources existent

Parce qu'un agent a parfois besoin de **relire** un resultat sans repasser par le tool qui l'a calcule.

Exemple :

- `preview_iac_import` renvoie un resume compact
- `ifs://imports/{previewId}` permet de relire le preview detaille

## 9.3. Prompt

| Prompt | Classe source | Role |
|---|---|---|
| `project_creation_guide` | [../../src/Mcp/InfraFlowSculptor.Mcp/Prompts/ProjectCreationPrompts.cs](../../src/Mcp/InfraFlowSculptor.Mcp/Prompts/ProjectCreationPrompts.cs) | Guide l'agent sur le bon workflow de creation de projet |

Le point cle de ce prompt est qu'il rappelle a l'agent :

- d'appeler d'abord les tools de discovery
- de passer par le draft
- de ne **jamais deviner** la topologie du depot
- de demander confirmation avant creation

---

## 10. Comment un agent doit utiliser ce serveur MCP

### Regle generale

Un agent n'est pas cense appeler les tools au hasard.

Il doit suivre la logique suivante :

1. se connecter au serveur MCP
2. identifier si la demande est de la **discovery**, de la **clarification**, de la **mutation** ou de la **lecture**
3. appeler les tools/resources/prompts dans le bon ordre
4. demander confirmation avant les mutations irreversibles

## 10.1. Workflow type : creation de projet par langage naturel

Sequence recommandee :

1. `list_repository_topologies`
2. `list_supported_resource_types`
3. `draft_project_from_prompt`
4. si `RequiresClarification`, poser les questions manquantes
5. `validate_project_draft`
6. quand le draft est `ReadyToCreate`, montrer un resume a l'utilisateur
7. demander confirmation
8. `create_project_from_draft`
9. si besoin ensuite : `generate_project_bicep`

### Pourquoi cet ordre est bon

Parce qu'il separe clairement :

- l'intention utilisateur
- la clarification
- la validation
- la mutation effective

## 10.2. Workflow type : import IaC

Sequence recommandee :

1. `preview_iac_import`
2. relire au besoin `ifs://imports/{previewId}`
3. expliquer les gaps a l'utilisateur
4. choisir les environments et filtres de ressources si necessaire
5. `apply_import_preview`
6. si besoin : `generate_project_bicep`

### Le point critique ici

L'import est en **deux etapes** pour une bonne raison :

- preview d'abord
- mutation ensuite

## 10.3. Workflow type : lecture d'un projet existant

Sequence recommandee :

1. lire `ifs://projects/{projectId}/summary`
2. expliquer ce que le projet contient
3. proposer ensuite une generation ou un import si la demande le justifie

---

## 11. Guide de lecture du code

Si tu veux comprendre le systeme sans te perdre, ouvre les fichiers dans cet ordre.

1. [../../src/Mcp/InfraFlowSculptor.Mcp/Program.cs](../../src/Mcp/InfraFlowSculptor.Mcp/Program.cs)
Ce fichier te montre le runtime reel : transport HTTP, auth obligatoire, registration des tools/resources/prompts, puis mapping du endpoint MCP avec les options chargees.

2. [../../src/Mcp/InfraFlowSculptor.Mcp/Common/McpOptions.cs](../../src/Mcp/InfraFlowSculptor.Mcp/Common/McpOptions.cs)
Lis-le juste apres `Program.cs` pour voir quelles valeurs sont des **defaults** (`http://127.0.0.1:5258` et `/mcp`) et quelles valeurs sont configurables.

3. [../../.vscode/mcp.json](../../.vscode/mcp.json)
Tu vois comment VS Code se connecte au serveur et injecte le PAT, mais il faut se concentrer sur l'entree `infraflowsculptor-mcp` parmi les autres serveurs du workspace.

4. [../../src/Mcp/InfraFlowSculptor.Mcp/Prompts/ProjectCreationPrompts.cs](../../src/Mcp/InfraFlowSculptor.Mcp/Prompts/ProjectCreationPrompts.cs)
Tres bon point d'entree pour comprendre **comment un agent est suppose utiliser les tools**.

5. [../../src/Mcp/InfraFlowSculptor.Mcp/Tools/DiscoveryTools.cs](../../src/Mcp/InfraFlowSculptor.Mcp/Tools/DiscoveryTools.cs)
Tu vois les metadata statiques exposees au host.

6. [../../src/Mcp/InfraFlowSculptor.Mcp/Tools/ProjectDraftTools.cs](../../src/Mcp/InfraFlowSculptor.Mcp/Tools/ProjectDraftTools.cs)
Le coeur du pattern conversationnel se trouve ici : on extrait l'intention avant toute creation.

7. [../../src/Mcp/InfraFlowSculptor.Mcp/Tools/ProjectCreationTools.cs](../../src/Mcp/InfraFlowSculptor.Mcp/Tools/ProjectCreationTools.cs)
Lis-le pour comprendre ce qui se passe quand on cree vraiment un projet.

8. [../../src/Mcp/InfraFlowSculptor.Mcp/Tools/IacImportTools.cs](../../src/Mcp/InfraFlowSculptor.Mcp/Tools/IacImportTools.cs)
Tres utile pour comprendre la difference entre preview et apply.

9. [../../src/Mcp/InfraFlowSculptor.Mcp/Resources/ProjectResources.cs](../../src/Mcp/InfraFlowSculptor.Mcp/Resources/ProjectResources.cs)
Montre comment le projet expose des resources de lecture propres.

10. [../../src/Mcp/InfraFlowSculptor.Mcp/Imports/Resources/ImportPreviewResources.cs](../../src/Mcp/InfraFlowSculptor.Mcp/Imports/Resources/ImportPreviewResources.cs)
Montre comment un preview stocke est relu via resource.

10. [../../src/Api/InfraFlowSculptor.Api/Controllers/PersonalAccessTokenController.cs](../../src/Api/InfraFlowSculptor.Api/Controllers/PersonalAccessTokenController.cs)
Point d'entree API pour creer, lister et revoquer les PAT.

11. [../../src/Api/InfraFlowSculptor.Application/PersonalAccessTokens/Commands/CreatePersonalAccessToken/CreatePersonalAccessTokenCommandHandler.cs](../../src/Api/InfraFlowSculptor.Application/PersonalAccessTokens/Commands/CreatePersonalAccessToken/CreatePersonalAccessTokenCommandHandler.cs)
Montre comment le use case applicatif cree un PAT pour l'utilisateur courant.

12. [../../src/Api/InfraFlowSculptor.Domain/PersonalAccessTokenAggregate/PersonalAccessToken.cs](../../src/Api/InfraFlowSculptor.Domain/PersonalAccessTokenAggregate/PersonalAccessToken.cs)
Lis ce fichier pour comprendre la verite de domaine : generation, hash, prefixe, expiration, revocation, usage.

13. [../../src/Api/InfraFlowSculptor.Infrastructure/Auth/PersonalAccessTokenAuthenticationHandler.cs](../../src/Api/InfraFlowSculptor.Infrastructure/Auth/PersonalAccessTokenAuthenticationHandler.cs)
Ici, tu comprends comment le Bearer token devient un utilisateur authentifie pour MCP.

14. [../../src/Front/src/app/features/settings/settings.component.ts](../../src/Front/src/app/features/settings/settings.component.ts) et [../../src/Front/src/app/features/settings/create-pat-dialog/create-pat-dialog.component.ts](../../src/Front/src/app/features/settings/create-pat-dialog/create-pat-dialog.component.ts)
Ces fichiers te montrent l'experience utilisateur reelle pour creer et gerer les PAT.

---

## 12. Les pieges a eviter

1. Ne pas confondre le **PAT interne MCP** avec les PAT GitHub / Azure DevOps documentes ailleurs.
2. Ne jamais stocker le token plaintext dans le depot ou dans `mcp.json`.
3. Ne pas oublier que la valeur du PAT n'est visible **qu'une seule fois** a la creation.
4. Ne pas appeler `create_project_from_draft` si le draft n'est pas pret.
5. Ne pas deviner une topologie de depot absente du prompt.
6. Ne pas oublier que `/mcp` est protege, alors que `/health` et `/alive` sont anonymes.
7. Ne pas oublier que l'import IaC V1 supporte actuellement **ARM JSON** et pas n'importe quel format.
8. Ne pas transformer MCP en backend parallele qui parle directement au `DbContext`.

---

## 13. Questions de review utiles

Quand tu relis ou fais evoluer cette partie du projet, pose-toi au minimum ces questions :

1. Est-ce que le serveur MCP reste un adaptateur et non un second backend metier ?
2. Est-ce que les tools mutateurs restent prudents et explicites ?
3. Est-ce que le PAT est toujours hash avant persistence ?
4. Est-ce que la valeur plaintext reste one-shot uniquement ?
5. Est-ce que `LastUsedAt` est bien persiste apres authentification reussie ?
6. Est-ce que le host VS Code peut se connecter sans secret committe ?
7. Est-ce que la surface MCP exposee est bien celle attendue : tools, resources, prompts ?
8. Est-ce que les nouveaux tools ont une responsabilite claire ?

---

## 14. Ce qu'il faut absolument retenir

Si tu ne retiens que 10 idees, retiens celles-ci :

1. MCP est une interface standard entre un agent IA et ton systeme.
2. Dans ce projet, MCP est implemente comme un **serveur HTTP ASP.NET Core**.
3. La route MCP reelle est `/mcp`.
4. L'acces a `/mcp` est protege par un **Bearer PAT**.
5. Le PAT interne du projet n'est pas le meme sujet que les PAT GitHub / Azure DevOps.
6. La valeur complete d'un PAT n'est affichee qu'une seule fois a la creation.
7. Le serveur ne persiste que le **hash** du token, pas sa valeur brute.
8. La surface MCP actuelle expose 8 tools, 2 resources et 1 prompt.
9. Le bon workflow agent passe par la discovery, la clarification, puis seulement la mutation.
10. Si tu ouvres les fichiers dans l'ordre propose, tu comprendras le systeme sans te perdre.

---

## 15. Pour aller plus loin

Pour continuer dans le bon ordre :

1. lire [overview.md](overview.md) pour replacer MCP dans l'architecture globale
2. lire [cqrs-patterns.md](cqrs-patterns.md) pour mieux comprendre les handlers reutilises par les tools
3. lire [mcp-v1-implementation-plan.md](mcp-v1-implementation-plan.md) comme **document de design initial**, pas comme photographie parfaite du runtime actuel
4. lire [../features/push-bicep-to-git.md](../features/push-bicep-to-git.md) si tu veux comparer le PAT interne MCP avec les PAT externes GitHub / Azure DevOps
5. relire le skill projet [../../.github/skills/mcp-dotnet-server/SKILL.md](../../.github/skills/mcp-dotnet-server/SKILL.md)