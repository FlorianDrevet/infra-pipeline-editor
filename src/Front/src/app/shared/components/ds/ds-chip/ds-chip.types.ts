/**
 * Visual variants exposed by {@link DsChipComponent}.
 *
 * V2 variants: `neutral`, `success`, `warning`, `danger`, `accent`, `info`.
 *
 * @deprecated `primary`, `error`, `cyan` are kept for backward compatibility
 * and are mapped to V2 variants internally.
 */
export type DsChipVariant =
  | 'neutral'
  | 'success'
  | 'warning'
  | 'danger'
  | 'accent'
  | 'info'
  | 'primary'
  | 'error'
  | 'cyan';

/** Input variants accepted by {@link DsChipComponent}. */
export type DsChipInputVariant =
  | 'neutral'
  | 'success'
  | 'warning'
  | 'danger'
  | 'accent'
  | 'info'
  | 'primary'
  | 'error'
  | 'cyan';

/** Available sizes for {@link DsChipComponent}. */
export type DsChipSize = 'sm' | 'md';
