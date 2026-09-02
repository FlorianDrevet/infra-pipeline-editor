import { signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ActivatedRoute, Router, convertToParamMap, provideRouter } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';

import { SidebarContextService } from '../../../core/layouts/sidebar/sidebar-context.service';
import { BicepFileNode } from '../../../shared/components/bicep-file-panel/bicep-file-panel.component';
import {
  GenerateProjectBicepResponse,
  GenerateProjectBootstrapPipelineResponse,
  GenerateProjectPipelineResponse,
  ProjectResponse,
} from '../../../shared/interfaces/project.interface';
import { InfrastructureConfigResponse } from '../../../shared/interfaces/infra-config.interface';
import { ProjectLayoutPreset, ProjectRepositoryResponse } from '../../../shared/interfaces/project-repository.interface';
import { ProjectService } from '../../../shared/services/project.service';
import { ProjectDetailGenerationWorkflowService } from '../project-detail-generation-workflow.service';
import { GenerationBoardComponent } from './generation-board.component';

interface GroupMetricExpectation {
  readonly labelKey: string;
  readonly value: number;
}

interface RepositoryGroupExpectation {
  readonly key: string;
  readonly metrics: readonly GroupMetricExpectation[];
}

interface MonoRepoTabExpectation {
  readonly id: string;
  readonly downloadBusyLabelKey: string;
}

class ProjectDetailGenerationWorkflowServiceStub {
  readonly validatingDiagnostics = signal(false);
  readonly lastGenerationLoading = signal(false);
  readonly lastGenerationAvailable = signal<boolean | null>(true);
  readonly lastGenerationErrorKey = signal('');
  readonly viewingHistoricalGeneration = signal(false);
  readonly displayedHistoricalGenerationAt = signal<string | null>(null);
  readonly projectGenerateAllLoading = signal(false);
  readonly projectBicepLoading = signal(false);
  readonly projectBicepResult = signal<GenerateProjectBicepResponse | null>(null);
  readonly projectBicepDownloading = signal(false);
  readonly projectInfraArtifactsDownloading = signal(false);
  readonly projectBicepErrorKey = signal('');
  readonly projectGenerationPanelCollapsed = signal(false);
  readonly projectPipelineLoading = signal(false);
  readonly projectPipelineResult = signal<GenerateProjectPipelineResponse | null>(null);
  readonly projectPipelineDownloading = signal(false);
  readonly projectCodeArtifactsDownloading = signal(false);
  readonly projectPipelineErrorKey = signal('');
  readonly projectBootstrapLoading = signal(false);
  readonly projectBootstrapResult = signal<GenerateProjectBootstrapPipelineResponse | null>(null);
  readonly projectBootstrapDownloading = signal(false);
  readonly projectBootstrapErrorKey = signal('');
  readonly canPushAllProjectArtifacts = signal(false);
  readonly isSplitInfraCodeLayout = signal(false);
  readonly isMultiRepoLayout = signal(false);
  readonly canPushMultiRepoArtifacts = signal(false);
  readonly projectBicepNodes = signal<BicepFileNode[]>([]);
  readonly projectPipelineNodes = signal<BicepFileNode[]>([]);
  readonly projectBootstrapNodes = signal<BicepFileNode[]>([]);
  readonly deferMonoRepoBatchReveal = signal(false);
  readonly projectGenerationPanelOpen = signal(false);

  readonly loadProjectBicepFile = jasmine.createSpy('loadProjectBicepFile').and.resolveTo('main content');
  readonly loadProjectPipelineFile = jasmine.createSpy('loadProjectPipelineFile').and.resolveTo('pipeline content');
  readonly loadProjectBootstrapFile = jasmine.createSpy('loadProjectBootstrapFile').and.resolveTo('bootstrap content');

