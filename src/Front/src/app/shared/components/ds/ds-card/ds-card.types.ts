/**
 * Visual variants exposed by {@link DsCardComponent}.
 *
 * V2 variants: `default`, `interactive`, `outlined`.
 *
 * @deprecated `elevated` and `glass` are kept only for backward compatibility
 * and are silently rendered as `default`. They will be removed in a later wave.
 */
export type DsCardVariant =
  | 'default'
  | 'interactive'
  | 'outlined'
  | 'elevated'
  | 'glass';

/** Padding tokens accepted by {@link DsCardComponent}. */
export type DsCardPadding = 'none' | 'sm' | 'md' | 'lg';

/**
 * Border-left accent kept on the public input for backward compatibility.
 *
 * @deprecated V2 cards do not render coloured border-left accents anymore.
 * The input still exists but is ignored. Use {@link DsAlertComponent} or a
 * dedicated wrapper when a status accent is required.
 */
export type DsCardAccent = 'none' | 'primary' | 'success' | 'warning' | 'error';
