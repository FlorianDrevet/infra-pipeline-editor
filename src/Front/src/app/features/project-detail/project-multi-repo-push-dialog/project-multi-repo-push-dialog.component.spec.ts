import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { By } from '@angular/platform-browser';
import { TranslateModule } from '@ngx-translate/core';

import { DsTextareaComponent } from '../../../shared/components/ds';
import { InfrastructureConfigResponse } from '../../../shared/interfaces/infra-config.interface';
import { ProjectMultiRepoPushResponse } from '../../../shared/interfaces/project-multi-repo-push.interface';
import { ProjectService } from '../../../shared/services/project.service';
import {
  ProjectMultiRepoPushDialogComponent,
  ProjectMultiRepoPushDialogData,
} from './project-multi-repo-push-dialog.component';

interface ProjectMultiRepoPushDialogComponentTestApi {
  state: () => 'form' | 'pushing' | 'success' | 'partial' | 'error';
  canPush: () => boolean;
  formFor(targetKey: string): {
    controls: {
      branch: { value: string; setValue(value: string): void };
      commit: { value: string; setValue(value: string): void };
    };
  };
  onPush(): Promise<void>;
}

const PROJECT_ID = 'project-1';
const CONFIG_ID = 'config-1';
const INFRA_REPOSITORY_ID = 'infra-repository-1';
const APP_REPOSITORY_ID = 'app-repository-1';

describe('ProjectMultiRepoPushDialogComponent', () => {
  let fixture: ComponentFixture<ProjectMultiRepoPushDialogComponent>;
  let componentTestApi: ProjectMultiRepoPushDialogComponentTestApi;
  let projectServiceSpy: jasmine.SpyObj<ProjectService>;

  beforeEach(async () => {
    projectServiceSpy = jasmine.createSpyObj<ProjectService>(
      'ProjectService',
      ['pushProjectMultiRepoArtifacts'],
    );

    await TestBed.configureTestingModule({
      imports: [ProjectMultiRepoPushDialogComponent, TranslateModule.forRoot()],
      providers: [
        {
          provide: MAT_DIALOG_DATA,
          useValue: {
            projectId: PROJECT_ID,
            configurations: [createSplitConfiguration()],
          } satisfies ProjectMultiRepoPushDialogData,
        },
        {
          provide: MatDialogRef,
          useValue: jasmine.createSpyObj<MatDialogRef<ProjectMultiRepoPushDialogComponent>>('MatDialogRef', ['close']),
        },
        {
          provide: ProjectService,
          useValue: projectServiceSpy,
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(ProjectMultiRepoPushDialogComponent);
    componentTestApi = fixture.componentInstance as unknown as ProjectMultiRepoPushDialogComponentTestApi;
    fixture.detectChanges();
  });

  it('requires a commit message for every configuration repository', () => {
    const textareas = fixture.debugElement.queryAll(By.directive(DsTextareaComponent));

    expect(textareas.length).toBe(2);
    expect(componentTestApi.formFor(`${CONFIG_ID}:${INFRA_REPOSITORY_ID}`).controls.commit.value).toBe('');
    expect(componentTestApi.formFor(`${CONFIG_ID}:${APP_REPOSITORY_ID}`).controls.commit.value).toBe('');
    expect(componentTestApi.canPush()).toBeFalse();
  });

  it('builds the project bulk request with configuration-owned repository ids', async () => {
    const response: ProjectMultiRepoPushResponse = { results: [] };
    projectServiceSpy.pushProjectMultiRepoArtifacts.and.resolveTo(response);

    const infraForm = componentTestApi.formFor(`${CONFIG_ID}:${INFRA_REPOSITORY_ID}`);
    const appForm = componentTestApi.formFor(`${CONFIG_ID}:${APP_REPOSITORY_ID}`);
    infraForm.controls.commit.setValue('Push infra artifacts');
    appForm.controls.commit.setValue('Push app artifacts');
    fixture.detectChanges();

    expect(componentTestApi.canPush()).toBeTrue();
    await componentTestApi.onPush();

    expect(projectServiceSpy.pushProjectMultiRepoArtifacts).toHaveBeenCalledOnceWith(PROJECT_ID, {
      configurations: [
        {
          infrastructureConfigId: CONFIG_ID,
          repositories: [
            {
              repositoryId: INFRA_REPOSITORY_ID,
              branchName: 'main',
              commitMessage: 'Push infra artifacts',
            },
            {
              repositoryId: APP_REPOSITORY_ID,
              branchName: 'develop',
              commitMessage: 'Push app artifacts',
            },
          ],
        },
      ],
    });
  });

  it('shows a partial state when one independent repository push fails', async () => {
    projectServiceSpy.pushProjectMultiRepoArtifacts.and.resolveTo({
      results: [
        {
          infrastructureConfigId: CONFIG_ID,
          repositoryId: INFRA_REPOSITORY_ID,
          success: true,
          branchUrl: 'https://example.test/infra',
          commitSha: 'infra-sha',
          fileCount: 3,
          errorCode: null,
          errorDescription: null,
        },
        {
          infrastructureConfigId: CONFIG_ID,
          repositoryId: APP_REPOSITORY_ID,
          success: false,
          branchUrl: null,
          commitSha: null,
          fileCount: 0,
          errorCode: 'GitRepository.PushFailed',
          errorDescription: 'Application repository rejected the push.',
        },
      ],
    });

    componentTestApi.formFor(`${CONFIG_ID}:${INFRA_REPOSITORY_ID}`).controls.commit.setValue('Push infra');
    componentTestApi.formFor(`${CONFIG_ID}:${APP_REPOSITORY_ID}`).controls.commit.setValue('Push app');
    fixture.detectChanges();

    await componentTestApi.onPush();

    expect(componentTestApi.state()).toBe('partial');
  });
});

function createSplitConfiguration(): InfrastructureConfigResponse {
  return {
    id: CONFIG_ID,
    name: 'Core infrastructure',
    defaultNamingTemplate: null,
    projectId: PROJECT_ID,
    useProjectNamingConventions: true,
    resourceNamingTemplates: [],
    resourceAbbreviationOverrides: [],
    resourceGroupCount: 1,
    resourceCount: 3,
    crossConfigReferenceCount: 0,
    appPipelineMode: 'Isolated',
    tags: [],
    layoutMode: 'SplitInfraCode',
    repositories: [
      {
        id: INFRA_REPOSITORY_ID,
        providerType: 'GitHub',
        repositoryUrl: 'https://github.com/example/infra',
        owner: 'example',
        repositoryName: 'infra',
        defaultBranch: 'main',
        contentKinds: ['Infrastructure'],
      },
      {
        id: APP_REPOSITORY_ID,
        providerType: 'GitHub',
        repositoryUrl: 'https://github.com/example/app',
        owner: 'example',
        repositoryName: 'app',
        defaultBranch: 'develop',
        contentKinds: ['ApplicationCode'],
      },
    ],
  };
}