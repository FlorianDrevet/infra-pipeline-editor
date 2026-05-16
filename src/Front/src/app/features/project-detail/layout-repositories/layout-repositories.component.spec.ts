import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { TranslateModule } from '@ngx-translate/core';

import { ProjectResponse } from '../../../shared/interfaces/project.interface';
import { ProjectLayoutPreset } from '../../../shared/interfaces/project-repository.interface';
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
  readonly presetChanged: {
    subscribe(callback: (value: ProjectLayoutPreset) => void): { unsubscribe(): void };
  };
}

describe('LayoutRepositoriesComponent', () => {
  let fixture: ComponentFixture<LayoutRepositoriesComponent>;
  let component: LayoutRepositoriesComponent;
  let projectServiceSpy: jasmine.SpyObj<ProjectService>;
  let snackBarSpy: jasmine.SpyObj<MatSnackBar>;
  let projectResponse: ProjectResponse;

  beforeEach(async () => {
    projectResponse = createProjectResponse('AllInOne');
    projectServiceSpy = jasmine.createSpyObj<ProjectService>('ProjectService', [
      'getProject',
      'setLayoutPreset',
      'invalidateProjectCache',
      'removeRepository',
    ]);
    snackBarSpy = jasmine.createSpyObj<MatSnackBar>('MatSnackBar', ['open']);

    projectServiceSpy.getProject.and.callFake(async () => projectResponse);
    projectServiceSpy.setLayoutPreset.and.resolveTo();

    await TestBed.configureTestingModule({
      imports: [LayoutRepositoriesComponent, TranslateModule.forRoot()],
      providers: [
        { provide: ProjectService, useValue: projectServiceSpy },
        { provide: MatDialog, useValue: jasmine.createSpyObj<MatDialog>('MatDialog', ['open']) },
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

  async function createComponent(): Promise<void> {
    fixture = TestBed.createComponent(LayoutRepositoriesComponent);
    fixture.componentRef.setInput('projectId', 'project-1');
    component = fixture.componentInstance;
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

function createProjectResponse(layoutPreset: ProjectLayoutPreset): ProjectResponse {
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
    repositories: [],
    layoutPreset,
  };
}