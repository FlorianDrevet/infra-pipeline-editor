# 10 — Configuration applicative

> Statut : v0 rétro-spécification (code au 2026-10-02).
> Feature-map : F10 (app settings, `keep`), F10b (clés App Configuration, `fix`, D07), F10d (paramètres sécurisés, `keep`).

## 1. Objectif

Fournir aux applications leur configuration sans la coder en dur : variables d'environnement
(app settings), clés d'une App Configuration, secrets lus dans un Key Vault, valeurs produites par
d'autres ressources (URI, chaîne de connexion), valeurs fournies par les pipelines.

## 2. Sources de valeur

Les app settings et les clés App Configuration partagent les mêmes six sources :

| Source | Contenu saisi | Valeur à l'exécution |
|---|---|---|
| **Statique par environnement** | Une valeur par environnement | La valeur de l'environnement déployé. |
| **Output de ressource** | Ressource source + nom d'output (section 3) | Propriété de la ressource, calculée au déploiement. |
| **Référence Key Vault** | Key Vault + nom du secret + mode d'alimentation | Référence Key Vault résolue à l'exécution par Azure. |
| **Output sensible exporté vers Key Vault** | Ressource source + output sensible + Key Vault + nom du secret | Le déploiement écrit l'output dans le Key Vault ; l'application lit le secret. |
| **Variable group** | Variable group + nom de la variable | Valeur fournie par la pipeline de release. |
| **Variable group vers Key Vault** | Variable group + variable + Key Vault + nom du secret + mode | La pipeline fournit la valeur, écrite dans le Key Vault ; l'application lit le secret. |

### Mode d'alimentation d'un secret (SecretValueAssignment)

| Mode | Sens |
|---|---|
| `DirectInKeyVault` (défaut) | Le secret est déjà dans le Key Vault ; IFS ne fait que le référencer. |
| `ViaBicepparam` | La valeur du secret passe par le fichier de paramètres / la pipeline, et le Bicep l'écrit dans le Key Vault. |

### Règles communes

**RG-APP-01 — Nom de secret Key Vault.** 1 à 127 caractères, lettres, chiffres et tirets.

**RG-APP-02 — Cohérence variable group.** Si un variable group est choisi, le nom de variable est
obligatoire (256 caractères max) ; le variable group doit appartenir au projet.

**RG-APP-03 — Export vers Key Vault.** L'export exige à la fois un output source et une référence
Key Vault (Key Vault + nom de secret).

**RG-APP-04 — Output valide.** L'output doit exister dans le catalogue du type de la ressource
source (section 3).

**RG-APP-05 — Key Vault réel.** La ressource référencée comme Key Vault doit être de type KeyVault.

## 3. Catalogue des outputs

| Type source | Outputs (sensible = *) |
|---|---|
| KeyVault | `vaultUri`, `vaultName` |
| RedisCache | `hostName`, `sslPort`, `connectionString`*, `primaryKey`* |
| StorageAccount | `storageAccountName`, `primaryBlobEndpoint`, `primaryQueueEndpoint`, `primaryTableEndpoint`, `connectionString`*, `primaryKey`* |
| AppConfiguration | `endpoint` |
| ApplicationInsights | `connectionString`, `instrumentationKey` |
| LogAnalyticsWorkspace | `workspaceId` |
| CosmosDb | `documentEndpoint`, `primaryConnectionString`*, `primaryKey`* |
| SqlServer | `fullyQualifiedDomainName`, `connectionString` |
| ContainerAppEnvironment | `defaultDomain`, `staticIp` |
| UserAssignedIdentity | `principalId`, `clientId` |
| AppServicePlan | `id` |
| WebApp, FunctionApp | `defaultHostName` |
| ContainerApp | `fqdn` |
| SqlDatabase | `id` |
| ServiceBusNamespace | `connectionString` |
| DocumentIntelligence | `endpoint`, `id` |

Chaque output a un nom, une description et une expression Bicep.

**UC-APP-01 — Lister les outputs d'une ressource** (pour choisir la source d'un app setting).

## 4. App settings

Applicables à : **WebApp, FunctionApp, ContainerApp**.

| Champ | Règles |
|---|---|
| Nom | Obligatoire, 256 caractères max. Unique par ressource (contrainte en base). |
| Source | Une des six sources (section 2). |

**UC-APP-02 — Ajouter un app setting** (Contributor). L'utilisateur choisit la source et saisit ses
champs. Pour une référence Key Vault, la réponse indique si la ressource a déjà le rôle Key Vault
Secrets User sur ce Key Vault (sinon l'écran propose de l'accorder).

**UC-APP-03 — Modifier un app setting statique** : nom et valeurs par environnement (au moins une
valeur). Les autres sources ne se modifient pas : il faut supprimer et recréer.

**UC-APP-04 — Supprimer un app setting.**

**UC-APP-05 — Lister les app settings** d'une ressource.

**UC-APP-06 — Importer des app settings depuis un fichier.** L'écran lit un fichier et crée des app
settings statiques. Formats :

