/**
 * Visual variants exposed by {@link DsButtonComponent}.
 * `success` is kept for backward compatibility with existing call-sites.
 */
export type DsButtonVariant =
  | 'primary'
  | 'secondary'
  | 'ghost'
  | 'danger'
  | 'subtle'
  | 'success';

/** Available sizes for {@link DsButtonComponent}. */
export type DsButtonSize = 'sm' | 'md' | 'lg';

/** Icon placement inside {@link DsButtonComponent}. */
export type DsButtonIconPosition = 'leading' | 'trailing';

/** Native button type forwarded by {@link DsButtonComponent}. */
export type DsButtonType = 'button' | 'submit';
