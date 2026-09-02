# NEXT — où on en est

> Fichier de passage de relais entre sessions et entre postes. **Lu en premier** à chaque
> session, **réécrit en dernier**. Une session ne se termine pas sans l'avoir mis à jour.

**Dernière mise à jour :** 2026-09-02

## Outillage de cartographie

Le dépôt utilise désormais Graphify comme graphe unique pour le code, la documentation, les audits et les diagrammes. Le graphe versionné se trouve dans `graphify-out/graph.json`, son rapport dans `graphify-out/GRAPH_REPORT.md`, et se rafraîchit avec `python -m graphify update .`.

## Contexte de la démarche

Projet vibe-codé pendant un mois, parti dans tous les sens. On stabilise l'existant avant
d'ajouter des features. Ambition confirmée : **produit destiné à la vente**.
Golden path confirmé : **modéliser → Bicep → pipelines → push git**.

Plan en 3 phases :
1. **Carte** — inventaire fonctionnel depuis le code → `feature-map.md` ✅ **terminée**
2. **Tri** — avec le porteur du projet, ligne par ligne : `keep` / `fix` / `cut` ✅ **terminée**
3. **Réparation** — une feature par session, en partant du golden path ← *en cours*

La phase 0 (build / tests / démarrage de la stack), initialement écartée, a été **rouverte et
validée** le 2026-08-28 avant de lancer la réparation. Voir « État de la phase 0 » ci-dessous.

## Phase 2 (Tri) — terminée, toutes les décisions dans `feature-map.md`

Arbitrages posés avec le porteur du projet le 2026-08-28 :

1. **`MultiRepo` → `fix`, priorité produit forte.** Réparer D01 (PAT) + D09 (bootstrap) + D10
   (push projet) plutôt que retirer le layout.
2. **D05, D08, D11 → `fix`, confirmé.** Les trois mensonges à l'utilisateur (nom généré, DNS,
   privatisation).
3. **Code mort vs. roadmap future, tranché ressource par ressource :**
   - `cut` : `NetworkingProfile` V2 (D04/F35), dialogue PAT autonome (F20), `PrivateDnsZone`
     — tous les trois couverts ailleurs (V3 privatisation, modale de dépôt, `PrivateEndpointDnsMode`).
   - **conservé, roadmap future** : `NetworkSecurityGroup`, `FrontDoor` — démolis
     volontairement en V1 (2026-05-28) mais le porteur veut les réintroduire ; ne pas purger
     `Subnet.NsgId` ni y toucher avant reprise de cette feature.
4. **Reste de la carte** : `keep` pour F02/F03/F04/F07/F22/F30/F31, `fix` pour F01 (assistant
   de création — pas de vérification git avant stockage PAT), F11b (génération Bicep niveau
   config, lié à MultiRepo), F15 (références cross-config, silencieusement incomplètes).

Détail complet et justifications ligne par ligne : `feature-map.md`, section « Décisions de tri ».

## État de la phase 0 (validée le 2026-08-28)

- **Build backend** : `dotnet build .\InfraFlowSculptor.slnx` → **0 erreur**, 168 warnings
  pré-existants (nullable refs, xUnit1013). Sain.
- **Tests backend** : `dotnet test .\InfraFlowSculptor.slnx` → 3827 tests, **51 échecs**.
  Aucun n'est lié aux zones qu'on va toucher en phase 3 (MultiRepo, naming, DNS, privatisation).
  Détail dans `.claude/test-debt.md` (entrée du 2026-08-28) :
  - 44 dans `Application.Tests` = dette déjà trackée le 2026-06-03, confirmée non résorbée,
    root cause identifiée (tests `Get<X>QueryHandlerTests` désynchronisés de la forme actuelle
    du handler, pas un bug de prod).
  - 6 déjà connus/documentés ailleurs (`SecurityMiddlewareIntegrationTests` ×4,
    `UpdateProjectEnvironmentRequestTests` ×1) + 2 nouveaux petits gaps identifiés
    (`ContractsResponseShapeSnapshotTests` drift de snapshot, `Subnet.AddressPrefix` sans
    `HasMaxLength`).
