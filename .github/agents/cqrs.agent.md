---
name: cqrs-generator
description: 'Génère les commandes/queries, endpoint, requests/reponses, les services et les handlers associés pour le projet CQRS.'
---
# CRQS Agent
L'objectif de cet agent est de générer les commandes/queries, endpoint, requests/reponses, les services et les handlers associés pour le projet CQRS.

Il doit suivre les règles strictes de DDD pour le domain, et respecter la structure du projet.

Les controllers se trouvent dans src/Api/InfraFlowSculptor.Api/Controllers.
Les requests et responses se trouvent dans src/Api/InfraFlowSculptor.Contracts.
Les interfaces des services se trouvent dans src/Api/InfraFlowSculptor.Application/Common et leur implementation dans src/Api/InfraFlowSculptor.Infrastructure/Services.
Les query/command, validateurs et les handlers se trouvent dans src/Api/InfraFlowSculptor.Application.
Pour passer de request -> query/command -> query/command response -> reponse j'utilise un mapper qui s'appelle Mapster
et toute la configuration de Mapster se trouve dans src/Api/InfraFlowSculptor.Api/Common/Mapping.

Tu partiras soit d'un model d'entity que tu devras créer dans la couche domain, avec les repositories associés et la configuration EF Core,
soit d'une entité déjà existante dans la couche domain pour laquelle tu devras uniquement créer les commandes/queries, endpoint, requests/reponses, les services et les handlers associés.

Il doit également générer les configurations EF Core associées dans le dossier src/Api/InfraFlowSculptor.Infrastructure/Persistence/Configurations.

Si un contrat backend est modifié, il doit vérifier l'impact frontend dans src/Front (interfaces/services/facades) et proposer la mise à jour associée.

---

## Pull Request — Règles obligatoires

Avant de créer ou soumettre une PR, consulter `.github/agents/pr-conventions.agent.md`.

### Titre de la PR

```
type(scope): description courte du but principal
```

- `type` : `feat` | `fix` | `refactor` | `perf` | `docs` | `test` | `chore` | `ci` | `style` | `revert`
- `scope` : aggregate ou composant en kebab-case (ex : `key-vault`, `storage-account`, `bicep`)
- Le titre décrit le **but global**, jamais la dernière tâche effectuée.

### Description de la PR

Utiliser le template `.github/PULL_REQUEST_TEMPLATE.md` et lister tous les fichiers créés/modifiés par couche (Domain, Application, Infrastructure, Contracts, API, etc.).
