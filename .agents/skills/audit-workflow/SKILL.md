---
name: audit-workflow
description: "Use when: code audit, technical audit, security audit, performance audit, scalability audit, database audit, audit markdown, GitHub audit issues, findings reconciliation, labels sync, close resolved audit issues, create new audit issues."
---

# Skill: audit-workflow

## Goal

Fournir une methode stable pour produire un audit expert du depot et synchroniser les findings avec GitHub sans perdre l'historique entre deux audits.

## Audit Scope

Un audit complet doit couvrir au minimum :

1. Securite
2. Base de donnees / EF Core
3. Domain / DDD
4. Application / CQRS
5. API / Contracts
6. Generation Bicep / Pipeline
7. Architecture transversale
8. Tests / couverture / regressions

## Severity Rubric

- `critical` : fuite de donnees, execution non autorisee, corruption potentielle, crash critique, aucune protection de base
- `high` : risque significatif de perf, architecture fragile, forte dette, regressions probables
- `medium` : problemes structurants mais non bloquants a court terme
- `low` : backlog et hygiene secondaire

Le rapport peut afficher les severites en francais, mais la synchronisation GitHub doit les convertir vers :

- `severity: critical`
- `severity: high`
- `severity: medium`
- `severity: low`

## Finding Format

Chaque finding doit respecter ce format logique :

- identifiant stable : `SEC-001`, `DB-004`, `APP-010`, etc.
- titre court et precis
- severite
- fichier(s) ou zone(s) impacte(s)
- constat factuel
- risque / impact
- recommandation concrete

Ne renumerote pas des findings historiques si le probleme est le meme. La stabilite des IDs conditionne la reconciliation GitHub.

## Audit File Convention

Nom de fichier attendu :

- `audits/audit-dd-MM-yyyy.md`

Structure recommandee :

1. titre du rapport
2. resume executif
3. chiffres cles du codebase
4. findings par severite
5. plan d'action par phases
6. metriques cibles
7. references techniques

## GitHub Label Strategy

Source de verite : `.github/audit/config.json`

Le repertoire doit au minimum gerer :

- label generique `audit`
- label mensuel `audit: yyyy-MM`
- label `status: new`
- labels `severity:*`
- labels `area:*`
- labels `phase:*`

Quand un nouveau besoin de label apparait :

1. ajouter la definition dans `.github/audit/config.json`
2. executer la synchronisation avec `-EnsureLabels`

## Audit Issue Lifecycle

Les issues d'audit doivent etre gerees par identifiant de finding, jamais par similitude approximative de titre.

Regles :

1. Si le finding existe deja et reste present :
   l'issue reste ouverte, le corps est mis a jour, `status: new` est retire s'il etait encore present.
2. Si le finding n'existait pas :
   une issue est creee avec `status: new`.
3. Si le finding etait ferme puis reapparait :
   l'issue est reouverte et recoit a nouveau `status: new`.
4. Si une issue d'audit ouverte n'apparait plus dans le dernier rapport :
   elle est fermee automatiquement avec un commentaire de resolution base sur l'audit.

## Audit Issue Body Requirements

Chaque issue d'audit doit contenir des marqueurs machine lisibles :

```md
<!-- audit-finding-id: SEC-001 -->
<!-- audit-source: audits/audit-14-04-2026 -->
<!-- audit-first-seen: audit-14-04-2026 -->
<!-- audit-last-seen: audit-14-04-2026 -->
```

Ces marqueurs ne doivent pas etre supprimes par l'automatisation.

## PowerShell Execution

Commande standard de synchronisation :

```powershell
pwsh -NoLogo -NoProfile -File .\scripts\sync-audit-issues.ps1 -AuditFile .\audits\audit-14-04-2026 -Repo FlorianDrevet/infra-pipeline-editor -EnsureLabels
```

Commande pour seulement lister les labels :

```powershell
pwsh -NoLogo -NoProfile -File .\scripts\sync-audit-issues.ps1 -Repo FlorianDrevet/infra-pipeline-editor -ListLabels
```

Commande de validation sans effet distant :

```powershell
pwsh -NoLogo -NoProfile -File .\scripts\sync-audit-issues.ps1 -AuditFile .\audits\audit-14-04-2026 -Repo FlorianDrevet/infra-pipeline-editor -EnsureLabels -WhatIf
```

## Heuristics For Type Labels

- `type: bug` pour securite, crash, autorisation, erreurs de comportement
- `type: performance` pour N+1, table scans, index manquants, tracking inutile, complexite runtime
- `type: refactor` pour god class, duplication, encapsulation faible, cohesion faible
- `type: tech-debt` par defaut si aucun type plus precis ne s'impose

## Practical Rule

L'audit n'est pas termine tant que :

- le rapport n'est pas ecrit dans `audits/`
- les labels requis sont connus et maintenus
- les issues GitHub ont ete reconciliees avec le dernier audit