# Frontend - InfraFlowSculptor

Frontend Angular 19 base pour piloter l'API CQRS du projet.

## Stack

- Angular 19 (standalone + zoneless change detection)
- Angular Material 19
- Tailwind CSS 3
- Axios + JWT (`@auth0/angular-jwt`) + cookies

## Commandes

```bash
npm install
npm run start
npm run start:aspire
npm run build
npm run build:dev
npm run typecheck
```

- `npm run start` lance le frontend en mode développement manuel sur `http://localhost:4201`
- `npm run start:aspire` réserve `http://localhost:4200` pour l'exécution orchestrée par Aspire

## Configuration API

- Environnement dev: `src/environments/environment.development.ts`
- Environnement prod: `src/environments/environment.ts`
- Environnement Aspire: `src/environments/environment.aspire.ts`
- URL API utilisée par Axios: `environment.api_url`

En développement, le frontend utilise le proxy `/api-proxy` et `/bicep-api-proxy`.
Par défaut (hors variables Aspire), le proxy pointe vers:
- API principale: `http://localhost:5257`
- API Bicep: `http://localhost:5258`

## Structure

- `src/app/core`: layouts globaux (navigation, footer)
- `src/app/shared`: briques transverses (services, facades, guards, enums, interfaces)
- `src/environments`: configuration runtime par environnement
- `src/scss`: variables et theming global

## Theming

- Theme Material defini dans `src/styles.scss`
- Tailwind configure dans `tailwind.config.js`

## Azure Storage Emulator (optionnel)

```bash
azurite --silent --location c:\azurite --debug c:\azurite\debug.log
```
