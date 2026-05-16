import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { TranslateModule } from '@ngx-translate/core';
import { Observable, Subject } from 'rxjs';

import { ProjectResponse } from '../../../shared/interfaces/project.interface';
import {
  ProjectLayoutPreset,
  ProjectRepositoryResponse,
  RepositoryContentKind,
} from '../../../shared/interfaces/project-repository.interface';
import { ProjectService } from '../../../shared/services/project.service';
import { LayoutRepositoriesComponent } from './layout-repositories.component';

interface DeferredPromise<T> {
  readonly promise: Promise<T>;
  resolve(value: T | PromiseLike<T>): void;
  reject(reason?: unknown): void;
}

interface LayoutRepositoriesComponentTestApi {
  readonly currentPreset: () => ProjectLayoutPreset;
  readonly onPresetChange: (preset: ProjectLayoutPreset) => Promise<void>;
  readonly openAllInOneDialog: () => void;
  readonly openSlotDialog: (slot: LayoutRepositoriesRepoSlot) => void;
  readonly openRemoveRepoDialog: (repo: ProjectRepositoryResponse) => void;
  readonly splitSlots: () => LayoutRepositoriesRepoSlot[];
  readonly presetChanged: {
    subscribe(callback: (value: ProjectLayoutPreset) => void): { unsubscribe(): void };
  };
  readonly projectChanged: {
    subscribe(callback: (value: ProjectResponse) => void): { unsubscribe(): void };
  };
}

interface LayoutRepositoriesRepoSlot {
  readonly kind: RepositoryContentKind;
  readonly labelKey: string;
  readonly repo: ProjectRepositoryResponse | null;
}

