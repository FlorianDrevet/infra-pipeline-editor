---
description: 'Conventions obligatoires pour la création de Pull Requests par les agents GitHub Copilot.'
---
# Conventions Pull Request — InfraFlowSculptor

> Ce fichier est lu par tous les agents GitHub Copilot avant de créer ou de soumettre une Pull Request.  
> Ces conventions sont **obligatoires** et non-négociables.

---

## 1. Format du titre de PR

### Règle absolue

Le titre de la PR doit **toujours** décrire le **but principal** de la PR, jamais la dernière tâche effectuée.

### Format

```
type(scope): description courte du but principal
```

- **type** : un des types listés ci-dessous (obligatoire)
- **scope** : composant ou aggregate concerné, en kebab-case (optionnel, mais recommandé)
- **description** : phrase courte au présent, sans majuscule initiale, sans point final

### Types autorisés

| Type       | Quand l'utiliser |
|------------|-----------------|
| `feat`     | Nouvelle fonctionnalité ou feature CQRS complète |
| `fix`      | Correction d'un bug ou d'un comportement incorrect |
| `refactor` | Restructuration sans changement fonctionnel |
| `perf`     | Amélioration des performances |
| `docs`     | Documentation uniquement (README, MEMORY.md, commentaires) |
| `test`     | Ajout ou modification de tests |
| `chore`    | Maintenance, mise à jour des dépendances, Dockerfile |
| `ci`       | Changements liés aux pipelines CI/CD / GitHub Actions |
| `style`    | Formatage, indentation, lint (aucun impact fonctionnel) |
| `revert`   | Annulation d'un commit ou d'une feature précédente |

### Exemples corrects

```
feat(storage-account): add StorageAccount aggregate with full CRUD
fix(key-vault): correct EF Core LINQ translation for KeyVaultId comparison
refactor(member): extract MemberCommandHelper to reduce duplication
feat(role-assignment): add role assignment management on Azure resources
chore: update Directory.Packages.props to latest package versions
docs: update MEMORY.md with StorageAccount conventions
```

### Exemples incorrects ❌

```
Add StorageAccountConfiguration.cs          ← dernière tâche, pas le but
WIP                                         ← pas de type ni description
Update files                                ← trop vague
feat: done                                  ← pas de description
```

---

## 2. Description de la PR

Utiliser le **template `.github/PULL_REQUEST_TEMPLATE.md`** fourni dans ce dépôt.

### Ce qui est obligatoire dans la description

1. **But principal** — une phrase résumant l'objectif global de la PR
2. **Type de changement** — coche la ou les cases correspondantes
3. **Changements par couche** — pour chaque couche impactée :
   - Domaine : nouveaux agrégats, entités, value objects, erreurs
   - Application : commandes, queries, handlers, validators
   - Infrastructure : configurations EF Core, repositories, migrations
   - Contrats : requests, responses
   - API : endpoints, mappings Mapster
   - Frontend : components/routes/services/facades/guards/environments dans `src/Front`
4. **Migration EF Core** — indiquer si une migration a été ajoutée et son nom
5. **Checklist** — valider chaque point avant de soumettre

### Règle : exhaustivité

Chaque fichier créé ou modifié doit apparaître dans au moins une section de la description.  
Ne pas regrouper tous les changements en une seule ligne vague comme "ajout de la feature".

---

## 3. Protocole de création de PR pour les agents

Quand un agent crée une PR, il doit :

1. **Identifier le but principal** de l'ensemble du travail effectué (pas la dernière modification).
2. **Construire le titre** selon le format `type(scope): description`.
3. **Remplir la description** en utilisant le template et en listant **tous** les fichiers créés ou modifiés, groupés par couche.
4. **S'assurer que le build passe** (`dotnet build .\InfraFlowSculptor.slnx`) avant de soumettre.
5. **Mettre à jour `MEMORY.md`** et inclure cette mise à jour dans la même PR.

---

## 4. Scope recommandés par aggregate / composant

| Scope              | Concerne |
|--------------------|---------|
| `infra-config`     | InfrastructureConfig aggregate |
| `resource-group`   | ResourceGroup aggregate |
| `key-vault`        | KeyVault aggregate |
| `redis-cache`      | RedisCache aggregate |
| `role-assignment`  | RoleAssignment feature |
| `storage-account`  | StorageAccount aggregate (futur) |
| `member`           | Member management within InfrastructureConfig |
| `bicep`            | BicepGenerator API |
| `aspire`           | Aspire AppHost / service orchestration |
| `auth`             | Authentication / authorization |
| `shared`           | Shared.* projects |
| `ef-core`          | Migrations, configurations EF Core transversales |
| `ci`               | GitHub Actions, Dockerfile |

