# 17 — Import d'un template ARM

> Statut : v0 rétro-spécification (code au 2026-10-02).
> Feature-map : F30 (aperçu, `keep`), F31 (application, `keep`). **Aucun écran** : seul le serveur MCP
> ([18](18-serveur-mcp.md)) et l'API directe y accèdent.

## 1. Objectif

Partir d'une infrastructure existante décrite en template ARM (JSON) pour créer un projet IFS, au
lieu de tout modéliser à la main.

## 2. Aperçu

**UC-IMP-01 — Prévisualiser un import.** Entrée : format (`arm-json`, seul supporté) + contenu du
template. Sortie :

| Élément | Contenu |
|---|---|
| Ressources | Pour chaque ressource ARM : type et nom source, type IFS et nom proposés, confiance (`High` si le type est reconnu, sinon `Low`), propriétés extraites, propriétés non reprises, stack applicative suggérée. |
| Dépendances | Liens `dependsOn` entre ressources. |
| Écarts | Gravité (`Info`, `Warning`, `Error`), catégorie (`unsupported_resource`, `unmapped_property`), message, ressource source. |
| Résumé | Texte de synthèse. |

**RG-IMP-01 — Types reconnus.** Environ 20 types ARM correspondent aux types du catalogue ; les autres
deviennent des écarts « ressource non supportée ».

**RG-IMP-02 — Stack suggérée.** Pour les applications en conteneur, la stack est devinée depuis le nom
de l'image (`dotnet`/`aspnet`, `node`, `python`, `java`/`openjdk`, `php`, `golang`).

## 3. Application

**UC-IMP-02 — Créer un projet depuis un aperçu.** Entrée : nom du projet (3 à 80 caractères),
topologie, aperçu, environnements (facultatif), filtre de ressources (facultatif).

Effets, dans l'ordre :
1. création d'un **nouveau** projet (même parcours que l'assistant, [03](03-projets.md)) ; sans
   environnement fourni : un environnement `Development` / `dev` ;
2. création d'une configuration `<projet>-config` et d'un resource group `<projet>-rg` ;
3. création des ressources retenues, dans l'ordre de leurs dépendances.

Résultat : projet, configuration, resource group, ressources créées, ressources ignorées (avec la
raison).

**RG-IMP-03 — Échec partiel.** Une ressource qui ne peut pas être créée est ignorée et listée ; les
autres sont créées.

## 4. Écarts constatés

- **EC-IMP-01 — Pas d'écran.**
- **EC-IMP-02 — Nouveau projet uniquement.** Impossible d'importer dans un projet ou une configuration
  existants (limite assumée pour l'instant).
- **EC-IMP-03 — Un seul resource group.** Toutes les ressources atterrissent dans un resource group
  unique, quelle que soit leur organisation d'origine.
- **EC-IMP-04 — ARM JSON seulement.** Ni Bicep, ni Terraform, ni lecture directe d'un abonnement Azure.
- **EC-IMP-05 — Réglages par environnement perdus.** Les propriétés extraites ne sont pas réparties par
  environnement ; rôles, app settings et identités du template ne sont pas repris
  (**non vérifié en détail**).
- **EC-IMP-06 — Abonnement vide.** L'environnement par défaut a un identifiant d'abonnement nul.

## 5. Questions ouvertes

- **Q-IMP-01** — Importer dans un projet existant ?
- **Q-IMP-02** — Importer depuis Bicep, Terraform, ou par lecture d'un abonnement / resource group
  Azure ?
- **Q-IMP-03** — Reprendre les rôles, identités et app settings ?
- **Q-IMP-04** — Offrir un écran d'import avec revue des écarts ?