- **Frontend** : `node_modules/typescript` était tronqué (`npm run typecheck` cassé depuis le
  2026-06-02) — corrigé par réinstallation ciblée. `npm run typecheck` maintenant propre.
- **Stack Aspire** : démarrage propre confirmé (Postgres, Azurite, émulateur Key Vault, DbGate
  tous `Up`, dashboard répond, 0 erreur dans les logs). Stack arrêtée après vérification.

**Conséquence pour la phase 3** : on peut maintenant valider une réparation par exécution
(tests + démarrage stack), pas seulement par lecture de code. Les 51 échecs actuels sont un
bruit de fond connu et documenté, à ne pas confondre avec une régression introduite pendant
la réparation — toujours comparer le nombre d'échecs avant/après une session de repair.

## État de la phase 3 — session 1 : MultiRepo

Premier chantier de réparation, le plus gros et le plus prioritaire (D01 + D09 + D10 + F05 +
F06 + F11b + F13 + F14, tous `fix`, tous dépendants les uns des autres). L'implémentation
backend, API et frontend de D10 est maintenant en place. Il reste la validation contre un vrai
fournisseur Git avant de déclarer le flux livrable. Voir le détail de chaque défaut dans
`feature-map.md`, section « Défauts structurels confirmés ».

Ordre suggéré :
1. **D01 — donner un PAT aux dépôts de config `MultiRepo`. ✅ Fait, back et front [2026-08-28, dotnet-dev + angular-front].**
   `AddInfraConfigRepositoryRequest`/`UpdateInfraConfigRepositoryRequest` portent `PersonalAccessToken`,
   `ProjectGitSecretNames.GetInfraConfigRepositoryPatSecretName(InfraConfigRepositoryId)` nomme le
   secret, `RepositoryTargetResolver.BuildFromConfigRepository` ne pose plus `PatSecretName: null`,
   et `infra-config-repository-dialog` affiche + transmet le champ PAT (toujours à la création,
   seulement si renseigné à l'édition). Build solution + build front : 0 erreur. Aucune régression
   sur la baseline de tests connue. Voir `feature-map.md` section D01. **Reste à faire** : la
   revalidation en conditions réelles (étape 4 ci-dessous) une fois D09/D10 traités — le fix est
   vérifié par tests unitaires + relecture, pas encore par un vrai push MultiRepo bout en bout.
2. **D09 — rendre le bootstrap atteignable en MultiRepo. ✅ Fait, back et front
   [2026-08-31, architect (plan) + dotnet-dev + angular-front].** Asymétrie racine identifiée
   par l'agent `architect` : contrairement à Bicep (F11b) et Pipeline (F12), le bootstrap n'avait
   qu'un niveau (projet), qui échoue toujours en MultiRepo faute de dépôt projet. Ajout d'un
  niveau config symétrique : nouveau slice `InfrastructureConfig/Commands/GenerateBootstrap` +
  `PushBootstrapToGit` (+ Download/GetFileContent), réutilisant `IProjectBootstrapDefinitionBuilder`
  tel quel avec une liste à un seul élément. `AllInOne` reste en `FullOwner`; `SplitInfraCode`
  sépare le bootstrap `FullOwner` infra du bootstrap `ApplicationOnly` applicatif. Le bootstrap
  projet est maintenant gardé explicitement contre MultiRepo (`Project.CanGenerateAllFromProjectLevel()`,
  même pattern que Bicep/Pipeline) au lieu d'échouer silencieusement. Front : 3ᵉ onglet
   "Bootstrap" dans l'écran de génération config (visible seulement en MultiRepo, comme F11b),
   bouton Push to Git dédié posant `isBootstrap: true`. Build solution + build front : 0 erreur.
  Le push config-level direct sélectionne uniquement le bucket `infra/` en `SplitInfraCode`,
  refuse une cible sans secret au lieu d'utiliser un secret projet et propage l'annulation par
  Key Vault, Blob et les providers Git. Build solution + build front : 0 erreur. Détail complet et plan :
   `docs/features/multirepo-bootstrap-d09-implementation-tracker.md`. **Reste à faire** : la
  revalidation en conditions réelles (étape 4 ci-dessous), différée jusqu'à ce que D10 soit
  aussi traité (même push combiné à valider ensemble). **Défaut adjacent découvert (D12,
   non corrigé, à trier)** : le bouton "Push to Git" unique de l'en-tête `config-detail` ne pousse
   jamais le Pipeline (`isPipeline` jamais activé), uniquement le Bicep, quel que soit l'onglet
   actif — voir tracker D09 section "Défaut adjacent découvert".
3. **D10 — push projet MultiRepo. ✅ Implémenté, backend/API/frontend [2026-09-01].**
  Nouveau contrat bulk par `InfrastructureConfig` et `InfraConfigRepository`, routage par
  `ConfigLayoutMode`/`ContentKinds`, prévalidation avant le premier push, résultats indépendants
  par dépôt et dialogue frontend dédié. L'ancien flux `SplitInfraCode` est exposé sous une route
  explicite pour ne plus confondre les deux topologies. Les plans sont construits avant le premier
  push et une annulation arrête la séquence sans tenter la cible suivante. Détail dans
  `docs/features/multirepo-push-d10-implementation-tracker.md`.
4. **Prochaine action : valider D01 + D09 + D10 par exécution réelle.** Utiliser un vrai projet
  MultiRepo avec au moins deux configurations `AllInOne`, puis une configuration `SplitInfraCode`;
  vérifier la génération Bicep/pipeline/bootstrap, le PAT config-level, un commit par dépôt et un
  échec partiel. Le build et les tests automatisés sont passés; cette preuve réelle manque encore.
  Utiliser le dossier [docs/manual-tests/](../manual-tests/README.md) pour enregistrer la campagne
  et ses preuves.

Ensuite : D05/D08/D11 (les 3 mensonges), puis F01/F11b(reste)/F15, puis nettoyage du code mort
confirmé `cut` (D04/F35, F20, `PrivateDnsZone`).

## Ce qu'il reste à mettre en place

- [x] Poser la colonne `Décision` sur toutes les lignes de `feature-map.md` (session de tri).
- [x] Rouvrir et valider la phase 0 (build / tests / démarrage de la stack).
- [x] Implémenter et durcir la réparation MultiRepo (D01 + D09 + D10 + features dépendantes) —
  phase 3, session 1. Validation ciblée et build solution passés le 2026-09-01.
- [x] Produire le dossier complet de tests manuels — index, préparation, smoke test, recette
  détaillée, scénarios négatifs, MCP/import et modèle de compte-rendu — le 2026-09-02.
- [ ] Valider le flux MultiRepo en conditions réelles contre un fournisseur Git — dernière preuve
  avant de fermer D01/D09/D10.
- [ ] Réparer D05 / D08 / D11 (les 3 mensonges à l'utilisateur) — phase 3, session 2.
- [ ] Résorber la dette de tests `Application.Tests` (44 tests, trackée depuis 2026-06-03,
      root cause connue) — à planifier, pas bloquant pour la réparation fonctionnelle mais
      fausse le signal "0 failure" qu'on veut pouvoir utiliser comme preuve de non-régression.

## Note de méthode — vérifier les rendus d'agents

Sur une session précédente, un constat d'agent s'est révélé **faux à la vérification** : « le
nettoyage des fichiers obsolètes ne se déclenche jamais au push ». `BasePath` est bien codé à
`null` (`RepositoryTargetResolver.cs:51-73`), mais `MultiScopeGitPushRequestBuilder.SplitRootScopedPath:120`
re-découpe par dossier de premier niveau et rétablit les cleanup roots. Seuls les fichiers
générés à la racine du dépôt échappent au nettoyage.

Le rendu d'un `feature-mapper` (ou de toute lecture de code) est une **piste**, pas un fait.
Tout ce qui monte en défaut structurel dans `feature-map.md` est relu dans le code — et,
maintenant que la phase 0 est validée, revérifié par exécution quand c'est possible — avant
d'être écrit.