| Format | Fichiers exemples | Règle de lecture |
|---|---|---|
| JSON .NET | `appsettings.json`, `appsettings.Development.json` | Objets aplatis avec `__` comme séparateur. |
| JSON Functions | `local.settings.json` | Section `Values`, aplatie avec `__`. |
| .env | `.env`, `.env.production` | Lignes `CLÉ=valeur`. |
| Properties Java | `application.properties` | Lignes `clé=valeur` ou `clé: valeur`. |
| YAML Java | `application.yml` | Aplatie avec `.`. |

**UC-APP-07 — Vérifier l'accès Key Vault** d'une ressource à un Key Vault (rôle Key Vault Secrets
User présent ?).

### Génération des app settings

**RG-APP-06 — Paramètre de la ressource.** Une WebApp ou FunctionApp reçoit un paramètre
`appSettings` ; une ContainerApp un paramètre `envVars`.

**RG-APP-07 — Par source.**
- Statique : valeurs écrites dans le fichier de paramètres de chaque environnement.
- Output : la ressource source reçoit l'output correspondant, branché dans le paramètre.
- Référence Key Vault : la valeur est une référence Key Vault vers le secret.
- Export sensible et `ViaBicepparam` : un module `kvSecrets` écrit le secret dans le Key Vault ;
  l'output exporté est marqué sécurisé.
- Variable group : la valeur est injectée par la pipeline de release (paramètres surchargés).

**RG-APP-08 — Contrôle d'accès.** Le diagnostic « accès Key Vault manquant » signale une référence
sans rôle Key Vault Secrets User (voir [16](16-diagnostics.md)).

## 5. Clés App Configuration

Applicables à : **AppConfiguration**.

| Champ | Règles |
|---|---|
| Clé | Obligatoire, 512 caractères max. Unique par App Configuration (contrainte en base sur la clé seule). |
| Label | Facultatif, 128 caractères max. |
| Source | Une des six sources (section 2). Valeurs par environnement pour la source statique. |

**UC-APP-08 — Ajouter une clé** (Contributor).

**UC-APP-09 — Supprimer une clé.**

**UC-APP-10 — Lister les clés.**

Une App Configuration qui référence un Key Vault est concernée par l'analyse d'impact RBAC
([09 § 6](09-identites-et-rbac.md)).

## 6. Paramètres sécurisés

Certains modules Bicep ont des paramètres sensibles, jamais écrits en clair dans les fichiers :

| Type | Paramètre sensible | Condition |
|---|---|---|
| SqlServer | Mot de passe administrateur | Toujours |
| WebApp, FunctionApp | Mot de passe du registre | Mode conteneur avec authentification `AdminCredentials` |
| ContainerApp | Mot de passe du registre | Authentification `AdminCredentials` |

**RG-APP-09 — Valeur fournie par la pipeline.** La pipeline de release surcharge ces paramètres.
Par défaut, elle attend une variable de pipeline au nom dérivé du module et du paramètre.

**UC-APP-11 — Associer un paramètre sécurisé à un variable group** (Contributor). L'utilisateur
choisit le variable group et le nom de variable (lettre ou souligné, puis lettres, chiffres,
soulignés). Le bootstrap crée alors la variable (secrète) dans ce groupe.

**UC-APP-12 — Retirer l'association** : envoyer une association sans variable group la supprime ;
il ne faut alors pas fournir de nom de variable.

**UC-APP-13 — Lister les associations** d'une ressource.

## 7. Écarts constatés

- **EC-APP-01 (D07) — Clés App Configuration jamais générées.** Les clés sont saisies, stockées,
  affichées, et ne sont lues par aucune étape de la génération. Rien n'en sort dans le Bicep.
- **EC-APP-02 — Clé unique sans label.** L'unicité porte sur la clé seule : impossible d'avoir la même
  clé avec deux labels, alors que c'est l'usage normal des labels.
- **EC-APP-03 — Doublon de nom d'app setting en erreur technique.** L'unicité du nom n'est garantie que
  par la base : un doublon produit une erreur technique au lieu d'un message clair (**non vérifié à
  l'exécution**).
- **EC-APP-04 — Modification limitée.** Seuls les app settings statiques sont modifiables.
- **EC-APP-05 — Lectures sans contrôle.** La liste des outputs disponibles et la vérification d'accès
  Key Vault ne vérifient pas que l'utilisateur a accès à la ressource (EC-ACC-01).
- **EC-APP-06 — Pas d'output cross-config explicite.** Un app setting peut utiliser l'output d'une
  ressource d'une autre configuration seulement si une référence cross-config existe ; le parcours
  n'est pas guidé à l'écran.

## 8. Questions ouvertes

- **Q-APP-01** — Générer les clés App Configuration (ressources `keyValues`) ou retirer la fonction ?
- **Q-APP-02** — Une même définition de configuration doit-elle pouvoir alimenter à la fois des app
  settings et des clés App Configuration ?
- **Q-APP-03** — Ajouter des outputs (serveur de connexion ACR, nom d'hôte Service Bus, chaînes de
  connexion Event Hubs) ?
- **Q-APP-04** — Faire évoluer l'import vers une détection automatique depuis le dépôt de code ?
