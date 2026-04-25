import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';
import {
  DsChipComponent,
  DsSectionHeaderComponent,
} from '../../../../shared/components/ds';
import {
  CreateProjectWizardDraft,
  RepositoryDraft,
} from '../create-project-wizard.types';

const LAYOUT_LABELS: Record<string, { titleKey: string; icon: string }> = {
  AllInOne: { titleKey: 'PROJECT_CREATE.STEP.LAYOUT.ALL_IN_ONE_TITLE', icon: 'inventory_2' },
  SplitInfraCode: { titleKey: 'PROJECT_CREATE.STEP.LAYOUT.SPLIT_TITLE', icon: 'call_split' },
  MultiRepo: { titleKey: 'PROJECT_CREATE.STEP.LAYOUT.MULTI_TITLE', icon: 'hub' },
};

/**
 * Step 5 — Read-only summary just before submission.
 */
@Component({
  selector: 'app-review-step',
  standalone: true,
  imports: [TranslateModule, DsChipComponent, DsSectionHeaderComponent],
  templateUrl: './review-step.component.html',
  styleUrl: './review-step.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ReviewStepComponent {
  public readonly draft = input.required<CreateProjectWizardDraft>();

  protected readonly layoutLabel = computed(() => {
    const preset = this.draft().layoutPreset;
    return preset ? LAYOUT_LABELS[preset] : undefined;
  });

  protected isRepoConfigured(repo: RepositoryDraft): boolean {
    return Boolean(repo.providerType && repo.repositoryUrl.trim() && repo.defaultBranch.trim());
  }
}
