# Design System

## Philosophy

The InfraFlowSculptor Design System (`app-ds-*`) provides a **single source of truth** for all UI primitives, ensuring visual consistency and reducing duplication.

---

## Available DS Components

| Component | Selector | Purpose |
|-----------|----------|---------|
| Button | `app-ds-button` | All button variants |
| Card | `app-ds-card` | Content containers |
| Input | `app-ds-input` | Form text inputs |
| Select | `app-ds-select` | Dropdown selects |
| Table | `app-ds-table` | Data tables with sorting |
| Dialog | `app-ds-dialog` | Modal dialogs |
| Chip List | `app-ds-chip-list` | Tag/chip collections |
| Empty State | `app-ds-empty-state` | No-data illustrations |
| Section Header | `app-ds-section-header` | Page/section titles |
| Skeleton | `app-ds-skeleton` | Loading placeholders |
| Sidebar | `app-ds-sidebar` | Navigation sidebar |

---

## Usage Rules

### MUST

- Use `app-ds-*` for **every** UI primitive (buttons, cards, inputs, tables)
- If a pattern doesn't have a DS component, create one in `shared/components/ds/` first
- Keep DS components generic and reusable — no feature-specific logic inside

### MUST NOT

- Use raw Material components directly in feature code
- Duplicate DS component logic in feature components
- Add feature-specific behavior to DS components
- Use inline styles that contradict DS patterns

---

## Component Anatomy

```typescript
@Component({
  selector: 'app-ds-button',
  standalone: true,
  imports: [MatButtonModule],
  template: `
    <button
      [mat-raised-button]="variant() === 'raised'"
      [mat-flat-button]="variant() === 'flat'"
      [color]="color()"
      [disabled]="disabled()"
      (click)="onClick.emit($event)">
      <ng-content />
    </button>
  `,
})
export class DsButtonComponent {
  readonly variant = input<'raised' | 'flat' | 'stroked'>('raised');
  readonly color = input<'primary' | 'accent' | 'warn'>('primary');
  readonly disabled = input(false);
  readonly onClick = output<MouseEvent>();
}
```

---

## Styling Architecture

```
src/Front/src/styles/
├── _variables.scss        → CSS custom properties (colors, spacing)
├── _typography.scss       → Font definitions
├── _theme.scss            → Material theme customization
├── _utilities.scss        → Tailwind extensions
└── styles.scss            → Entry point (imports above)
```

### Tailwind + Material Coexistence

- **Material** handles complex component behavior (dialogs, tables, overlays)
- **Tailwind** handles layout, spacing, and utility classes
- **No conflicts** because Material's structural styles are preserved

---

## Migration Status (Completed)

The DS migration (waves W1-W8) is complete:

| Wave | Scope | Status |
|------|-------|--------|
| W1 | Buttons, Cards | ✅ Done |
| W2 | Inputs, Selects | ✅ Done |
| W3 | Tables | ✅ Done |
| W4 | Dialogs | ✅ Done |
| W5 | Chips, Tags | ✅ Done |
| W6 | Empty states | ✅ Done |
| W7 | Section headers | ✅ Done |
| W8 | Skeletons, Sidebar | ✅ Done |

**Result:** Zero raw Material usage in feature code. All UI goes through `app-ds-*`.
