import { DsSelectOption } from '../../../shared/components/ds';

// ─── Application Stack ───

export const APPLICATION_STACK_OPTIONS: DsSelectOption[] = [
  { value: null, label: 'Unknown' },
  { value: 'DotNet', label: '.NET' },
  { value: 'NodeJs', label: 'Node.js' },
  { value: 'Angular', label: 'Angular' },
  { value: 'Java', label: 'Java' },
  { value: 'Python', label: 'Python' },
  { value: 'StaticSite', label: 'Static Site' },
  { value: 'Custom', label: 'Custom' },
];

// ─── Stack Profile Discriminated Union ───

export interface DotNetProfile {
  kind: 'DotNet';
  testFramework?: string;
  collectCoverage: boolean;
  customTestProjectGlob?: string;
}

export interface NodeJsProfile {
  kind: 'NodeJs';
  packageManager?: string;
  testFramework?: string;
  runLintScript: boolean;
  testScriptName?: string;
  lintScriptName?: string;
}

export interface AngularProfile {
  kind: 'Angular';
  packageManager?: string;
  runNgTest: boolean;
  runNgLint: boolean;
  runNgBuildProduction: boolean;
  projectName?: string;
}

export interface JavaProfile {
  kind: 'Java';
  buildTool?: string;
  testFramework?: string;
  collectCoverage: boolean;
}

export interface PythonProfile {
  kind: 'Python';
  packageManager?: string;
  testFramework?: string;
  collectCoverage: boolean;
}

export interface StaticSiteProfile {
  kind: 'StaticSite';
  buildCommand?: string;
  outputDirectory?: string;
}

export interface CustomProfile {
  kind: 'Custom';
  customTestCommand?: string;
  customLintCommand?: string;
  customBuildCommand?: string;
}

export type PipelineStackProfile =
  | DotNetProfile
  | NodeJsProfile
  | AngularProfile
  | JavaProfile
  | PythonProfile
  | StaticSiteProfile
  | CustomProfile;

// ─── Per-stack select option constants ───

export const DOTNET_TEST_FRAMEWORK_OPTIONS: DsSelectOption[] = [
  { value: 'XUnit', label: 'xUnit' },
  { value: 'NUnit', label: 'NUnit' },
  { value: 'MSTest', label: 'MSTest' },
];

export const NODE_PACKAGE_MANAGER_OPTIONS: DsSelectOption[] = [
  { value: 'Npm', label: 'npm' },
  { value: 'Yarn', label: 'Yarn' },
  { value: 'Pnpm', label: 'pnpm' },
];

export const NODE_TEST_FRAMEWORK_OPTIONS: DsSelectOption[] = [
  { value: 'Jest', label: 'Jest' },
  { value: 'Vitest', label: 'Vitest' },
  { value: 'Mocha', label: 'Mocha' },
];

export const JAVA_BUILD_TOOL_OPTIONS: DsSelectOption[] = [
  { value: 'Maven', label: 'Maven' },
  { value: 'Gradle', label: 'Gradle' },
];

export const JAVA_TEST_FRAMEWORK_OPTIONS: DsSelectOption[] = [
  { value: 'JUnit5', label: 'JUnit 5' },
  { value: 'JUnit4', label: 'JUnit 4' },
  { value: 'TestNG', label: 'TestNG' },
];

export const PYTHON_PACKAGE_MANAGER_OPTIONS: DsSelectOption[] = [
  { value: 'Pip', label: 'pip' },
  { value: 'Poetry', label: 'Poetry' },
  { value: 'Uv', label: 'uv' },
];

export const PYTHON_TEST_FRAMEWORK_OPTIONS: DsSelectOption[] = [
  { value: 'Pytest', label: 'pytest' },
  { value: 'Unittest', label: 'unittest' },
];

// ─── Main interface ───

export interface PipelineStepOptions {
  stack?: string;
  profile?: PipelineStackProfile;
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
  stack: undefined,
  profile: undefined,
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

// ─── Default profiles per stack ───

export function getDefaultProfileForStack(stack: string | null | undefined): PipelineStackProfile | undefined {
  switch (stack) {
    case 'DotNet':
      return { kind: 'DotNet', collectCoverage: true };
    case 'NodeJs':
      return { kind: 'NodeJs', runLintScript: true };
    case 'Angular':
      return { kind: 'Angular', runNgTest: true, runNgLint: true, runNgBuildProduction: true };
    case 'Java':
      return { kind: 'Java', collectCoverage: true };
    case 'Python':
      return { kind: 'Python', collectCoverage: true };
    case 'StaticSite':
      return { kind: 'StaticSite' };
    case 'Custom':
      return { kind: 'Custom' };
    default:
      return undefined;
  }
}

// ─── Generic option constants ───

export const TEST_RESULTS_FORMAT_OPTIONS: DsSelectOption[] = [
  { label: 'VSTest', value: 'VSTest' },
  { label: 'JUnit', value: 'JUnit' },
];

export const COVERAGE_TOOL_OPTIONS: DsSelectOption[] = [
  { label: 'Cobertura', value: 'Cobertura' },
  { label: 'JaCoCo', value: 'JaCoCo' },
];

export const DEPENDENCY_SCAN_TOOL_OPTIONS: DsSelectOption[] = [
  { label: 'OWASP Dependency-Check', value: 'OWASPDependencyCheck' },
  { label: 'npm audit', value: 'NpmAudit' },
  { label: 'pip-audit', value: 'PipAudit' },
  { label: 'Snyk', value: 'Snyk' },
];
