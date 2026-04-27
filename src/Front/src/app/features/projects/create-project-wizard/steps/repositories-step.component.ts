import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  model,
  output,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import {
  DsAlertComponent,
  DsCardComponent,
  DsChipComponent,
  DsSelectComponent,
  DsTextFieldComponent,
} from '../../../../shared/components/ds';
import { DsSelectOption } from '../../../../shared/components/ds/ds-select/ds-select.component';
import {
  CreateProjectWizardDraft,
  RepositoryDraft,
} from '../create-project-wizard.types';

const ALIAS_PATTERN = /^[a-z0-9-]+$/;

interface RepoCardLabel {
  index: number;
  titleKey: string;
  contentKindKeys: string[];
}

/**
 * Step 4 — Repositories. Only invoked for AllInOne / SplitInfraCode layouts.
 * Connection details (provider/url/branch) follow an all-or-nothing rule mirroring the backend.
 */
@Component({
  selector: 'app-repositories-step',
  standalone: true,
  imports: [
    FormsModule,
    TranslateModule,
    DsAlertComponent,
    DsCardComponent,
    DsChipComponent,
    DsSelectComponent,
    DsTextFieldComponent,
  ],
  templateUrl: './repositories-step.component.html',
  styleUrl: './repositories-step.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RepositoriesStepComponent {
  public readonly draft = model.required<CreateProjectWizardDraft>();
  public readonly validityChange = output<boolean>();

  protected readonly providerOptions: DsSelectOption[] = [
    { value: '', label: '—' },
    { value: 'GitHub', label: 'GitHub' },
    { value: 'AzureDevOps', label: 'Azure DevOps' },
  ];

  protected readonly repositories = computed(() => this.draft().repositories);

  protected readonly cardLabels = computed<RepoCardLabel[]>(() => {
    const layout = this.draft().layoutPreset;
    if (layout === 'AllInOne') {
      return [
        {
          index: 0,
          titleKey: 'PROJECT_CREATE.STEP.REPOSITORIES.MAIN',
          contentKindKeys: ['Infrastructure', 'ApplicationCode'],
        },
      ];
    }
    if (layout === 'SplitInfraCode') {
      return this.repositories().map((repo, idx) => ({
        index: idx,
        titleKey: repo.contentKinds.includes('Infrastructure')
          ? 'PROJECT_CREATE.STEP.REPOSITORIES.INFRA'
          : 'PROJECT_CREATE.STEP.REPOSITORIES.APP',
        contentKindKeys: repo.contentKinds,
      }));
    }
    return [];
  });

  protected readonly invalidConnectionIndexes = computed<number[]>(() => {
    const result: number[] = [];
    this.repositories().forEach((repo, idx) => {
      if (this.connectionPartial(repo)) {
        result.push(idx);
      }
    });
    return result;
  });

  protected readonly allValid = computed(() => {
    const repos = this.repositories();
    if (repos.length === 0) {
      return false;
    }
    return repos.every(
      (repo) => ALIAS_PATTERN.test(repo.alias) && !this.connectionPartial(repo),
    );
  });

  public constructor() {
    effect(() => {
      this.validityChange.emit(this.allValid());
    });
  }

  protected updateField<K extends keyof RepositoryDraft>(
    index: number,
    field: K,
    value: RepositoryDraft[K],
  ): void {
    this.draft.update((d) => {
      const next = d.repositories.map((repo, i) =>
        i === index ? { ...repo, [field]: value } : repo,
      );
      return { ...d, repositories: next };
    });
  }

  protected aliasError(repo: RepositoryDraft): string | undefined {
    if (!repo.alias) {
      return undefined;
    }
    return ALIAS_PATTERN.test(repo.alias) ? undefined : 'PROJECT_CREATE.STEP.REPOSITORIES.ALIAS_HINT';
  }

  protected connectionPartial(repo: RepositoryDraft): boolean {
    const filled = [repo.providerType, repo.repositoryUrl.trim(), repo.defaultBranch.trim()].filter(
      (v) => v !== '',
    ).length;
    return filled !== 0 && filled !== 3;
  }
}
