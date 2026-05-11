/** Density preset controlling the row height of {@link DsTableComponent}. */
export type DsTableDensity = 'compact' | 'cozy' | 'comfortable';

/** Allowed sort directions for a column. `null` means "unsorted". */
export type DsTableSortDirection = 'asc' | 'desc' | null;

/** Sort state emitted / received by {@link DsTableComponent}. */
export interface DsTableSortState {
  readonly key: string;
  readonly direction: DsTableSortDirection;
}

/**
 * Column descriptor for {@link DsTableComponent}.
 *
 * @typeParam T Row type (used only for documentation; cell access goes
 *              through `key` because column descriptors are erased at runtime
 *              by Angular templates).
 */
// eslint-disable-next-line @typescript-eslint/no-unused-vars
export interface DsTableColumn<T> {
  /** Stable identifier — also used as the row property accessor by default. */
  readonly key: string;
  /** Visible header label. */
  readonly header: string;
  /**
   * Optional CSS grid track size. Examples: `120px`, `1fr`,
   * `minmax(120px, 1fr)`. Defaults to `minmax(120px, 1fr)` when omitted.
   */
  readonly width?: string;
  /** Cell text alignment (defaults to `start`). */
  readonly align?: 'start' | 'center' | 'end';
  /** When true, click on the header cycles the sort state through null→asc→desc→null. */
  readonly sortable?: boolean;
  /** When true, the header cell sticks to the top during vertical scrolls. */
  readonly sticky?: boolean;
  /** Extra CSS class applied to body cells of this column. */
  readonly cellClass?: string;
  /** When true, the cell content is rendered with `font-variant-numeric: tabular-nums`. */
  readonly mono?: boolean;
}
