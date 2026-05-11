/**
 * Visual variants exposed by {@link DsIconButtonComponent}.
 *
 * V2 variants: `neutral`, `accent`, `danger`.
 *
 * @deprecated `ghost`, `primary`, `subtle` are kept for backward compatibility
 * and are mapped to V2 variants internally (`ghost`→`neutral`,
 * `primary`→`accent`, `subtle`→`neutral` filled).
 */
export type DsIconButtonVariant =
  | 'neutral'
  | 'accent'
  | 'danger'
  | 'ghost'
  | 'primary'
  | 'subtle';

/** Input variants accepted by {@link DsIconButtonComponent}. */
export type DsIconButtonInputVariant =
  | 'neutral'
  | 'accent'
  | 'danger'
  | 'ghost'
  | 'primary'
  | 'subtle';

/** Available sizes for {@link DsIconButtonComponent}. */
export type DsIconButtonSize = 'sm' | 'md' | 'lg';

/** Native button type forwarded by {@link DsIconButtonComponent}. */
export type DsIconButtonType = 'button' | 'submit';
