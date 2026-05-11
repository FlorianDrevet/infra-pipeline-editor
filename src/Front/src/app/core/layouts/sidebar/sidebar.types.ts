/**
 * Static configuration of a sidebar navigation item.
 */
export interface SidebarNavItem {
  /** Stable identifier used for tracking in @for loops. */
  readonly id: string;
  /** Material icon ligature rendered to the left of the label. */
  readonly icon: string;
  /** i18n key for the visible label and tooltip. */
  readonly labelKey: string;
  /** Internal Angular router link target. */
  readonly routerLink: string;
  /** When true, the item is matched as active even on child routes. */
  readonly exact?: boolean;
}
