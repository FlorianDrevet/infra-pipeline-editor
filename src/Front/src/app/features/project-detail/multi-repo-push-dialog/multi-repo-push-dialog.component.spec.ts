import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTooltip } from '@angular/material/tooltip';
import { By } from '@angular/platform-browser';
import { TranslateModule } from '@ngx-translate/core';

import { DsAutocompleteComponent, DsAutocompleteOption, DsButtonComponent, DsTextareaComponent } from '../../../shared/components/ds';
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
      branch: { value: string; setValue(value: string): void };
      commit: { hasError(errorCode: string): boolean; markAsTouched(): void; setValue(value: string): void };
    };
  };
  codeForm: {
    controls: {
      branch: { value: string; setValue(value: string): void };
      commit: { hasError(errorCode: string): boolean; markAsTouched(): void; setValue(value: string): void };
    };
  };
  branchesLoading: () => boolean;
  infraBranchOptions: () => DsAutocompleteOption<string>[];
  codeBranchOptions: () => DsAutocompleteOption<string>[];
  filteredInfraBranches: () => string[];
  filteredCodeBranches: () => string[];
  canPush: () => boolean;
  showAllInfraBranches(): void;
  onPush(): Promise<void>;
}

interface DeferredPromise<TValue> {
  promise: Promise<TValue>;
  resolve(value: TValue): void;
}

interface ComponentSetupOptions {
  branchesPromise?: Promise<GitBranchResponse[]>;
  codeBranchesPromise?: Promise<GitBranchResponse[]>;
  waitForBranches?: boolean;
}

