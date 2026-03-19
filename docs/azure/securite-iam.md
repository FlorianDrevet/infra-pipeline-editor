# Sécurité & IAM — Infra Flow Sculptor

## Azure Entra ID (anciennement Azure Active Directory)

### App Registrations

| Rôle | Client ID | Audience |
|------|-----------|---------|
| API principale | `4f7f2dbd-8a11-42c4-bedc-acbb127e9394` | `api://4f7f2dbd-8a11-42c4-bedc-acbb127e9394` |
| Frontend Angular | `24c34231-a984-43b3-8ac3-9278ebd067ef` | N/A (MSAL PKCE) |

### Scopes exposés (API)

| Scope | Description | Accès |
|-------|-------------|-------|
| `Configuration.Write` | Écriture de configuration infrastructure | Contributeurs et Owners |

### Rôles applicatifs (domaine)

Les rôles sont gérés **au niveau applicatif** (pas des rôles Azure RBAC natifs) :

| Rôle | Droits |
|------|--------|
| `Owner` | Lecture + Écriture + Gestion des membres |
| `Contributor` | Lecture + Écriture |
| `Reader` | Lecture seule |

---

## Authentification des services

### Backend → Azure Resources

| Service | Méthode d'auth | Notes |
|---------|----------------|-------|
| API → PostgreSQL | Connection string | Configurable via Key Vault en prod |
| API → Blob Storage | Connection string / Managed Identity | `AzureBlobStorageConnectionString` |
| API → Key Vault | Managed Identity | En production |

### Frontend → API

```
Angular (MSAL PKCE) → Azure Entra ID → JWT Bearer → InfraFlowSculptor.Api
```

- Le token JWT est validé côté API via `MicrosoftIdentityWeb`.
- L'audience attendue : `api://4f7f2dbd-8a11-42c4-bedc-acbb127e9394`.
- La politique par défaut (`FallbackPolicy`) exige un utilisateur authentifié.
- La politique `IsAdmin` exige le rôle `Admin` dans le token.

---

## Bonnes pratiques sécurité

1. **Ne jamais stocker de secrets dans le code** — utiliser Azure Key Vault ou les variables d'environnement.
2. **Managed Identity en production** — éviter les service principals avec secrets rotatifs.
3. **TLS minimum** — toutes les ressources Azure (Redis, Storage) doivent avoir TLS 1.2 minimum en production.
4. **HTTPS uniquement** — `supportsHttpsTrafficOnly: true` sur les Storage Accounts de production.
5. **RBAC Azure** — appliquer le principe du moindre privilège pour les Managed Identities.

---

*Référence : [Authentification (wiki ADO)](https://dev.azure.com/florian-drevet/Infra%20Flow%20Sculptor/_wiki/wikis/Infra-Flow-Sculptor-Wiki/Reference-Technique/Authentification)*
