/**
 * Available sizes for the {@link DsSegmentedControlComponent}.
 */
export type DsSegmentedSize = 'sm' | 'md';

/**
 * Definition of a single option inside the segmented control.
 */
export interface DsSegmentedOption {
  /** Stable value used as the control value (and tracking key). */
  readonly value: string;
  /** Visible label of the option. */
  readonly label: string;
  /** Optional Material icon ligature rendered to the left of the label. */
  readonly icon?: string;
  /** When true, the option is rendered as disabled and cannot be selected. */
  readonly disabled?: boolean;
}
