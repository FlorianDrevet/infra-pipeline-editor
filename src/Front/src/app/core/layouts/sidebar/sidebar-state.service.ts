import { DOCUMENT } from '@angular/common';
import { Injectable, computed, effect, inject, signal } from '@angular/core';

/** localStorage key used to persist the sidebar collapsed state across reloads. */
const SidebarCollapsedStorageKey = 'ifs.sidebar.collapsed';

/** CSS custom property mirroring the current sidebar width on the document root. */
const SidebarWidthCssVariable = '--ifs-sidebar-width';

/** Width applied when the sidebar is fully expanded. */
const ExpandedWidth = '240px';

/** Width applied when the sidebar is collapsed (icons only). */
const CollapsedWidth = '56px';

/**
 * Holds the persisted collapse/expand state of the application sidebar and
 * synchronises the corresponding CSS custom property on the document root so
 * the shell grid track adapts in real time.
 */
@Injectable({ providedIn: 'root' })
export class SidebarStateService {
  private readonly document = inject(DOCUMENT);

  private readonly _collapsed = signal<boolean>(this.readPersistedCollapsed());

  /** Signal exposing whether the sidebar is currently collapsed. */
  public readonly collapsed = this._collapsed.asReadonly();

  /** Computed CSS width string applied to the sidebar and shell grid track. */
  public readonly width = computed<string>(() => (this._collapsed() ? CollapsedWidth : ExpandedWidth));

  public constructor() {
    effect(() => {
      const collapsed = this._collapsed();
      this.persistCollapsed(collapsed);
      this.document.documentElement.style.setProperty(SidebarWidthCssVariable, this.width());
    });
  }

  /** Toggle the collapsed state. */
  public toggle(): void {
    this._collapsed.update((current) => !current);
  }

  /** Force the collapsed state to a specific value. */
  public setCollapsed(collapsed: boolean): void {
    this._collapsed.set(collapsed);
  }

  private readPersistedCollapsed(): boolean {
    try {
      return this.document.defaultView?.localStorage.getItem(SidebarCollapsedStorageKey) === 'true';
    } catch {
      return false;
    }
  }

  private persistCollapsed(collapsed: boolean): void {
    try {
      this.document.defaultView?.localStorage.setItem(SidebarCollapsedStorageKey, String(collapsed));
    } catch {
      // Ignore storage failures (private mode, quota exceeded, SSR-like environment).
    }
  }
}
