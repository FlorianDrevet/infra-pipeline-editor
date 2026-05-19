import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { By } from '@angular/platform-browser';
import { TranslateModule } from '@ngx-translate/core';

import { DsAutocompleteComponent, DsTextareaComponent } from '../../../shared/components/ds';
import { GitBranchResponse } from '../../../shared/interfaces/project.interface';
import { MultiRepoPushResponse } from '../../../shared/interfaces/multi-repo-push.interface';
import { ProjectService } from '../../../shared/services/project.service';
import {
  MultiRepoPushDialogComponent,
  MultiRepoPushDialogData,
} from './multi-repo-push-dialog.component';

interface MultiRepoPushDialogComponentTestApi {
  state: { set(value: 'form' | 'pushing' | 'success' | 'partial' | 'error'): void };
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
  filteredInfraBranches: () => string[];
  filteredCodeBranches: () => string[];
  canPush: () => boolean;
  onPush(): Promise<void>;
}

function createBranchResponses(): GitBranchResponse[] {
  return [
    { name: 'main', isProtected: true },
    { name: 'release/1.0', isProtected: false },
    { name: 'feature/demo', isProtected: false },
  ];
}

function createPushResponse(): MultiRepoPushResponse {
  return {
    results: [
      {
        repositoryId: 'repo-infra',
        success: true,
        branchUrl: 'https://example.test/infra',
        commitSha: '12345678',
        fileCount: 1,
        errorCode: null,
        errorDescription: null,
      },
      {
        repositoryId: 'repo-code',
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
    projectServiceSpy = jasmine.createSpyObj<ProjectService>('ProjectService', ['pushProjectArtifactsToMultiRepo', 'listBranches']);
    projectServiceSpy.listBranches.and.resolveTo(createBranchResponses());
    projectServiceSpy.pushProjectArtifactsToMultiRepo.and.resolveTo(createPushResponse());

    await TestBed.configureTestingModule({
      imports: [MultiRepoPushDialogComponent, TranslateModule.forRoot()],
      providers: [
        {
          provide: MAT_DIALOG_DATA,
          useValue: {
            projectId: 'project-42',
            infraRepositoryId: 'repo-infra',
            codeRepositoryId: 'repo-code',
            infraRepositoryLabel: 'example/infra-repo',
            codeRepositoryLabel: 'example/code-repo',
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
    await fixture.whenStable();
    fixture.detectChanges();
  });

  afterEach(() => {
    localStorage.removeItem('ifs-push-branch-multi-project-42-repo-infra');
    localStorage.removeItem('ifs-push-branch-multi-project-42-repo-code');
  });

  it('requires commit messages before enabling push in both mode', () => {
    expect(componentTestApi.infraForm.controls.commit.hasError('required')).toBeTrue();
    expect(componentTestApi.codeForm.controls.commit.hasError('required')).toBeTrue();
    expect(componentTestApi.canPush()).toBeFalse();
  });

  it('enables push once both commit messages are provided', () => {
    componentTestApi.infraForm.controls.commit.setValue('chore: update infra artifacts');
    componentTestApi.codeForm.controls.commit.setValue('chore: update app artifacts');
    fixture.detectChanges();

    expect(componentTestApi.canPush()).toBeTrue();
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

  it('replaces commit textareas with dedicated loading states while pushing', () => {
    componentTestApi.state.set('pushing');
    fixture.detectChanges();

    const textareaComponents = fixture.debugElement.queryAll(By.directive(DsTextareaComponent));
    const loadingStates = fixture.debugElement.queryAll(By.css('.mr-card__state--loading'));

    expect(textareaComponents.length).toBe(0);
    expect(loadingStates.length).toBe(2);
  });

  it('does not call pushProjectArtifactsToMultiRepo when a commit message is missing', async () => {
    componentTestApi.infraForm.controls.branch.setValue('main');
    componentTestApi.codeForm.controls.branch.setValue('main');

    await componentTestApi.onPush();

    expect(projectServiceSpy.pushProjectArtifactsToMultiRepo).not.toHaveBeenCalled();
  });

  it('loads and filters existing branches for both repo branch fields', async () => {
    expect(projectServiceSpy.listBranches).toHaveBeenCalledWith('project-42');
    expect(componentTestApi.filteredInfraBranches()).toEqual(['main']);
    expect(componentTestApi.filteredCodeBranches()).toEqual(['main']);

    componentTestApi.infraForm.controls.branch.setValue('release');
    fixture.detectChanges();
    await fixture.whenStable();

    expect(componentTestApi.filteredInfraBranches()).toEqual(['release/1.0']);
    expect(componentTestApi.filteredCodeBranches()).toEqual(['main']);
  });

  it('renders one shared design-system autocomplete per visible repo card', () => {
    const autocompleteComponents = fixture.debugElement.queryAll(By.directive(DsAutocompleteComponent));

    expect(autocompleteComponents.length).toBe(2);
  });

  it('pushes using repository ids', async () => {
    componentTestApi.infraForm.controls.commit.setValue('chore: update infra artifacts');
    componentTestApi.codeForm.controls.commit.setValue('chore: update app artifacts');
    fixture.detectChanges();

    await componentTestApi.onPush();

    expect(projectServiceSpy.pushProjectArtifactsToMultiRepo).toHaveBeenCalledOnceWith('project-42', {
      infra: {
        repositoryId: 'repo-infra',
        branchName: 'main',
        commitMessage: 'chore: update infra artifacts',
      },
      code: {
        repositoryId: 'repo-code',
        branchName: 'main',
        commitMessage: 'chore: update app artifacts',
      },
    });
  });
});