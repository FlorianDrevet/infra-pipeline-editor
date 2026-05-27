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
import { TranslateModule } from '@ngx-translate/core';
import { DsChipComponent, DsOptionCardComponent } from '../../../../shared/components/ds';
import { RepositoryConnectionFormComponent } from '../../../../shared/components/repository-connection-form/repository-connection-form.component';
import {
  CreateProjectWizardDraft,
  LayoutPreset,
  RepositoryDraft,
  RepositorySlotState,
  createEmptyRepository,
} from '../create-project-wizard.types';
import { RepositoryContentKind } from '../../../../shared/interfaces/project-repository.interface';

interface LayoutOption {
  value: LayoutPreset;
  titleKey: string;
  descKey: string;
  icon: string;
}

interface RepositorySlot {
  index: number;
  titleKey: string;
  contentKinds: RepositoryContentKind[];
  initialData: RepositoryDraft | undefined;
}

/**
 * Step 2 — Repository organisation preset (AllInOne / SplitInfraCode / MultiRepo).
 * When preset is not MultiRepo, inline repository connection forms are shown.
 */
@Component({
  selector: 'app-layout-step',
  standalone: true,
  imports: [TranslateModule, DsOptionCardComponent, DsChipComponent, RepositoryConnectionFormComponent],
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

  protected readonly showRepositorySlots = computed(
    () => this.selected() !== '' && this.selected() !== 'MultiRepo',
  );

  protected readonly repositorySlots = computed<RepositorySlot[]>(() => {
    const preset = this.selected();
    const repos = this.draft().repositories;

    if (preset === 'AllInOne') {
      return [
        {
          index: 0,
          titleKey: 'PROJECT_CREATE.STEP.REPOSITORIES.MAIN',
          contentKinds: ['Infrastructure', 'ApplicationCode'] as RepositoryContentKind[],
          initialData: repos[0],
        },
      ];
    }

    if (preset === 'SplitInfraCode') {
      return [
        {
          index: 0,
          titleKey: 'PROJECT_CREATE.STEP.REPOSITORIES.INFRA',
          contentKinds: ['Infrastructure'] as RepositoryContentKind[],
          initialData: repos[0],
        },
        {
          index: 1,
          titleKey: 'PROJECT_CREATE.STEP.REPOSITORIES.APP',
          contentKinds: ['ApplicationCode'] as RepositoryContentKind[],
          initialData: repos[1],
        },
      ];
    }

    return [];
  });

  private readonly slotStates = signal<Map<number, RepositorySlotState>>(new Map());

  public constructor() {
    effect(() => {
      const hasPreset = this.selected() !== '';
      const needsRepos = this.showRepositorySlots();

      if (!hasPreset) {
        this.validityChange.emit(false);
        return;
      }

      if (!needsRepos) {
        this.validityChange.emit(true);
        return;
      }

      // All-or-nothing: either all slots are valid, or all are completely empty
      const states = this.slotStates();
      const slots = this.repositorySlots();
      const repos = this.draft().repositories;

      const allEmpty = repos.every(
        (r) => !r.repositoryUrl.trim() && !r.providerType && !r.personalAccessToken,
      );

      if (allEmpty && states.size === 0) {
        this.validityChange.emit(true);
        return;
      }

      const allValid = slots.every((slot) => {
        const state = states.get(slot.index);
        return state?.isValid === true;
      });

      this.validityChange.emit(allValid || allEmpty);
    });
  }

  public ngOnInit(): void {
    this.validityChange.emit(this.selected() !== '');
  }

  protected select(preset: LayoutPreset): void {
    this.slotStates.set(new Map());
    this.draft.update((d) => {
      const next = { ...d, layoutPreset: preset };
      next.repositories = this.seedRepositoriesFor(preset, d.repositories);
      return next;
    });
  }

  protected onSlotStateChanged(index: number, state: RepositorySlotState): void {
    this.slotStates.update((map) => {
      const next = new Map(map);
      next.set(index, state);
      return next;
    });

    if (state.data) {
      this.draft.update((d) => {
        const repos = [...d.repositories];
        if (repos[index]) {
          repos[index] = {
            ...repos[index],
            providerType: state.data!.providerType as RepositoryDraft['providerType'],
            repositoryUrl: state.data!.repositoryUrl,
            defaultBranch: state.data!.defaultBranch,
            personalAccessToken: state.data!.personalAccessToken,
          };
        }
        return { ...d, repositories: repos };
      });
    }
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
