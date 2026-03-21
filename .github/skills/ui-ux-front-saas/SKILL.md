---
name: ui-ux-front-saas
description: "Use when: ui ux, design system, figma handoff, wireframes, user flow, dashboard, landing page, angular frontend screen, component styling, responsive, accessibility, dark mode, visual consistency, SaaS B2B cloud"
---

# Skill: ui-ux-front-saas

Use this skill for frontend UI/UX work in InfraFlowSculptor.

This skill transforms product/design intent into implementable Angular UI decisions, while keeping consistency with the existing login page and product goals.

## 1) Product Context (source of truth)

- Product: InfraFlowSculptor
- Goal: Configure Azure infrastructure as data, then generate Bicep and Azure DevOps pipeline artifacts.
- Target frontend stack: Angular 19 (standalone), Angular Material, Tailwind CSS, SCSS, Axios.
- Backend context: .NET Minimal API with CQRS/DDD.
- Auth: Microsoft Entra ID with roles and permissions.
- Tone: professional, reliable, premium, clear, operationally efficient.

## 2) Visual Baseline to Reuse (existing login)

Before proposing any new UI direction, read and align with:

- src/Front/src/app/features/login/login.component.html
- src/Front/src/app/features/login/login.component.scss

Extract and preserve the existing visual DNA:

- Split-screen composition for strategic pages (where useful)
- Cloud-inspired gradient direction (deep blue to cyan), premium and technical
- Clear typography hierarchy and strong onboarding copy
- Visible status/error affordances
- Controlled density and clean spacing

Do not introduce random style shifts that clash with the login visual baseline.

## 3) Personas to Design For

1. Cloud/Platform Engineer (power user)
2. DevOps Engineer (artifact consumer)
3. Tech Lead/Manager (governance and global visibility)

## 4) UX Objectives (must be visible in the UI)

- Reduce perceived Azure complexity
- Make creation/editing fast and safe
- Make relations explicit: Infrastructure Config -> Resource Groups -> Resources
- Explain validation errors pedagogically
- Reinforce control, security, and traceability

## 5) Feature Surfaces to Cover

- Auth flow (Entra ID + unauthenticated state)
- Connected dashboard/landing
- Infrastructure Config list (search, sort, filters, status)
- Infrastructure Config detail sections:
  - Members & roles (Owner/Contributor/Reader)
  - Environments
  - Parameter definitions/values
  - Naming templates
  - Linked resource groups
- CRUD: Resource Group, Key Vault, Redis Cache, Storage Account
- Generate Bicep CTA: loading/success/error + simple history
- Empty states, API error states, destructive confirmations, snackbars
- User profile and visible permissions

## 6) Design System Requirements

Always design and implement with reusable tokens/components first.

### 6.1 Tokens

Define and use tokens for:

- Color (semantic + brand + neutral)
- Typography scale
- Spacing scale
- Radius scale
- Elevation/shadow
- Focus states
- Disabled/loading/error/success states

### 6.2 Components

Ensure reusable component coverage for:

- Buttons
- Inputs
- Selects
- Tables
- Chips/tags
- Tabs
- Cards
- Modals/dialogs
- Drawers
- Breadcrumbs
- Pagination
- Alerts/snackbars

Each component must define variants and states (hover, focus, disabled, error, loading).

## 7) Accessibility and Responsiveness (non-negotiable)

- WCAG 2.1 AA contrast
- Visible focus style
- Keyboard navigation paths
- Adequate hit areas and touch targets
- Responsive behavior:
  - Desktop-first
  - Tablet adaptation
  - Mobile adaptation
- Support both light mode and dark mode

## 8) Visual Direction

- Style: Azure-native SaaS (technical, modern, trustworthy, not cold)
- Palette: deep blues + cyan/violet accents, with enterprise neutral fallback
- Typography: highly readable with strong hierarchy
- Iconography: coherent infra/cloud/security semantics

No gratuitous visual effects. Function clarity comes first.

## 9) Output Contract for Design Tasks

When the task is design-heavy (Figma, screen architecture, UX specs), produce:

1. Information architecture (site map)
2. At least 5 key user flows
3. Low-fi wireframes of key screens
4. High-fi mockups ready for dev handoff
5. Design system and reusable component library
6. Clickable prototype for critical journeys
7. Handoff specs:
   - Dimensions and spacing
   - Tokens
   - Component behavior
   - Interaction/state annotations
   - Responsive rules

Also provide:

- V1 (MVP) scope
- V2 (advanced UX improvements)

## 10) Output Contract for Angular Implementation Tasks

When coding Angular screens/components:

- Reuse existing shared styles/patterns before creating new ones
- Keep design system consistency across pages
- Implement clear loading/empty/error states
- Keep permission visibility explicit in UI actions and labels
- Use realistic domain data examples:
  - InfrastructureConfig
  - ResourceGroup
  - KeyVault
  - RedisCache
  - StorageAccount
  - Members
  - Environments
  - Generate Bicep

## 11) Guardrails

- Do not optimize for flashy visuals over task efficiency
- Do not create one-off components when reusable patterns are possible
- Do not break existing login visual language
- Do not ship inaccessible color/focus combinations

## 12) Practical Workflow

1. Read login HTML/SCSS baseline
2. Map task to persona and UX objective
3. Use/extend tokens and reusable components first
4. Validate states (loading, empty, error, destructive confirmation)
5. Validate responsive + keyboard + contrast
6. Ship with V1 now, and list V2 improvements separately
