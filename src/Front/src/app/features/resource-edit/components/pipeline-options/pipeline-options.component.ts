import { ChangeDetectionStrategy, Component, computed, effect, inject, input, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';

import { ToggleSectionCardComponent } from '../../../../shared/components/toggle-section-card/toggle-section-card.component';
import { DsTextFieldComponent, DsSelectComponent, DsSelectOption } from '../../../../shared/components/ds';
import { DsButtonComponent } from '../../../../shared/components/ds/ds-button/ds-button.component';
import { TestFrameworkCatalogService } from '../../../../shared/services/test-framework-catalog.service';
import {
  PipelineStepOptions,
  PipelineStackProfile,
  DotNetProfile,
  NodeJsProfile,
  AngularProfile,
  JavaProfile,
  PythonProfile,
  StaticSiteProfile,
  CustomProfile,
  DEFAULT_PIPELINE_STEP_OPTIONS,
  APPLICATION_STACK_OPTIONS,
  TEST_RESULTS_FORMAT_OPTIONS,
  COVERAGE_TOOL_OPTIONS,
  DEPENDENCY_SCAN_TOOL_OPTIONS,
  NODE_PACKAGE_MANAGER_OPTIONS,
  JAVA_BUILD_TOOL_OPTIONS,
  PYTHON_PACKAGE_MANAGER_OPTIONS,
  getDefaultProfileForStack,
} from '../../models/pipeline-step-options.model';

@Component({
  selector: 'app-pipeline-options',
  standalone: true,
  imports: [
    FormsModule,
    DsButtonComponent,
    TranslateModule,
    ToggleSectionCardComponent,
    DsTextFieldComponent,
    DsSelectComponent,
  ],
  templateUrl: './pipeline-options.component.html',
  styleUrl: './pipeline-options.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PipelineOptionsComponent {
  readonly pipelineStepOptions = input<PipelineStepOptions | null>(null);

  readonly optionsChanged = output<PipelineStepOptions>();
  readonly detectOptions = output<void>();

  protected readonly detecting = signal(false);

  // ─── Internal state (cloned from input) ───
  protected readonly options = signal<PipelineStepOptions>({ ...DEFAULT_PIPELINE_STEP_OPTIONS });

  // ─── Catalog-driven test framework options ───
  private readonly catalog = inject(TestFrameworkCatalogService);
  protected readonly testFrameworkOptions = computed(() => this.catalog.getOptionsForStack(this.selectedStack()));

  // ─── Stack select options ───
  protected readonly applicationStackOptions: DsSelectOption[] = APPLICATION_STACK_OPTIONS;
  protected readonly nodePackageManagerOptions: DsSelectOption[] = NODE_PACKAGE_MANAGER_OPTIONS;
  protected readonly javaBuildToolOptions: DsSelectOption[] = JAVA_BUILD_TOOL_OPTIONS;
  protected readonly pythonPackageManagerOptions: DsSelectOption[] = PYTHON_PACKAGE_MANAGER_OPTIONS;

  // ─── Generic select options ───
  protected readonly testResultsFormatOptions: DsSelectOption[] = TEST_RESULTS_FORMAT_OPTIONS;
  protected readonly coverageToolOptions: DsSelectOption[] = COVERAGE_TOOL_OPTIONS;
  protected readonly dependencyScanToolOptions: DsSelectOption[] = DEPENDENCY_SCAN_TOOL_OPTIONS;

  // ─── Stack-derived state ───
  protected readonly selectedStack = computed(() => this.options().stack ?? null);
  protected readonly profile = computed(() => this.options().profile);

  protected readonly dotnetProfile = computed(() => {
    const p = this.profile();
    return p?.kind === 'DotNet' ? p : undefined;
  });

  protected readonly nodeJsProfile = computed(() => {
    const p = this.profile();
    return p?.kind === 'NodeJs' ? p : undefined;
  });

  protected readonly angularProfile = computed(() => {
    const p = this.profile();
    return p?.kind === 'Angular' ? p : undefined;
  });

  protected readonly javaProfile = computed(() => {
    const p = this.profile();
    return p?.kind === 'Java' ? p : undefined;
  });

  protected readonly pythonProfile = computed(() => {
    const p = this.profile();
    return p?.kind === 'Python' ? p : undefined;
  });

  protected readonly staticSiteProfile = computed(() => {
    const p = this.profile();
    return p?.kind === 'StaticSite' ? p : undefined;
  });

  protected readonly customProfile = computed(() => {
    const p = this.profile();
    return p?.kind === 'Custom' ? p : undefined;
  });

  // ─── Generic derived state ───
  protected readonly runUnitTests = computed(() => this.options().runUnitTests);
  protected readonly publishCodeCoverage = computed(() => this.options().publishCodeCoverage);
  protected readonly runSonarAnalysis = computed(() => this.options().runSonarAnalysis);
  protected readonly runLinting = computed(() => this.options().runLinting);
  protected readonly runDependencyScan = computed(() => this.options().runDependencyScan);
  protected readonly runBuildValidation = computed(() => this.options().runBuildValidation);
  protected readonly enableDependencyCache = computed(() => this.options().enableDependencyCache);
  protected readonly runSmokeTests = computed(() => this.options().runSmokeTests);

  private readonly syncEffect = effect(() => {
    const incoming = this.pipelineStepOptions();
    if (incoming) {
      this.options.set({ ...incoming });
    }
  });

  constructor() {
    this.catalog.loadCatalog();
  }

  protected patch(partial: Partial<PipelineStepOptions>): void {
    const updated = { ...this.options(), ...partial };
    this.options.set(updated);
    this.optionsChanged.emit(updated);
  }

  protected onStackChange(value: string | number | null): void {
    const stack = value as string | null;
    const profile = getDefaultProfileForStack(stack);
    this.patch({ stack: stack ?? undefined, profile });
  }

  protected patchProfile(partial: Partial<PipelineStackProfile>): void {
    const current = this.options().profile;
    if (!current) return;
    const updated = { ...current, ...partial } as PipelineStackProfile;
    this.patch({ profile: updated });
  }

  protected onAutoDetect(): void {
    this.detectOptions.emit();
  }
}
