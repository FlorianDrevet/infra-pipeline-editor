import { ChangeDetectionStrategy, Component, computed, effect, input, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';

import { ToggleSectionCardComponent } from '../../../../shared/components/toggle-section-card/toggle-section-card.component';
import { DsTextFieldComponent, DsSelectComponent, DsSelectOption } from '../../../../shared/components/ds';
import {
  PipelineStepOptions,
  DEFAULT_PIPELINE_STEP_OPTIONS,
  TEST_RESULTS_FORMAT_OPTIONS,
  COVERAGE_TOOL_OPTIONS,
  DEPENDENCY_SCAN_TOOL_OPTIONS,
} from '../../models/pipeline-step-options.model';

@Component({
  selector: 'app-pipeline-options',
  standalone: true,
  imports: [
    FormsModule,
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

  // ─── Internal state (cloned from input) ───
  protected readonly options = signal<PipelineStepOptions>({ ...DEFAULT_PIPELINE_STEP_OPTIONS });

  // ─── Select options ───
  protected readonly testResultsFormatOptions: DsSelectOption[] = TEST_RESULTS_FORMAT_OPTIONS;
  protected readonly coverageToolOptions: DsSelectOption[] = COVERAGE_TOOL_OPTIONS;
  protected readonly dependencyScanToolOptions: DsSelectOption[] = DEPENDENCY_SCAN_TOOL_OPTIONS;

  // ─── Derived state ───
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

  protected patch(partial: Partial<PipelineStepOptions>): void {
    const updated = { ...this.options(), ...partial };
    this.options.set(updated);
    this.optionsChanged.emit(updated);
  }
}
