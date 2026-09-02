import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { TranslateModule } from '@ngx-translate/core';

import {
  DsAutocompleteComponent,
  DsAutocompleteOption,
  DsBannerComponent,
  DsButtonComponent,
  DsCardMatComponent,
  DsSpinnerComponent,
  DsTextareaComponent,
} from '../../../shared/components/ds';
import { InfrastructureConfigResponse } from '../../../shared/interfaces/infra-config.interface';
import {
  ProjectMultiRepoPushConfigurationTarget,
  ProjectMultiRepoPushRequest,
  ProjectMultiRepoPushResult,
} from '../../../shared/interfaces/project-multi-repo-push.interface';
import { RepositoryContentKind } from '../../../shared/interfaces/project-repository.interface';
import { ProjectService } from '../../../shared/services/project.service';

export interface ProjectMultiRepoPushDialogData {
  projectId: string;
  configurations: InfrastructureConfigResponse[];
}

type DialogState = 'form' | 'pushing' | 'success' | 'partial' | 'error';

interface ProjectMultiRepoPushTarget {
  key: string;
  infrastructureConfigId: string;
  configurationName: string;
  repositoryId: string;
  repositoryLabel: string;
  defaultBranch: string;
  contentKinds: RepositoryContentKind[];
}

type PushForm = FormGroup<{
  branch: FormControl<string>;
  commit: FormControl<string>;
}>;

const DEFAULT_BRANCH_NAME = 'main';
const BRANCH_STORAGE_PREFIX = 'ifs-push-branch-project-multi';
const INFRASTRUCTURE_CONTENT_KIND: RepositoryContentKind = 'Infrastructure';
const APPLICATION_CODE_CONTENT_KIND: RepositoryContentKind = 'ApplicationCode';

const CONTENT_KIND_LABEL_KEYS: Record<RepositoryContentKind, string> = {
  [INFRASTRUCTURE_CONTENT_KIND]: 'PROJECT_DETAIL.MULTI_REPO_BULK_PUSH.INFRASTRUCTURE_KIND',
  [APPLICATION_CODE_CONTENT_KIND]: 'PROJECT_DETAIL.MULTI_REPO_BULK_PUSH.APPLICATION_KIND',
};