  readonly setProject = jasmine.createSpy('setProject');
  readonly setConfigs = jasmine.createSpy('setConfigs');
  readonly checkLastGenerationAvailable = jasmine.createSpy('checkLastGenerationAvailable').and.resolveTo();
  readonly generateAll = jasmine.createSpy('generateAll').and.resolveTo();
  readonly generateProjectBicep = jasmine.createSpy('generateProjectBicep').and.resolveTo();
  readonly generateProjectPipeline = jasmine.createSpy('generateProjectPipeline').and.resolveTo();
  readonly generateProjectBootstrap = jasmine.createSpy('generateProjectBootstrap').and.resolveTo();
  readonly toggleProjectGenerationPanelCollapsed = jasmine.createSpy('toggleProjectGenerationPanelCollapsed');
  readonly downloadProjectBicepFiles = jasmine.createSpy('downloadProjectBicepFiles').and.resolveTo();
  readonly downloadProjectInfraArtifacts = jasmine.createSpy('downloadProjectInfraArtifacts').and.resolveTo();
  readonly downloadProjectCodeArtifacts = jasmine.createSpy('downloadProjectCodeArtifacts').and.resolveTo();
  readonly openProjectPushAllToGitDialog = jasmine.createSpy('openProjectPushAllToGitDialog');
  readonly openProjectMultiRepoPushDialog = jasmine.createSpy('openProjectMultiRepoPushDialog');
  readonly openProjectMultiRepoArtifactsPushDialog = jasmine.createSpy('openProjectMultiRepoArtifactsPushDialog');
  readonly downloadProjectPipelineFiles = jasmine.createSpy('downloadProjectPipelineFiles').and.resolveTo();
  readonly downloadProjectBootstrapFiles = jasmine.createSpy('downloadProjectBootstrapFiles').and.resolveTo();
  readonly loadLastGeneration = jasmine.createSpy('loadLastGeneration').and.resolveTo();
}