function createDeferredPromise<TValue>(): DeferredPromise<TValue> {
  let resolvePromise: (value: TValue | PromiseLike<TValue>) => void = () => {};
  const promise = new Promise<TValue>((resolve) => {
    resolvePromise = resolve;
  });

  return {
    promise,
    resolve: resolvePromise,
  };
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

  async function createComponent(options: ComponentSetupOptions = {}): Promise<void> {
    projectServiceSpy.listBranches.and.returnValue(options.branchesPromise ?? Promise.resolve(createBranchResponses()));
    projectServiceSpy.listCodeBranches.and.returnValue(options.codeBranchesPromise ?? Promise.resolve(createBranchResponses()));

    fixture = TestBed.createComponent(MultiRepoPushDialogComponent);
    component = fixture.componentInstance;
    componentTestApi = component as unknown as MultiRepoPushDialogComponentTestApi;
    fixture.detectChanges();

    if (options.waitForBranches ?? true) {
      await fixture.whenStable();
      fixture.detectChanges();
    }
  }

  beforeEach(async () => {
    localStorage.removeItem('ifs-push-branch-multi-project-42-repo-infra');
    localStorage.removeItem('ifs-push-branch-multi-project-42-repo-code');

    projectServiceSpy = jasmine.createSpyObj<ProjectService>('ProjectService', ['pushProjectArtifactsToMultiRepo', 'listBranches', 'listCodeBranches']);
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
  });

  afterEach(() => {
    localStorage.removeItem('ifs-push-branch-multi-project-42-repo-infra');
    localStorage.removeItem('ifs-push-branch-multi-project-42-repo-code');
  });

  it('requires commit messages before enabling push in both mode', async () => {
    await createComponent();

    expect(componentTestApi.infraForm.controls.commit.hasError('required')).toBeTrue();
    expect(componentTestApi.codeForm.controls.commit.hasError('required')).toBeTrue();
    expect(componentTestApi.canPush()).toBeFalse();
  });

  it('enables push once both commit messages are provided', async () => {
    await createComponent();

    componentTestApi.infraForm.controls.commit.setValue('chore: update infra artifacts');
    componentTestApi.codeForm.controls.commit.setValue('chore: update app artifacts');
    fixture.detectChanges();

    expect(componentTestApi.canPush()).toBeTrue();
  });

  it('passes required, hint, and error bindings to both commit textareas', async () => {
    await createComponent();

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

  it('replaces commit textareas with dedicated loading states while pushing', async () => {
    await createComponent();

    componentTestApi.state.set('pushing');
    fixture.detectChanges();

    const textareaComponents = fixture.debugElement.queryAll(By.directive(DsTextareaComponent));
    const loadingStates = fixture.debugElement.queryAll(By.css('.mr-card__state--loading'));

    expect(textareaComponents.length).toBe(0);
    expect(loadingStates.length).toBe(2);
  });

  it('does not call pushProjectArtifactsToMultiRepo when a commit message is missing', async () => {
    await createComponent();

    componentTestApi.infraForm.controls.branch.setValue('main');
    componentTestApi.codeForm.controls.branch.setValue('main');

    await componentTestApi.onPush();

    expect(projectServiceSpy.pushProjectArtifactsToMultiRepo).not.toHaveBeenCalled();
  });

  it('loads and filters existing branches for both repo branch fields', async () => {
    await createComponent();

    expect(projectServiceSpy.listBranches).toHaveBeenCalledWith('project-42');
    expect(projectServiceSpy.listCodeBranches).toHaveBeenCalledWith('project-42');
    expect(componentTestApi.filteredInfraBranches()).toEqual(['main']);
    expect(componentTestApi.filteredCodeBranches()).toEqual(['main']);

    componentTestApi.infraForm.controls.branch.setValue('release');
    fixture.detectChanges();
    await fixture.whenStable();

    expect(componentTestApi.filteredInfraBranches()).toEqual(['release/1.0']);
    expect(componentTestApi.filteredCodeBranches()).toEqual(['main']);
  });

  it('renders one shared design-system autocomplete per visible repo card', async () => {
    await createComponent();

    const autocompleteComponents = fixture.debugElement.queryAll(By.directive(DsAutocompleteComponent));

    expect(autocompleteComponents.length).toBe(2);
  });

  it('starts branch loading on open without displaying a default branch before the fetch resolves', async () => {
    const branchesDeferred = createDeferredPromise<GitBranchResponse[]>();

    await createComponent({ branchesPromise: branchesDeferred.promise, codeBranchesPromise: branchesDeferred.promise, waitForBranches: false });

    expect(projectServiceSpy.listBranches).toHaveBeenCalledOnceWith('project-42');
    expect(projectServiceSpy.listCodeBranches).toHaveBeenCalledOnceWith('project-42');
    expect(componentTestApi.branchesLoading()).toBeTrue();
    expect(componentTestApi.infraForm.controls.branch.value).toBe('');
    expect(componentTestApi.codeForm.controls.branch.value).toBe('');
    expect(componentTestApi.infraBranchOptions()).toEqual([]);
    expect(componentTestApi.codeBranchOptions()).toEqual([]);

    const autocompleteComponents = fixture.debugElement
      .queryAll(By.directive(DsAutocompleteComponent))
      .map(debugElement => debugElement.componentInstance as DsAutocompleteComponent);
    const pushButton = fixture.debugElement.query(By.directive(DsButtonComponent)).componentInstance as DsButtonComponent;

    expect(autocompleteComponents.length).toBe(2);
    expect(autocompleteComponents.every(autocomplete => autocomplete.loading())).toBeTrue();
    expect(autocompleteComponents.every(autocomplete => autocomplete.options().length === 0)).toBeTrue();

    componentTestApi.infraForm.controls.commit.setValue('chore: update infra artifacts');
    componentTestApi.codeForm.controls.commit.setValue('chore: update app artifacts');
    fixture.detectChanges();

    expect(componentTestApi.canPush()).toBeFalse();
    expect(pushButton.disabled()).toBeTrue();

    branchesDeferred.resolve(createBranchResponses());
    await new Promise(resolve => setTimeout(resolve));
    await fixture.whenStable();
    fixture.detectChanges();

    expect(componentTestApi.branchesLoading()).toBeFalse();
    expect(componentTestApi.infraForm.controls.branch.value).toBe('main');
    expect(componentTestApi.codeForm.controls.branch.value).toBe('main');
    expect(componentTestApi.canPush()).toBeTrue();
  });

  it('applies stored branches only after branches have loaded and keeps focus showing all options', async () => {
    localStorage.setItem('ifs-push-branch-multi-project-42-repo-infra', 'release/1.0');
    localStorage.setItem('ifs-push-branch-multi-project-42-repo-code', 'feature/demo');
    const branchesDeferred = createDeferredPromise<GitBranchResponse[]>();

    await createComponent({ branchesPromise: branchesDeferred.promise, codeBranchesPromise: branchesDeferred.promise, waitForBranches: false });

    expect(componentTestApi.infraForm.controls.branch.value).toBe('');
    expect(componentTestApi.codeForm.controls.branch.value).toBe('');

    branchesDeferred.resolve(createBranchResponses());
    await new Promise(resolve => setTimeout(resolve));
    await fixture.whenStable();
    fixture.detectChanges();

    expect(componentTestApi.infraForm.controls.branch.value).toBe('release/1.0');
    expect(componentTestApi.codeForm.controls.branch.value).toBe('feature/demo');
    expect(componentTestApi.filteredInfraBranches()).toEqual(['release/1.0']);

    componentTestApi.showAllInfraBranches();

    expect(componentTestApi.filteredInfraBranches()).toEqual(['main', 'release/1.0', 'feature/demo']);
  });

  it('keeps branch fields empty and usable when branch loading fails', async () => {
    const failedPromise = Promise.reject(new Error('Branch lookup failed'));
    await createComponent({
      branchesPromise: failedPromise,
      codeBranchesPromise: failedPromise,
    });

    expect(componentTestApi.branchesLoading()).toBeFalse();
    expect(componentTestApi.infraForm.controls.branch.value).toBe('');
    expect(componentTestApi.codeForm.controls.branch.value).toBe('');
    expect(componentTestApi.infraBranchOptions()).toEqual([]);

    componentTestApi.infraForm.controls.branch.setValue('hotfix/manual');
    componentTestApi.codeForm.controls.branch.setValue('hotfix/manual');
    componentTestApi.infraForm.controls.commit.setValue('chore: update infra artifacts');
    componentTestApi.codeForm.controls.commit.setValue('chore: update app artifacts');
    fixture.detectChanges();

    expect(componentTestApi.canPush()).toBeTrue();
  });

  it('anchors the mode header and exposes full repository labels as tooltips', async () => {
    await createComponent();

    const header = fixture.debugElement.query(By.css('.mr-dialog__header'));
    const repositoryLabels = fixture.debugElement.queryAll(By.css('.mr-card__repository'));

    expect(header).not.toBeNull();
    expect(header.nativeElement.textContent).toContain('PROJECT_DETAIL.MULTI_REPO_PUSH.TITLE');
    expect(header.nativeElement.textContent).toContain('PROJECT_DETAIL.MULTI_REPO_PUSH.SUBTITLE');
    expect(repositoryLabels[0].injector.get(MatTooltip).message).toBe('example/infra-repo');
    expect(repositoryLabels[1].injector.get(MatTooltip).message).toBe('example/code-repo');
  });

  it('pushes using repository ids', async () => {
    await createComponent();

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