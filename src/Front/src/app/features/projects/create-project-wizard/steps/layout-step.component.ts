import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  computed,
  effect,
  model,
  output,
} from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { TranslateModule } from '@ngx-translate/core';
import {
  CreateProjectWizardDraft,
  LayoutPreset,
  createEmptyRepository,
} from '../create-project-wizard.types';

interface LayoutOption {
  value: LayoutPreset;
  titleKey: string;
  descKey: string;
  icon: string;
}

/**
 * Step 2 — Repository organisation preset (AllInOne / SplitInfraCode / MultiRepo).
 * Selecting a preset also seeds the `repositories` list so step 4 has the right
 * shape when the user enters it.
 */
@Component({
  selector: 'app-layout-step',
  standalone: true,
  imports: [TranslateModule, MatIconModule],
  templateUrl: './layout-step.component.html',
  styleUrl: './layout-step.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LayoutStepComponent implements OnInit {
  public readonly draft = model.required<CreateProjectWizardDraft>();
  public readonly validityChange = output<boolean>();

  protected readonly options: LayoutOption[] = [
    {
      value: 'AllInOne',
      titleKey: 'PROJECT_CREATE.STEP.LAYOUT.ALL_IN_ONE_TITLE',
      descKey: 'PROJECT_CREATE.STEP.LAYOUT.ALL_IN_ONE_DESC',
      icon: 'inventory_2',
    },
    {
      value: 'SplitInfraCode',
      titleKey: 'PROJECT_CREATE.STEP.LAYOUT.SPLIT_TITLE',
      descKey: 'PROJECT_CREATE.STEP.LAYOUT.SPLIT_DESC',
      icon: 'call_split',
    },
    {
      value: 'MultiRepo',
      titleKey: 'PROJECT_CREATE.STEP.LAYOUT.MULTI_TITLE',
      descKey: 'PROJECT_CREATE.STEP.LAYOUT.MULTI_DESC',
      icon: 'hub',
    },
  ];

  protected readonly selected = computed(() => this.draft().layoutPreset);

  public constructor() {
    effect(() => {
      this.validityChange.emit(this.selected() !== '');
    });
  }

  public ngOnInit(): void {
    this.validityChange.emit(this.selected() !== '');
  }

  protected select(preset: LayoutPreset): void {
    this.draft.update((d) => {
      const next = { ...d, layoutPreset: preset };
      next.repositories = this.seedRepositoriesFor(preset, d.repositories);
      return next;
    });
  }

  private seedRepositoriesFor(preset: LayoutPreset, current: CreateProjectWizardDraft['repositories']) {
    if (preset === 'MultiRepo') {
      return [];
    }
    if (preset === 'AllInOne') {
      if (current.length === 1 && current[0].contentKinds.length === 2) {
        return current;
      }
      return [createEmptyRepository(['Infrastructure', 'ApplicationCode'])];
    }
    // SplitInfraCode
    const hasInfra = current.find((r) => r.contentKinds.length === 1 && r.contentKinds[0] === 'Infrastructure');
    const hasApp = current.find((r) => r.contentKinds.length === 1 && r.contentKinds[0] === 'ApplicationCode');
    if (hasInfra && hasApp && current.length === 2) {
      return current;
    }
    return [
      hasInfra ?? createEmptyRepository(['Infrastructure']),
      hasApp ?? createEmptyRepository(['ApplicationCode']),
    ];
  }
}
