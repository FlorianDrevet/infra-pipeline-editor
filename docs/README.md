# Documentation — Infra Flow Sculptor

Ce dossier contient la documentation technique du projet **Infra Flow Sculptor**, versionnée directement dans le dépôt Git.

---

## Structure

| Dossier | Contenu |
|---------|---------|
| [`architecture/`](./architecture/overview.md) | Architecture du projet, DDD, CQRS, couche API, persistance, guide de navigation |
| [`azure/`](./azure/README.md) | Documentation Azure : ressources, architecture cloud, conventions de nommage, sécurité |
| [`features/`](./features/) | Documentation fonctionnelle par feature |

---

## Architecture et concepts

> **Point d'entrée recommandé pour les nouveaux développeurs.**

1. [Vue d'ensemble de l'architecture](architecture/overview.md) — Stack, structure de la solution, couches, flux d'une requête
2. [Domain-Driven Design (DDD)](architecture/ddd-concepts.md) — Agrégats, entités, value objects, erreurs de domaine
3. [CQRS et MediatR](architecture/cqrs-patterns.md) — Commandes, queries, handlers, validation, pipeline, ErrorOr
4. [Unit of Work](architecture/unit-of-work.md) — Persistance atomique, interfaces de marquage CQRS, pipeline MediatR
5. [Couche API](architecture/api-layer.md) — Endpoints Minimal API, Mapster, gestion d'erreurs, contracts
6. [Persistance EF Core](architecture/persistence.md) — Repositories, configurations, TPT, converters, migrations
7. [Lecture des ressources projet et views SQL](architecture/project-resource-queries.md) — Flux `GET /projects/{id}/resources`, read models keyless, rôle de `vw_ResourceEnvironmentEntries` et `vw_ChildToParentLinks`
8. [Guide de navigation](architecture/getting-started.md) — Comment trouver du code, ajouter un nouveau type de ressource
9. [Génération Bicep](architecture/bicep-generation.md) — Guide complet d’onboarding : flux de génération, pipeline, IR, choix d’architecture, navigation pas à pas dans le code
10. [Génération Pipeline](architecture/pipeline-generation.md) — Moteur de génération de pipelines Azure DevOps

---

## Features

1. [Push Bicep vers Git](features/push-bicep-to-git.md) — Fonctionnalité de push des fichiers générés vers un dépôt Git
2. [Références cross-config](features/cross-config-references.md) — Dépendances entre configurations d'infrastructure

---

## Liens utiles

- [Wiki Azure DevOps](https://dev.azure.com/florian-drevet/Infra%20Flow%20Sculptor/_wiki/wikis/Infra-Flow-Sculptor-Wiki)
- [Dépôt GitHub](https://github.com/FlorianDrevet/infra-pipeline-editor)
- [Backlog Azure DevOps](https://dev.azure.com/florian-drevet/Infra%20Flow%20Sculptor/_backlogs)

---

> **Convention :** la documentation est versionnée avec le code. Toute documentation Azure va dans `docs/azure/`, l'architecture et les concepts dans `docs/architecture/`.
