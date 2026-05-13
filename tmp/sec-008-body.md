Source : detection automatique pendant build session 2026-05-12 (PR #328).

## Constat
La compilation de `InfraFlowSculptor.Infrastructure` (et par transitivite `Infrastructure.Tests` + `Mcp.Tests`) emet le warning NuGet `NU1904` :

> Package 'Microsoft.AspNetCore.DataProtection' 10.0.0 has a known **critical** severity vulnerability
> https://github.com/advisories/GHSA-9mv3-2cwr-p262

Le package est resolu transitivement (probablement via `Microsoft.Identity.Web` ou Aspire). La version 10.0.0 est marquee comme vulnerable par GHSA.

## Risque
- Severite **CRITIQUE** d'apres NuGet / GitHub Security Advisory
- Le package gere les ASP.NET Core Data Protection keys (signature antiforgery, cookies d'authentification, secrets temporaires)
- Une faille critique sur ce composant peut compromettre l'authentification, la signature des cookies, et l'integrite des donnees protegees

## Contexte
- Detecte sur la branche `refacto/audit-12-05` (PR #328) lors d'une session autonome de triage 2026-05-12
- Le build passe (warning et non erreur), mais la production ne devrait pas embarquer cette version
- Sonar pourrait egalement remonter cette vulnerabilite

## Recommandation
1. Identifier la source de la dependance transitive : `dotnet list package --vulnerable --include-transitive --project src/Api/InfraFlowSculptor.Infrastructure/InfraFlowSculptor.Infrastructure.csproj`
2. Forcer une version corrigee dans `Directory.Packages.props` (ex. 10.0.1+ une fois disponible) ou pin du parent
3. Verifier l'impact sur Aspire / `Microsoft.Identity.Web`
4. Ajouter `dotnet list package --vulnerable` dans le pipeline CI comme gate de qualite

## Criteres d'acceptation
- [ ] Le warning NU1904 disparait du build
- [ ] La fonctionnalite Data Protection (cookies auth, antiforgery) reste operationnelle
- [ ] Le pipeline CI bloque toute nouvelle vulnerabilite critique transitive
