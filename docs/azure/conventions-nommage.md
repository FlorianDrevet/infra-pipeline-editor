# Conventions de Nommage — Ressources Azure

Ce document définit les conventions de nommage pour les ressources Azure du projet **Infra Flow Sculptor**.

---

## Règle générale

```
{préfixe}-{type}-{environnement}-{suffixe}
```

Exemple : `ifs-kv-prod-01` pour un Key Vault en production.

---

## Préfixes par type de ressource

| Type de ressource | Abréviation | Exemple |
|-------------------|-------------|---------|
| Resource Group | `rg` | `rg-ifs-prod` |
| Key Vault | `kv` | `kv-ifs-prod-01` |
| Redis Cache | `redis` | `redis-ifs-prod` |
| Storage Account | `st` | `stifsprod01` (sans tirets — contrainte Azure) |
| App Service | `app` | `app-ifs-api-prod` |
| PostgreSQL | `psql` | `psql-ifs-prod` |

---

## Environnements

| Environnement | Abréviation |
|---------------|-------------|
| Développement | `dev` |
| Staging | `staging` |
| Production | `prod` |

---

## Contraintes Azure

- **Storage Account** : 3–24 caractères, lettres minuscules et chiffres uniquement (pas de tirets).
- **Key Vault** : 3–24 caractères, lettres, chiffres et tirets.
- **Resource Group** : 1–90 caractères, lettres, chiffres, tirets, underscores, parenthèses et points.

---

## Paramètres Infra Flow Sculptor

Dans l'application, les noms sont gérés via des `ParameterDefinition` (avec préfixe et suffixe configurables) :

| Champ | Value Object | Exemple |
|-------|-------------|---------|
| Préfixe | `Prefix` | `ifs` |
| Suffixe | `Suffix` | `01` |
| Nom de base | `Name` | `keyvault` |
| Résultat | — | `ifs-kv-prod-01` (généré par Bicep) |

---

*Référence : [Référence Technique (wiki ADO)](https://dev.azure.com/florian-drevet/Infra%20Flow%20Sculptor/_wiki/wikis/Infra-Flow-Sculptor-Wiki/Reference-Technique)*
