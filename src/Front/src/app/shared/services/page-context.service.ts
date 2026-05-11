import { Injectable, signal } from '@angular/core';

/**
 * Single breadcrumb segment exposed by {@link PageContextService}.
 *
 * The last segment is implicitly considered the current page and is rendered
 * with a stronger visual weight by the consumer (top-bar).
 */
export interface PageBreadcrumbSegment {
  /** Visible label for the segment. */
  readonly label: string;
  /** Optional Angular router link target. When omitted the segment is plain text. */
  readonly routerLink?: string;
}

/**
 * Page-level UI context shared between feature components and the application
 * shell. Pages declaratively call {@link setBreadcrumb} from their
 * initialization or `effect()` to populate the top-bar breadcrumb.
 *
 * The service is intentionally minimal — additional slots (page actions,
 * inline status) can be added as new signals when waves 5+ require them.
 */
@Injectable({ providedIn: 'root' })
export class PageContextService {
  private readonly _breadcrumb = signal<readonly PageBreadcrumbSegment[]>([]);

  /** Read-only signal exposing the current breadcrumb to the shell. */
  public readonly breadcrumb = this._breadcrumb.asReadonly();

  /**
   * Replace the current breadcrumb. Pass an empty array (or call
   * {@link clear}) when leaving the page that owns the context.
   */
  public setBreadcrumb(segments: readonly PageBreadcrumbSegment[]): void {
    this._breadcrumb.set(segments);
  }

  /** Reset the breadcrumb to an empty state. */
  public clear(): void {
    this._breadcrumb.set([]);
  }
}
