/**
 * Definition of a single tab inside an {@link DsTabsComponent}.
 *
 * Use a stable unique `id` per tab — it is used both for tracking and as the
 * value emitted/received via the `activeTabId` two-way binding.
 */
export interface DsTabDefinition {
  /** Stable identifier used by the parent for tracking and active selection. */
  readonly id: string;
  /** Visible label rendered inside the tab. */
  readonly label: string;
  /** Optional Material icon ligature rendered to the left of the label. */
  readonly icon?: string;
  /** Optional badge text rendered to the right of the label as a chip. */
  readonly badge?: string;
  /** When true, the tab is rendered as disabled and cannot be selected. */
  readonly disabled?: boolean;
}
