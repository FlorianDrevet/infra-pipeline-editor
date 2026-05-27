/**
 * Visual tone for a {@link DsMenuItem}.
 */
export type DsMenuItemTone = 'neutral' | 'danger';

/**
 * Placement of a menu relative to its trigger origin.
 */
export type DsMenuPlacement = 'bottom-start' | 'bottom-end' | 'top-start' | 'top-end';

/**
 * A single entry in a design system menu.
 */
export interface DsMenuItem {
  /** Stable identifier emitted via `itemSelected`. */
  id: string;
  /** Visible label (already localized — callers translate before passing). */
  label: string;
  /** Optional leading Material icon name. */
  icon?: string;
  /** Optional trailing Material icon name. */
  iconTrailing?: string;
  /** Visual tone. Defaults to `neutral`. */
  tone?: DsMenuItemTone;
  /** When true, the item is rendered but not interactive. */
  disabled?: boolean;
  /** When true, renders a horizontal separator instead of an interactive row. */
  divider?: boolean;
}
