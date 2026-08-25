# NEXT — où on en est

> Fichier de passage de relais entre sessions et entre postes. **Lu en premier** à chaque
> session, **réécrit en dernier**. Une session ne se termine pas sans l'avoir mis à jour.

**Dernière mise à jour :** 2026-08-25

## Contexte de la démarche

Projet vibe-codé pendant un mois, parti dans tous les sens. On stabilise l'existant avant
d'ajouter des features. Ambition confirmée : **produit destiné à la vente**.
Golden path confirmé : **modéliser → Bicep → pipelines → push git**.

Plan en 3 phases :
1. **Carte** — inventaire fonctionnel depuis le code → `feature-map.md` *(en cours)*
2. **Tri** — avec le porteur du projet, ligne par ligne : `keep` / `fix` / `cut`
3. **Réparation** — une feature par session, en partant du golden path

La phase 0 (build / tests / démarrage de la stack) a été explicitement écartée par le
porteur du projet. Conséquence assumée : aucun statut `🟢` ne peut être posé pour l'instant,
faute de preuve d'exécution.

## Où on s'est arrêté

La cartographie se fait par fan-out de 7 sous-agents en lecture seule, un par tranche.
**2 tranches sur 7 sont rendues** — les 5 autres se sont arrêtées sur une limite de session,
sans résultat exploitable.

| Tranche | État |
|---|---|
| 1. Projet & topologie dépôts/git | rendue |
| 2. InfraConfig, ResourceGroup & naming | à relancer |
| 3. Catalogue des types de ressources Azure | à relancer |
| 4. Câblage entre ressources | à relancer |
| 5. Génération Bicep | à relancer |
| 6. Pipelines, bootstrap & push git × layouts | à relancer — **prioritaire** |
| 7. Périphérie : import ARM, MCP/PAT, networking | rendue |

Résultats consignés dans `feature-map.md` : 15 features cartographiées, **4 défauts
structurels confirmés** (D01 à D04), dont un bloquant produit — D01 : le layout MultiRepo
ne peut pas pousser, faute de tout chemin de stockage de PAT.

## À faire à la prochaine session

1. **Relancer les tranches 6, 5, 3, 2, 4** (dans cet ordre de valeur) avec l'agent
   `feature-mapper`, désormais présent dans le registre. Périmètres ci-dessous.
2. Compléter `feature-map.md` au fur et à mesure — ne pas attendre que tout soit rendu.
3. Quand la carte est complète : **session de tri** avec le porteur du projet.

## Ce qu'il reste à mettre en place

- [ ] Ajouter la règle de rituel dans `CLAUDE.md` §0 : lire ce fichier en début de session,
      le réécrire en fin de session, commiter avec le travail.
- [ ] Corriger `.claude/memory/12-api-endpoints.md` et `08-frontend.md` (voir la section
      « Mémoire projet à corriger » de `feature-map.md`).

## Consignes de tranche (à réutiliser telles quelles)

Invoquer l'agent `feature-mapper` avec, pour chaque tranche, le périmètre d'endpoints et
les points d'attention. Règles communes déjà portées par l'agent : le code fait foi, la
mémoire n'est pas une source, jamais de statut vert/rouge sans exécution, toute suspicion
porte un chemin de fichier.

- **Tranche 2** — `/infra-config`, `/resource-group`, naming aux deux niveaux,
  `check-availability`, références cross-config. Vérifier la cascade
  `Config → Projet → catalogue` et identifier le service qui résout le nom final.
- **Tranche 3** — les 22 groupes CRUD par type + sous-ressources. Regrouper en features
  transverses, pas une ligne par type. Trancher : `NetworkSecurityGroup` /
  `PrivateDnsZone` / `FrontDoor` existent-ils encore ? `DocumentIntelligence` a-t-il un
  contrôleur, un générateur, un écran ?
- **Tranche 4** — role assignments, identités, app settings, outputs, configuration keys,
  secure parameter mappings, custom domains, dépendances. Vérifier le passage
  Pending → Validated des custom domains et repérer les endpoints sans consommateur front.
- **Tranche 5** — génération Bicep aux deux niveaux. Trancher la relation entre
  `Generate(...)` et `GenerateMonoRepo(...)`, qui les appelle, si les endpoints de niveau
  config sont encore atteignables depuis le front. Vérifier l'exhaustivité de
  `ResourceTypeMetadata.GetBaseModuleName(...)` : tout type ARM manquant y perd
  silencieusement ses overrides par environnement.
- **Tranche 6 (prioritaire)** — pipelines, bootstrap, push git. Pour **chaque** feature,
  dire quelles variantes de `LayoutPreset` sont réellement gérées. Vérifier `BootstrapMode`,
  `AppPipelineMode`, le routage `IRepositoryTargetResolver` / `AppPipelineFileClassifier`,
  le nettoyage des fichiers obsolètes au push, et l'application de `AgentPoolName` à tous
  les pipelines générés.
