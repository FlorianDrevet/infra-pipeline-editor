---
name: tdd-workflow
description: "Use when: any code modification, feature implementation, bug fix, refactoring. Enforces TDD Red-Green-Refactor cycle. Mandatory for all coding agents."
---

# Skill : tdd-workflow — Test-Driven Development Workflow

> **BLOCKING SKILL — Doit être chargé par tout agent qui modifie du code dans ce dépôt.**
> Ce skill impose le cycle TDD Red → Green → Refactor avant toute modification de code.
> Il complète `xunit-unit-testing` (qui définit COMMENT écrire les tests) en imposant QUAND les écrire.

---

## 1. Principe fondamental

**Aucune ligne de code de production ne doit être écrite ou modifiée AVANT que les tests associés n'existent.**

Le cycle obligatoire est :

```
1. RED    — Écrire les tests qui décrivent le comportement attendu. Ils doivent échouer (ou ne pas compiler).
2. GREEN  — Implémenter/modifier le code de production pour que tous les tests passent.
3. REFACTOR — Nettoyer le code en gardant les tests verts.
4. VERIFY — `dotnet test` sur le projet de tests puis sur la solution complète.
```

---

## 2. Protocole TDD obligatoire — Étapes détaillées

### Étape 0 — Identifier la zone de test

Avant de modifier du code :
1. Identifier l'assembly cible (ex: `InfraFlowSculptor.Domain`, `InfraFlowSculptor.Application`, etc.)
2. Vérifier si le projet `tests/<Assembly>.Tests/` existe.
3. **Si le projet n'existe pas** → le créer en suivant le skill `xunit-unit-testing` section 2 (template de projet).
4. Vérifier si une classe de test `<SutName>Tests.cs` existe pour le SUT (System Under Test).
5. **Si la classe de test n'existe pas** → la créer avec le constructeur et le setup du SUT.

### Étape 1 — RED : Écrire les tests en premier

1. Identifier les comportements à tester pour le changement prévu.
2. Écrire les tests unitaires qui capturent ces comportements :
   - Pour un **nouveau feature** : tests des cas nominaux + cas d'erreur.
   - Pour un **bug fix** : écrire le test qui reproduit le bug (test rouge).
   - Pour un **refactoring** : vérifier que les tests existants couvrent le comportement, en ajouter si lacunaire.
3. Les tests doivent suivre le naming `Given_{Preconditions}_When_{StateUnderTest}_Then_{ExpectedBehavior}`.
4. Les tests DOIVENT compiler mais PEUVENT échouer (c'est normal — c'est le RED).

> **Exception compilabilité** : Si le SUT n'existe pas encore (nouveau handler, nouveau service),
> le test peut ne pas compiler. C'est acceptable. Créer d'abord la structure minimale du SUT
> (classe vide, méthode vide avec `throw new NotImplementedException()`) pour que les tests compilent,
> puis passer à l'étape GREEN.

### Étape 2 — GREEN : Implémenter le code de production

1. Écrire le code de production minimal pour faire passer les tests.
2. Ne pas anticiper des besoins futurs — écrire uniquement ce que les tests demandent.
3. Exécuter `dotnet test .\tests\<Assembly>.Tests\<Assembly>.Tests.csproj` — tous les tests doivent passer.

### Étape 3 — REFACTOR : Nettoyer

1. Nettoyer le code de production et les tests (duplication, nommage, structure).
2. Ré-exécuter les tests — ils doivent toujours passer.

### Étape 4 — VERIFY : Validation complète

1. Exécuter `dotnet test .\InfraFlowSculptor.slnx` — aucune régression sur l'ensemble de la solution.
2. Exécuter `dotnet build .\InfraFlowSculptor.slnx` — zero warnings si possible.

---

## 3. Quand le TDD s'applique

