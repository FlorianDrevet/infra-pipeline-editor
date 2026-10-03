# Refonte IFS — suivi (design, specs, reconstruction)

> Document de reprise entre sessions et entre postes pour la démarche de refonte commencée le
> 2026-10-02. À lire après `docs/stabilization/NEXT.md`. Mis à jour le 2026-10-03.

## 1. Où on en est en une phrase

La maquette cible et son design system existent dans Claude Design, les specs fonctionnelles v0
de l'existant sont écrites. **Prochaine étape : le porteur relit les specs et les passe en « v1
cible ».**

## 2. Chronologie des décisions

| Date | Étape | Résultat |
|---|---|---|
| 2026-10-02 | Refonte du design | Maquette complète dans Claude Design, à partir de la mémoire, du plan de refonte et du code. On a décrit le produit **tel qu'il doit être**, pas tel qu'il est codé. |
| 2026-10-02 | Choix de design (questions au porteur) | Design system **inventé**, aligné avec le produit, sans repartir de l'existant. Thème **sombre**. Les 4 écrans clés, puis toute la maquette. **Vue graphe** des ressources **en plus** de la vue liste. |
| 2026-10-02 | Design system | Projet Design System « **Strata** » dans Claude Design, source de vérité pour la maquette et pour le code, avec les **icônes Azure officielles** de Microsoft. Installé dans la maquette. |
| 2026-10-02 | Repartir de zéro ? | Recommandation : **nouvelle base, mais pas de zéro absolu** (section 4). |
| 2026-10-02 → 03 | Specs fonctionnelles | Rétro-spécification v0 de tout l'existant dans `docs/specs/` (section 5). Branche `docs/functional-specs`, poussée (commits `38cb5e28` et suivants). |

## 3. Design

| Élément | Lien | Contenu |
|---|---|---|
| Design system Strata | https://claude.ai/artifact/4M9UorSE1agmiyxSa3vuqJ | Tokens (`project/tokens.json`), brand book en français (`project/README.md`), 13 composants React (`window.Strata`), dont `ResourceIcon` avec les icônes Azure V24. |
| Maquette | https://claude.ai/artifact/EVxbfuoq2WMs8Dy9YHZSgP | 49 écrans sur 8 pages (Design system ; Espace de travail et création ; Projet ; Configuration ; Ressource ; Génération et livraison ; Compte et accès ; Import ARM), en prototype cliquable. Strata installé sous `project/ds/strata/`. |

Règles :
- Toute évolution visuelle part de Strata : mettre à jour Strata, puis réinstaller sa nouvelle version
  dans la maquette.
- Le front Angular n'importe pas Strata : il reprend ses tokens (préfixe `--ifs-*`) et la
  correspondance composants Strata ↔ `app-ds-*` décrite dans le README de Strata.
- Les scripts qui ont généré la maquette vivaient dans un dossier temporaire de session : ils ne sont
  **pas** dans le dépôt. La source de vérité est l'artefact Claude Design.

**Question ouverte :** la licence Microsoft des icônes Azure couvre les diagrammes, la formation et
la documentation, pas explicitement l'interface d'un produit vendu. À valider avant la
commercialisation. Solution de repli : `ResourceIcon variant="tile"`.

## 4. Recommandation sur la reconstruction

**Avis : repartir sur une base neuve, sans partir de zéro absolu.** On garde le moteur de génération et
on réécrit tout le reste à partir des specs.

| Partie | Taille (2026-10-02) | Décision |
|---|---|---|
| Moteur de génération (`BicepGeneration`, `PipelineGeneration`, `GenerationCore`) | ~19 400 lignes, ~1 180 tests | **Porté tel quel avec ses tests**, puis corrigé (D06, deux énumérations `AppPipelineMode`, `GenerateCombined`). Il ne dépend que d'`ErrorOr`, pas du domaine, d'EF ni de l'API. |
| Domaine, Application, Infrastructure, API, Contracts | ~74 000 lignes, ~2 600 tests | **Réécrits** depuis le template .NET du porteur, à partir des specs, avec un seul modèle de ressource cohérent. |
| Front Angular | ~81 000 lignes, 142 specs | **Réécrit** depuis le template Angular du porteur, avec Strata et la maquette. |
| Utilitaires (contraintes de nommage Azure, fournisseurs git, Key Vault) | — | Repris au cas par cas, relus avant copie. |
| Import ARM, serveur MCP | — | Hors v1, réintégrés ensuite. |
| Dépôt actuel | — | Gardé en lecture comme référence jusqu'à la bascule. |