@Component({
  selector: 'app-project-multi-repo-push-dialog',
  standalone: true,
  imports: [
    DsAutocompleteComponent,
    DsBannerComponent,
    DsButtonComponent,
    DsCardMatComponent,
    DsSpinnerComponent,
    DsTextareaComponent,
    MatDialogModule,
    MatIconModule,
    ReactiveFormsModule,
    TranslateModule,
  ],
  templateUrl: './project-multi-repo-push-dialog.component.html',
  styleUrl: './project-multi-repo-push-dialog.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProjectMultiRepoPushDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<ProjectMultiRepoPushDialogComponent>);
  protected readonly data = inject(MAT_DIALOG_DATA) as ProjectMultiRepoPushDialogData;
  private readonly projectService = inject(ProjectService);

  protected readonly state = signal<DialogState>('form');
  protected readonly errorKey = signal('');
  protected readonly targets = this.buildTargets(this.data.configurations);

  private readonly formsByTargetKey = new Map<string, PushForm>();
  private readonly resultsByTargetKey = signal<Map<string, ProjectMultiRepoPushResult>>(new Map());
  private readonly formsRevision = signal(0);

  protected readonly canPush = computed(() => {
    this.formsRevision();
    return this.state() !== 'pushing'
      && this.targets.length > 0
      && this.targets.every(target => this.formFor(target.key).valid);
  });

  public constructor() {
    for (const target of this.targets) {
      const form = this.createForm(target);
      this.formsByTargetKey.set(target.key, form);
      form.statusChanges
        .pipe(takeUntilDestroyed())
        .subscribe(() => this.formsRevision.update(revision => revision + 1));
    }
  }

  protected formFor(targetKey: string): PushForm {
    const form = this.formsByTargetKey.get(targetKey);
    if (!form) {
      throw new Error(`No push form exists for target '${targetKey}'.`);
    }

    return form;
  }

  protected resultFor(targetKey: string): ProjectMultiRepoPushResult | null {
    return this.resultsByTargetKey().get(targetKey) ?? null;
  }

  protected branchOptions(target: ProjectMultiRepoPushTarget): DsAutocompleteOption<string>[] {
    return [{ value: target.defaultBranch, label: target.defaultBranch }];
  }

  protected contentKindLabelKey(contentKind: RepositoryContentKind): string {
    return CONTENT_KIND_LABEL_KEYS[contentKind];
  }

  protected async onPush(): Promise<void> {
    if (!this.canPush()) return;

    this.state.set('pushing');
    this.errorKey.set('');
    this.resultsByTargetKey.set(new Map());

    const request: ProjectMultiRepoPushRequest = {
      configurations: this.buildRequestConfigurations(),
    };

    try {
      const response = await this.projectService.pushProjectMultiRepoArtifacts(
        this.data.projectId,
        request,
      );
      const results = new Map<string, ProjectMultiRepoPushResult>();
      for (const result of response.results) {
        results.set(this.targetKey(result.infrastructureConfigId, result.repositoryId), result);
      }
      this.resultsByTargetKey.set(results);

      const successfulPushes = this.targets.filter(target => this.resultFor(target.key)?.success === true).length;
      if (successfulPushes === this.targets.length) {
        this.state.set('success');
      } else if (successfulPushes > 0) {
        this.state.set('partial');
      } else {
        this.state.set('error');
      }
    } catch {
      this.errorKey.set('PROJECT_DETAIL.MULTI_REPO_BULK_PUSH.ERROR_DESC');
      this.state.set('error');
    }
  }

  protected onRetry(): void {
    this.state.set('form');
    this.resultsByTargetKey.set(new Map());
    this.errorKey.set('');
  }

  protected onClose(): void {
    this.dialogRef.close();
  }

  private createForm(target: ProjectMultiRepoPushTarget): PushForm {
    return new FormGroup({
      branch: new FormControl<string>(this.readPreferredBranch(target), {
        nonNullable: true,
        validators: [Validators.required],
      }),
      commit: new FormControl<string>('', {
        nonNullable: true,
        validators: [Validators.required],
      }),
    });
  }

  private buildTargets(
    configurations: readonly InfrastructureConfigResponse[],
  ): readonly ProjectMultiRepoPushTarget[] {
    return configurations.flatMap(configuration =>
      (configuration.repositories ?? []).map(repository => ({
        key: this.targetKey(configuration.id, repository.id),
        infrastructureConfigId: configuration.id,
        configurationName: configuration.name,
        repositoryId: repository.id,
        repositoryLabel: repository.repositoryUrl ?? repository.repositoryName ?? repository.id,
        defaultBranch: repository.defaultBranch ?? DEFAULT_BRANCH_NAME,
        contentKinds: repository.contentKinds,
      })));
  }

  private buildRequestConfigurations(): ProjectMultiRepoPushConfigurationTarget[] {
    return this.data.configurations
      .map(configuration => ({
        infrastructureConfigId: configuration.id,
        repositories: (configuration.repositories ?? []).map(repository => {
          const targetKey = this.targetKey(configuration.id, repository.id);
          const form = this.formFor(targetKey);
          const branchName = form.controls.branch.value;
          localStorage.setItem(this.storageKey(targetKey), branchName);

          return {
            repositoryId: repository.id,
            branchName,
            commitMessage: form.controls.commit.value,
          };
        }),
      }))
      .filter(configuration => configuration.repositories.length > 0);
  }

  private readPreferredBranch(target: ProjectMultiRepoPushTarget): string {
    return localStorage.getItem(this.storageKey(target.key)) ?? target.defaultBranch;
  }

  private storageKey(targetKey: string): string {
    return `${BRANCH_STORAGE_PREFIX}-${this.data.projectId}-${targetKey}`;
  }

  private targetKey(configurationId: string, repositoryId: string): string {
    return `${configurationId}:${repositoryId}`;
  }
}