| Situation | TDD obligatoire ? | Détail |
|-----------|-------------------|--------|
| Nouveau handler / service / validator | ✅ OUI | RED: tests du handler → GREEN: implémenter → VERIFY |
| Bug fix sur du code existant | ✅ OUI | RED: test qui reproduit le bug → GREEN: corriger → VERIFY |
| Refactoring de code existant | ✅ OUI | Vérifier couverture existante, ajouter si lacunaire, puis refactorer |
| Nouveau domain aggregate / entity | ✅ OUI | RED: tests des invariants → GREEN: implémenter → VERIFY |
| Modification d'un Bicep generator | ✅ OUI | RED: tests du spec produit → GREEN: modifier → VERIFY |
| Ajout d'un endpoint API | ⚠️ PARTIEL | Tests du handler et validator obligatoires, endpoint wiring en intégration |
| Migration EF Core | ❌ NON | Pas de test unitaire sur les migrations |
| Configuration DI / startup | ❌ NON | Pas de test unitaire sur le câblage DI |
| Fichier `.md`, `.json`, config | ❌ NON | Pas de code exécutable |
| Code Angular frontend | ⚠️ QUAND APPLICABLE | Si des tests Jasmine/Jest existent pour la zone, les maintenir |

---

## 4. Détection et enregistrement de la dette de tests

Quand un agent touche une zone de code et constate qu'il n'y a **aucun test existant** pour cette zone :

1. **Écrire les tests pour le changement en cours** (TDD obligatoire).
2. **Enregistrer la dette** pour le reste de la zone non couverte dans `.github/test-debt.md` :
   - Ajouter une ligne avec l'assembly, la classe/zone, la description du manque, la priorité, et la date.
   - Ne pas essayer de couvrir toute la zone d'un coup — juste signaler.
3. **Ne jamais ignorer** la dette détectée — l'enregistrement est obligatoire.

### Priorités de dette

| Priorité | Critère |
|----------|---------|
| **P1 — Critique** | Code métier non testé : aggregates, handlers, validators, domain services |
| **P2 — Important** | Code infrastructure : repositories, mappers, transformers, clients |
| **P3 — Souhaitable** | Code utilitaire : extensions, options, helpers, configuration |

---

## 5. Initialisation d'un nouveau projet de tests

Si le projet de tests n'existe pas, le créer **avant** d'écrire les tests :

1. Créer le dossier `tests/<Assembly>.Tests/`
2. Créer le `.csproj` selon le template du skill `xunit-unit-testing` section 2
3. Ajouter le projet à `InfraFlowSculptor.slnx` via `dotnet sln .\InfraFlowSculptor.slnx add .\tests\<Assembly>.Tests\<Assembly>.Tests.csproj`
4. Vérifier le build : `dotnet build .\tests\<Assembly>.Tests\<Assembly>.Tests.csproj`
5. Puis commencer le cycle RED

---

## 6. Intégration avec les autres skills et agents

| Skill / Agent | Interaction |
|---------------|-------------|
| `xunit-unit-testing` | Définit le HOW (conventions, toolbox, AAA, nommage). Toujours charger en complément. |
| `dotnet-patterns` | Définit les conventions du code de production. Les tests ne suivent pas les XML docs mais suivent le nommage. |
| `cqrs-feature` | Pour une feature CQRS complète, les tests de chaque couche sont écrits AVANT l'implémentation de cette couche. |
| `dotnet-dev` | Agent exécutant : charge ce skill + `xunit-unit-testing` avant toute modification de code. |
| `dev` | Orchestrateur : s'assure que ce skill est mentionné dans les prompts de délégation. |

---

## 7. Checklist TDD — À vérifier avant de clore toute tâche de code

```
[ ] Projet de tests existe pour l'assembly modifié
[ ] Tests écrits AVANT le code de production (cycle RED → GREEN)
[ ] Tous les tests du projet passent : dotnet test .\tests\<Assembly>.Tests\
[ ] Aucune régression solution : dotnet test .\InfraFlowSculptor.slnx
[ ] Dette de tests enregistrée dans .github/test-debt.md si zone non couverte détectée
[ ] Pas de test dans InfraFlowSculptor.GenerationParity.Tests
```

---

## 8. Exceptions au TDD strict

Le cycle RED peut être assoupli dans ces cas précis (mais les tests restent obligatoires) :

1. **Scaffolding initial massif** (ex: `cqrs-feature` génère 15+ fichiers) : écrire les tests par couche après la génération de cette couche, mais AVANT de passer à la couche suivante.
2. **Correction d'urgence en production** : le fix peut être écrit en premier, mais le test de non-régression DOIT suivre immédiatement dans la même tâche.
3. **Code purement déclaratif** (Mapster configs, DI registrations, EF configurations) : pas de test unitaire requis, mais un test d'intégration futur est souhaitable (noter en dette P3).