Pourquoi :
1. Le front se réécrit de toute façon (nouvelle maquette, composants géants).
2. Pas encore de clients : aucune donnée à migrer, aucune compatibilité d'API à tenir.
3. Les défauts viennent de la structure (maillons manquants entre couches, deux versions d'un même
   concept) : une base où chaque concept a une seule implémentation, définie par une spec, supprime
   cette classe de bugs.
4. Le moteur contient le savoir le plus coûteux ; ses tests encodent des cas limites déjà payés.

Plan proposé :
1. Specs fonctionnelles d'abord : v0 faite, v1 cible à écrire avec le porteur.
2. Squelette depuis les deux templates : Aspire, authentification Entra, CI, tokens Strata.
3. Filet de sécurité : générer avec l'application actuelle le Bicep et les pipelines du projet de
   référence (snapshot `fb8699ea`), les garder comme fichiers attendus. La nouvelle base doit produire
   la même chose, ou une différence voulue et documentée.
4. Tranches verticales sur le golden path : projet → configuration → 3 types de ressource → Bicep →
   téléchargement ; puis pipelines ; puis push AllInOne ; puis élargissement (catalogue complet,
   SplitInfraCode, MultiRepo, réseau, domaines).
5. Bascule quand la nouvelle base couvre le golden path ; archivage de l'ancien dépôt.

Risques et garde-fous :
- On perd le travail de stabilisation MultiRepo (D01, D09, D10) côté API et front ; la partie moteur
  reste.
- Principal danger : vouloir « refaire mieux » sans limite. Garde-fou : une v1 strictement bornée par
  les specs et la maquette.
- L'avis changerait s'il existait déjà des utilisateurs ou des données à préserver, ou si les templates
  imposaient une architecture incompatible avec le moteur (il attend un `GenerationRequest` en entrée).

**Statut : recommandation, pas encore validée par le porteur.**

## 5. Specs fonctionnelles

- Dossier : `docs/specs/`, index `README.md`.
- 19 documents de domaine (01–19), `90-ecarts.md` (119 écarts, synthèse en 5 familles, lien vers
  la feature-map), `A1-routes-api.md` (229 routes générées depuis les contrôleurs).
- Format : règles `RG-`, cas d'usage `UC-`, écarts `EC-`, questions `Q-`, par code de domaine.
- Statut : **v0 rétro**, décrit le code au commit `e2346f43`. Le document 13 (génération Bicep) est
  plus léger que les autres : la liste exacte des fichiers par dossier reste à détailler.

Constats nouveaux (pas encore dans `docs/stabilization/feature-map.md`) : bootstrap sans approbations
ni création de service connections, variables secrètes créées vides, mode DNS des Private Endpoints
ignoré, plusieurs sous-ressources dans un seul Private Endpoint (Storage, Cosmos DB), pas de pull
request après push, paramètres de projet « à venir » factices, lectures sans contrôle d'appartenance
au projet. `CLAUDE.md` cite `src/BicepGenerators`, qui n'existe plus.

## 6. Ce qui attend le porteur

- [ ] Relire `docs/specs/` domaine par domaine, corriger ce qui ne correspond pas à l'intention.
- [ ] Trancher les questions `Q-*`, transformer les `EC-*` en règles cibles, ajouter les idées long
      terme, passer chaque document en « v1 cible ».
- [ ] Valider ou amender la recommandation de reconstruction (section 4).
- [ ] Donner l'emplacement des **templates bootstrap Angular et .NET**.
- [ ] Faire valider la licence des icônes Azure avant la commercialisation.
- [ ] Décider du sort de la branche `docs/functional-specs` (PR vers `main` ou non).

## 7. Prochaines actions possibles pour l'agent

- Compléter le document 13 (arborescence exacte des fichiers générés).
- Reporter les écarts nouveaux dans la feature-map (tri avec le porteur).
- Corriger `CLAUDE.md` (mention de `src/BicepGenerators`).
- Quand les templates sont fournis : vérifier leur compatibilité avec le moteur de génération, puis
  produire le plan de lots de la reconstruction.