describe('GenerationBoardComponent', () => {
  let fixture: ComponentFixture<GenerationBoardComponent>;
  let component: GenerationBoardComponent;
  let projectServiceSpy: jasmine.SpyObj<ProjectService>;
  let dialogSpy: jasmine.SpyObj<MatDialog>;
  let router: Router;
  let sidebarContextServiceSpy: jasmine.SpyObj<SidebarContextService>;
  let workflowStub: ProjectDetailGenerationWorkflowServiceStub;

  beforeEach(async () => {
    projectServiceSpy = jasmine.createSpyObj<ProjectService>('ProjectService', ['getProject', 'getProjectConfigs']);
    dialogSpy = jasmine.createSpyObj<MatDialog>('MatDialog', ['open']);
    sidebarContextServiceSpy = jasmine.createSpyObj<SidebarContextService>('SidebarContextService', ['setProjectContext']);
    workflowStub = new ProjectDetailGenerationWorkflowServiceStub();

    projectServiceSpy.getProject.and.resolveTo(createProject());
    projectServiceSpy.getProjectConfigs.and.resolveTo([createConfig()]);

    TestBed.overrideComponent(GenerationBoardComponent, {
      remove: {
        providers: [ProjectDetailGenerationWorkflowService],
      },
      add: {
        providers: [{ provide: ProjectDetailGenerationWorkflowService, useValue: workflowStub }],
      },
    });

    await TestBed.configureTestingModule({
      imports: [GenerationBoardComponent, TranslateModule.forRoot()],
      providers: [
        provideRouter([]),
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: {
              paramMap: convertToParamMap({ id: 'project-1' }),
            },
          },
        },
        { provide: ProjectService, useValue: projectServiceSpy },
        { provide: MatDialog, useValue: dialogSpy },
        { provide: MatSnackBar, useValue: jasmine.createSpyObj<MatSnackBar>('MatSnackBar', ['open']) },
        { provide: SidebarContextService, useValue: sidebarContextServiceSpy },
      ],
    }).compileComponents();

    router = TestBed.inject(Router);
  });

  it('syncs the loaded project and configs into the generation workflow service', async () => {
    await createComponent();

    expect(workflowStub.setProject).toHaveBeenCalledWith(createProject());
    expect(workflowStub.setConfigs).toHaveBeenCalledWith([createConfig()]);
  });

  it('syncs the loaded project into the sidebar context so the generation entry can stay active', async () => {
    await createComponent();

    expect(sidebarContextServiceSpy.setProjectContext).toHaveBeenCalledOnceWith('project-1', 'Project 1');
  });

  it('keeps the board loading until the initial last-generation availability check settles', async () => {
    const availabilityCheck = createDeferredPromise<void>();
    workflowStub.lastGenerationAvailable.set(null);
    workflowStub.checkLastGenerationAvailable.and.callFake(async () => {
      await availabilityCheck.promise;
      workflowStub.lastGenerationAvailable.set(true);
    });

    fixture = TestBed.createComponent(GenerationBoardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
    await flushPromises();
    fixture.detectChanges();

    expect(getIsLoading(component)).toBeTrue();
    expect(getLoadingStateElement(fixture)).not.toBeNull();
    expect(getReadyActionsElement(fixture)).toBeNull();

    availabilityCheck.resolve();
    await flushPromises();
    await fixture.whenStable();
    await flushPromises();
    await fixture.whenRenderingDone();
    fixture.detectChanges();

    expect(getIsLoading(component)).toBeFalse();
    expect(findButtonByText(fixture, 'PROJECT_DETAIL.BOARD.LOAD_LAST_GENERATION')).not.toBeNull();
  });

  it('delegates the generate action to the workflow service instead of opening dialogs directly', async () => {
    await createComponent();

    invokeGenerateAll(component);

    expect(workflowStub.generateAll).toHaveBeenCalledOnceWith();
    expect(dialogSpy.open).not.toHaveBeenCalled();
  });

  it('renders a single generate-all action while keeping the ready state call to action', async () => {
    await createComponent();

    const generateAllButtons = Array.from(
      (fixture.nativeElement as HTMLElement).querySelectorAll('button')
    ).filter((button) => button.textContent?.includes('PROJECT_DETAIL.BOARD.GENERATE_ALL_CONFIGS'));

    expect(generateAllButtons.length).toBe(1);
  });

  it('hides the project-level generate-all action for MultiRepo projects', async () => {
    projectServiceSpy.getProject.and.resolveTo(createProject('MultiRepo'));
    workflowStub.isMultiRepoLayout.set(true);

    await createComponent();

    const generateAllButton = Array.from(
      (fixture.nativeElement as HTMLElement).querySelectorAll('button')
    ).find((button) => button.textContent?.includes('PROJECT_DETAIL.BOARD.GENERATE_ALL_CONFIGS'));

    expect(generateAllButton).toBeUndefined();
  });

  it('renders the ready-state actions inside a dedicated action group', async () => {
    await createComponent();

    const actionGroup = (fixture.nativeElement as HTMLElement).querySelector('.board__ready-actions');

    expect(actionGroup).not.toBeNull();
  });

  it('removes the per-config list from repository cards', async () => {
    await createComponent();

    expect(fixture.nativeElement.querySelector('.repo-card__configs')).toBeNull();
  });

  it('renders the mono-repo explorer when generated nodes are available', async () => {
    await createComponent();
    workflowStub.projectGenerationPanelOpen.set(true);
    workflowStub.projectBicepResult.set(createBicepResponse());
    workflowStub.projectBicepNodes.set([createNode('main.bicep')]);

    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('app-bicep-file-panel')).not.toBeNull();
  });

  it('renders the split infra/code explorer when the workflow is in split mode', async () => {
    projectServiceSpy.getProject.and.resolveTo(createProject('SplitInfraCode'));

    await createComponent();
    workflowStub.projectGenerationPanelOpen.set(true);
    workflowStub.isSplitInfraCodeLayout.set(true);

    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('app-split-generation-switcher')).not.toBeNull();
  });

  it('renders both target repository cards for the split infra/code layout', async () => {
    projectServiceSpy.getProject.and.resolveTo(createSplitProject());

    await createComponent();

    const groups = getGroupedByRepository(component);
    const repositoryCount = (component as unknown as {
      repositoryCount(): number;
    }).repositoryCount();

    expect(groups.map((group) => group.key)).toEqual(['repo-infra', 'repo-code']);
    expect(repositoryCount).toBe(2);
  });

  it('renders the configure action so layout and repositories can be edited from the generation board', async () => {
    const navigateSpy = spyOn(router, 'navigate').and.resolveTo(true);

    await createComponent();

    const configureButton = Array.from((fixture.nativeElement as HTMLElement).querySelectorAll('button'))
      .find((button) => button.textContent?.includes('PROJECT_DETAIL.BOARD.CONFIGURE'));

    expect(configureButton).toBeTruthy();

    invokeNavigateToConfig(component);

    expect(navigateSpy).toHaveBeenCalledOnceWith(['/projects', 'project-1', 'generate', 'config']);
  });

  it('builds dedicated metrics for the split application-code repository card', async () => {
    projectServiceSpy.getProject.and.resolveTo(createSplitProject());
    projectServiceSpy.getProjectConfigs.and.resolveTo([
      createConfig({
        id: 'config-1',
        name: 'Config 1',
        resourceGroupCount: 2,
        resourceCount: 5,
        appPipelineMode: 'Combined',
      }),
      createConfig({
        id: 'config-2',
        name: 'Config 2',
        resourceGroupCount: 3,
        resourceCount: 8,
        appPipelineMode: 'Isolated',
      }),
    ]);

    await createComponent();

    const groups = getGroupedByRepository(component);
    const infraGroup = groups.find((group) => group.key === 'repo-infra');
    const codeGroup = groups.find((group) => group.key === 'repo-code');

    expect(infraGroup?.metrics).toEqual([
      { labelKey: 'PROJECT_DETAIL.BOARD.SUMMARY_CONFIGS', value: 2 },
      { labelKey: 'PROJECT_DETAIL.BOARD.SUMMARY_RESOURCE_GROUPS', value: 5 },
      { labelKey: 'PROJECT_DETAIL.BOARD.SUMMARY_RESOURCES', value: 13 },
    ]);
    expect(codeGroup?.metrics).toEqual([
      { labelKey: 'PROJECT_DETAIL.BOARD.SUMMARY_APPLICATIONS', value: 2 },
      { labelKey: 'PROJECT_DETAIL.BOARD.SUMMARY_PIPELINE_SCOPES', value: 2 },
      { labelKey: 'PROJECT_DETAIL.BOARD.SUMMARY_SHARED_PIPELINES', value: 1 },
    ]);
  });

  it('keeps the legacy repository identity rows out of the generation board', async () => {
    await createComponent();

    const nativeElement = fixture.nativeElement as HTMLElement;

    expect(nativeElement.querySelector('.repo-card__subtitle')).toBeNull();
    expect(nativeElement.querySelector('.repo-card__details a')).toBeNull();
    expect(nativeElement.querySelector('.repo-card__detail-label')).toBeNull();
  });

  it('defines exactly three mono-repo tabs and uses the bootstrap downloading label for the busy download state', async () => {
    await createComponent();

    const monoRepoTabs = getMonoRepoTabs(component);
    const bootstrapTab = monoRepoTabs.find((tab) => tab.id === 'bootstrap');

    expect(monoRepoTabs.map((tab) => tab.id)).toEqual(['bicep', 'pipeline', 'bootstrap']);
    expect(bootstrapTab?.downloadBusyLabelKey).toBe('PROJECT_DETAIL.BOOTSTRAP.DOWNLOADING');
  });

  async function createComponent(): Promise<void> {
    fixture = TestBed.createComponent(GenerationBoardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
    await flushPromises();
    await fixture.whenStable();
    await flushPromises();
    await fixture.whenRenderingDone();
    fixture.detectChanges();
  }
});

