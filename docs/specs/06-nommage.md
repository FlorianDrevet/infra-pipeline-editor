# 06 — Nommage

> Statut : v0 rétro-spécification (code au 2026-10-02).

## 1. Objectif

Chaque ressource a un **nom logique** saisi par l'utilisateur (`orders-api`). Le **nom Azure**
réel est calculé pour chaque environnement en appliquant un **gabarit** (`{name}-{resourceAbbr}{suffix}`)
qui combine le nom logique, l'abréviation du type et les attributs de l'environnement. Le nommage
se règle au niveau projet et peut être surchargé au niveau configuration.

## 2. Gabarit

### 2.1 Jetons

| Jeton | Remplacé par (calcul affiché à l'écran) | Remplacé par (dans le Bicep généré) |
|---|---|---|
| `{name}` | Nom logique de la ressource | Nom logique |
| `{prefix}` | Préfixe de l'environnement | `env.envPrefix` |
| `{suffix}` | Suffixe de l'environnement | `env.envSuffix` |
| `{env}` | Nom de l'environnement | `env.envName` |
| `{envShort}` | Nom court de l'environnement | `env.envShort` |
| `{location}` | Région de l'environnement (nom de l'énumération, exemple `FranceCentral`) | `env.location` (clé de région, exemple `francecentral`) — voir EC-NOM-03 |
| `{resourceType}` | Type de ressource (exemple `KeyVault`) | Type de ressource |
| `{resourceAbbr}` | Abréviation du type (section 3) | Abréviation du type |

Les jetons sont insensibles à la casse.

**RG-NOM-01 — Jetons autorisés.** Un gabarit ne contient que les huit jetons ci-dessus. Un jeton
inconnu est refusé avec la liste des jetons autorisés (gabarit par défaut du projet, gabarits par
type projet et configuration).

**RG-NOM-02 — Texte fixe.** En dehors des jetons, le gabarit ne contient que lettres, chiffres,
tirets et soulignés.

**RG-NOM-03 — Texte fixe compatible avec le type.** Pour un gabarit par type, le texte fixe doit
respecter les caractères autorisés par Azure pour ce type (section 5). Exemple : pas de tiret dans
un gabarit `StorageAccount`.

**RG-NOM-04 — Longueur.** 500 caractères maximum.

### 2.2 Niveaux

| Niveau | Gabarit par défaut | Gabarits par type | Abréviations par type |
|---|---|---|---|
| Projet | Oui | Oui | Oui |
| Configuration | Oui | Oui | Oui |

À la création d'un projet, IFS pose :

| Portée | Gabarit |
|---|---|
| Par défaut | `{name}-{resourceAbbr}{suffix}` |
| `ResourceGroup` | `{resourceAbbr}-{name}{suffix}` |
| `StorageAccount` | `{name}{resourceAbbr}{envShort}` |
| `ContainerRegistry` | `{name}{resourceAbbr}{envShort}` |

Un gabarit par type doit viser un type du catalogue (section 5).

**UC-NOM-01 à UC-NOM-06** — Définir/retirer le gabarit par défaut, un gabarit par type, une
abréviation par type, au niveau projet puis au niveau configuration (Contributor).

**UC-NOM-07 — Choisir l'héritage d'une configuration.** Une configuration « utilise le nommage du
projet » (par défaut) ou « définit le sien ».

## 3. Abréviations

**RG-NOM-05 — Format.** 1 à 10 caractères, minuscules et chiffres uniquement.

Abréviations par défaut du catalogue :

| Type | Abréviation | Type | Abréviation |
|---|---|---|---|
| KeyVault | `kv` | ContainerApp | `ca` |
| RedisCache | `redis` | LogAnalyticsWorkspace | `law` |
| StorageAccount | `stg` | ApplicationInsights | `appi` |
| ResourceGroup | `rg` | CosmosDb | `cosmos` |
| AppServicePlan | `asp` | SqlServer | `sql` |
| WebApp | `app` | SqlDatabase | `sqldb` |
| FunctionApp | `func` | ServiceBusNamespace | `sb` |
| UserAssignedIdentity | `id` | ContainerRegistry | `acr` |
| AppConfiguration | `appcs` | EventHubNamespace | `evhns` |
| ContainerAppEnvironment | `cae` | DocumentIntelligence | `docint` |

