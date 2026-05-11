/**
 * Severity variants exposed by {@link DsAlertComponent}.
 *
 * V2 keeps `info`, `success`, `warning`, `danger`. `error` remains as a
 * backward-compatible alias for `danger`.
 */
export type DsAlertSeverity =
  | 'info'
  | 'success'
  | 'warning'
  | 'danger'
  | 'error';
