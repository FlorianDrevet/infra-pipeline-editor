/**
 * A single key-value pair managed by {@link DsKeyValueInputComponent}.
 */
export interface DsKeyValueItem {
  /** Unique identifier key. */
  key: string;
  /** Associated value. */
  value: string;
}

/**
 * Validator signature for {@link DsKeyValueInputComponent}.
 *
 * Return `true` to accept the pair, `false` to reject silently,
 * or an error i18n key / message string to reject with feedback.
 */
export type DsKeyValueValidator = (
  key: string,
  value: string,
  existing: readonly DsKeyValueItem[],
) => boolean | string;