VirtualNetwork n'a pas d'abréviation dans le catalogue (voir EC-NOM-08).

## 4. Calcul du nom

### 4.1 Calcul affiché à l'écran (aperçu et contrôle de disponibilité)

Pour une ressource de type *T* dans une configuration *C* d'un projet *P* :

**Gabarit retenu**, dans cet ordre :
1. si *C* définit son propre nommage **et** a un gabarit pour *T* → ce gabarit ;
2. sinon, si *P* a un gabarit pour *T* → ce gabarit ;
3. sinon, si *C* définit son propre nommage **et** a un gabarit par défaut → ce gabarit ;
4. sinon, si *P* a un gabarit par défaut → le **gabarit recommandé** du type s'il en a un
   (`StorageAccount`, `ContainerRegistry` : `{name}{resourceAbbr}{envShort}`), sinon le gabarit
   par défaut de *P* ;
5. sinon → `{name}`.

**Abréviation retenue** : abréviation de *C* (si elle définit son nommage) → de *P* → du catalogue.

Le nom est calculé pour chaque environnement du projet. Sans environnement, aucun nom n'est
calculé.

### 4.2 Calcul dans le Bicep généré

Le Bicep ne contient pas les noms calculés : il contient des **fonctions** (`BuildResourceName`,
`Build<Type>Name`) qui appliquent le gabarit à l'objet environnement au moment du déploiement.

Gabarits et abréviations utilisés, **tout ou rien** :
- si la configuration utilise le nommage du projet → uniquement ceux du projet ;
- sinon → uniquement ceux de la configuration, **sans repli sur le projet**.

Pour un type sans gabarit propre, le gabarit par défaut s'applique ; sans gabarit par défaut, le
nom logique est utilisé tel quel. Le gabarit recommandé du type n'est **pas** appliqué.

Le resource group suit le même mécanisme avec le type `ResourceGroup`.

### 4.3 Nom personnalisé

Le modèle de ressource prévoit un nom Azure imposé qui remplacerait le calcul. Aucune action ne
permet de le définir et la génération ne le lit pas (voir EC-NOM-04).

## 5. Contraintes Azure par type

| Type | Caractères autorisés | Longueur | Gabarit recommandé |
|---|---|---|---|
| ContainerRegistry | lettres et chiffres | 5–50 | `{name}{resourceAbbr}{envShort}` |
| StorageAccount | minuscules et chiffres | 3–24 | `{name}{resourceAbbr}{envShort}` |
| KeyVault | lettres, chiffres, tirets | 3–24 | — |
| SqlServer | minuscules, chiffres, tirets | 1–63 | — |
| SqlDatabase | lettres, chiffres, tirets, soulignés, points | 1–128 | — |
| CosmosDb | minuscules, chiffres, tirets | 3–44 | — |
| RedisCache | lettres, chiffres, tirets | 1–63 | — |
| ServiceBusNamespace | lettres, chiffres, tirets | 6–50 | — |
| EventHubNamespace | lettres, chiffres, tirets | 6–50 | — |
| LogAnalyticsWorkspace | lettres, chiffres, tirets | 4–63 | — |
| ApplicationInsights | lettres, chiffres, tirets, soulignés, points, parenthèses | 1–260 | — |
| AppServicePlan | lettres, chiffres, tirets | 1–40 | — |
| WebApp | lettres, chiffres, tirets | 2–60 | — |
| FunctionApp | lettres, chiffres, tirets | 2–60 | — |
| UserAssignedIdentity | lettres, chiffres, tirets, soulignés | 3–128 | — |
| AppConfiguration | lettres, chiffres, tirets, soulignés | 5–50 | — |
| ContainerAppEnvironment | minuscules, chiffres, tirets | 2–32 | — |
| ContainerApp | minuscules, chiffres, tirets | 2–32 | — |
| ResourceGroup | lettres, chiffres, tirets, soulignés, points, parenthèses | 1–90 | — |

VirtualNetwork et DocumentIntelligence n'ont pas de contrainte déclarée.

## 6. Contrôle de disponibilité

**UC-NOM-08 — Vérifier un nom.** Pour une ressource (type, nom logique, configuration), IFS calcule
le nom de chaque environnement (§ 4.1) et renvoie pour chacun un statut :

