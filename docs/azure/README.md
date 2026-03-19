# Documentation Azure — Infra Flow Sculptor

Documentation de référence pour les ressources Azure utilisées par le projet **Infra Flow Sculptor**.

---

## Sommaire

| Page | Description |
|------|-------------|
| [Architecture Cloud](./architecture.md) | Vue d'ensemble de l'architecture Azure (Entra ID, PostgreSQL, Blob Storage, Key Vault…) |
| [Ressources Managées](./ressources.md) | Détail des ressources Azure provisionnées (groupes de ressources, nommage, localisation) |
| [Sécurité & IAM](./securite-iam.md) | Azure Entra ID, rôles RBAC, service principals, secrets |
| [Conventions de Nommage](./conventions-nommage.md) | Règles de nommage des ressources Azure du projet |

---

## Contexte

**Infra Flow Sculptor** est une solution .NET qui permet de :
1. Stocker et versionner des configurations d'infrastructure Azure via une API REST.
2. Générer automatiquement des fichiers **Azure Bicep** à partir de ces configurations.
3. Provisionner les ressources Azure de manière reproductible via des pipelines CI/CD.

---

## Mise à jour de cette documentation

Pour modifier la documentation :
1. Créer une branche Git depuis `main`.
2. Modifier ou ajouter les fichiers Markdown dans ce dossier (`docs/azure/`).
3. Ouvrir une Pull Request — la review de la doc fait partie du processus normal.
4. Après merge, mettre à jour la page correspondante dans le [wiki Azure DevOps](https://dev.azure.com/florian-drevet/Infra%20Flow%20Sculptor/_wiki/wikis/Infra-Flow-Sculptor-Wiki/Documentation-Azure) si nécessaire.

---

*Dernière mise à jour : Mars 2026*
