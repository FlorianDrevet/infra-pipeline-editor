export interface TestFrameworkDefinitionResponse {
  stack: string;
  frameworkKey: string;
  displayName: string;
  defaultCommand: string;
  defaultResultsFormat: string;
  defaultCoverageTool: string | null;
  defaultCoverageReportGlob: string | null;
}
