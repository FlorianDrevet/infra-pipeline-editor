/**
 * Validator signature for {@link DsListInputComponent}.
 *
 * Return `true` to accept the candidate value, `false` to reject silently,
 * or an error i18n key / message string to reject with feedback.
 */
export type DsListInputValidator = (
  value: string,
  existing: readonly string[],
) => boolean | string;
