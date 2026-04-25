import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  computed,
  effect,
  model,
  output,
  signal,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { TranslateModule } from '@ngx-translate/core';
import {
  DsAlertComponent,
  DsButtonComponent,
  DsCardComponent,
  DsChipComponent,
  DsIconButtonComponent,
  DsSelectComponent,
  DsTextFieldComponent,
  DsToggleComponent,
} from '../../../../shared/components/ds';
import { DsSelectOption } from '../../../../shared/components/ds/ds-select/ds-select.component';
import { LOCATION_OPTIONS } from '../../../../shared/enums/location.enum';
import {
  CreateProjectWizardDraft,
  EnvironmentDraft,
  createEmptyEnvironment,
} from '../create-project-wizard.types';

const STANDARD_ENV_PRESETS: EnvironmentDraft[] = [
  { name: 'Development', shortName: 'dev', prefix: 'dev-', suffix: '-dev', location: 'WestEurope', subscriptionId: '', order: 0, requiresApproval: false },
  { name: 'Staging', shortName: 'stg', prefix: 'stg-', suffix: '-stg', location: 'WestEurope', subscriptionId: '', order: 1, requiresApproval: true },
  { name: 'Production', shortName: 'prd', prefix: 'prd-', suffix: '-prd', location: 'WestEurope', subscriptionId: '', order: 2, requiresApproval: true },
];

/**
 * Step 3 — One or more environment definitions. Order is auto-recomputed and a
 * default Development environment is seeded if the list is empty when the step opens.
 */
@Component({
  selector: 'app-environment-step',
  standalone: true,
  imports: [
    FormsModule,
    MatIconModule,
    TranslateModule,
    DsAlertComponent,
    DsButtonComponent,
    DsCardComponent,
    DsChipComponent,
    DsIconButtonComponent,
    DsSelectComponent,
    DsTextFieldComponent,
    DsToggleComponent,
  ],
  templateUrl: './environment-step.component.html',
  styleUrl: './environment-step.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EnvironmentStepComponent implements OnInit {
  public readonly draft = model.required<CreateProjectWizardDraft>();
  public readonly validityChange = output<boolean>();

  protected readonly locationOptions: DsSelectOption[] = LOCATION_OPTIONS.map((option) => ({
    value: option.value,
    label: option.label,
  }));

  protected readonly expandedIndex = signal<number>(0);
  protected readonly showSuggestionsConfirm = signal<boolean>(false);

  protected readonly environments = computed(() => this.draft().environments);
  protected readonly canRemove = computed(() => this.environments().length > 1);

  protected readonly allValid = computed(() => {
    const envs = this.environments();
    if (envs.length === 0) {
      return false;
    }
    return envs.every(
      (env) => env.name.trim() !== '' && env.shortName.trim() !== '' && env.location.trim() !== '',
    );
  });

  public constructor() {
    effect(() => {
      this.validityChange.emit(this.allValid());
    });
  }

  public ngOnInit(): void {
    if (this.draft().environments.length === 0) {
      this.draft.update((d) => ({
        ...d,
        environments: [createEmptyEnvironment(0)],
      }));
    }
    this.validityChange.emit(this.allValid());
  }

  protected toggleExpanded(index: number): void {
    this.expandedIndex.update((current) => (current === index ? -1 : index));
  }

  protected addEnvironment(): void {
    this.draft.update((d) => {
      const next = [...d.environments, createEmptyEnvironment(d.environments.length)];
      return { ...d, environments: this.reindex(next) };
    });
    this.expandedIndex.set(this.draft().environments.length - 1);
  }

  protected removeEnvironment(index: number): void {
    if (!this.canRemove()) {
      return;
    }
    this.draft.update((d) => {
      const next = d.environments.filter((_, i) => i !== index);
      return { ...d, environments: this.reindex(next) };
    });
    this.expandedIndex.set(0);
  }

  protected updateField<K extends keyof EnvironmentDraft>(
    index: number,
    field: K,
    value: EnvironmentDraft[K],
  ): void {
    this.draft.update((d) => {
      const next = d.environments.map((env, i) => (i === index ? { ...env, [field]: value } : env));
      return { ...d, environments: next };
    });
  }

  protected requestSuggestions(): void {
    if (this.isEnvironmentsDirty()) {
      this.showSuggestionsConfirm.set(true);
    } else {
      this.confirmSuggestions();
    }
  }

  private isEnvironmentsDirty(): boolean {
    const envs = this.environments();
    if (envs.length > 1) return true;
    if (envs.length === 0) return false;
    const e = envs[0];
    return (
      e.name.trim() !== '' ||
      e.shortName.trim() !== '' ||
      e.subscriptionId.trim() !== '' ||
      e.prefix.trim() !== '' ||
      e.suffix.trim() !== ''
    );
  }

  protected confirmSuggestions(): void {
    this.draft.update((d) => ({
      ...d,
      environments: STANDARD_ENV_PRESETS.map((env) => ({ ...env })),
    }));
    this.showSuggestionsConfirm.set(false);
    this.expandedIndex.set(0);
  }

  protected cancelSuggestions(): void {
    this.showSuggestionsConfirm.set(false);
  }

  private reindex(envs: EnvironmentDraft[]): EnvironmentDraft[] {
    return envs.map((env, i) => ({ ...env, order: i }));
  }
}
