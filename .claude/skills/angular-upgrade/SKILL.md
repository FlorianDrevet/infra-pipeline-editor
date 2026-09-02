---
name: angular-upgrade
description: "Use when: Angular version upgrade, ng update, CLI/framework/TypeScript/RxJS/Material/npm bump, breaking changes detection, control-flow/signals/standalone migration, new syntax proposals."
---

# Skill: angular-upgrade — Migration Angular, TypeScript, RxJS, npm packages

> **Quand charger :** dès qu'une tâche concerne une montée de version Angular
> (CLI, framework, TypeScript, RxJS, Angular Material, ou packages npm de l'écosystème).

---

## 1. Inventaire de l'état actuel

### Fichiers à inspecter

| Fichier | Information |
|---------|-------------|
| `src/Front/package.json` | Versions Angular, TS, RxJS, Material, dépendances |
| `src/Front/package-lock.json` | Arbre de dépendances résolu |
| `src/Front/angular.json` | Configuration CLI, builders, schematic defaults |
| `src/Front/tsconfig.json` | Options TypeScript, paths, strict mode |
| `src/Front/.eslintrc.*` ou `eslint.config.*` | Configuration lint |

### Commandes de diagnostic

```powershell
cd src\Front

# Version Angular CLI
npx ng version

# Packages outdated
npm outdated

# Vulnérabilités
npm audit

# Vérifier la compatibilité
npx ng update
```

---

## 2. Sources de release notes et breaking changes

### URLs de référence par composant

| Composant | URL |
|-----------|-----|
| Angular Update Guide (interactif) | `https://angular.dev/update-guide` |
| Angular Blog (annonces) | `https://blog.angular.dev/` |
| Angular CHANGELOG | `https://github.com/angular/angular/blob/main/CHANGELOG.md` |
| Angular CLI CHANGELOG | `https://github.com/angular/angular-cli/blob/main/CHANGELOG.md` |
| TypeScript Release Notes | `https://devblogs.microsoft.com/typescript/` |
| TypeScript Breaking Changes | `https://github.com/microsoft/TypeScript/wiki/Breaking-Changes` |
| RxJS CHANGELOG | `https://github.com/ReactiveX/rxjs/blob/master/CHANGELOG.md` |
| Angular Material CHANGELOG | `https://github.com/angular/components/blob/main/CHANGELOG.md` |
| Angular CDK CHANGELOG | inclus dans Angular Components |

### Ce qu'il faut extraire

Pour chaque source :

1. **Breaking changes** — APIs supprimées, comportement modifié, types changés
2. **Deprecations** — APIs marquées deprecated, suppression planifiée
3. **Nouvelles syntaxes** — control flow, signals, deferrable views
4. **Migrations automatiques** — schematics disponibles via `ng update`
5. **Prérequis de version** — TS minimum requis, Node.js minimum, RxJS compatible

---

## 3. Procédure de migration Angular

### Règle fondamentale

**Toujours utiliser `ng update` quand disponible** — il exécute les schematics de migration automatiques qui corrigent une partie des breaking changes.

### Étape 1 — Mettre à jour Angular CLI globalement (optionnel)

```powershell
npm install -g @angular/cli@{NEW_MAJOR}
```

### Étape 2 — Mettre à jour le framework avec ng update

```powershell
cd src\Front
npx ng update @angular/core@{NEW_MAJOR} @angular/cli@{NEW_MAJOR}
```

Si Angular Material est utilisé :
```powershell
npx ng update @angular/material@{NEW_MAJOR}
```

### Étape 3 — Mettre à jour TypeScript si nécessaire

Vérifier la version TS minimale requise par Angular :
```powershell
npm install typescript@{REQUIRED_VERSION} --save-dev
```

### Étape 4 — Mettre à jour RxJS si nécessaire

```powershell
npm install rxjs@{NEW_VERSION}
```

### Étape 5 — Mettre à jour les autres dépendances

```powershell
npm outdated
npm update
# OU mise à jour manuelle des packages critiques
```

### Étape 6 — Compiler et valider

```powershell
npm run typecheck
npm run build
npm run start  # vérifier que l'app démarre
```

---

## 4. Procédure de migration TypeScript

### Points d'attention

TypeScript a des breaking changes fréquents sur :
- La résolution stricte des types (`strictNullChecks`, `exactOptionalPropertyTypes`)
- Les `import type` vs `import`
- Les decorators (legacy vs TC39)
- Le target/module resolution

### Stratégie

1. Mettre à jour la version dans `package.json`
2. `npm install`
3. `npm run typecheck` — lire les erreurs
4. Corriger les erreurs de type une par une
5. Vérifier que `tsconfig.json` est compatible avec le nouveau TS

---

## 5. Patterns de breaking changes courants (Angular)

### 5.1 Suppression d'APIs deprecated

```typescript
// AVANT (Angular < 19)
import { NgModule } from '@angular/core';
@NgModule({ ... })
export class AppModule {}

// APRÈS (standalone)
// Plus de NgModule — standalone components avec bootstrapApplication
```

**Stratégie** : ce projet utilise déjà standalone components, vérifier qu'aucun NgModule résiduel n'existe.

### 5.2 Migration control flow syntax

```html
<!-- AVANT -->
<div *ngIf="condition">...</div>
<div *ngFor="let item of items">...</div>
<div [ngSwitch]="value">...</div>

<!-- APRÈS -->
@if (condition) { <div>...</div> }
@for (item of items; track item.id) { <div>...</div> }
@switch (value) { @case ('a') { ... } }
```

**Stratégie** : Angular fournit un schematic automatique :
```powershell
npx ng generate @angular/core:control-flow
```

### 5.3 Migration vers Signals

```typescript
// AVANT
@Input() name: string;
@Output() nameChange = new EventEmitter<string>();

// APRÈS (signal-based)
name = input<string>();
nameChange = output<string>();
```

**Stratégie** : migration progressive — pas obligatoire d'un coup mais recommandé pour les nouveaux composants.

### 5.4 Migration inject() au lieu de constructor injection

```typescript
// AVANT
constructor(private readonly service: MyService) {}

// APRÈS
private readonly service = inject(MyService);
```

**Stratégie** : ce projet utilise probablement déjà `inject()` — vérifier la cohérence.

### 5.5 RxJS operators deprecated

```typescript
// AVANT
import { map } from 'rxjs/operators';
source.pipe(map(...));

// APRÈS (toujours valide mais vérifier les imports)
import { map } from 'rxjs';
```

### 5.6 HttpClient standalone

```typescript
// AVANT (avec HttpClientModule)
imports: [HttpClientModule]

// APRÈS (standalone)
provideHttpClient(withInterceptorsFromDi())
```

**Note** : ce projet utilise Axios, pas HttpClient Angular natif. Vérifier si Axios a ses propres breaking changes.

---

## 6. Gestion des dépendances npm

### Stratégie de mise à jour par priorité

1. **Angular core** (`@angular/*`) — toujours en premier
2. **TypeScript** — contraint par Angular
3. **RxJS** — contraint par Angular
4. **Angular Material / CDK** — même version major qu'Angular
5. **Build tools** (ESLint, Tailwind, PostCSS) — après le framework
6. **Librairies tierces** (axios, chart.js, etc.) — en dernier

### Peer dependency conflicts

```powershell
# Identifier les conflits
npm install --dry-run

# Si conflit, vérifier la compatibilité :
# - Le package tiers a-t-il une version compatible avec le nouveau Angular ?
# - Existe-t-il une alternative ?
# - Peut-on utiliser --legacy-peer-deps temporairement ? (NON recommandé)
```

### Règles strictes

- **Ne jamais** utiliser `--force` ou `--legacy-peer-deps` sans comprendre pourquoi
- **Ne jamais** downgrade un package sans raison documentée
- **Toujours** vérifier que le `package-lock.json` est cohérent après modification

---

## 7. Propositions de nouvelles fonctionnalités

### Format de proposition

```markdown
## Proposition : [Nom de la feature Angular]

**Source :** [lien release notes / blog]
**Applicable à :** [composants/services/routes du projet]
**Bénéfice :** performance / DX / maintenabilité / UX
**Effort estimé :** faible / moyen / élevé
**Migration automatique disponible :** oui / non (schematic `ng generate ...`)

### Avant
[code actuel]

### Après
[code proposé]

### Impact
[ce qui change concrètement]
```

### Catégories de propositions par release Angular

| Catégorie | Exemples |
|-----------|----------|
| Performance | Signals, deferrable views (`@defer`), zoneless |
| DX | Control flow syntax, built-in pipes améliorés |
| Type safety | Typed forms, typed router, strict inputs |
| Bundle size | Tree-shaking amélioré, standalone bootstrap |
| Testing | Component testing avec signals, TestBed amélioré |
| SSR | Hydration, server-side rendering improvements |

---

## 8. Pièges connus spécifiques au projet

### Axios vs HttpClient

- Ce projet utilise **Axios** (pas le HttpClient Angular natif)
- Les breaking changes Angular sur `HttpClient` ne s'appliquent pas directement
- Mais vérifier les interceptors et la configuration Axios après upgrade TS

### Angular Material + Tailwind

- Le projet combine Angular Material et Tailwind CSS
- Après upgrade Material, vérifier que les styles custom ne sont pas cassés
- Les palettes de couleurs Material changent entre versions majeures

### Standalone components

- Le projet est full standalone — pas de `NgModule`
- Les schematics `ng update` supposent parfois NgModule → vérifier qu'ils n'en réintroduisent pas

### i18n / ngx-translate

- Vérifier la compatibilité des packages de traduction avec le nouveau Angular
- Les fichiers JSON de traduction ne changent pas, mais le service peut avoir des breaking changes

### Auth (MSAL / Azure AD)

- `@azure/msal-angular` a sa propre cadence de release
- Toujours vérifier la compatibilité MSAL ↔ Angular version

### Tailwind CSS

- Si Tailwind passe en major (ex: 3→4), c'est une migration séparée
- Ne pas mélanger upgrade Angular et upgrade Tailwind dans la même PR

---

## 9. Commandes de validation post-migration

```powershell
cd src\Front

# Typecheck strict
npm run typecheck

# Build production
npm run build

# Lint (si configuré)
npm run lint

# Démarrage local (vérification manuelle)
npm run start
```

---

## 10. Checklist de validation post-migration

```
[ ] package.json versions mises à jour
[ ] package-lock.json régénéré proprement (npm install)
[ ] npm run typecheck — 0 erreurs
[ ] npm run build — build production OK
[ ] npm run lint — aucune nouvelle erreur lint
[ ] Pas de NgModule réintroduit par les schematics
[ ] Styles Material/Tailwind visuellement corrects
[ ] Auth (MSAL) fonctionne
[ ] i18n fonctionne
[ ] Aucun warning deprecation non traité dans la console
[ ] Mémoire projet mise à jour (.claude/memory/)
```
