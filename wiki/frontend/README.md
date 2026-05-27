# Frontend Documentation

> Angular 21 SPA: standalone components, signals, Material 21 + Tailwind, design system.

## Table of Contents

1. [Architecture](architecture.md) — Module structure, routing, state management
2. [Design System](design-system.md) — DS components, patterns, migration
3. [Authentication](authentication.md) — MSAL flow, guards, token management
4. [Services](services.md) — HTTP layer, caching, promise coalescing
5. [i18n](i18n.md) — Internationalization strategy
6. [Routing](routing.md) — Route definitions, lazy loading, guards

---

## Tech Stack

| Technology | Version | Purpose |
|-----------|---------|---------|
| Angular | 21 | Framework |
| TypeScript | 5.8+ | Language |
| Angular Material | 21 | Component library |
| Tailwind CSS | 4 | Utility-first styling |
| Axios | Latest | HTTP client |
| MSAL Angular | Latest | Azure AD auth |
| ngx-translate | Latest | i18n |
| zoneless | Default | No Zone.js, signals-based |

---

## Key Architectural Decisions

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Components | Standalone | No NgModules, tree-shakable |
| State | Signals | Built-in, no RxJS complexity |
| HTTP | Axios (not HttpClient) | Promise-based, interceptors, cancel |
| DS | `app-ds-*` components | Consistent UI, single point of change |
| Styling | Material + Tailwind | Rich components + utility styling |
| Auth | MSAL direct | Official Microsoft library |
| i18n | ngx-translate | Runtime language switching |
