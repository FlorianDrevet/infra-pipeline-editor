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
npm run build
npm run build:dev
npm run typecheck
```

## Configuration API

- Environnement dev: `src/environments/environment.development.ts`
- Environnement prod: `src/environments/environment.ts`
- URL API utilisée par Axios: `environment.api_url`

Par defaut, l'URL est `http://localhost:8080`.

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