describe('LayoutRepositoriesComponent', () => {
  let fixture: ComponentFixture<LayoutRepositoriesComponent>;
  let component: LayoutRepositoriesComponent;
  let dialogSpy: jasmine.SpyObj<MatDialog>;
  let projectServiceSpy: jasmine.SpyObj<ProjectService>;
  let snackBarSpy: jasmine.SpyObj<MatSnackBar>;
  let projectResponse: ProjectResponse;
  let loadSequence: string[];

  beforeEach(async () => {
    projectResponse = createProjectResponse('AllInOne');
    loadSequence = [];
    projectServiceSpy = jasmine.createSpyObj<ProjectService>('ProjectService', [
      'getProject',
      'setLayoutPreset',
      'invalidateProjectCache',
      'removeRepository',
    ]);
    dialogSpy = jasmine.createSpyObj<MatDialog>('MatDialog', ['open']);
    snackBarSpy = jasmine.createSpyObj<MatSnackBar>('MatSnackBar', ['open']);

    projectServiceSpy.getProject.and.callFake(async () => {
      loadSequence.push('getProject');
      return projectResponse;
    });
    projectServiceSpy.setLayoutPreset.and.resolveTo();
    projectServiceSpy.invalidateProjectCache.and.callFake(() => {
      loadSequence.push('invalidateProjectCache');
    });
    projectServiceSpy.removeRepository.and.callFake(async () => {
      loadSequence.push('removeRepository');
    });

    await TestBed.configureTestingModule({
      imports: [LayoutRepositoriesComponent, TranslateModule.forRoot()],
      providers: [
        { provide: ProjectService, useValue: projectServiceSpy },
        { provide: MatDialog, useValue: dialogSpy },
        { provide: MatSnackBar, useValue: snackBarSpy },
      ],
    }).compileComponents();
  });

  it('reflects the selected preset immediately before the save round-trip resolves', async () => {
    const saveDeferred = createDeferredPromise<void>();
    const emittedPresets: ProjectLayoutPreset[] = [];

    projectServiceSpy.setLayoutPreset.and.returnValue(saveDeferred.promise);
    await createComponent();
    getComponentTestApi().presetChanged.subscribe((preset) => emittedPresets.push(preset));

    const changeTask = getComponentTestApi().onPresetChange('SplitInfraCode');
    fixture.detectChanges();

    expect(getComponentTestApi().currentPreset()).toBe('SplitInfraCode');
    expect(getSelectedPresetButtonText()).toContain('PROJECT_DETAIL.LAYOUT.PRESET_SPLIT_INFRA_CODE');
    expect(emittedPresets).toEqual(['SplitInfraCode']);
    expect(projectServiceSpy.invalidateProjectCache).not.toHaveBeenCalled();

    projectResponse = createProjectResponse('SplitInfraCode');
    saveDeferred.resolve();
    await changeTask;
    fixture.detectChanges();

    expect(projectServiceSpy.setLayoutPreset).toHaveBeenCalledOnceWith('project-1', 'SplitInfraCode');
    expect(projectServiceSpy.invalidateProjectCache).toHaveBeenCalledOnceWith('project-1');
    expect(projectServiceSpy.getProject).toHaveBeenCalledTimes(2);
    expect(getComponentTestApi().currentPreset()).toBe('SplitInfraCode');
  });

  it('reverts the optimistic preset and re-emits the previous value when saving fails', async () => {
    const saveDeferred = createDeferredPromise<void>();
    const emittedPresets: ProjectLayoutPreset[] = [];

    projectServiceSpy.setLayoutPreset.and.returnValue(saveDeferred.promise);
    await createComponent();
    getComponentTestApi().presetChanged.subscribe((preset) => emittedPresets.push(preset));

    const changeTask = getComponentTestApi().onPresetChange('MultiRepo');
    fixture.detectChanges();

    expect(getComponentTestApi().currentPreset()).toBe('MultiRepo');
    expect(getSelectedPresetButtonText()).toContain('PROJECT_DETAIL.LAYOUT.PRESET_MULTI_REPO');

    saveDeferred.reject(new Error('preset save failed'));
    await changeTask;
    fixture.detectChanges();

    expect(projectServiceSpy.invalidateProjectCache).not.toHaveBeenCalled();
    expect(getComponentTestApi().currentPreset()).toBe('AllInOne');
    expect(getSelectedPresetButtonText()).toContain('PROJECT_DETAIL.LAYOUT.PRESET_ALL_IN_ONE');
    expect(emittedPresets).toEqual(['MultiRepo', 'AllInOne']);
    expect(snackBarSpy.open).toHaveBeenCalledTimes(1);
  });

  it('invalidates cache and emits the refreshed project when the all-in-one dialog saves', async () => {
    const closeSubject = new Subject<boolean | undefined>();
    const emittedProjects: ProjectResponse[] = [];

    dialogSpy.open.and.returnValue(createDialogRef(closeSubject.asObservable()));
    await createComponent();
    resetRefreshTracking();

    getComponentTestApi().projectChanged.subscribe((project) => emittedProjects.push(project));

    const refreshedProject = createProjectResponse('AllInOne', [
      createRepositoryResponse('repo-1', ['Infrastructure', 'ApplicationCode']),
    ]);

    getComponentTestApi().openAllInOneDialog();
    projectResponse = refreshedProject;
    closeSubject.next(true);
    closeSubject.complete();
    await settleComponent();

    expect(projectServiceSpy.invalidateProjectCache).toHaveBeenCalledOnceWith('project-1');
    expect(projectServiceSpy.getProject).toHaveBeenCalledOnceWith('project-1');
    expect(loadSequence).toEqual(['invalidateProjectCache', 'getProject']);
    expect(emittedProjects).toEqual([refreshedProject]);
  });

  it('invalidates cache and emits the refreshed project when a split slot dialog saves', async () => {
    const closeSubject = new Subject<boolean | undefined>();
    const emittedProjects: ProjectResponse[] = [];

    projectResponse = createProjectResponse('SplitInfraCode');
    dialogSpy.open.and.returnValue(createDialogRef(closeSubject.asObservable()));
    await createComponent();
    resetRefreshTracking();

    getComponentTestApi().projectChanged.subscribe((project) => emittedProjects.push(project));

    const infrastructureSlot = getComponentTestApi()
      .splitSlots()
      .find((slot) => slot.kind === 'Infrastructure');

    expect(infrastructureSlot).withContext('expected infrastructure slot').toBeDefined();
    if (!infrastructureSlot) {
      fail('Expected infrastructure slot to be defined');
      return;
    }

    const refreshedProject = createProjectResponse('SplitInfraCode', [
      createRepositoryResponse('repo-1', ['Infrastructure']),
    ]);

    getComponentTestApi().openSlotDialog(infrastructureSlot);
    projectResponse = refreshedProject;
    closeSubject.next(true);
    closeSubject.complete();
    await settleComponent();

    expect(projectServiceSpy.invalidateProjectCache).toHaveBeenCalledOnceWith('project-1');
    expect(projectServiceSpy.getProject).toHaveBeenCalledOnceWith('project-1');
    expect(loadSequence).toEqual(['invalidateProjectCache', 'getProject']);
    expect(emittedProjects).toEqual([refreshedProject]);
  });

  it('invalidates cache and emits the refreshed project after deleting a repository', async () => {
    const closeSubject = new Subject<boolean | undefined>();
    const emittedProjects: ProjectResponse[] = [];
    const existingRepository = createRepositoryResponse('repo-1', ['Infrastructure']);

    projectResponse = createProjectResponse('SplitInfraCode', [existingRepository]);
    dialogSpy.open.and.returnValue(createDialogRef(closeSubject.asObservable()));
    await createComponent();
    resetRefreshTracking();

    getComponentTestApi().projectChanged.subscribe((project) => emittedProjects.push(project));

    const refreshedProject = createProjectResponse('SplitInfraCode');

    getComponentTestApi().openRemoveRepoDialog(existingRepository);
    projectResponse = refreshedProject;
    closeSubject.next(true);
    closeSubject.complete();
    await settleComponent();

    expect(projectServiceSpy.removeRepository).toHaveBeenCalledOnceWith('project-1', 'repo-1');
    expect(projectServiceSpy.invalidateProjectCache).toHaveBeenCalledOnceWith('project-1');
    expect(projectServiceSpy.getProject).toHaveBeenCalledOnceWith('project-1');
    expect(loadSequence).toEqual([
      'removeRepository',
      'invalidateProjectCache',
      'getProject',
    ]);
    expect(emittedProjects).toEqual([refreshedProject]);
  });

  async function createComponent(): Promise<void> {
    fixture = TestBed.createComponent(LayoutRepositoriesComponent);
    fixture.componentRef.setInput('projectId', 'project-1');
    component = fixture.componentInstance;
    setDialogSpy();
    fixture.detectChanges();
    await fixture.whenStable();
    await fixture.whenRenderingDone();
    fixture.detectChanges();
  }

  function getComponentTestApi(): LayoutRepositoriesComponentTestApi {
    return component as unknown as LayoutRepositoriesComponentTestApi;
  }

  function getSelectedPresetButtonText(): string {
    const selectedButton = fixture.nativeElement.querySelector(
      '.ds-option-card--selected'
    ) as HTMLButtonElement | null;

    expect(selectedButton).withContext('expected one selected preset card').not.toBeNull();
    return selectedButton?.textContent?.trim() ?? '';
  }

  async function settleComponent(): Promise<void> {
    await Promise.resolve();
    await fixture.whenStable();
    await fixture.whenRenderingDone();
    fixture.detectChanges();
  }

  function resetRefreshTracking(): void {
    loadSequence = [];
    projectServiceSpy.getProject.calls.reset();
    projectServiceSpy.invalidateProjectCache.calls.reset();
    projectServiceSpy.removeRepository.calls.reset();
  }

  function setDialogSpy(): void {
    (component as unknown as { dialog: MatDialog }).dialog = dialogSpy;
  }
});

