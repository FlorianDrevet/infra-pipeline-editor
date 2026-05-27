export interface DetectedPipelineOptionsResponse {
  testFramework: string | null;
  suggestedTestCommand: string | null;
  suggestedTestResultsFormat: string | null;
  suggestedCoverageTool: string | null;
  suggestedCoverageReportPath: string | null;
  lintingAvailable: boolean;
  suggestedLintCommand: string | null;
  sonarConfigDetected: boolean;
  suggestedSonarProjectKey: string | null;
  dependencyScanAvailable: boolean;
  suggestedDependencyScanTool: string | null;
}
