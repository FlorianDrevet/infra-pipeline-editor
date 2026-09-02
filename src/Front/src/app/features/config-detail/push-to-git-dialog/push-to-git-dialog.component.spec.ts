import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { TranslateModule } from '@ngx-translate/core';
import { PushToGitDialogComponent, PushToGitDialogData } from './push-to-git-dialog.component';
import { DsAutocompleteComponent, DsAutocompleteOption, DsButtonComponent } from '../../../shared/components/ds';
import { BicepGeneratorService } from '../../../shared/services/bicep-generator.service';
import { PipelineGeneratorService } from '../../../shared/services/pipeline-generator.service';
import { BootstrapGeneratorService } from '../../../shared/services/bootstrap-generator.service';
import { ProjectService } from '../../../shared/services/project.service';
import { GitBranchResponse } from '../../../shared/interfaces/project.interface';

interface PushToGitDialogComponentTestApi {
  branchesLoading: () => boolean;
  branchOptions: () => DsAutocompleteOption<string>[];
  branchControl: { value: string; setValue(value: string): void };
  onPush(): Promise<void>;
}

interface DeferredPromise<TValue> {
  promise: Promise<TValue>;
  resolve(value: TValue): void;
}

interface ComponentSetupOptions {
  branchesPromise?: Promise<GitBranchResponse[]>;
  waitForBranches?: boolean;
  data?: Partial<PushToGitDialogData>;
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

describe('PushToGitDialogComponent', () => {
  let fixture: ComponentFixture<PushToGitDialogComponent>;
  let component: PushToGitDialogComponent;
  let componentTestApi: PushToGitDialogComponentTestApi;
  let bicepGeneratorServiceSpy: jasmine.SpyObj<BicepGeneratorService>;
  let pipelineGeneratorServiceSpy: jasmine.SpyObj<PipelineGeneratorService>;
  let bootstrapGeneratorServiceSpy: jasmine.SpyObj<BootstrapGeneratorService>;
  let projectServiceSpy: jasmine.SpyObj<ProjectService>;

  const mockData: PushToGitDialogData = {
    configId: 'cfg-1',
    projectId: 'proj-1',
  };

  async function createComponent(options: ComponentSetupOptions = {}): Promise<void> {
    projectServiceSpy.listBranches.and.returnValue(options.branchesPromise ?? Promise.resolve(createBranchResponses()));

    if (options.data) {
      TestBed.overrideProvider(MAT_DIALOG_DATA, { useValue: { ...mockData, ...options.data } });
    }

    fixture = TestBed.createComponent(PushToGitDialogComponent);
    component = fixture.componentInstance;
    componentTestApi = component as unknown as PushToGitDialogComponentTestApi;
    fixture.detectChanges();

    if (options.waitForBranches ?? true) {
      await fixture.whenStable();
      fixture.detectChanges();
    }
  }

  beforeEach(async () => {
    localStorage.removeItem('ifs-push-branch-proj-1');

    bicepGeneratorServiceSpy = jasmine.createSpyObj<BicepGeneratorService>('BicepGeneratorService', ['pushToGit']);
    pipelineGeneratorServiceSpy = jasmine.createSpyObj<PipelineGeneratorService>('PipelineGeneratorService', ['pushToGit']);
    bootstrapGeneratorServiceSpy = jasmine.createSpyObj<BootstrapGeneratorService>('BootstrapGeneratorService', ['pushToGit']);
    projectServiceSpy = jasmine.createSpyObj<ProjectService>('ProjectService', [
      'listBranches',
      'pushProjectBicepToGit',
      'pushProjectPipelineToGit',
      'pushProjectBootstrapPipelineToGit',
      'pushProjectGeneratedArtifactsToGit',
    ]);
    bicepGeneratorServiceSpy.pushToGit.and.resolveTo({
      branchName: 'main',
      branchUrl: 'https://example.test/branch/main',
      commitSha: '12345678',
      fileCount: 1,
    });
    bootstrapGeneratorServiceSpy.pushToGit.and.resolveTo({
      branchName: 'main',
      branchUrl: 'https://example.test/branch/main',
      commitSha: '87654321',
      fileCount: 1,
    });

    await TestBed.configureTestingModule({
      imports: [PushToGitDialogComponent, TranslateModule.forRoot()],
      providers: [
        { provide: MatDialogRef, useValue: jasmine.createSpyObj('MatDialogRef', ['close']) },
        { provide: MAT_DIALOG_DATA, useValue: mockData },
        { provide: BicepGeneratorService, useValue: bicepGeneratorServiceSpy },
        { provide: PipelineGeneratorService, useValue: pipelineGeneratorServiceSpy },
        { provide: BootstrapGeneratorService, useValue: bootstrapGeneratorServiceSpy },
        { provide: ProjectService, useValue: projectServiceSpy },
      ],
    }).compileComponents();
  });

  afterEach(() => {
    localStorage.removeItem('ifs-push-branch-proj-1');
  });

  it('should create', async () => {
    await createComponent();

    expect(component).toBeTruthy();
  });

  it('renders the shared design-system autocomplete for branch selection', async () => {
    await createComponent();

    const autocomplete = fixture.debugElement.query(By.directive(DsAutocompleteComponent));

    expect(autocomplete).not.toBeNull();
  });

  it('starts branch loading on open without displaying a default branch before the fetch resolves', async () => {
    const branchesDeferred = createDeferredPromise<GitBranchResponse[]>();

    await createComponent({ branchesPromise: branchesDeferred.promise, waitForBranches: false });

    expect(projectServiceSpy.listBranches).toHaveBeenCalledOnceWith('proj-1');
    expect(componentTestApi.branchesLoading()).toBeTrue();
    expect(componentTestApi.branchControl.value).toBe('');
    expect(componentTestApi.branchOptions()).toEqual([]);

    const autocomplete = fixture.debugElement.query(By.directive(DsAutocompleteComponent)).componentInstance as DsAutocompleteComponent;
    // The form state renders "Close" (ghost) then "Push" (primary) — take the
    // Push button specifically, not the first DsButtonComponent match.
    const pushButton = getPushButton();

    expect(autocomplete.loading()).toBeTrue();
    expect(autocomplete.options()).toEqual([]);
    expect(pushButton.disabled()).toBeTrue();

    await componentTestApi.onPush();

    expect(bicepGeneratorServiceSpy.pushToGit).not.toHaveBeenCalled();

    branchesDeferred.resolve(createBranchResponses());
    await fixture.whenStable();
    fixture.detectChanges();

    expect(componentTestApi.branchesLoading()).toBeFalse();
    expect(componentTestApi.branchControl.value).toBe('main');
    expect(pushButton.disabled()).toBeFalse();
  });

  it('applies the stored branch only after branches have loaded', async () => {
    localStorage.setItem('ifs-push-branch-proj-1', 'release/1.0');
    const branchesDeferred = createDeferredPromise<GitBranchResponse[]>();

    await createComponent({ branchesPromise: branchesDeferred.promise, waitForBranches: false });

    expect(componentTestApi.branchControl.value).toBe('');
    expect(componentTestApi.branchOptions()).toEqual([]);

    branchesDeferred.resolve(createBranchResponses());
    await fixture.whenStable();
    fixture.detectChanges();

    expect(componentTestApi.branchControl.value).toBe('release/1.0');
    expect(componentTestApi.branchOptions()).toEqual([{ value: 'release/1.0', label: 'release/1.0' }]);
  });

  it('keeps the branch field empty and usable when branch loading fails', async () => {
    await createComponent({ branchesPromise: Promise.reject(new Error('Branch lookup failed')) });

    expect(componentTestApi.branchesLoading()).toBeFalse();
    expect(componentTestApi.branchControl.value).toBe('');
    expect(componentTestApi.branchOptions()).toEqual([]);

    componentTestApi.branchControl.setValue('hotfix/manual');
    fixture.detectChanges();

    const pushButton = getPushButton();

    expect(pushButton.disabled()).toBeFalse();
  });

  it('routes config-level pushes to the bootstrap service when isBootstrap is set', async () => {
    await createComponent({ data: { isBootstrap: true } });

    await componentTestApi.onPush();

    expect(bootstrapGeneratorServiceSpy.pushToGit).toHaveBeenCalledOnceWith(
      'cfg-1',
      jasmine.objectContaining({ branchName: 'main' }),
    );
    expect(bicepGeneratorServiceSpy.pushToGit).not.toHaveBeenCalled();
    expect(pipelineGeneratorServiceSpy.pushToGit).not.toHaveBeenCalled();
  });

  // The "form" state renders two DsButtonComponent instances (Close, then Push);
  // querying by directive alone would silently match Close instead of Push.
  function getPushButton(): DsButtonComponent {
    const buttons = fixture.debugElement.queryAll(By.directive(DsButtonComponent));
    return buttons[buttons.length - 1].componentInstance as DsButtonComponent;
  }
});
