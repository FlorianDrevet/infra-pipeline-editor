# 03 — Projets

> Statut : v0 rétro-spécification (code au 2026-10-02).
> Hors de ce document : topologie et dépôts ([04](04-topologie-et-depots.md)), nommage ([06](06-nommage.md)), droits ([02](02-acces-et-droits.md)).

## 1. Objectif

Le projet est le conteneur racine : il porte l'équipe, les environnements de déploiement, les
conventions communes (nommage, tags), les variable groups Azure DevOps, la topologie des dépôts et
les configurations d'infrastructure.

## 2. Données d'un projet

| Champ | Type / valeurs | Obligatoire | Règles | Défaut |
|---|---|---|---|---|
| Nom | Texte | Oui | 3 à 80 caractères via l'assistant ; 1 à 80 via la création simple (voir EC-PRJ-01) | — |
| Description | Texte | Non | 500 caractères max (création simple) ; 1 000 max (assistant) | — |
| Topologie (LayoutPreset) | `AllInOne`, `SplitInfraCode`, `MultiRepo` | Oui | Voir [04](04-topologie-et-depots.md) | `MultiRepo` en création simple ; choisie dans l'assistant |
| Membres | Liste (utilisateur, rôle) | — | Le créateur est Owner | Créateur Owner |
| Environnements | Liste ordonnée | — | Voir section 4 | Aucun (création simple) |
| Gabarit de nommage par défaut | Gabarit | — | Voir [06](06-nommage.md) | `{name}-{resourceAbbr}{suffix}` |
| Gabarits par type | Dictionnaire type → gabarit | — | — | `ResourceGroup` : `{resourceAbbr}-{name}{suffix}` ; `StorageAccount` et `ContainerRegistry` : `{name}{resourceAbbr}{envShort}` |
| Abréviations par type | Dictionnaire type → abréviation | — | — | Aucune (catalogue par défaut) |
| Tags | Liste (clé, valeur) | — | Voir section 6 | Aucun |
| Variable groups | Liste de noms | — | Voir section 7 | Aucun |
| Agent pool | Texte | Non | 200 caractères max ; vide = aucun | Aucun |
| Dépôts projet | Liste | — | Voir [04](04-topologie-et-depots.md) | Aucun |

## 3. Cycle de vie

### UC-PRJ-01 — Créer un projet (création simple)

Saisie : nom, description. IFS crée le projet avec le créateur Owner, la topologie
`MultiRepo` et les gabarits de nommage par défaut. Aucun environnement, aucun dépôt.

### UC-PRJ-02 — Créer un projet avec l'assistant

Écran en 5 étapes (le brouillon est sauvegardé dans le navigateur, clé `ifs:create-project-draft`) :

1. **Identité** : nom, description.
2. **Topologie** : `AllInOne`, `SplitInfraCode` ou `MultiRepo`.
3. **Environnements** : au moins un. Pour chacun : nom, nom court, préfixe, suffixe, région,
   abonnement Azure, ordre, approbation requise.
4. **Dépôts** : selon la topologie (voir RG-PRJ-02). Pour chacun : contenu (Infrastructure,
   Code applicatif), fournisseur, URL, branche par défaut, jeton git.
5. **Récapitulatif** et création.

Environnement proposé par défaut par l'écran : « Development », nom court `dev`, région
`westeurope`, sans approbation.

**RG-PRJ-01 — Validation de l'assistant.** Nom 3–80 caractères ; description 1 000 max ;
topologie parmi les trois valeurs ; au moins un environnement ; pour chaque environnement :
nom (100 max), nom court (20 max), région valide, ordre ≥ 0.

**RG-PRJ-02 — Nombre de dépôts selon la topologie.** `MultiRepo` : aucun dépôt projet.
`AllInOne` : exactement un. `SplitInfraCode` : exactement deux (un Infrastructure, un Code).

**RG-PRJ-03 — Dépôt complet ou vide.** Pour chaque dépôt, fournisseur, URL et branche sont soit
tous renseignés, soit tous vides (un dépôt vide est un emplacement à compléter plus tard).
Fournisseurs acceptés : `GitHub`, `AzureDevOps`.