function createProject(layoutPreset: ProjectLayoutPreset = 'AllInOne'): ProjectResponse {
  return {
    id: 'project-1',
    name: 'Project 1',
    description: 'Project description',
    members: [],
    environmentDefinitions: [],
    defaultNamingTemplate: null,
    resourceNamingTemplates: [],
    resourceAbbreviations: [],
    tags: [],
    agentPoolName: null,
    repositories: createProjectRepositories(layoutPreset),
    layoutPreset,
    usedResourceTypes: [],
  };
}

function createSplitProject(): ProjectResponse {
  return createProject('SplitInfraCode');
}

function getGroupedByRepository(component: GenerationBoardComponent): readonly RepositoryGroupExpectation[] {
  return (component as unknown as {
    groupedByRepository(): readonly RepositoryGroupExpectation[];
  }).groupedByRepository();
}

function getMonoRepoTabs(component: GenerationBoardComponent): readonly MonoRepoTabExpectation[] {
  return (component as unknown as {
    monoRepoTabs: readonly MonoRepoTabExpectation[];
  }).monoRepoTabs;
}

function createProjectRepositories(layoutPreset: ProjectLayoutPreset): ProjectRepositoryResponse[] {
  if (layoutPreset === 'SplitInfraCode') {
    return [
      createRepository('repo-infra', 'infra', ['Infrastructure']),
      createRepository('repo-code', 'code', ['ApplicationCode']),
    ];
  }

  return [createRepository('repo-1', 'default', ['Infrastructure'])];
}