| Statut | Sens |
|---|---|
| `current` | Le nom n'a pas changé par rapport au nom enregistré : rien à vérifier. |
| `invalid` | Nom vide, trop court, trop long ou avec des caractères interdits pour le type (§ 5). |
| `available` | Le nom semble libre. |
| `unavailable` | Le nom semble déjà pris. |
| `unknown` | Type non vérifiable, ou erreur de résolution. |

**RG-NOM-06 — Méthode de vérification.** La disponibilité est déduite d'une **résolution DNS
publique** du nom de domaine du service, pour 10 types :

| Type | Domaine testé |
|---|---|
| ContainerRegistry | `<nom>.azurecr.io` |
| StorageAccount | `<nom>.blob.core.windows.net` |
| KeyVault | `<nom>.vault.azure.net` |
| RedisCache | `<nom>.redis.cache.windows.net` |
| AppConfiguration | `<nom>.azconfig.io` |
| ServiceBusNamespace, EventHubNamespace | `<nom>.servicebus.windows.net` |
| WebApp, FunctionApp | `<nom>.azurewebsites.net` |
| SqlServer | `<nom>.database.windows.net` |

Le nom résout → `unavailable`. Domaine inexistant → `available`. Autre erreur → `unknown`.
L'abonnement de l'environnement est transmis mais n'est pas utilisé par cette méthode.

**RG-NOM-07 — Diagnostic.** Le même contrôle alimente le diagnostic « nom indisponible »
([16](16-diagnostics.md)).

## 7. Écarts constatés

- **EC-NOM-01 (D05) — Deux calculs différents.** L'aperçu et le contrôle de disponibilité (§ 4.1)
  font une cascade fine type par type avec repli sur le projet et gabarit recommandé ; la génération
  (§ 4.2) fait un choix tout-ou-rien sans repli ni gabarit recommandé. Pour un type non surchargé
  dans une configuration qui définit son propre nommage, **l'écran affiche un nom et le Bicep en
  produit un autre**, potentiellement invalide pour Azure (tirets dans un nom de registre).
- **EC-NOM-02 — Vérification DNS au lieu de l'API Azure.** La disponibilité n'utilise pas l'API
  `checkNameAvailability` d'Azure ; le résultat peut différer selon le pare-feu, les zones DNS
  privées, ou pour un nom réservé mais non résolvable.
- **EC-NOM-03 — `{location}` différent entre écran et Bicep.** L'écran remplace `{location}` par le
  nom d'énumération (`FranceCentral`), le Bicep par la clé de région (`francecentral`).
- **EC-NOM-04 — Nom personnalisé mort.** Le champ « nom imposé » existe mais n'est ni saisissable ni
  généré.
- **EC-NOM-05 — Contrôles inégaux.** Le gabarit par défaut d'une configuration n'est contrôlé que sur
  ses caractères fixes, pas sur ses jetons ; ceux du projet et des types le sont.
- **EC-NOM-06 — Le gabarit recommandé ne protège que l'écran.** Il ne s'applique que dans l'aperçu,
  et seulement si le projet a un gabarit par défaut.
- **EC-NOM-07 — Pas de contrôle à la génération.** Un nom invalide pour Azure n'empêche pas la
  génération ; l'erreur apparaît au déploiement.
- **EC-NOM-08 — VirtualNetwork sans abréviation.** Le type n'est pas dans le catalogue
  d'abréviations ni dans les contraintes de nom ; `{resourceAbbr}` y donne une valeur de repli
  (**valeur exacte non vérifiée**).

## 8. Questions ouvertes

- **Q-NOM-01** — Quelle cascade est la bonne (fine ou tout-ou-rien) ? Elle doit être unique et servir
  à la fois l'aperçu et la génération.
- **Q-NOM-02** — Le Bicep doit-il contenir les noms déjà calculés (lisibles, vérifiables) plutôt que
  des fonctions évaluées au déploiement ?
- **Q-NOM-03** — Utiliser l'API Azure `checkNameAvailability` (exige une identité avec accès aux
  abonnements des environnements) ?
- **Q-NOM-04** — Ajouter des jetons (`{project}`, `{config}`, `{instance}`) et des transformations
  (minuscules, troncature) ?
- **Q-NOM-05** — Rendre le nom personnalisé utilisable (cas des ressources aux noms historiques) ?
- **Q-NOM-06** — Bloquer la génération quand un nom calculé est invalide ?
