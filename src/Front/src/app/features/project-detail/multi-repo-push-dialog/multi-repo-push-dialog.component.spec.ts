import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { By } from '@angular/platform-browser';
import { TranslateModule } from '@ngx-translate/core';

import { DsTextareaComponent } from '../../../shared/components/ds';
import { MultiRepoPushResponse } from '../../../shared/interfaces/multi-repo-push.interface';
import { ProjectService } from '../../../shared/services/project.service';
import {
  MultiRepoPushDialogComponent,
  MultiRepoPushDialogData,
} from './multi-repo-push-dialog.component';

interface MultiRepoPushDialogComponentTestApi {
  infraForm: {
    controls: {
      branch: { setValue(value: string): void };
      commit: { hasError(errorCode: string): boolean; markAsTouched(): void; setValue(value: string): void };
    };
  };
  codeForm: {
    controls: {
      branch: { setValue(value: string): void };
      commit: { hasError(errorCode: string): boolean; markAsTouched(): void; setValue(value: string): void };
    };
  };
  canPush: () => boolean;
  onPush(): Promise<void>;
}

function createPushResponse(): MultiRepoPushResponse {
  return {
    results: [
      {
        alias: 'infra-repo',
        success: true,
        branchUrl: 'https://example.test/infra',
        commitSha: '12345678',
        fileCount: 1,
        errorCode: null,
        errorDescription: null,
      },
      {
        alias: 'code-repo',
        success: true,
        branchUrl: 'https://example.test/code',
        commitSha: '87654321',
        fileCount: 1,
        errorCode: null,
        errorDescription: null,
      },
    ],
  };
}

describe('MultiRepoPushDialogComponent', () => {
  let fixture: ComponentFixture<MultiRepoPushDialogComponent>;
  let component: MultiRepoPushDialogComponent;
  let componentTestApi: MultiRepoPushDialogComponentTestApi;
  let projectServiceSpy: jasmine.SpyObj<ProjectService>;

  beforeEach(async () => {
    projectServiceSpy = jasmine.createSpyObj<ProjectService>('ProjectService', ['pushProjectArtifactsToMultiRepo']);
    projectServiceSpy.pushProjectArtifactsToMultiRepo.and.resolveTo(createPushResponse());

    await TestBed.configureTestingModule({
      imports: [MultiRepoPushDialogComponent, TranslateModule.forRoot()],
      providers: [
        {
          provide: MAT_DIALOG_DATA,
          useValue: {
            projectId: 'project-42',
            infraAlias: 'infra-repo',
            codeAlias: 'code-repo',
            mode: 'both',
          } satisfies MultiRepoPushDialogData,
        },
        {
          provide: MatDialogRef,
          useValue: jasmine.createSpyObj<MatDialogRef<MultiRepoPushDialogComponent>>('MatDialogRef', ['close']),
        },
        {
          provide: ProjectService,
          useValue: projectServiceSpy,
        },
        {
          provide: MatSnackBar,
          useValue: jasmine.createSpyObj<MatSnackBar>('MatSnackBar', ['open']),
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(MultiRepoPushDialogComponent);
    component = fixture.componentInstance;
    componentTestApi = component as unknown as MultiRepoPushDialogComponentTestApi;
    fixture.detectChanges();
  });

  afterEach(() => {
    localStorage.removeItem('ifs-push-branch-multi-project-42-infra-repo');
    localStorage.removeItem('ifs-push-branch-multi-project-42-code-repo');
  });

  it('requires commit messages before enabling push in both mode', () => {
    expect(componentTestApi.infraForm.controls.commit.hasError('required')).toBeTrue();
    expect(componentTestApi.codeForm.controls.commit.hasError('required')).toBeTrue();
    expect(componentTestApi.canPush()).toBeFalse();
  });

  it('passes required, hint, and error bindings to both commit textareas', () => {
    componentTestApi.infraForm.controls.commit.markAsTouched();
    componentTestApi.codeForm.controls.commit.markAsTouched();
    fixture.detectChanges();

    const textareaComponents = fixture.debugElement
      .queryAll(By.directive(DsTextareaComponent))
      .map(debugElement => debugElement.componentInstance as DsTextareaComponent);

    expect(textareaComponents.length).toBe(2);
    expect(textareaComponents[0].required()).toBeTrue();
    expect(textareaComponents[0].hint()).toBe('PROJECT_DETAIL.MULTI_REPO_PUSH.COMMIT_HINT');
    expect(textareaComponents[0].error()).toBe('PROJECT_DETAIL.MULTI_REPO_PUSH.COMMIT_REQUIRED_ERROR');
    expect(textareaComponents[1].required()).toBeTrue();
    expect(textareaComponents[1].hint()).toBe('PROJECT_DETAIL.MULTI_REPO_PUSH.COMMIT_HINT');
    expect(textareaComponents[1].error()).toBe('PROJECT_DETAIL.MULTI_REPO_PUSH.COMMIT_REQUIRED_ERROR');
  });

  it('does not call pushProjectArtifactsToMultiRepo when a commit message is missing', async () => {
    componentTestApi.infraForm.controls.branch.setValue('main');
    componentTestApi.codeForm.controls.branch.setValue('main');

    await componentTestApi.onPush();

    expect(projectServiceSpy.pushProjectArtifactsToMultiRepo).not.toHaveBeenCalled();
  });
});