function createDeferredPromise<T>(): DeferredPromise<T> {
  let resolveFn!: (value: T | PromiseLike<T>) => void;
  let rejectFn!: (reason?: unknown) => void;
  const promise = new Promise<T>((resolve, reject) => {
    resolveFn = resolve;
    rejectFn = reject;
  });

  return {
    promise,
    resolve: resolveFn,
    reject: rejectFn,
  };
}

function createProjectResponse(
  layoutPreset: ProjectLayoutPreset,
  repositories: ProjectRepositoryResponse[] = [],
): ProjectResponse {
  return {
    id: 'project-1',
    name: 'Project 1',
    description: 'Generated project',
    members: [],
    environmentDefinitions: [],
    defaultNamingTemplate: null,
    resourceNamingTemplates: [],
    resourceAbbreviations: [],
    tags: [],
    agentPoolName: null,
    repositories,
    layoutPreset,
  };
}

function createDialogRef<TResult>(afterClosed$: Observable<TResult>): MatDialogRef<unknown, TResult> {
  return {
    afterClosed: () => afterClosed$,
  } as unknown as MatDialogRef<unknown, TResult>;
}

function createRepositoryResponse(
  id: string,
  contentKinds: RepositoryContentKind[],
): ProjectRepositoryResponse {
  return {
    id,
    alias: `alias-${id}`,
    providerType: 'GitHub',
    repositoryUrl: `https://github.com/example/${id}`,
    owner: 'example',
    repositoryName: id,
    defaultBranch: 'main',
    contentKinds,
  };
}