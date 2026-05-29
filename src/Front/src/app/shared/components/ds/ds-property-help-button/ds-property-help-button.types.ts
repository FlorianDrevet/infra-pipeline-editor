/**
 * Describes a single section of structured help content displayed
 * by {@link DsPropertyHelpDialogComponent}.
 */
export interface DsPropertyHelpSection {
  readonly icon: string;
  readonly title: string;
  readonly body: string;
  readonly guidance?: string;
  readonly examples?: readonly string[];
}

/**
 * Data contract injected into the help dialog via {@link MAT_DIALOG_DATA}.
 */
export interface DsPropertyHelpDialogData {
  readonly title: string;
  readonly sections: readonly DsPropertyHelpSection[];
}
