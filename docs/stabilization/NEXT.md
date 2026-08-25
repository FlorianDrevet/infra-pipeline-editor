# NEXT — où on en est

> Fichier de passage de relais entre sessions et entre postes. **Lu en premier** à chaque
> session, **réécrit en dernier**. Une session ne se termine pas sans l'avoir mis à jour.

**Dernière mise à jour :** 2026-08-25

## Contexte de la démarche

Projet vibe-codé pendant un mois, parti dans tous les sens. On stabilise l'existant avant
d'ajouter des features. Ambition confirmée : **produit destiné à la vente**.
Golden path confirmé : **modéliser → Bicep → pipelines → push git**.

Plan en 3 phases :
1. **Carte** — inventaire fonctionnel depuis le code → `feature-map.md` ✅ **terminée**
2. **Tri** — avec le porteur du projet, ligne par ligne : `keep` / `fix` / `cut` ← *prochaine étape*
3. **Réparation** — une feature par session, en partant du golden path

La phase 0 (build / tests / démarrage de la stack) a été explicitement écartée par le
porteur du projet. Conséquence assumée : aucun statut `🟢` ne peut être posé pour l'instant,
faute de preuve d'exécution.

## Où on s'est arrêté

**Les 7 tranches sont cartographiées.** `feature-map.md` couvre ~25 features et
**11 défauts structurels** (D01 → D11), tous vérifiés directement dans le code.

Les corrections de mémoire projet qui étaient en attente sont faites : `12-api-endpoints.md`
(routes networking inventées), `08-frontend.md` (points d'entrée PAT inexistants),
`07-bicep-generation.md` (D01 consigné), `03-domain-model.md` (`NetworkingProfile` marqué orphelin).

### Ce que la carte dit du produit

**Le layout `MultiRepo` n'est pas livrable.** Ce n'est pas « partiellement supporté », c'est
trois trous cumulés qui se recouvrent :

- **D01** — aucun chemin de stockage de PAT pour un dépôt de config → tout push échoue en Key Vault.
- **D09** — le bootstrap est inatteignable : le résolveur de dépôt échoue avant d'entrer dans la branche prévue.
- **D10** — `PushProjectArtifactsToMultiRepo` refuse en fait MultiRepo (il est SplitInfraCode-only, malgré son nom).

C'est le **premier arbitrage** de la session de tri : réparer MultiRepo, ou le retirer du produit.

**Trois défauts font mentir l'application à l'utilisateur** — à traiter en priorité quel que
soit l'arbitrage layout :

- **D05** — l'écran de vérification de nom n'utilise pas la même cascade que la génération. Le nom validé n'est pas le nom généré.
- **D08** — le bouton « Valider le DNS » d'un domaine personnalisé ne fait aucune résolution DNS. Il fait confiance à la déclaration, et ce statut déclaré conditionne le binding réellement généré.
- **D11** — privatiser une ressource sans configurer de Private Endpoint la génère sans accès public **et** sans point d'entrée privé.

**Deux features saisissent des données qui ne partent nulle part** : D07 (clés App Configuration)
et D06 (overrides par environnement de DocumentIntelligence).

**Le motif transverse est confirmé** : les défauts ne sont pas des features cassées, ce sont
des features **jamais raccordées**, plus deux motifs supplémentaires — deux implémentations
concurrentes du même concept, et des noms qui promettent ce que le code ne fait pas
(`ValidateDns` ne valide pas, `GenerateCombined` ne combine pas). Corollaire opérationnel :
**la documentation interne du code est activement trompeuse**, XML docs compris. Ne pas s'y fier.

## À faire à la prochaine session

**Session de tri, avec le porteur du projet.** La carte est finie, elle ne sert à rien tant
que la colonne `Décision` est vide. Ordre suggéré :

1. **Trancher `MultiRepo`** : réparer (D01 + D09 + D10) ou retirer. Tout le reste en dépend —
   c'est un axe transverse, pas une feature.
2. **Trancher les 3 mensonges à l'utilisateur** (D05, D08, D11) — a priori `fix`, mais le
   coût de D05 mérite discussion : deux cascades à réunifier.
3. **Trancher le code mort** : D04 (`NetworkingProfile` V2), F35, F20 (modale PAT orpheline),
   `NetworkSecurityGroup` / `PrivateDnsZone` / `FrontDoor`. A priori `cut`, décision rapide.
4. **Parcourir le reste de la carte ligne par ligne** pour poser `keep` / `fix` / `cut`.

Ensuite seulement : phase 3, une feature par session, en partant du golden path.

## Ce qu'il reste à mettre en place

- [ ] Poser la colonne `Décision` sur toutes les lignes de `feature-map.md` (session de tri).
- [ ] Après le tri : rouvrir la question de la phase 0 (build / tests / démarrage de la stack).
      Sans elle, aucune feature ne pourra jamais passer `🟢` — la réparation se fera à l'aveugle.

## Note de méthode — vérifier les rendus d'agents

Sur cette session, un constat d'agent s'est révélé **faux à la vérification** : « le nettoyage
des fichiers obsolètes ne se déclenche jamais au push ». `BasePath` est bien codé à `null`
(`RepositoryTargetResolver.cs:51-73`), mais `MultiScopeGitPushRequestBuilder.SplitRootScopedPath:120`
re-découpe par dossier de premier niveau et rétablit les cleanup roots. Seuls les fichiers
générés à la racine du dépôt échappent au nettoyage.

Le rendu d'un `feature-mapper` est une **piste**, pas un fait. Tout ce qui monte en défaut
structurel dans `feature-map.md` est relu dans le code avant d'être écrit.
