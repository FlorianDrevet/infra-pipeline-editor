# UI Refresh 2026-05 — Direction artistique enterprise & plan de refonte phasé

> Livrable architecte. Ne contient aucun code de production hors annexe tokens.
> Cible : transformer le frontend Angular 21 InfraFlowSculptor en plateforme enterprise premium (référentiel : Linear, Vercel, Stripe Dashboard, Datadog, GitHub Enterprise, Azure Portal modernisé).
> Consommateur : agent `angular-front`, vague par vague, en respectant le skill `tdd-workflow`.

---

## 1. Résumé exécutif

L'UI actuelle empile trois couches d'identité incompatibles : un fond app dégradé radial+linéaire ([app.component.scss](src/Front/src/app/app.component.scss#L13-L17)), un voile glassy+blur omniprésent (nav, footer, cards) et des dégradés brand cyan agressifs sur quasi chaque surface (277 occurrences `gradient|backdrop-filter|blur` dans `project-detail.component.scss`). Résultat : look « template IA flashy » qui décrédibilise l'usage enterprise long. L'objectif est d'imposer une seule identité dark sobre type Linear/Vercel : palette bleu désaturé + 1 accent cyan unique réservé aux actions critiques, surfaces planes à 2 niveaux d'élévation, gradients réservés à login + bouton primary, typo Inter à échelle resserrée, radius limités à 4/8/12px, motion 120/180/240ms. La refonte se fait en 6 vagues mergeables. La vague 1 (tokens + typographie + tailwind + override Material) est la fondation : elle change l'image globale sans toucher aux templates HTML, par retombée des tokens. Les vagues suivantes refondent primitives DS, shell, pages denses, formulaires, puis polish.

---

## 2. Audit visuel actuel

### 2.1 Palette & couleurs

- Famille brand sursaturée et trop variée : 7 bleus/cyan définis dans [_tokens.scss](src/Front/src/scss/_tokens.scss#L7-L13) (`#0d2f66`, `#1565c0`, `#1a3a8a`, `#0288d1`, `#00bcd4`, `#00acc1`, `#009fbd`) → bruit visuel, aucune hiérarchie d'accent.
- 5 dégradés signature concurrents ([_tokens.scss](src/Front/src/scss/_tokens.scss#L54-L66)) : `brand`, `brand-soft`, `login`, `nav`, `cta`, `card-soft`, `app-bg`, `glass-footer`. Aucun n'est réservé à un rôle, ils s'empilent.
- Fond app dégradé radial cyan + bleu + linéaire ([app.component.scss](src/Front/src/app/app.component.scss#L13-L17)) → rend chaque écran « hero » et écrase la hiérarchie des cards.
- Border tokens définis en `rgba(146,191,235, …)` ([_tokens.scss](src/Front/src/scss/_tokens.scss#L70-L72)) — teinte brand bleu-cyan injectée même dans des surfaces neutres, donc aucune surface vraiment neutre.
- Aucun token dark mode alors que la cible affichée dans le brief est dark-by-default. Les composants dark sont fabriqués ad hoc (ex. nav `linear-gradient(135deg, #0d2f66 0%, #1565c0 55%, #0288d1 100%)` dans [navigation.component.scss](src/Front/src/app/core/layouts/navigation/navigation.component.scss#L11)).

### 2.2 Typographie

- 9 niveaux de scale (`xs` à `4xl`) dans [_typography.scss](src/Front/src/scss/_typography.scss#L9-L17) → trop d'options, hiérarchie diluée.
- Poids incohérents : `xs` 500, `sm` 400, `base` 400, `md` 500, `lg` 600, `xl/2xl/3xl/4xl` 700 → le 700 sur tous les headings est lourd, manque de finesse type SaaS premium.
- Stack système (`-apple-system, Segoe UI, …`) au lieu d'une font enterprise neutre type Inter / Geist → rendu différent Mac/Windows, manque de cohérence de marque.
- `letter-spacing` non standardisé (présent ad hoc partout : nav `0.01em`, language switch `0.03em`, etc.).
- `line-height` 1.6 sur body ([_typography.scss](src/Front/src/scss/_typography.scss#L11)) excessif pour des écrans denses (project-detail, resource-edit).

### 2.3 Spacing & rythme

- Échelle existante en rem mais discontinue : `0, 0.25, 0.5, 0.75, 1, 1.25, 1.5, 2, 2.5, 3, 4` ([_tokens.scss](src/Front/src/scss/_tokens.scss#L100-L111)) → manque 0.375 (6), 0.625 (10), pas de 5rem (80) pour gros gaps.
- Pages denses utilisent des valeurs ad hoc non tokenisées : `0.85rem`, `0.35rem`, `0.82rem` (ex. [ds-text-field.component.scss](src/Front/src/app/shared/components/ds/ds-text-field/ds-text-field.component.scss#L13-L18)) → personne ne peut auditer la grille.
- `min-height: 5rem` sur la nav ([navigation.component.scss](src/Front/src/app/core/layouts/navigation/navigation.component.scss#L19)) → top-bar trop haute pour usage enterprise (cible : 3.5–4rem).
- Aucune notion de densité (compact/cozy/comfortable) alors que `resource-edit` et `home` ont des besoins opposés.

### 2.4 Surfaces & profondeur

- 9 ombres définies ([_tokens.scss](src/Front/src/scss/_tokens.scss#L86-L96)) avec teintes bleues fortes (`rgba(13,47,102,0.22)` à `0.35`) → toutes les ombres sont colorées, fatiguent l'œil et sapent la sensation neutre/enterprise.
- 7 radius (`6,12,16,20.8,22.4,25.6` + pill) → cards pleines de tailles, perte d'unité.
- Glass partout : nav `backdrop-filter: blur(18px)` ([navigation.component.scss](src/Front/src/app/core/layouts/navigation/navigation.component.scss#L10)), footer `blur(14px)` ([footer.component.scss](src/Front/src/app/core/layouts/footer/footer.component.scss#L10)), `ds-card--glass` ([ds-card.component.scss](src/Front/src/app/shared/components/ds/ds-card/ds-card.component.scss#L23-L27)), mixin `ifs-glass` ([_mixins.scss](src/Front/src/scss/_mixins.scss#L9-L13)) → effet "macOS Big Sur démo", incompatible avec un usage clavier-orienté long.
- Bordures triples : `border-soft`, `border-medium`, `border-strong` toutes en bleu/cyan rgba ([_tokens.scss](src/Front/src/scss/_tokens.scss#L70-L72)) → impossible d'avoir un trait neutre.

### 2.5 Composants

- `app-ds-button` primary : double gradient + ombre colorée + inset highlight + transitions sur 5 propriétés ([ds-button.component.scss](src/Front/src/app/shared/components/ds/ds-button/ds-button.component.scss#L60-L82)) → bouton « hero » répliqué dans toutes les dialogues, casse l'usage standard.
- `app-ds-text-field` : `border: 1.5px` ([ds-text-field.component.scss](src/Front/src/app/shared/components/ds/ds-text-field/ds-text-field.component.scss#L33)), focus shadow `rgba(21,101,192,0.15)` + `inset 0 0 0 0.5px` ([ds-text-field.component.scss](src/Front/src/app/shared/components/ds/ds-text-field/ds-text-field.component.scss#L40-L43)) → halo lourd, label en couleur brand ([ds-text-field.component.scss](src/Front/src/app/shared/components/ds/ds-text-field/ds-text-field.component.scss#L18)) au lieu de neutre = soupe bleue.
- `app-ds-card` : 3 variantes paddings + 4 accents colorés (border-left 3px) + variant glass + variant elevated. Aucun composant d'usage clair, tout est paramétrable, donc rien n'est consistant.
- `app-ds-tabs`, `app-ds-table`, `app-ds-tooltip`, `app-ds-skeleton`, `app-ds-empty-state`, `app-ds-tree-view`, `app-ds-segmented-control` n'existent pas dans `shared/components/ds/` (cf. ls `ds/`) → re-implémentés à la main dans `project-detail.component.scss` (50 220 octets de SCSS local) et `config-detail.component.scss` (62 784 octets).

### 2.6 Layout & navigation

- Nav 80px de haut, dégradé bleu+cyan, blur 18px, ombre `0 4px 24px rgba(13,47,102,0.35)` ([navigation.component.scss](src/Front/src/app/core/layouts/navigation/navigation.component.scss#L8-L13), [_tokens.scss](src/Front/src/scss/_tokens.scss#L96)) → top-bar dominante alors que les références (Linear/Vercel) ont 48–56px sobres.
- Pas de sidebar applicative pour project-detail : toute la nav contextuelle vit dans la page, mélangée au contenu.
- Footer plein largeur grid 3 colonnes avec backdrop-filter ([footer.component.scss](src/Front/src/app/core/layouts/footer/footer.component.scss#L10)) → un footer marketing dans une app SaaS, à supprimer ou réduire à une status bar.
- Container `width: min(1200px, calc(100% - 2rem))` ([navigation.component.scss](src/Front/src/app/core/layouts/navigation/navigation.component.scss#L17)) → max 1200px imposé même sur écrans 4K, gâche la densité disponible pour les pages denses.

### 2.7 Micro-détails

- Hover lift `translateY(-2px) + ombre amplifiée` ([_mixins.scss](src/Front/src/scss/_mixins.scss#L28-L33)) appliqué aux cards → effet "carte qui flotte" enfantin pour une app de production.
- Focus ring défini en token mais override local courant (ex. `outline: 2px solid rgba(0, 188, 212, 0.85)` dans [app.component.scss](src/Front/src/app/app.component.scss#L48)) → ring inconsistant.
- Transitions multi-propriétés (`transition: all`, `transition: background…color…box-shadow…border-color…transform`) → coût rendu et animations fragiles.
- Annotation trigger en floating cyan flashy ([app.component.scss](src/Front/src/app/app.component.scss#L23-L41)) — micro-détail emblématique du look "template IA".

### 2.8 États

- Pas de skeleton primitif. Les écrans de chargement sont gérés par mat-spinner sur fond blanc.
- Pas d'empty state primitif. Chaque feature ré-invente (icône + texte + CTA) sans alignement.
- Erreurs réseau : alertes en `app-ds-alert` mais variantes invalid sur les inputs renvoient un halo rouge `rgba(244,67,54,0.12)` ([ds-text-field.component.scss](src/Front/src/app/shared/components/ds/ds-text-field/ds-text-field.component.scss#L46-L49)) → trop coloré, coupe la lecture.
- Loading state des boutons = spinner blanc, pas de skeleton inline ni de barre de progression contextuelle.

---

## 3. Direction artistique cible

### 3.1 Manifeste (7 principes)

1. **Calme visuel avant tout.** Une page = une seule zone d'attention. Le reste s'efface.
2. **Profondeur par couches, pas par glow.** 2 niveaux d'élévation max. Pas de glassmorphism systémique.
3. **Couleur = signal, pas décor.** L'accent cyan est réservé à l'action critique, jamais à du chrome.
4. **Densité maîtrisée.** Espacement sur grille 4px, lignes hautes lisibles mais resserrées (1.4–1.5).
5. **Typographie = hiérarchie première.** 6 niveaux max, contrastes par poids et taille, pas par couleur.
6. **Mouvement subtil.** 120–240ms, transform/opacity uniquement, easing standard. Aucun rebond.
7. **Surfaces honnêtes.** Pas de fausse transparence, pas de blur décoratif. Une surface = une fonction.

### 3.2 Palette enterprise premium (dark mode primary)

Choix structurant : passage à un système OKLCH/HSL-friendly avec une seule famille brand désaturée + une seule famille neutrale slate. Les hex ci-dessous sont la cible exacte.

**Brand (1 seule famille bleu désaturée, accent cyan unique)**

| Token | Hex | Rôle |
|---|---|---|
| `--ifs-brand-50` | `#eef4ff` | wash très pâle (light mode hover) |
| `--ifs-brand-100` | `#dbe5ff` | wash léger |
| `--ifs-brand-200` | `#b6c7ee` | bordure brand light |
| `--ifs-brand-400` | `#6f8fc4` | brand muted |
| `--ifs-brand-500` | `#4f74b3` | **brand primary** (désaturé volontaire vs `#1565c0`) |
| `--ifs-brand-600` | `#3a5a96` | brand hover/active |
| `--ifs-brand-700` | `#2c4778` | brand pressed |
| `--ifs-accent-500` | `#3aa3c9` | **accent cyan unique** — actions critiques (CTA principale, focus) |
| `--ifs-accent-600` | `#2a8aae` | accent hover |

Rationale : `#4f74b3` est encore lisible comme « bleu » mais perd la sursaturation `#1565c0` qui faisait « gaming/IA ». L'accent `#3aa3c9` remplace `#0288d1`/`#00bcd4` et n'est utilisé qu'à un endroit par écran.

**Neutrals — Slate dark + Slate light**

| Token | Hex (dark) | Hex (light) | Rôle |
|---|---|---|---|
| `--ifs-bg` | `#0b0d10` | `#f7f8fa` | fond app |
| `--ifs-surface-1` | `#111418` | `#ffffff` | surface card / panel |
| `--ifs-surface-2` | `#171b21` | `#fbfbfd` | surface élévée (popover, dropdown) |
| `--ifs-surface-3` | `#1d222a` | `#f1f3f7` | surface alt (zebra, header table) |
| `--ifs-border-subtle` | `#1f242c` | `#e6e8ed` | bordure standard |
| `--ifs-border-strong` | `#2a313b` | `#d3d7df` | bordure d'emphase |
| `--ifs-text-primary` | `#e7eaef` | `#0e1116` | texte principal |
| `--ifs-text-secondary` | `#a8b0bc` | `#525a66` | texte secondaire |
| `--ifs-text-muted` | `#6f7681` | `#8a93a0` | texte tertiaire/captions |
| `--ifs-text-disabled` | `#4a4f58` | `#b6bbc3` | disabled |

**Semantic states (discrets, pas de saturé pur)**

| Token | Hex | Rôle |
|---|---|---|
| `--ifs-success` | `#3fa66b` | success solid |
| `--ifs-success-bg` | `rgba(63,166,107,0.12)` | success subtle |
| `--ifs-warning` | `#d6a23a` | warning solid |
| `--ifs-warning-bg` | `rgba(214,162,58,0.12)` | warning subtle |
| `--ifs-danger` | `#d24a4a` | danger solid |
| `--ifs-danger-bg` | `rgba(210,74,74,0.12)` | danger subtle |
| `--ifs-info` | `var(--ifs-accent-500)` | info = accent |

### 3.3 Échelle typographique resserrée

**Famille** : `Inter` (variable font, supports OpenType numerals + tabular nums). Fallback `-apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif`. Justif : Inter est l'identité de fait pour Linear/Vercel/Stripe ; tabular numerals nécessaires pour les nombres dans tableaux/dashboards (ressources, IDs). Pas de Geist (lié à Vercel, marque). Pas de Segoe seul (irrégulier sur macOS).

**Mono** : `JetBrains Mono` conservé pour code/Bicep (déjà installé).

**Échelle resserrée (6 niveaux)**

| Token | Size | Line | Weight | Usage |
|---|---|---|---|---|
| `--ifs-text-caption` | 12px | 16px | 500 | captions, helper text, table labels |
| `--ifs-text-body-sm` | 13px | 18px | 400 | UI dense (tables, sidebars) |
| `--ifs-text-body` | 14px | 20px | 400 | corps standard |
| `--ifs-text-h4` | 15px | 22px | 600 | section header / card title |
| `--ifs-text-h3` | 18px | 24px | 600 | sub-title page |
| `--ifs-text-h2` | 22px | 28px | 600 | page title |
| `--ifs-text-h1` | 28px | 34px | 600 | hero (login, home) |

Aucun 700. Le poids 600 fait toute la hiérarchie haute. Letter-spacing : `-0.01em` sur h2/h1 uniquement.

### 3.4 Spacing harmonisé (grille 4px stricte)

Échelle exclusive : `2, 4, 6, 8, 12, 16, 20, 24, 32, 40, 48, 64`. Tokens en px (pas rem) pour grille pixel-perfect.

| Token | Value |
|---|---|
| `--ifs-space-2` | 2px |
| `--ifs-space-4` | 4px |
| `--ifs-space-6` | 6px |
| `--ifs-space-8` | 8px |
| `--ifs-space-12` | 12px |
| `--ifs-space-16` | 16px |
| `--ifs-space-20` | 20px |
| `--ifs-space-24` | 24px |
| `--ifs-space-32` | 32px |
| `--ifs-space-40` | 40px |
| `--ifs-space-48` | 48px |
| `--ifs-space-64` | 64px |

Tout autre valeur (`0.85rem`, `0.35rem`, `1.25rem`, etc.) doit être supprimée du codebase au fur et à mesure des vagues.

### 3.5 Radius cohérents (3 valeurs)

| Token | Value | Usage |
|---|---|---|
| `--ifs-radius-sm` | 4px | inputs, chips, tags |
| `--ifs-radius-md` | 8px | boutons, cards, panels |
| `--ifs-radius-lg` | 12px | dialogs, hero panels, login card |
| `--ifs-radius-pill` | 999px | **uniquement** chips status, badges, segmented |

Suppression de `xl`, `2xl`, `3xl` (`1.3rem`, `1.4rem`, `1.6rem`).

### 3.6 Profondeur (3 niveaux max, ombres neutres)

| Token | Value | Usage |
|---|---|---|
| `--ifs-shadow-sm` | `0 1px 2px rgba(0,0,0,0.06), 0 1px 1px rgba(0,0,0,0.04)` | cards par défaut |
| `--ifs-shadow-md` | `0 4px 12px rgba(0,0,0,0.10), 0 2px 4px rgba(0,0,0,0.06)` | popovers, dropdowns |
| `--ifs-shadow-lg` | `0 16px 32px rgba(0,0,0,0.18), 0 4px 8px rgba(0,0,0,0.08)` | dialogs, command palette |

Suppression de `cta`, `cta-hover`, `nav`, `hero`, `xs`, `xl`. Plus aucune ombre teintée bleu.

### 3.7 Motion (durées + easings)

| Token | Value | Usage |
|---|---|---|
| `--ifs-duration-fast` | 120ms | hover, focus ring |
| `--ifs-duration-base` | 180ms | open/close panels, transitions cards |
| `--ifs-duration-slow` | 240ms | dialogs, route transitions |
| `--ifs-ease-standard` | `cubic-bezier(0.2, 0, 0, 1)` | défaut |
| `--ifs-ease-emphasized` | `cubic-bezier(0.3, 0, 0, 1)` | dialogs |

Règle : `transition` cible **uniquement** `transform`, `opacity`, `background-color`, `border-color`, `box-shadow`. Jamais `all`. Jamais `translateY` au hover sur card.

### 3.8 Iconographie

Conserver `Material Icons` (déjà chargé, dépendance Material). Standardiser **3 tailles uniquement** :
- `--ifs-icon-sm` : 16px (boutons compacts, chips, inline)
- `--ifs-icon-md` : 20px (boutons standards, nav, headers)
- `--ifs-icon-lg` : 24px (dialogs, empty state, hero)

Couleur d'icône = couleur du texte adjacent par défaut (`color: currentColor`), sauf icônes status qui héritent du token semantic.

---

## 4. Inventaire tokens à refondre (AVANT → APRÈS)

| Token actuel | Statut | Token cible / valeur cible | Notes |
|---|---|---|---|
| `$ifs-brand-dark-blue #0d2f66` | DELETE | — | absorbé dans `--ifs-brand-700` |
| `$ifs-brand-blue #1565c0` | CHANGE | `--ifs-brand-500 #4f74b3` | désaturation forte |
| `$ifs-brand-blue-deep #1a3a8a` | DELETE | — | redondant avec brand-700 |
| `$ifs-brand-cyan #0288d1` | CHANGE | `--ifs-accent-500 #3aa3c9` | cyan unique, usage restreint |
| `$ifs-brand-cyan-light #00bcd4` | DELETE | — | trop saturé |
| `$ifs-brand-cyan-deep #00acc1` | DELETE | — | redondant |
| `$ifs-brand-teal #009fbd` | DELETE | — | hors palette |
| `$ifs-ink-900..100` (8 valeurs) | CHANGE | `--ifs-text-*` (4 valeurs) + `--ifs-border-*` (2 valeurs) | mapper vers slate dark+light |
| `$ifs-surface-0..300, tinted` | CHANGE | `--ifs-surface-1..3` | 3 niveaux suffisent |
| `$ifs-success #2e7d32` | CHANGE | `--ifs-success #3fa66b` | moins sombre, plus clair sur dark |
| `$ifs-success-soft/border` | CHANGE | `--ifs-success-bg` | un seul bg subtle |
| `$ifs-error*` | CHANGE | `--ifs-danger*` | renommer en danger (convention SaaS) |
| `$ifs-warning*` | CHANGE | `--ifs-warning*` | nouvelle teinte ambre |
| `$ifs-info*` | KEEP-RENAME | `--ifs-info` = `--ifs-accent-500` | info = accent unique |
| `$ifs-gradient-brand` | DELETE | — | aucun usage en V2 |
| `$ifs-gradient-brand-soft` | DELETE | — | idem |
| `$ifs-gradient-login` | KEEP-CHANGE | `--ifs-gradient-login` | conservé, désaturé : `linear-gradient(160deg, #0e1a35 0%, #1a2b52 50%, #233e6e 100%)` |
| `$ifs-gradient-nav` | DELETE | — | nav devient surface plate |
| `$ifs-gradient-cta` | KEEP-CHANGE | `--ifs-gradient-cta` | uniquement bouton primary : `linear-gradient(180deg, #4f74b3 0%, #3a5a96 100%)` |
| `$ifs-gradient-card-soft` | DELETE | — | cards plates |
| `$ifs-gradient-app-bg` | DELETE | — | fond app = `--ifs-bg` plat |
| `$ifs-gradient-glass-footer` | DELETE | — | footer minimal sans glass |
| `$ifs-border-soft/medium/strong` | CHANGE | `--ifs-border-subtle` + `--ifs-border-strong` (2 valeurs) | suppression du tint bleu |
| `$ifs-radius-sm` (6px) | CHANGE | `--ifs-radius-sm` 4px | plus carré |
| `$ifs-radius-md` (12px) | CHANGE | `--ifs-radius-md` 8px | plus carré |
| `$ifs-radius-lg` (16px) | CHANGE | `--ifs-radius-lg` 12px | plus carré |
| `$ifs-radius-xl/2xl/3xl` | DELETE | — | ramené à 3 valeurs |
| `$ifs-radius-pill` | KEEP | `--ifs-radius-pill` 999px | chips, badges uniquement |
| `$ifs-shadow-xs/sm/md/lg/xl` | CHANGE | `--ifs-shadow-sm/md/lg` | 3 ombres neutres |
| `$ifs-shadow-cta/cta-hover/hero/nav` | DELETE | — | aucune ombre teintée |
| `$ifs-space-0..16` (rem) | CHANGE | `--ifs-space-2..64` (px) | grille 4px stricte, px |
| `$ifs-z-*` | KEEP | `--ifs-z-*` | identique |
| `$ifs-ease-out/in-out` | CHANGE | `--ifs-ease-standard/emphasized` | nouvelle convention |
| `$ifs-duration-fast/base/slow` (0.15/0.25/0.4) | CHANGE | 120/180/240 ms | durées plus courtes |
| `$ifs-focus-ring` | CHANGE | `0 0 0 2px var(--ifs-bg), 0 0 0 4px var(--ifs-accent-500)` | ring 2px accent + offset bg, plus pro |
| `$ifs-on-brand-*` (4) | DELETE | — | inutile : sur brand on a `--ifs-text-on-brand` simple |
| `$ifs-on-dark-*` (3) | DELETE | — | idem |
| — | NEW | `--ifs-text-on-brand` `#ffffff` | texte sur surfaces brand pleines |
| — | NEW | `--ifs-icon-sm/md/lg` | 16/20/24 |
| — | NEW | `--ifs-density-row-sm/md/lg` | 28/32/36 px (hauteurs de ligne table/list) |

---

## 5. Composants DS à refondre / créer

### 5.1 Existants (refonte)

| Composant | État actuel | Direction cible | Priorité | Vague |
|---|---|---|---|---|
| `app-ds-button` | Primary trop chargé (gradient + ombre colorée + inset). Variants secondary/ghost/danger/success. | Primary plat slate (`--ifs-brand-500`) avec ombre `shadow-sm` uniquement. Subtle gradient interne 4% pour profondeur. Pas de translate au hover, juste `background-color`. Variants : `primary`, `secondary` (border + transparent), `ghost`, `danger` (texte rouge + bg subtle), `subtle`. Loading = spinner monochrome. Sizes : `sm 28px`, `md 32px`, `lg 36px`. | P0 | 2 |
| `app-ds-card` | 3 paddings + 4 accents border-left + variant glass + variant elevated + hover-lift. | 1 surface plate sur `--ifs-surface-1`, border `--ifs-border-subtle`. Variants : `default`, `interactive` (hover `surface-2`), `outlined` (sans shadow). Suppression de `glass`, `elevated`, accents border-left, hover-lift. Padding tokens `12/16/20/24`. | P0 | 2 |
| `app-ds-text-field` | Border 1.5px, label brand-blue, halo focus 3px brand. | Border 1px `--ifs-border-strong`, label `--ifs-text-secondary` 12px. Focus = border `--ifs-accent-500` + ring 2px `accent/30%`. Hauteur 32px (md). Pas d'inset shadow. | P0 | 2 |
| `app-ds-textarea` | Cohérent avec text-field. | Aligner sur text-field. Hauteur min 64px, resize vertical. | P0 | 2 |
| `app-ds-select` | CdkConnectedOverlay déjà ok. | Mêmes règles que text-field. Panel dropdown surface-2 + shadow-md. Item hover surface-3. Item selected = check icon `accent-500` + texte `text-primary`. | P0 | 2 |
| `app-ds-chip` | Pill brand. | Pill `surface-3` + texte `text-secondary` par défaut. Variants `neutral`, `success`, `warning`, `danger`, `accent`. Taille 22px (sm) / 26px (md). Pas de shadow. | P0 | 2 |
| `app-ds-alert` | Card colorée + icône. | Aligner sur surfaces semantic-bg + border 1px semantic. Icon size 16px. Padding 12/16. Pas de shadow. | P1 | 2 |
| `app-ds-icon-button` | Carré 36px brand. | Carré 28px (sm) / 32px (md). Hover `surface-3`. Variants `neutral`, `accent`, `danger`. | P1 | 2 |
| `app-ds-toggle` | Material override interne. | Pill 32×18 avec knob 14px. Off `surface-3` + border. On `accent-500`. Animation 120ms. | P1 | 2 |
| `app-ds-checkbox` | Custom. | Carré 16px, radius 4px. Off border `--ifs-border-strong`. On `accent-500` + check blanc. | P1 | 2 |
| `app-ds-radio-group` | Custom. | Cercle 16px. On dot `accent-500`. | P1 | 2 |
| `app-ds-section-header` | Texte + actions à droite. | h4 (15px/600) + subtitle 13px. Border-bottom optionnelle. Pas de gradient, pas d'accent left. | P1 | 3 |
| `app-ds-page-header` | Hero brand. | Titre h2 + breadcrumbs + actions. Aucune ombre, fond `--ifs-bg`. Hauteur 56px contenu + padding 24/32. | P1 | 3 |
| `app-ds-panel-action-button` | Glassy soft-square. | Devenir variante `subtle` du `ds-button` size `sm` icon-only. À supprimer comme composant distinct, migrer les call-sites. | P1 | 3 |
| `app-ds-option-card` | Card cliquable. | `ds-card interactive` + radio interne. Suppression possible si pas d'usage hors resource-edit ; sinon refonte cohérente. | P2 | 4 |
| `app-ds-date-picker` | CDK overlay custom, gradient header. | Header surface-2 + texte primary. Selected day = pastille `accent-500`. Today = ring 1px `accent-500`. Suppression du gradient header. | P2 | 5 |

### 5.2 Nouveaux primitives à créer

| Composant | Pourquoi maintenant | Vague |
|---|---|---|
| `app-ds-tabs` | `project-detail` et `config-detail` ré-implémentent leurs tabs en SCSS local (cf. 50KB+ scss chacun). Primitive obligatoire. | 3 |
| `app-ds-segmented-control` | Pour switcher de mode (split/all-in-one), aujourd'hui ad hoc. | 3 |
| `app-ds-table` | Aucune table tokenisée ; `_tables.scss` propose un mixin mais pas de primitive Angular. Doit gérer header sticky, zebra, dense/cozy, sort, empty. | 4 |
| `app-ds-tree-view` | `project-detail` a un arbre maison ; doit devenir primitive (expand/collapse, indent, focus management). | 4 |
| `app-ds-empty-state` | Aucune cohérence empty actuellement. Primitive icon + title + description + CTA. | 6 |
| `app-ds-tooltip` | Aujourd'hui Material. Primitive surface-3 + texte 12px + delay 350ms. | 6 |
| `app-ds-skeleton` | Pas de shimmer/skeleton. Primitive box+lines+circle, animation `--ifs-duration-slow`. | 6 |
| `app-ds-banner` | Pour annonces top-page (preview, maintenance). Surface-3 + border-bottom + dismiss. | 6 |
| `app-ds-status-dot` | Petit point coloré 8px pour status (success/warning/danger/idle), à utiliser dans tables, sidebars. | 6 |

---

## 6. Plan d'exécution phasé (6 vagues mergeables)

> Chaque vague est mergeable indépendamment. Aucune vague ne doit casser la précédente.
> TDD obligatoire : pour les composants Angular (vagues 2+), tests Karma/Jasmine sur chaque variant et état (hover, focus, disabled, error, loading).
> Pour la vague 1 (tokens uniquement), un snapshot Playwright des écrans clés (login, home, projects, project-detail, config-detail, resource-edit) avant/après valide la non-régression structurelle.

---

### Vague 1 — Tokens & fondations

**Objectif** : substituer la base SCSS (tokens, typo, mixins, tailwind, override Material) par la nouvelle direction artistique sans modifier un seul template HTML.

**Fichiers touchés**
- `src/Front/src/scss/_tokens.scss` (réécriture intégrale, voir annexe §8)
- `src/Front/src/scss/_typography.scss` (nouvelle échelle 6 niveaux + Inter)
- `src/Front/src/scss/_mixins.scss` (suppression `ifs-glass`, `ifs-hover-lift` ; refonte `ifs-card-surface`, `ifs-focus-visible`)
- `src/Front/src/scss/_main.scss` (suppression du forward `colors` qui est vide)
- `src/Front/src/scss/_colors.scss` (DELETE)
- `src/Front/src/scss/_animations.scss` (audit + standardisation des durées)
- `src/Front/src/scss/_tables.scss` (alignement avec nouveaux tokens density)
- `src/Front/tailwind.config.js` (miroir nouveaux tokens, suppression backgrounds gradients)
- `src/Front/src/styles.scss` (override Material : couleurs accent → `--ifs-accent-500`, radius form-field 8px, tabs slate, dialog surface-2, snackbar surface-2)
- `src/Front/src/app/app.component.scss` (suppression des dégradés app-main et de l'annotation-trigger flashy → utilise `var(--ifs-bg)` et accent neutre)
- `src/Front/index.html` (ajout import Inter via `<link rel="preconnect">` + `<link rel="stylesheet" href="https://rsms.me/inter/inter.css">` ou self-hosted)

**Tests à écrire**
- Aucun test unitaire (pas de logique). Snapshots Playwright des 7 écrans clés (login, home, projects, project-detail vide, project-detail rempli, config-detail, resource-edit) en mode dark. Critère : pas d'élément cassé/débordement, focus visible visible, pas de ratio contraste WCAG sous 4.5:1.

**Critère de done**
- `npm run typecheck` + `npm run build` passent.
- Aucune référence aux anciens tokens (`$ifs-brand-cyan`, `$ifs-gradient-brand`, `$ifs-shadow-cta`, etc.) dans `src/Front/src` (vérification grep CI). → Si des composants en utilisent encore, fournir un mapping ad hoc temporaire dans `_tokens.scss` qui ré-exporte ces noms vers les nouveaux (compat layer documenté à supprimer en vague 6).
- Lighthouse contrast >= AA partout.

**Risques**
- Cassure visuelle massive sur les écrans denses (project-detail, config-detail) car ils dépendent de tokens individuels. → Atténué par le compat layer ré-exportant les anciens noms.
- Inter via CDN : prévoir le self-host pour environnements air-gapped (sera traité en vague 6).
- Material override : certains composants (datepicker non-DS, autocomplete) peuvent perdre leur radius. → Snapshot ciblé.

---

### Vague 2 — Primitives DS critiques

**Objectif** : refondre les 7 primitives quotidiennes (`button`, `card`, `text-field`, `textarea`, `select`, `chip`, `alert`) pour incarner la nouvelle DA.

**Fichiers touchés**
- `src/Front/src/app/shared/components/ds/ds-button/ds-button.component.{html,scss,ts}`
- `src/Front/src/app/shared/components/ds/ds-card/ds-card.component.{html,scss,ts}`
- `src/Front/src/app/shared/components/ds/ds-text-field/ds-text-field.component.{html,scss,ts}`
- `src/Front/src/app/shared/components/ds/ds-textarea/ds-textarea.component.{html,scss,ts}`
- `src/Front/src/app/shared/components/ds/ds-select/ds-select.component.{html,scss,ts}`
- `src/Front/src/app/shared/components/ds/ds-chip/ds-chip.component.{html,scss,ts}`
- `src/Front/src/app/shared/components/ds/ds-alert/ds-alert.component.{html,scss,ts}`
- `src/Front/src/app/shared/components/ds/ds-icon-button/ds-icon-button.component.{html,scss,ts}`
- `src/Front/src/app/shared/components/ds/ds-toggle/ds-toggle.component.{html,scss,ts}`
- `src/Front/src/app/shared/components/ds/ds-checkbox/ds-checkbox.component.{html,scss,ts}`
- `src/Front/src/app/shared/components/ds/ds-radio-group/ds-radio-group.component.{html,scss,ts}`
- (Spec) ajout/maj de `*.component.spec.ts` pour chaque composant ci-dessus.

**Tests à écrire (avant code, RED → GREEN)**
- `ds-button` : variants × sizes, états disabled/loading, click handler, ariaLabel, full-width, slot icon, `type="submit"` propagation.
- `ds-card` : variants `default/interactive/outlined`, paddings, slots header/body/footer, click event si interactive.
- `ds-text-field` : CVA write/read, placeholder, error message, hint, prefix/suffix icon, clear button, type="date" (existant), `disabled`, `readonly`.
- `ds-select` : sélection via clavier, fermeture sur escape/outside, panel width = trigger width, single/multi (si multi déjà supporté), CVA.
- `ds-chip` : variants neutral/success/warning/danger/accent, sizes sm/md, removable.
- `ds-alert` : variants info/success/warning/danger, slot title/description/actions, dismissible.
- `ds-toggle/checkbox/radio` : CVA, ariaLabel, focus visible, états disabled, événement change.

**Critère de done**
- 100% specs vertes (`npm test`).
- Aucun composant n'utilise un dégradé brand ou un blur. (vérification grep par composant)
- Hauteur boutons cohérente : `sm 28 / md 32 / lg 36`. Hauteur inputs : `md 32`. Alignement vertical pixel-perfect.
- Storybook ou page demo `/dev/ds-showcase` (si existante) à jour.

**Risques**
- Régressions sur formulaires resource-edit qui dépendent du look précédent → snapshots Playwright sur resource-edit avant/après.
- API publique : ne **rien** casser côté inputs/outputs. Si renommage nécessaire, garder alias deprecated.

---

### Vague 3 — Layout & navigation (shell)

**Objectif** : refondre le shell applicatif (top-bar resserrée, sidebar enterprise sobre, footer minimal status-bar) et créer les primitives `ds-tabs` + `ds-segmented-control`.

**Fichiers touchés**
- `src/Front/src/app/app.component.{html,scss,ts}` (passage à un layout `[sidebar][topbar+content]`)
- `src/Front/src/app/core/layouts/navigation/navigation.component.{html,scss,ts}` (top-bar 48px, suppression gradient nav et blur, language switch et user menu en `ds-icon-button`)
- `src/Front/src/app/core/layouts/footer/footer.component.{html,scss,ts}` (réduction à status-bar 28px : version + env + lien docs)
- `src/Front/src/app/core/layouts/sidebar/sidebar.component.{html,scss,ts}` (NOUVEAU, 240px collapsable à 56px, items projects/settings/docs)
- `src/Front/src/app/shared/components/ds/ds-tabs/` (NOUVEAU)
- `src/Front/src/app/shared/components/ds/ds-segmented-control/` (NOUVEAU)
- `src/Front/src/app/shared/components/ds/ds-page-header/ds-page-header.component.{html,scss}` (refonte)
- `src/Front/src/app/shared/components/ds/ds-section-header/ds-section-header.component.{html,scss}` (refonte)
- Routes : aucun changement.

**Tests à écrire**
- `ds-tabs` : navigation clavier (←/→/Home/End), `aria-selected`, lazy mount, query param sync (option), active indicator.
- `ds-segmented-control` : sélection, CVA, disabled item.
- `sidebar` : collapse persistant (localStorage), focus trap menu, active route highlight.
- `navigation` : language switch fonctionnel, menu user menu visible, no-gradient assertions visuelles.
- `footer` : version + env affichés depuis env injection.

**Critère de done**
- Top-bar = 48px exactement. Sidebar = 240px expanded / 56px collapsed.
- Aucune référence à `$ifs-gradient-nav`, `$ifs-gradient-glass-footer` dans le code.
- A11y : navigation clavier complète shell sans souris.
- Snapshot Playwright shell vide.

**Risques**
- Réorganisation du DOM root → impact tests E2E existants. Coordonner avec QA.
- Sidebar nouvelle : décider très tôt si on garde la nav top hybride (Linear-style) ou pas. Décision proposée : sidebar permanente + top-bar mince contextuelle (breadcrumbs + actions de page).

---

### Vague 4 — Pages denses (project-detail + config-detail)

**Objectif** : appliquer la nouvelle DA aux deux écrans de travail principaux, tout en remplaçant les tabs/tree/table maison par les primitives DS.

**Fichiers touchés**
- `src/Front/src/app/features/project-detail/project-detail.component.{html,scss,ts}`
- `src/Front/src/app/features/project-detail/project-detail-tree.helpers.ts` (adaptation au `ds-tree-view`)
- `src/Front/src/app/features/config-detail/config-detail.component.{html,scss,ts}`
- `src/Front/src/app/shared/components/ds/ds-table/` (NOUVEAU)
- `src/Front/src/app/shared/components/ds/ds-tree-view/` (NOUVEAU)
- Composants de génération embarqués (split-generation-switcher, bicep-file-panel) : ré-alignement sans changement fonctionnel.

**Tests à écrire**
- `ds-table` : sort, row hover/selected, sticky header, density, empty slot.
- `ds-tree-view` : expand/collapse clavier (←/→), focus, selection, drag (si déjà existant), aria-tree.
- `project-detail` : helpers `tree-ordering` et `generation-visibility` déjà testés, garder verts. Ajouter test composant pour : ouverture tab par défaut, breadcrumb, action push-to-git visible selon state.
- `config-detail` : tab par défaut, sort des resource-types, action add-resource visible selon role.

**Critère de done**
- `project-detail.component.scss` réduit de 50KB à <15KB (la majorité du SCSS local devient inutile car porté par les primitives).
- `config-detail.component.scss` réduit de 62KB à <15KB.
- Aucun gradient local, aucun blur local, aucune ombre teintée locale.
- Tabs et tree fonctionnent à clavier.
- Hauteur ligne table = `--ifs-density-row-md` (32px).

**Risques**
- Refactor important des templates HTML (54KB et 84KB respectivement). À découper en 2-3 PRs intermédiaires si trop gros.
- Bicep file panel a déjà été retravaillé en mai (workspace/editor surface). Préserver cette direction.

---

### Vague 5 — Resource-edit & formulaires denses

**Objectif** : refondre resource-edit (178KB HTML) et les dialogs de création (add-resource, push-to-git, add-project-member) pour rythme/alignement/validation cohérents avec la nouvelle DA.

**Fichiers touchés**
- `src/Front/src/app/features/resource-edit/resource-edit.component.{html,scss,ts}`
- `src/Front/src/app/features/projects/dialogs/*` (tous les dialogs)
- `src/Front/src/app/features/project-detail/dialogs/*` (idem)
- `src/Front/src/app/features/config-detail/dialogs/*` (idem)
- `src/Front/src/app/shared/components/ds/ds-option-card/ds-option-card.component.{html,scss}` (refonte)
- Formulaires : passage strict aux primitives DS, suppression des `gradient`/`backdrop-filter` locaux.

**Tests à écrire**
- Resource-edit : test composant minimum sur la nav inter-sections + état dirty/save + validation requise.
- Dialogs : tests existants à conserver, ajout test snapshot d'ouverture/fermeture.
- Validation : message d'erreur affiché sous champ avec `--ifs-text-caption` + `--ifs-danger`.

**Critère de done**
- Resource-edit n'utilise plus aucun `linear-gradient` ni `backdrop-filter`.
- Hauteur des inputs uniforme dans toute la page.
- Form grid 2 colonnes 16px gutter / 12px row-gap. Section header sticky.
- Mat-autocomplete restant (push-to-git, add-project-member) restylé via `styles.scss` pour matcher `ds-text-field`.

**Risques**
- Resource-edit a 159KB de TS et 178KB de HTML. Risque de régression fonctionnelle (validation, save, dynamic forms). À couvrir par une passe E2E avant merge.

---

### Vague 6 — Polish & micro-détails

**Objectif** : empty states, skeletons, tooltips, focus-visible cross-app, animations subtiles, audit a11y, suppression du compat layer tokens.

**Fichiers touchés**
- `src/Front/src/app/shared/components/ds/ds-empty-state/` (NOUVEAU)
- `src/Front/src/app/shared/components/ds/ds-skeleton/` (NOUVEAU)
- `src/Front/src/app/shared/components/ds/ds-tooltip/` (NOUVEAU)
- `src/Front/src/app/shared/components/ds/ds-banner/` (NOUVEAU)
- `src/Front/src/app/shared/components/ds/ds-status-dot/` (NOUVEAU)
- Application des skeletons sur les routes async : `home`, `projects`, `project-detail`, `config-detail`, `resource-edit`.
- Application des empty states sur listes vides : projects, resources, members.
- `src/Front/src/scss/_tokens.scss` : suppression du compat layer (anciens noms).
- Self-host Inter (assets/fonts/inter/) + suppression du CDN.
- Audit a11y final : focus-visible cross-app, contrastes, aria-labels manquants.

**Tests à écrire**
- `ds-empty-state` : slot icon/title/description/cta.
- `ds-skeleton` : variantes box/line/circle, props width/height.
- `ds-tooltip` : delay show, hide on blur, keyboard trigger.
- `ds-banner` : dismiss, variants info/success/warning/danger.
- `ds-status-dot` : variants + ariaLabel.

**Critère de done**
- Zéro reference aux anciens tokens (suppression du compat layer effective).
- Lighthouse a11y >= 95 sur toutes les routes principales.
- 100% des routes async ont un skeleton.
- 100% des listes ont un empty state.
- Self-host Inter : pas d'appel réseau externe au boot.

**Risques**
- Suppression du compat layer : tout consommateur restant casse. Faire grep + correction dans la même PR.

---

## 7. Risques, contraintes, anti-patterns à interdire

### 7.1 Anti-patterns proscrits (CI doit échouer)

- `linear-gradient` ou `radial-gradient` hors de `_tokens.scss` (sauf 2 exceptions whitelistées : `--ifs-gradient-login`, `--ifs-gradient-cta`).
- `backdrop-filter` ou `-webkit-backdrop-filter` n'importe où en V2.
- Box-shadow teintée brand (`rgba(13,47,102, …)`, `rgba(21,101,192, …)`, etc.). Toutes les ombres doivent être `rgba(0,0,0, …)`.
- `transition: all`. Toujours énumérer les propriétés.
- `transform: translateY(-Npx)` au hover sur card/button.
- Border-radius > 12px hors chips/badges/pill.
- Couleurs hex inline dans les composants (sauf `_tokens.scss`). Toujours via `var(--ifs-…)`.
- Multi-shadow décoratif (>2 shadows par règle).
- Polices custom additionnelles (Inter + JetBrains Mono uniquement).
- Animations bounce/spring/pop. Easings standard et emphasized uniquement.
- Plus d'un composant DS public par fichier (pitfall #14).
- Magic strings pour variants de composants → enums TS.

### 7.2 Risques techniques

- **Material override** : la migration des tokens Material (form-field, tabs, dialogs) doit se faire via `--mat-…` CSS vars. Vérifier que la version Material 21 expose bien tous ces tokens (cf. [styles.scss](src/Front/src/styles.scss#L70-L86)). Sinon, sass `@use mat.theme` à reconfigurer.
- **Breaking changes Tailwind** : suppression des couleurs `ifs-blue-deep`, `ifs-cyan-light`, etc. → tout consommateur Tailwind dans les templates doit être migré ou aliasé. Faire un grep avant la vague 1.
- **Contraste WCAG** : `--ifs-text-secondary` sur `--ifs-bg` doit être >= 4.5:1. Vérifier `#a8b0bc` sur `#0b0d10` = 11.8:1 ✓ ; `#525a66` sur `#f7f8fa` = 7.2:1 ✓.
- **Dark mode toggle** : si on garde uniquement dark, retirer la machinerie light/dark Material. Si on garde les deux, faire deux blocs `:root` + `[data-theme="light"]`.
- **Inter via CDN** : risque de FOUT (Flash Of Unstyled Text). Préférer `font-display: swap` + self-host en vague 6.

### 7.3 Stratégie de non-régression

- **Snapshots Playwright** : capture full-page en mode dark des 7 routes clés avant chaque vague, comparaison après. Seuil tolérance pixel : 0.1%.
- **Smoke screens** scénarisés : login → projects → create project (wizard) → project-detail → add resource → resource-edit → push to git. À jouer après chaque vague.
- **Compat layer tokens** maintenu jusqu'à la vague 6 pour éviter les explosions transverses. Documenté `// DEPRECATED — to remove in wave 6`.
- **A11y** : axe-core run automatique sur chaque route après chaque vague.
- **CI grep guards** : règles ESLint/StyleLint custom (ou simple grep step) pour bloquer les anti-patterns listés en 7.1.

---

## 8. Annexe — Patch tokens prêt à appliquer (vague 1)

> Bloc complet à insérer en remplacement intégral de `src/Front/src/scss/_tokens.scss`.
> Ne pas appliquer sans relire la section 4. Les anciens noms exposés en bas du fichier sont le **compat layer temporaire** prévu par la vague 1 ; à supprimer en vague 6.

```scss
// ============================================================================
// InfraFlowSculptor — Design Tokens V2 (UI Refresh 2026-05)
// Source of truth: docs/design/ui-refresh-2026-05.md
// Dark mode is primary. Light mode tokens overridden under [data-theme="light"].
// ============================================================================

// ----------------------------------------------------------------------------
// CSS Custom Properties (consommées partout via var(--ifs-…))
// ----------------------------------------------------------------------------
:root {
  // --- Brand (one desaturated blue family) ---
  --ifs-brand-50:  #eef4ff;
  --ifs-brand-100: #dbe5ff;
  --ifs-brand-200: #b6c7ee;
  --ifs-brand-400: #6f8fc4;
  --ifs-brand-500: #4f74b3;
  --ifs-brand-600: #3a5a96;
  --ifs-brand-700: #2c4778;

  // --- Single accent (cyan, used sparingly: primary CTA, focus, info) ---
  --ifs-accent-500: #3aa3c9;
  --ifs-accent-600: #2a8aae;

  // --- Surfaces & text (DARK by default) ---
  --ifs-bg:              #0b0d10;
  --ifs-surface-1:       #111418;
  --ifs-surface-2:       #171b21;
  --ifs-surface-3:       #1d222a;
  --ifs-border-subtle:   #1f242c;
  --ifs-border-strong:   #2a313b;
  --ifs-text-primary:    #e7eaef;
  --ifs-text-secondary:  #a8b0bc;
  --ifs-text-muted:      #6f7681;
  --ifs-text-disabled:   #4a4f58;
  --ifs-text-on-brand:   #ffffff;

  // --- Semantic states (discreet) ---
  --ifs-success:    #3fa66b;
  --ifs-success-bg: rgba(63, 166, 107, 0.12);
  --ifs-warning:    #d6a23a;
  --ifs-warning-bg: rgba(214, 162, 58, 0.12);
  --ifs-danger:     #d24a4a;
  --ifs-danger-bg:  rgba(210, 74, 74, 0.12);
  --ifs-info:       var(--ifs-accent-500);
  --ifs-info-bg:    rgba(58, 163, 201, 0.12);

  // --- Gradients (max 2, role-restricted) ---
  --ifs-gradient-login: linear-gradient(160deg, #0e1a35 0%, #1a2b52 50%, #233e6e 100%);
  --ifs-gradient-cta:   linear-gradient(180deg, #4f74b3 0%, #3a5a96 100%);

  // --- Radius (3 values + pill) ---
  --ifs-radius-sm:   4px;
  --ifs-radius-md:   8px;
  --ifs-radius-lg:   12px;
  --ifs-radius-pill: 999px;

  // --- Shadows (3 values, neutral, no brand tint) ---
  --ifs-shadow-sm: 0 1px 2px rgba(0, 0, 0, 0.06), 0 1px 1px rgba(0, 0, 0, 0.04);
  --ifs-shadow-md: 0 4px 12px rgba(0, 0, 0, 0.10), 0 2px 4px rgba(0, 0, 0, 0.06);
  --ifs-shadow-lg: 0 16px 32px rgba(0, 0, 0, 0.18), 0 4px 8px rgba(0, 0, 0, 0.08);

  // --- Spacing (4px grid, px units) ---
  --ifs-space-2:  2px;
  --ifs-space-4:  4px;
  --ifs-space-6:  6px;
  --ifs-space-8:  8px;
  --ifs-space-12: 12px;
  --ifs-space-16: 16px;
  --ifs-space-20: 20px;
  --ifs-space-24: 24px;
  --ifs-space-32: 32px;
  --ifs-space-40: 40px;
  --ifs-space-48: 48px;
  --ifs-space-64: 64px;

  // --- Density (row heights for tables / lists) ---
  --ifs-density-row-sm: 28px;
  --ifs-density-row-md: 32px;
  --ifs-density-row-lg: 36px;

  // --- Icon sizes ---
  --ifs-icon-sm: 16px;
  --ifs-icon-md: 20px;
  --ifs-icon-lg: 24px;

  // --- Motion ---
  --ifs-duration-fast: 120ms;
  --ifs-duration-base: 180ms;
  --ifs-duration-slow: 240ms;
  --ifs-ease-standard:    cubic-bezier(0.2, 0, 0, 1);
  --ifs-ease-emphasized:  cubic-bezier(0.3, 0, 0, 1);

  // --- Focus ring (a11y) ---
  --ifs-focus-ring:
    0 0 0 2px var(--ifs-bg),
    0 0 0 4px var(--ifs-accent-500);

  // --- Z-index ---
  --ifs-z-base:    1;
  --ifs-z-sticky:  100;
  --ifs-z-overlay: 1000;
  --ifs-z-modal:   1100;
  --ifs-z-popover: 1200;
  --ifs-z-tooltip: 1300;
}

// ----------------------------------------------------------------------------
// Light mode (opt-in via [data-theme="light"] on <html> or <body>)
// ----------------------------------------------------------------------------
[data-theme="light"] {
  --ifs-bg:              #f7f8fa;
  --ifs-surface-1:       #ffffff;
  --ifs-surface-2:       #fbfbfd;
  --ifs-surface-3:       #f1f3f7;
  --ifs-border-subtle:   #e6e8ed;
  --ifs-border-strong:   #d3d7df;
  --ifs-text-primary:    #0e1116;
  --ifs-text-secondary:  #525a66;
  --ifs-text-muted:      #8a93a0;
  --ifs-text-disabled:   #b6bbc3;

  --ifs-shadow-sm: 0 1px 2px rgba(13, 17, 22, 0.06), 0 1px 1px rgba(13, 17, 22, 0.04);
  --ifs-shadow-md: 0 4px 12px rgba(13, 17, 22, 0.08), 0 2px 4px rgba(13, 17, 22, 0.05);
  --ifs-shadow-lg: 0 16px 32px rgba(13, 17, 22, 0.12), 0 4px 8px rgba(13, 17, 22, 0.06);
}

// ----------------------------------------------------------------------------
// SCSS aliases (only for files still using SCSS variables — refactor into vars)
// ----------------------------------------------------------------------------
$ifs-brand-500: var(--ifs-brand-500);
$ifs-brand-600: var(--ifs-brand-600);
$ifs-brand-700: var(--ifs-brand-700);
$ifs-accent-500: var(--ifs-accent-500);
$ifs-accent-600: var(--ifs-accent-600);

$ifs-bg:             var(--ifs-bg);
$ifs-surface-1:      var(--ifs-surface-1);
$ifs-surface-2:      var(--ifs-surface-2);
$ifs-surface-3:      var(--ifs-surface-3);
$ifs-border-subtle:  var(--ifs-border-subtle);
$ifs-border-strong:  var(--ifs-border-strong);
$ifs-text-primary:   var(--ifs-text-primary);
$ifs-text-secondary: var(--ifs-text-secondary);
$ifs-text-muted:     var(--ifs-text-muted);

$ifs-success:    var(--ifs-success);
$ifs-warning:    var(--ifs-warning);
$ifs-danger:     var(--ifs-danger);
$ifs-info:       var(--ifs-info);
$ifs-success-bg: var(--ifs-success-bg);
$ifs-warning-bg: var(--ifs-warning-bg);
$ifs-danger-bg:  var(--ifs-danger-bg);

$ifs-radius-sm:   var(--ifs-radius-sm);
$ifs-radius-md:   var(--ifs-radius-md);
$ifs-radius-lg:   var(--ifs-radius-lg);
$ifs-radius-pill: var(--ifs-radius-pill);

$ifs-shadow-sm: var(--ifs-shadow-sm);
$ifs-shadow-md: var(--ifs-shadow-md);
$ifs-shadow-lg: var(--ifs-shadow-lg);

$ifs-duration-fast:    var(--ifs-duration-fast);
$ifs-duration-base:    var(--ifs-duration-base);
$ifs-duration-slow:    var(--ifs-duration-slow);
$ifs-ease-standard:    var(--ifs-ease-standard);
$ifs-ease-emphasized:  var(--ifs-ease-emphasized);

$ifs-focus-ring: var(--ifs-focus-ring);

// ----------------------------------------------------------------------------
// DEPRECATED — Compat layer (REMOVE IN WAVE 6)
// Maps legacy SCSS names used across components to V2 tokens to avoid mass
// refactor in wave 1. Each entry must be replaced by direct usage of V2 tokens
// before wave 6 closure.
// ----------------------------------------------------------------------------
$ifs-brand-dark-blue: var(--ifs-brand-700);   // DEPRECATED
$ifs-brand-blue:      var(--ifs-brand-500);   // DEPRECATED
$ifs-brand-blue-deep: var(--ifs-brand-700);   // DEPRECATED
$ifs-brand-cyan:      var(--ifs-accent-500);  // DEPRECATED
$ifs-brand-cyan-light: var(--ifs-accent-500); // DEPRECATED
$ifs-brand-cyan-deep: var(--ifs-accent-600);  // DEPRECATED
$ifs-brand-teal:      var(--ifs-accent-500);  // DEPRECATED

$ifs-ink-900: var(--ifs-text-primary);    // DEPRECATED
$ifs-ink-800: var(--ifs-text-primary);    // DEPRECATED
$ifs-ink-700: var(--ifs-text-primary);    // DEPRECATED
$ifs-ink-500: var(--ifs-text-secondary);  // DEPRECATED
$ifs-ink-400: var(--ifs-text-muted);      // DEPRECATED
$ifs-ink-300: var(--ifs-border-strong);   // DEPRECATED
$ifs-ink-200: var(--ifs-border-subtle);   // DEPRECATED
$ifs-ink-100: var(--ifs-border-subtle);   // DEPRECATED

$ifs-surface-0:   var(--ifs-surface-1);   // DEPRECATED
$ifs-surface-50:  var(--ifs-surface-1);   // DEPRECATED
$ifs-surface-100: var(--ifs-surface-2);   // DEPRECATED
$ifs-surface-200: var(--ifs-surface-2);   // DEPRECATED
$ifs-surface-300: var(--ifs-surface-3);   // DEPRECATED
$ifs-surface-tinted: var(--ifs-surface-2); // DEPRECATED

$ifs-success-soft: var(--ifs-success-bg);    // DEPRECATED
$ifs-success-border: var(--ifs-success);     // DEPRECATED
$ifs-error: var(--ifs-danger);               // DEPRECATED
$ifs-error-strong: var(--ifs-danger);        // DEPRECATED
$ifs-error-soft: var(--ifs-danger-bg);       // DEPRECATED
$ifs-error-border: var(--ifs-danger);        // DEPRECATED
$ifs-warning-strong: var(--ifs-warning);     // DEPRECATED
$ifs-warning-soft: var(--ifs-warning-bg);    // DEPRECATED
$ifs-warning-border: var(--ifs-warning);     // DEPRECATED
$ifs-info-soft: var(--ifs-info-bg);          // DEPRECATED
$ifs-info-border: var(--ifs-info);           // DEPRECATED

// Gradients legacy → fall back to single CTA gradient (any usage other than CTA
// is a wave-2+ migration target). Login keeps its own gradient.
$ifs-gradient-brand:       var(--ifs-gradient-cta);   // DEPRECATED
$ifs-gradient-brand-soft:  var(--ifs-gradient-cta);   // DEPRECATED
$ifs-gradient-login:       var(--ifs-gradient-login); // DEPRECATED (rename only)
$ifs-gradient-nav:         var(--ifs-surface-1);      // DEPRECATED — nav becomes flat
$ifs-gradient-cta:         var(--ifs-gradient-cta);   // DEPRECATED (rename only)
$ifs-gradient-card-soft:   var(--ifs-surface-1);      // DEPRECATED — cards are flat
$ifs-gradient-app-bg:      var(--ifs-bg);             // DEPRECATED — app bg is flat
$ifs-gradient-glass-footer: var(--ifs-surface-1);     // DEPRECATED — footer is flat

$ifs-border-soft:   var(--ifs-border-subtle); // DEPRECATED
$ifs-border-medium: var(--ifs-border-subtle); // DEPRECATED
$ifs-border-strong: var(--ifs-border-strong); // (kept name)

$ifs-radius-xl:   var(--ifs-radius-lg);  // DEPRECATED
$ifs-radius-2xl:  var(--ifs-radius-lg);  // DEPRECATED
$ifs-radius-3xl:  var(--ifs-radius-lg);  // DEPRECATED

$ifs-shadow-xs: var(--ifs-shadow-sm);    // DEPRECATED
$ifs-shadow-xl: var(--ifs-shadow-lg);    // DEPRECATED
$ifs-shadow-cta: var(--ifs-shadow-md);   // DEPRECATED
$ifs-shadow-cta-hover: var(--ifs-shadow-md); // DEPRECATED
$ifs-shadow-hero: var(--ifs-shadow-lg);  // DEPRECATED
$ifs-shadow-nav:  var(--ifs-shadow-sm);  // DEPRECATED

// Spacing legacy (rem) → mapped to closest 4px-grid token in px
$ifs-space-0:  0;                        // DEPRECATED
$ifs-space-1:  var(--ifs-space-4);       // DEPRECATED
$ifs-space-2:  var(--ifs-space-8);       // DEPRECATED
$ifs-space-3:  var(--ifs-space-12);      // DEPRECATED
$ifs-space-4:  var(--ifs-space-16);      // DEPRECATED
$ifs-space-5:  var(--ifs-space-20);      // DEPRECATED
$ifs-space-6:  var(--ifs-space-24);      // DEPRECATED
$ifs-space-8:  var(--ifs-space-32);      // DEPRECATED
$ifs-space-10: var(--ifs-space-40);      // DEPRECATED
$ifs-space-12: var(--ifs-space-48);      // DEPRECATED
$ifs-space-16: var(--ifs-space-64);      // DEPRECATED

$ifs-ease-out:    var(--ifs-ease-standard);    // DEPRECATED
$ifs-ease-in-out: var(--ifs-ease-emphasized);  // DEPRECATED

// On-brand text legacy (collapse to a single token)
$ifs-on-brand-strong: var(--ifs-text-on-brand);  // DEPRECATED
$ifs-on-brand-high:   var(--ifs-text-on-brand);  // DEPRECATED
$ifs-on-brand-muted:  rgba(255, 255, 255, 0.78); // DEPRECATED
$ifs-on-brand-subtle: rgba(255, 255, 255, 0.7);  // DEPRECATED
$ifs-on-dark-strong:  var(--ifs-text-primary);   // DEPRECATED
$ifs-on-dark-muted:   var(--ifs-text-secondary); // DEPRECATED
$ifs-on-dark-subtle:  var(--ifs-text-muted);     // DEPRECATED

$ifs-text-primary:    var(--ifs-text-primary);   // (kept name)
$ifs-text-secondary:  var(--ifs-text-secondary); // (kept name)
$ifs-accent-soft-contrast: var(--ifs-accent-500); // DEPRECATED
```

Fin du livrable.
