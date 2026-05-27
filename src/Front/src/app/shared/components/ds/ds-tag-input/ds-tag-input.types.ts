/**
 * A single tag managed by {@link DsTagInputComponent}.
 */
export interface DsTagInputItem {
  /** Stable value used for equality and deduplication. */
  value: string;
  /** Optional human-readable label. Falls back to `value` when omitted. */
  label?: string;
  /** When false, the chip will not display a remove affordance. Defaults to true. */
  removable?: boolean;
}

/**
 * Validator signature for {@link DsTagInputComponent}.
 *
 * Return `true` to accept the candidate value, `false` to reject silently,
 * or an error i18n key / message string to reject with feedback.
 */
export type DsTagInputValidator = (
  value: string,
  existing: readonly DsTagInputItem[],
) => boolean | string;