function createRepository(
  id: string,
  repositoryName: string,
  contentKinds: ProjectRepositoryResponse['contentKinds'],
): ProjectRepositoryResponse {
  return {
    id,
    providerType: 'GitHub',
    repositoryUrl: `https://example.test/org/${repositoryName}`,
    owner: 'org',
    repositoryName,
    defaultBranch: 'main',
    isConfigured: true,
    contentKinds,
  };
}

function createConfig(overrides: Partial<InfrastructureConfigResponse> = {}): InfrastructureConfigResponse {
  return {
    id: 'config-1',
    name: 'Config 1',
    defaultNamingTemplate: null,
    projectId: 'project-1',
    useProjectNamingConventions: true,
    resourceNamingTemplates: [],
    resourceAbbreviationOverrides: [],
    resourceGroupCount: 2,
    resourceCount: 5,
    crossConfigReferenceCount: 0,
    appPipelineMode: 'Combined',
    tags: [],
    layoutMode: null,
    repositories: [],
    ...overrides,
  };
}

function createBicepResponse(): GenerateProjectBicepResponse {
  return {
    commonFileUris: { 'main.bicep': '/files/main.bicep' },
    configFileUris: {},
  };
}

function createNode(path: string): BicepFileNode {
  return {
    kind: 'file',
    path,
    displayName: path,
    type: 'entry-point',
    uri: path,
    depth: 0,
    parentFolderKey: '',
  };
}

function getIsLoading(component: GenerationBoardComponent): boolean {
  return (component as unknown as { isLoading(): boolean }).isLoading();
}

function getLoadingStateElement(fixture: ComponentFixture<GenerationBoardComponent>): Element | null {
  return fixture.nativeElement.querySelector('.board-state--loading');
}

function getReadyActionsElement(fixture: ComponentFixture<GenerationBoardComponent>): Element | null {
  return fixture.nativeElement.querySelector('.board__ready-actions');
}

function findButtonByText(
  fixture: ComponentFixture<GenerationBoardComponent>,
  text: string,
): HTMLButtonElement | null {
  return Array.from((fixture.nativeElement as HTMLElement).querySelectorAll('button'))
    .find((button) => button.textContent?.includes(text) ?? false) ?? null;
}

function createDeferredPromise<T>(): {
  promise: Promise<T>;
  resolve: (value: T | PromiseLike<T>) => void;
  reject: (reason?: unknown) => void;
} {
  let resolve!: (value: T | PromiseLike<T>) => void;
  let reject!: (reason?: unknown) => void;

  const promise = new Promise<T>((promiseResolve, promiseReject) => {
    resolve = promiseResolve;
    reject = promiseReject;
  });

  return { promise, resolve, reject };
}

async function flushPromises(): Promise<void> {
  await Promise.resolve();
  await Promise.resolve();
  await Promise.resolve();
}

function invokeGenerateAll(component: GenerationBoardComponent): void {
  (component as unknown as { onGenerateAll(): void }).onGenerateAll();
}

function invokeNavigateToConfig(component: GenerationBoardComponent): void {
  (component as unknown as { navigateToConfig(): void }).navigateToConfig();
}