**RG-PRJ-04 — Jeton git.** Si un jeton est fourni pour un dépôt, il est enregistré dans
Azure Key Vault. **Il n'est pas testé avant enregistrement** (voir EC-PRJ-02).

**RG-PRJ-05 — Création atomique.** Si une étape échoue (région invalide, dépôt refusé par la
topologie…), le projet n'est pas créé. Exception : un jeton déjà écrit dans Key Vault avant
l'échec y reste (**non vérifié à l'exécution**).

### UC-PRJ-03 — Consulter ses projets

L'utilisateur voit la liste des projets dont il est membre. Pour chaque projet la liste expose son
nom, sa description et des compteurs (**détail des compteurs à confirmer dans le contrat**).

### UC-PRJ-04 — Consulter un projet

Vue d'ensemble : nom, description, configurations (nombre de resource groups et de ressources par
configuration), environnements, nommage, tags, variable groups, dépôts, membres.

### UC-PRJ-05 — Supprimer un projet

Réservé à l'Owner. Le projet est supprimé avec, en cascade en base, ses configurations, resource
groups et ressources (**cascade non vérifiée à l'exécution**). Les dépôts git et les ressources
Azure déjà déployées ne sont pas touchés.

## 4. Environnements

| Champ | Type / valeurs | Obligatoire | Règles |
|---|---|---|---|
| Nom | Texte | Oui | 100 caractères max. Sert de clé pour rattacher les réglages par environnement des ressources (voir RG-PRJ-09). |
| Nom court | Texte | Oui | 20 caractères max (assistant). Utilisé dans les noms (`{envShort}`), les noms de fichiers générés (`main.{envShort}.bicepparam`), les environnements Azure DevOps. |
| Préfixe | Texte | Oui (peut être vide) | 50 caractères max. Jeton `{prefix}`. |
| Suffixe | Texte | Oui (peut être vide) | 50 caractères max. Jeton `{suffix}`. |
| Région | Une des 31 régions Azure supportées (liste en annexe de [07](07-modele-commun-ressources.md)) | Oui | — |
| Abonnement | Identifiant GUID | Oui | Non vérifié auprès d'Azure. |
| Ordre | Entier ≥ 0 | Oui | Définit l'ordre de promotion dans le pipeline de release. |
| Approbation requise | Booléen | Oui | Reportée dans le bootstrap (voir [14](14-generation-pipelines-bootstrap.md)). |
| Service connection Azure | Texte | Non | Nom de la service connection Azure Resource Manager de cet environnement. Non saisissable dans l'assistant. |
| Tags | Liste (clé, valeur) | Non | Clé et valeur 1 à 100 caractères. Tags propres à l'environnement (voir section 6). |

**RG-PRJ-06 — Ordre continu.** Ajouter un environnement à l'ordre *n* décale d'un cran les
environnements d'ordre ≥ *n*. Supprimer un environnement d'ordre *n* rapproche ceux d'ordre > *n*.
Modifier l'ordre déplace l'environnement et réordonne les autres.

**RG-PRJ-07 — Environnements partagés.** Toutes les configurations du projet utilisent tous les
environnements du projet. Il n'existe pas d'environnement propre à une configuration.

**RG-PRJ-08 — Pas d'unicité.** Rien n'empêche deux environnements de même nom ou de même nom court
(**non vérifié à l'exécution**).

**RG-PRJ-09 — Rattachement par nom.** Les réglages par environnement des ressources, les valeurs
d'app settings et de clés App Configuration, et les domaines personnalisés sont rattachés à un
environnement **par son nom (texte)**, pas par son identifiant.

UC-PRJ-06 à UC-PRJ-08 : ajouter, modifier, supprimer un environnement (Contributor).

## 5. Membres

Voir [02 § 3.3](02-acces-et-droits.md).

## 6. Tags

**RG-PRJ-10 — Tags du projet.** Le projet porte une liste de tags (clé, valeur) remplacée en bloc
à chaque enregistrement.

**RG-PRJ-11 — Fusion dans le Bicep.** Le Bicep généré applique à chaque ressource la fusion
`tags du projet` ∪ `tags de la configuration` ∪ `tags de l'environnement` ; en cas de clé commune,
l'environnement l'emporte sur la configuration, qui l'emporte sur le projet.

**RG-PRJ-12 — Pas de tag système.** IFS n'ajoute aucun tag automatique (ni projet, ni
« géré par IFS »).

## 7. Variable groups

**RG-PRJ-13 — Déclaration.** Le projet déclare des noms de variable groups Azure DevOps
(200 caractères max). Deux groupes ne peuvent pas avoir le même nom (comparaison insensible à la
casse).

**RG-PRJ-14 — Jeton `{env}`.** Un nom peut contenir `{env}` : il désigne alors un groupe par
environnement (`{env}` remplacé par le nom court en minuscules). C'est le seul jeton accepté ; les
expressions Azure DevOps `${{ }}` sont refusées à la génération.

**RG-PRJ-15 — Usage.** Un variable group sert de source à des app settings, des clés App
Configuration et des paramètres sécurisés (voir [10](10-configuration-applicative.md)). Le
bootstrap le crée dans Azure DevOps avec les variables référencées (voir [14](14-generation-pipelines-bootstrap.md)).

UC-PRJ-09 à UC-PRJ-10 : ajouter, supprimer un variable group.

## 8. Agent pool

**RG-PRJ-16 — Agent pool.** Nom de l'agent pool Azure DevOps utilisé par les pipelines générés.
Vide : les pipelines utilisent le pool hébergé par défaut (`ubuntu-latest`) (**à confirmer pour
chaque pipeline**).

## 9. Dernière génération

**UC-PRJ-11 — Retrouver la dernière génération.** Le projet expose les chemins des fichiers de la
dernière génération (Bicep, pipelines, bootstrap) sans régénérer. Voir [15](15-livraison-git.md).

## 10. Paramètres du projet (écran)

L'écran « Paramètres du projet » contient l'agent pool et affiche comme **« Bientôt disponible »** :
génération automatique, notifications, politique de rétention, connexions de service. Ces
fonctions n'existent pas dans le code.

## 11. Écarts constatés

- **EC-PRJ-01 — Deux créations, deux règles.** Création simple : nom 1–80, description 500, topologie
  `MultiRepo` imposée. Assistant : nom 3–80, description 1 000, topologie choisie.
- **EC-PRJ-02 (F01) — Jeton non testé à la création.** L'assistant enregistre le jeton git sans
  tester la connexion, alors que l'ajout d'un dépôt après coup la teste. Un projet peut naître avec
  une URL ou un jeton invalides.
- **EC-PRJ-03 — Renommer un environnement casse des liens.** Les réglages par environnement, valeurs
  d'app settings, valeurs de clés et domaines sont rattachés par nom ; renommer l'environnement ne
  les met pas à jour. Ils deviennent orphelins et disparaissent de la génération.
- **EC-PRJ-04 — Supprimer un environnement laisse des données.** Les réglages et valeurs rattachés au
  nom de l'environnement supprimé restent en base.
- **EC-PRJ-05 — Jetons orphelins.** La suppression d'un projet ou d'un dépôt ne supprime pas le jeton
  git dans Key Vault (aucun appel de suppression de secret dans le code).
- **EC-PRJ-06 — Défauts de nommage dupliqués.** Les gabarits par défaut sont codés deux fois (création
  simple et assistant) ; une modification dans l'un ne suit pas dans l'autre.
- **EC-PRJ-07 — Région par défaut.** L'environnement par défaut de l'assistant est en `westeurope`
  alors que le reste du produit présente `francecentral` dans les exemples.

## 12. Questions ouvertes

- **Q-PRJ-01** — Garder deux modes de création ou seulement l'assistant ?
- **Q-PRJ-02** — Rendre le nom et le nom court d'environnement uniques par projet, et rattacher les
  réglages par identifiant ?
- **Q-PRJ-03** — Autoriser des environnements propres à une configuration (exemple : un socle sans
  staging) ?
- **Q-PRJ-04** — Ajouter des tags système (projet, configuration, géré par IFS) ?
- **Q-PRJ-05** — Les fonctions « Bientôt disponible » (génération automatique, notifications,
  rétention, connexions de service) sont-elles dans la cible v1 ?
- **Q-PRJ-06** — Stocker les approbateurs d'un environnement pour que le bootstrap crée la
  vérification d'approbation (aujourd'hui impossible, voir [14](14-generation-pipelines-bootstrap.md)) ?
