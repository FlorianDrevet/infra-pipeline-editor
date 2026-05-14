export interface PipelineStepOptions {
  runUnitTests: boolean;
  testCommand?: string;
  testFramework?: string;
  testResultsFormat?: string;
  testResultsPath?: string;
  publishTestResults: boolean;
  publishCodeCoverage: boolean;
  coverageTool?: string;
  coverageReportPath?: string;
  runSonarAnalysis: boolean;
  sonarProjectKey?: string;
  sonarOrganization?: string;
  sonarServiceConnection?: string;
  runLinting: boolean;
  lintCommand?: string;
  runDependencyScan: boolean;
  dependencyScanTool?: string;
  runBuildValidation: boolean;
  enableDependencyCache: boolean;
  runSmokeTests: boolean;
  smokeTestCommand?: string;
}

export const DEFAULT_PIPELINE_STEP_OPTIONS: PipelineStepOptions = {
  runUnitTests: false,
  publishTestResults: false,
  publishCodeCoverage: false,
  runSonarAnalysis: false,
  runLinting: false,
  runDependencyScan: false,
  runBuildValidation: false,
  enableDependencyCache: false,
  runSmokeTests: false,
};

export const TEST_RESULTS_FORMAT_OPTIONS = [
  { label: 'VSTest', value: 'VSTest' },
  { label: 'JUnit', value: 'JUnit' },
];

export const COVERAGE_TOOL_OPTIONS = [
  { label: 'Cobertura', value: 'Cobertura' },
  { label: 'JaCoCo', value: 'JaCoCo' },
];

export const DEPENDENCY_SCAN_TOOL_OPTIONS = [
  { label: 'OWASP Dependency-Check', value: 'OWASPDependencyCheck' },
  { label: 'npm audit', value: 'NpmAudit' },
  { label: 'pip-audit', value: 'PipAudit' },
  { label: 'Snyk', value: 'Snyk' },
];
